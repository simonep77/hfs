using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.CLASSI.FileHandling;
using Hfs.Server.CODICE.VFS;
using Hfs.Server.HfsCore.Commands;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Crea i command in base all'action
    /// </summary>
    public class CommandFactory
    {

        internal delegate ICommand CommandCreator();

        internal static Dictionary<string, CommandTemplate> AvailableCommands { get; } = new Dictionary<string, CommandTemplate>(60);

        /// <summary>
        /// Classe contenente istanza base di command per le Info
        /// </summary>
        internal class CommandTemplate
        {
            public ICommand Command { get; }
            internal CommandCreator Create { get; }

            internal CommandTemplate(Type tCommand)
            {
                //Crea fast constructor
                this.Create = getFastActivator(tCommand);
                //Assegna istanza di comando
                this.Command = this.Create();
            }

            private static CommandCreator getFastActivator(Type t)
            {
                //ParameterExpression param = Expression.Parameter(typeof(object[]), "args");

                // Make a NewExpression that calls the ctor with the args we just created
                NewExpression newExp = Expression.New(t.GetConstructor(Type.EmptyTypes));

                // Create a lambda with the New expression as body and our param object[] as arg
                //LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), newExp, param);
                LambdaExpression lambda = Expression.Lambda(typeof(CommandCreator), newExp, null);

                // Compile it
                return (CommandCreator)lambda.Compile();
            }
        }

        static CommandFactory()
        {
            //Crea tipo interfaccia
            var tInt = typeof(ICommand);
            //Cerca tipi ICommand non astratti
            var cmdTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                           where tInt.IsAssignableFrom(t) && !t.IsAbstract
                           select t;

            foreach (var item in cmdTypes)
            {
                //Crea fast constructor
                var tpl = new CommandTemplate(item);
                //Aggiunge il command
                AvailableCommands[tpl.Command.ActionKey] = tpl;
            }

        }




        /// <summary>
        /// Crea comando da contesto Http
        /// </summary>
        /// <param name="action"></param>
        /// <cmd = s></cmd = s>
        public static ICommand Create(Controller controller)
        {

            HfsRequest oReq = new HfsRequest();
            oReq.ReadFromContext(controller.HttpContext);
            ICommand cmd = null;
            
            try
            {
                cmd = AvailableCommands[oReq.ActionStr].Create();
            }
            catch (KeyNotFoundException)
            {
                cmd = new CommandUnknown();
            }

            //Inizializza
            cmd.Init(controller, oReq);

            //Ritorna comando
            return cmd;
        }

    }
}
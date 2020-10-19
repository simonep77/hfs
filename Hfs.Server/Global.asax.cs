using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            HfsConfig.Init();
        }


        protected void Application_End(object sender, EventArgs e)
        {
            HfsConfig.Stop();
        }
    }
}

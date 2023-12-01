using System;
using System.IO;
using System.Xml;


namespace Hfs.Server.Core.Common
{ 
    /// <summary>

    /// ''' Classe utility per scrivere xml con un solo oggetto

    /// ''' </summary>

    /// ''' <remarks></remarks>
    public class XmlWrite : IDisposable
    {
        private StringWriter _Tw;
        private XmlTextWriter _Xw;


        public XmlWrite()
        {
            _Tw = new StringWriter();
            _Xw = new XmlTextWriter(_Tw);
        }


        public void WriteStartElement(string nomeElemento)
        {
            this._Xw.WriteStartElement(nomeElemento);
        }

        public void WriteEndElement()
        {
            this._Xw.WriteEndElement();
        }

        public virtual void WriteElementString(string nomeElemento, string valore)
        {
            this._Xw.WriteElementString(nomeElemento, valore);
        }

        public void WriteRaw(string rawXml)
        {
            this._Xw.WriteRaw(rawXml);
        }

        public void WriteAttributeString(string nome, string valore)
        {
            this._Xw.WriteAttributeString(nome, valore);
        }

        public void WriteBase64(byte[] buffer)
        {
            this._Xw.WriteBase64(buffer, 0, buffer.Length);
        }

        public void WriteCData(string valore)
        {
            this._Xw.WriteCData(valore);
        }

        public void WriteComment(string valore)
        {
            this._Xw.WriteComment(valore);
        }

        public void WriteValue(object value)
        {
            this._Xw.WriteValue(value);
        }

        public override string ToString()
        {
            this._Xw.Flush();
            return this._Tw.ToString();
        }


        public void Dispose()
        {
            this._Xw.Close();
            this._Xw = null;
            this._Tw = null;
        }
    }

}
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Hfs.Client;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// File handler per directory su hfs remoti
    /// </summary>
    internal class DirHandlerHfs: DirHandlerBase
    {
        private HfsResponseVfs mVfsResp;
        private HfsClient mClient;
        private string mFullName;

        public DirHandlerHfs(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;
            this.mClient = new HfsClient(resp.Path.Physical);
            this.mFullName = Utility.HfsCombine(this.mClient.CurrentDirectory, resp.VirtualPath);
        }

        public override string FullName
        {
            get { return this.mVfsResp.VirtualPath; }
        }

        public override string Name
        {
            get { return Path.GetFileName(this.mVfsResp.VirtualPath); }
        }

        public override void Create()
        {
            this.mClient.DirectoryCreate(this.mVfsResp.VirtualPath);
        }

        public override bool Exist()
        {
            return this.mClient.DirectoryExist(this.mVfsResp.VirtualPath);
        }

        public override void Delete()
        {
            this.mClient.DirectoryDelete(this.mVfsResp.VirtualPath);
        }

        public override long GetSize()
        {
            return this.mClient.DirectoryQuota(this.mVfsResp.VirtualPath);
        }

        public override string GetFilesXml(string pattern, bool recursive)
        {
            Regex rgx = new Regex(@"(<name>" + this.mClient.CurrentDirectory + ")");

            return rgx.Replace(this.mClient.DirectoryListFilesFilterXML(this.mVfsResp.VirtualPath, pattern, recursive), string.Concat("<name>", this.mVfsResp.Path.Virtual));
        }

        public override string GetDirsXml(string pattern, bool recursive)
        {
            Regex rgx = new Regex(@"(<name>" + this.mClient.CurrentDirectory + ")");

            return rgx.Replace(this.mClient.DirectoryListSubDirFilterXML(this.mVfsResp.VirtualPath, pattern, recursive), string.Concat("<name>", this.mVfsResp.Path.Virtual));
        }

        public override IFileHandler GetFileHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new FileHandlerHfs(resp, this.mClient);
        }

        public override IDirHandler GetDirHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new DirHandlerHfs(resp);
        }

        public override IFileHandler[] GetFiles()
        {
            return this.GetFiles("*");
        }

        public override IFileHandler[] GetFiles(string pattern)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(this.GetFilesXml(pattern, false));
            XmlNodeList files = xd.SelectNodes("//file");
            IFileHandler[] ret = new IFileHandler[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                if (this.mVfsResp.PhysicalPath.Length > 0)
                    ret[i] = this.GetFileHandler(files[i].SelectSingleNode("name").InnerText);
                else
                    ret[i] = this.GetFileHandler(files[i].SelectSingleNode("name").InnerText.Remove(0, this.mVfsResp.PhysicalPath.Length + 1));
            }

            return ret;
        }

        public override IDirHandler[] GetDirs()
        {
            return this.GetDirs("*");
        }

        public override IDirHandler[] GetDirs(string pattern)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(this.GetDirsXml(pattern, false));
            XmlNodeList files = xd.SelectNodes("//dir");
            IDirHandler[] ret = new IDirHandler[files.Count];

            for (int i = 0; i < files.Count; i++)
            {
                HfsResponseVfs resp = this.mVfsResp.Clone();
                resp.VirtualPath = string.Concat(resp.VirtualPath, Utility.NormalizeVirtualPath(files[i].SelectSingleNode("name").InnerText.Remove(0, this.mFullName.Length + 1)));
                resp.PhysicalPath = resp.VirtualPath;

                ret[i] = this.GetDirHandler(files[i].SelectSingleNode("name").InnerText.Remove(0, this.mFullName.Length + 1));
            }

            return ret;
        }

    }
}
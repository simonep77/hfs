using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Hfs.Server.Core.Common;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// Dir handler per directory locali
    /// </summary>
    internal class DirHandlerLocal : DirHandlerBase
    {
        private HfsResponseVfs mVfsResp;
        private DirectoryInfo mDirInfo;

        public DirHandlerLocal(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;
            this.mDirInfo = new DirectoryInfo(resp.PhysicalPath);
        }

        public override string FullName
        {
            get { return this.mDirInfo.FullName; }
        }

        public override string Name
        {
            get { return this.mDirInfo.Name; }
        }

        public override void Create()
        {
            Directory.CreateDirectory(this.mDirInfo.FullName);
        }

        public override bool Exist()
        {
            return this.mDirInfo.Exists;
        }

        public override void Delete()
        {
            if (this.mDirInfo.Exists)
                this.mDirInfo.Delete(true);
        }

        public override long GetSize()
        {
            this.checkExist();
            return Utility.GetDirSize(this.mDirInfo.FullName);
        }

        public override string GetFilesXml(string pattern, bool recursive)
        {
            this.checkExist();
            using (var xw = new XmlWrite())
            {
                Utility.SetFileListInfoXml(xw, this.mVfsResp.VirtualPath, this.mDirInfo, this.mVfsResp.Access.ToString(), pattern, recursive);
                return xw.ToString();
            }

        }

        public override string GetDirsXml(string pattern, bool recursive)
        {
            this.checkExist();
            using (var xw = new XmlWrite())
            {
                Utility.SetDirListInfoXml(xw, this.mVfsResp.VirtualPath, this.mDirInfo, this.mVfsResp.Access.ToString(), pattern, recursive);
                return xw.ToString();
            }

        }

        public override IFileHandler GetFileHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, Utility.NormalizeVirtualPath(name));
            resp.PhysicalPath = Path.Combine(this.mDirInfo.FullName, name.TrimStart(Path.DirectorySeparatorChar));
            var temp = new FileHandlerLocal(resp);

            return temp;
        }


        public override IDirHandler GetDirHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, Utility.NormalizeVirtualPath(name));
            resp.PhysicalPath = Path.Combine(this.mDirInfo.FullName, name);
            return new DirHandlerLocal(resp);
        }


        public override IFileHandler[] GetFiles()
        {
            checkExist();
            string[] files = Directory.GetFiles(this.mDirInfo.FullName);
            IFileHandler[] ret = new IFileHandler[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                ret[i] = this.GetFileHandler(files[i].Remove(0, this.FullName.Length + 1));
            }

            return ret;
        }


        public override IFileHandler[] GetFiles(string pattern)
        {
            checkExist();
            string[] files = Directory.GetFiles(this.mDirInfo.FullName, pattern);
            IFileHandler[] ret = new IFileHandler[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                ret[i] = this.GetFileHandler(files[i].Remove(0, this.FullName.Length + 1));
            }

            return ret;
        }


        public override IDirHandler[] GetDirs()
        {
            checkExist();
            string[] dirs = Directory.GetDirectories(this.mDirInfo.FullName);
            IDirHandler[] ret = new IDirHandler[dirs.Length];

            for (int i = 0; i < dirs.Length; i++)
            {
                ret[i] = this.GetDirHandler(dirs[i]);
            }

            return ret;
        }

        public override IDirHandler[] GetDirs(string pattern)
        {
            checkExist();
            string[] dirs = Directory.GetDirectories(this.mDirInfo.FullName, pattern);
            IDirHandler[] ret = new IDirHandler[dirs.Length];

            for (int i = 0; i < dirs.Length; i++)
            {
                ret[i] = this.GetDirHandler(dirs[i]);
            }

            return ret;
        }

        private void checkExist()
        {
            if (!this.mDirInfo.Exists)
                throw new HfsException(EStatusCode.FileNotFound, $"Directory {this.mVfsResp.VirtualPath} non trovata");
        }



        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
using Hfs.Client;
using System;
using System.IO;

namespace Hfs.Core.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (var cli = new HfsClient(@"http://user2:user2@cdpwfias01.casagit.it"))
            {
                var localfile = @"C:\Users\simone.pelaia\Documents\TEST_DATA\TestPNG.PDF";
                var remotefile = @"/temp/TestPNG.PDF";

                cli.FileWriteFromBuffer(remotefile, File.ReadAllBytes(localfile));



                var b = cli.FileExist(remotefile);

                Console.WriteLine($"File exists: {b}");

                var f1 = cli.FileReadToBuffer(remotefile);

                Console.WriteLine($"File read: {f1.Length}");

                var finfo = cli.FileGetInfo(remotefile);

                Console.WriteLine($"File info: {finfo.MimeType}");

            }


        }
    }
}

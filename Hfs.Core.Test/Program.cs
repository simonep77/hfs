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

            using (var cli = new HfsClient(@"http://user2:user2@uploadars/hfs?cd=/PraticheSediin"))
            {


                cli.FileWriteFromBuffer(@"TestPNG.PDF", File.ReadAllBytes(@"C:\Users\simone.pelaia\Desktop\TestPNG.PDF"));

                using (var ms = new MemoryStream(File.ReadAllBytes(@"C:\Users\simone.pelaia\Desktop\TestPNG.PDF")))
                {

                    cli.FileWriteFromBuffer(@"TestPNG.PDF", ms.ToArray());
                }

                using (var f = File.OpenRead(@"C:\Users\simone.pelaia\Desktop\TestPNG.PDF"))
                {

                    cli.FileWriteFromStream(@"TestPNG.PDF", f);
                }


                var b = cli.FileExist(@"TestPNG.PDF");

                Console.WriteLine($"File exists: {b}");

                var f1 = cli.FileReadToBuffer(@"TestPNG.PDF");

                Console.WriteLine($"File read: {f1.Length}");


            }


        }
    }
}

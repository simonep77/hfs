using Hfs.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Hfs.App.Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var cli = new HfsClient(this.textBox1.Text))
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

            }
        }
    }
}

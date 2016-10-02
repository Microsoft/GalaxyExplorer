using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GE_POIMaker
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            textBox3.Text = Path.GetTempPath() + "POI_MyPOI.png";  //set the default save location to the users temp directory by default
        }


        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                int fontSize1 = Convert.ToInt32(textBox4.Text);
                int fontSize2 = Convert.ToInt32(textBox5.Text);
                int OutputImageHeight = Convert.ToInt32(textBox8.Text);
                int OutputImageWidth = Convert.ToInt32(textBox7.Text);
                int blurFactor = Convert.ToInt32(textBox6.Text);

                Bitmap fullBmp = new Bitmap(imageTools.convertText(textBox1.Text.ToUpper(), textBox2.Text.ToUpper(), "Orbitron", fontSize1, fontSize2, OutputImageWidth, OutputImageHeight, blurFactor));

                fullBmp.Save(textBox3.Text, System.Drawing.Imaging.ImageFormat.Png);
                String savePath = textBox3.Text;
                fullBmp.Dispose();

                if (MessageBox.Show(
                    "POI bitmap written to: " + savePath + " Exit application?", "",
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                {


                    Application.Exit();

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }



  

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {


        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        //private void button2_Click(object sender, EventArgs e)
        //{

        //    try
        //    {
        //        int result = AddFontResource(@"C:\Users\elite\Downloads\Orbitron\orbitron-medium.otf");
        //        MessageBox.Show("The font has been installed to your system", "All Done" , MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Warning! This font is already installed, exititng function", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }




        //}

        //[DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
        //public static extern int AddFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
        //                                 string lpFileName);

        //[DllImport("gdi32.dll", EntryPoint = "RemoveFontResourceW", SetLastError = true)]
        //public static extern int RemoveFontResource([In][MarshalAs(UnmanagedType.LPWStr)]
        //                                    string lpFileName);

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        int result = RemoveFontResource(@"C:\Windows\Fonts\orbitron-medium.TTF");
        //        MessageBox.Show("The font has been removed from your system", "All Done", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Warning! This font was not installed, exititng function", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        //    }

        //}
    }
}

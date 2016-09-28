using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        }
        public static Bitmap convertText(string txt1, string txt2, string fontName, int fontSize1, int fontSize2)
        {
            // Create a new image
            Bitmap bmp = new Bitmap(1, 1);

            Graphics graphics = Graphics.FromImage(bmp);

            string imagepath = Application.StartupPath;

            // string maskImage = (@"./Assets/Images/Mask.png");

            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            Stream myStream = myAssembly.GetManifestResourceStream("GE_POIMaker.Mask.png");

            //  Image mask = Image.FromFile("~../../Mask.png");

            Image mask = Image.FromStream(myStream);

            Graphics Mask = Graphics.FromImage(mask);



            Font font1 = new Font(fontName, fontSize1);
            Font font2 = new Font(fontName, fontSize2);

            int mwidth = 1000;


            int twidth = 13662;
            int theight = 2048;



            SizeF stringSize = graphics.MeasureString(txt1, font1);
            int mheight = ((int)stringSize.Height - 400);

            bmp = new Bitmap(bmp, twidth, theight);

            graphics = Graphics.FromImage(bmp);

            graphics.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);


            graphics.DrawString(txt1, font1, Brushes.DarkRed, 0, 0);

            //graphics.DrawString(txt2, font2, Brushes.DarkRed, mwidth, (int)stringSize.Height);

            graphics.DrawString(txt2, font2, Brushes.DarkRed, mwidth, mheight);

            //graphics.FillRectangle(Brushes.LawnGreen, 0, (int)stringSize.Height, mwidth, mheight);
            graphics.DrawImage(mask, 0, (mheight + 150));

            font1.Dispose();
            font2.Dispose();
            graphics.Flush();
            graphics.Dispose();
            return bmp;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }



        private void button1_Click(object sender, EventArgs e)
        {

            String savePath = textBox3.Text;


            Bitmap fullBmp = new Bitmap(convertText(textBox1.Text.ToUpper(), textBox2.Text.ToUpper(), "Orbitron", 725, 375));


            fullBmp.Save(textBox3.Text, System.Drawing.Imaging.ImageFormat.Png);

            fullBmp.Dispose();


            if (MessageBox.Show("POI bitmap written to: " + savePath + " Exit application?", "", MessageBoxButtons.YesNo) ==
      DialogResult.Yes)
            {


                Application.Exit();

            }

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

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
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

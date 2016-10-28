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
        }

        /// <summary>
        /// Draws blurred text.
        /// </summary>
        /// <param name="dest">The graphic object into which to render the text</param>
        /// <param name="clipRect">If not Rectangle.Empty, output is clipped to this rectangle; otherwise the graphic object's clipping region is used</param>
        /// <param name="x">The horizontal offset at which to start the render</param>
        /// <param name="y">The vertical offset at which to start the render</param>
        /// <param name="txt">The text blur</param>
        /// <param name="font">The font with which to render the text</param>
        /// <param name="color">The final color the text is rendered</param>
        /// <param name="alpha">The alpha value to use when creating the blur effect</param>
        public static void drawBlurredText(Graphics dest, Rectangle clipRect, int x, int y, string txt, Font font, Color color, byte alpha)
        {
            // remember the original clipping region if a non-empty clipping rectangle is provided
            Region oldClipRegion = null;
            if (clipRect != Rectangle.Empty)
            {
                oldClipRegion = dest.Clip;
                dest.Clip = new Region(clipRect);
            }

            // create a path and draw our string into it
            GraphicsPath path = new GraphicsPath();
            path.AddString(txt, font.FontFamily, (int)font.Style, font.SizeInPoints, new Point(x, y), StringFormat.GenericDefault);

            // iteratively draw the path with an increasingly narrower pen
            for (int penWidth = 400; penWidth >= 0; penWidth -= 20)
            {
                Pen pen = new Pen(Color.FromArgb(alpha, color.R, color.G, color.B), penWidth);
                pen.LineJoin = LineJoin.Round;
                dest.DrawPath(pen, path);
                pen.Dispose();
            }

            // fill in the final path
            SolidBrush fillBrush = new SolidBrush(color);
            dest.FillPath(fillBrush, path);

            // clean up
            if (oldClipRegion != null)
            {
                dest.Clip = oldClipRegion;
            }
            fillBrush.Dispose();
            path.Dispose();
        }
        public static Bitmap convertText(string txt1, string txt2, string fontName, int fontSize1, int fontSize2)
        {
            int twidth = 13662; // the width of the destination image
            int theight = 2048; // the height of the destination image

            Font font1 = new Font(fontName, fontSize1);
            Font font2 = new Font(fontName, fontSize2);

            // Create the new image
            Bitmap bmp = new Bitmap(twidth, theight);
            Graphics graphics = Graphics.FromImage(bmp);

            // fill the image with the blackness of space
            graphics.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);

            // Measure the size of our title text
            SizeF stringSize = graphics.MeasureString(txt1, font1);
            int mheight = (int)stringSize.Height - 500; // 500 is a magic number. Can we calculate it?

            // draw the title text
            drawBlurredText(graphics, Rectangle.Empty, 0, 0, txt1, font1, Color.FromArgb(0xFF, 157, 0, 0), 12);

            // create and render the glyph mask
            Font font3 = new Font("Arial", fontSize2);
            StringBuilder sb = new StringBuilder(" \u25AA"); // Space + Unicode Black Square
            sb.Append(@"\\\\\");
            string glyphText = sb.ToString();
            stringSize = graphics.MeasureString(glyphText, font3);
            SolidBrush glyphBackFillBrush = new SolidBrush(Color.FromArgb(0xFF, 0, 0xFF, 0));
            Rectangle glyphRect = new Rectangle(0, mheight, (int)stringSize.Width, (int)stringSize.Height);
            graphics.FillRectangle(glyphBackFillBrush, glyphRect);
            drawBlurredText(graphics, glyphRect, 0, mheight, glyphText, font3, Color.FromArgb(0xFF, 0xFF, 0xFF, 0), 8);

            // draw the subtitle text on a black background
            stringSize = graphics.MeasureString(txt2, font2);
            Rectangle text2Rect = new Rectangle(glyphRect.Width, mheight, (int)stringSize.Width, (int)stringSize.Height);
            graphics.FillRectangle(Brushes.Black, text2Rect);
            drawBlurredText(graphics, text2Rect, glyphRect.Width, mheight, txt2, font2, Color.FromArgb(0xFF, 157, 0, 0), 12);

            // clean up
            font1.Dispose();
            font2.Dispose();
            font3.Dispose();
            glyphBackFillBrush.Dispose();
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

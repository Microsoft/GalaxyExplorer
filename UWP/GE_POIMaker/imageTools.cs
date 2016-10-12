using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GE_POIMaker
{
    class imageTools
    {

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
        /// <param name="blurFactor">The starting int for the penWidth "for" loop (how many blur loops will be performed / 20)</param>
        public static void drawBlurredText(Graphics dest, Rectangle clipRect, int x, int y, string txt, Font font, Color mtColor, Color stColor, Color gColor, Color gmColor, int gTrans, int blurFactor)
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
            for (int penWidth = blurFactor; penWidth >= 0; penWidth -= 20)
            {
                Pen pen = new Pen(Color.FromArgb(gTrans, mtColor), penWidth);
                pen.LineJoin = LineJoin.Round;
                dest.DrawPath(pen, path);
                pen.Dispose();
            }

            // fill in the final path
            SolidBrush fillBrush = new SolidBrush(mtColor);
            dest.FillPath(fillBrush, path);

            // clean up
            if (oldClipRegion != null)
            {
                dest.Clip = oldClipRegion;
            }
            fillBrush.Dispose();
            path.Dispose();
        }

        public static void drawGlyphText(Graphics dest, Rectangle clipRect, int x, int y, string txt, Font font, Color mtColor, Color stColor, Color gColor, Color gmColor, int gTrans, int blurFactor)
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
            for (int penWidth = blurFactor; penWidth >= 0; penWidth -= 20)
            {
                Pen pen = new Pen(Color.FromArgb(gTrans, gColor), penWidth);
                pen.LineJoin = LineJoin.Round;
                dest.DrawPath(pen, path);
                pen.Dispose();
            }

            // fill in the final path
            SolidBrush fillBrush = new SolidBrush(gColor);
            dest.FillPath(fillBrush, path);

            // clean up
            if (oldClipRegion != null)
            {
                dest.Clip = oldClipRegion;
            }
            fillBrush.Dispose();
            path.Dispose();
        }


        public static Color colorPicker()
        {
            // Show a color dialog box
            ColorDialog colorDlg = new ColorDialog();
            colorDlg.FullOpen = true;
            colorDlg.Color.A.Equals(255);
            //  colorDlg.ShowDialog();
            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                // Pick a color, any color, as long as it is the right one
                Color cp = colorDlg.Color;
                return cp;
            }

            else
            {
                // Default to Red if no color is chosen, that is not the right answer
                return (Color.Red);
            }

        }

        public static Bitmap convertText(string txt1, string txt2, string fontName, int fontSize1, int fontSize2, int OutputImageWidth, int OutputImageHeight, int blurFactor, Color mtColor, Color stColor, Color gColor, Color gmColor, int gTrans)
        {
            //   int twidth = 13662; // the width of the destination image
            //   int theight = 2048; // the height of the destination image
            //   sColor = selected color from color dialog

            Font font1 = new Font(fontName, fontSize1);
            Font font2 = new Font(fontName, fontSize2);

            // Create the new image
            Bitmap bmp = new Bitmap(OutputImageWidth, OutputImageHeight);
            Graphics graphics = Graphics.FromImage(bmp);

            // fill the image with the blackness of space
            graphics.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);

            // Measure the size of our title text
            SizeF stringSize = graphics.MeasureString(txt1, font1);
            //Reduce the string height by 30% to compact the main and sub-titles (plus the glyph mask assembly)
            int mheight = Convert.ToInt32((Double)stringSize.Height * .7);

            int mwidth = Convert.ToInt32((Double)stringSize.Width * .014);

            // draw the title text  
            // drawBlurredText(graphics, Rectangle.Empty, 0, 0, txt1, font1, Color.FromArgb(0xFF, 157, 0, 0), 12, blurFactor);
            //~  drawBlurredText(graphics, Rectangle.Empty, 0, 0, txt1, font1, sColor, 12, blurFactor);
            drawBlurredText(graphics, Rectangle.Empty, 0, 0, txt1, font1, mtColor, stColor, gColor, gmColor, 12, blurFactor);

            // create and render the glyph mask
            Font font3 = new Font("Arial", fontSize2);
            StringBuilder sb = new StringBuilder("  \u25AA\\\\\\\\\\"); // Space + Unicode Black Square + \\\\\

            string glyphText = sb.ToString();
            graphics.PageUnit = GraphicsUnit.Pixel;
            stringSize = graphics.MeasureString(glyphText, font3);


            SolidBrush glyphBackFillBrush = new SolidBrush(gmColor);
            //  SolidBrush glyphBackFillBrush = new SolidBrush(Color.FromArgb(0xFF, 0, 0xFF, 0));

            // calculate the glyphRectangle
            Rectangle glyphRect = new Rectangle(0, mheight, (int)stringSize.Width, (int)stringSize.Height);

            // draw the subtitle text on a black background
            stringSize = graphics.MeasureString(txt2, font2);
            Rectangle text2Rect = new Rectangle(glyphRect.Width, mheight, (int)stringSize.Width, (int)stringSize.Height);
            graphics.FillRectangle(Brushes.Black, text2Rect);
            drawBlurredText(graphics, Rectangle.Empty, glyphRect.Width, mheight, txt2, font2, mtColor, stColor, gColor, gmColor, 12, blurFactor);

            // draw the glyph
            graphics.FillRectangle(glyphBackFillBrush, glyphRect);
            drawGlyphText(graphics, glyphRect, 0, mheight, glyphText, font3, mtColor, stColor, gColor, gmColor, gTrans, blurFactor);

            // clean up
            font1.Dispose();
            font2.Dispose();
            font3.Dispose();
            glyphBackFillBrush.Dispose();
            graphics.Flush();
            graphics.Dispose();
            return bmp;
        }

    }
}

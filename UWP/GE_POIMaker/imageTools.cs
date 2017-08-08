using System;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

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
        /// <param name="blurFactor">The starting int for the penWidth "for" loop (how many blur loops will be performed / MyGlobals.pwMod)</param>
        /// 

        public static void drawBlurredText(Graphics dest, Rectangle clipRect, int x, int y, string txt, Font font)
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

            // fill in the final path
            SolidBrush fillBrush = new SolidBrush(MyGlobals.mtColor);
            dest.FillPath(fillBrush, path);


            // iteratively draw the path with an increasingly smaller and opaque pen
            for (int penWidth = MyGlobals.blurFactor; penWidth >= 0; penWidth -= MyGlobals.pwMod, MyGlobals.gTrans += MyGlobals.gtMod)
            {
                if (MyGlobals.gTrans <= 255)
                {
                    Pen pen = new Pen(Color.FromArgb(MyGlobals.gTrans, MyGlobals.mtColor), penWidth);
                    pen.LineJoin = LineJoin.Round;
                    dest.DrawPath(pen, path);
                    pen.Dispose();
                }

            }

            MyGlobals.gTrans = 0;

            // clean up
            if (oldClipRegion != null)
            {
                dest.Clip = oldClipRegion;
            }
            fillBrush.Dispose();
            path.Dispose();
        }

        public static void drawGlyphText(Graphics dest, Rectangle clipRect, int x, int y, string txt, Font font)
        {
            //// remember the original clipping region if a non-empty clipping rectangle is provided
            Region oldClipRegion = null;
            if (clipRect != Rectangle.Empty)
            {
                oldClipRegion = dest.Clip;
                dest.Clip = new Region(clipRect);
            }

            // create a path and draw our string into it
            GraphicsPath path = new GraphicsPath();
            path.AddString(txt, font.FontFamily, (int)font.Style, font.SizeInPoints, new Point(x, y), StringFormat.GenericDefault);

            // fill in the final path
            SolidBrush fillBrush = new SolidBrush(MyGlobals.gColor);
            dest.FillPath(fillBrush, path);

            // iteratively draw the path with an increasingly and increasingly opaque pen
            for (int penWidth = MyGlobals.blurFactor; penWidth >= 0; penWidth -= MyGlobals.pwMod, MyGlobals.gTrans += MyGlobals.gtMod)
            {
                if (MyGlobals.gTrans <= 255)
                {
                    Pen pen = new Pen(Color.FromArgb(MyGlobals.gTrans, MyGlobals.gColor), penWidth);
                    pen.LineJoin = LineJoin.Round;
                    dest.DrawPath(pen, path);
                    MyGlobals.gTrans = 0;
                    pen.Dispose();
                }
            }

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

        public static Bitmap convertText(string txt1, string txt2, string fontName)
        {

            Font font1 = new Font(fontName, MyGlobals.fontSize1);
            Font font2 = new Font(fontName, MyGlobals.fontSize2);

            // Create the new image
            Bitmap bmp = new Bitmap(MyGlobals.OutputImageWidth, MyGlobals.OutputImageHeight);
            Graphics graphics = Graphics.FromImage(bmp);
            
            // fill the image with the blackness of space
            graphics.FillRectangle(Brushes.Black, 0, 0, bmp.Width, bmp.Height);

            // Measure the size of our title text
            SizeF stringSize = graphics.MeasureString(txt1, font1);

            //Reduce the string height by 40% to compact the main and sub-titles (plus the glyph mask assembly)
            int mheight = Convert.ToInt32((Double)stringSize.Height * .6);
            mheight += MyGlobals.blurFactor / 4;
            int mwidth = Convert.ToInt32((Double)stringSize.Width * .014);

            // draw the title text  
            drawBlurredText(graphics, Rectangle.Empty, 0, 0, txt1, font1);

            // create and render the glyph mask
            Font font3 = new Font("Arial", MyGlobals.fontSize2, FontStyle.Bold);
            StringBuilder sb = new StringBuilder("  \u25AA\\\\\\\\\\"); // Space + Unicode Black Square
                                                                        //  sb.Append(@"\\\\\");
            string glyphText = sb.ToString();
            graphics.PageUnit = GraphicsUnit.Pixel;
            stringSize = graphics.MeasureString(glyphText, font3);

            SolidBrush glyphBackFillBrush = new SolidBrush(MyGlobals.gmColor);

            // calculate the glyphRectangle
            Rectangle glyphRect = new Rectangle(0, mheight, (int)(stringSize.Width * 0.8f), (int)stringSize.Height);

            // draw the subtitle text on a black background
            stringSize = graphics.MeasureString(txt2, font2);
            drawBlurredText(graphics, Rectangle.Empty, glyphRect.Width + (int)(MyGlobals.blurFactor * 0.1f), mheight, txt2, font2);

            // draw the glyph
            graphics.FillRectangle(glyphBackFillBrush, glyphRect);
            drawGlyphText(graphics, glyphRect, 0, mheight, glyphText, font3);

            // clean up
            font1.Dispose();
            font2.Dispose();
            font3.Dispose();
            glyphBackFillBrush.Dispose();
            graphics.Flush();
            graphics.Dispose();
            return bmp;
        }

        public static void processPOIs() 
        {
            //Re-create all POIs using params read from "POIParams.xml"
            string FileName = "";
            string MainString = "";
            string SubString = "";

            using (XmlReader reader = XmlReader.Create(AppDomain.CurrentDomain.BaseDirectory + "\\..\\..\\..\\POIParams.xml"))
            {

                //Loop through the elements in the file
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        //  Read each element value by name and assign to variable
                        if (reader.Name.ToString().Equals("POIFileName"))
                        {
                            FileName = reader.ReadElementContentAsString();
                        }
                        if (reader.Name.ToString().Equals("POIMainString"))
                        {
                            MainString = reader.ReadElementContentAsString();
                        }
                        if (reader.Name.ToString().Equals("POISubString"))
                        {
                            SubString = reader.ReadElementContentAsString();

                            //Create POI .png files using data read from xml file
                            MyGlobals.fullBmp = imageTools.convertText(MainString.ToUpper(), SubString.ToUpper(), "Orbitron");
                            MyGlobals.fullBmp.Save(MyGlobals.savePath + "\\" + FileName, System.Drawing.Imaging.ImageFormat.Png);
                            MyGlobals.poiFileCount++; 
                            MyGlobals.fullBmp.Dispose();
                            MyGlobals.fullBmp = null;
                        }
                    }
                }
                reader.Dispose();
            }
        }
    }
}

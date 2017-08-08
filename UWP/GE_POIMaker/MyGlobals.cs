using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GE_POIMaker
{
    class MyGlobals
    {
        public static Color mtColor = Color.FromArgb(255, 157, 0, 0); //Main text default color
        public static Color stColor = Color.FromArgb(255, 157, 0, 0); //Sub text default color
        public static Color gColor = Color.FromArgb(255, 180, 255, 0); //Glyph text default color
        public static Color gmColor = Color.FromArgb(255, 0, 255, 0); // Glyph mask default color 
        public static int fontSize1 = 830; //Main title defsult font size
        public static int fontSize2 = 280; //Sub title default font size
        public static int OutputImageHeight = 2048; //Default image height
        public static int OutputImageWidth = 13662; //Default image width
        public static int gTrans = 1;  //initialize to nearly fully transparent
        public static int pwMod = (20); //This represents the amount the pen width is reduced by in each iteration
        public static int blurFactor = (400); //This is the initial pen width
        public static int gtMod = (2); //This represents the amount the pen transparency is adusted by each iteration
        public static Bitmap fullBmp; //bitmap object
        public static string savePath = Directory.GetCurrentDirectory(); //default savepath
        public static DateTime startTimer; //timer instrumentation
        public static DateTime endTimer; //timer instrumentation
        public static int poiFileCount = 0; //Files processed counter variable
    }
}

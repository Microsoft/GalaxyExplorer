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
using static GE_POIMaker.imageTools;

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
                /// <param name="fontSize1">Title text font size</param>
                /// <param name="fontSize2">Sub text font size</param>
                /// <param name="OutputImageHeight">Height of the final image</param>
                /// <param name="OutputImageWidth">Width of the final image</param>
                /// <param name="blurFactor">The text blur factor (amount of blur effect)</param>
                /// <param name="gTrans">The transparency ("a" channel in argb) setting</param>


                int fontSize1 = Convert.ToInt32(textBox4.Text);
                int fontSize2 = Convert.ToInt32(textBox5.Text);
                int OutputImageHeight = Convert.ToInt32(textBox8.Text);
                int OutputImageWidth = Convert.ToInt32(textBox7.Text);
                int blurFactor = Convert.ToInt32(textBox6.Text);
                int gTrans = Convert.ToInt32(textBox9.Text);

                Bitmap fullBmp = new Bitmap(imageTools.convertText(POIMainTitile.Text.ToUpper(), textBox2.Text.ToUpper(), "Orbitron", fontSize1, fontSize2, OutputImageWidth, OutputImageHeight));

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

        public void button2_Click(object sender, EventArgs e)
        {

            MyGlobals.mtColor = imageTools.colorPicker();
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    stColor = imageTools.colorPicker();
        //}

        private void button3_Click(object sender, EventArgs e)
        {
            MyGlobals.gColor = imageTools.colorPicker();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MyGlobals.gmColor = imageTools.colorPicker();
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.theleagueofmoveabletype.com/orbitron");

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {

                button2.Show();
                button3.Show();
                button5.Show();
                textBox4.Show();
                textBox5.Show();
                textBox6.Show();
                textBox7.Show();
                textBox8.Show();
                textBox9.Show();
                label7.Show();
                label8.Show();
                label9.Show();
                label10.Show();
                label11.Show();
                label12.Show();
            }
            else
            {
                button2.Hide();
                button3.Hide();
                button5.Hide();
                textBox4.Hide();
                textBox5.Hide();
                textBox6.Hide();
                textBox7.Hide();
                textBox8.Hide();
                textBox9.Hide();
                label7.Hide();
                label8.Hide();
                label9.Hide();
                label10.Hide();
                label11.Hide();
                label12.Hide();
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {

            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    textBox3.Text = saveFileDialog1.FileName;
                    myStream.Close();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var myForm = new Form2();
            myForm.Show();
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            
        }
    }
}

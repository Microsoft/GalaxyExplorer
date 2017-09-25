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
        }

        private void UpdateGlobals()
        {
            MyGlobals.fontSize1 = Convert.ToInt32(textBox4.Text);
            MyGlobals.fontSize2 = Convert.ToInt32(textBox5.Text);
            MyGlobals.OutputImageHeight = Convert.ToInt32(textBox8.Text);
            MyGlobals.OutputImageWidth = Convert.ToInt32(textBox7.Text);
            MyGlobals.blurFactor = Convert.ToInt32(textBox6.Text);
            MyGlobals.gTrans = Convert.ToInt32(textBox9.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap fullBmp = new Bitmap(imageTools.convertText(POIMainTitile.Text.ToUpper(), textBox2.Text.ToUpper(), "Orbitron"));

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

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox3.Text = Path.GetTempPath() + "POI_MyPOI.png";  //set the default save location to the users temp directory by default
            textBox4.Text = MyGlobals.fontSize1.ToString();
            textBox5.Text = MyGlobals.fontSize2.ToString();
            textBox6.Text = MyGlobals.blurFactor.ToString();
            textBox7.Text = MyGlobals.OutputImageWidth.ToString();
            textBox8.Text = MyGlobals.OutputImageHeight.ToString();
            textBox9.Text = MyGlobals.gTrans.ToString();
        }

        public void button2_Click(object sender, EventArgs e)
        {
            MyGlobals.mtColor = imageTools.colorPicker();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MyGlobals.gColor = imageTools.colorPicker();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MyGlobals.gmColor = imageTools.colorPicker();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.theleagueofmoveabletype.com/orbitron");

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            button2.Visible = checkBox1.Checked;
            button3.Visible = checkBox1.Checked;
            button5.Visible = checkBox1.Checked;
            textBox4.Visible = checkBox1.Checked;
            textBox5.Visible = checkBox1.Checked;
            textBox6.Visible = checkBox1.Checked;
            textBox7.Visible = checkBox1.Checked;
            textBox8.Visible = checkBox1.Checked;
            textBox9.Visible = checkBox1.Checked;
            label7.Visible = checkBox1.Checked;
            label8.Visible = checkBox1.Checked;
            label9.Visible = checkBox1.Checked;
            label10.Visible = checkBox1.Checked;
            label11.Visible = checkBox1.Checked;
            label12.Visible = checkBox1.Checked;
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
            UpdateGlobals();
            var myForm = new Form2();
            myForm.Show();
        }
    }
}

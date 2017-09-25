using System;
using System.IO;
using System.Windows.Forms;

namespace GE_POIMaker
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            try
            {
                //Set the current directory to the POI path relative to default current.
                Directory.SetCurrentDirectory("..\\..\\..\\..\\Assets\\UI\\POIs\\");
                MyGlobals.savePath = Directory.GetCurrentDirectory().ToString();
                textBox1.Text = MyGlobals.savePath;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("The specified directory does not exist. {0}", e);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = "...processing...please wait....";
            clickedButton.Enabled = false;

            Form3 from3 = new Form3();
            from3.Show();

            clickedButton.Text = "Done!";
           
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void browseButton_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog FolderBrowserDialog1 = new FolderBrowserDialog();
            FolderBrowserDialog1.SelectedPath = MyGlobals.savePath;

            if (FolderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = FolderBrowserDialog1.SelectedPath;
                MyGlobals.savePath = FolderBrowserDialog1.SelectedPath;
            }
        }

    }
}


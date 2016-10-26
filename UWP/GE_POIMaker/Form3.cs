using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace GE_POIMaker
{

    public partial class Form3 : Form
    {
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        public Form3()
        {
            InitializeComponent();

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(
            backgroundWorker1_ProgressChanged);
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 30;

            // Start the asynchronous operation.
            backgroundWorker1.RunWorkerAsync();
            label1.Text = ("Processing.....");

        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Capture the start time
            MyGlobals.startTimer = DateTime.Now;
           // execute POI process asynchronously
            imageTools.processPOIs();

        }
        // This event handler updates the progress.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            
        }

 
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                label1.Text = "Canceled!";
            }
            else if (e.Error != null)
            {
                label1.Text = "Error: " + e.Error.Message;
            }
            else
            {
                //Capture the end time
                MyGlobals.endTimer = DateTime.Now;
                //Calculate execution time
                TimeSpan exectuionTime = (MyGlobals.endTimer - MyGlobals.startTimer);
                //Write to log file in user temp 
                File.AppendAllText(Path.GetTempPath() + @"GE_POI_Log.txt", "\r\n Processed " + MyGlobals.poiFileCount + " .png files succsessfully written to : ," + MyGlobals.savePath + " ,processed in : ," + exectuionTime);
            
                label1.Text = "Done!";
                //Display result message box, ask to exit or continue
                if (MessageBox.Show(
                "Processed " +MyGlobals.poiFileCount + " .png files successfully written to: " + MyGlobals.savePath + " processed in : " + exectuionTime + "\n Exit application?", "What now?",
                 MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Application.Exit();
                }
                else
                {
                    MyGlobals.poiFileCount = 0;
                    Close();
                }
            }
        }
    }
}

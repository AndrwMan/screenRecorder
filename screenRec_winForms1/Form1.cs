using System;
using System.Drawing;
using System.Windows.Forms;

namespace screenRec_winForms1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Init variables:
        string outputPath = "";
        bool pathSelected = false;
        string finalVidName = "FinalVideo.mp4";

        // Screen recorder object:
        ScreenRecorder screenRec = new ScreenRecorder(new Rectangle(), "");

        private void Form1_Load(object sender, EventArgs e)
        {
            //no current functionalities upon Form loading
        }

        //StartBtn functionalities
        private void button1_Click(object sender, EventArgs e)
        {
            bool containsMP4 = finalVidName.Contains(".mp4");

            if (pathSelected && containsMP4)
            {
                //Cant set finalName = finalVidName directly since private variable
                //need to add public method
                screenRec.setVideoName(finalVidName);
                tmrRecord.Start();
            }
            else if (!pathSelected && containsMP4)
            {
                MessageBox.Show("You must select an output path first", "Error");
            }
            else if (pathSelected && !containsMP4)
            {
                MessageBox.Show("You must select video name that ends in '.mp4'", "Error");
                txtSetName.Text = "FinalVideo.mp4";
                finalVidName = "FinalVideo.mp4";
            }
            else
            {
                MessageBox.Show("You must select video name that ends in '.mp4' " +
                    "and you must select an output path", "Error");
                txtSetName.Text = "FinalVideo.mp4";
                finalVidName = "FinalVideo.mp4";
            }
        }

        //StopBtn functionalities
        private void button1_Click_1(object sender, EventArgs e)
        {
            tmrRecord.Stop();
            screenRec.StopRecord();
            //resets app when 1 video is done
            Application.Restart();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //no current functionalities when timer display clicked
        }

        private void selectFolder_Click(object sender, EventArgs e)
        {
            //Create output path:
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select an Output Folder";

            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputPath = @folderBrowser.SelectedPath;
                pathSelected = true;

                //Finish screen recorder object:
                Rectangle bounds = Screen.FromControl(this).Bounds;
                screenRec = new ScreenRecorder(bounds, outputPath);
            }
            else
            {
                MessageBox.Show("Please select an output folder.", "Error");
            }
        }

        private void tmrRecord_Tick(object sender, EventArgs e)
        {
            screenRec.RecordVid();
            screenRec.RecordAudio();
            lblTimer.Text = screenRec.GetElapsedTime();
        }

        private void txtSetName_TextChanged(object sender, EventArgs e)
        {
            //finalVidName = txtSetName.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            screenRec.cleanUp();
        }

    }
}
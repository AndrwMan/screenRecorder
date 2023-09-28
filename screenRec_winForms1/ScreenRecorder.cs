using System;
using System.Collections.Generic;

//need to modify graphics
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

//need for input-output handling
using System.IO;

//need for time-based features
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

//import FFMPEG functionalities
using Accord.Video.FFMPEG;

namespace screenRec_winForms1
{
    class ScreenRecorder
    {
        //public static string FrameworkDescription { get; }
        private Rectangle bounds;           //screen max dimensions
        private string outputPath = "";     //user-specified directory to save vid
        private string tempPath = "";       //directory for screenshots to be stitched as vid
        private int imgNum = 1;             //counter appended to imgName to uniquely ID img
        private List<string> inputImgs = new List<string>(); //array storing imgNames to be stitched as vid

        private string videoName = "video.mp4";         //separate video-only output
        private string audioName = "audio.wav";         //separate audio-only output
        private string finalName = "finalVideo.mp4";    //user-editable name for merged output

        Stopwatch watch = new Stopwatch();

        //record audio
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

        //main control driver
        public ScreenRecorder(Rectangle currWindow, string currOutPath)
        {
            CreateTempFolder("tempScreenshots");

            bounds = currWindow;
            //bounds = GetScaledScreenBounds();
            outputPath = currOutPath;
        }

        public void SetBounds(Rectangle newBounds, string currOutPath)
        {
            Console.Write(newBounds); 
            //bounds = newBounds
            bounds = GetScaledScreenBounds();
            Console.Write("done");
            outputPath = currOutPath;
        }


        //Adjust for screen scaling and resolution
        public Rectangle GetScaledScreenBounds()
        {
            Screen primaryScreen = Screen.PrimaryScreen;
            //FIXME: for variable resolution both approaches are unable to 
            // correctly get scale factor needed to calc scaled width/height

            //it turns out the entire issue is not having any way 
            //of getting true scaled width & height directly...

            //float dpiX = primaryScreen.Bounds.Width / primaryScreen.WorkingArea.Width;
            //float dpiY = primaryScreen.Bounds.Height / primaryScreen.WorkingArea.Height;
            //Console.Write("dpiX" + dpiX);
            //Console.Write("dpiY" + dpiY);

            int origWidth = primaryScreen.Bounds.Width;
            int origHeight = primaryScreen.Bounds.Height; 
            //int scaledWidth = (int)Math.Round(primaryScreen.Bounds.Width / dpiX);
            //int scaledHeight = (int)Math.Round(primaryScreen.Bounds.Height / dpiY);
            //Console.Write("scaledWidth" + scaledWidth);
            //Console.Write("scaledHeight" + scaledHeight)

            int desiredWidth = 1920;  // width after applying true scale factor
            int desiredHeight = 1080; // height after applying true scale factor
                
            //ratio gives true scale factors
            float widthScaleFactor = (float)desiredWidth / origWidth;
            float heightScaleFactor = (float)desiredHeight / origHeight;
            //float widthScaleFactor = Screen.PrimaryScreen.Bounds.Width * dpiX;
            //float heightScaleFactor = Screen.PrimaryScreen.Bounds.Height * dpiY;

            int adjustedWidth = (int)Math.Round(origWidth * widthScaleFactor);
            int adjustedHeight = (int)Math.Round(origHeight * heightScaleFactor);

            //float scaleFactor = GetScalingFactor();
            //int adjustedWidth = (int)Math.Round(desiredWidth * scaleFactor);
            //int adjustedHeight = (int)Math.Round(desiredHeight * scaleFactor);

            return new Rectangle(primaryScreen.Bounds.Left, primaryScreen.Bounds.Top, adjustedWidth, adjustedHeight);
            //return new Rectangle(primaryScreen.Bounds.Left, primaryScreen.Bounds.Top, scaledWidth, scaledHeight);
        }

        //explictly calculate scale factor to 
        // verify against primaryScreen approach
        private static float GetScalingFactor()
        {
            float dpiX, dpiY;

            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }

            // Calculate the scaling factor (assuming X and Y scaling factors are the same)
            float scalingFactor = dpiX / 96.0f;
            Console.Write("scalingFactor" + scalingFactor);

            return scalingFactor;
        }


        //TODO: styled docs for every function
        private void CreateTempFolder(string name)
        {
            //prioritize temp dir creation on D drive
            //(typcial for mass data storage)
            if (Directory.Exists("D://"))
            {
                string pathName = $"D://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
            else
            {
                string pathName = $"C://{name}";
                Directory.CreateDirectory(pathName);
                tempPath = pathName;
            }
        }

        private void ClearTempFolder(string targetDir)
        {
            string[] storedImgs = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            //set permissions, then delete all imgs
            foreach (string img in storedImgs)
            {
                File.SetAttributes(img, FileAttributes.Normal);
                File.Delete(img);
            }

            //recursively delete all paths
            foreach (string dir in dirs)
            {
                ClearTempFolder(dir);
            }

            //delete top-lvl, parent folder only
            Directory.Delete(targetDir, false);
        }

        private void ClearOutputFolder(string targetDir, string keepFile)
        {
            string[] files = Directory.GetFiles(targetDir);

            //delete every file that is Not final output
            foreach (string file in files)
            {
                if (file != keepFile)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
            }
        }

        public void cleanUp()
        {
            //handles unexpected app closure
            //clear lingering files 
            Console.Write("CLEANUP ENTERED");
            if (Directory.Exists(tempPath))
            {
                ClearTempFolder(tempPath);
            }
        }

        public string GetElapsedTime()
        {
            //tracks time in HH/MM/SS format
            return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
        }

        public void RecordVid()
        {
            watch.Start();

            //handle entire frames creation in RecordVid() function

            //Calc frame capture interval in millisecs based on desired frame rate
            // redundant with tmrRecord_Tick() 
            //int frameCaptureInterval = 1000 / 60; 
            //while (true)

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                //determine where to capture
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }

                //programmaticaly generate img file (& and their names) 
                string imgPath = tempPath + "//screenshot-" + imgNum + ".png";
                bitmap.Save(imgPath, ImageFormat.Png);
                inputImgs.Add(imgPath);
                imgNum++;

                bitmap.Dispose();
            }

            // Check if we have reached the desired recording duration
            // but do not know recordingDuration until recording finishes
            // a problem when we need record frames in real time
            /*if (watch.Elapsed.TotalSeconds >= recordingDuration)
                break;*/

            // Sleep for a short time to control the frame capture rate
            //Thread.Sleep(16); 
        }

        public void RecordAudio()
        {
            //string is Not arbitary, representative of lpstrCommand
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsound", "", 0, 0);
        }

        //private void SaveVid(int width, int height, int frameRate)
        private void SaveVid(int width, int height, int frameRate, int numFrames)
        {

            using (VideoFileWriter vidWriter = new VideoFileWriter())
            {
                //using MPEG to be consistent w/ the file extension we appended in videoName
                vidWriter.Open(outputPath + "//" + videoName, width, height, frameRate, VideoCodec.MPEG4);

                //use all imgs to construct vid
                // numFrames ~= inputImgs.Count when desiredFrameRate = frameRate * 6.5 
                Console.Write("numFrames" + numFrames);
                Console.Write("inputImgs.Count" + inputImgs.Count);

                //foreach (string imgPath in inputImgs)
                for (int i = 0; i < numFrames && i < inputImgs.Count; i++)
                {
                    //must use `as` type conversion to bitmap to avoid error
                    //Bitmap currFrame = System.Drawing.Image.FromFile(imgPath) as Bitmap;
                    Bitmap currFrame = System.Drawing.Image.FromFile(inputImgs[i]) as Bitmap;
                    vidWriter.WriteVideoFrame(currFrame);
                    currFrame.Dispose();
                }

                vidWriter.Close();
            }

        }

        private void SaveAudio()
        {
            string audioPath = "save recsound " + outputPath + "//" + audioName;
            NativeMethods.record(audioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }

        private void CombineVidAudio(string video, string audio)
        {
            string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName} ";
            //create temp cmd prompt to run the ffmpeg cmd
            //must ensure dir is correct so that cmd can find all args
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = command
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
            }
        }

        public void StopRecord()
        {
            //orchestrate entire recording creation after stop
            watch.Stop();

            int width = bounds.Width;
            int height = bounds.Height;
            //the rate which approximately worked before adding 
            // tmrRecord.Interval and recordingDuration adjustments
            //int frameRate = 35;       
            //the rate which works w/ tmrRecord.Interval, recordingDuration adjustments
            // needs to be ~6 times less than desired frameRate in tmrRecord.Interval
            int frameRate = 10;

            // Get recording duration from the Stopwatch()
            double recordingDuration = watch.Elapsed.TotalSeconds;
            Console.Write("recordingDuration:" +  recordingDuration);

            //alternative method: but tmrRecord is not avaliable
            //double recordingDuration2 = tmrRecord.TickCount * (tmrRecord.Interval / 1000.0);
            //Console.Write("recordingDuration2:" + recordingDuration2);

            // Calculate the number of frames to include in the video
            int numFrames = (int)(recordingDuration * frameRate);
            Console.Write("numFrames:" + numFrames);

            SaveAudio();
            //SaveVid(width, height, frameRate);
            SaveVid(width, height, frameRate, numFrames); 
            CombineVidAudio(videoName, audioName);

            ClearTempFolder(tempPath);
            ClearOutputFolder(outputPath, outputPath + "\\" + finalName);
        }
        public void setVideoName(string name)
        {
            finalName = name;
        }
    }
}
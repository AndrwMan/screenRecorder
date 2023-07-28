using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//need to modify graphics
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

//need for input-output handling
using System.IO;

//need for time-based features
using System.Runtime.InteropServices;

//import FFMPEG functionalities
using Accord.Video.FFMPEG;

namespace ScreenRec
{
	class ScreenRecorder
	{
		//public static string FrameworkDescription { get; }
		private Rectangle bounds;			//screen max dimensions
		private string outputPath = ""; 	//user-specified directory to save vid
		private string tempPath = "";		//directory where screenshots to be stitched as vid
		private int imgNum = 1;				//counter appended to imgName to uniquely ID img
		private List<string> inputImgs = new List<string>(); //array storing imgNames to be stitched as vid
	
		private string videoName = "video.mp4";			//separate video-only output
		private string audioName = "audio.wav";			//separate audio-only output
		private string finalName = "finalVideo.mp4";	//user-editable name for merged output

		Stopwatch watch = new Stopwatch();

		//record audio
		public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }

		//main control driver
		public ScreenRecorder(Rectangle currWindow, string currOutPath) {
			CreateTempFolder("tempScreenshots");

			bounds = currWindow;
			outputPath = currOutPath;
		}

		//TODO: docs
		private void CreateTempFolder(string name) {
			//prioritize temp dir creation on D drive
			//(typcial for mass data storage)
			if (Directory.Exists("D://")) {
				string pathName = $"D://{name}";
				Directory.CreateDirectory(pathName);
				tempPath = pathName;
			}
			else {
				string pathName = $"C://{name}";
				Directory.CreateDirectory(pathName);
				tempPath = pathName; 
			}
		}

		private void ClearTempFolder(string targetDir) {
			string[] storedImgs = Directory.GetFiles(targetDir);
			string[] dirs = Directory.GetFiles(targetDir);

			//set permissions, then delete all imgs
			foreach(string img in storedImgs) {
				File.SetAttributes(img, FileAttributes.Normal);
				File.Delete(img);
			}

			//recursively delete all paths
			foreach(string dir in dirs) {
				ClearTempFolder(dir); 
			}

			//delete top-lvl, parent folder only
			Directory.Delete(targetDir, false);	
		}

		private void ClearOutputFolder(string targetDir, string keepFile) {
			string[] files = Directory.GetFiles(targetDir);

			//delete every file that is Not final output
			foreach(string file in files) {
				if(file != keepFile) {
					File.SetAttributes(file, FileAttributes.Normal);
					File.Delete(file);
				}
			}
		}

		public void cleanUp() {
			//handles unexpected app closure
			//clear lingering files 
			if(Directory.Exists(tempPath)) {
				ClearTempFolder(tempPath);
			}
		}

		public string GetElapsedTime() {
			//tracks time in HH/MM/SS format
			return string.Format("{0:D2}:{1:D2}:{2:D2}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
		}

		public void RecordVid(){

			using(Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height)) {
				//determine where to capture
				using(Graphics graphics = Graphics.FromImage(bitmap)) {
					graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
				}

				//programmaticaly generate img file (& and their names) 
				string imgName = tempPath + "//screenshot-" + imgNum + ".png";
				bitmap.Save(imgName, ImageFormat.Png);
				inputImgs.Add(imgName);
				imgNum++;

				bitmap.Dispose();
			}
		}
	}
}
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
		private string outputPath = ""; 	//user-specified location to save vid
		private string tempPath = "";		//location where screenshots to be stitched as vid
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
	}
}

## A simple full screen recorder

## Motivation:
* Since I anticipate recording running GUI and simulations in action, 
I figure it is a good oppurtunity to learn to code a screen recorder 
(as opposed to outright using OBS as the easy solution)
* Good pratice of ffmpeg-based library that is commonly used to handle audio and video files
(I was correct about this; my MixMatch group project later on used ffmpeg binaries)
* Exposure to C# language and .NET environment

## Usage
Intended for personal use. Still, the current build process involves the following:

* Confirm that your project is set to target .NET Framework 4.7.2 
(as mentioned in the previous answer). You can do this by right-clicking 
on the project in Solution Explorer, selecting "Properties," and going to 
the "Application" tab.

* (Optional) Clean the Solution to remove any previous build artifacts. 
Right-click on the solution in Solution Explorer and select "Clean."

* Open a command prompt or terminal and 
navigate to the project's root directory where .sln file is located.
Run `nuget restore screenRec_winForms1.sln` to download and restore NuGet packages. 

* Return to Visual Studio. Right-click on the solution in Solution Explorer and 
select "Build" or "Rebuild." Building the solution will compile source code 
and create executable files or libraries based on project settings.

* After the build process completes, check the "Output" window for any build 
errors or warnings. Fix any issues that are reported.

  * build output files (i.e., EXE files, DLLs) will be located in the project's 
    "bin" directory (screenRec_winForms1\bin).
  
* In Visual Studio 'Start' to run executable. In GUI, type in desired file name 
and choose output video location then click start 'Start' button. 
Similarly, dlick 'Stop' when finished and check the destination folder 
selected earlier.

### License
**Copyright (c) [Andrew Man] [2019-2023]. All Rights Reserved.**
# MRTK-Quest
MRTK-Quest is a Mixed Reality Toolkit (MRTK) extension for Oculus Quest, now with support for Rift/Rift S as well.
It was built to showcase the hand-driven interaction model designed by Microsoft for HoloLens 2, on the Oculus ecosystem.

## Main features
- Full support for articulated hand tracking, and simulated hand tracking using controllers with avatar hands.
- Support for Oculus Link on Quest with controllers, which means rapid iteration without builds.
- Full support for any interaction in the MRTK designed to work for HoloLens 2.

## Demo Video
[![Demo video](https://i.imgur.com/wWzTaAw.png)](https://twitter.com/prvncher/status/1211768281536847872)

# Supported versions
- Unity 2018.4.x (Currently targetting 2018.4.14f1)
- Oculus Integration 12.0
- Mixed Reality Toolkit v2.2.0+

# Supported target devices
- Oculus Rift/S - Windows Standalone
- Oculus Quest  - Android / Windows Standalone w/ Link

# Getting started with my fork
## 1. Clone this repo
Clone this repository, and then make sure to initialize submodules.
To do this, open a command line terminal, rooted on the folder you'd like the project to be in. 
(Hold shift + right click -> Select "Open Powershell Window Here")

Then clone using this command "git clone --recurse-submodules https://github.com/provencher/MRTK-Quest.git"

This will the official MRTK development branch as well. If you'd like your own version of MRTK, simply remove "--recurse-submodules" from the command, and copy your MRTK files to the External folder, before proceeding to step 2.

## 2. Run SymLink bat
Run bat External/createSymlink.bat by double clicking it.
This will link the MRTK folders cloned via the submodule into the project.

## 3. Import Oculus Integration
Download Oculus Integration 12.0 from Asset Store and import it.
- Alternatively just drag and drop the Oculus folder into Assets/

## 4. Project Configuration Window
MRTK has a Project Configuration modal window that pops up when you first open a project.
In this window, there is a checkbox for MSBuild, which will attempt to add MSBuild to your manifest.json that then adds various DLLs to your project via NuGET.
If like myself, your git folder is not in your drive root, you may run into [errors](https://github.com/microsoft/MixedRealityToolkit-Unity/issues/6972) as I have. For now, it seems that avoiding MSBuild does not raise any problems, but that may change in the future.

## 5. Configure Oculus Avatar
To be able to see your avatar hands/controllers, you **must** set an App Id by going to `Oculus > Avatars > Edit Settings`. During development, you can use a random number (such as 12345) but for release you must use a valid App Id. Please check the [Oculus Avatar SDK official documentation](https://developer.oculus.com/documentation/unity/as-avatars-sdk-intro/) for more information.


# Author
Eric Provencher [@prvncher](https://twitter.com/prvncher)

Modified from: 
Furuta, Yusuke ([@tarukosu](https://twitter.com/tarukosu))

# License
MIT

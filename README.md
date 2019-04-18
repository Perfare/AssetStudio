# AssetStudio
[![Build status](https://ci.appveyor.com/api/projects/status/rnu7l90422pdewx4?svg=true)](https://ci.appveyor.com/project/Perfare/assetstudio/branch/master/artifacts)

**None of the repo, the tool, nor the repo owner is affiliated with, or sponsored or authorized by, Unity Technologies or its affiliates.**

AssetStudio is a tool for exploring, extracting and exporting assets and assetbundles.

## Features
* Support version:
  * 2.5 - 2019.1
* Support asset types:
  * **Texture2D** : support convert to bmp, png or jpeg. export to containers: DDS, PVR and KTX
  * **Sprite** : bmp, png or jpeg
  * **AudioClip** : mp3, ogg, wav, m4a, fsb. support convert FSB file to WAV(PCM)
  * **Font**
  * **Mesh** : obj
  * **TextAsset**
  * **Shader**
  * **MovieTexture**
  * **VideoClip**
  * **MonoBehaviour**
  * **Animator** : export to FBX file with bound AnimationClip

## Usage
### Requirements

- [.NET Framework 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=17718)
- [Microsoft Visual C++ 2013 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=40784)
- [Microsoft Visual C++ 2015 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=53840)

### How to use

* Use **File-Load file**, **File-Load folder** to load assets or assetbundles from multiple files or folder  
* Use **File-Extract file**, **File-Extract folder** to export assetbundles to assets from multiple files or folder  
* Export assets: use **Export** menu  
* Export model:  
  * Export model from "Scene Hierarchy" using the **Model** menu  
  * Export Animator from "Asset List" using the **Export** menu  
  * With AnimationClip:
    * Select model from "Scene Hierarchy" then select the AnimationClip from "Asset List", using **Model-Export selected objects with AnimationClip** to export
    * Export Animator will export bound AnimationClip or use **Ctrl** to select Animator and AnimationClip from "Asset List", using **Export-Export Animator with selected AnimationClip** to export
  
## Build

* The project uses some C# 7 syntax, need Visual Studio 2017 or newer
* **AssetStudioFBX** uses FBX SDK 2019.0 VS2015, before building, you need to install the FBX SDK and modify the project file, change include directory and library directory to point to the FBX SDK directory
* If you want to change the FBX SDK version, you need to replace `libfbxsdk.dll` which in `AssetStudioGUI/Libraries/x86/` and `AssetStudioGUI/Libraries/x64` directory to the new version

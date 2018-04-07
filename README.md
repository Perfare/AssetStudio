# AssetStudio
Latest build: [![Build status](https://ci.appveyor.com/api/projects/status/rnu7l90422pdewx4?svg=true)](https://ci.appveyor.com/project/Perfare/assetstudio/branch/master/artifacts)

**None of the repo, the tool, nor the repo owner is affiliated with, or sponsored or authorized by, Unity Technologies or its affiliates.**

AssetStudio is a tool for exploring, extracting and exporting assets and assetbundles. It has been tested with builds from most platforms, ranging from Web, PC, Linux, MacOS to Xbox360, PS3, Android and iOS, and it is currently maintained to be compatible with assets from 2.5 up to the 2017.4 version.

## Features

* Support asset types:  
  * **Texture2D** : support convert to bmp, png or jpeg. export to containers: DDS, PVR and KTX  
  * **Sprite** : bmp, png or jpeg  
  * **AudioClip** : mp3, ogg, wav, m4a, fsb. support convert FSB file to WAV(PCM)  
  * **Font** : ttf, otf  
  * **Mesh** : obj  
  * **TextAsset** : txt
  * **Shader**
  * **MovieTexture** : ogv
  * **VideoClip**
  * **MonoBehaviour**
* Export model to FBX format, with complete hierarchy, transformations, materials and textures. Geometry is exported with normals, tangents, UV coordinates, vertex colors and deformers. Skeleton nodes can be exported either as bones or dummy deformers.
* Real-time preview window for the above-mentioned assets
* Diagnostics mode with useful tools for research

## Usage
### Requirements

- [.NET Framework 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=17718)
- [Microsoft Visual C++ 2013 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=40784)
- [Microsoft Visual C++ 2015 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=53840)

### UI guide

| Item                          | Action
| :---------------------------- | :----------------------------
| File -> Load file/folder      | Open Assetfiles and load their assets. Load file can also decompress and load bundle files straight into memory
| File -> Extract bundle/folder | Extract Assetfiles from bundle files compressed with lzma or lz4
| Scene Hierarchy search box    | Search nodes using * and ? wildcards. Press Enter to loop through results or Ctrl+Enter to select all matching nodes
| Asset List filter box         | Enter a keyword to filter the list of available assets; wildcards are added automatically
| Diagnostics                   | press Ctrl+Alt+D to bring up a hidden menu and a new list
| Bulid class structures        | Create human-readable structures for each type of asset

Other interface elements have tooltips or are self-explanatory.

## Build

* The project uses some C# 7 syntax, need Visual Studio 2017
* **AssetStudioFBX** uses FBX SDK 2015.1, before building, you need to install the FBX SDK and modify the project file, change include directory and library directory to point to the FBX SDK directory
* If you want to change the FBX SDK version, you need to replace `libfbxsdk.dll` which in `AssetStudio/Library/x86/` and `x64` directory to the new version

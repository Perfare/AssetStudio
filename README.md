# UnityStudio
Latest build: [![Build status](https://ci.appveyor.com/api/projects/status/amw5n3607g45n2v0?svg=true)](https://ci.appveyor.com/project/Perfare/unitystudio/branch/master/artifacts)

Unity Studio is a tool for exploring, extracting and exporting assets from Unity games and apps. It has been tested with Unity builds from most platforms, ranging from Web, PC, Linux, MacOS to Xbox360, PS3, Android and iOS, and it is currently maintained to be compatible with Unity builds from 2.5 up to the 2017.3 version.

## Features

* Support Unity3D Asset types:  
  * **Texture2D** : support convert to bmp, png or jpeg. export to containers: DDS, PVR and KTX  
  * **Sprite** : png  
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

### Requirements:

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
| Bulid class structures        | Create human-readable structures for each type of Unity asset

Other interface elements have tooltips or are self-explanatory.

## Build
Program uses some C# 7 syntax, need Visual Studio 2017  

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Unity_Studio.UnityStudio;

namespace Unity_Studio
{
    static class UnityImporter
    {
        public static List<string> unityFiles = new List<string>(); //files to load
        public static HashSet<string> unityFilesHash = new HashSet<string>(); //to improve the loading speed
        public static HashSet<string> assetsfileListHash = new HashSet<string>(); //to improve the loading speed

        public static void LoadFile(string fullName)
        {
            switch (CheckFileType(fullName, out var reader))
            {
                case FileType.AssetsFile:
                    LoadAssetsFile(fullName, reader);
                    break;
                case FileType.BundleFile:
                    LoadBundleFile(fullName, reader);
                    break;
                case FileType.WebFile:
                    LoadWebFile(fullName, reader);
                    break;
            }
        }

        private static void LoadAssetsFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            StatusStripUpdate("Loading " + fileName);
            if (!assetsfileListHash.Contains(fileName.ToUpper()))
            {
                var assetsFile = new AssetsFile(fullName, reader);
                if (assetsFile.valid)
                {
                    assetsfileList.Add(assetsFile);
                    assetsfileListHash.Add(assetsFile.upperFileName);

                    #region for 2.6.x find mainData and get string version
                    if (assetsFile.fileGen == 6 && fileName != "mainData")
                    {
                        var mainDataFile = assetsfileList.Find(aFile => aFile.fileName == "mainData");
                        if (mainDataFile != null)
                        {
                            assetsFile.m_Version = mainDataFile.m_Version;
                            assetsFile.version = mainDataFile.version;
                            assetsFile.buildType = mainDataFile.buildType;
                        }
                        else if (File.Exists(Path.GetDirectoryName(fullName) + "\\mainData"))
                        {
                            mainDataFile = new AssetsFile(Path.GetDirectoryName(fullName) + "\\mainData", new EndianBinaryReader(File.OpenRead(Path.GetDirectoryName(fullName) + "\\mainData")));
                            assetsFile.m_Version = mainDataFile.m_Version;
                            assetsFile.version = mainDataFile.version;
                            assetsFile.buildType = mainDataFile.buildType;
                        }
                    }
                    #endregion

                    int value = 0;
                    foreach (var sharedFile in assetsFile.sharedAssetsList)
                    {
                        var sharedFilePath = Path.GetDirectoryName(fullName) + "\\" + sharedFile.fileName;
                        var sharedFileName = sharedFile.fileName;

                        if (!unityFilesHash.Contains(sharedFileName.ToUpper()))
                        {
                            if (!File.Exists(sharedFilePath))
                            {
                                var findFiles = Directory.GetFiles(Path.GetDirectoryName(fullName), sharedFileName, SearchOption.AllDirectories);
                                if (findFiles.Length > 0)
                                {
                                    sharedFilePath = findFiles[0];
                                }
                            }

                            if (File.Exists(sharedFilePath))
                            {
                                unityFiles.Add(sharedFilePath);
                                unityFilesHash.Add(sharedFileName.ToUpper());
                                value++;
                            }
                        }
                    }
                    if (value > 0)
                        ProgressBarMaximumAdd(value);
                }
            }
        }

        private static void LoadBundleFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            StatusStripUpdate("Decompressing " + fileName);
            var bundleFile = new BundleFile(reader);
            foreach (var file in bundleFile.fileList)
            {
                if (!assetsfileListHash.Contains(file.fileName.ToUpper()))
                {
                    StatusStripUpdate("Loading " + file.fileName);
                    var assetsFile = new AssetsFile(Path.GetDirectoryName(fullName) + "\\" + file.fileName, new EndianBinaryReader(file.stream));
                    if (assetsFile.valid)
                    {
                        assetsFile.bundlePath = fullName;

                        if (assetsFile.fileGen == 6) //2.6.x and earlier don't have a string version before the preload table
                        {
                            //make use of the bundle file version
                            assetsFile.m_Version = bundleFile.versionEngine;
                            assetsFile.version = Array.ConvertAll((from Match m in Regex.Matches(assetsFile.m_Version, @"[0-9]") select m.Value).ToArray(), int.Parse);
                            assetsFile.buildType = bundleFile.versionEngine.Split(AssetsFile.buildTypeSplit, StringSplitOptions.RemoveEmptyEntries);
                        }

                        assetsfileList.Add(assetsFile);
                        assetsfileListHash.Add(assetsFile.upperFileName);
                    }
                    else
                    {
                        resourceFileReaders.Add(assetsFile.upperFileName, assetsFile.assetsFileReader);
                    }
                }
            }
            reader.Dispose();
        }

        private static void LoadWebFile(string fullName, EndianBinaryReader reader)
        {
            var fileName = Path.GetFileName(fullName);
            StatusStripUpdate("Loading " + fileName);
            var bundleFile = new WebFile(reader);
            reader.Dispose();
            foreach (var file in bundleFile.fileList)
            {
                var dummyName = Path.GetDirectoryName(fullName) + "\\" + file.fileName;
                switch (CheckFileType(file.stream, out reader))
                {
                    case FileType.AssetsFile:
                        LoadAssetsFile(dummyName, reader);
                        break;
                    case FileType.BundleFile:
                        LoadBundleFile(dummyName, reader);
                        break;
                    case FileType.WebFile:
                        LoadWebFile(dummyName, reader);
                        break;
                }
                resourceFileReaders.Add(file.fileName.ToUpper(), reader);
            }
        }

        public static void MergeSplitAssets(string dirPath)
        {
            string[] splitFiles = Directory.GetFiles(dirPath, "*.split0");
            foreach (var splitFile in splitFiles)
            {
                string destFile = Path.GetFileNameWithoutExtension(splitFile);
                string destPath = Path.GetDirectoryName(splitFile) + "\\";
                var destFull = destPath + destFile;
                if (!File.Exists(destFull))
                {
                    string[] splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                    using (var destStream = File.Create(destFull))
                    {
                        for (int i = 0; i < splitParts.Length; i++)
                        {
                            string splitPart = destFull + ".split" + i;
                            using (var sourceStream = File.OpenRead(splitPart))
                                sourceStream.CopyTo(destStream);
                        }
                    }
                }
            }
        }
    }
}

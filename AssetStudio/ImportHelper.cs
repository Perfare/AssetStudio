﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetStudio
{
    public enum FileType
    {
        AssetsFile,
        BundleFile,
        WebFile
    }

    public static class ImportHelper
    {
        public static void MergeSplitAssets(string path, bool allDirectories = false)
        {
            var splitFiles = Directory.GetFiles(path, "*.split0", allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var splitFile in splitFiles)
            {
                var destFile = Path.GetFileNameWithoutExtension(splitFile);
                var destPath = Path.GetDirectoryName(splitFile) + "\\";
                var destFull = destPath + destFile;
                if (!File.Exists(destFull))
                {
                    var splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                    using (var destStream = File.Create(destFull))
                    {
                        for (int i = 0; i < splitParts.Length; i++)
                        {
                            var splitPart = destFull + ".split" + i;
                            using (var sourceStream = File.OpenRead(splitPart))
                            {
                                sourceStream.CopyTo(destStream);
                            }
                        }
                    }
                }
            }
        }

        public static string[] ProcessingSplitFiles(List<string> selectFile)
        {
            var splitFiles = selectFile.Where(x => x.Contains(".split"))
                .Select(x => Path.GetDirectoryName(x) + "\\" + Path.GetFileNameWithoutExtension(x))
                .Distinct()
                .ToList();
            selectFile.RemoveAll(x => x.Contains(".split"));
            foreach (var file in splitFiles)
            {
                if (File.Exists(file))
                {
                    selectFile.Add(file);
                }
            }
            return selectFile.Distinct().ToArray();
        }

        public static FileType CheckFileType(Stream stream, out EndianBinaryReader reader)
        {
            reader = new EndianBinaryReader(stream);
            return CheckFileType(reader);
        }

        public static FileType CheckFileType(string fileName, out EndianBinaryReader reader)
        {
            var file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite|FileShare.Delete|FileShare.Inheritable);
            reader = new EndianBinaryReader(file);
            return CheckFileType(reader);
        }

        private static FileType CheckFileType(EndianBinaryReader reader)
        {
            var signature = reader.ReadStringToNull(20);
            reader.Position = 0;
            switch (signature)
            {
                case "UnityWeb":
                case "UnityRaw":
                case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                default:
                    {
                        var magic = reader.ReadBytes(2);
                        reader.Position = 0;
                        if (WebFile.gzipMagic.SequenceEqual(magic))
                        {
                            return FileType.WebFile;
                        }
                        reader.Position = 0x20;
                        magic = reader.ReadBytes(6);
                        reader.Position = 0;
                        if (WebFile.brotliMagic.SequenceEqual(magic))
                        {
                            return FileType.WebFile;
                        }
                        return FileType.AssetsFile;
                    }
            }
        }
    }
}

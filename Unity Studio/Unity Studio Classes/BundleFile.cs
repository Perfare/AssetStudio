﻿using System;
using System.Collections.Generic;
using System.IO;
using Lz4;

namespace Unity_Studio
{
    public class BundleFile
    {
        public int format;
        public string versionPlayer;
        public string versionEngine;
        public List<MemoryAssetsFile> MemoryAssetsFileList = new List<MemoryAssetsFile>();

        public class MemoryAssetsFile
        {
            public string fileName;
            public MemoryStream memStream;
        }

        public BundleFile(string fileName)
        {
            if (Path.GetExtension(fileName) == ".lz4")
            {
                byte[] filebuffer;

                using (BinaryReader lz4Stream = new BinaryReader(File.OpenRead(fileName)))
                {
                    int version = lz4Stream.ReadInt32();
                    int uncompressedSize = lz4Stream.ReadInt32();
                    int compressedSize = lz4Stream.ReadInt32();
                    int something = lz4Stream.ReadInt32(); //1

                    byte[] lz4buffer = new byte[compressedSize];
                    lz4Stream.Read(lz4buffer, 0, compressedSize);

                    using (var inputStream = new MemoryStream(lz4buffer))
                    {
                        var decoder = new Lz4DecoderStream(inputStream);
                        filebuffer = new byte[uncompressedSize];
                        decoder.Read(filebuffer, 0, uncompressedSize);
                        decoder.Dispose();
                    }
                }
                using (var b_Stream = new EndianStream(new MemoryStream(filebuffer), EndianType.BigEndian))
                {
                    readBundle(b_Stream);
                }
            }
            else
            {
                using (var b_Stream = new EndianStream(File.OpenRead(fileName), EndianType.BigEndian))
                {
                    readBundle(b_Stream);
                }
            }
        }

        private void readBundle(EndianStream b_Stream)
        {
            var signature = b_Stream.ReadStringToNull();

            if (signature == "UnityWeb" || signature == "UnityRaw" || signature == "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
            {
                format = b_Stream.ReadInt32();
                versionPlayer = b_Stream.ReadStringToNull();
                versionEngine = b_Stream.ReadStringToNull();
                if (format < 6)
                {
                    int bundleSize = b_Stream.ReadInt32();
                }
                else if (format == 6)
                {
                    ReadFormat6(b_Stream, true);
                    return;
                }
                short dummy2 = b_Stream.ReadInt16();
                int offset = b_Stream.ReadInt16();
                int dummy3 = b_Stream.ReadInt32();
                int lzmaChunks = b_Stream.ReadInt32();

                int lzmaSize = 0;
                long streamSize = 0;

                for (int i = 0; i < lzmaChunks; i++)
                {
                    lzmaSize = b_Stream.ReadInt32();
                    streamSize = b_Stream.ReadInt32();
                }

                b_Stream.Position = offset;
                switch (signature)
                {
                    case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA": //.bytes
                    case "UnityWeb":
                        {
                            byte[] lzmaBuffer = new byte[lzmaSize];
                            b_Stream.Read(lzmaBuffer, 0, lzmaSize);

                            using (var lzmaStream = new EndianStream(SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(new MemoryStream(lzmaBuffer)), EndianType.BigEndian))
                            {
                                getFiles(lzmaStream, 0);
                            }
                            break;
                        }
                    case "UnityRaw":
                        {
                            getFiles(b_Stream, offset);
                            break;
                        }
                }
            }
            else if (signature == "UnityFS")
            {
                format = b_Stream.ReadInt32();
                versionPlayer = b_Stream.ReadStringToNull();
                versionEngine = b_Stream.ReadStringToNull();
                if (format == 6)
                {
                    ReadFormat6(b_Stream);
                }
            }
        }

        private void getFiles(EndianStream f_Stream, int offset)
        {
            int fileCount = f_Stream.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                MemoryAssetsFile memFile = new MemoryAssetsFile();
                memFile.fileName = f_Stream.ReadStringToNull();
                int fileOffset = f_Stream.ReadInt32();
                fileOffset += offset;
                int fileSize = f_Stream.ReadInt32();
                long nextFile = f_Stream.Position;
                f_Stream.Position = fileOffset;

                byte[] buffer = new byte[fileSize];
                f_Stream.Read(buffer, 0, fileSize);
                memFile.memStream = new MemoryStream(buffer);
                MemoryAssetsFileList.Add(memFile);
                f_Stream.Position = nextFile;
            }
        }

        private void ReadFormat6(EndianStream b_Stream, bool padding = false)
        {
            var bundleSize = b_Stream.ReadInt64();
            int compressedSize = b_Stream.ReadInt32();
            int uncompressedSize = b_Stream.ReadInt32();
            int flag = b_Stream.ReadInt32();
            if (padding)
                b_Stream.ReadByte();
            byte[] blocksInfoBytes;
            if ((flag & 0x80) != 0)//at end of file
            {
                var position = b_Stream.Position;
                b_Stream.Position = b_Stream.BaseStream.Length - compressedSize;
                blocksInfoBytes = b_Stream.ReadBytes(compressedSize);
                b_Stream.Position = position;
            }
            else
            {
                blocksInfoBytes = b_Stream.ReadBytes(compressedSize);
            }
            EndianStream blocksInfo;
            switch (flag & 0x3F)
            {
                default:
                case 0://None
                    {
                        blocksInfo = new EndianStream(new MemoryStream(blocksInfoBytes), EndianType.BigEndian);
                        break;
                    }
                case 1://LZMA
                    {
                        blocksInfo = new EndianStream(SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(new MemoryStream(blocksInfoBytes)), EndianType.BigEndian);
                        break;
                    }
                case 2://LZ4
                case 3://LZ4HC
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (var mstream = new MemoryStream(blocksInfoBytes))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        blocksInfo = new EndianStream(new MemoryStream(uncompressedBytes), EndianType.BigEndian);
                        break;
                    }
                    //case 4:LZHAM?
            }
            using (blocksInfo)
            {
                blocksInfo.Position = 0x10;
                int blockcount = blocksInfo.ReadInt32();
                EndianStream assetsData;
                var assetsDataStream = new MemoryStream();
                for (int i = 0; i < blockcount; i++)
                {
                    uncompressedSize = blocksInfo.ReadInt32();
                    compressedSize = blocksInfo.ReadInt32();
                    flag = blocksInfo.ReadInt16();
                    var compressedBytes = b_Stream.ReadBytes(compressedSize);
                    switch (flag & 0x3F)
                    {
                        default:
                        case 0://None
                            {
                                assetsDataStream.Write(compressedBytes, 0, compressedSize);
                                break;
                            }
                        case 1://LZMA
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var mstream = new MemoryStream(compressedBytes))
                                {
                                    var decoder = SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(mstream, uncompressedSize);
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                    decoder.Dispose();
                                }
                                assetsDataStream.Write(uncompressedBytes, 0, uncompressedSize);
                                break;
                            }
                        case 2://LZ4
                        case 3://LZ4HC
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var mstream = new MemoryStream(compressedBytes))
                                {
                                    var decoder = new Lz4DecoderStream(mstream);
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                    decoder.Dispose();
                                }
                                assetsDataStream.Write(uncompressedBytes, 0, uncompressedSize);
                                break;
                            }
                            //case 4:LZHAM?
                    }
                }
                assetsData = new EndianStream(assetsDataStream, EndianType.BigEndian);
                using (assetsData)
                {
                    var entryinfo_count = blocksInfo.ReadInt32();
                    for (int i = 0; i < entryinfo_count; i++)
                    {
                        var memFile = new MemoryAssetsFile();
                        var entryinfo_offset = blocksInfo.ReadInt64();
                        var entryinfo_size = blocksInfo.ReadInt64();
                        var unknown = blocksInfo.ReadInt32();
                        memFile.fileName = blocksInfo.ReadStringToNull();
                        assetsData.Position = entryinfo_offset;
                        var buffer = new byte[entryinfo_size];
                        assetsData.Read(buffer, 0, (int)entryinfo_size);
                        memFile.memStream = new MemoryStream(buffer);
                        MemoryAssetsFileList.Add(memFile);
                    }
                }
            }
        }
    }
}

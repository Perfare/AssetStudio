using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip;
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
                else
                {
                    long bundleSize = b_Stream.ReadInt64();
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
                    var bundleSize = b_Stream.ReadInt64();
                    int compressedSize = b_Stream.ReadInt32();
                    int uncompressedSize = b_Stream.ReadInt32();
                    // (UnityFS) flags
                    //  0x100 = <unknown>
                    //  0x80 = data header at end of file
                    //  0x40 = entry info present
                    //  0x3f = low six bits are data header compression method
                    //         0 = none
                    //         1 = LZMA
                    //         3 = LZ4
                    int unknown = b_Stream.ReadInt32();
                    var entryinfoBytes = b_Stream.ReadBytes(compressedSize);
                    EndianStream entryinfo;
                    if (uncompressedSize > compressedSize)
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (var mstream = new MemoryStream(entryinfoBytes))
                        {
                            var decoder = new Lz4DecoderStream(mstream);
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                            decoder.Dispose();
                        }
                        entryinfo = new EndianStream(new MemoryStream(uncompressedBytes), EndianType.BigEndian);
                    }
                    else
                    {
                        entryinfo = new EndianStream(new MemoryStream(entryinfoBytes), EndianType.BigEndian);
                    }
                    using (entryinfo)
                    {
                        entryinfo.Position = 0x10;
                        int blockcount = entryinfo.ReadInt32();
                        EndianStream assetsData;
                        var assetsDatam = new MemoryStream();
                        for (int i = 0; i < blockcount; i++)
                        {
                            uncompressedSize = entryinfo.ReadInt32();
                            compressedSize = entryinfo.ReadInt32();
                            //0 = none
                            //1 = LZMA
                            //3 = LZ4
                            //0x40 = 0?
                            unknown = entryinfo.ReadInt16();
                            var compressedBytes = b_Stream.ReadBytes(compressedSize);
                            if (uncompressedSize > compressedSize)
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var mstream = new MemoryStream(compressedBytes))
                                {
                                    var decoder = new Lz4DecoderStream(mstream);
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                    decoder.Dispose();
                                }
                                assetsDatam.Write(uncompressedBytes, 0, uncompressedSize);
                            }
                            else
                            {
                                assetsDatam.Write(compressedBytes, 0, compressedSize);
                            }
                        }
                        //assetsDatam.Capacity = (int)assetsDatam.Length;
                        assetsData = new EndianStream(assetsDatam, EndianType.BigEndian);
                        using (assetsData)
                        {
                            var entryinfo_count = entryinfo.ReadInt32();
                            for (int i = 0; i < entryinfo_count; i++)
                            {
                                MemoryAssetsFile memFile = new MemoryAssetsFile();
                                var entryinfo_offset = entryinfo.ReadInt64();
                                var entryinfo_size = entryinfo.ReadInt64();
                                unknown = entryinfo.ReadInt32();
                                memFile.fileName = entryinfo.ReadStringToNull();
                                assetsData.Position = entryinfo_offset;
                                byte[] buffer = new byte[entryinfo_size];
                                assetsData.Read(buffer, 0, (int)entryinfo_size);
                                memFile.memStream = new MemoryStream(buffer);
                                MemoryAssetsFileList.Add(memFile);
                            }
                        }
                    }
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
    }
}

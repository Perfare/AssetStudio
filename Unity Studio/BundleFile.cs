using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SevenZip;
using Lz4;

namespace Unity_Studio
{
    public class BundleFile : IDisposable
    {
        private EndianStream Stream;
        public byte ver1;
        public string ver2;
        public string ver3;
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

                        filebuffer = new byte[uncompressedSize]; //is this ok?
                        for (;;)
                        {
                            int nRead = decoder.Read(filebuffer, 0, uncompressedSize);
                            if (nRead == 0)
                                break;
                        }
                    }
                }

                Stream = new EndianStream(new MemoryStream(filebuffer), EndianType.BigEndian);
            }
            else { Stream = new EndianStream(File.OpenRead(fileName), EndianType.BigEndian); }

            long magicHeader = Stream.ReadInt64();

            if (magicHeader == -361700864190383366 || magicHeader == 6155973689634940258 || magicHeader == 6155973689634611575)
            {
                int dummy = Stream.ReadInt32();
                ver1 = Stream.ReadByte();
                ver2 = Stream.ReadStringToNull();
                ver3 = Stream.ReadStringToNull();
                int lzmaSize = 0;
                int fileSize = Stream.ReadInt32();
                short dummy2 = Stream.ReadInt16();
                int offset = Stream.ReadInt16();
                int dummy3 = Stream.ReadInt32();
                int lzmaChunks = Stream.ReadInt32();

                for (int i = 0; i < lzmaChunks; i++)
                {
                    lzmaSize = Stream.ReadInt32();
                    fileSize = Stream.ReadInt32();
                }

                Stream.Position = offset;
                switch (magicHeader)
                {
                    case -361700864190383366: //.bytes
                    case 6155973689634940258: //UnityWeb
                        {
                            byte[] lzmaBuffer = new byte[lzmaSize];
                            Stream.Read(lzmaBuffer, 0, lzmaSize);
                            Stream.Close();
                            Stream.Dispose();

                            Stream = new EndianStream(SevenZip.Compression.LZMA.SevenZipHelper.StreamDecompress(new MemoryStream(lzmaBuffer)), EndianType.BigEndian);
                            offset = 0;
                            break;
                        }
                    case 6155973689634611575: //UnityRaw
                        {

                            break;
                        }
                }

                int fileCount = Stream.ReadInt32();
                for (int i = 0; i < fileCount; i++)
                {
                    MemoryAssetsFile memFile = new MemoryAssetsFile();
                    memFile.fileName = Stream.ReadStringToNull();
                    int fileOffset = Stream.ReadInt32();
                    fileOffset += offset;
                    fileSize = Stream.ReadInt32();
                    long nextFile = Stream.Position;
                    Stream.Position = fileOffset;

                    byte[] buffer = new byte[fileSize];
                    Stream.Read(buffer, 0, fileSize);
                    memFile.memStream = new MemoryStream(buffer);
                    MemoryAssetsFileList.Add(memFile);
                    Stream.Position = nextFile;
                }
            }

            Stream.Close();
        }

        ~BundleFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            Stream.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

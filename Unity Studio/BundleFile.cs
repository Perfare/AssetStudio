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
        public int ver1;
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
            var header = b_Stream.ReadStringToNull();

            if (header == "UnityWeb" || header == "UnityRaw" || header == "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA")
            {
                ver1 = b_Stream.ReadInt32();
                ver2 = b_Stream.ReadStringToNull();
                ver3 = b_Stream.ReadStringToNull();
                if (ver1 < 6) { int bundleSize = b_Stream.ReadInt32(); }
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
                switch (header)
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
            else if (header == "UnityFS")
            {
                ver1 = b_Stream.ReadInt32();
                ver2 = b_Stream.ReadStringToNull();
                ver3 = b_Stream.ReadStringToNull();
                long bundleSize = b_Stream.ReadInt64();
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

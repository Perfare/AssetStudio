using System.Collections.Generic;
using System.IO;
using Lz4;
using SevenZip.Compression.LZMA;

namespace Unity_Studio
{
    public class MemoryFile
    {
        public string fileName;
        public MemoryStream stream;
    }

    public class BundleFile
    {
        public int format;
        public string versionPlayer;
        public string versionEngine;
        public List<MemoryFile> fileList = new List<MemoryFile>();

        public BundleFile(EndianBinaryReader bundleReader)
        {
            var signature = bundleReader.ReadStringToNull();
            switch (signature)
            {
                case "UnityWeb":
                case "UnityRaw":
                case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA":
                    {
                        format = bundleReader.ReadInt32();
                        versionPlayer = bundleReader.ReadStringToNull();
                        versionEngine = bundleReader.ReadStringToNull();
                        if (format < 6)
                        {
                            int bundleSize = bundleReader.ReadInt32();
                        }
                        else if (format == 6)
                        {
                            ReadFormat6(bundleReader, true);
                            return;
                        }
                        short dummy2 = bundleReader.ReadInt16();
                        int offset = bundleReader.ReadInt16();
                        int dummy3 = bundleReader.ReadInt32();
                        int lzmaChunks = bundleReader.ReadInt32();

                        int lzmaSize = 0;
                        long streamSize = 0;

                        for (int i = 0; i < lzmaChunks; i++)
                        {
                            lzmaSize = bundleReader.ReadInt32();
                            streamSize = bundleReader.ReadInt32();
                        }

                        bundleReader.Position = offset;
                        switch (signature)
                        {
                            case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA": //.bytes
                            case "UnityWeb":
                                {
                                    var lzmaBuffer = bundleReader.ReadBytes(lzmaSize);
                                    using (var lzmaStream = new EndianBinaryReader(SevenZipHelper.StreamDecompress(new MemoryStream(lzmaBuffer))))
                                    {
                                        GetAssetsFiles(lzmaStream, 0);
                                    }
                                    break;
                                }
                            case "UnityRaw":
                                {
                                    GetAssetsFiles(bundleReader, offset);
                                    break;
                                }
                        }
                        break;
                    }
                case "UnityFS":
                    format = bundleReader.ReadInt32();
                    versionPlayer = bundleReader.ReadStringToNull();
                    versionEngine = bundleReader.ReadStringToNull();
                    if (format == 6)
                    {
                        ReadFormat6(bundleReader);
                    }
                    break;
            }
        }

        private void GetAssetsFiles(EndianBinaryReader reader, int offset)
        {
            int fileCount = reader.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                var file = new MemoryFile();
                file.fileName = reader.ReadStringToNull();
                int fileOffset = reader.ReadInt32();
                fileOffset += offset;
                int fileSize = reader.ReadInt32();
                long nextFile = reader.Position;
                reader.Position = fileOffset;
                var buffer = reader.ReadBytes(fileSize);
                file.stream = new MemoryStream(buffer);
                fileList.Add(file);
                reader.Position = nextFile;
            }
        }

        private void ReadFormat6(EndianBinaryReader bundleReader, bool padding = false)
        {
            var bundleSize = bundleReader.ReadInt64();
            int compressedSize = bundleReader.ReadInt32();
            int uncompressedSize = bundleReader.ReadInt32();
            int flag = bundleReader.ReadInt32();
            if (padding)
                bundleReader.ReadByte();
            byte[] blocksInfoBytes;
            if ((flag & 0x80) != 0)//at end of file
            {
                var position = bundleReader.Position;
                bundleReader.Position = bundleReader.BaseStream.Length - compressedSize;
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
                bundleReader.Position = position;
            }
            else
            {
                blocksInfoBytes = bundleReader.ReadBytes(compressedSize);
            }
            MemoryStream blocksInfoStream;
            switch (flag & 0x3F)
            {
                default://None
                    {
                        blocksInfoStream = new MemoryStream(blocksInfoBytes);
                        break;
                    }
                case 1://LZMA
                    {
                        blocksInfoStream = SevenZipHelper.StreamDecompress(new MemoryStream(blocksInfoBytes));
                        break;
                    }
                case 2://LZ4
                case 3://LZ4HC
                    {
                        byte[] uncompressedBytes = new byte[uncompressedSize];
                        using (var decoder = new Lz4DecoderStream(new MemoryStream(blocksInfoBytes)))
                        {
                            decoder.Read(uncompressedBytes, 0, uncompressedSize);
                        }
                        blocksInfoStream = new MemoryStream(uncompressedBytes);
                        break;
                    }
                    //case 4:LZHAM?
            }
            using (var blocksInfo = new EndianBinaryReader(blocksInfoStream))
            {
                blocksInfo.Position = 0x10;
                int blockcount = blocksInfo.ReadInt32();
                var assetsDataStream = new MemoryStream();
                for (int i = 0; i < blockcount; i++)
                {
                    uncompressedSize = blocksInfo.ReadInt32();
                    compressedSize = blocksInfo.ReadInt32();
                    flag = blocksInfo.ReadInt16();
                    var compressedBytes = bundleReader.ReadBytes(compressedSize);
                    switch (flag & 0x3F)
                    {
                        default://None
                            {
                                assetsDataStream.Write(compressedBytes, 0, compressedSize);
                                break;
                            }
                        case 1://LZMA
                            {
                                var uncompressedBytes = new byte[uncompressedSize];
                                using (var mstream = new MemoryStream(compressedBytes))
                                {
                                    var decoder = SevenZipHelper.StreamDecompress(mstream, uncompressedSize);
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
                                using (var decoder = new Lz4DecoderStream(new MemoryStream(compressedBytes)))
                                {
                                    decoder.Read(uncompressedBytes, 0, uncompressedSize);
                                }
                                assetsDataStream.Write(uncompressedBytes, 0, uncompressedSize);
                                break;
                            }
                            //case 4:LZHAM?
                    }
                }
                using (var assetsDataReader = new EndianBinaryReader(assetsDataStream))
                {
                    var entryinfo_count = blocksInfo.ReadInt32();
                    for (int i = 0; i < entryinfo_count; i++)
                    {
                        var file = new MemoryFile();
                        var entryinfo_offset = blocksInfo.ReadInt64();
                        var entryinfo_size = blocksInfo.ReadInt64();
                        flag = blocksInfo.ReadInt32();
                        file.fileName = Path.GetFileName(blocksInfo.ReadStringToNull());
                        assetsDataReader.Position = entryinfo_offset;
                        var buffer = assetsDataReader.ReadBytes((int)entryinfo_size);
                        file.stream = new MemoryStream(buffer);
                        fileList.Add(file);
                    }
                }
            }
        }
    }
}

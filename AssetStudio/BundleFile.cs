using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lz4;

namespace AssetStudio
{
    public class StreamFile
    {
        public string fileName;
        public Stream stream;
    }

    public class BlockInfo
    {
        public uint compressedSize;
        public uint uncompressedSize;
        public short flag;
    }

    public class BundleFile
    {
        private string path;
        public string versionPlayer;
        public string versionEngine;
        public List<StreamFile> fileList = new List<StreamFile>();

        public BundleFile(EndianBinaryReader bundleReader, string path)
        {
            this.path = path;
            var signature = bundleReader.ReadStringToNull();
            switch (signature)
            {
                case "UnityWeb":
                case "UnityRaw":
                case "\xFA\xFA\xFA\xFA\xFA\xFA\xFA\xFA":
                    {
                        var format = bundleReader.ReadInt32();
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
                    {
                        var format = bundleReader.ReadInt32();
                        versionPlayer = bundleReader.ReadStringToNull();
                        versionEngine = bundleReader.ReadStringToNull();
                        if (format == 6)
                        {
                            ReadFormat6(bundleReader);
                        }
                        break;
                    }
            }
        }

        private void GetAssetsFiles(EndianBinaryReader reader, int offset)
        {
            int fileCount = reader.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                var file = new StreamFile();
                file.fileName = Path.GetFileName(reader.ReadStringToNull());
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
            using (var blocksInfoReader = new EndianBinaryReader(blocksInfoStream))
            {
                blocksInfoReader.Position = 0x10;
                int blockcount = blocksInfoReader.ReadInt32();
                var blockInfos = new BlockInfo[blockcount];
                for (int i = 0; i < blockcount; i++)
                {
                    blockInfos[i] = new BlockInfo
                    {
                        uncompressedSize = blocksInfoReader.ReadUInt32(),
                        compressedSize = blocksInfoReader.ReadUInt32(),
                        flag = blocksInfoReader.ReadInt16()
                    };
                }
                Stream dataStream;
                var uncompressedSizeSum = blockInfos.Sum(x => x.uncompressedSize);
                if (uncompressedSizeSum > int.MaxValue)
                {
                    /*var memoryMappedFile = MemoryMappedFile.CreateNew(Path.GetFileName(path), uncompressedSizeSum);
                    assetsDataStream = memoryMappedFile.CreateViewStream();*/
                    dataStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                }
                else
                {
                    dataStream = new MemoryStream();
                }
                foreach (var blockInfo in blockInfos)
                {
                    switch (blockInfo.flag & 0x3F)
                    {
                        default://None
                            {
                                bundleReader.BaseStream.CopyTo(dataStream, blockInfo.compressedSize);
                                break;
                            }
                        case 1://LZMA
                            {
                                SevenZipHelper.StreamDecompress(bundleReader.BaseStream, dataStream, blockInfo.compressedSize, blockInfo.uncompressedSize);
                                break;
                            }
                        case 2://LZ4
                        case 3://LZ4HC
                            {
                                var lz4Stream = new Lz4DecoderStream(bundleReader.BaseStream, blockInfo.compressedSize);
                                lz4Stream.CopyTo(dataStream, blockInfo.uncompressedSize);
                                break;
                            }
                            //case 4:LZHAM?
                    }
                }
                dataStream.Position = 0;
                using (dataStream)
                {
                    var entryinfo_count = blocksInfoReader.ReadInt32();
                    for (int i = 0; i < entryinfo_count; i++)
                    {
                        var file = new StreamFile();
                        var entryinfo_offset = blocksInfoReader.ReadInt64();
                        var entryinfo_size = blocksInfoReader.ReadInt64();
                        flag = blocksInfoReader.ReadInt32();
                        file.fileName = Path.GetFileName(blocksInfoReader.ReadStringToNull());
                        if (entryinfo_size > int.MaxValue)
                        {
                            /*var memoryMappedFile = MemoryMappedFile.CreateNew(file.fileName, entryinfo_size);
                            file.stream = memoryMappedFile.CreateViewStream();*/
                            var extractPath = path + "_unpacked\\";
                            Directory.CreateDirectory(extractPath);
                            file.stream = File.Create(extractPath + file.fileName);
                        }
                        else
                        {
                            file.stream = new MemoryStream();
                        }
                        dataStream.Position = entryinfo_offset;
                        dataStream.CopyTo(file.stream, entryinfo_size);
                        file.stream.Position = 0;
                        fileList.Add(file);
                    }
                }
            }
        }
    }
}

using System.IO;

namespace AssetStudio
{
    public class ResourceReader
    {
        private bool needSearch;
        private string path;
        private SerializedFile assetsFile;
        private long offset;
        private int size;
        private BinaryReader reader;


        public ResourceReader(string path, SerializedFile assetsFile, ulong offset, int size)
        {
            needSearch = true;
            this.path = path;
            this.assetsFile = assetsFile;
            this.offset = (long)offset;
            this.size = size;
        }

        public ResourceReader(BinaryReader reader, long offset, int size)
        {
            this.reader = reader;
            this.offset = offset;
            this.size = size;
        }

        public byte[] GetData()
        {
            if (needSearch)
            {
                var resourceFileName = Path.GetFileName(path);

                if (assetsFile.assetsManager.resourceFileReaders.TryGetValue(resourceFileName, out reader))
                {
                    needSearch = false;
                    reader.BaseStream.Position = offset;
                    return reader.ReadBytes(size);
                }

                var assetsFileDirectory = Path.GetDirectoryName(assetsFile.fullName);
                var resourceFilePath = assetsFileDirectory + Path.DirectorySeparatorChar + resourceFileName;
                if (!File.Exists(resourceFilePath))
                {
                    var findFiles = Directory.GetFiles(assetsFileDirectory, resourceFileName, SearchOption.AllDirectories);
                    if (findFiles.Length > 0)
                    {
                        resourceFilePath = findFiles[0];
                    }
                }
                if (File.Exists(resourceFilePath))
                {
                    reader = new BinaryReader(File.OpenRead(resourceFilePath));
                    needSearch = false;
                    assetsFile.assetsManager.resourceFileReaders.Add(resourceFileName, reader);
                    reader.BaseStream.Position = offset;
                    return reader.ReadBytes(size);
                }

                throw new FileNotFoundException($"Can't find the resource file {resourceFileName}");
            }

            reader.BaseStream.Position = offset;
            return reader.ReadBytes(size);
        }
    }
}

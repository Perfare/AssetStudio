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


        public ResourceReader(string path, SerializedFile assetsFile, long offset, int size)
        {
            needSearch = true;
            this.path = path;
            this.assetsFile = assetsFile;
            this.offset = offset;
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

                if (assetsFile.assetsManager.resourceFileReaders.TryGetValue(resourceFileName.ToUpper(), out var reader))
                {
                    reader.Position = offset;
                    return reader.ReadBytes(size);
                }

                var currentDirectory = Path.GetDirectoryName(assetsFile.fullName);
                var resourceFilePath = currentDirectory + "\\" + resourceFileName;
                if (!File.Exists(resourceFilePath))
                {
                    var findFiles = Directory.GetFiles(currentDirectory, resourceFileName, SearchOption.AllDirectories);
                    if (findFiles.Length > 0)
                    {
                        resourceFilePath = findFiles[0];
                    }
                }
                if (File.Exists(resourceFilePath))
                {
                    using (var resourceReader = new BinaryReader(File.OpenRead(resourceFilePath)))
                    {
                        resourceReader.BaseStream.Position = offset;
                        return resourceReader.ReadBytes(size);
                    }
                }

                throw new FileNotFoundException($"Can't find the resource file {resourceFileName}");
            }

            reader.BaseStream.Position = offset;
            return reader.ReadBytes(size);
        }
    }
}

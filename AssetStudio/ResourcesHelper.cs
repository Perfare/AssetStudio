using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    internal static class ResourcesHelper
    {
        public static byte[] GetData(string path, SerializedFile assetsFile, long offset, int size)
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
    }
}

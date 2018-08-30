using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public static class ResourcesHelper
    {
        public static byte[] GetData(string path, string sourceFilePath, long offset, int size)
        {
            var resourceFileName = Path.GetFileName(path);

            if (Studio.resourceFileReaders.TryGetValue(resourceFileName.ToUpper(), out var reader))
            {
                reader.Position = offset;
                return reader.ReadBytes(size);
            }

            var resourceFilePath = Path.GetDirectoryName(sourceFilePath) + "\\" + resourceFileName;
            if (!File.Exists(resourceFilePath))
            {
                var findFiles = Directory.GetFiles(Path.GetDirectoryName(sourceFilePath), resourceFileName, SearchOption.AllDirectories);
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
            else
            {
                MessageBox.Show($"can't find the resource file {resourceFileName}");
                return null;
            }
        }
    }
}

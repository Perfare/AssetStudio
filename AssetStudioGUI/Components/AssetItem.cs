using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    internal class AssetItem : ListViewItem
    {
        public Object Asset;
        public SerializedFile SourceFile;
        public string Container = string.Empty;
        public string TypeString;
        public long m_PathID;
        public long FullSize;
        public ClassIDType Type;
        public string InfoText;
        public string UniqueID;
        public GameObjectTreeNode TreeNode;

        public AssetItem(Object asset)
        {
            Asset = asset;
            SourceFile = asset.assetsFile;
            Type = asset.type;
            TypeString = Type.ToString();
            m_PathID = asset.m_PathID;
            FullSize = asset.byteSize;
        }

        public void SetSubItems()
        {
            SubItems.AddRange(new[]
            {
                Container, //Container
                TypeString, //Type
                m_PathID.ToString(), //PathID
                HumanSize(FullSize),//.ToString(), //Size
            });
        }

        private string HumanSize(long size)
        {
            if (size < 1024)
                return size.ToString();
            else if (size < 1024 * 1024)
                return (size / 1024f).ToString("#.000K");
            else //(size < 1024 * 1024 * 1024)
                return (size / (1024 * 1024f)).ToString("#.000M");
        }
    }
}

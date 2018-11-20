using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    internal class AssetItem : ListViewItem
    {
        public SerializedFile sourceFile;
        public ObjectReader reader;
        public long FullSize;
        public ClassIDType Type;
        public string TypeString;
        public string InfoText;
        public string UniqueID;
        public GameObjectTreeNode TreeNode;

        public AssetItem(ObjectReader reader)
        {
            sourceFile = reader.assetsFile;
            this.reader = reader;
            FullSize = reader.byteSize;
            Type = reader.type;
            TypeString = Type.ToString();
        }
    }
}

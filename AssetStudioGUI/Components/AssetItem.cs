using System.Windows.Forms;
using AssetStudio;

namespace AssetStudioGUI
{
    internal class AssetItem : ListViewItem
    {
        public Object Asset;
        public SerializedFile SourceFile;
        public long FullSize;
        public ClassIDType Type;
        public string TypeString;

        public string Extension;
        public string InfoText;
        public string UniqueID;
        public GameObjectTreeNode TreeNode;

        public AssetItem(Object asset)
        {
            Asset = asset;
            SourceFile = asset.assetsFile;
            FullSize = asset.byteSize;
            Type = asset.type;
            TypeString = Type.ToString();
        }
    }
}

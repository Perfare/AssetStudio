using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AssetStudio
{
    public class AssetItem : ListViewItem
    {
        public AssetsFile sourceFile;
        public ObjectReader reader;
        public long FullSize;
        public ClassIDType Type;
        public string TypeString;
        public string InfoText;
        public string UniqueID;
        public GameObject gameObject;

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

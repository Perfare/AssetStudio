using static AssetStudio.Studio;

namespace AssetStudio
{
    public class PPtr
    {
        public int m_FileID;
        public long m_PathID;

        //custom
        public AssetsFile assetsFile;
        public int index = -2; //-2 - Prepare, -1 - Missing

        private bool TryGet(out AssetsFile result)
        {
            result = null;
            if (m_FileID == 0)
            {
                result = assetsFile;
                return true;
            }

            if (m_FileID > 0 && m_FileID - 1 < assetsFile.m_Externals.Count)
            {
                if (index == -2)
                {
                    var m_External = assetsFile.m_Externals[m_FileID - 1];
                    var name = m_External.fileName.ToUpper();
                    if (!assetsFileIndexCache.TryGetValue(name, out index))
                    {
                        index = assetsfileList.FindIndex(x => x.upperFileName == name);
                        assetsFileIndexCache.Add(name, index);
                    }
                }

                if (index >= 0)
                {
                    result = assetsfileList[index];
                    return true;
                }
            }

            return false;
        }

        public bool TryGetPD(out AssetPreloadData result)
        {
            result = null;
            if (TryGet(out var sourceFile))
            {
                if (sourceFile.preloadTable.TryGetValue(m_PathID, out result))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetTransform(out Transform m_Transform)
        {
            if (TryGet(out var sourceFile))
            {
                if (sourceFile.TransformList.TryGetValue(m_PathID, out m_Transform))
                {
                    return true;
                }
            }

            m_Transform = null;
            return false;
        }

        public bool TryGetGameObject(out GameObject m_GameObject)
        {
            if (TryGet(out var sourceFile))
            {
                if (sourceFile.GameObjectList.TryGetValue(m_PathID, out m_GameObject))
                {
                    return true;
                }
            }

            m_GameObject = null;
            return false;
        }
    }
}

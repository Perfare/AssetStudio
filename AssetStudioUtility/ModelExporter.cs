namespace AssetStudio
{
    public static class ModelExporter
    {
        public static void ExportFbx(string path, IImported imported, bool eulerFilter, float filterPrecision, bool allFrames, bool allBones, bool skins, float boneSize, float scaleFactor, bool flatInbetween, int versionIndex, bool isAscii)
        {
            Fbx.Exporter.Export(path, imported, eulerFilter, filterPrecision, allFrames, allBones, skins, boneSize, scaleFactor, flatInbetween, versionIndex, isAscii);
        }
    }
}

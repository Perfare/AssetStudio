using System.Runtime.InteropServices;
using AssetStudio.FbxInterop;

namespace AssetStudio
{
    partial class Fbx
    {

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsUtilQuaternionToEuler(float qx, float qy, float qz, float qw, out float vx, out float vy, out float vz);

        [DllImport(FbxDll.DllName, CallingConvention = CallingConvention.Winapi)]
        private static extern void AsUtilEulerToQuaternion(float vx, float vy, float vz, out float qx, out float qy, out float qz, out float qw);

    }
}

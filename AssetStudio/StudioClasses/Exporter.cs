using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AssetStudio
{
    static class Exporter
    {
        public static bool ExportTexture2D(AssetPreloadData asset, string exportPathName, bool flip)
        {
            var texture2D = new Texture2D(asset, true);
            if (texture2D.image_data == null || texture2D.image_data.Length == 0)
                return false;
            var m_Texture2D = new Texture2DConverter(texture2D);
            var convert = (bool)Properties.Settings.Default["convertTexture"];
            var bitmap = m_Texture2D.ConvertToBitmap(flip);
            if (convert && bitmap != null)
            {
                ImageFormat format = null;
                var ext = (string)Properties.Settings.Default["convertType"];
                switch (ext)
                {
                    case "BMP":
                        format = ImageFormat.Bmp;
                        break;
                    case "PNG":
                        format = ImageFormat.Png;
                        break;
                    case "JPEG":
                        format = ImageFormat.Jpeg;
                        break;
                }
                var exportFullName = exportPathName + asset.Text + "." + ext.ToLower();
                if (ExportFileExists(exportFullName))
                    return false;
                bitmap.Save(exportFullName, format);
                bitmap.Dispose();
                return true;
            }
            else
            {
                var exportFullName = exportPathName + asset.Text + m_Texture2D.GetExtensionName();
                if (ExportFileExists(exportFullName))
                    return false;
                File.WriteAllBytes(exportFullName, m_Texture2D.ConvertToContainer());
                return true;
            }
        }

        public static bool ExportAudioClip(AssetPreloadData asset, string exportPath)
        {
            var m_AudioClip = new AudioClip(asset, true);
            if (m_AudioClip.m_AudioData == null)
                return false;
            var convertAudio = (bool)Properties.Settings.Default["convertAudio"];
            if (convertAudio && m_AudioClip.IsFMODSupport)
            {
                var exportFullName = exportPath + asset.Text + ".wav";
                if (ExportFileExists(exportFullName))
                    return false;
                FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO();
                var result = FMOD.Factory.System_Create(out var system);
                if (result != FMOD.RESULT.OK)
                    return false;
                result = system.init(1, FMOD.INITFLAGS.NORMAL, IntPtr.Zero);
                if (result != FMOD.RESULT.OK)
                    return false;
                exinfo.cbsize = Marshal.SizeOf(exinfo);
                exinfo.length = (uint)m_AudioClip.m_Size;
                result = system.createSound(m_AudioClip.m_AudioData, FMOD.MODE.OPENMEMORY, ref exinfo, out var sound);
                if (result != FMOD.RESULT.OK)
                    return false;
                result = sound.getSubSound(0, out var subsound);
                if (result != FMOD.RESULT.OK)
                    return false;
                result = subsound.getFormat(out var type, out var format, out int NumChannels, out int BitsPerSample);
                if (result != FMOD.RESULT.OK)
                    return false;
                result = subsound.getDefaults(out var frequency, out int priority);
                if (result != FMOD.RESULT.OK)
                    return false;
                var SampleRate = (int)frequency;
                result = subsound.getLength(out var length, FMOD.TIMEUNIT.PCMBYTES);
                if (result != FMOD.RESULT.OK)
                    return false;
                result = subsound.@lock(0, length, out var ptr1, out var ptr2, out var len1, out var len2);
                if (result != FMOD.RESULT.OK)
                    return false;
                byte[] buffer = new byte[len1 + 44];
                //添加wav头
                Encoding.UTF8.GetBytes("RIFF").CopyTo(buffer, 0);
                BitConverter.GetBytes(len1 + 36).CopyTo(buffer, 4);
                Encoding.UTF8.GetBytes("WAVEfmt ").CopyTo(buffer, 8);
                BitConverter.GetBytes(16).CopyTo(buffer, 16);
                BitConverter.GetBytes((short)1).CopyTo(buffer, 20);
                BitConverter.GetBytes((short)NumChannels).CopyTo(buffer, 22);
                BitConverter.GetBytes(SampleRate).CopyTo(buffer, 24);
                BitConverter.GetBytes(SampleRate * NumChannels * BitsPerSample / 8).CopyTo(buffer, 28);
                BitConverter.GetBytes((short)(NumChannels * BitsPerSample / 8)).CopyTo(buffer, 32);
                BitConverter.GetBytes((short)BitsPerSample).CopyTo(buffer, 34);
                Encoding.UTF8.GetBytes("data").CopyTo(buffer, 36);
                BitConverter.GetBytes(len1).CopyTo(buffer, 40);
                Marshal.Copy(ptr1, buffer, 44, (int)len1);
                File.WriteAllBytes(exportFullName, buffer);
                result = subsound.unlock(ptr1, ptr2, len1, len2);
                if (result != FMOD.RESULT.OK)
                    return false;
                subsound.release();
                sound.release();
                system.release();
            }
            else
            {
                var exportFullName = exportPath + asset.Text + asset.extension;
                if (ExportFileExists(exportFullName))
                    return false;
                File.WriteAllBytes(exportFullName, m_AudioClip.m_AudioData);
            }
            return true;
        }

        public static bool ExportShader(AssetPreloadData asset, string exportPath)
        {
            var m_Shader = new Shader(asset, true);
            var exportFullName = exportPath + asset.Text + asset.extension;
            if (ExportFileExists(exportFullName))
                return false;
            File.WriteAllBytes(exportFullName, m_Shader.m_Script);
            return true;
        }

        public static bool ExportTextAsset(AssetPreloadData asset, string exportPath)
        {
            var m_TextAsset = new TextAsset(asset, true);
            var exportFullName = exportPath + asset.Text + asset.extension;
            if (ExportFileExists(exportFullName))
                return false;
            File.WriteAllBytes(exportFullName, m_TextAsset.m_Script);
            return true;
        }

        public static bool ExportMonoBehaviour(AssetPreloadData asset, string exportPath)
        {
            var exportFullName = exportPath + asset.Text + ".txt";
            if (ExportFileExists(exportFullName))
                return false;
            var m_MonoBehaviour = new MonoBehaviour(asset);
            string str;
            if (asset.Type1 != asset.Type2 && asset.sourceFile.ClassStructures.ContainsKey(asset.Type1))
            {
                str = asset.GetClassString();
            }
            else
            {
                str = Studio.GetScriptString(asset);
            }
            File.WriteAllText(exportFullName, str);
            return true;
        }

        public static bool ExportFont(AssetPreloadData asset, string exportPath)
        {
            var m_Font = new UFont(asset, true);
            if (m_Font.m_FontData != null)
            {
                var exportFullName = exportPath + asset.Text + asset.extension;
                if (ExportFileExists(exportFullName))
                    return false;
                File.WriteAllBytes(exportFullName, m_Font.m_FontData);
                return true;
            }
            return false;
        }

        public static bool ExportMesh(AssetPreloadData asset, string exportPath)
        {
            var m_Mesh = new Mesh(asset, true);
            if (m_Mesh.m_VertexCount <= 0)
                return false;
            var exportFullName = exportPath + asset.Text + asset.extension;
            if (ExportFileExists(exportFullName))
                return false;
            var sb = new StringBuilder();
            sb.AppendLine("g " + m_Mesh.m_Name);
            #region Vertices
            if (m_Mesh.m_Vertices == null || m_Mesh.m_Vertices.Length == 0)
            {
                return false;
            }
            int c = 3;
            if (m_Mesh.m_Vertices.Length == m_Mesh.m_VertexCount * 4)
            {
                c = 4;
            }
            for (int v = 0; v < m_Mesh.m_VertexCount; v++)
            {
                sb.AppendFormat("v {0} {1} {2}\r\n", -m_Mesh.m_Vertices[v * c], m_Mesh.m_Vertices[v * c + 1], m_Mesh.m_Vertices[v * c + 2]);
            }
            #endregion

            #region UV
            if (m_Mesh.m_UV1 != null && m_Mesh.m_UV1.Length == m_Mesh.m_VertexCount * 2)
            {
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    sb.AppendFormat("vt {0} {1}\r\n", m_Mesh.m_UV1[v * 2], m_Mesh.m_UV1[v * 2 + 1]);
                }
            }
            else if (m_Mesh.m_UV2 != null && m_Mesh.m_UV2.Length == m_Mesh.m_VertexCount * 2)
            {
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    sb.AppendFormat("vt {0} {1}\r\n", m_Mesh.m_UV2[v * 2], m_Mesh.m_UV2[v * 2 + 1]);
                }
            }
            #endregion

            #region Normals
            if (m_Mesh.m_Normals != null && m_Mesh.m_Normals.Length > 0)
            {
                if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 3)
                {
                    c = 3;
                }
                else if (m_Mesh.m_Normals.Length == m_Mesh.m_VertexCount * 4)
                {
                    c = 4;
                }
                for (int v = 0; v < m_Mesh.m_VertexCount; v++)
                {
                    sb.AppendFormat("vn {0} {1} {2}\r\n", -m_Mesh.m_Normals[v * c], m_Mesh.m_Normals[v * c + 1], m_Mesh.m_Normals[v * c + 2]);
                }
            }
            #endregion

            #region Face
            int sum = 0;
            for (var i = 0; i < m_Mesh.m_SubMeshes.Count; i++)
            {
                sb.AppendLine($"g {m_Mesh.m_Name}_{i}");
                int indexCount = (int)m_Mesh.m_SubMeshes[i].indexCount;
                var end = sum + indexCount / 3;
                for (int f = sum; f < end; f++)
                {
                    sb.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\r\n", m_Mesh.m_Indices[f * 3 + 2] + 1, m_Mesh.m_Indices[f * 3 + 1] + 1, m_Mesh.m_Indices[f * 3] + 1);
                }
                sum = end;
            }
            #endregion

            sb.Replace("NaN", "0");
            File.WriteAllText(exportFullName, sb.ToString());
            return true;
        }

        public static bool ExportVideoClip(AssetPreloadData asset, string exportPath)
        {
            var m_VideoClip = new VideoClip(asset, true);
            if (m_VideoClip.m_VideoData != null)
            {
                var exportFullName = exportPath + asset.Text + asset.extension;
                if (ExportFileExists(exportFullName))
                    return false;
                File.WriteAllBytes(exportFullName, m_VideoClip.m_VideoData);
                return true;
            }
            return false;
        }

        public static bool ExportMovieTexture(AssetPreloadData asset, string exportPath)
        {
            var m_MovieTexture = new MovieTexture(asset, true);
            var exportFullName = exportPath + asset.Text + asset.extension;
            if (ExportFileExists(exportFullName))
                return false;
            File.WriteAllBytes(exportFullName, m_MovieTexture.m_MovieData);
            return true;
        }

        public static bool ExportSprite(AssetPreloadData asset, string exportPath)
        {
            ImageFormat format = null;
            var type = (string)Properties.Settings.Default["convertType"];
            switch (type)
            {
                case "BMP":
                    format = ImageFormat.Bmp;
                    break;
                case "PNG":
                    format = ImageFormat.Png;
                    break;
                case "JPEG":
                    format = ImageFormat.Jpeg;
                    break;
            }
            var exportFullName = exportPath + asset.Text + "." + type.ToLower();
            if (ExportFileExists(exportFullName))
                return false;
            var bitmap = SpriteHelper.GetImageFromSprite(asset);
            if (bitmap != null)
            {
                bitmap.Save(exportFullName, format);
                return true;
            }
            return false;
        }

        public static bool ExportRawFile(AssetPreloadData asset, string exportPath)
        {
            var exportFullName = exportPath + asset.Text + ".dat";
            if (ExportFileExists(exportFullName))
                return false;
            var bytes = asset.InitReader().ReadBytes(asset.Size);
            File.WriteAllBytes(exportFullName, bytes);
            return true;
        }

        private static bool ExportFileExists(string filename)
        {
            if (File.Exists(filename))
            {
                return true;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            return false;
        }

        public static bool ExportAnimator(AssetPreloadData animator, string exportPath, List<AssetPreloadData> animationList = null)
        {
            var m_Animator = new Animator(animator);
            var convert = animationList != null ? new ModelConverter(m_Animator, animationList) : new ModelConverter(m_Animator);
            exportPath = exportPath + Studio.FixFileName(animator.Text) + ".fbx";
            return ModelConverter(convert, exportPath);
        }

        public static bool ExportGameObject(GameObject gameObject, string exportPath, List<AssetPreloadData> animationList = null)
        {
            var convert = animationList != null ? new ModelConverter(gameObject, animationList) : new ModelConverter(gameObject);
            exportPath = exportPath + Studio.FixFileName(gameObject.Text) + ".fbx";
            return ModelConverter(convert, exportPath);
        }

        private static bool ModelConverter(ModelConverter convert, string exportPath)
        {
            var EulerFilter = (bool)Properties.Settings.Default["EulerFilter"];
            var filterPrecision = (float)(decimal)Properties.Settings.Default["filterPrecision"];
            var allFrames = (bool)Properties.Settings.Default["allFrames"];
            var allBones = (bool)Properties.Settings.Default["allBones"];
            var skins = (bool)Properties.Settings.Default["skins"];
            var boneSize = (int)(decimal)Properties.Settings.Default["boneSize"];
            var flatInbetween = (bool)Properties.Settings.Default["flatInbetween"];
            var compatibility = (bool)Properties.Settings.Default["compatibility"];
            Fbx.Exporter.Export(exportPath, convert, EulerFilter, filterPrecision, allFrames, allBones, skins, boneSize, flatInbetween, compatibility);
            return true;
        }
    }
}

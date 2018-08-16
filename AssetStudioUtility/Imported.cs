using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace AssetStudio
{
    public interface IImported
    {
        List<ImportedFrame> FrameList { get; }
        List<ImportedMesh> MeshList { get; }
        List<ImportedMaterial> MaterialList { get; }
        List<ImportedTexture> TextureList { get; }
        List<ImportedAnimation> AnimationList { get; }
        List<ImportedMorph> MorphList { get; }
    }

    public class ImportedFrame : IEnumerable<ImportedFrame>
    {
        public string Name { get; set; }
        public float[] LocalRotation { get; set; }
        public float[] LocalPosition { get; set; }
        public float[] LocalScale { get; set; }
        public ImportedFrame Parent { get; set; }

        private List<ImportedFrame> children;

        public ImportedFrame this[int i] => children[i];

        public int Count => children.Count;

        public void InitChildren(int count)
        {
            children = new List<ImportedFrame>(count);
        }

        public void AddChild(ImportedFrame obj)
        {
            children.Add(obj);
            obj.Parent = this;
        }

        public void InsertChild(int i, ImportedFrame obj)
        {
            children.Insert(i, obj);
            obj.Parent = this;
        }

        public void RemoveChild(ImportedFrame obj)
        {
            obj.Parent = null;
            children.Remove(obj);
        }

        public void RemoveChild(int i)
        {
            children[i].Parent = null;
            children.RemoveAt(i);
        }

        public int IndexOf(ImportedFrame obj)
        {
            return children.IndexOf(obj);
        }

        public IEnumerator<ImportedFrame> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ImportedMesh
    {
        public string Name { get; set; }
        public List<ImportedSubmesh> SubmeshList { get; set; }
        public List<ImportedBone> BoneList { get; set; }
    }

    public class ImportedSubmesh
    {
        public List<ImportedVertex> VertexList { get; set; }
        public List<ImportedFace> FaceList { get; set; }
        public string Material { get; set; }
    }

    public class ImportedVertex
    {
        public Vector3 Position { get; set; }
        public float[] Weights { get; set; }
        public byte[] BoneIndices { get; set; }
        public Vector3 Normal { get; set; }
        public float[] UV { get; set; }
        public Vector4 Tangent { get; set; }
    }

    public class ImportedVertexWithColour : ImportedVertex
    {
        public Color4 Colour { get; set; }
    }

    public class ImportedFace
    {
        public int[] VertexIndices { get; set; }
    }

    public class ImportedBone
    {
        public string Name { get; set; }
        public float[,] Matrix { get; set; }
    }

    public class ImportedMaterial
    {
        public string Name { get; set; }
        public Color4 Diffuse { get; set; }
        public Color4 Ambient { get; set; }
        public Color4 Specular { get; set; }
        public Color4 Emissive { get; set; }
        public float Power { get; set; }
        public string[] Textures { get; set; }
        public Vector2[] TexOffsets { get; set; }
        public Vector2[] TexScales { get; set; }
    }

    public class ImportedTexture
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }

        public ImportedTexture(MemoryStream stream, string name)
        {
            Name = name;
            Data = stream.ToArray();
        }
    }

    public abstract class ImportedAnimation
    {
        public string Name { get; set; }
    }

    public abstract class ImportedAnimationTrackContainer<TrackType> : ImportedAnimation where TrackType : ImportedAnimationTrack
    {
        public List<TrackType> TrackList { get; set; }

        public TrackType FindTrack(string name)
        {
            return TrackList.Find(track => track.Name == name);
        }
    }

    public class ImportedKeyframedAnimation : ImportedAnimationTrackContainer<ImportedAnimationKeyframedTrack>
    {

    }

    public class ImportedSampledAnimation : ImportedAnimationTrackContainer<ImportedAnimationSampledTrack>
    {
        public float SampleRate { get; set; }
    }

    public abstract class ImportedAnimationTrack
    {
        public string Name { get; set; }
    }

    public class ImportedKeyframe<T>
    {
        public float time { get; set; }
        public T value { get; set; }

        public ImportedKeyframe(float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    public class ImportedAnimationKeyframedTrack : ImportedAnimationTrack
    {
        public List<ImportedKeyframe<Vector3>> Scalings = new List<ImportedKeyframe<Vector3>>();
        public List<ImportedKeyframe<Vector3>> Rotations = new List<ImportedKeyframe<Vector3>>();
        public List<ImportedKeyframe<Vector3>> Translations = new List<ImportedKeyframe<Vector3>>();
    }

    public class ImportedAnimationSampledTrack : ImportedAnimationTrack
    {
        public Vector3?[] Scalings;
        public Vector3?[] Rotations;
        public Vector3?[] Translations;
        public float?[] Curve;
    }

    public class ImportedMorph
    {
        public string Name { get; set; }
        public string ClipName { get; set; }
        public List<Tuple<float, int, int>> Channels { get; set; }
        public List<ImportedMorphKeyframe> KeyframeList { get; set; }
        public List<ushort> MorphedVertexIndices { get; set; }
    }

    public class ImportedMorphKeyframe
    {
        public string Name { get; set; }
        public List<ImportedVertex> VertexList { get; set; }
        public List<ushort> MorphedVertexIndices { get; set; }
        public float Weight { get; set; }
    }

    public static class ImportedHelpers
    {
        public static ImportedFrame FindFrame(String name, ImportedFrame root)
        {
            ImportedFrame frame = root;
            if ((frame != null) && (frame.Name == name))
            {
                return frame;
            }

            for (int i = 0; i < root.Count; i++)
            {
                if ((frame = FindFrame(name, root[i])) != null)
                {
                    return frame;
                }
            }

            return null;
        }

        public static ImportedMesh FindMesh(String frameName, List<ImportedMesh> importedMeshList)
        {
            foreach (ImportedMesh mesh in importedMeshList)
            {
                if (mesh.Name == frameName)
                {
                    return mesh;
                }
            }

            return null;
        }

        public static ImportedMesh FindMesh(ImportedFrame frame, List<ImportedMesh> importedMeshList)
        {
            string framePath = frame.Name;
            ImportedFrame root = frame;
            while (root.Parent != null)
            {
                root = root.Parent;
                framePath = root.Name + "/" + framePath;
            }

            foreach (ImportedMesh mesh in importedMeshList)
            {
                if (mesh.Name == framePath)
                {
                    return mesh;
                }
            }

            return null;
        }

        public static ImportedMaterial FindMaterial(String name, List<ImportedMaterial> importedMats)
        {
            foreach (ImportedMaterial mat in importedMats)
            {
                if (mat.Name == name)
                {
                    return mat;
                }
            }

            return null;
        }

        public static ImportedTexture FindTexture(string name, List<ImportedTexture> importedTextureList)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (ImportedTexture tex in importedTextureList)
            {
                if (tex.Name == name)
                {
                    return tex;
                }
            }

            return null;
        }
    }
}

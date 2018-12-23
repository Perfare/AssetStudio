using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SharpDX;

namespace AssetStudio
{
    public interface IImported
    {
        ImportedFrame RootFrame { get; }
        List<ImportedMesh> MeshList { get; }
        List<ImportedMaterial> MaterialList { get; }
        List<ImportedTexture> TextureList { get; }
        List<ImportedKeyframedAnimation> AnimationList { get; }
        List<ImportedMorph> MorphList { get; }
    }

    public class ImportedFrame
    {
        public string Name { get; set; }
        public Vector3 LocalRotation { get; set; }
        public Vector3 LocalPosition { get; set; }
        public Vector3 LocalScale { get; set; }
        public ImportedFrame Parent { get; set; }

        private List<ImportedFrame> children;

        public ImportedFrame this[int i] => children[i];

        public int Count => children.Count;

        public ImportedFrame(int childrenCount = 0)
        {
            children = new List<ImportedFrame>(childrenCount);
        }

        public void AddChild(ImportedFrame obj)
        {
            children.Add(obj);
            obj.Parent?.Remove(obj);
            obj.Parent = this;
        }

        public void Remove(ImportedFrame frame)
        {
            children.Remove(frame);
        }

        public ImportedFrame FindFrameByPath(string path)
        {
            var splitPath = path.Split('/');
            if (Name != splitPath[0])
                throw new Exception($"Couldn't find path {path}");
            var curFrame = this;
            for (int i = 1; i < splitPath.Length; i++)
            {
                curFrame = curFrame.FindChild(splitPath[i], false);
                if (curFrame == null)
                {
                    throw new Exception($"Couldn't find path {path}");
                }
            }
            return curFrame;
        }

        public ImportedFrame FindFrame(string name)
        {
            if (Name == name)
            {
                return this;
            }
            foreach (var child in children)
            {
                var frame = child.FindFrame(name);
                if (frame != null)
                {
                    return frame;
                }
            }
            return null;
        }

        public ImportedFrame FindChild(string name, bool recursive = true)
        {
            foreach (var child in children)
            {
                if (recursive)
                {
                    var frame = child.FindFrame(name);
                    if (frame != null)
                    {
                        return frame;
                    }
                }
                else
                {
                    if (child.Name == name)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        public IEnumerable<ImportedFrame> FindChilds(string name)
        {
            if (Name == name)
            {
                yield return this;
            }
            foreach (var child in children)
            {
                foreach (var item in child.FindChilds(name))
                {
                    yield return item;
                }
            }
        }
    }

    public class ImportedMesh
    {
        public string Path { get; set; }
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
        public int[] BoneIndices { get; set; }
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
        public string Path { get; set; }
        public Matrix Matrix { get; set; }
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

    public class ImportedKeyframedAnimation
    {
        public string Name { get; set; }

        public List<ImportedAnimationKeyframedTrack> TrackList { get; set; }

        public ImportedAnimationKeyframedTrack FindTrack(string path)
        {
            var track = TrackList.Find(x => x.Path == path);
            if (track == null)
            {
                track = new ImportedAnimationKeyframedTrack { Path = path };
                TrackList.Add(track);
            }

            return track;
        }
    }

    public class ImportedAnimationKeyframedTrack
    {
        public string Path { get; set; }
        public List<ImportedKeyframe<Vector3>> Scalings = new List<ImportedKeyframe<Vector3>>();
        public List<ImportedKeyframe<Vector3>> Rotations = new List<ImportedKeyframe<Vector3>>();
        public List<ImportedKeyframe<Vector3>> Translations = new List<ImportedKeyframe<Vector3>>();
    }

    public class ImportedKeyframe<T>
    {
        public float time { get; set; }
        public T value { get; set; }
        public T inSlope { get; set; }
        public T outSlope { get; set; }

        public ImportedKeyframe(float time, T value)
        {
            this.time = time;
            this.value = value;
        }
    }

    public class ImportedMorph
    {
        public string Path { get; set; }
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
        public static ImportedMesh FindMesh(string path, List<ImportedMesh> importedMeshList)
        {
            foreach (var mesh in importedMeshList)
            {
                if (mesh.Path == path)
                {
                    return mesh;
                }
            }

            return null;
        }

        public static ImportedMesh FindMesh(ImportedFrame frame, List<ImportedMesh> importedMeshList)
        {
            var framePath = frame.Name;
            var root = frame;
            while (root.Parent != null)
            {
                root = root.Parent;
                framePath = root.Name + "/" + framePath;
            }

            foreach (var mesh in importedMeshList)
            {
                if (mesh.Path == framePath)
                {
                    return mesh;
                }
            }

            return null;
        }

        public static ImportedMaterial FindMaterial(string name, List<ImportedMaterial> importedMats)
        {
            foreach (var mat in importedMats)
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

            foreach (var tex in importedTextureList)
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

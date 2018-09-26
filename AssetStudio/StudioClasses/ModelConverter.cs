using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;
using static AssetStudio.Studio;

namespace AssetStudio
{
    class ModelConverter : IImported
    {
        public List<ImportedFrame> FrameList { get; protected set; } = new List<ImportedFrame>();
        public List<ImportedMesh> MeshList { get; protected set; } = new List<ImportedMesh>();
        public List<ImportedMaterial> MaterialList { get; protected set; } = new List<ImportedMaterial>();
        public List<ImportedTexture> TextureList { get; protected set; } = new List<ImportedTexture>();
        public List<ImportedKeyframedAnimation> AnimationList { get; protected set; } = new List<ImportedKeyframedAnimation>();
        public List<ImportedMorph> MorphList { get; protected set; } = new List<ImportedMorph>();

        private Avatar avatar;
        private Dictionary<uint, string> morphChannelInfo = new Dictionary<uint, string>();
        private HashSet<AssetPreloadData> animationClipHashSet = new HashSet<AssetPreloadData>();
        private Dictionary<uint, string> bonePathHash = new Dictionary<uint, string>();
        private bool deoptimize;

        public ModelConverter(GameObject m_GameObject)
        {
            if (assetsfileList.TryGetPD(m_GameObject.m_Animator, out var m_Animator))
            {
                var animator = new Animator(m_Animator);
                InitWithAnimator(animator);
                CollectAnimationClip(animator);
            }
            else
                InitWithGameObject(m_GameObject);
            ConvertAnimations();
        }

        public ModelConverter(GameObject m_GameObject, List<AssetPreloadData> animationList)
        {
            if (assetsfileList.TryGetPD(m_GameObject.m_Animator, out var m_Animator))
            {
                var animator = new Animator(m_Animator);
                InitWithAnimator(animator);
            }
            else
                InitWithGameObject(m_GameObject);
            foreach (var assetPreloadData in animationList)
            {
                animationClipHashSet.Add(assetPreloadData);
            }
            ConvertAnimations();
        }

        public ModelConverter(Animator m_Animator)
        {
            InitWithAnimator(m_Animator);
            CollectAnimationClip(m_Animator);
            ConvertAnimations();
        }

        public ModelConverter(Animator m_Animator, List<AssetPreloadData> animationList)
        {
            InitWithAnimator(m_Animator);
            foreach (var assetPreloadData in animationList)
            {
                animationClipHashSet.Add(assetPreloadData);
            }
            ConvertAnimations();
        }

        private void InitWithAnimator(Animator m_Animator)
        {
            if (assetsfileList.TryGetPD(m_Animator.m_Avatar, out var m_Avatar))
                avatar = new Avatar(m_Avatar);

            assetsfileList.TryGetGameObject(m_Animator.m_GameObject, out var m_GameObject);
            InitWithGameObject(m_GameObject);
        }

        private void InitWithGameObject(GameObject m_GameObject)
        {
            assetsfileList.TryGetTransform(m_GameObject.m_Transform, out var m_Transform);
            var rootTransform = m_Transform;
            var frameList = new List<ImportedFrame>();
            while (assetsfileList.TryGetTransform(rootTransform.m_Father, out var m_Father))
            {
                frameList.Add(ConvertFrame(m_Father));
                rootTransform = m_Father;
            }
            if (frameList.Count > 0)
            {
                FrameList.Add(frameList[frameList.Count - 1]);
                for (var i = frameList.Count - 2; i >= 0; i--)
                {
                    var frame = frameList[i];
                    var parent = frameList[i + 1];
                    parent.AddChild(frame);
                }
                ConvertFrames(m_Transform, frameList[0]);
            }
            else
            {
                ConvertFrames(m_Transform, null);
            }

            CreateBonePathHash(rootTransform);
            ConvertMeshRenderer(m_Transform);
        }

        private void ConvertMeshRenderer(Transform m_Transform)
        {
            assetsfileList.TryGetGameObject(m_Transform.m_GameObject, out var m_GameObject);
            foreach (var m_Component in m_GameObject.m_Components)
            {
                if (assetsfileList.TryGetPD(m_Component, out var assetPreloadData))
                {
                    switch (assetPreloadData.Type)
                    {
                        case ClassIDReference.MeshRenderer:
                            {
                                var m_Renderer = new MeshRenderer(assetPreloadData);
                                ConvertMeshRenderer(m_Renderer);
                                break;
                            }
                        case ClassIDReference.SkinnedMeshRenderer:
                            {
                                var m_SkinnedMeshRenderer = new SkinnedMeshRenderer(assetPreloadData);
                                ConvertMeshRenderer(m_SkinnedMeshRenderer);
                                break;
                            }
                        case ClassIDReference.Animation:
                            {
                                var m_Animation = new Animation(assetPreloadData);
                                foreach (var animation in m_Animation.m_Animations)
                                {
                                    if (assetsfileList.TryGetPD(animation, out var animationClip))
                                    {
                                        animationClipHashSet.Add(animationClip);
                                    }
                                }
                                break;
                            }
                    }
                }
            }
            foreach (var pptr in m_Transform.m_Children)
            {
                if (assetsfileList.TryGetTransform(pptr, out var child))
                    ConvertMeshRenderer(child);
            }
        }

        private void CollectAnimationClip(Animator m_Animator)
        {
            if (assetsfileList.TryGetPD(m_Animator.m_Controller, out var assetPreloadData))
            {
                if (assetPreloadData.Type == ClassIDReference.AnimatorOverrideController)
                {
                    var m_AnimatorOverrideController = new AnimatorOverrideController(assetPreloadData);
                    if (assetsfileList.TryGetPD(m_AnimatorOverrideController.m_Controller, out assetPreloadData))
                    {
                        var m_AnimatorController = new AnimatorController(assetPreloadData);
                        foreach (var m_AnimationClip in m_AnimatorController.m_AnimationClips)
                        {
                            if (assetsfileList.TryGetPD(m_AnimationClip, out assetPreloadData))
                            {
                                animationClipHashSet.Add(assetPreloadData);
                            }
                        }
                    }
                    /*foreach (var clip in m_AnimatorOverrideController.m_Clips)
                    {
                        if (assetsfileList.TryGetPD(clip[1], out assetPreloadData))
                        {
                            animationList.Add(new AnimationClip(assetPreloadData));
                        }
                    }*/
                }
                else if (assetPreloadData.Type == ClassIDReference.AnimatorController)
                {
                    var m_AnimatorController = new AnimatorController(assetPreloadData);
                    foreach (var m_AnimationClip in m_AnimatorController.m_AnimationClips)
                    {
                        if (assetsfileList.TryGetPD(m_AnimationClip, out assetPreloadData))
                        {
                            animationClipHashSet.Add(assetPreloadData);
                        }
                    }
                }
            }
        }

        private ImportedFrame ConvertFrame(Transform trans)
        {
            var frame = new ImportedFrame();
            assetsfileList.TryGetGameObject(trans.m_GameObject, out var m_GameObject);
            frame.Name = m_GameObject.m_Name;
            frame.InitChildren(trans.m_Children.Count);
            var m_EulerRotation = QuatToEuler(new[] { trans.m_LocalRotation[0], -trans.m_LocalRotation[1], -trans.m_LocalRotation[2], trans.m_LocalRotation[3] });
            frame.LocalRotation = new[] { m_EulerRotation[0], m_EulerRotation[1], m_EulerRotation[2] };
            frame.LocalScale = new[] { trans.m_LocalScale[0], trans.m_LocalScale[1], trans.m_LocalScale[2] };
            frame.LocalPosition = new[] { -trans.m_LocalPosition[0], trans.m_LocalPosition[1], trans.m_LocalPosition[2] };
            return frame;
        }

        private ImportedFrame ConvertFrame(Vector3 t, Quaternion q, Vector3 s, string name)
        {
            var frame = new ImportedFrame();
            frame.Name = name;
            frame.InitChildren(0);
            var m_LocalPosition = new[] { t.X, t.Y, t.Z };
            var m_LocalRotation = new[] { q.X, q.Y, q.Z, q.W };
            var m_LocalScale = new[] { s.X, s.Y, s.Z };
            var m_EulerRotation = QuatToEuler(new[] { m_LocalRotation[0], -m_LocalRotation[1], -m_LocalRotation[2], m_LocalRotation[3] });
            frame.LocalRotation = new[] { m_EulerRotation[0], m_EulerRotation[1], m_EulerRotation[2] };
            frame.LocalScale = new[] { m_LocalScale[0], m_LocalScale[1], m_LocalScale[2] };
            frame.LocalPosition = new[] { -m_LocalPosition[0], m_LocalPosition[1], m_LocalPosition[2] };
            return frame;
        }

        private void ConvertFrames(Transform trans, ImportedFrame parent)
        {
            var frame = ConvertFrame(trans);
            if (parent == null)
            {
                FrameList.Add(frame);
            }
            else
            {
                parent.AddChild(frame);
            }
            foreach (var pptr in trans.m_Children)
            {
                if (assetsfileList.TryGetTransform(pptr, out var child))
                    ConvertFrames(child, frame);
            }
        }

        private void ConvertMeshRenderer(Renderer meshR)
        {
            var mesh = GetMesh(meshR);
            if (mesh == null)
                return;
            var iMesh = new ImportedMesh();
            assetsfileList.TryGetGameObject(meshR.m_GameObject, out var m_GameObject2);
            assetsfileList.TryGetTransform(m_GameObject2.m_Transform, out var meshTransform);
            iMesh.Name = GetTransformPath(meshTransform);
            iMesh.SubmeshList = new List<ImportedSubmesh>();
            var subHashSet = new HashSet<int>();
            var combine = false;
            int firstSubMesh = 0;
            if (meshR.m_StaticBatchInfo?.subMeshCount > 0)
            {
                firstSubMesh = meshR.m_StaticBatchInfo.firstSubMesh;
                var finalSubMesh = meshR.m_StaticBatchInfo.firstSubMesh + meshR.m_StaticBatchInfo.subMeshCount;
                for (int i = meshR.m_StaticBatchInfo.firstSubMesh; i < finalSubMesh; i++)
                {
                    subHashSet.Add(i);
                }
                combine = true;
            }
            else if (meshR.m_SubsetIndices?.Length > 0)
            {
                firstSubMesh = (int)meshR.m_SubsetIndices.Min(x => x);
                foreach (var index in meshR.m_SubsetIndices)
                {
                    subHashSet.Add((int)index);
                }
                combine = true;
            }
            int firstFace = 0;
            for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
            {
                int numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                if (subHashSet.Count > 0 && !subHashSet.Contains(i))
                {
                    firstFace += numFaces;
                    continue;
                }
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new ImportedSubmesh();
                Material mat = null;
                if (i - firstSubMesh < meshR.m_Materials.Length)
                {
                    if (assetsfileList.TryGetPD(meshR.m_Materials[i - firstSubMesh], out var MaterialPD))
                    {
                        mat = new Material(MaterialPD);
                    }
                }
                ImportedMaterial iMat = ConvertMaterial(mat);
                iSubmesh.Material = iMat.Name;
                iSubmesh.VertexList = new List<ImportedVertex>((int)submesh.vertexCount);
                var vertexColours = mesh.m_Colors != null && (mesh.m_Colors.Length == mesh.m_VertexCount * 3 || mesh.m_Colors.Length == mesh.m_VertexCount * 4);
                for (var j = mesh.m_SubMeshes[i].firstVertex; j < mesh.m_SubMeshes[i].firstVertex + mesh.m_SubMeshes[i].vertexCount; j++)
                {
                    var iVertex = vertexColours ? new ImportedVertexWithColour() : new ImportedVertex();
                    //Vertices
                    int c = 3;
                    if (mesh.m_Vertices.Length == mesh.m_VertexCount * 4)
                    {
                        c = 4;
                    }
                    iVertex.Position = new Vector3(-mesh.m_Vertices[j * c], mesh.m_Vertices[j * c + 1], mesh.m_Vertices[j * c + 2]);
                    //Normals
                    if (mesh.m_Normals?.Length > 0)
                    {
                        if (mesh.m_Normals.Length == mesh.m_VertexCount * 3)
                        {
                            c = 3;
                        }
                        else if (mesh.m_Normals.Length == mesh.m_VertexCount * 4)
                        {
                            c = 4;
                        }
                        iVertex.Normal = new Vector3(-mesh.m_Normals[j * c], mesh.m_Normals[j * c + 1], mesh.m_Normals[j * c + 2]);
                    }
                    //Colors
                    if (vertexColours)
                    {
                        if (mesh.m_Colors.Length == mesh.m_VertexCount * 3)
                        {
                            ((ImportedVertexWithColour)iVertex).Colour = new Color4(mesh.m_Colors[j * 3], mesh.m_Colors[j * 3 + 1], mesh.m_Colors[j * 3 + 2], 1.0f);
                        }
                        else
                        {
                            ((ImportedVertexWithColour)iVertex).Colour = new Color4(mesh.m_Colors[j * 4], mesh.m_Colors[j * 4 + 1], mesh.m_Colors[j * 4 + 2], mesh.m_Colors[j * 4 + 3]);
                        }
                    }
                    //UV
                    if (mesh.m_UV1 != null && mesh.m_UV1.Length == mesh.m_VertexCount * 2)
                    {
                        iVertex.UV = new[] { mesh.m_UV1[j * 2], -mesh.m_UV1[j * 2 + 1] };
                    }
                    else if (mesh.m_UV2 != null && mesh.m_UV2.Length == mesh.m_VertexCount * 2)
                    {
                        iVertex.UV = new[] { mesh.m_UV2[j * 2], -mesh.m_UV2[j * 2 + 1] };
                    }
                    //Tangent
                    if (mesh.m_Tangents != null && mesh.m_Tangents.Length == mesh.m_VertexCount * 4)
                    {
                        iVertex.Tangent = new Vector4(-mesh.m_Tangents[j * 4], mesh.m_Tangents[j * 4 + 1], mesh.m_Tangents[j * 4 + 2], mesh.m_Tangents[j * 4 + 3]);
                    }
                    //BoneInfluence
                    if (mesh.m_Skin.Length > 0)
                    {
                        var inf = mesh.m_Skin[j];
                        iVertex.BoneIndices = new byte[inf.Count];
                        iVertex.Weights = new float[inf.Count];
                        for (var k = 0; k < inf.Count; k++)
                        {
                            iVertex.BoneIndices[k] = (byte)inf[k].boneIndex;
                            iVertex.Weights[k] = inf[k].weight;
                        }
                    }
                    iSubmesh.VertexList.Add(iVertex);
                }
                //Face
                iSubmesh.FaceList = new List<ImportedFace>(numFaces);
                var end = firstFace + numFaces;
                for (int f = firstFace; f < end; f++)
                {
                    var face = new ImportedFace();
                    face.VertexIndices = new int[3];
                    face.VertexIndices[0] = (int)(mesh.m_Indices[f * 3 + 2] - submesh.firstVertex);
                    face.VertexIndices[1] = (int)(mesh.m_Indices[f * 3 + 1] - submesh.firstVertex);
                    face.VertexIndices[2] = (int)(mesh.m_Indices[f * 3] - submesh.firstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                firstFace = end;
                iMesh.SubmeshList.Add(iSubmesh);
            }

            if (meshR is SkinnedMeshRenderer sMesh)
            {
                //Bone
                iMesh.BoneList = new List<ImportedBone>(sMesh.m_Bones.Length);
                for (int i = 0; i < sMesh.m_Bones.Length; i++)
                {
                    var bone = new ImportedBone();
                    if (assetsfileList.TryGetTransform(sMesh.m_Bones[i], out var m_Transform))
                    {
                        if (assetsfileList.TryGetGameObject(m_Transform.m_GameObject, out var m_GameObject))
                        {
                            bone.Name = m_GameObject.m_Name;
                        }
                    }
                    //No first use m_BoneNameHashes, because it may be wrong
                    if (string.IsNullOrEmpty(bone.Name))
                    {
                        var boneHash = mesh.m_BoneNameHashes[i];
                        bone.Name = GetNameFromBonePathHashes(boneHash);
                        if (string.IsNullOrEmpty(bone.Name))
                        {
                            bone.Name = avatar?.FindBoneName(boneHash);
                        }
                        if (string.IsNullOrEmpty(bone.Name))
                        {
                            //throw new Exception("A Bone could neither be found by hash in Avatar nor by index in SkinnedMeshRenderer.");
                            continue;
                        }
                    }
                    var om = new float[4, 4];
                    var m = mesh.m_BindPose[i];
                    om[0, 0] = m[0, 0];
                    om[0, 1] = -m[1, 0];
                    om[0, 2] = -m[2, 0];
                    om[0, 3] = m[3, 0];
                    om[1, 0] = -m[0, 1];
                    om[1, 1] = m[1, 1];
                    om[1, 2] = m[2, 1];
                    om[1, 3] = m[3, 1];
                    om[2, 0] = -m[0, 2];
                    om[2, 1] = m[1, 2];
                    om[2, 2] = m[2, 2];
                    om[2, 3] = m[3, 2];
                    om[3, 0] = -m[0, 3];
                    om[3, 1] = m[1, 3];
                    om[3, 2] = m[2, 3];
                    om[3, 3] = m[3, 3];
                    bone.Matrix = om;
                    iMesh.BoneList.Add(bone);
                }

                if (sMesh.m_Bones.Length == 0 && mesh.m_BindPose?.Length > 0 && mesh.m_BoneNameHashes?.Length > 0)
                {
                    //TODO move to Init method use Animator.m_HasTransformHierarchy to judge
                    if (!deoptimize)
                    {
                        DeoptimizeTransformHierarchy();
                        deoptimize = true;
                    }
                    //TODO Repeat code with above
                    for (int i = 0; i < mesh.m_BindPose.Length; i++)
                    {
                        var bone = new ImportedBone();
                        var boneHash = mesh.m_BoneNameHashes[i];
                        bone.Name = GetNameFromBonePathHashes(boneHash);
                        if (string.IsNullOrEmpty(bone.Name))
                        {
                            bone.Name = avatar?.FindBoneName(boneHash);
                        }
                        if (string.IsNullOrEmpty(bone.Name))
                        {
                            //throw new Exception("A Bone could neither be found by hash in Avatar nor by index in SkinnedMeshRenderer.");
                            continue;
                        }
                        var om = new float[4, 4];
                        var m = mesh.m_BindPose[i];
                        om[0, 0] = m[0, 0];
                        om[0, 1] = -m[1, 0];
                        om[0, 2] = -m[2, 0];
                        om[0, 3] = m[3, 0];
                        om[1, 0] = -m[0, 1];
                        om[1, 1] = m[1, 1];
                        om[1, 2] = m[2, 1];
                        om[1, 3] = m[3, 1];
                        om[2, 0] = -m[0, 2];
                        om[2, 1] = m[1, 2];
                        om[2, 2] = m[2, 2];
                        om[2, 3] = m[3, 2];
                        om[3, 0] = -m[0, 3];
                        om[3, 1] = m[1, 3];
                        om[3, 2] = m[2, 3];
                        om[3, 3] = m[3, 3];
                        bone.Matrix = om;
                        iMesh.BoneList.Add(bone);
                    }
                }

                //Morphs
                if (mesh.m_Shapes != null)
                {
                    foreach (var channel in mesh.m_Shapes.channels)
                    {
                        morphChannelInfo[channel.nameHash] = channel.name;
                    }
                    if (mesh.m_Shapes.shapes.Count > 0)
                    {
                        ImportedMorph morph = null;
                        string lastGroup = "";
                        for (int i = 0; i < mesh.m_Shapes.channels.Count; i++)
                        {
                            string group = BlendShapeNameGroup(mesh, i);
                            if (group != lastGroup)
                            {
                                morph = new ImportedMorph();
                                MorphList.Add(morph);
                                morph.Name = iMesh.Name;
                                morph.ClipName = group;
                                morph.Channels = new List<Tuple<float, int, int>>(mesh.m_Shapes.channels.Count);
                                morph.KeyframeList = new List<ImportedMorphKeyframe>(mesh.m_Shapes.shapes.Count);
                                lastGroup = group;
                            }

                            morph.Channels.Add(new Tuple<float, int, int>(i < sMesh.m_BlendShapeWeights.Count ? sMesh.m_BlendShapeWeights[i] : 0f, morph.KeyframeList.Count, mesh.m_Shapes.channels[i].frameCount));
                            for (int frameIdx = 0; frameIdx < mesh.m_Shapes.channels[i].frameCount; frameIdx++)
                            {
                                ImportedMorphKeyframe keyframe = new ImportedMorphKeyframe();
                                keyframe.Name = BlendShapeNameExtension(mesh, i) + "_" + frameIdx;
                                int shapeIdx = mesh.m_Shapes.channels[i].frameIndex + frameIdx;
                                keyframe.VertexList = new List<ImportedVertex>((int)mesh.m_Shapes.shapes[shapeIdx].vertexCount);
                                keyframe.MorphedVertexIndices = new List<ushort>((int)mesh.m_Shapes.shapes[shapeIdx].vertexCount);
                                keyframe.Weight = shapeIdx < mesh.m_Shapes.fullWeights.Count ? mesh.m_Shapes.fullWeights[shapeIdx] : 100f;
                                int lastVertIndex = (int)(mesh.m_Shapes.shapes[shapeIdx].firstVertex + mesh.m_Shapes.shapes[shapeIdx].vertexCount);
                                for (int j = (int)mesh.m_Shapes.shapes[shapeIdx].firstVertex; j < lastVertIndex; j++)
                                {
                                    var morphVert = mesh.m_Shapes.vertices[j];
                                    ImportedVertex vert = GetSourceVertex(iMesh.SubmeshList, (int)morphVert.index);
                                    ImportedVertex destVert = new ImportedVertex();
                                    Vector3 morphPos = morphVert.vertex;
                                    morphPos.X *= -1;
                                    destVert.Position = vert.Position + morphPos;
                                    Vector3 morphNormal = morphVert.normal;
                                    morphNormal.X *= -1;
                                    destVert.Normal = morphNormal;
                                    Vector4 morphTangent = new Vector4(morphVert.tangent, 0);
                                    morphTangent.X *= -1;
                                    destVert.Tangent = morphTangent;
                                    keyframe.VertexList.Add(destVert);
                                    keyframe.MorphedVertexIndices.Add((ushort)morphVert.index);
                                }

                                morph.KeyframeList.Add(keyframe);
                            }
                        }
                    }
                }
            }

            //TODO
            if (combine)
            {
                assetsfileList.TryGetGameObject(meshR.m_GameObject, out var m_GameObject);
                foreach (var root in FrameList)
                {
                    var frame = ImportedHelpers.FindFrame(m_GameObject.m_Name, root);
                    if (frame?.Parent != null)
                    {
                        var parent = frame;
                        while (true)
                        {
                            if (parent.Parent != null)
                            {
                                parent = parent.Parent;
                            }
                            else
                            {
                                frame.LocalRotation = parent.LocalRotation;
                                frame.LocalScale = parent.LocalScale;
                                frame.LocalPosition = parent.LocalPosition;
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            MeshList.Add(iMesh);
        }

        private Mesh GetMesh(Renderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (assetsfileList.TryGetPD(sMesh.m_Mesh, out var MeshPD))
                {
                    return new Mesh(MeshPD);
                }
            }
            else
            {
                assetsfileList.TryGetGameObject(meshR.m_GameObject, out var m_GameObject);
                foreach (var m_Component in m_GameObject.m_Components)
                {
                    if (assetsfileList.TryGetPD(m_Component, out var assetPreloadData))
                    {
                        if (assetPreloadData.Type == ClassIDReference.MeshFilter)
                        {
                            var m_MeshFilter = new MeshFilter(assetPreloadData);
                            if (assetsfileList.TryGetPD(m_MeshFilter.m_Mesh, out var MeshPD))
                            {
                                return new Mesh(MeshPD);
                            }
                        }
                    }
                }
            }

            return null;
        }


        private string GetTransformPath(Transform meshTransform)
        {
            assetsfileList.TryGetGameObject(meshTransform.m_GameObject, out var m_GameObject);
            if (assetsfileList.TryGetTransform(meshTransform.m_Father, out var Father))
            {
                return GetTransformPath(Father) + "/" + m_GameObject.m_Name;
            }

            return m_GameObject.m_Name;
        }

        private ImportedMaterial ConvertMaterial(Material mat)
        {
            ImportedMaterial iMat;
            if (mat != null)
            {
                iMat = ImportedHelpers.FindMaterial(mat.m_Name, MaterialList);
                if (iMat != null)
                {
                    return iMat;
                }
                iMat = new ImportedMaterial();
                iMat.Name = mat.m_Name;
                foreach (var col in mat.m_Colors)
                {
                    var color = new Color4(col.second[0], col.second[1], col.second[2], col.second[3]);
                    switch (col.first)
                    {
                        case "_Color":
                            iMat.Diffuse = color;
                            break;
                        case "_SColor":
                            iMat.Ambient = color;
                            break;
                        case "_EmissionColor":
                            iMat.Emissive = color;
                            break;
                        case "_SpecColor":
                            iMat.Specular = color;
                            break;
                        case "_RimColor":
                        case "_OutlineColor":
                        case "_ShadowColor":
                            break;
                    }
                }

                foreach (var flt in mat.m_Floats)
                {
                    switch (flt.first)
                    {
                        case "_Shininess":
                            iMat.Power = flt.second;
                            break;
                        case "_RimPower":
                        case "_Outline":
                            break;
                    }
                }

                //textures
                iMat.Textures = new string[5];
                iMat.TexOffsets = new Vector2[5];
                iMat.TexScales = new Vector2[5];
                foreach (var texEnv in mat.m_TexEnvs)
                {
                    Texture2D tex2D = null;
                    if (assetsfileList.TryGetPD(texEnv.m_Texture, out var TexturePD) && TexturePD.Type == ClassIDReference.Texture2D)//TODO other Texture
                    {
                        tex2D = new Texture2D(TexturePD, true);
                    }

                    if (tex2D == null)
                    {
                        continue;
                    }
                    int dest = texEnv.name == "_MainTex" ? 0 : texEnv.name == "_BumpMap" ? 4 : texEnv.name.Contains("Spec") ? 2 : texEnv.name.Contains("Norm") ? 3 : -1;
                    if (dest < 0 || iMat.Textures[dest] != null)
                    {
                        continue;
                    }
                    iMat.Textures[dest] = TexturePD.Text + ".png";
                    iMat.TexOffsets[dest] = new Vector2(texEnv.m_Offset[0], texEnv.m_Offset[1]);
                    iMat.TexScales[dest] = new Vector2(texEnv.m_Scale[0], texEnv.m_Scale[1]);
                    ConvertTexture2D(tex2D, iMat.Textures[dest]);
                }

                MaterialList.Add(iMat);
            }
            else
            {
                iMat = new ImportedMaterial();
            }
            return iMat;
        }

        private void ConvertTexture2D(Texture2D tex2D, string name)
        {
            var iTex = ImportedHelpers.FindTexture(name, TextureList);
            if (iTex != null)
            {
                return;
            }

            using (var memStream = new MemoryStream())
            {
                var bitmap = new Texture2DConverter(tex2D).ConvertToBitmap(true);
                if (bitmap != null)
                {
                    bitmap.Save(memStream, ImageFormat.Png);
                    memStream.Position = 0;
                    iTex = new ImportedTexture(memStream, name);
                    TextureList.Add(iTex);
                    bitmap.Dispose();
                }
            }
        }

        private void ConvertAnimations()
        {
            foreach (var assetPreloadData in animationClipHashSet)
            {
                var animationClip = new AnimationClip(assetPreloadData);
                var iAnim = new ImportedKeyframedAnimation();
                AnimationList.Add(iAnim);
                iAnim.Name = animationClip.m_Name;
                iAnim.TrackList = new List<ImportedAnimationKeyframedTrack>();
                if (animationClip.m_Legacy)
                {
                    foreach (var m_CompressedRotationCurve in animationClip.m_CompressedRotationCurves)
                    {
                        var path = m_CompressedRotationCurve.m_Path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);

                        var numKeys = m_CompressedRotationCurve.m_Times.m_NumItems;
                        var data = m_CompressedRotationCurve.m_Times.UnpackInts();
                        var times = new float[numKeys];
                        int t = 0;
                        for (int i = 0; i < numKeys; i++)
                        {
                            t += data[i];
                            times[i] = t * 0.01f;
                        }
                        var quats = m_CompressedRotationCurve.m_Values.UnpackQuats();

                        for (int i = 0; i < numKeys; i++)
                        {
                            var quat = quats[i];
                            var value = Fbx.QuaternionToEuler(new Quaternion(quat.X, -quat.Y, -quat.Z, quat.W));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(times[i], value));
                        }
                    }
                    foreach (var m_RotationCurve in animationClip.m_RotationCurves)
                    {
                        var path = m_RotationCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        foreach (var m_Curve in m_RotationCurve.curve.m_Curve)
                        {
                            var value = Fbx.QuaternionToEuler(new Quaternion(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z, m_Curve.value.W));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, value));
                        }
                    }
                    foreach (var m_PositionCurve in animationClip.m_PositionCurves)
                    {
                        var path = m_PositionCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        foreach (var m_Curve in m_PositionCurve.curve.m_Curve)
                        {
                            track.Translations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(-m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    foreach (var m_ScaleCurve in animationClip.m_ScaleCurves)
                    {
                        var path = m_ScaleCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        foreach (var m_Curve in m_ScaleCurve.curve.m_Curve)
                        {
                            track.Scalings.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z)));
                        }
                    }
                    if (animationClip.m_EulerCurves != null)
                    {
                        foreach (var m_EulerCurve in animationClip.m_EulerCurves)
                        {
                            var path = m_EulerCurve.path;
                            var boneName = path.Substring(path.LastIndexOf('/') + 1);
                            var track = iAnim.FindTrack(boneName);
                            foreach (var m_Curve in m_EulerCurve.curve.m_Curve)
                            {
                                track.Rotations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, new Vector3(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z)));
                            }
                        }
                    }
                    foreach (var m_FloatCurve in animationClip.m_FloatCurves)
                    {
                        var path = m_FloatCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        foreach (var m_Curve in m_FloatCurve.curve.m_Curve)
                        {
                            track.Curve.Add(new ImportedKeyframe<float>(m_Curve.time, m_Curve.value));
                        }
                    }
                }
                else
                {
                    var m_Clip = animationClip.m_MuscleClip.m_Clip;
                    var streamedFrames = m_Clip.m_StreamedClip.ReadData();
                    var m_ClipBindingConstant = animationClip.m_ClipBindingConstant;
                    for (int frameIndex = 1; frameIndex < streamedFrames.Count - 1; frameIndex++)
                    {
                        var frame = streamedFrames[frameIndex];
                        var streamedValues = frame.keyList.Select(x => x.value).ToArray();
                        for (int curveIndex = 0; curveIndex < frame.keyList.Count;)
                        {
                            ReadCurveData(iAnim, m_ClipBindingConstant, frame.keyList[curveIndex].index, frame.time, streamedValues, 0, ref curveIndex);
                        }
                    }
                    var m_DenseClip = m_Clip.m_DenseClip;
                    var streamCount = m_Clip.m_StreamedClip.curveCount;
                    for (int frameIndex = 0; frameIndex < m_DenseClip.m_FrameCount; frameIndex++)
                    {
                        var time = frameIndex / m_DenseClip.m_SampleRate;
                        var frameOffset = frameIndex * m_DenseClip.m_CurveCount;
                        for (int curveIndex = 0; curveIndex < m_DenseClip.m_CurveCount;)
                        {
                            var index = streamCount + curveIndex;
                            ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time, m_DenseClip.m_SampleArray, (int)frameOffset, ref curveIndex);
                        }
                    }
                    var m_ConstantClip = m_Clip.m_ConstantClip;
                    var denseCount = m_Clip.m_DenseClip.m_CurveCount;
                    var time2 = 0.0f;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int curveIndex = 0; curveIndex < m_ConstantClip.data.Length;)
                        {
                            var index = streamCount + denseCount + curveIndex;
                            ReadCurveData(iAnim, m_ClipBindingConstant, (int)index, time2, m_ConstantClip.data, 0, ref curveIndex);
                        }
                        time2 = animationClip.m_MuscleClip.m_StopTime;
                    }
                }

                if ((bool)Properties.Settings.Default["FixRotation"])
                {
                    foreach (var track in iAnim.TrackList)
                    {
                        var prevKey = new Vector3();
                        foreach (var rotation in track.Rotations)
                        {
                            var value = rotation.value;
                            ReplaceOutOfBound(ref prevKey, ref value);
                            prevKey = value;
                            rotation.value = value;
                        }
                    }
                }
            }
        }

        private void ReadCurveData(ImportedKeyframedAnimation iAnim, AnimationClipBindingConstant m_ClipBindingConstant, int index, float time, float[] data, int offset, ref int curveIndex)
        {
            var binding = m_ClipBindingConstant.FindBinding(index);
            if (binding.path == 0)
            {
                curveIndex++;
                return;
            }
            var boneName = GetNameFromHashes(binding.path, binding.attribute);
            var track = iAnim.FindTrack(boneName);

            switch (binding.attribute)
            {
                case 1:
                    track.Translations.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                    (
                        -data[curveIndex++ + offset],
                        data[curveIndex++ + offset],
                        data[curveIndex++ + offset]
                    )));
                    break;
                case 2:
                    var value = Fbx.QuaternionToEuler(new Quaternion
                    (
                        data[curveIndex++ + offset],
                        -data[curveIndex++ + offset],
                        -data[curveIndex++ + offset],
                        data[curveIndex++ + offset]
                    ));
                    track.Rotations.Add(new ImportedKeyframe<Vector3>(time, value));
                    break;
                case 3:
                    track.Scalings.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                    (
                        data[curveIndex++ + offset],
                        data[curveIndex++ + offset],
                        data[curveIndex++ + offset]
                    )));
                    break;
                case 4:
                    track.Rotations.Add(new ImportedKeyframe<Vector3>(time, new Vector3
                    (
                        data[curveIndex++ + offset],
                        -data[curveIndex++ + offset],
                        -data[curveIndex++ + offset]
                    )));
                    break;
                default:
                    track.Curve.Add(new ImportedKeyframe<float>(time, data[curveIndex++]));
                    break;
            }
        }

        private string GetNameFromHashes(uint path, uint attribute)
        {
            var boneName = GetNameFromBonePathHashes(path);
            if (string.IsNullOrEmpty(boneName))
            {
                boneName = avatar?.FindBoneName(path);
            }
            if (string.IsNullOrEmpty(boneName))
            {
                boneName = "unknown " + path;
            }
            if (attribute > 4)
            {
                if (morphChannelInfo.TryGetValue(attribute, out var morphChannel))
                {
                    return boneName + "." + morphChannel;
                }
                return boneName + ".unknown_morphChannel " + attribute;
            }
            return boneName;
        }

        private string GetNameFromBonePathHashes(uint path)
        {
            if (bonePathHash.TryGetValue(path, out var boneName))
                boneName = boneName.Substring(boneName.LastIndexOf('/') + 1);
            return boneName;
        }

        private static string BlendShapeNameGroup(Mesh mesh, int index)
        {
            string name = mesh.m_Shapes.channels[index].name;
            int dotPos = name.IndexOf('.');
            if (dotPos >= 0)
            {
                return name.Substring(0, dotPos);
            }
            return "Ungrouped";
        }

        private static string BlendShapeNameExtension(Mesh mesh, int index)
        {
            string name = mesh.m_Shapes.channels[index].name;
            int dotPos = name.IndexOf('.');
            if (dotPos >= 0)
            {
                return name.Substring(dotPos + 1);
            }
            return name;
        }

        private static ImportedVertex GetSourceVertex(List<ImportedSubmesh> submeshList, int morphVertIndex)
        {
            foreach (var submesh in submeshList)
            {
                var vertList = submesh.VertexList;
                if (morphVertIndex < vertList.Count)
                {
                    return vertList[morphVertIndex];
                }
                morphVertIndex -= vertList.Count;
            }
            return null;
        }

        private void CreateBonePathHash(Transform m_Transform)
        {
            var name = GetTransformPath(m_Transform);
            var crc = new SevenZip.CRC();
            var bytes = Encoding.UTF8.GetBytes(name);
            crc.Update(bytes, 0, (uint)bytes.Length);
            bonePathHash[crc.GetDigest()] = name;
            int index;
            while ((index = name.IndexOf("/", StringComparison.Ordinal)) >= 0)
            {
                name = name.Substring(index + 1);
                crc = new SevenZip.CRC();
                bytes = Encoding.UTF8.GetBytes(name);
                crc.Update(bytes, 0, (uint)bytes.Length);
                bonePathHash[crc.GetDigest()] = name;
            }
            foreach (var pptr in m_Transform.m_Children)
            {
                if (assetsfileList.TryGetTransform(pptr, out var child))
                    CreateBonePathHash(child);
            }
        }

        private void ReplaceOutOfBound(ref Vector3 prevKey, ref Vector3 curKey)
        {
            curKey.X = ReplaceOutOfBound(prevKey.X, curKey.X);
            curKey.Y = ReplaceOutOfBound(prevKey.Y, curKey.Y);
            curKey.Z = ReplaceOutOfBound(prevKey.Z, curKey.Z);
        }

        private float ReplaceOutOfBound(float prevValue, float curValue)
        {
            double prev = prevValue;
            double cur = curValue;

            double prevAbs = Math.Abs(prev);
            double prevSign = Math.Sign(prev);

            double prevShift = 180.0 + prevAbs;
            double count = Math.Floor(prevShift / 360.0) * prevSign;
            double prevRemain = 180.0 + (prev - count * 360.0);

            double curShift = 180.0 + cur;

            if (prevRemain - curShift > 180)
            {
                count++;
            }
            else if (prevRemain - curShift < -180)
            {
                count--;
            }

            double newValue = count * 360.0 + cur;
            return (float)newValue;
        }

        private void DeoptimizeTransformHierarchy()
        {
            if (avatar == null)
                return;
            // 1. Figure out the skeletonPaths from the unstripped avatar
            var skeletonPaths = new List<string>();
            foreach (var id in avatar.m_Avatar.m_AvatarSkeleton.m_ID)
            {
                var path = avatar.FindBonePath(id);
                skeletonPaths.Add(path);
            }
            // 2. Restore the original transform hierarchy
            // Prerequisite: skeletonPaths follow pre-order traversal
            var rootFrame = FrameList[0];
            rootFrame.ClearChild();
            for (var i = 1; i < skeletonPaths.Count; i++) // start from 1, skip the root transform because it will always be there.
            {
                var path = skeletonPaths[i];
                var strs = path.Split('/');
                string transformName;
                ImportedFrame parentFrame;
                if (strs.Length == 1)
                {
                    transformName = path;
                    parentFrame = rootFrame;
                }
                else
                {
                    transformName = strs.Last();
                    var parentFrameName = strs[strs.Length - 2];
                    parentFrame = ImportedHelpers.FindFrame(parentFrameName, rootFrame);
                }

                var skeletonPose = avatar.m_Avatar.m_DefaultPose;
                var xform = skeletonPose.m_X[i];
                if (!(xform.t is Vector3 t))
                {
                    var v4 = (Vector4)xform.t;
                    t = (Vector3)v4;
                }
                if (!(xform.s is Vector3 s))
                {
                    var v4 = (Vector4)xform.s;
                    s = (Vector3)v4;
                }
                var frame = ConvertFrame(t, xform.q, s, transformName);
                parentFrame.AddChild(frame);
            }
        }
    }
}

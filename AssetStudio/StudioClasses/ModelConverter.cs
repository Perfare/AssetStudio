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
        public List<ImportedAnimation> AnimationList { get; protected set; } = new List<ImportedAnimation>();
        public List<ImportedMorph> MorphList { get; protected set; } = new List<ImportedMorph>();

        private Avatar avatar;
        private Dictionary<uint, string> morphChannelInfo = new Dictionary<uint, string>();
        private HashSet<AssetPreloadData> animationClipHashSet = new HashSet<AssetPreloadData>();
        private Dictionary<uint, string> bonePathHash = new Dictionary<uint, string>();

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
            //In fact, doesn't need this.
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
                frameList.Add(ConvertFrames(m_Father));
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

        private ImportedFrame ConvertFrames(Transform trans)
        {
            var frame = new ImportedFrame();
            assetsfileList.TryGetGameObject(trans.m_GameObject, out var m_GameObject);
            frame.Name = m_GameObject.m_Name;
            frame.InitChildren(trans.m_Children.Count);
            Quaternion mirroredRotation = new Quaternion(trans.m_LocalRotation[0], trans.m_LocalRotation[1], trans.m_LocalRotation[2], trans.m_LocalRotation[3]);
            mirroredRotation.Y *= -1;
            mirroredRotation.Z *= -1;
            var m_LocalScale = new Vector3(trans.m_LocalScale[0], trans.m_LocalScale[1], trans.m_LocalScale[2]);
            var m_LocalPosition = new Vector3(trans.m_LocalPosition[0], trans.m_LocalPosition[1], trans.m_LocalPosition[2]);
            frame.Matrix = Matrix.Scaling(m_LocalScale) * Matrix.RotationQuaternion(mirroredRotation) * Matrix.Translation(-m_LocalPosition.X, m_LocalPosition.Y, m_LocalPosition.Z);
            return frame;
        }

        private void ConvertFrames(Transform trans, ImportedFrame parent)
        {
            var frame = ConvertFrames(trans);
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

        private void ConvertMeshRenderer(MeshRenderer meshR)
        {
            var mesh = GetMesh(meshR);
            if (mesh == null)
                return;
            var iMesh = new ImportedMesh();
            assetsfileList.TryGetGameObject(meshR.m_GameObject, out var m_GameObject2);
            assetsfileList.TryGetTransform(m_GameObject2.m_Transform, out var meshTransform);
            iMesh.Name = GetTransformPath(meshTransform);
            iMesh.SubmeshList = new List<ImportedSubmesh>(mesh.m_SubMeshes.Count);
            int sum = 0;
            for (int i = 0; i < mesh.m_SubMeshes.Count; i++)
            {
                var submesh = mesh.m_SubMeshes[i];
                var iSubmesh = new ImportedSubmesh();
                Material mat = null;
                if (i < meshR.m_Materials.Length)
                {
                    if (assetsfileList.TryGetPD(meshR.m_Materials[i], out var MaterialPD))
                    {
                        mat = new Material(MaterialPD);
                    }
                }
                ImportedMaterial iMat = ConvertMaterial(mat);
                iSubmesh.Material = iMat.Name;
                iSubmesh.VertexList = new List<ImportedVertex>((int)submesh.vertexCount);
                var vertexColours = mesh.m_Colors != null && mesh.m_Colors.Length > 0;
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
                    if (mesh.m_Normals != null && mesh.m_Normals.Length > 0)
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
                    if (mesh.m_Tangents != null)
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
                int numFaces = (int)mesh.m_SubMeshes[i].indexCount / 3;
                iSubmesh.FaceList = new List<ImportedFace>(numFaces);
                var end = sum + numFaces;
                for (int f = sum; f < end; f++)
                {
                    var face = new ImportedFace();
                    face.VertexIndices = new int[3];
                    face.VertexIndices[0] = (int)(mesh.m_Indices[f * 3 + 2] - submesh.firstVertex);
                    face.VertexIndices[1] = (int)(mesh.m_Indices[f * 3 + 1] - submesh.firstVertex);
                    face.VertexIndices[2] = (int)(mesh.m_Indices[f * 3] - submesh.firstVertex);
                    iSubmesh.FaceList.Add(face);
                }
                sum = end;
                iMesh.SubmeshList.Add(iSubmesh);
            }

            if (meshR is SkinnedMeshRenderer sMesh)
            {
                //Bone
                iMesh.BoneList = new List<ImportedBone>(sMesh.m_Bones.Length);
                /*if (sMesh.m_Bones.Length >= 256)
                {
                    throw new Exception("Too many bones (" + mesh.m_BindPose.Length + ")");
                }*/
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
                            throw new Exception("A Bone could neither be found by hash in Avatar nor by index in SkinnedMeshRenderer.");
                        }
                    }

                    var om = new Matrix();
                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            om[x, y] = mesh.m_BindPose[i][x, y];
                        }
                    }
                    var m = Matrix.Transpose(om);
                    m.Decompose(out var s, out var q, out var t);
                    t.X *= -1;
                    q.Y *= -1;
                    q.Z *= -1;
                    bone.Matrix = Matrix.Scaling(s) * Matrix.RotationQuaternion(q) * Matrix.Translation(t);
                    iMesh.BoneList.Add(bone);
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

            MeshList.Add(iMesh);
        }

        private Mesh GetMesh(MeshRenderer meshR)
        {
            if (meshR is SkinnedMeshRenderer sMesh)
            {
                if (assetsfileList.TryGetPD(sMesh.m_Mesh, out var MeshPD))
                {
                    return new Mesh(MeshPD, true);
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
                                return new Mesh(MeshPD, true);
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

            return String.Empty + m_GameObject.m_Name;
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
                var clip = new AnimationClip(assetPreloadData);
                if (clip.m_Legacy)
                {
                    var iAnim = new ImportedKeyframedAnimation();
                    iAnim.Name = clip.m_Name;
                    AnimationList.Add(iAnim);
                    iAnim.TrackList = new List<ImportedAnimationKeyframedTrack>();
                    foreach (var m_RotationCurve in clip.m_RotationCurves)
                    {
                        var path = m_RotationCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        if (track == null)
                        {
                            track = new ImportedAnimationKeyframedTrack();
                            track.Name = boneName;
                            iAnim.TrackList.Add(track);
                        }
                        foreach (var m_Curve in m_RotationCurve.curve.m_Curve)
                        {
                            var value = Fbx.QuaternionToEuler(new Quaternion(m_Curve.value.X, -m_Curve.value.Y, -m_Curve.value.Z, m_Curve.value.W));
                            var inSlope = Fbx.QuaternionToEuler(new Quaternion(m_Curve.inSlope.X, -m_Curve.inSlope.Y, -m_Curve.inSlope.Z, m_Curve.inSlope.W));
                            var outSlope = Fbx.QuaternionToEuler(new Quaternion(m_Curve.outSlope.X, -m_Curve.outSlope.Y, -m_Curve.outSlope.Z, m_Curve.outSlope.W));
                            track.Rotations.Add(new ImportedKeyframe<Vector3>(m_Curve.time, value, inSlope, outSlope));
                        }
                    }
                    foreach (var m_PositionCurve in clip.m_PositionCurves)
                    {
                        var path = m_PositionCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        if (track == null)
                        {
                            track = new ImportedAnimationKeyframedTrack();
                            track.Name = boneName;
                            iAnim.TrackList.Add(track);
                        }
                        foreach (var m_Curve in m_PositionCurve.curve.m_Curve)
                        {
                            track.Translations.Add(new ImportedKeyframe<Vector3>(
                                m_Curve.time,
                                new Vector3(-m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z),
                                new Vector3(-m_Curve.inSlope.X, m_Curve.inSlope.Y, m_Curve.inSlope.Z),
                                new Vector3(-m_Curve.outSlope.X, m_Curve.outSlope.Y, m_Curve.outSlope.Z)));
                        }
                    }
                    foreach (var m_ScaleCurve in clip.m_ScaleCurves)
                    {
                        var path = m_ScaleCurve.path;
                        var boneName = path.Substring(path.LastIndexOf('/') + 1);
                        var track = iAnim.FindTrack(boneName);
                        if (track == null)
                        {
                            track = new ImportedAnimationKeyframedTrack();
                            track.Name = boneName;
                            iAnim.TrackList.Add(track);
                        }
                        foreach (var m_Curve in m_ScaleCurve.curve.m_Curve)
                        {
                            track.Scalings.Add(new ImportedKeyframe<Vector3>(
                                m_Curve.time,
                                new Vector3(m_Curve.value.X, m_Curve.value.Y, m_Curve.value.Z),
                                new Vector3(m_Curve.inSlope.X, m_Curve.inSlope.Y, m_Curve.inSlope.Z),
                                new Vector3(m_Curve.outSlope.X, m_Curve.outSlope.Y, m_Curve.outSlope.Z)));
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
                else
                {
                    var iAnim = new ImportedSampledAnimation();
                    iAnim.Name = clip.m_Name;
                    iAnim.SampleRate = clip.m_SampleRate;
                    AnimationList.Add(iAnim);
                    int numTracks = (clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + (int)clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount + 9) / 10;
                    iAnim.TrackList = new List<ImportedAnimationSampledTrack>(numTracks);
                    var streamedFrames = clip.m_MuscleClip.m_Clip.m_StreamedClip.ReadData();
                    float[] streamedValues = new float[clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount];
                    int numFrames = Math.Max(clip.m_MuscleClip.m_Clip.m_DenseClip.m_FrameCount, streamedFrames.Count - 2);
                    for (int frameIdx = 0; frameIdx < numFrames; frameIdx++)
                    {
                        if (1 + frameIdx < streamedFrames.Count)
                        {
                            for (int i = 0; i < streamedFrames[1 + frameIdx].keyList.Count; i++)
                            {
                                streamedValues[i] = streamedFrames[1 + frameIdx].keyList[i].value;
                            }
                        }

                        int numStreamedCurves = 1 + frameIdx < streamedFrames.Count ? streamedFrames[1 + frameIdx].keyList.Count : 0;
                        int numCurves = numStreamedCurves + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount + clip.m_MuscleClip.m_Clip.m_ConstantClip.data.Length;
                        int streamOffset = numStreamedCurves - (int)clip.m_MuscleClip.m_Clip.m_StreamedClip.curveCount;
                        for (int curveIdx = 0; curveIdx < numCurves;)
                        {
                            GenericBinding binding;
                            float[] data;
                            int dataOffset;
                            if (1 + frameIdx < streamedFrames.Count && curveIdx < streamedFrames[1 + frameIdx].keyList.Count)
                            {
                                binding = clip.m_ClipBindingConstant.FindBinding(streamedFrames[1 + frameIdx].keyList[curveIdx].index);
                                data = streamedValues;
                                dataOffset = 0;
                            }
                            else if (curveIdx < numStreamedCurves + clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount)
                            {
                                binding = clip.m_ClipBindingConstant.FindBinding(curveIdx - streamOffset);
                                data = clip.m_MuscleClip.m_Clip.m_DenseClip.m_SampleArray;
                                dataOffset = numStreamedCurves - frameIdx * (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount;
                            }
                            else
                            {
                                binding = clip.m_ClipBindingConstant.FindBinding(curveIdx - streamOffset);
                                data = clip.m_MuscleClip.m_Clip.m_ConstantClip.data;
                                dataOffset = numStreamedCurves + (int)clip.m_MuscleClip.m_Clip.m_DenseClip.m_CurveCount;
                            }

                            if (binding.path == 0)
                            {
                                curveIdx++;
                                continue;
                            }

                            string boneName = GetNameFromHashes(binding.path, binding.attribute);
                            ImportedAnimationSampledTrack track = iAnim.FindTrack(boneName);
                            if (track == null)
                            {
                                track = new ImportedAnimationSampledTrack();
                                track.Name = boneName;
                                iAnim.TrackList.Add(track);
                            }

                            try
                            {
                                switch (binding.attribute)
                                {
                                    case 1:
                                        if (track.Translations == null)
                                        {
                                            track.Translations = new Vector3?[numFrames];
                                        }

                                        track.Translations[frameIdx] = new Vector3
                                        (
                                            -data[curveIdx++ - dataOffset],
                                            data[curveIdx++ - dataOffset],
                                            data[curveIdx++ - dataOffset]
                                        );
                                        break;
                                    case 2:
                                        if (track.Rotations == null)
                                        {
                                            track.Rotations = new Vector3?[numFrames];
                                        }

                                        track.Rotations[frameIdx] = Fbx.QuaternionToEuler(new Quaternion
                                        (
                                            data[curveIdx++ - dataOffset],
                                            -data[curveIdx++ - dataOffset],
                                            -data[curveIdx++ - dataOffset],
                                            data[curveIdx++ - dataOffset]
                                        ));
                                        break;
                                    case 3:
                                        if (track.Scalings == null)
                                        {
                                            track.Scalings = new Vector3?[numFrames];
                                        }

                                        track.Scalings[frameIdx] = new Vector3
                                        (
                                            data[curveIdx++ - dataOffset],
                                            data[curveIdx++ - dataOffset],
                                            data[curveIdx++ - dataOffset]
                                        );
                                        break;
                                    case 4:
                                        if (track.Rotations == null)
                                        {
                                            track.Rotations = new Vector3?[numFrames];
                                        }

                                        track.Rotations[frameIdx] = new Vector3
                                        (
                                            data[curveIdx++ - dataOffset],
                                            -data[curveIdx++ - dataOffset],
                                            -data[curveIdx++ - dataOffset]
                                        );
                                        break;
                                    default:
                                        if (track.Curve == null)
                                        {
                                            track.Curve = new float?[numFrames];
                                        }

                                        track.Curve[frameIdx] = data[curveIdx++ - dataOffset];
                                        break;
                                }
                            }
                            catch
                            {
                                //errors.Append("   ").Append(boneName).Append(" a=").Append(binding.attribute).Append(" ci=").Append(curveIdx).Append("/#=").Append(numCurves).Append(" of=").Append(dataOffset).Append(" f=").Append(frameIdx).Append("/#=").Append(numFrames).Append("\n");
                                //TODO Display error
                                break;
                            }
                        }
                    }

                    if ((bool)Properties.Settings.Default["FixRotation"])
                    {
                        foreach (var track in iAnim.TrackList)
                        {
                            if (track.Rotations == null)
                                continue;
                            var prevKey = new Vector3();
                            for (var i = 0; i < track.Rotations.Length; i++)
                            {
                                var rotation = track.Rotations[i];
                                if (rotation == null)
                                    continue;
                                var value = new Vector3(rotation.Value.X, rotation.Value.Y, rotation.Value.Z);
                                ReplaceOutOfBound(ref prevKey, ref value);
                                prevKey = value;
                                track.Rotations[i] = value;
                            }
                        }
                    }
                }
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
    }
}

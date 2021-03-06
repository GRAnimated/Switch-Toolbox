﻿using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using OpenTK;
using Switch_Toolbox.Library.Rendering;
using System.Windows.Forms;

namespace Switch_Toolbox.Library
{
    public class AssimpData
    {
        public Scene scene;

        public List<STGenericObject> objects = new List<STGenericObject>();
        public List<STGenericMaterial> materials = new List<STGenericMaterial>();
        public List<STGenericTexture> textures = new List<STGenericTexture>();

        public AssimpContext Importer = new AssimpContext();

        public string[] GetSupportedImportFormats()
        {
            return Importer.GetSupportedImportFormats();
        }

        public AssimpData()
        {
        }
        public void LoadFile(string FileName)
        {
            try
            {
                AssimpContext Importer = new AssimpContext();

                scene = Importer.ImportFile(FileName,
                    PostProcessSteps.Triangulate           |
                    PostProcessSteps.JoinIdenticalVertices |
                    PostProcessSteps.FlipUVs               |
                    PostProcessSteps.LimitBoneWeights      |
                    PostProcessSteps.CalculateTangentSpace |
                    PostProcessSteps.GenerateNormals);
                LoadScene();
            }
            catch (Exception e)
            {
                if (e.ToString().Contains("Error loading unmanaged library from path"))
                {
                    MessageBox.Show($"Failed to load assimp! Make sure you have Assimp32.dll next to the program!");
                }
                Console.WriteLine(e);
            }
        }
        public void processNode()
        {
            Matrix4x4 identity = Matrix4x4.Identity;
            if (scene.RootNode != null)
            {
                BuildNode(scene.RootNode, ref identity);
            }
            else
            {
                int Index = 0;
                foreach (Mesh msh in scene.Meshes)
                {
                    objects.Add(CreateGenericObject(msh, Index, Matrix4.Identity));
                    Index++;
                }
            }
        }
        private void BuildNode(Node parent, ref Matrix4x4 rootTransform)
        {
            Matrix4x4 trafo = parent.Transform;
            Matrix4x4 world = trafo * rootTransform;
            Matrix4 worldTK = AssimpHelper.TKMatrix(world);

            foreach (int index in parent.MeshIndices)
                objects.Add(CreateGenericObject(scene.Meshes[index], index, worldTK));
            
            foreach (Node child in parent.Children)
                BuildNode(child, ref world);
        }
        public void LoadScene()
        {
            objects.Clear();
            textures.Clear();
            materials.Clear();

            processNode();
            if (scene.HasMaterials)
            {
                foreach (Material mat in scene.Materials)
                {
                    materials.Add(CreateGenericMaterial(mat));
                }
            }
            foreach (Assimp.Animation animation in scene.Animations)
            {
                  
            }
            foreach (var tex in scene.Textures)
            {
            }
        }
        public Animation CreateGenericAnimation(Assimp.Animation animation)
        {
            Animation STanim = new Animation();
            STanim.Text = animation.Name;
            STanim.FrameCount = (int)animation.DurationInTicks;

            //Load node animations
            if (animation.HasNodeAnimations)
            {
                var _channels = new NodeAnimationChannel[animation.NodeAnimationChannelCount];
                for (int i = 0; i < _channels.Length; i++)
                {
                    _channels[i] = new NodeAnimationChannel();
                }
            }

            //Load mesh animations
            if (animation.HasMeshAnimations)
            {
                var _meshChannels = new MeshAnimationChannel[animation.MeshAnimationChannelCount];
                for (int i = 0; i < _meshChannels.Length; i++)
                {
                    _meshChannels[i] = new MeshAnimationChannel();
                }
            }

            return STanim;
        }
        public STGenericTexture CreateGenericTexture(string Path)
        {
            STGenericTexture tex = new STGenericTexture();

            switch (System.IO.Path.GetExtension(Path))
            {
                case ".dds":
                    tex.LoadDDS(Path);
                    break;
                case ".tga":
                    tex.LoadTGA(Path);
                    break;
                default:
                    tex.LoadBitmap(Path);
                    break;
            }
            return tex;
        }
        public STGenericMaterial CreateGenericMaterial(Material material)
        {
            STGenericMaterial mat = new STGenericMaterial();
            mat.Text = material.Name;

            foreach (var slot in material.GetAllMaterialTextures())
            {
                textures.Add(CreateGenericTexture(slot.FilePath));
            }

            TextureSlot tex;
            if (material.GetMaterialTexture(TextureType.Diffuse, 0, out tex))
                mat.TextureMaps.Add(CreateTextureSlot(tex, TextureType.Diffuse));
            if (material.GetMaterialTexture(TextureType.Normals, 1, out tex))
                mat.TextureMaps.Add(CreateTextureSlot(tex, TextureType.Normals));
            if (material.GetMaterialTexture(TextureType.Specular, 1, out tex))
                mat.TextureMaps.Add(CreateTextureSlot(tex, TextureType.Specular));
            return mat;
        }
        private STGenericMatTexture CreateTextureSlot(TextureSlot tex, TextureType type)
        {
            var matTex = new STGenericMatTexture();

            switch (type)
            {
                case TextureType.Diffuse:
                    matTex.Type = STGenericMatTexture.TextureType.Diffuse;
                    break;
                case TextureType.Normals:
                    matTex.Type = STGenericMatTexture.TextureType.Normal;
                    break;
                case TextureType.Lightmap:
                    matTex.Type = STGenericMatTexture.TextureType.Light;
                    break;
                case TextureType.Emissive:
                    matTex.Type = STGenericMatTexture.TextureType.Emission;
                    break;
                case TextureType.Specular:
                    matTex.Type = STGenericMatTexture.TextureType.Specular;
                    break;
                case TextureType.Shininess:
                    matTex.Type = STGenericMatTexture.TextureType.Metalness;
                    break;
                case TextureType.Opacity:
                    matTex.Type = STGenericMatTexture.TextureType.Transparency;
                    break;
                case TextureType.Displacement:
                    break;
                default:
                    matTex.Type = STGenericMatTexture.TextureType.Unknown;
                    break;
            }

            matTex.Name = System.IO.Path.GetFileNameWithoutExtension(tex.FilePath);
            matTex.wrapModeS = SetWrapMode(tex.WrapModeU);
            matTex.wrapModeT = SetWrapMode(tex.WrapModeV);

            return matTex;
        }
        private int SetWrapMode(TextureWrapMode wrap)
        {
            switch (wrap)
            {
                case TextureWrapMode.Wrap:
                    return 0;
                case TextureWrapMode.Mirror:
                    return 1;
                case TextureWrapMode.Clamp:
                    return 2;
                case TextureWrapMode.Decal:
                    return 0;
                default:
                    return 0;
            }
        }
        public STGenericObject CreateGenericObject(Mesh msh, int Index, Matrix4 transform)
        {
            STGenericObject obj = new STGenericObject();

            Console.WriteLine(msh.MaterialIndex);
            if (msh.MaterialIndex != -1)
                obj.MaterialIndex = msh.MaterialIndex;
            else
                scene.Materials.Add(new Material() { Name = msh.Name });

            if (scene.Materials[msh.MaterialIndex].Name == "")
                scene.Materials[msh.MaterialIndex].Name = msh.Name;

            obj.HasPos = msh.HasVertices;
            obj.HasNrm = msh.HasNormals;
            obj.HasUv0 = msh.HasTextureCoords(0);
            obj.HasUv1 = msh.HasTextureCoords(1);
            obj.HasUv2 = msh.HasTextureCoords(2);
            obj.HasIndices = msh.HasBones;
            if (msh.HasBones)
                obj.HasWeights = msh.Bones[0].HasVertexWeights;

            obj.HasTans = msh.HasTangentBasis;
            obj.HasBitans = msh.HasTangentBasis;
            obj.HasVertColors = msh.HasVertexColors(0);
            obj.ObjectName = msh.Name;
            obj.boneList = GetBoneList(msh);
            obj.MaxSkinInfluenceCount = GetVertexSkinCount(msh);

            STGenericObject.LOD_Mesh lod = new STGenericObject.LOD_Mesh();
            lod.faces = GetFaces(msh);
            lod.IndexFormat = STIndexFormat.UInt16;
            lod.PrimitiveType = STPolygonType.Triangle;
            lod.GenerateSubMesh();
            obj.lodMeshes.Add(lod);
            obj.vertices = GetVertices(msh, transform, obj);
            obj.VertexBufferIndex = Index;

            return obj;
        }
        public void SaveFromModel(STGenericModel model, string FileName, List<STGenericTexture> Textures)
        {
            Scene scene = new Scene();
            scene.RootNode = new Node("RootNode");
           

            int MeshIndex = 0;
            foreach (var obj in model.Nodes[0].Nodes)
            {
                var genericObj = (STGenericObject)obj;

                Mesh mesh = new Mesh(genericObj.Text, PrimitiveType.Triangle);
                mesh.MaterialIndex = genericObj.MaterialIndex;

                List<Vector3D> textureCoords0 = new List<Vector3D>();
                List<Vector3D> textureCoords1 = new List<Vector3D>();
                List<Vector3D> textureCoords2 = new List<Vector3D>();
                List<Color4D> vertexColors = new List<Color4D>();
                foreach (Vertex v in genericObj.vertices)
                {
                    mesh.Vertices.Add(new Vector3D(v.pos.X, v.pos.Y, v.pos.Z));
                    mesh.Normals.Add(new Vector3D(v.nrm.X, v.nrm.Y, v.nrm.Z));
                    textureCoords0.Add(new Vector3D(v.uv0.X, v.uv0.Y, 0));
                    textureCoords1.Add(new Vector3D(v.uv1.X, v.uv1.Y, 0));
                    textureCoords2.Add(new Vector3D(v.uv2.X, v.uv2.Y, 0));
                    vertexColors.Add(new Color4D(v.col.X, v.col.Y, v.col.Z, v.col.W));
                    mesh.TextureCoordinateChannels[0] = textureCoords0;
                    mesh.TextureCoordinateChannels[1] = textureCoords1;
                    mesh.TextureCoordinateChannels[2] = textureCoords2;
                    mesh.VertexColorChannels[0] = vertexColors;
                }
                List<int> faces = genericObj.lodMeshes[genericObj.DisplayLODIndex].faces;
                for (int f = 0; f < faces.Count; f++)
                    mesh.Faces.Add(new Face(new int[] { faces[f++], faces[f++], faces[f] }));
                
                mesh.TextureCoordinateChannels.SetValue(textureCoords0, 0);

                scene.Meshes.Add(mesh);

                MeshIndex++;
            }

            string TextureExtension = ".png";
            string TexturePath = System.IO.Path.GetDirectoryName(FileName);

            foreach (var mat in model.Nodes[1].Nodes)
            {
                var genericMat = (STGenericMaterial)mat;

                Material material = new Material();
                material.Name = genericMat.Text;

                foreach (var tex in genericMat.TextureMaps)
                {
                    TextureSlot slot = new TextureSlot();
                    string path = System.IO.Path.Combine(TexturePath, tex.Name + TextureExtension);
                    slot.FilePath = path;
                    slot.UVIndex = 0;
                    slot.Flags = 0;
                    slot.TextureIndex = 0;
                    slot.BlendFactor = 1.0f;
                    slot.Mapping = TextureMapping.FromUV;
                    slot.Operation = TextureOperation.Add;

                    if (tex.Type == STGenericMatTexture.TextureType.Diffuse)
                        slot.TextureType = TextureType.Diffuse;
                    else if (tex.Type == STGenericMatTexture.TextureType.Normal)
                        slot.TextureType = TextureType.Normals;
                    else if (tex.Type == STGenericMatTexture.TextureType.Specular)
                        slot.TextureType = TextureType.Specular;
                    else if (tex.Type == STGenericMatTexture.TextureType.Emission)
                        slot.TextureType = TextureType.Emissive;
                    else if (tex.Type == STGenericMatTexture.TextureType.Light)
                    {
                        slot.TextureType = TextureType.Lightmap;
                        slot.UVIndex = 2;
                    }
                    else if (tex.Type == STGenericMatTexture.TextureType.Shadow)
                    {
                        slot.TextureType = TextureType.Ambient;
                        slot.UVIndex = 1;
                    }
                    else
                        slot.TextureType = TextureType.Unknown;

                    if (tex.wrapModeS == 0)
                        slot.WrapModeU = TextureWrapMode.Wrap;
                    if (tex.wrapModeS == 1)
                        slot.WrapModeU = TextureWrapMode.Mirror;
                    if (tex.wrapModeS == 2)
                        slot.WrapModeU = TextureWrapMode.Clamp;
                    if (tex.wrapModeT == 0)
                        slot.WrapModeV = TextureWrapMode.Wrap;
                    if (tex.wrapModeT == 1)
                        slot.WrapModeV = TextureWrapMode.Mirror;
                    if (tex.wrapModeT == 2)
                        slot.WrapModeV = TextureWrapMode.Clamp;
                    else
                    {
                        slot.WrapModeU = TextureWrapMode.Wrap;
                        slot.WrapModeV = TextureWrapMode.Wrap;
                    }

                    material.AddMaterialTexture(ref slot);
                }
                scene.Materials.Add(material);
            }
            foreach (var tex in Textures)
            {
                string path = System.IO.Path.Combine(TexturePath, tex.Name + TextureExtension);
                if (!System.IO.File.Exists(path))
                    tex.GetBitmap().Save(path);
            }

            using (var v = new AssimpContext())
            {
                string ext = System.IO.Path.GetExtension(FileName);

                string formatID = "obj";
                if (ext == ".obj")
                    formatID = "obj";
                if (ext == ".fbx")
                    formatID = "collada";
                if (ext == ".dae")
                    formatID = "collada";

                if (v.ExportFile(scene, FileName, formatID, PostProcessSteps.ValidateDataStructure))
                    System.Windows.Forms.MessageBox.Show($"Exported {FileName} Successfuly!");
                else
                    System.Windows.Forms.MessageBox.Show($"Failed to export {FileName}!");
            }
        }
        public void SaveFromObject(List<Vertex> vertices, List<int> faces, string MeshName, string FileName)
        {
            Scene scene = new Scene();
            scene.RootNode = new Node("Root");

            Mesh mesh = new Mesh(MeshName, PrimitiveType.Triangle);

            List<Vector3D> textureCoords0 = new List<Vector3D>();
            List<Vector3D> textureCoords1 = new List<Vector3D>();
            List<Vector3D> textureCoords2 = new List<Vector3D>();
            List<Color4D> vertexColors = new List<Color4D>();

            foreach (Vertex v in vertices)
            {
                mesh.Vertices.Add(new Vector3D(v.pos.X, v.pos.Y, v.pos.Z));
                mesh.Normals.Add(new Vector3D(v.nrm.X, v.nrm.Y, v.nrm.Z));
                textureCoords0.Add(new Vector3D(v.uv0.X, v.uv0.Y, 0));
                textureCoords1.Add(new Vector3D(v.uv1.X, v.uv1.Y, 0));
                textureCoords2.Add(new Vector3D(v.uv2.X, v.uv2.Y, 0));
                vertexColors.Add(new Color4D(v.col.X, v.col.Y, v.col.Z, v.col.W));
                mesh.TextureCoordinateChannels[0] = textureCoords0;
                mesh.TextureCoordinateChannels[1] = textureCoords1;
                mesh.TextureCoordinateChannels[2] = textureCoords2;
                mesh.VertexColorChannels[0] = vertexColors;
            }
            for (int f = 0; f < faces.Count; f++)
            {
                mesh.Faces.Add(new Face(new int[] { faces[f++], faces[f++], faces[f] }));
            }
            mesh.MaterialIndex = 0;

            mesh.TextureCoordinateChannels.SetValue(textureCoords0, 0);
            scene.Meshes.Add(mesh);

            Material material = new Material();
            material.Name = "NewMaterial";
            scene.Materials.Add(material);

            using (var v = new AssimpContext())
            {
                v.ExportFile(scene, FileName, "obj");
            }
        }
        public List<int> GetFaces(Mesh msh)
        {
            List<int> faces = new List<int>();

            if (msh.HasFaces)
            {
                foreach (Face f in msh.Faces)
                {
                    if (f.HasIndices)
                    {
                        foreach (int indx in f.Indices)
                            faces.Add(indx);
                    }
                }
            }

            return faces;
        }
        public List<string> GetBoneList(Mesh msh)
        {
            List<string> bones = new List<string>();
            foreach (Bone b in msh.Bones)
            {
                if (!bones.Contains(b.Name))
                    bones.Add(b.Name);
            }
            return bones;
        }
        public int GetVertexSkinCount(Mesh msh)
        {

            List<int> indciesTotal = new List<int>();

            var blendIndexes = new List<List<int>>();
            var blendWeights = new List<List<float>>();

            int i;
            for (i = 0; i < msh.VertexCount; i++)
            {
                blendIndexes.Add(new List<int>());
                blendWeights.Add(new List<float>());
            }

            foreach (var bone in msh.Bones)
            {
                var bi = msh.Bones.IndexOf(bone);
                foreach (var vw in bone.VertexWeights)
                {
                    blendIndexes[vw.VertexID].Add(bi);
                    blendWeights[vw.VertexID].Add(vw.Weight);
                }
            }

            foreach (Bone b in msh.Bones)
                Console.WriteLine(b.VertexWeights.Count);

            if (msh.HasBones)
                return msh.Bones.Max(b => b.VertexWeightCount);

            return 0;
        }
        public List<Vertex> GetVertices(Mesh msh, Matrix4 transform, STGenericObject STobj)
        {

            List<Vertex> vertices = new List<Vertex>();
            for (int v = 0; v < msh.VertexCount; v++)
            {
                Vertex vert = new Vertex();

                if (msh.HasVertices)
                    vert.pos = Vector3.TransformPosition(FromVector(msh.Vertices[v]), transform);
                if (msh.HasNormals)
                    vert.nrm = Vector3.TransformNormal(FromVector(msh.Normals[v]), transform);
                if (msh.HasTextureCoords(0))
                    vert.uv0 = new Vector2(msh.TextureCoordinateChannels[0][v].X, msh.TextureCoordinateChannels[0][v].Y);
                if (msh.HasTextureCoords(1))
                    vert.uv1 = new Vector2(msh.TextureCoordinateChannels[1][v].X, msh.TextureCoordinateChannels[1][v].Y);
                if (msh.HasTextureCoords(2))
                    vert.uv2 = new Vector2(msh.TextureCoordinateChannels[2][v].X, msh.TextureCoordinateChannels[2][v].Y);
                if (msh.HasTangentBasis)
                    vert.tan = new Vector4(msh.Tangents[v].X, msh.Tangents[v].Y, msh.Tangents[v].Z, 1);
                if (msh.HasVertexColors(0))
                    vert.col = new Vector4(msh.VertexColorChannels[0][v].R, msh.VertexColorChannels[0][v].G, msh.VertexColorChannels[0][v].B, msh.VertexColorChannels[0][v].A);
                if (msh.HasTangentBasis)
                    vert.bitan = new Vector4(msh.BiTangents[v].X, msh.BiTangents[v].Y, msh.BiTangents[v].Z, 1);
                vertices.Add(vert);
            }
            if (msh.HasBones)
            {
                for (int i = 0; i < msh.BoneCount; i++)
                {
                    Bone bn  = msh.Bones[i];
                    if (bn.HasVertexWeights)
                    {
                        foreach (VertexWeight w in bn.VertexWeights)
                        {
                          //  vertices[w.VertexID].pos = Vector3.TransformPosition(vertices[w.VertexID].pos, AssimpHelper.TKMatrix(bn.OffsetMatrix));
                            vertices[w.VertexID].boneWeights.Add(w.Weight);
                            vertices[w.VertexID].boneNames.Add(bn.Name);
                        }
                    }
                }
            }


            return vertices;
        }
        private Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }
        public static OpenTK.Matrix4 TKMatrix2(Assimp.Matrix4x4 matOut)
        {
            var matIn = new OpenTK.Matrix4();

            matOut.A1 = matIn.M11;
            matOut.B1 = matIn.M12;
            matOut.C1 = matIn.M13;
            matOut.D1 = matIn.M14;

            //Y
            matOut.A2 = matIn.M21;
            matOut.B2 = matIn.M22;
            matOut.C2 = matIn.M23;
            matOut.D2 = matIn.M24;

            //Z
            matOut.A3 = matIn.M31;
            matOut.B3 = matIn.M32;
            matOut.C3 = matIn.M33;
            matOut.D3 = matIn.M34;

            //Translation
            matOut.A4 = matIn.M41;
            matOut.B4 = matIn.M42;
            matOut.C4 = matIn.M43;
            matOut.D4 = matIn.M44;

            return matIn;
        }

        private Matrix4 FromAssimpMatrix(Matrix4x4 mat)
        {
            Vector3D scaling;
            Vector3D tranlation;
            Assimp.Quaternion rot;
            mat.Decompose(out scaling, out rot, out tranlation);

            Console.WriteLine($"rotQ " + rot);

            Matrix4 positionMat = Matrix4.CreateTranslation(FromVector(tranlation));
            Matrix4 rotQ = Matrix4.CreateFromQuaternion(AssimpHelper.TKQuaternion(rot));
            Matrix4 scaleMat = Matrix4.CreateScale(FromVector(scaling));
            Matrix4 matrixFinal = scaleMat * rotQ * positionMat;

            return matrixFinal;
        }
    }
}

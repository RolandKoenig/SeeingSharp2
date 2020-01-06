using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    [SupportedFileFormat("obj", "Wavefront Object format (*.obj)")]
    [SupportedFileFormat("fbx", "(*.fbx)")]
    [SupportedFileFormat("3ds", "3D Studio Max (*.3ds)")]
    [SupportedFileFormat("dae", "Collada (*.dae)")]
    [SupportedFileFormat("c4d", "Cinema 4D (*.c4d)")]
    [SupportedFileFormat("ac", "AC3D format (*.ac)")]
    public class AssimpImporter : IModelImporter
    {
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            var modelContainer = new ImportedModelContainer(importOptions);

            using var assimpContext = new Assimp.AssimpContext();
            var scene = assimpContext.ImportFileFromStream(
                sourceFile.OpenInputStream(),
                Assimp.PostProcessPreset.TargetRealTimeFast);

            ProcessMaterials(modelContainer, scene);
            ProcessNode(modelContainer, scene, scene.RootNode, null);

            return modelContainer;
        }

        private static void ProcessMaterials(ImportedModelContainer modelContainer, Assimp.Scene scene)
        {
            var materialCount = scene.MaterialCount;
            for(var materialIndex=0; materialIndex < materialCount; materialIndex++)
            {
                var actMaterial = scene.Materials[materialIndex];

                modelContainer.ImportedResources.Add(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Material", materialIndex.ToString()),
                    (device) =>
                    {
                        var materialResource = new StandardMaterialResource();
                        if (actMaterial.HasColorDiffuse)
                        {
                            materialResource.UseVertexColors = false;
                            materialResource.MaterialDiffuseColor =
                                AssimpHelper.Color4FromAssimp(actMaterial.ColorDiffuse);
                        }
                        return materialResource;
                    }));
            }

        }

        private static void ProcessNode(ImportedModelContainer modelContainer, Assimp.Scene scene, Assimp.Node actNode, SceneObject? actParent)
        {
            SceneObject? nextParent = null;
            if (actNode.HasMeshes)
            {
                // Count vertices
                var fullVertexCount = 0;
                foreach (var actMeshID in actNode.MeshIndices)
                {
                    var actMesh = scene.Meshes[actMeshID];
                    fullVertexCount += actMesh.VertexCount;
                }

                // This one has true geometry
                var meshCount = actNode.MeshCount;
                var newGeometry = new Geometry(fullVertexCount);
                var materialKeys = new NamedOrGenericKey[meshCount];
                for (var meshIndex = 0; meshIndex < meshCount; meshIndex++)
                {
                    var actMeshID = actNode.MeshIndices[meshIndex]; 
                    var actBaseVertex = newGeometry.CountVertices;
                    var actMesh = scene.Meshes[actMeshID];

                    List<Assimp.Color4D>? vertexColors = null;
                    if (actMesh.HasVertexColors(0))
                    {
                        vertexColors = actMesh.VertexColorChannels[0];
                    }

                    List<Assimp.Vector3D>? textureCoords1 = null;
                    if (actMesh.TextureCoordinateChannelCount > 0)
                    {
                        textureCoords1 = actMesh.TextureCoordinateChannels[0];
                    }

                    // Create all vertices
                    var vertexCount = actMesh.VertexCount;
                    for (var actVertexID = 0; actVertexID < vertexCount; actVertexID++)
                    {
                        var vertexIndex = newGeometry.AddVertex();
                        ref var newVertex = ref newGeometry.GetVertexBasicRef(vertexIndex);

                        newVertex.Position = AssimpHelper.Vector3FromAssimp(actMesh.Vertices[actVertexID]);
                        if (actMesh.HasNormals)
                        {
                            newVertex.Normal = AssimpHelper.Vector3FromAssimp(actMesh.Normals[actVertexID]);
                        }
                        if (vertexColors != null)
                        {
                            newVertex.Color = AssimpHelper.Color4FromAssimp(vertexColors[actVertexID]);
                        }
                        if (textureCoords1 != null)
                        {
                            newVertex.TexCoord1 = AssimpHelper.Vector2FromAssimp(textureCoords1[actVertexID]);
                        }
                    }

                    // Create all faces
                    var newSurface = newGeometry.CreateSurface(actMesh.FaceCount * 3);
                    foreach (var actFace in actMesh.Faces)
                    {
                        if(actFace.IndexCount != 3){ continue; }

                        newSurface.AddTriangle(
                            actBaseVertex + actFace.Indices[0],
                            actBaseVertex + actFace.Indices[1],
                            actBaseVertex + actFace.Indices[2]);
                    }

                    materialKeys[meshIndex] = modelContainer.GetResourceKey("Material", meshIndex.ToString());
                }

                var geometryKey = modelContainer.GetResourceKey("Geometry", actNode.Name);
                modelContainer.ImportedResources.Add(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Geometry", actNode.Name),
                    (device)=> new GeometryResource(newGeometry)));

                var newMesh = new Mesh(geometryKey, materialKeys);
                newMesh.CustomTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));
                newMesh.TransformationType = SpacialTransformationType.CustomTransform;
                modelContainer.Objects.Add(newMesh);
                nextParent = newMesh;

                if (actParent != null)
                {
                    modelContainer.ParentChildRelationships.Add(Tuple.Create<SceneObject, SceneObject>(actParent, newMesh));
                }
            }
            else if(actNode.HasChildren)
            {
                // This one is just a pivot
                var actPivotObject = new ScenePivotObject();
                actPivotObject.CustomTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));
                actPivotObject.TransformationType = SpacialTransformationType.CustomTransform;
                modelContainer.Objects.Add(actPivotObject);
                nextParent = actPivotObject;

                if (actParent != null)
                {
                    modelContainer.ParentChildRelationships.Add(Tuple.Create<SceneObject, SceneObject>(actParent, actPivotObject));
                }
            }
            else
            {
                // Node should be something else, e. g. light, camera
            }

            // Process all children
            foreach (var actChildNode in actNode.Children)
            {
                ProcessNode(modelContainer, scene, actChildNode, nextParent);
            }
        }

        public ImportOptions CreateDefaultImportOptions()
        {
            return new AssimpImportOptions();
        }
    }
}

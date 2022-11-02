using System.Collections.Generic;
using Assimp;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Util;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Mesh = SeeingSharp.Drawing3D.Mesh;
using Scene = Assimp.Scene;

namespace SeeingSharp.AssimpImporter
{
    [SupportedFileFormat("obj", "Wavefront Object format (*.obj)")]
    [SupportedFileFormat("fbx", "(*.fbx)")]
    [SupportedFileFormat("3ds", "3D Studio Max (*.3ds)")]
    [SupportedFileFormat("dae", "Collada (*.dae)")]
    [SupportedFileFormat("gltf", "GL Transmission Format (*.gltf)")]
    [SupportedFileFormat("glb", "GL Transmission Format (*.glb)")]
    public class AssimpImporter : IModelImporter
    {
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            var modelContainer = new ImportedModelContainer(sourceFile, importOptions);

            // Load Assimp scene
            using var assimpContext = new AssimpContext();
            assimpContext.SetIOSystem(new AssimpIOSystem(sourceFile));
            var scene = assimpContext.ImportFile(
                sourceFile.FileNameWithExtension,
                PostProcessPreset.TargetRealTimeFast | PostProcessSteps.SplitLargeMeshes);

            // Load all materials
            ProcessMaterials(modelContainer, scene);

            ProcessTextures(sourceFile, modelContainer, scene);

            // Load all scene objects
            var boundBoxCalculator = new ObjectTreeBoundingBoxCalculator();
            ProcessNode(modelContainer, scene, scene.RootNode, null, boundBoxCalculator);

            // Finish loading
            modelContainer.FinishLoading(boundBoxCalculator.CreateBoundingBox());

            return modelContainer;
        }

        public ImportOptions CreateDefaultImportOptions()
        {
            return new AssimpImportOptions();
        }

        private static void ProcessMaterials(ImportedModelContainer modelContainer, Scene scene)
        {
            var materialCount = scene.MaterialCount;
            for(var materialIndex=0; materialIndex < materialCount; materialIndex++)
            {
                var actMaterial = scene.Materials[materialIndex];

                modelContainer.AddResource(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Material", materialIndex.ToString()),
                    _ =>
                    {
                        var textureKey = NamedOrGenericKey.Empty;
                        if (actMaterial.HasTextureDiffuse)
                        {
                            textureKey =
                                modelContainer.GetResourceKey("Texture", actMaterial.TextureDiffuse.FilePath);
                        }

                        var materialResource = new StandardMaterialResource(textureKey);
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

        private static void ProcessTextures(ResourceLink currentFileLink, ImportedModelContainer modelContainer, Scene scene)
        {
            var materialCount = scene.MaterialCount;
            for (var materialIndex = 0; materialIndex < materialCount; materialIndex++)
            {
                var actMaterial = scene.Materials[materialIndex];
                if(!actMaterial.HasTextureDiffuse){ continue; }
                if(string.IsNullOrEmpty(actMaterial.TextureDiffuse.FilePath)){ continue; }
                
                modelContainer.AddResource(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Texture", actMaterial.TextureDiffuse.FilePath),
                    _ => new StandardTextureResource(
                        currentFileLink.GetForAnotherFile(actMaterial.TextureDiffuse.FilePath))));
            }

            // var textureCount = scene.TextureCount;
            // for (var textureIndex = 0; textureIndex < textureCount; textureIndex++)
            // {
            //     var actTexture = scene.Textures[textureIndex];
            // }
        }

        private static void ProcessNode(
            ImportedModelContainer modelContainer,
            Scene scene, Node actNode, SceneObject? actParent, 
            ObjectTreeBoundingBoxCalculator boundingBoxCalc)
        {
            SceneObject? nextParent = null;
            if (actNode.HasMeshes)
            {
                var actTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));
                boundingBoxCalc.PushTransform(ref actTransform);

                // Count vertices
                var fullVertexCount = 0;
                foreach (var actMeshId in actNode.MeshIndices)
                {
                    var actMesh = scene.Meshes[actMeshId];
                    fullVertexCount += actMesh.VertexCount;
                }

                // This one has true geometry
                var meshCount = actNode.MeshCount;
                var newGeometry = new Geometry(fullVertexCount);
                var materialKeys = new NamedOrGenericKey[meshCount];
                for (var meshIndex = 0; meshIndex < meshCount; meshIndex++)
                {
                    var actMeshId = actNode.MeshIndices[meshIndex]; 
                    var actBaseVertex = newGeometry.CountVertices;
                    var actMesh = scene.Meshes[actMeshId];

                    List<Color4D>? vertexColors = null;
                    if (actMesh.HasVertexColors(0))
                    {
                        vertexColors = actMesh.VertexColorChannels[0];
                    }

                    List<Vector3D>? textureCoords1 = null;
                    if (actMesh.TextureCoordinateChannelCount > 0)
                    {
                        textureCoords1 = actMesh.TextureCoordinateChannels[0];
                    }

                    // Create all vertices
                    var vertexCount = actMesh.VertexCount;
                    for (var actVertexId = 0; actVertexId < vertexCount; actVertexId++)
                    {
                        var vertexIndex = newGeometry.AddVertex();
                        ref var newVertex = ref newGeometry.GetVertexBasicRef(vertexIndex);

                        newVertex.Position = AssimpHelper.Vector3FromAssimp(actMesh.Vertices[actVertexId]);
                        if (actMesh.HasNormals)
                        {
                            newVertex.Normal = AssimpHelper.Vector3FromAssimp(actMesh.Normals[actVertexId]);
                        }
                        if (vertexColors != null)
                        {
                            newVertex.Color = AssimpHelper.Color4FromAssimp(vertexColors[actVertexId]);
                        }
                        if (textureCoords1 != null)
                        {
                            newVertex.TexCoord1 = AssimpHelper.TextureCoord2FromAssimp(textureCoords1[actVertexId]);
                        }

                        boundingBoxCalc.AddCoordinate(ref newVertex.Position);
                    }

                    // Create all faces
                    var newSurface = newGeometry.CreateSurface(actMesh.FaceCount * 3);
                    foreach (var actFace in actMesh.Faces)
                    {
                        if (actFace.IndexCount != 3){ continue; }

                        newSurface.AddTriangle(
                            actBaseVertex + actFace.Indices[0],
                            actBaseVertex + actFace.Indices[1],
                            actBaseVertex + actFace.Indices[2]);
                    }

                    materialKeys[meshIndex] = modelContainer.GetResourceKey("Material", actMesh.MaterialIndex.ToString());
                }

                var geometryKey = modelContainer.GetResourceKey("Geometry", actNode.Name);
                modelContainer.AddResource(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Geometry", actNode.Name),
                    _=> new GeometryResource(newGeometry)));

                var newMesh = new Mesh(geometryKey, materialKeys);
                newMesh.CustomTransform = actTransform;
                newMesh.TransformationType = SpacialTransformationType.CustomTransform;
                newMesh.Name = actNode.Name;
                modelContainer.AddObject(newMesh);
                nextParent = newMesh;

                if (actParent != null)
                {
                    modelContainer.AddParentChildRelationship(new ParentChildRelationship(actParent, newMesh));
                }
            }
            else if(actNode.HasChildren)
            {
                var actTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));

                // Do only process this pivot node if it changes behavior on the model
                if ((actTransform != Matrix4x4.Identity) ||    
                    (actParent != null))
                {
                    boundingBoxCalc.PushTransform(ref actTransform);

                    // This one is just a pivot
                    var actPivotObject = new ScenePivotObject();
                    actPivotObject.CustomTransform = actTransform;
                    actPivotObject.TransformationType = SpacialTransformationType.CustomTransform;
                    modelContainer.AddObject(actPivotObject);
                    nextParent = actPivotObject;

                    if (actParent != null)
                    {
                        modelContainer.AddParentChildRelationship(new ParentChildRelationship(actParent, actPivotObject));
                    }
                }
            }

            // Process all children
            foreach (var actChildNode in actNode.Children)
            {
                ProcessNode(modelContainer, scene, actChildNode, nextParent, boundingBoxCalc);
            }

            boundingBoxCalc.PopTransform();
        }
    }
}

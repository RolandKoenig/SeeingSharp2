/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.ModelViewer
{
    [SupportedFileFormat("obj", "Wavefront Object format (*.obj)")]
    [SupportedFileFormat("fbx", "(*.fbx)")]
    [SupportedFileFormat("3ds", "3D Studio Max (*.3ds)")]
    [SupportedFileFormat("dae", "Collada (*.dae)")]
    public class AssimpImporter : IModelImporter
    {
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            var modelContainer = new ImportedModelContainer(importOptions);

            // Load Assimp scene
            using var assimpContext = new Assimp.AssimpContext();
            assimpContext.SetIOSystem(new AssimpIOSystem(sourceFile));
            var scene = assimpContext.ImportFile(
                sourceFile.FileNameWithExtension,
                Assimp.PostProcessPreset.TargetRealTimeFast | Assimp.PostProcessSteps.SplitLargeMeshes);

            // Load all materials
            ProcessMaterials(modelContainer, scene);

            // Load all scene objects
            var sceneRoot = modelContainer.CreateAndAddRootObject();
            var boundBoxCalculator = new ObjectTreeBoundingBoxCalculator();
            ProcessNode(modelContainer, scene, scene.RootNode, sceneRoot, boundBoxCalculator);

            // Configure object scaling
            var boundingBox = boundBoxCalculator.CreateBoundingBox();
            var scaleFactor = Math.Min(
                (1f / boundingBox.Width),
                Math.Min((1f / boundingBox.Height), (1f / boundingBox.Depth)));
            sceneRoot.Scaling *= scaleFactor;

            // Configuration object position(bottom middle of the scene)
            boundingBox.Minimum *= scaleFactor;
            boundingBox.Maximum *= scaleFactor;
            sceneRoot.Position = new Vector3(
                (0f - (boundingBox.Minimum.X + (boundingBox.Maximum.X - boundingBox.Minimum.X) / 2f)),
                (0f - (boundingBox.Minimum.Y + (boundingBox.Maximum.Y - boundingBox.Minimum.Y) / 2f)),
                (0f - (boundingBox.Minimum.Z + (boundingBox.Maximum.Z - boundingBox.Minimum.Z) / 2f)));

            boundingBox.Transform(Matrix4x4.CreateTranslation(sceneRoot.Position));
            var boundingBoxObject = new WireObject(Color4.RedColor, boundingBox);
            modelContainer.Objects.Add(boundingBoxObject);

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

        private static void ProcessNode(
            ImportedModelContainer modelContainer,
            Assimp.Scene scene, Assimp.Node actNode, SceneObject? actParent, 
            ObjectTreeBoundingBoxCalculator boundingBoxCalc)
        {
            SceneObject? nextParent = null;
            if (actNode.HasMeshes)
            {
                var actTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));
                boundingBoxCalc.PushTransform(ref actTransform);

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
                modelContainer.ImportedResources.Add(new ImportedResourceInfo(
                    modelContainer.GetResourceKey("Geometry", actNode.Name),
                    (device)=> new GeometryResource(newGeometry)));

                var newMesh = new Mesh(geometryKey, materialKeys);
                newMesh.CustomTransform = actTransform;
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
                var actTransform = Matrix4x4.Transpose(AssimpHelper.MatrixFromAssimp(actNode.Transform));
                boundingBoxCalc.PushTransform(ref actTransform);

                // This one is just a pivot
                var actPivotObject = new ScenePivotObject();
                actPivotObject.CustomTransform = actTransform;
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
                ProcessNode(modelContainer, scene, actChildNode, nextParent, boundingBoxCalc);
            }

            boundingBoxCalc.PopTransform();
        }

        public ImportOptions CreateDefaultImportOptions()
        {
            return new AssimpImportOptions();
        }
    }
}

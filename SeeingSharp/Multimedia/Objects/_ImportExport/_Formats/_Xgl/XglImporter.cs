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
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Xml;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    // Hirarchy in XGL file:
    //
    //  WORLD
    //   - Data
    //   - Background (BACKGROUND)
    //     - Backcolor (BACKCOLOR)
    //   - Lights
    //   - Textures (TEXTURE)
    //     - Modulate (MODULATE)
    //     - Repeat (REPEAT)
    //     - Colors (TEXTURERGBA, TEXTURERGB)
    //   - Meshes (MESH)
    //     - IsBothSide? (SURFACE)
    //     - Points (P)
    //     - Normals (N)
    //     - Materials (MAT)
    //       - Diffuse (DIFF)
    //       - Ambient (AMB)
    //       - Emissive (EMISS)
    //       - Specular (SPEC)
    //       - Alpha (ALPHA)
    //       - Shininess (SHINE)
    //     - Patches (PATCH)
    //       - Faces (F)
    //         - Vertices (FV1, FV2, FV3)
    //         - MaterialRef (MATREF)
    //         - TextureRef (TEXTUREREF)
    //   - Objects (OBJECT)
    //     - Transform (TRANSFORM)
    //       - Forward
    //       - Up
    //       - Position
    //     - MeshReference (MESHREF)

    /// <summary>
    /// An importer which loads files in xgl format.
    /// See http://web.archive.org/web/20060218030828/http://www.xglspec.org/
    /// </summary>
    [SupportedFileFormat("xgl", "SGI OpenGL render library format")]
    [SupportedFileFormat("zgl", "SGI OpenGL render library format (compressed)")]
    public class XglImporter : IModelImporter
    {
        // Internal resource names
        private const string RES_CLASS_TEXTURE = "Tex";
        private const string RES_CLASS_MATERIAL = "Mat";
        private const string RES_CLASS_MESH = "Mesh";

        // All common nodes
        private const string NODE_NAME_DATA = "DATA";
        private const string NODE_NAME_BACKGROUND = "BACKGROUND";
        private const string NODE_NAME_LIGHTING = "LIGHTING";
        private const string NODE_NAME_TEXTURE = "TEXTURE";
        private const string NODE_NAME_MESH = "MESH";
        private const string NODE_NAME_OBJECT = "OBJECT";

        // All nodes containing texture information
        private const string NODE_NAME_TEXTURE_RGBA = "TEXTURERGBA";
        private const string NODE_NAME_TEXTURE_RGB = "TEXTURERGB";
        private const string NODE_NAME_TEXTURE_MODULATE = "MODULATE";
        private const string NODE_NAME_TEXTURE_REPEAT = "REPEAT";

        // All nodes containing material information
        private const string NODE_NAME_MAT = "MAT";
        private const string NODE_NAME_MAT_DIFFUSE = "DIFF";
        private const string NODE_NAME_MAT_AMB = "AMB";
        private const string NODE_NAME_MAT_EMISS = "EMISS";
        private const string NODE_NAME_MAT_SPEC = "SPEC";
        private const string NODE_NAME_MAT_ALPHA = "ALPHA";
        private const string NODE_NAME_MAT_SHINE = "SHINE";

        // All nodes containing mesh information
        private const string NODE_NAME_NORMAL = "N";
        private const string NODE_NAME_POINT = "P";
        private const string NODE_NAME_FACE = "F";
        private const string NODE_NAME_FACE_VERTEX_1 = "FV1";
        private const string NODE_NAME_FACE_VERTEX_2 = "FV2";
        private const string NODE_NAME_FACE_VERTEX_3 = "FV3";
        private const string NODE_NAME_FACE_MATERIAL_REF = "MATREF";
        private const string NODE_NAME_FACE_TEXTUREREF_REF = "TEXTUREREF";
        private const string NODE_NAME_FACE_POINT_REF = "PREF";
        private const string NODE_NAME_FACE_NORMAL_REF = "NREF";
        private const string NODE_NAME_PATCH = "PATCH";
        private const string NODE_NAME_TC = "TC";
        private const string NODE_NAME_SURFACE = "SURFACE";

        // All nodes containing object information
        private const string NODE_NAME_MESHREF = "MESHREF";
        private const string NODE_NAME_TRANS_FORWARD = "FORWARD";
        private const string NODE_NAME_TRANSFORM = "TRANSFORM";
        private const string NODE_NAME_TRANS_UP = "UP";
        private const string NODE_NAME_TRANS_POSITION = "POSITION";

        /// <summary>
        /// Imports the texture node where the xml reader is currently located.
        /// </summary>
        /// <param name="inStreamXml">The xml reader object.</param>
        /// <param name="container">The container where to import to.</param>
        private void ImportTexture(XmlReader inStreamXml, ImportedModelContainer container)
        {
            var id = inStreamXml.GetAttribute("ID");
            var width = 0;
            var height = 0;
            MemoryMappedTexture32bpp inMemoryTexture = null;

            while (inStreamXml.Read())
            {
                // Ending condition
                if (inStreamXml.NodeType == XmlNodeType.EndElement &&
                   inStreamXml.Name == NODE_NAME_TEXTURE)
                {
                    break;
                }

                // Continue condition
                if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                // Read all texture data

                switch (inStreamXml.Name)
                {
                    case NODE_NAME_TEXTURE_RGBA:
                        width = int.Parse(inStreamXml.GetAttribute("WIDTH"));
                        height = int.Parse(inStreamXml.GetAttribute("HEIGHT"));
                        unsafe
                        {
                            inMemoryTexture = new MemoryMappedTexture32bpp(new Size2(width, height));
                            var inMemoryTextureP = (byte*)inMemoryTexture.Pointer.ToPointer();

                            var fullSize = width * height * 4;
                            var sourceBuffer = new byte[fullSize];
                            inStreamXml.Read();
                            inStreamXml.ReadContentAsBinHex(sourceBuffer, 0, fullSize);
                            for (var loop = 0; loop < fullSize; loop += 4)
                            {
                                // Target format is BGRA (default one in Seeing#)

                                inMemoryTextureP[loop + 2] = sourceBuffer[loop];
                                inMemoryTextureP[loop + 1] = sourceBuffer[loop + 1];
                                inMemoryTextureP[loop + 0] = sourceBuffer[loop + 2];
                                inMemoryTextureP[loop + 3] = sourceBuffer[loop + 3];
                            }
                        }
                        break;

                    case NODE_NAME_TEXTURE_RGB:
                        width = int.Parse(inStreamXml.GetAttribute("WIDTH"));
                        height = int.Parse(inStreamXml.GetAttribute("HEIGHT"));
                        unsafe
                        {
                            inMemoryTexture = new MemoryMappedTexture32bpp(new Size2(width, height));
                            var inMemoryTextureP = (byte*)inMemoryTexture.Pointer.ToPointer();

                            var fullSize = width * height * 3;
                            var loopTarget = 0;
                            var sourceBuffer = new byte[fullSize];
                            inStreamXml.Read();
                            inStreamXml.ReadContentAsBinHex(sourceBuffer, 0, fullSize);
                            for (var loop = 0; loop < fullSize; loop += 3)
                            {
                                // Target format is BGRA (default one in Seeing#)
                                inMemoryTextureP[loopTarget + 2] = sourceBuffer[loop];
                                inMemoryTextureP[loopTarget + 1] = sourceBuffer[loop + 1];
                                inMemoryTextureP[loopTarget + 0] = sourceBuffer[loop + 2];
                                inMemoryTextureP[loopTarget + 3] = 255;
                                loopTarget += 4;
                            }
                        }
                        break;

                    case NODE_NAME_TEXTURE_MODULATE:
                        break;

                    case NODE_NAME_TEXTURE_REPEAT:
                        break;
                }
            }

            // Check values
            if (width < 0) { throw new SeeingSharpGraphicsException("Unable to read with of a texture!"); }
            if (height < 0) { throw new SeeingSharpGraphicsException("Unable to read height of a texture!"); }
            if (inMemoryTexture == null) { throw new SeeingSharpGraphicsException("Unable to read the contents of a texture!"); }

            // Add the imported texture
            var resKeyTexture = container.GetResourceKey(RES_CLASS_TEXTURE, id);
            container.ImportedResources.Add(new ImportedResourceInfo(
                resKeyTexture,
                () => new StandardTextureResource(inMemoryTexture)));
            container.ImportedResources.Add(new ImportedResourceInfo(
                container.GetResourceKey(RES_CLASS_MATERIAL, id),
                () => new SimpleColoredMaterialResource(resKeyTexture)));
        }

        /// <summary>
        /// Imports the texture node where the xml reader is currently located.
        /// </summary>
        /// <param name="inStreamXml">The xml reader object.</param>
        /// <param name="container">The container where to import to.</param>
        /// <param name="xglImportOptions">Current import options.</param>
        private void ImportMesh(XmlReader inStreamXml, ImportedModelContainer container, XglImportOptions xglImportOptions)
        {
            var id = inStreamXml.GetAttribute("ID");
            var actVertexStructure = new VertexStructure();
            var minVertexID = int.MaxValue;
            var actVertexIndex = -1;
            var actNormalIndex = -1;
            var actTextureIndex = -1;
            var actTempVertex = Vertex.Empty;
            var actFaceReferences = new int[3];
            var localMaterialInfos = new Dictionary<int, MaterialProperties>();

            while (inStreamXml.Read())
            {
                // Ending condition
                if (inStreamXml.NodeType == XmlNodeType.EndElement &&
                    inStreamXml.Name == NODE_NAME_MESH)
                {
                    break;
                }

                // Continue condition
                if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                switch (inStreamXml.Name)
                {
                    case NODE_NAME_SURFACE:
                        // If this tag is present, faces will be visible from both sides. If this flag is absent, faces will be visible from only one side as described in the <F> tag
                        break;

                    case NODE_NAME_MAT:
                        // Read the next material
                        var materialAndID = ImportMaterial(inStreamXml, container, xglImportOptions);
                        localMaterialInfos[materialAndID.Item1] = materialAndID.Item2;
                        break;

                    case NODE_NAME_PATCH:
                        // A patch is a group of faces
                        // We don't need to handle this one
                        break;

                    case NODE_NAME_POINT:
                        // Read next point
                        if (minVertexID == int.MaxValue) { minVertexID = int.Parse(inStreamXml.GetAttribute("ID")); }
                        actVertexIndex++;
                        actTempVertex = actVertexStructure.EnsureVertexAt(actVertexIndex);
                        actTempVertex.Color = Color4.White;
                        inStreamXml.Read();
                        actTempVertex.Position = inStreamXml.ReadContentAsVector3() * xglImportOptions.ResizeFactor;
                        actVertexStructure.Vertices[actVertexIndex] = actTempVertex;
                        break;

                    case NODE_NAME_NORMAL:
                        // Read next normal
                        if (minVertexID == int.MaxValue) { minVertexID = int.Parse(inStreamXml.GetAttribute("ID")); }
                        actNormalIndex++;
                        actTempVertex = actVertexStructure.EnsureVertexAt(actNormalIndex);
                        inStreamXml.Read();
                        actTempVertex.Normal = inStreamXml.ReadContentAsVector3();
                        actVertexStructure.Vertices[actNormalIndex] = actTempVertex;
                        break;

                    case NODE_NAME_TC:
                        // Read next texture coordinate
                        if (minVertexID == int.MaxValue) { minVertexID = int.Parse(inStreamXml.GetAttribute("ID")); }
                        actTextureIndex++;
                        actTempVertex = actVertexStructure.EnsureVertexAt(actTextureIndex);
                        inStreamXml.Read();
                        actTempVertex.TexCoord = inStreamXml.ReadContentAsVector2();
                        actVertexStructure.Vertices[actTextureIndex] = actTempVertex;
                        break;

                    case NODE_NAME_FACE:
                        // Read next face
                        actFaceReferences[0] = 0;
                        actFaceReferences[1] = 0;
                        actFaceReferences[2] = 0;
                        var loopFacePoint = 0;
                        var referencedMat = -1;
                        var referencedTexture = -1;
                        while (inStreamXml.Read())
                        {
                            // Ending condition
                            if (inStreamXml.NodeType == XmlNodeType.EndElement &&
                                inStreamXml.Name == NODE_NAME_FACE)
                            {
                                break;
                            }
                            if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                            // Read next face index
                            if (inStreamXml.Name == NODE_NAME_FACE_MATERIAL_REF)
                            {
                                inStreamXml.Read();
                                referencedMat = inStreamXml.ReadContentAsInt();
                            }
                            else if (inStreamXml.Name == NODE_NAME_FACE_TEXTUREREF_REF)
                            {
                                inStreamXml.Read();
                                referencedTexture = inStreamXml.ReadContentAsInt();
                            }
                            else if (inStreamXml.Name == NODE_NAME_FACE_POINT_REF)
                            {
                                if (loopFacePoint >= 3) { throw new SeeingSharpGraphicsException("Invalid face index count!"); }
                                inStreamXml.Read();
                                actFaceReferences[loopFacePoint] = inStreamXml.ReadContentAsInt() - minVertexID;
                                loopFacePoint++;
                            }
                        }

                        // Get the correct material
                        var referencedMatObject = MaterialProperties.Empty;

                        if(referencedMat > -1)
                        {
                            localMaterialInfos.TryGetValue(referencedMat, out referencedMatObject);
                        }

                        if(referencedTexture > -1)
                        {
                            referencedMatObject = referencedMatObject.Clone();
                            referencedMatObject.TextureKey = container.GetResourceKey(RES_CLASS_TEXTURE, referencedTexture.ToString());
                        }

                        //for (int actFaceLoc = 0; actFaceLoc < 3; actFaceLoc++)
                        //{
                        //    int actVertexIndexInner = actFaceReferences[actFaceLoc];
                        //    actTempVertex = actVertexStructure.Vertices[actVertexIndexInner];
                        //    actTempVertex.Color = referencedMatObject.DiffuseColor;
                        //    actVertexStructure.Vertices[actVertexIndexInner] = actTempVertex;
                        //}

                        // Add the triangle
                        if (loopFacePoint != 3) { throw new SeeingSharpGraphicsException("Invalid face index count!"); }
                        actVertexStructure
                            .CreateOrGetExistingSurface(referencedMatObject)
                            .AddTriangle(actFaceReferences[0], actFaceReferences[1], actFaceReferences[2]);
                        break;
                }
            }

            // Add the geometry resource
            container.ImportedResources.Add(new ImportedResourceInfo(
                container.GetResourceKey(RES_CLASS_MESH, id),
                () => new GeometryResource(actVertexStructure)));
        }

        /// <summary>
        /// Imports the mode node where the xml reader is currently located.
        /// </summary>
        /// <param name="inStreamXml">The xml reader object.</param>
        /// <param name="container">The container where to import to.</param>
        /// <param name="xglImportOptions">Current import options.</param>
        private Tuple<int, MaterialProperties> ImportMaterial(XmlReader inStreamXml, ImportedModelContainer container, XglImportOptions xglImportOptions)
        {
            var resultID = int.Parse(inStreamXml.GetAttribute("ID"));
            var result = new MaterialProperties();

            while (inStreamXml.Read())
            {
                // Ending condition
                if (inStreamXml.NodeType == XmlNodeType.EndElement &&
                    inStreamXml.Name == NODE_NAME_MAT)
                {
                    break;
                }

                // Continue condition
                if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                switch (inStreamXml.Name)
                {
                    case NODE_NAME_MAT_DIFFUSE:
                        if (result == null) { continue; }
                        inStreamXml.Read();
                        result.DiffuseColor = new Color4(inStreamXml.ReadContentAsVector3(), 1f);
                        break;

                    case NODE_NAME_MAT_AMB:
                        if (result == null) { continue; }
                        inStreamXml.Read();
                        result.AmbientColor = new Color4(inStreamXml.ReadContentAsVector3(), 1f);
                        break;

                    case NODE_NAME_MAT_EMISS:
                        if (result == null) { continue; }
                        inStreamXml.Read();
                        result.EmissiveColor = new Color4(inStreamXml.ReadContentAsVector3(), 1f);
                        break;

                    case NODE_NAME_MAT_SPEC:
                        if (result == null) { continue; }
                        inStreamXml.Read();
                        result.SpecularColor = new Color4(inStreamXml.ReadContentAsVector3(), 1f);
                        break;

                    case NODE_NAME_MAT_ALPHA:
                        break;

                    case NODE_NAME_MAT_SHINE:
                        if (result == null) { continue; }
                        inStreamXml.Read();
                        result.Shininess = inStreamXml.ReadContentAsFloat();
                        break;
                }
            }

            return Tuple.Create(resultID, result);
        }

        /// <summary>
        /// Imports the texture node where the xml reader is currently located.
        /// </summary>
        private void ImportObject(XmlReader inStreamXml, ImportedModelContainer container, SceneObject parentObject, XglImportOptions xglImportOptions)
        {
            var upVector = Vector3.UnitY;
            var forwardVector = Vector3.UnitZ;
            var positionVector = Vector3.UnitX;
            var meshID = string.Empty;

            // Define a action which finally create the new object
            //  (may be called on two locations here.. so this action was defined
            SceneSpacialObject newObject = null;

            Action actionFinalizeObject = () =>
            {
                if (newObject != null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(meshID))
                {
                    // Generate a Pivot on which other objects are orientated
                    newObject = new ScenePivotObject();
                }
                else
                {
                    // Generate an instance of a 3d mesh
                    newObject = new GenericObject(
                        container.GetResourceKey(RES_CLASS_MESH, meshID));
                }

                newObject.Position = positionVector;
                newObject.TransformationType = SpacialTransformationType.TranslationDirection;
                newObject.RotationForward = forwardVector;
                newObject.RotationUp = upVector;
                container.Objects.Add(newObject);

                // Add dependency (parent => child)
                if (parentObject != null)
                {
                    container.ParentChildRelationships.Add(Tuple.Create<SceneObject, SceneObject>(
                        parentObject, newObject));
                }
            };

            // Read xml contents
            while (inStreamXml.Read())
            {
                // Ending condition
                if (inStreamXml.NodeType == XmlNodeType.EndElement &&
                   inStreamXml.Name == NODE_NAME_OBJECT)
                {
                    break;
                }

                // Continue condition
                if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                switch (inStreamXml.Name)
                {
                    case NODE_NAME_OBJECT:
                        actionFinalizeObject();
                        ImportObject(inStreamXml, container, newObject, xglImportOptions);
                        break;

                    case NODE_NAME_TRANS_FORWARD:
                        inStreamXml.Read();
                        forwardVector = inStreamXml.ReadContentAsVector3();
                        break;

                    case NODE_NAME_TRANS_UP:
                        inStreamXml.Read();
                        upVector = inStreamXml.ReadContentAsVector3();
                        break;

                    case NODE_NAME_TRANS_POSITION:
                        inStreamXml.Read();
                        positionVector = inStreamXml.ReadContentAsVector3() * xglImportOptions.ResizeFactor;
                        break;

                    case NODE_NAME_MESHREF:
                        inStreamXml.Read();
                        meshID = inStreamXml.ReadContentAsString();
                        break;

                    case NODE_NAME_TRANSFORM:
                        break;
                }
            }

            // finalize object here
            actionFinalizeObject();
        }

        /// <summary>
        /// Imports a model from the given file.
        /// </summary>
        /// <param name="sourceFile">The source file to be loaded.</param>
        /// <param name="importOptions">Some configuration for the importer.</param>
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            // Get import options
            var xglImportOptions = importOptions as XglImportOptions;

            if (xglImportOptions == null)
            {
                throw new SeeingSharpException("Invalid import options for ACImporter!");
            }

            var result = new ImportedModelContainer(importOptions);

            // Append an object which transform the whole coordinate system
            var rootObject = result.CreateAndAddRootObject();

            // Load current model by walking through xml nodes
            using (var inStream = sourceFile.OpenInputStream())
            {
                // Handle compressed xgl files (extension zgl)
                var nextStream = inStream;

                if (sourceFile.FileExtension.Equals("zgl", StringComparison.OrdinalIgnoreCase))
                {
                    // Skip the first bytes in case of compression
                    //  see https://github.com/assimp/assimp/blob/master/code/XGLLoader.cpp
                    inStream.ReadByte();
                    inStream.ReadByte();
                    nextStream = new DeflateStream(inStream, CompressionMode.Decompress);
                }

                // Read all xml data
                try
                {
                    using (var inStreamXml = XmlReader.Create(nextStream, new XmlReaderSettings { CloseInput = false }))
                    {
                        while (inStreamXml.Read())
                        {
                            try
                            {
                                if (inStreamXml.NodeType != XmlNodeType.Element) { continue; }

                                switch (inStreamXml.Name)
                                {
                                    case NODE_NAME_DATA:
                                        break;

                                    case NODE_NAME_BACKGROUND:
                                        break;

                                    case NODE_NAME_LIGHTING:
                                        break;

                                    case NODE_NAME_TEXTURE:
                                        ImportTexture(inStreamXml, result);
                                        break;

                                    case NODE_NAME_MESH:
                                        ImportMesh(inStreamXml, result, xglImportOptions);
                                        break;

                                    case NODE_NAME_OBJECT:
                                        ImportObject(inStreamXml, result, rootObject, xglImportOptions);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                // Get current line and column
                                var currentLine = 0;
                                var currentColumn = 0;
                                var lineInfo = inStreamXml as IXmlLineInfo;

                                if (lineInfo != null)
                                {
                                    currentLine = lineInfo.LineNumber;
                                    currentColumn = lineInfo.LinePosition;
                                }

                                // Throw an exception with more detail where the error was raised originally
                                throw new SeeingSharpGraphicsException(
                                    $"Unable to read file {sourceFile} because of an error while reading xml at {currentLine + ", " + currentColumn}", ex);
                            }
                        }
                    }
                }
                finally
                {
                    if (inStream != nextStream) { nextStream.Dispose(); }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a default import options object for this importer.
        /// </summary>
        public ImportOptions CreateDefaultImportOptions()
        {
            var options = new XglImportOptions
            {
                ResizeFactor = 0.01f,
                ResourceCoordinateSystem = CoordinateSystem.RightHanded_UpZ
            };

            return options;
        }
    }
}
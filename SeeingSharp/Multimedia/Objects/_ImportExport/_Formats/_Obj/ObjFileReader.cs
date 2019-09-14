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
using System.Globalization;
using System.IO;
using System.Text;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    internal class ObjFileReader
    {
        // Constants
        private const string SUBDIRECTORY_TEXTURES = "Texture";
        private static readonly char[] ARGUMENT_SPLITTER = { ' ' };
        private static readonly CultureInfo FILE_CULTURE = new CultureInfo("en-US");
        private static readonly NumberFormatInfo FILE_NUMBER_FORMAT = FILE_CULTURE.NumberFormat;

        // Parameters
        private ResourceLink m_resource;
        private ImportedModelContainer m_targetContainer;
        private ObjImportOptions m_importOptions;

        // Current object
        private GeometrySurface m_currentSurface;
        private GeometrySurface m_currentMaterialDefinition;

        // Raw data
        private List<Vector3> m_rawVertices;
        private List<Vector3> m_rawNormals;
        private List<Vector2> m_rawTextureCoordinates;

        // Common objects (to prevent high load on garbage collector)
        private float[] m_dummyFloatArguments_3 = new float[3];
        private int[] m_dummyIntArguments_3 = new int[3];
        private FaceIndices[] m_dummyFaceIndices_3 = new FaceIndices[3];
        private FaceIndices[] m_dummyFaceIndices_4 = new FaceIndices[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjFileReader"/> class.
        /// </summary>
        /// <param name="resource">The resource to be loaded.</param>
        /// <param name="targetContainer">The target ModelContainer into which to put the generated objects and resources.</param>
        /// <param name="importOptions">Current import settings.</param>
        public ObjFileReader(ResourceLink resource, ImportedModelContainer targetContainer, ObjImportOptions importOptions)
        {
            resource.EnsureNotNull(nameof(resource));
            targetContainer.EnsureNotNull(nameof(targetContainer));
            importOptions.EnsureNotNull(nameof(importOptions));

            m_resource = resource;
            m_targetContainer = targetContainer;
            m_importOptions = importOptions;

            m_rawVertices = new List<Vector3>(1024);
            m_rawNormals = new List<Vector3>(1014);
            m_rawTextureCoordinates = new List<Vector2>(1024);
            this.TargetGeometry = new Geometry();

            // Apply transform matrix in case of a different coordinate system (switched coordinate axes)
            var coordSpaceTransformMatrix = importOptions.GetTransformMatrixForCoordinateSystem();

            if(coordSpaceTransformMatrix != Matrix.Identity)
            {
                this.TargetGeometry.EnableBuildTimeTransform(coordSpaceTransformMatrix);
            }
        }

        /// <summary>
        /// Creates all objects and puts them into the given ModelContainer.
        /// </summary>
        public void GenerateObjects()
        {
            // For some files we have a separate subfolder for textures
            var textureSubfolderPath =
                string.IsNullOrEmpty(m_importOptions.TextureSubfolderName) ? new string[0] : new string[1] { m_importOptions.TextureSubfolderName };

            // Toggle triangle order, when configured
            if(m_importOptions.ToggleTriangleIndexOrder)
            {
                this.TargetGeometry.ToggleTriangleIndexOrder();
            }

            // Define all material resources which where defined in the material file
            var resMaterials = new NamedOrGenericKey[this.TargetGeometry.CountSurfaces];
            var actSurfaceIndex = 0;
            foreach (var actSurface in this.TargetGeometry.Surfaces)
            {
                // Handle texture
                var textureKey = NamedOrGenericKey.Empty;

                if (!string.IsNullOrEmpty(actSurface.TextureName))
                {
                    var actTextureName = actSurface.TextureName;
                    textureKey = m_targetContainer.GetResourceKey("Texture", actSurface.TextureName);
                    m_targetContainer.ImportedResources.Add(
                        new ImportedResourceInfo(
                            textureKey,
                            device => new StandardTextureResource(m_resource.GetForAnotherFile(actTextureName, textureSubfolderPath))));
                }
                actSurface.TextureKey = textureKey;

                // Handle material itself
                var actMaterialKey = m_targetContainer.GetResourceKey("Material", actSurface.CommonMaterialProperties.Name);
                actSurface.Material = actMaterialKey;
                m_targetContainer.ImportedResources.Add(new ImportedResourceInfo(
                    actMaterialKey,
                    device => new SimpleColoredMaterialResource(textureKey)
                    {
                        ClipFactor = textureKey != NamedOrGenericKey.Empty ? 0.1f : 0f,
                        MaterialDiffuseColor = actSurface.DiffuseColor
                    }));

                resMaterials[actSurfaceIndex] = actMaterialKey;
                actSurfaceIndex++;
            }

            // Define geometry resource
            var resGeometry = m_targetContainer.GetResourceKey("Geometry", "1");
            var newObjType = new CustomGeometryFactory(this.TargetGeometry);
            m_targetContainer.ImportedResources.Add(new ImportedResourceInfo(
                resGeometry,
                device => new GeometryResource(newObjType)));
            m_targetContainer.Objects.Add(new Mesh(resGeometry, resMaterials));
        }

        /// <summary>
        /// Reads all contents of the Obj file.
        /// </summary>
        public void Read()
        {
            var actLineNumber = 0;

            try
            {
                using (var inStreamReader = new StreamReader(m_resource.OpenInputStream()))
                {
                    var multiLineBuilder = new StringBuilder(32);

                    while (!inStreamReader.EndOfStream)
                    {
                        actLineNumber++;

                        var actLine = inStreamReader.ReadLine();

                        if (string.IsNullOrEmpty(actLine))
                        {
                            continue;
                        }

                        // Remove leading and ending spaces
                        actLine = actLine.Trim();

                        // Discard comments
                        if (actLine[0] == '#')
                        {
                            continue;
                        }

                        // Handle multiline entries (they have a \ at the end of the line)
                        if (actLine[actLine.Length - 1] == '\\')
                        {
                            multiLineBuilder.Append(actLine.TrimEnd('\\'));
                            continue;
                        }

                        if (multiLineBuilder.Length > 0)
                        {
                            multiLineBuilder.Append(actLine);
                            actLine = multiLineBuilder.ToString();
                            multiLineBuilder.Clear();
                        }

                        // Parse current keyword and arguments
                        if (!TrySplitLine(actLine, out var actKeyword, out var actArguments))
                        {
                            continue;
                        }

                        // Handle current keyword
                        switch (actKeyword.ToLower())
                        {
                            case "mtllib":
                                this.HandleKeyword_Obj_Mtllib(actArguments);
                                break;

                            case "usemtl":
                                this.HandleKeyword_Obj_UseMtl(actArguments);
                                break;

                            case "o":
                                break;

                            case "v":
                                this.HandleKeyword_Obj_V(actArguments);
                                break;

                            case "vn":
                                this.HandleKeyword_Obj_VN(actArguments);
                                break;

                            case "vt":
                                this.HandleKeyword_Obj_VT(actArguments);
                                break;

                            case "vp":
                                // Parameter space is not supported
                                break;

                            case "f":
                                this.HandleKeyword_Obj_F(actArguments);
                                break;

                            case "p":
                            case "l":
                                // Points and lines not supported currently
                                break;
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                throw new SeeingSharpGraphicsException($"Unable to read obj file {m_resource}: Error at line {actLineNumber}: {ex.Message}", ex);
            }

            // Reorders all triangle indices when necessary
            if(m_importOptions.IsChangeTriangleOrderNeeded())
            {
                this.TargetGeometry.ToggleTriangleIndexOrder();
            }
        }

        /// <summary>
        /// Reads all contents of the given material library.
        /// </summary>
        private void ReadMaterialLibrary(ResourceLink resource)
        {
            var actLineNumber = 0;

            try
            {
                using (var inStreamReader = new StreamReader(resource.OpenInputStream()))
                {
                    var multiLineBuilder = new StringBuilder(32);

                    while (!inStreamReader.EndOfStream)
                    {
                        actLineNumber++;

                        var actLine = inStreamReader.ReadLine();

                        if (string.IsNullOrEmpty(actLine))
                        {
                            continue;
                        }

                        // Remove leading and ending spaces
                        actLine = actLine.Trim();

                        // Discard comments
                        if (actLine[0] == '#')
                        {
                            continue;
                        }

                        // Handle multiline entries (they have a \ at the end of the line)
                        if (actLine[actLine.Length - 1] == '\\')
                        {
                            multiLineBuilder.Append(actLine.TrimEnd('\\'));
                            continue;
                        }

                        if (multiLineBuilder.Length > 0)
                        {
                            multiLineBuilder.Append(actLine);
                            actLine = multiLineBuilder.ToString();
                            multiLineBuilder.Clear();
                        }

                        // Parse current keyword and arguments
                        if (!TrySplitLine(actLine, out var actKeyword, out var actArguments))
                        {
                            continue;
                        }

                        switch(actKeyword.ToLower())
                        {
                            case "newmtl":
                                this.HandleKeyword_Mtl_NewMtl(actArguments);
                                break;

                            case "ka":
                                this.HandleKeyword_Mtl_Ka(actArguments);
                                break;

                            case "kd":
                                this.HandleKeyword_Mtl_Kd(actArguments);
                                break;

                            case "ks":
                                this.HandleKeyword_Mtl_Ks(actArguments);
                                break;

                            case "ke":
                                break;

                            case "map_kd":
                                this.HandleKeyword_Mtl_Map_Kd(actArguments);
                                break;

                            case "ni":       // Optical density
                            case "ns":       // Specular exponent
                            case "d":        // Dissolve factor (transparency)
                            case "tf":       // Transmission filter
                            case "sharpness":
                            case "illum":    // Illumination model
                                // Not supported
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SeeingSharpGraphicsException($"Unable to read material library {m_resource}: Error at line {actLineNumber}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Starts reading all configured material libraries.
        /// </summary>
        private void HandleKeyword_Obj_Mtllib(string arguments)
        {
            var mtlFiles = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for(var loop =0; loop<mtlFiles.Length; loop++)
            {
                var mtlLibResource = m_resource.GetForAnotherFile(mtlFiles[loop]);

                if (mtlLibResource.Exists())
                {
                    this.ReadMaterialLibrary(mtlLibResource);
                }
            }
        }

        /// <summary>
        /// Reads the definition of a new material.
        /// </summary>
        /// <param name="arguments">Passed arguments.</param>
        private void HandleKeyword_Mtl_NewMtl(string arguments)
        {
            var names = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(names.Length != 1)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'newmtl', (expected=1, got={names.Length})!");
            }

            m_currentMaterialDefinition = this.TargetGeometry.CreateSurface(name: names[0]);
        }

        /// <summary>
        /// Reads ambient component of a material.
        /// </summary>
        private void HandleKeyword_Mtl_Ka(string arguments)
        {
            m_currentMaterialDefinition.CommonMaterialProperties.AmbientColor = this.ParseColor("Ka", arguments);
        }

        /// <summary>
        /// Reads diffuse component of a material.
        /// </summary>
        private void HandleKeyword_Mtl_Kd(string arguments)
        {
            m_currentMaterialDefinition.CommonMaterialProperties.DiffuseColor = this.ParseColor("Kd", arguments);
        }

        /// <summary>
        /// Reads specular component of a material.
        /// </summary>
        private void HandleKeyword_Mtl_Ks(string arguments)
        {
            m_currentMaterialDefinition.CommonMaterialProperties.SpecularColor = this.ParseColor("Ks", arguments);
        }

        /// <summary>
        /// Reads the diffuse texture information.
        /// </summary>
        private void HandleKeyword_Mtl_Map_Kd(string arguments)
        {
            var subArguments = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if(subArguments.Length < 1)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'usemtl', (expected= > 0, got={subArguments.Length})!");
            }

            var texFileName = subArguments[subArguments.Length - 1];
            m_currentMaterialDefinition.TextureName = texFileName;
        }

        /// <summary>
        /// Applies the material with the given name for following surfaces.
        /// </summary>
        /// <param name="arguments">Passed arguments.</param>
        private void HandleKeyword_Obj_UseMtl(string arguments)
        {
            var names = arguments.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (names.Length != 1)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'usemtl', (expected=1, got={names.Length})!");
            }

            m_currentSurface = this.TargetGeometry.CreateOrGetExistingSurfaceByName(names[0]);
        }

        /// <summary>
        /// Reads a line containing vertex information.
        /// </summary>
        private void HandleKeyword_Obj_V(string arguments)
        {
            // Split arguments
            var vertexArguments = arguments.Split(ARGUMENT_SPLITTER, StringSplitOptions.RemoveEmptyEntries);

            if(vertexArguments.Length < 3 || vertexArguments.Length > 4)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'v', (expected=3, got={vertexArguments.Length})!");
            }

            // Parse vertex arguments (we don't support the w coordinate)
            if(!TryParseStringsToFloats(vertexArguments, m_dummyFloatArguments_3, 3))
            {
                throw new SeeingSharpGraphicsException($"Unable to parse vertex arguments: {arguments}!");
            }

            // Store vertex
            var actCoordinate = new Vector3(
                m_dummyFloatArguments_3[0] * m_importOptions.ResizeFactor,
                m_dummyFloatArguments_3[1] * m_importOptions.ResizeFactor,
                m_dummyFloatArguments_3[2] * m_importOptions.ResizeFactor);
            m_rawVertices.Add(actCoordinate);
        }

        /// <summary>
        /// Reads a line containing vertex normal information.
        /// </summary>
        private void HandleKeyword_Obj_VN(string arguments)
        {
            // Split arguments
            var vertexArguments = arguments.Split(ARGUMENT_SPLITTER, StringSplitOptions.RemoveEmptyEntries);

            if (vertexArguments.Length != 3)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'v', (expected=3, got={vertexArguments.Length})!");
            }

            // Parse normal arguments
            if (!TryParseStringsToFloats(vertexArguments, m_dummyFloatArguments_3, 3))
            {
                throw new SeeingSharpGraphicsException($"Unable to parse normal arguments: {arguments}!");
            }

            // Store vertex
            m_rawNormals.Add(new Vector3(m_dummyFloatArguments_3[0], m_dummyFloatArguments_3[1], m_dummyFloatArguments_3[2]));
        }

        /// <summary>
        /// Reads a line containing texture coordinate information.
        /// </summary>
        private void HandleKeyword_Obj_VT(string arguments)
        {
            // Split arguments
            var vertexArguments = arguments.Split(ARGUMENT_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            if (vertexArguments.Length < 1 || vertexArguments.Length > 3)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'v', (expected=3, got={vertexArguments.Length})!");
            }

            // We don't support 1D texture, so add a dummy coordinate here
            if(vertexArguments.Length == 1)
            {
                m_rawTextureCoordinates.Add(new Vector2(0f, 0f));
                return;
            }

            // Parse normal arguments
            //  We don't support 3D texture, so the third coordinate is ignored
            if (!TryParseStringsToFloats(vertexArguments, m_dummyFloatArguments_3, 2))
            {
                throw new SeeingSharpGraphicsException($"Unable to parse normal arguments: {arguments}!");
            }

            // Store vertex
            m_rawTextureCoordinates.Add(new Vector2(m_dummyFloatArguments_3[0], 1f - m_dummyFloatArguments_3[1]));
        }

        /// <summary>
        /// Reads a line containing face information.
        /// </summary>
        private void HandleKeyword_Obj_F(string arguments)
        {
            // This is needed, when no mtlib is defined
            if(m_currentSurface == null)
            {
                m_currentSurface = this.TargetGeometry.CreateSurface();
            }

            // Split arguments
            var faceArguments = arguments.Split(ARGUMENT_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            if (faceArguments.Length < 3)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword 'f', (expected = >2, got={faceArguments.Length})!");
            }

            // Prepare FaceIndices array
            FaceIndices[] faceIndices = null;
            if (faceArguments.Length == 3) { faceIndices = m_dummyFaceIndices_3; }
            else if (faceArguments.Length == 4) { faceIndices = m_dummyFaceIndices_4; }
            else { faceIndices = new FaceIndices[faceArguments.Length]; }

            // Parse and preprocess all arguments
            for (var loop = 0; loop < faceArguments.Length; loop++)
            {
                ParseFaceData(faceArguments[loop], m_dummyIntArguments_3);

                // Preprocess vertex index
                var actIndex = m_dummyIntArguments_3[0];
                if (actIndex < 0)
                {
                    var newIndex = m_rawVertices.Count + actIndex;
                    if (newIndex < 0)
                    {
                        throw new SeeingSharpGraphicsException($"Invalid vertex index: {actIndex} (we currently have {m_rawVertices.Count} vertices)!");
                    }
                    faceIndices[loop].VertexIndex = newIndex;
                }
                else
                {
                    faceIndices[loop].VertexIndex = m_dummyIntArguments_3[0] - 1;
                }

                // Preprocess texture coordinates
                actIndex = m_dummyIntArguments_3[1];
                if (actIndex < 0 && actIndex != int.MinValue)
                {
                    var newIndex = m_rawTextureCoordinates.Count + actIndex;
                    if (newIndex < 0)
                    {
                        throw new SeeingSharpGraphicsException($"Invalid vertex index: {actIndex} (we currently have {m_rawTextureCoordinates.Count} texture coordinates)!");
                    }
                    faceIndices[loop].TextureCoordinateIndex = newIndex;
                }
                else if (actIndex != int.MinValue)
                {
                    faceIndices[loop].TextureCoordinateIndex = m_dummyIntArguments_3[1] - 1;
                }
                else
                {
                    faceIndices[loop].TextureCoordinateIndex = int.MinValue;
                }

                // Preprocess normal coordinates
                actIndex = m_dummyIntArguments_3[2];
                if (actIndex < 0 && actIndex != int.MinValue)
                {
                    var newIndex = m_rawNormals.Count + actIndex;
                    if (newIndex < 0)
                    {
                        throw new SeeingSharpGraphicsException($"Invalid vertex index: {actIndex} (we currently have {m_rawNormals.Count} normals)!");
                    }
                    faceIndices[loop].NormalIndex = newIndex;
                }
                else if (actIndex != int.MinValue)
                {
                    faceIndices[loop].NormalIndex = m_dummyIntArguments_3[2] - 1;
                }
                else
                {
                    faceIndices[loop].NormalIndex = int.MinValue;
                }
            }

            // Generate vertices and triangles on current Geometry
            if(faceIndices.Length == 3)
            {
                this.GenerateFaceVertices(faceIndices).ForEachInEnumeration(actIndex => { });
                var highestVertexIndex = this.TargetGeometry.CountVertices - 1;
                m_currentSurface.AddTriangle(
                    highestVertexIndex - 2,
                    highestVertexIndex - 1,
                    highestVertexIndex);
                if(m_importOptions.TwoSidedSurfaces)
                {
                    m_currentSurface.AddTriangle(
                        highestVertexIndex,
                        highestVertexIndex - 1,
                        highestVertexIndex - 2);
                }
            }
            else
            {
                m_currentSurface.AddPolygonByCuttingEars(this.GenerateFaceVertices(faceIndices),
                    m_importOptions.TwoSidedSurfaces);
            }
        }

        /// <summary>
        /// Generates all vertices for given FaceIndices.
        /// </summary>
        private IEnumerable<int> GenerateFaceVertices(FaceIndices[] faceIndices)
        {
            // Generate vertex
            for (var loop = 0; loop < faceIndices.Length; loop++)
            {
                var actFaceIndices = faceIndices[loop];

                var actVertex = new Vertex
                {
                    Position = m_rawVertices[actFaceIndices.VertexIndex]
                };


                if (actFaceIndices.TextureCoordinateIndex > int.MinValue)
                {
                    actVertex.TexCoord = m_rawTextureCoordinates[actFaceIndices.TextureCoordinateIndex];
                }

                if (actFaceIndices.NormalIndex > int.MinValue)
                {
                    actVertex.Normal = m_rawNormals[actFaceIndices.NormalIndex];
                }

                yield return this.TargetGeometry.AddVertex(actVertex);
            }
        }

        /// <summary>
        /// Tries to split the given line into keyword and arguments.
        /// Returns true if this is a valid line.
        /// </summary>
        /// <param name="line">The line to be splitted.</param>
        /// <param name="keyword">The keyword.</param>
        /// <param name="arguments">All arguments for the keyword.</param>
        private static bool TrySplitLine(string line, out string keyword, out string arguments)
        {
            var idx = line.IndexOf(' ');
            if (idx < 0)
            {
                keyword = line;
                arguments = null;
                return false;
            }

            keyword = line.Substring(0, idx);
            arguments = line.Substring(idx + 1);
            return true;
        }

        /// <summary>
        /// Tries to parse all given strings into the given float array..
        /// </summary>
        /// <param name="sourceStrings">The source strings.</param>
        /// <param name="targetFloats">The target floats.</param>
        /// <param name="countValuesToParse">Total count of values to parse.</param>
        private static bool TryParseStringsToFloats(string[] sourceStrings, float[] targetFloats, int countValuesToParse)
        {
            for (var loop = 0; loop < countValuesToParse; loop++)
            {
                if(!float.TryParse(sourceStrings[loop], NumberStyles.Float, FILE_NUMBER_FORMAT, out var actParsedFloat))
                {
                    return false;
                }
                targetFloats[loop] = actParsedFloat;
            }
            return true;
        }

        /// <summary>
        /// Tries to parse all given strings into the given int array..
        /// </summary>
        /// <param name="sourceStrings">The source strings.</param>
        /// <param name="targetInts">The target ints.</param>
        private static bool TryParseStringsToInts(string[] sourceStrings, int[] targetInts)
        {
            var length = sourceStrings.Length;
            for (var loop = 0; loop < length; loop++)
            {
                if (!int.TryParse(sourceStrings[loop], NumberStyles.Integer, FILE_NUMBER_FORMAT, out var actParsedInt))
                {
                    return false;
                }
                targetInts[loop] = actParsedInt;
            }
            return true;
        }

        /// <summary>
        /// Tries to parse all given strings into the given int array..
        /// </summary>
        /// <param name="sourceString">The string to be parsed.</param>
        /// <param name="targetInts">The target array where to put all values.</param>
        private static bool ParseFaceData(string sourceString, int[] targetInts)
        {
            var splitted = sourceString.Split('/');
            if(splitted.Length < 1 || splitted.Length > 3)
            {
                throw new SeeingSharpGraphicsException($"Invalid face argument: {sourceString}! (invalid count of items)");
            }

            for (var loop = 0; loop < targetInts.Length; loop++)
            {
                if (splitted.Length <= loop ||
                    string.IsNullOrEmpty(splitted[loop]))
                {
                    if(loop==0)
                    {
                        throw new SeeingSharpGraphicsException($"Invalid face argument: {sourceString} (first value missing)!");
                    }

                    // Missing value
                    targetInts[loop] = int.MinValue;
                }
                else
                {
                    if(!int.TryParse(splitted[loop], NumberStyles.Integer, FILE_NUMBER_FORMAT, out var parsedValue))
                    {
                        throw new SeeingSharpGraphicsException($"Invalid face argument: {sourceString} (unable to parse int)!");
                    }
                    targetInts[loop] = parsedValue;
                }
            }

            return true;
        }

        private Color4 ParseColor(string keyword, string arguments)
        {
            if (arguments.Contains("spectral") ||
               arguments.Contains("xyz"))
            {
                // We don't support spectral or xyz arguments
                return new Color4(1f, 1f, 1f, 1f);
            }

            // Split arguments
            var ambientArguments = arguments.Split(ARGUMENT_SPLITTER, StringSplitOptions.RemoveEmptyEntries);
            if (ambientArguments.Length < 1 ||
                ambientArguments.Length < 3)
            {
                throw new SeeingSharpGraphicsException($"Invalid count of arguments for keyword '{keyword}', (expected=1-3, got={ambientArguments.Length})!");
            }

            // Parse vertex arguments (we don't support the w coordinate)
            if (!TryParseStringsToFloats(ambientArguments, m_dummyFloatArguments_3, ambientArguments.Length))
            {
                throw new SeeingSharpGraphicsException($"Unable to parse arguments for keyword {keyword}: {arguments}!");
            }

            // G and B components are optional...
            for (var loop = 2; loop > 0; loop--)
            {
                if (ambientArguments.Length <= loop)
                {
                    m_dummyFaceIndices_3[loop] = m_dummyFaceIndices_3[0];
                }
            }

            // Return current color value
            return new Color4(m_dummyFloatArguments_3[0], m_dummyFloatArguments_3[1], m_dummyFloatArguments_3[2], 1f);
        }

        public Geometry TargetGeometry { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private struct FaceIndices
        {
            public int VertexIndex;
            public int TextureCoordinateIndex;
            public int NormalIndex;
        }
    }
}
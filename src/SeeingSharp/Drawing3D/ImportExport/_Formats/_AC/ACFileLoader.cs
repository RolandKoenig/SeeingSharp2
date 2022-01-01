using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    public static class ACFileLoader
    {
        /// <summary>
        /// Gets the default extension (e. g. ".ac").
        /// </summary>
        public static string DefaultExtension => ".ac";

        /// <summary>
        /// Imports an object-type form given raw model file.
        /// </summary>
        /// <param name="rawBytes">Raw model file.</param>
        public static GeometryFactory ImportGeometryFactory(byte[] rawBytes)
        {
            using (var inStream = new MemoryStream(rawBytes))
            {
                return ImportGeometryFactory(inStream);
            }
        }

        /// <summary>
        /// Loads an object from the given uri
        /// </summary>
        public static GeometryFactory ImportGeometryFactory(Stream inStream)
        {
            var geometry = ImportGeometry(inStream);
            return new CustomGeometryFactory(geometry);
        }

        /// <summary>
        /// Imports a ac file from the given stream.
        /// </summary>
        /// <param name="resourceLink">The link to the ac file.</param>
        public static GeometryFactory ImportGeometryFactory(ResourceLink resourceLink)
        {
            using var inStream = resourceLink.OpenInputStream();

            var geometry = ImportGeometry(inStream, resourceLink);
            return new CustomGeometryFactory(geometry);
        }

        /// <summary>
        /// Imports a ac file from the given resource.
        /// </summary>
        /// <param name="resourceLink">The link to the ac file.</param>
        public static Geometry ImportGeometry(ResourceLink resourceLink)
        {
            using var inStream = resourceLink.OpenInputStream();

            return ImportGeometry(inStream, resourceLink);
        }

        /// <summary>
        /// Imports a Geometry from the given stream.
        /// </summary>
        /// <param name="inStream">The stream to load the data from.</param>
        public static Geometry ImportGeometry(Stream inStream)
        {
            return ImportGeometry(inStream, null);
        }

        /// <summary>
        /// Import Geometry from the given stream.
        /// </summary>
        /// <param name="inStream">The stream to load the data from.</param>
        /// <param name="originalSource">The original source of the generated geometry.</param>
        public static Geometry ImportGeometry(Stream inStream, ResourceLink originalSource)
        {
            try
            {
                // Load the file and generate all Geometries
                var fileInfo = LoadFile(inStream);
                var result = GenerateGeometry(fileInfo);

                // return the result
                return result;
            }
            catch (Exception)
            {
                // Create dummy Geometry
                var dummyGeometry = new Geometry();
                dummyGeometry.FirstSurface.BuildCube(
                    new Vector3(),
                    new Vector3(1f, 1f, 1f));
                return dummyGeometry;
            }
        }

        /// <summary>
        /// Generates the geometry needed for this object
        /// </summary>
        internal static Geometry GenerateGeometry(ACFileInfo fileInfo)
        {
            var result = new Geometry();

            // Create a surface for each material first
            var materialList = fileInfo.Materials;
            for (var actMaterialIndex = 0; actMaterialIndex < materialList.Count; actMaterialIndex++)
            {
                result.CreateSurface();
            }

            // Now load the geometry itself
            var transformStack = new Matrix4Stack();
            foreach (var actObject in fileInfo.Objects)
            {
                FillGeometry(result, materialList, actObject, transformStack);
            }

            return result;
        }

        /// <summary>
        /// Loads a ac file from the given uri
        /// </summary>
        internal static ACFileInfo LoadFile(Stream inStream)
        {
            ACFileInfo result = null;

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(inStream);

                //Check for correct header
                var header = reader.ReadLine();
                if (header == null ||
                    !header.StartsWith("AC3D"))
                {
                    throw new SeeingSharpGraphicsException("Header of AC3D file not found!");
                }

                //Create file information object
                result = new ACFileInfo();

                //Create a loaded objects stack
                var loadedObjects = new Stack<ACObjectInfo>();
                var parentObjects = new Stack<ACObjectInfo>();
                ACSurface currentSurface = null;

                //Read the file
                while (!reader.EndOfStream)
                {
                    var actLine = reader.ReadLine().Trim();

                    var firstWord = string.Empty;
                    var spaceIndex = actLine.IndexOf(' ');
                    if (spaceIndex == -1) { firstWord = actLine; }
                    else { firstWord = firstWord = actLine.Substring(0, spaceIndex); }

                    switch (firstWord)
                    {
                        //New Material info
                        case "MATERIAL":
                            var materialInfo = new ACMaterialInfo();
                            {
                                //Get the name of the material
                                var materialData = actLine.Split(' ');

                                if (materialData.Length > 1) { materialInfo.Name = materialData[1].Trim(' ', '"'); }

                                //Parse components
                                for (var loop = 0; loop < materialData.Length; loop++)
                                {
                                    switch (materialData[loop])
                                    {
                                        case "rgb":
                                            var diffuseColor = materialInfo.Diffuse;
                                            diffuseColor.Alpha = 1f;
                                            diffuseColor.Red = float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture);
                                            diffuseColor.Green = float.Parse(materialData[loop + 2], CultureInfo.InvariantCulture);
                                            diffuseColor.Blue = float.Parse(materialData[loop + 3], CultureInfo.InvariantCulture);
                                            materialInfo.Diffuse = diffuseColor;
                                            break;

                                        case "amb":
                                            var ambientColor = new Color4();
                                            ambientColor.Red = float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture);
                                            ambientColor.Green = float.Parse(materialData[loop + 2], CultureInfo.InvariantCulture);
                                            ambientColor.Blue = float.Parse(materialData[loop + 3], CultureInfo.InvariantCulture);
                                            materialInfo.Ambient = ambientColor;
                                            break;

                                        case "emis":
                                            var emissiveColor = new Color4();
                                            emissiveColor.Red = float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture);
                                            emissiveColor.Green = float.Parse(materialData[loop + 2], CultureInfo.InvariantCulture);
                                            emissiveColor.Blue = float.Parse(materialData[loop + 3], CultureInfo.InvariantCulture);
                                            materialInfo.Emissive = emissiveColor;
                                            break;

                                        case "spec":
                                            var specularColor = new Color4();
                                            specularColor.Red = float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture);
                                            specularColor.Green = float.Parse(materialData[loop + 2], CultureInfo.InvariantCulture);
                                            specularColor.Blue = float.Parse(materialData[loop + 3], CultureInfo.InvariantCulture);
                                            materialInfo.Specular = specularColor;
                                            break;

                                        case "shi":
                                            materialInfo.Shininess = float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture);
                                            break;

                                        case "trans":
                                            diffuseColor = materialInfo.Diffuse;
                                            diffuseColor.Alpha = 1f - EngineMath.Clamp(float.Parse(materialData[loop + 1], CultureInfo.InvariantCulture), 0f, 1f);
                                            materialInfo.Diffuse = diffuseColor;
                                            break;
                                    }
                                }
                                result.Materials.Add(materialInfo);
                            }
                            break;

                        //New object starts here
                        case "OBJECT":
                            {
                                var newObject = new ACObjectInfo();

                                var lineData = actLine.Split(' ');
                                if (lineData[1] == "poly") { newObject.Type = ACObjectType.Poly; }
                                else if (lineData[1] == "group") { newObject.Type = ACObjectType.Group; }
                                else if (lineData[1] == "world") { newObject.Type = ACObjectType.World; }

                                loadedObjects.Push(newObject);
                            }
                            break;

                        //End of an object, kids following
                        case "kids":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                //Parse kid count
                                var kidCount = 0;
                                var lineData = actLine.Split(' ');

                                if (lineData != null && lineData.Length >= 1)
                                {
                                    int.TryParse(lineData[1], out kidCount);
                                }

                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    //AddObject object to parent object, if any related
                                    var addedToParent = false;

                                    if (parentObjects.Count > 0)
                                    {
                                        var currentParent = parentObjects.Peek();

                                        if (currentParent.Children.Count < currentParent.KidCount)
                                        {
                                            currentParent.Children.Add(currentObject);
                                            addedToParent = true;
                                        }
                                        else
                                        {
                                            while (parentObjects.Count > 0)
                                            {
                                                parentObjects.Pop();

                                                if (parentObjects.Count == 0) { break; }

                                                currentParent = parentObjects.Peek();

                                                if (currentParent == null) { break; }
                                                if (currentParent.Children.Count < currentParent.KidCount) { break; }
                                            }

                                            if (currentParent != null &&
                                                currentParent.Children.Count < currentParent.KidCount)
                                            {
                                                currentParent.Children.Add(currentObject);
                                                addedToParent = true;
                                            }
                                        }
                                    }

                                    //Enable this object as parent object
                                    currentObject.KidCount = kidCount;

                                    if (currentObject.KidCount > 0) { parentObjects.Push(currentObject); }

                                    //AddObject to scene root if this object has no parent
                                    loadedObjects.Pop();

                                    if (!addedToParent)
                                    {
                                        if (loadedObjects.Count == 0)
                                        {
                                            result.Objects.Add(currentObject);
                                        }
                                        else
                                        {
                                            loadedObjects.Peek().Children.Add(currentObject);
                                        }
                                    }
                                    currentObject = null;
                                }
                            }
                            break;

                        //Current object's name
                        case "name":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();
                                if (currentObject != null)
                                {
                                    currentObject.Name = actLine.Replace("name ", "").Replace("\"", "");
                                }
                            }
                            break;

                        case "data":
                            break;

                        case "texture":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');
                                    currentObject.Texture = lineData[1].Trim('"');
                                }
                            }
                            break;

                        case "texrep":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');

                                    var repetition = new Vector2
                                    {
                                        X = float.Parse(lineData[1], CultureInfo.InvariantCulture),
                                        Y = float.Parse(lineData[2], CultureInfo.InvariantCulture)
                                    };

                                    currentObject.TextureRepeat = repetition;
                                }
                            }
                            break;

                        case "texoff":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');

                                    var offset = new Vector2
                                    {
                                        X = float.Parse(lineData[1], CultureInfo.InvariantCulture),
                                        Y = float.Parse(lineData[2], CultureInfo.InvariantCulture)
                                    };

                                    currentObject.TextureRepeat = offset;
                                }
                            }
                            break;

                        case "rot":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');

                                    var rotation = Matrix4x4.Identity;
                                    rotation.M11 = !string.IsNullOrEmpty(lineData[1]) ? float.Parse(lineData[1], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M12 = !string.IsNullOrEmpty(lineData[2]) ? float.Parse(lineData[2], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M13 = !string.IsNullOrEmpty(lineData[3]) ? float.Parse(lineData[3], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M21 = !string.IsNullOrEmpty(lineData[4]) ? float.Parse(lineData[4], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M22 = !string.IsNullOrEmpty(lineData[5]) ? float.Parse(lineData[5], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M23 = !string.IsNullOrEmpty(lineData[6]) ? float.Parse(lineData[6], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M31 = !string.IsNullOrEmpty(lineData[7]) ? float.Parse(lineData[7], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M32 = !string.IsNullOrEmpty(lineData[8]) ? float.Parse(lineData[8], CultureInfo.InvariantCulture) : 0f;
                                    rotation.M33 = !string.IsNullOrEmpty(lineData[9]) ? float.Parse(lineData[9], CultureInfo.InvariantCulture) : 0f;

                                    currentObject.Rotation = rotation;
                                }
                            }
                            break;

                        case "url":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');
                                    currentObject.Url = lineData[1].Trim('"');
                                }
                            }
                            break;

                        //Current object's location
                        case "loc":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');

                                    var location = new Vector3
                                    {
                                        X = float.Parse(lineData[1], CultureInfo.InvariantCulture),
                                        Y = float.Parse(lineData[2], CultureInfo.InvariantCulture),
                                        Z = float.Parse(lineData[3], CultureInfo.InvariantCulture)
                                    };

                                    currentObject.Translation = location;
                                }
                            }
                            break;

                        case "numvert":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                var currentObject = loadedObjects.Peek();

                                if (currentObject != null)
                                {
                                    var lineData = actLine.Split(' ');
                                    var numberOfVertices = int.Parse(lineData[1], CultureInfo.InvariantCulture);

                                    for (var loop = 0; loop < numberOfVertices; loop++)
                                    {
                                        var actInnerLine = reader.ReadLine().Trim();
                                        var splittedVertex = actInnerLine.Split(' ');

                                        var position = new Vector3
                                        {
                                            X = float.Parse(splittedVertex[0], CultureInfo.InvariantCulture),
                                            Y = float.Parse(splittedVertex[1], CultureInfo.InvariantCulture),
                                            Z = float.Parse(splittedVertex[2], CultureInfo.InvariantCulture)
                                        };

                                        currentObject.Vertices.Add(new ACVertex { Position = position });
                                    }
                                }
                            }
                            break;

                        //Start of a list of surfaces
                        case "numsurf":
                            break;

                        //New surface starts here
                        case "SURF":
                            {
                                if (currentSurface == null) { currentSurface = new ACSurface(); }

                                var lineData = actLine.Split(' ');
                                lineData[1] = lineData[1].Substring(2);
                                currentSurface.Flags = int.Parse(lineData[1], NumberStyles.HexNumber);
                            }
                            break;

                        //Current surface's material
                        case "mat":
                            {
                                if (currentSurface == null) { currentSurface = new ACSurface(); }

                                var lineData = actLine.Split(' ');
                                currentSurface.Material = int.Parse(lineData[1], CultureInfo.InvariantCulture);
                            }
                            break;

                        //Current surface's indices
                        case "refs":
                            if (loadedObjects.Count == 0) { break; }
                            {
                                if (currentSurface == null)
                                {
                                    currentSurface = new ACSurface();
                                }

                                var lineData = actLine.Split(' ');
                                var numberOfRefs = int.Parse(lineData[1], CultureInfo.InvariantCulture);

                                for (var loop = 0; loop < numberOfRefs; loop++)
                                {
                                    var actInnerLine = reader.ReadLine().Trim();
                                    var splittedRef = actInnerLine.Split(' ');

                                    var texCoord = new Vector2();
                                    int vertexReference = ushort.Parse(splittedRef[0], CultureInfo.InvariantCulture);
                                    texCoord.X = float.Parse(splittedRef[1], CultureInfo.InvariantCulture);
                                    texCoord.Y = float.Parse(splittedRef[2], CultureInfo.InvariantCulture);

                                    currentSurface.TextureCoordinates.Add(texCoord);
                                    currentSurface.VertexReferences.Add(vertexReference);
                                }

                                var currentObject = loadedObjects.Peek();

                                currentObject?.Surfaces.Add(currentSurface);

                                currentSurface = null;
                            }
                            break;
                    }
                }
            }
            finally
            {
                SeeingSharpUtil.SafeDispose(ref reader);
            }

            return result;
        }

        /// <summary>
        /// Fills the given geometry using information from the given AC-File-Objects.
        /// </summary>
        /// <param name="objInfo">The object information from the AC file.</param>
        /// <param name="acMaterials">A list containing all materials from the AC file.</param>
        /// <param name="geometry">The Geometry to be filled.</param>
        /// <param name="transformStack">Current matrix stack (for stacked objects).</param>
        private static void FillGeometry(Geometry geometry, List<ACMaterialInfo> acMaterials, ACObjectInfo objInfo, Matrix4Stack transformStack)
        {
            var standardShadedVertices = new List<Tuple<int, int>>();

            transformStack.Push();
            try
            {
                // Perform local transformation for the current AC object
                transformStack.TransformLocal(objInfo.Rotation);
                transformStack.TranslateLocal(objInfo.Translation);

                // Build geometry material by material
                for (var actMaterialIndex = 0; actMaterialIndex < acMaterials.Count; actMaterialIndex++)
                {
                    var actGeometrySurface = geometry.Surfaces[actMaterialIndex];

                    // Initialize local index table (needed for vertex reuse)
                    var oneSideVertexCount = objInfo.Vertices.Count;
                    var localIndices = new int[oneSideVertexCount * 2];

                    for (var loop = 0; loop < localIndices.Length; loop++)
                    {
                        localIndices[loop] = int.MaxValue;
                    }

                    // Process all surfaces
                    foreach (var actSurface in objInfo.Surfaces)
                    {
                        // Get the vertex index on which to start
                        var startVertexIndex = geometry.CountVertices;
                        var startTriangleIndex = actGeometrySurface.CountTriangles;

                        // Only handle surfaces of the current material
                        if (actSurface.Material != actMaterialIndex) { continue; }

                        // Sort out unsupported surfaces
                        if (actSurface.VertexReferences.Count < 3) { continue; }
                        if (actSurface.IsLine) { continue; }
                        if (actSurface.IsClosedLine) { continue; }

                        // Preprocess referenced vertices
                        var oneSideSurfaceVertexCount = actSurface.VertexReferences.Count;
                        var countSurfaceSides = actSurface.IsTwoSided ? 2 : 1;
                        var onGeometryReferencedVertices = new int[oneSideSurfaceVertexCount * countSurfaceSides];
                        var surfaceVertexReferences = actSurface.VertexReferences;

                        for (var loop = 0; loop < surfaceVertexReferences.Count; loop++)
                        {
                            var actTexCoord = actSurface.TextureCoordinates[loop];

                            if (!actSurface.IsFlatShaded)
                            {
                                // Try to reuse vertices on standard shading
                                if (localIndices[surfaceVertexReferences[loop]] == int.MaxValue)
                                {
                                    var position = Vector3.Transform(
                                        objInfo.Vertices[surfaceVertexReferences[loop]].Position,
                                        transformStack.Top);
                                    localIndices[surfaceVertexReferences[loop]] = geometry.AddVertex(new VertexBasic(
                                        position, Color4.White, actTexCoord, Vector3.Zero));
                                    if (actSurface.IsTwoSided)
                                    {
                                        localIndices[surfaceVertexReferences[loop] + oneSideVertexCount] = geometry.AddVertex(new VertexBasic(
                                            position, Color4.White, actTexCoord, Vector3.Zero));
                                    }
                                }

                                // Store vertex reference for this surface's index
                                onGeometryReferencedVertices[loop] = localIndices[surfaceVertexReferences[loop]];
                                if (actSurface.IsTwoSided)
                                {
                                    onGeometryReferencedVertices[loop + oneSideSurfaceVertexCount] =
                                        localIndices[surfaceVertexReferences[loop] + oneSideVertexCount];
                                }
                            }
                            else
                            {
                                // Create one vertex for one reference for flat shading
                                var position = Vector3.Transform(
                                    objInfo.Vertices[surfaceVertexReferences[loop]].Position,
                                    transformStack.Top);
                                onGeometryReferencedVertices[loop] = geometry.AddVertex(new VertexBasic(
                                    position, Color4.White, actTexCoord, Vector3.Zero));

                                if (actSurface.IsTwoSided)
                                {
                                    onGeometryReferencedVertices[loop + oneSideSurfaceVertexCount] = geometry.AddVertex(new VertexBasic(
                                        position, Color4.White, actTexCoord, Vector3.Zero));
                                }
                            }
                        }

                        // Build object geometry
                        switch (actSurface.VertexReferences.Count)
                        {
                            case 3:
                                // Front side
                                actGeometrySurface.AddTriangle(
                                    onGeometryReferencedVertices[0],
                                    onGeometryReferencedVertices[1],
                                    onGeometryReferencedVertices[2]);

                                // Back side
                                if (actSurface.IsTwoSided)
                                {
                                    actGeometrySurface.AddTriangle(
                                        onGeometryReferencedVertices[5],
                                        onGeometryReferencedVertices[4],
                                        onGeometryReferencedVertices[3]);
                                }
                                break;

                            case 4:
                                // Front side
                                actGeometrySurface.AddTriangle(
                                    onGeometryReferencedVertices[0],
                                    onGeometryReferencedVertices[1],
                                    onGeometryReferencedVertices[2]);
                                actGeometrySurface.AddTriangle(
                                    onGeometryReferencedVertices[2],
                                    onGeometryReferencedVertices[3],
                                    onGeometryReferencedVertices[0]);

                                // Back side
                                if (actSurface.IsTwoSided)
                                {
                                    actGeometrySurface.AddTriangle(
                                        onGeometryReferencedVertices[6],
                                        onGeometryReferencedVertices[5],
                                        onGeometryReferencedVertices[4]);
                                    actGeometrySurface.AddTriangle(
                                        onGeometryReferencedVertices[4],
                                        onGeometryReferencedVertices[7],
                                        onGeometryReferencedVertices[6]);
                                }
                                break;

                            default:
                                if (!actSurface.IsTwoSided)
                                {
                                    // Front side
                                    actGeometrySurface.AddPolygonByCuttingEars(onGeometryReferencedVertices);
                                }
                                else
                                {
                                    // Front and back side
                                    actGeometrySurface.AddPolygonByCuttingEars(onGeometryReferencedVertices.Subset(0, oneSideSurfaceVertexCount));
                                    actGeometrySurface.AddPolygonByCuttingEars(onGeometryReferencedVertices.Subset(oneSideSurfaceVertexCount, oneSideSurfaceVertexCount));
                                }
                                break;
                        }

                        // Perform shading
                        if (actSurface.IsFlatShaded)
                        {
                            actGeometrySurface.CalculateNormalsFlat(
                                startTriangleIndex, actGeometrySurface.CountTriangles - startTriangleIndex);
                        }
                        else
                        {
                            // Nothing to be done for now..
                            var vertexCount = geometry.CountVertices - startVertexIndex;
                            if (vertexCount > 0)
                            {
                                standardShadedVertices.Add(
                                    Tuple.Create(startVertexIndex, vertexCount));
                            }
                        }
                    }

                    // Calculate default shading finally (if any)
                    foreach (var actStandardShadedPair in standardShadedVertices)
                    {
                        geometry.CalculateNormals(
                            actStandardShadedPair.Item1,
                            actStandardShadedPair.Item2);
                    }
                    standardShadedVertices.Clear();
                }

                // Fill in all child object data
                foreach (var actObjInfo in objInfo.Children)
                {
                    FillGeometry(geometry, acMaterials, actObjInfo, transformStack);
                }
            }
            finally
            {
                transformStack.Pop();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using SeeingSharp.Checking;
using System.Text;
using System.Text.RegularExpressions;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    // This code is based on HelixToolkit (MIT license)
    // https://github.com/helix-toolkit/helix-toolkit/blob/master/Source/HelixToolkit.Wpf/Importers/StLReader.cs

    /// <summary>
    /// Provides an importer for StereoLithography .StL files.
    /// </summary>
    /// <remarks>
    /// The format is documented on <a href="http://en.wikipedia.org/wiki/STL_(file_format)">Wikipedia</a>.
    /// </remarks>
    [SupportedFileFormat("stl", "StereoLithography .StL files")]
    public class StLImporter : IModelImporter
    {
        // Constants
        private const string RES_KEY_GEO_CLASS = "Geometry";
        private const string RES_KEY_GEO_NAME = "Main";
        private const string RES_KEY_MAT_CLASS = "Material";
        private const string RES_KEY_MAT_NAME = "Main";

        // Static regular expressions for parsing
        private static readonly Encoding s_encoding = Encoding.GetEncoding("us-ascii");
        private static readonly Regex s_normalRegex = new Regex(@"normal\s*(\S*)\s*(\S*)\s*(\S*)");
        private static readonly Regex s_vertexRegex = new Regex(@"vertex\s*(\S*)\s*(\S*)\s*(\S*)");

        // Just for caching
        private List<Vector3> _cachedPoints = new List<Vector3>(3);

        /// <summary>
        /// Imports a model from the given file.
        /// </summary>
        /// <param name="importOptions">Some configuration for the importer.</param>
        /// <param name="sourceFile">The source file to be loaded.</param>
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            // Get import options
            var stlImportOptions = importOptions as StlImportOptions;
            if (stlImportOptions == null)
            {
                throw new SeeingSharpException("Invalid import options for StlImporter!");
            }

            ImportedModelContainer? result = null;
            try
            {
                switch (stlImportOptions.Format)
                {
                    case StlFileFormat.Unknown:
                        result = this.TryReadBinary(sourceFile, stlImportOptions, true);
                        if (result == null)
                        {
                            result = this.TryReadAscii(sourceFile, stlImportOptions);
                        }
                        break;

                    case StlFileFormat.Ascii:
                        result = this.TryReadAscii(sourceFile, stlImportOptions);
                        break;

                    case StlFileFormat.Binary:
                        result = this.TryReadBinary(sourceFile, stlImportOptions, false);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new SeeingSharpException($"Unable to read Stl file {sourceFile}: {ex.Message}!", ex);
            }

            // Handle empty result (unknown format error)
            if (result == null)
            {
                throw new SeeingSharpException($"Unable to read Stl file {sourceFile}: Unrecognized format error!");
            }

            return result;
        }

        /// <summary>
        /// Creates a default import options object for this importer.
        /// </summary>
        public ImportOptions CreateDefaultImportOptions()
        {
            return new StlImportOptions();
        }

        /// <summary>
        /// Parses the ID and values from the specified line.
        /// </summary>
        private static void ParseLine(string line, out string id, out string values)
        {
            line = line.Trim();
            var idx = line.IndexOf(' ');
            if (idx == -1)
            {
                id = line;
                values = string.Empty;
            }
            else
            {
                id = line.Substring(0, idx).ToLower();
                values = line.Substring(idx + 1);
            }
        }

        /// <summary>
        /// Parses a normal string.
        /// </summary>
        private static Vector3 ParseNormal(string input)
        {
            var match = s_normalRegex.Match(input);
            if (!match.Success)
            {
                throw new SeeingSharpException($"Unexpected line while reading Stl file. Line content: {Environment.NewLine}{input}");
            }

            var x = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var y = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var z = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Reads a float (4 byte)
        /// </summary>
        private static float ReadFloat(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Reads a line from the stream reader.
        /// </summary>
        private static void ReadLine(StreamReader reader, string token)
        {
            token.EnsureNotNull(nameof(token));

            var line = reader.ReadLine();
            if (line == null) { throw new SeeingSharpException("Unexpected end of file!"); }

            ParseLine(line, out var id, out _);

            if (!string.Equals(token, id, StringComparison.OrdinalIgnoreCase))
            {
                throw new SeeingSharpException($"Unexpected line (expected: {token}, got:{id})");
            }
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The unsigned integer.
        /// </returns>
        private static ushort ReadUInt16(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(2);
            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The unsigned integer.
        /// </returns>
        private static uint ReadUInt32(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Tries to parse a vertex from a string.
        /// </summary>
        private static bool TryParseVertex(string line, out Vector3 point)
        {
            var match = s_vertexRegex.Match(line);
            if (!match.Success)
            {
                point = new Vector3();
                return false;
            }

            var x = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var y = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var z = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            point = new Vector3(x, y, z);
            return true;
        }

        /// <summary>
        /// Reads a facet.
        /// </summary>
        private void ReadFacet(StreamReader reader, string normalString, Geometry newGeometry, StlImportOptions importOptions)
        {
            _cachedPoints.Clear();

            // Read all geometry
            var normal = ParseNormal(normalString);
            ReadLine(reader, "outer");

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) { throw new SeeingSharpException("Unexpected end of file!"); }

                if (TryParseVertex(line, out var point))
                {
                    _cachedPoints.Add(point);
                    continue;
                }

                ParseLine(line, out var id, out _);

                if (id == "endloop")
                {
                    break;
                }
            }

            // Read end
            ReadLine(reader, "endfacet");

            // Overtake geometry data
            var targetSurface = newGeometry.FirstSurface;
            var pointCount = _cachedPoints.Count;

            switch (_cachedPoints.Count)
            {
                case 0:
                case 1:
                case 2:
                    break;

                case 3:
                    if (importOptions.IsChangeTriangleOrderNeeded())
                    {
                        targetSurface.AddTriangle(
                            new VertexBasic(_cachedPoints[2], Color4.Transparent, Vector2.Zero, normal),
                            new VertexBasic(_cachedPoints[1], Color4.Transparent, Vector2.Zero, normal),
                            new VertexBasic(_cachedPoints[0], Color4.Transparent, Vector2.Zero, normal));
                    }
                    else
                    {
                        targetSurface.AddTriangle(
                            new VertexBasic(_cachedPoints[0], Color4.Transparent, Vector2.Zero, normal),
                            new VertexBasic(_cachedPoints[1], Color4.Transparent, Vector2.Zero, normal),
                            new VertexBasic(_cachedPoints[2], Color4.Transparent, Vector2.Zero, normal));
                    }
                    break;

                default:
                    var indices = new int[pointCount];
                    if (importOptions.IsChangeTriangleOrderNeeded())
                    {
                        for (var loop = pointCount - 1; loop > -1; loop--)
                        {
                            indices[loop] = newGeometry.AddVertex(
                                new VertexBasic(_cachedPoints[loop], Color4.Transparent, Vector2.Zero, normal));
                        }
                    }
                    else
                    {
                        for (var loop = 0; loop < pointCount; loop++)
                        {
                            indices[loop] = newGeometry.AddVertex(
                                new VertexBasic(_cachedPoints[loop], Color4.Transparent, Vector2.Zero, normal));
                        }
                    }

                    targetSurface.AddPolygonByCuttingEars(indices);
                    break;
            }
        }

        /// <summary>
        /// Reads a triangle from a binary STL file.
        /// </summary>
        private static void ReadTriangle(BinaryReader reader, Geometry geometry, StlImportOptions importOptions)
        {
            var ni = ReadFloat(reader);
            var nj = ReadFloat(reader);
            var nk = ReadFloat(reader);
            var normal = new Vector3(ni, nj, nk);

            var x1 = ReadFloat(reader);
            var y1 = ReadFloat(reader);
            var z1 = ReadFloat(reader);
            var v1 = new Vector3(x1, y1, z1);

            var x2 = ReadFloat(reader);
            var y2 = ReadFloat(reader);
            var z2 = ReadFloat(reader);
            var v2 = new Vector3(x2, y2, z2);

            var x3 = ReadFloat(reader);
            var y3 = ReadFloat(reader);
            var z3 = ReadFloat(reader);
            var v3 = new Vector3(x3, y3, z3);

            // Try to read color information
            var attrib = Convert.ToString(ReadUInt16(reader), 2).PadLeft(16, '0').ToCharArray();
            var hasColor = attrib[0].Equals('1');
            var currentColor = Color.Transparent;

            if (hasColor)
            {
                var blue = attrib[15].Equals('1') ? 1 : 0;
                blue = attrib[14].Equals('1') ? blue + 2 : blue;
                blue = attrib[13].Equals('1') ? blue + 4 : blue;
                blue = attrib[12].Equals('1') ? blue + 8 : blue;
                blue = attrib[11].Equals('1') ? blue + 16 : blue;
                var b = blue * 8;

                var green = attrib[10].Equals('1') ? 1 : 0;
                green = attrib[9].Equals('1') ? green + 2 : green;
                green = attrib[8].Equals('1') ? green + 4 : green;
                green = attrib[7].Equals('1') ? green + 8 : green;
                green = attrib[6].Equals('1') ? green + 16 : green;
                var g = green * 8;

                var red = attrib[5].Equals('1') ? 1 : 0;
                red = attrib[4].Equals('1') ? red + 2 : red;
                red = attrib[3].Equals('1') ? red + 4 : red;
                red = attrib[2].Equals('1') ? red + 8 : red;
                red = attrib[1].Equals('1') ? red + 16 : red;
                var r = red * 8;

                currentColor = new Color(Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
            }

            var targetSurface = geometry.FirstSurface;

            if (importOptions.IsChangeTriangleOrderNeeded())
            {
                targetSurface.AddTriangle(
                    new VertexBasic(v3, currentColor, Vector2.Zero, normal),
                    new VertexBasic(v2, currentColor, Vector2.Zero, normal),
                    new VertexBasic(v1, currentColor, Vector2.Zero, normal));
            }
            else
            {
                targetSurface.AddTriangle(
                    new VertexBasic(v1, currentColor, Vector2.Zero, normal),
                    new VertexBasic(v2, currentColor, Vector2.Zero, normal),
                    new VertexBasic(v3, currentColor, Vector2.Zero, normal));
            }
        }

        /// <summary>
        /// Reads the model in ASCII format from the specified stream.
        /// </summary>
        private ImportedModelContainer TryReadAscii(ResourceLink source, StlImportOptions importOptions)
        {
            using var stream = source.OpenInputStream();
            using var reader = new StreamReader(stream, s_encoding, false, 128, true);

            var newGeometry = new Geometry();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line == null)
                {
                    continue;
                }

                line = line.Trim();

                if (line.Length == 0 || 
                    line.StartsWith("#") || 
                    line.StartsWith("!") || 
                    line.StartsWith("$"))
                {
                    continue;
                }

                ParseLine(line, out var id, out var values);
                switch (id)
                {
                    // Header.. not needed here
                    case "solid":
                        break;

                    // Geometry data
                    case "facet":
                        this.ReadFacet(reader, values, newGeometry, importOptions);
                        break;

                    // End of file
                    case "endsolid":
                        break;
                }
            }

            // Generate result container
            var modelContainer = new ImportedModelContainer(source, importOptions);
            var resGeometryKey = modelContainer.GetResourceKey(RES_KEY_GEO_CLASS, RES_KEY_GEO_NAME);
            var resMaterialKey = modelContainer.GetResourceKey(RES_KEY_MAT_CLASS, RES_KEY_MAT_NAME);
            modelContainer.AddResource(new ImportedResourceInfo(
                resGeometryKey,
                _ => new GeometryResource(newGeometry)));
            modelContainer.AddResource(new ImportedResourceInfo(
                resMaterialKey,
                _ => new StandardMaterialResource()));
            var loadedMesh = new Mesh(resGeometryKey, resMaterialKey);
            modelContainer.AddObject(loadedMesh);

            // Append an object which transform the whole coordinate system
            modelContainer.FinishLoading(newGeometry.GenerateBoundingBox());

            return modelContainer;
        }

        /// <summary>
        /// Reads the model from the specified binary stream.
        /// </summary>
        private ImportedModelContainer? TryReadBinary(ResourceLink source, StlImportOptions importOptions, bool checkHeader)
        {
            using var stream = source.OpenInputStream();

            // Check length
            var length = stream.Length;
            if (length < 84)
            {
                throw new SeeingSharpException("Incomplete file (smaller that 84 bytes)");
            }

            using var reader = new BinaryReader(stream, Encoding.GetEncoding("us-ascii"), true);

            // Read header (is not needed)
            //  (solid stands for Ascii format)
            var headerBytes = reader.ReadBytes(80);
            if (checkHeader)
            {
                var header = s_encoding.GetString(headerBytes, 0, 80).Trim();
                if ((header.StartsWith("solid", StringComparison.OrdinalIgnoreCase)) &&
                    (header.IndexOf("binary", StringComparison.OrdinalIgnoreCase) == -1))
                {
                    return null;
                }
            }

            // Read and check number of triangles
            var numberTriangles = ReadUInt32(reader);
            if (length - 84 != numberTriangles * 50)
            {
                throw new SeeingSharpException("Incomplete file (smaller that expected byte count)");
            }

            // Read geometry data
            var newGeometry = new Geometry((int) numberTriangles * 3);
            newGeometry.CreateSurface((int) numberTriangles);

            for (var loop = 0; loop < numberTriangles; loop++)
            {
                ReadTriangle(reader, newGeometry, importOptions);
            }

            // Generate result container
            var modelContainer = new ImportedModelContainer(source, importOptions);
            var resGeometryKey = modelContainer.GetResourceKey(RES_KEY_GEO_CLASS, RES_KEY_GEO_NAME);
            var resMaterialKey = modelContainer.GetResourceKey(RES_KEY_MAT_CLASS, RES_KEY_MAT_NAME);
            modelContainer.AddResource(new ImportedResourceInfo(
                resGeometryKey,
                _ => new GeometryResource(newGeometry)));
            modelContainer.AddResource(new ImportedResourceInfo(
                resMaterialKey,
                _ => new StandardMaterialResource()));
            var loadedMesh = new Mesh(resGeometryKey, resMaterialKey);
            modelContainer.AddObject(loadedMesh);

            // Append an object which transform the whole coordinate system
            modelContainer.FinishLoading(newGeometry.GenerateBoundingBox());

            return modelContainer;
        }
    }
}
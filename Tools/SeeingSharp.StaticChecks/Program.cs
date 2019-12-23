using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;

namespace PublicSharpDXChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0) { CheckAssemblies(args); }
            else { CheckAssemblies(Directory.GetFiles(Environment.CurrentDirectory)); }
        }

        private static void CheckAssemblies(IEnumerable<string> assemblyFiles)
        {
            foreach (var actAssemblyFile in assemblyFiles)
            {
                try
                {
                    var myLibrary = AssemblyDefinition.ReadAssembly(actAssemblyFile);

                    foreach (var actModule in myLibrary.Modules)
                    {
                        foreach (var actTypeDefinition in actModule.Types)
                        {
                            CheckType(actTypeDefinition);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while checking assembly {actAssemblyFile}: {ex.Message}");
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static void CheckType(TypeDefinition typeDefinition)
        {
            // Filter out generated types
            foreach (var customAttribute in typeDefinition.CustomAttributes)
            {
                if (customAttribute.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute")
                {
                    return;
                }
            }

            // Check for correct namespace
            if (typeDefinition.Namespace.Contains("._"))
            {
                WriteError($"Error in type {typeDefinition.FullName}: Namespace contains '_'");
            }

            if (typeDefinition.IsPublic || typeDefinition.IsNestedPublic)
            {
                // Check methods
                foreach (var actMethod in typeDefinition.Methods)
                {
                    if (!actMethod.IsPublic) { continue; }

                    if (IsInvalidPublicSharpDXType(actMethod.ReturnType))
                    {
                        WriteError(
                            $"Error in type {typeDefinition.FullName}: Method {actMethod.Name} returns a SharpDX type ({actMethod.ReturnType.FullName})!");
                    }
                    foreach (var actParameter in actMethod.Parameters)
                    {
                        if (IsInvalidPublicSharpDXType(actParameter.ParameterType))
                        {
                            WriteError(
                                $"Error in type {typeDefinition.FullName}: Parameter {actParameter.Name} of method {actMethod.Name} is a SharpDX type ({actParameter.ParameterType.FullName})!");
                        }
                    }
                }

                // Check properties
                foreach (var actProperty in typeDefinition.Properties)
                {
                    var anyPublic = (actProperty.GetMethod?.IsPublic ?? false) ||
                                    (actProperty.SetMethod?.IsPublic ?? false);
                    if (!anyPublic) { continue; }

                    if (IsInvalidPublicSharpDXType(actProperty.PropertyType))
                    {
                        WriteError(
                            $"Error in type {typeDefinition.FullName}: Property {actProperty.Name} has a SharpDX type ({actProperty.PropertyType.FullName})!");
                    }
                }

                // Check fields
                foreach (var actField in typeDefinition.Fields)
                {
                    if (!actField.IsPublic) { continue; }

                    if (IsInvalidPublicSharpDXType(actField.FieldType))
                    {
                        WriteError(
                            $"Error in type {typeDefinition.FullName}: Property {actField.Name} has a SharpDX type ({actField.FieldType.FullName})!");
                    }
                }

                // Perform checking for tested types too
                foreach (var actNestedType in typeDefinition.NestedTypes)
                {
                    if (actNestedType.IsNestedPublic && actNestedType.FullName.EndsWith("Internals"))
                    {
                        continue;
                    }

                    CheckType(actNestedType);
                }
            }
        }

        private static bool IsInvalidPublicSharpDXType(TypeReference typeDefinition)
        {
            return typeDefinition.Namespace.StartsWith("SharpDX");
        }

        private static void WriteError(string error)
        {
            var prevConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(error);
            Console.ForegroundColor = prevConsoleColor;
        }
    }
}

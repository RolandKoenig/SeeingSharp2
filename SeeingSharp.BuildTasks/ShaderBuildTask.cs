#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
#endregion
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;

namespace SeeingSharp.BuildTasks
{
    public class ShaderBuildTask : Task
    {
        /// <summary>
        /// Executes this build task.
        /// </summary>
        /// <returns>
        /// true, wenn die Aufgabe erfolgreich ausgeführt wurde, andernfalls false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                // Check for input files
                if (this.ShaderFiles == null) { return true; }

                // Search the windows sdk path
                string winSdkPath = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Microsoft SDKs\\Windows\\v10.0", "InstallationFolder", null)
                    as string;
                if (string.IsNullOrEmpty(winSdkPath))
                {
                    base.Log.LogError("Unable to compile shader: Windows SDK 10.0 64-Bit not found!");
                    return false;
                }

                // Search fxc.exe
                string fxcPath = Path.Combine(winSdkPath, "bin\\x64\\fxc.exe");
                if (!File.Exists(fxcPath))
                {
                    base.Log.LogError("Unable to compile shader: Fxc.exe not found!");
                    return false;
                }

                // Process each file
                // Continue after each error so that all error are listed in the output window
                bool result = true;
                List<ITaskItem> outputFiles = new List<ITaskItem>();
                foreach (var actShaderFile in this.ShaderFiles)
                {
                    // Check for source file existance
                    if (!File.Exists(actShaderFile.ItemSpec))
                    {
                        base.Log.LogError("Shader file not found: {0}", actShaderFile.ItemSpec);
                        result = false;
                        continue;
                    }

                    // Get output directory (same directory as source file)
                    string outputDirectory = Path.Combine(Path.GetDirectoryName(actShaderFile.ItemSpec), "bin");
                    if(!Directory.Exists(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                    }

                    // Generate output task (pointer to output file) and other output paths
                    string additionalExtension = !string.IsNullOrEmpty(this.OutputAdditionalExtension) ? "." + this.OutputAdditionalExtension : "";
                    string debugExtension = this.AppendDebugInfo ? ".Debug" : string.Empty;
                    string shaderNameWithoutExt = Path.GetFileNameWithoutExtension(actShaderFile.ItemSpec);
                    string outputFilePath = Path.Combine(outputDirectory, shaderNameWithoutExt + additionalExtension + debugExtension + ".cso");
                    string outputInfoFilePath = Path.Combine(outputDirectory, shaderNameWithoutExt + additionalExtension + debugExtension + ".txt");
                    TaskItem actOutput = new TaskItem(outputFilePath);

                    // Configure process call to fxc.exe
                    bool fxcNotifiedErrors = false;
                    using (Process fxcProcess = new Process())
                    {
                        fxcProcess.StartInfo = new ProcessStartInfo(
                            fxcPath,
                            "/T " + this.ShaderProfile + " " +
                            "/Dlowlevel " +
                            "/Fo \"" + outputFilePath + "\" \"" + actShaderFile.ItemSpec + "\" " +
                            "/Fc \"" + outputInfoFilePath + "\" " +
                            (this.AppendDebugInfo ? "/Zi " : ""));
                        fxcProcess.EnableRaisingEvents = true;
                        fxcProcess.StartInfo.StandardErrorEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                        fxcProcess.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
                        fxcProcess.StartInfo.UseShellExecute = false;
                        fxcProcess.StartInfo.CreateNoWindow = true;
                        fxcProcess.StartInfo.RedirectStandardError = true;
                        fxcProcess.StartInfo.RedirectStandardInput = true;
                        fxcProcess.StartInfo.RedirectStandardOutput = true;

                        // Register on error data
                        StringBuilder errorStringBuilder = new StringBuilder();
                        fxcProcess.ErrorDataReceived += (sender, eArgs) =>
                        {
                            if (string.IsNullOrWhiteSpace(eArgs.Data)) { return; }
                            errorStringBuilder.Append(eArgs.Data);

                            if (!eArgs.Data.Contains(" warning "))
                            {
                                fxcNotifiedErrors = true;
                            }
                        };

                        // Start the process and wait for exit
                        fxcProcess.Start();
                        fxcProcess.BeginErrorReadLine();
                        fxcProcess.WaitForExit();
                        fxcProcess.CancelErrorRead();

                        // Check fxc process result
                        if ((fxcProcess.ExitCode != 0) ||
                            (fxcNotifiedErrors))
                        {
                            if (errorStringBuilder.Length > 0)
                            {
                                base.Log.LogError(errorStringBuilder.ToString());
                            }
                            base.Log.LogError(
                                "Error while compiling shader {0}, ResultCode={1}, Profile={2}", 
                                actShaderFile.ItemSpec,
                                fxcProcess.ExitCode,
                                this.ShaderProfile);
                            result = false;
                            break;
                        }
                    }

                    // Append task to output task list if compilation succeeded
                    outputFiles.Add(actOutput);
                }
                this.OutputFiles = outputFiles.ToArray();

                // Return process result
                return result;
            }
            catch (Exception ex)
            {
                base.Log.LogError("Unhandled exception while compiling shaders: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// List containing all input files.
        /// </summary>
        [Required]
        public ITaskItem[] ShaderFiles
        {
            get;
            set;
        }

        /// <summary>
        /// The shader profile to be used.
        /// </summary>
        [Required]
        public string ShaderProfile
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the output without extension.
        /// </summary>
        public string OutputAdditionalExtension
        {
            get;
            set;
        }

        /// <summary>
        /// Append debug info to output file?
        /// </summary>
        public bool AppendDebugInfo
        {
            get;
            set;
        }

        /// <summary>
        /// A list containing all output files.
        /// </summary>
        [Output]
        public ITaskItem[] OutputFiles
        {
            get;
            set;
        }
    }
}

#region License information
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
#endregion

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.Util
{
    #region using
    #endregion

    public static class TestUtilities
    {
        public static async Task InitializeWithGrahicsAsync()
        {
            // Initialize the graphics engine
            if (!GraphicsCore.IsLoaded)
            {
                await GraphicsCore.Loader
                    .SupportWinForms()
                    .LoadAsync();

                GraphicsCore.Current.SetDefaultDeviceToSoftware();
                GraphicsCore.Current.DefaultDevice.ForceDetailLevel(DetailLevel.High);
            }

            Assert.IsTrue(GraphicsCore.IsLoaded, "GraphicsCore could not be initialized!");
        }

        public static AssemblyResourceLink CreateResourceLink(string subfolderName, string fileName)
        {
            return new AssemblyResourceLink(
                Assembly.GetExecutingAssembly(),
                $"SeeingSharp.Tests.Resources.{subfolderName}",
                fileName);
        }

        public static Bitmap LoadBitmapFromResource(string subfolderName, string fileName)
        {
            var resourceLink = CreateResourceLink(subfolderName, fileName);
            using (var inStream = resourceLink.OpenRead())
            {
                return (Bitmap)Image.FromStream(inStream);
            }
        }

        /// <summary>
        /// Writes the given bitmap to the desktop.
        /// </summary>
        /// <param name="bitmap">The bitmap to write to the desktop.</param>
        /// <param name="fileName">The target file name.</param>
        public static void DumpToDesktop(Bitmap bitmap, string fileName)
        {
            var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            bitmap.Save(Path.Combine(desktopDir, fileName));
        }

        public static IDisposable FailTestOnInternalExceptions()
        {
            Exception internalEx = null;
            var location = InternalExceptionLocation.DisposeGraphicsObject;

            EventHandler<InternalCatchedExceptionEventArgs> eventHandler = (sender, e) =>
            {
                if (internalEx == null)
                {
                    internalEx = e.Exception;
                    location = e.Location;
                }
            };

            GraphicsCore.InternalCachedException += eventHandler;
            return new DummyDisposable(() =>
            {
                GraphicsCore.InternalCachedException -= eventHandler;

                Assert.IsTrue(internalEx == null, $"Internal exception at {location}: {internalEx}");
            });
        }
    }
}

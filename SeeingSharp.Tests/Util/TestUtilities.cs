using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Tests.Util
{
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

        public static Bitmap LoadBitmapFromResource(string subfolderName, string fileName)
        {
            var resourceLink = new AssemblyResourceLink(
                Assembly.GetExecutingAssembly(),
                $"SeeingSharp.Tests.Resources.{subfolderName}",
                fileName);
            using (var inStream = resourceLink.OpenRead())
            {
                return (Bitmap)Bitmap.FromStream(inStream);
            }
        }

        /// <summary>
        /// Writes the given bitmap to the desktop.
        /// </summary>
        /// <param name="bitmap">The bitmap to write to the desktop.</param>
        /// <param name="fileName">The target file name.</param>
        public static void DumpToDesktop(Bitmap bitmap, string fileName)
        {
            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            bitmap.Save(Path.Combine(desktopDir, fileName));
        }

        public static IDisposable FailTestOnInternalExceptions()
        {
            Exception internalEx = null;
            InternalExceptionLocation location = InternalExceptionLocation.DisposeGraphicsObject;

            EventHandler<InternalCatchedExceptionEventArgs> eventHandler = (object sender, InternalCatchedExceptionEventArgs e) =>
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

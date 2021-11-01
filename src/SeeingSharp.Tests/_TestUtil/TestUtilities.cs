using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Tests
{
    public static class TestUtilities
    {
        public static async Task InitializeWithGraphicsAsync()
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

        public static D3D11.Device CreateWarpDevice()
        {
            return new D3D11.Device(DriverType.Warp, D3D11.DeviceCreationFlags.BgraSupport);
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

        public static void DumpToDesktop(RenderPassDump dumpedRenderPipeline)
        {
            foreach (var actDumpEntry in dumpedRenderPipeline.DumpResults)
            {
                using var actDumpColorBitmap =
                    GraphicsHelperWinForms.LoadBitmapFromMemoryMappedTexture(actDumpEntry.BufferColor);
                DumpToDesktop(actDumpColorBitmap, $"{actDumpEntry.Key}.png");
            }
        }

        public static IDisposable FailTestOnInternalExceptions()
        {
            Exception internalEx = null;
            var location = InternalExceptionLocation.DisposeGraphicsObject;

            void EventHandler(object sender, InternalCatchedExceptionEventArgs e)
            {
                if (internalEx == null)
                {
                    internalEx = e.Exception;
                    location = e.Location;
                }
            }

            GraphicsCore.InternalCatchedException += EventHandler;
            return new DummyDisposable(() =>
            {
                GraphicsCore.InternalCatchedException -= EventHandler;

                Assert.IsTrue(internalEx == null, $"Internal exception at {location}: {internalEx}");
            });
        }
    }
}

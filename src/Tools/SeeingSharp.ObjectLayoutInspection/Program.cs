using ObjectLayoutInspector;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.ObjectLayoutInspection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TypeLayout.PrintLayout<VertexBasic>();
            //TypeLayout.PrintLayout<VertexBinormalTangent>();

            //TypeLayout.PrintLayout<CBPerFrame>();
            //TypeLayout.PrintLayout<CBPerView>();
            //TypeLayout.PrintLayout<CBPerObject>();
            //TypeLayout.PrintLayout<CBPerMaterial>();

            TypeLayout.PrintLayout<CBPerObject>();

        }
    }
}

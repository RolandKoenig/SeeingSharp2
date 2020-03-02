using System;
using ObjectLayoutInspector;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.ObjectLayoutInspection
{
    class Program
    {
        static void Main(string[] args)
        {
            TypeLayout.PrintLayout<VertexBasic>();
            TypeLayout.PrintLayout<VertexBinormalTangent>();

            //TypeLayout.PrintLayout<CBPerFrame>();
            //TypeLayout.PrintLayout<CBPerView>();
            //TypeLayout.PrintLayout<CBPerObject>();
            //TypeLayout.PrintLayout<CBPerMaterial>();
        }
    }
}

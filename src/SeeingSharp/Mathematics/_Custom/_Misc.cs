namespace SeeingSharp.Mathematics
{
    public enum EdgeOrder
    {
        Unknown,

        Clockwise,

        CounterClockwise
    }

    public struct Polygon2DMergeOptions
    {
        public static readonly Polygon2DMergeOptions DEFAULT = new Polygon2DMergeOptions();

        public bool MakeMergepointSpaceForTriangulation;
    }
}

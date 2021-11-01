namespace SeeingSharp
{
    public partial struct Color
    {
        public static Color FromArgb(byte a, byte r, byte g, byte b)
        {
            return new Color(r, g, b, a);
        }

        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((byte)r, (byte)g, (byte)b, (byte)a);
        }
    }
}

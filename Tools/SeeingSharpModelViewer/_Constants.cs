#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# Tools. More info at
     - https://github.com/RolandKoenig/SeeingSharp/tree/master/Tools (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)

	This program is distributed under the terms of the Microsoft Public License (Ms-PL)-
	More info at https://msdn.microsoft.com/en-us/library/ff647676.aspx
*/
#endregion License information (SeeingSharp and all based games/applications)

namespace SeeingSharpModelViewer
{
    internal static class Constants
    {
        // Floor tiles
        public const int COUNT_TILES_MIN = 16;
        public const int COUNT_TILES_MAX = 128;

        // Graphics Layer names
        public const string LAYER_BACKGROUND = "Background";
        public const string LAYER_BACKGROUND_FLAT = "BackgroundFlat";
        public const string LAYER_HOVER = "HoveredObjects";
        public const string LAYER_SELECTION = "SelectedObjects";
    }
}
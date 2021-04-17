/*
    SeeingSharp and all applications distributed together with it. 
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
namespace SeeingSharp
{
    public partial struct Color4
    {
        /// <summary>
        /// Transparent color
        /// </summary>
        public static Color4 Transparent => new Color4(0.0f, 0.0f, 0.0f, 0.0f);

        public static Color4 Empty => Transparent;

        /// <summary>
        /// AliceBlue color
        /// </summary>
        public static Color4 AliceBlue => new Color4(0.941176470588235f, 0.972549019607843f, 1f, 1f);

        /// <summary>
        /// AntiqueWhite color
        /// </summary>
        public static Color4 AntiqueWhite => new Color4(0.980392156862745f, 0.92156862745098f, 0.843137254901961f, 1f);

        /// <summary>
        /// Aqua color
        /// </summary>
        public static Color4 Aqua => new Color4(0f, 1f, 1f, 1f);

        /// <summary>
        /// Aquamarine color
        /// </summary>
        public static Color4 Aquamarine => new Color4(0.498039215686275f, 1f, 0.831372549019608f, 1f);

        /// <summary>
        /// Azure color
        /// </summary>
        public static Color4 Azure => new Color4(0.941176470588235f, 1f, 1f, 1f);

        /// <summary>
        /// Beige color
        /// </summary>
        public static Color4 Beige => new Color4(0.96078431372549f, 0.96078431372549f, 0.862745098039216f, 1f);

        /// <summary>
        /// Bisque color
        /// </summary>
        public static Color4 Bisque => new Color4(1, 0.894117647058824f, 0.768627450980392f, 1f);

        /// <summary>
        /// BlanchedAlmond color
        /// </summary>
        public static Color4 BlanchedAlmond => new Color4(1f, 0.92156862745098f, 0.803921568627451f, 1f);

        /// <summary>
        /// Blue color
        /// </summary>
        public static Color4 BlueColor => new Color4(0f, 0f, 1f, 1f);

        /// <summary>
        /// BlueViolet color
        /// </summary>
        public static Color4 BlueViolet => new Color4(0.541176470588235f, 0.168627450980392f, 0.886274509803922f, 1f);

        /// <summary>
        /// Brown color
        /// </summary>
        public static Color4 Brown => new Color4(0.647058823529412f, 0.164705882352941f, 0.164705882352941f, 1f);

        /// <summary>
        /// BurlyWood color
        /// </summary>
        public static Color4 BurlyWood => new Color4(0.870588235294118f, 0.72156862745098f, 0.529411764705882f, 1f);

        /// <summary>
        /// CadetBlue color
        /// </summary>
        public static Color4 CadetBlue => new Color4(0.372549019607843f, 0.619607843137255f, 0.627450980392157f, 1f);

        /// <summary>
        /// Chartreuse color
        /// </summary>
        public static Color4 Chartreuse => new Color4(0.498039215686275f, 1f, 0f, 1f);

        /// <summary>
        /// Chocolate color
        /// </summary>
        public static Color4 Chocolate => new Color4(0.823529411764706f, 0.411764705882353f, 0.117647058823529f, 1f);

        /// <summary>
        /// Coral color
        /// </summary>
        public static Color4 Coral => new Color4(1f, 0.498039215686275f, 0.313725490196078f, 1f);

        /// <summary>
        /// CornflowerBlue color
        /// </summary>
        public static Color4 CornflowerBlue => new Color4(0.392156862745098f, 0.584313725490196f, 0.929411764705882f, 1f);

        /// <summary>
        /// Cornsilk color
        /// </summary>
        public static Color4 Cornsilk => new Color4(1f, 0.972549019607843f, 0.862745098039216f, 1f);

        /// <summary>
        /// Crimson color
        /// </summary>
        public static Color4 Crimson => new Color4(0.862745098039216f, 0.0784313725490196f, 0.235294117647059f, 1f);

        /// <summary>
        /// Cyan color
        /// </summary>
        public static Color4 Cyan => new Color4(0f, 1f, 1f, 1f);

        /// <summary>
        /// DarkBlue color
        /// </summary>
        public static Color4 DarkBlue => new Color4(0f, 0f, 0.545098039215686f, 1f);

        /// <summary>
        /// DarkCyan color
        /// </summary>
        public static Color4 DarkCyan => new Color4(0f, 0.545098039215686f, 0.545098039215686f, 1f);

        /// <summary>
        /// DarkGoldenrod color
        /// </summary>
        public static Color4 DarkGoldenrod => new Color4(0.72156862745098f, 0.525490196078431f, 0.0431372549019608f, 1f);

        /// <summary>
        /// DarkGray color
        /// </summary>
        public static Color4 DarkGray => new Color4(0.662745098039216f, 0.662745098039216f, 0.662745098039216f, 1f);

        /// <summary>
        /// DarkGreen color
        /// </summary>
        public static Color4 DarkGreen => new Color4(0f, 0.392156862745098f, 0f, 1f);

        /// <summary>
        /// DarkKhaki color
        /// </summary>
        public static Color4 DarkKhaki => new Color4(0.741176470588235f, 0.717647058823529f, 0.419607843137255f, 1f);

        /// <summary>
        /// DarkMagenta color
        /// </summary>
        public static Color4 DarkMagenta => new Color4(0.545098039215686f, 0f, 0.545098039215686f, 1f);

        /// <summary>
        /// DarkOliveGreen color
        /// </summary>
        public static Color4 DarkOliveGreen => new Color4(0.333333333333333f, 0.419607843137255f, 0.184313725490196f, 1f);

        /// <summary>
        /// DarkOrange color
        /// </summary>
        public static Color4 DarkOrange => new Color4(1f, 0.549019607843137f, 0f, 1f);

        /// <summary>
        /// DarkOrchid color
        /// </summary>
        public static Color4 DarkOrchid => new Color4(0.6f, 0.196078431372549f, 0.8f, 1f);

        /// <summary>
        /// DarkRed color
        /// </summary>
        public static Color4 DarkRed => new Color4(0.545098039215686f, 0f, 0f, 1f);

        /// <summary>
        /// DarkSalmon color
        /// </summary>
        public static Color4 DarkSalmon => new Color4(0.913725490196078f, 0.588235294117647f, 0.47843137254902f, 1f);

        /// <summary>
        /// DarkSeaGreen color
        /// </summary>
        public static Color4 DarkSeaGreen => new Color4(0.56078431372549f, 0.737254901960784f, 0.56078431372549f, 1f);

        /// <summary>
        /// DarkSlateBlue color
        /// </summary>
        public static Color4 DarkSlateBlue => new Color4(0.282352941176471f, 0.23921568627451f, 0.545098039215686f, 1f);

        /// <summary>
        /// DarkSlateGray color
        /// </summary>
        public static Color4 DarkSlateGray => new Color4(0.184313725490196f, 0.309803921568627f, 0.309803921568627f, 1f);

        /// <summary>
        /// DarkTurquoise color
        /// </summary>
        public static Color4 DarkTurquoise => new Color4(0f, 0.807843137254902f, 0.819607843137255f, 1f);

        /// <summary>
        /// DarkViolet color
        /// </summary>
        public static Color4 DarkViolet => new Color4(0.580392156862745f, 0f, 0.827450980392157f, 1f);

        /// <summary>
        /// DeepPink color
        /// </summary>
        public static Color4 DeepPink => new Color4(1f, 0.0784313725490196f, 0.576470588235294f, 1f);

        /// <summary>
        /// DeepSkyBlue color
        /// </summary>
        public static Color4 DeepSkyBlue => new Color4(0f, 0.749019607843137f, 1f, 1f);

        /// <summary>
        /// DimGray color
        /// </summary>
        public static Color4 DimGray => new Color4(0.411764705882353f, 0.411764705882353f, 0.411764705882353f, 1f);

        /// <summary>
        /// DodgerBlue color
        /// </summary>
        public static Color4 DodgerBlue => new Color4(0.117647058823529f, 0.564705882352941f, 1f, 1f);

        /// <summary>
        /// Firebrick color
        /// </summary>
        public static Color4 Firebrick => new Color4(0.698039215686274f, 0.133333333333333f, 0.133333333333333f, 1f);

        /// <summary>
        /// FloralWhite color
        /// </summary>
        public static Color4 FloralWhite => new Color4(1f, 0.980392156862745f, 0.941176470588235f, 1f);

        /// <summary>
        /// ForestGreen color
        /// </summary>
        public static Color4 ForestGreen => new Color4(0.133333333333333f, 0.545098039215686f, 0.133333333333333f, 1f);

        /// <summary>
        /// Fuchsia color
        /// </summary>
        public static Color4 Fuchsia => new Color4(1f, 0f, 1f, 1f);

        /// <summary>
        /// Gainsboro color
        /// </summary>
        public static Color4 Gainsboro => new Color4(0.862745098039216f, 0.862745098039216f, 0.862745098039216f, 1f);

        /// <summary>
        /// GhostWhite color
        /// </summary>
        public static Color4 GhostWhite => new Color4(0.972549019607843f, 0.972549019607843f, 1f, 1f);

        /// <summary>
        /// Gold color
        /// </summary>
        public static Color4 Gold => new Color4(1f, 0.843137254901961f, 0f, 1f);

        /// <summary>
        /// Goldenrod color
        /// </summary>
        public static Color4 Goldenrod => new Color4(0.854901960784314f, 0.647058823529412f, 0.125490196078431f, 1f);

        /// <summary>
        /// Gray color
        /// </summary>
        public static Color4 Gray => new Color4(0.501960784313725f, 0.501960784313725f, 0.501960784313725f, 1f);

        /// <summary>
        /// Green color
        /// </summary>
        public static Color4 GreenColor => new Color4(0f, 0.501960784313725f, 0f, 1f);

        /// <summary>
        /// GreenYellow color
        /// </summary>
        public static Color4 GreenYellow => new Color4(0.67843137254902f, 1f, 0.184313725490196f, 1f);

        /// <summary>
        /// Honeydew color
        /// </summary>
        public static Color4 Honeydew => new Color4(0.941176470588235f, 1f, 0.941176470588235f, 1f);

        /// <summary>
        /// HotPink color
        /// </summary>
        public static Color4 HotPink => new Color4(1f, 0.411764705882353f, 0.705882352941177f, 1f);

        /// <summary>
        /// IndianRed color
        /// </summary>
        public static Color4 IndianRed => new Color4(0.803921568627451f, 0.36078431372549f, 0.36078431372549f, 1f);

        /// <summary>
        /// Indigo color
        /// </summary>
        public static Color4 Indigo => new Color4(0.294117647058824f, 0f, 0.509803921568627f, 1f);

        /// <summary>
        /// Ivory color
        /// </summary>
        public static Color4 Ivory => new Color4(1f, 1f, 0.941176470588235f, 1f);

        /// <summary>
        /// Khaki color
        /// </summary>
        public static Color4 Khaki => new Color4(0.941176470588235f, 0.901960784313726f, 0.549019607843137f, 1f);

        /// <summary>
        /// Lavender color
        /// </summary>
        public static Color4 Lavender => new Color4(0.901960784313726f, 0.901960784313726f, 0.980392156862745f, 1f);

        /// <summary>
        /// LavenderBlush color
        /// </summary>
        public static Color4 LavenderBlush => new Color4(1f, 0.941176470588235f, 0.96078431372549f, 1f);

        /// <summary>
        /// LawnGreen color
        /// </summary>
        public static Color4 LawnGreen => new Color4(0.486274509803922f, 0.988235294117647f, 0f, 1f);

        /// <summary>
        /// LemonChiffon color
        /// </summary>
        public static Color4 LemonChiffon => new Color4(1f, 0.980392156862745f, 0.803921568627451f, 1f);

        /// <summary>
        /// LightBlue color
        /// </summary>
        public static Color4 LightBlue => new Color4(0.67843137254902f, 0.847058823529412f, 0.901960784313726f, 1f);

        /// <summary>
        /// LightCoral color
        /// </summary>
        public static Color4 LightCoral => new Color4(0.941176470588235f, 0.501960784313725f, 0.501960784313725f, 1f);

        /// <summary>
        /// LightCyan color
        /// </summary>
        public static Color4 LightCyan => new Color4(0.87843137254902f, 1f, 1f, 1f);

        /// <summary>
        /// LightGoldenrodYellow color
        /// </summary>
        public static Color4 LightGoldenrodYellow => new Color4(0.980392156862745f, 0.980392156862745f, 0.823529411764706f, 1f);

        /// <summary>
        /// LightGray color
        /// </summary>
        public static Color4 LightGray => new Color4(0.827450980392157f, 0.827450980392157f, 0.827450980392157f, 1f);

        /// <summary>
        /// LightGreen color
        /// </summary>
        public static Color4 LightGreen => new Color4(0.564705882352941f, 0.933333333333333f, 0.564705882352941f, 1f);

        /// <summary>
        /// LightPink color
        /// </summary>
        public static Color4 LightPink => new Color4(1f, 0.713725490196078f, 0.756862745098039f, 1f);

        /// <summary>
        /// LightSalmon color
        /// </summary>
        public static Color4 LightSalmon => new Color4(1f, 0.627450980392157f, 0.47843137254902f, 1f);

        /// <summary>
        /// LightSeaGreen color
        /// </summary>
        public static Color4 LightSeaGreen => new Color4(0.125490196078431f, 0.698039215686274f, 0.666666666666667f, 1f);

        /// <summary>
        /// LightSkyBlue color
        /// </summary>
        public static Color4 LightSkyBlue => new Color4(0.529411764705882f, 0.807843137254902f, 0.980392156862745f, 1f);

        /// <summary>
        /// LightSlateGray color
        /// </summary>
        public static Color4 LightSlateGray => new Color4(0.466666666666667f, 0.533333333333333f, 0.6f, 1f);

        /// <summary>
        /// LightSteelBlue color
        /// </summary>
        public static Color4 LightSteelBlue => new Color4(0.690196078431373f, 0.768627450980392f, 0.870588235294118f, 1f);

        /// <summary>
        /// LightYellow color
        /// </summary>
        public static Color4 LightYellow => new Color4(1f, 1f, 0.87843137254902f, 1f);

        /// <summary>
        /// Lime color
        /// </summary>
        public static Color4 Lime => new Color4(0f, 1f, 0f, 1f);

        /// <summary>
        /// LimeGreen color
        /// </summary>
        public static Color4 LimeGreen => new Color4(0.196078431372549f, 0.803921568627451f, 0.196078431372549f, 1f);

        /// <summary>
        /// Linen color
        /// </summary>
        public static Color4 Linen => new Color4(0.980392156862745f, 0.941176470588235f, 0.901960784313726f, 1f);

        /// <summary>
        /// Magenta color
        /// </summary>
        public static Color4 Magenta => new Color4(1f, 0f, 1f, 1f);

        /// <summary>
        /// Maroon color
        /// </summary>
        public static Color4 Maroon => new Color4(0.501960784313725f, 0f, 0f, 1f);

        /// <summary>
        /// MediumAquamarine color
        /// </summary>
        public static Color4 MediumAquamarine => new Color4(0.4f, 0.803921568627451f, 0.666666666666667f, 1f);

        /// <summary>
        /// MediumBlue color
        /// </summary>
        public static Color4 MediumBlue => new Color4(0f, 0f, 0.803921568627451f, 1f);

        /// <summary>
        /// MediumOrchid color
        /// </summary>
        public static Color4 MediumOrchid => new Color4(0.729411764705882f, 0.333333333333333f, 0.827450980392157f, 1f);

        /// <summary>
        /// MediumPurple color
        /// </summary>
        public static Color4 MediumPurple => new Color4(0.576470588235294f, 0.43921568627451f, 0.858823529411765f, 1f);

        /// <summary>
        /// MediumSeaGreen color
        /// </summary>
        public static Color4 MediumSeaGreen => new Color4(0.235294117647059f, 0.701960784313725f, 0.443137254901961f, 1f);

        /// <summary>
        /// MediumSlateBlue color
        /// </summary>
        public static Color4 MediumSlateBlue => new Color4(0.482352941176471f, 0.407843137254902f, 0.933333333333333f, 1f);

        /// <summary>
        /// MediumSpringGreen color
        /// </summary>
        public static Color4 MediumSpringGreen => new Color4(0f, 0.980392156862745f, 0.603921568627451f, 1f);

        /// <summary>
        /// MediumTurquoise color
        /// </summary>
        public static Color4 MediumTurquoise => new Color4(0.282352941176471f, 0.819607843137255f, 0.8f, 1f);

        /// <summary>
        /// MediumVioletRed color
        /// </summary>
        public static Color4 MediumVioletRed => new Color4(0.780392156862745f, 0.0823529411764706f, 0.52156862745098f, 1f);

        /// <summary>
        /// MidnightBlue color
        /// </summary>
        public static Color4 MidnightBlue => new Color4(0.0980392156862745f, 0.0980392156862745f, 0.43921568627451f, 1f);

        /// <summary>
        /// MintCream color
        /// </summary>
        public static Color4 MintCream => new Color4(0.96078431372549f, 1f, 0.980392156862745f, 1f);

        /// <summary>
        /// MistyRose color
        /// </summary>
        public static Color4 MistyRose => new Color4(1f, 0.894117647058824f, 0.882352941176471f, 1f);

        /// <summary>
        /// Moccasin color
        /// </summary>
        public static Color4 Moccasin => new Color4(1f, 0.894117647058824f, 0.709803921568627f, 1f);

        /// <summary>
        /// NavajoWhite color
        /// </summary>
        public static Color4 NavajoWhite => new Color4(1f, 0.870588235294118f, 0.67843137254902f, 1f);

        /// <summary>
        /// Navy color
        /// </summary>
        public static Color4 Navy => new Color4(0f, 0f, 0.501960784313725f, 1f);

        /// <summary>
        /// OldLace color
        /// </summary>
        public static Color4 OldLace => new Color4(0.992156862745098f, 0.96078431372549f, 0.901960784313726f, 1f);

        /// <summary>
        /// Olive color
        /// </summary>
        public static Color4 Olive => new Color4(0.501960784313725f, 0.501960784313725f, 0f, 1f);

        /// <summary>
        /// OliveDrab color
        /// </summary>
        public static Color4 OliveDrab => new Color4(0.419607843137255f, 0.556862745098039f, 0.137254901960784f, 1f);

        /// <summary>
        /// Orange color
        /// </summary>
        public static Color4 Orange => new Color4(1f, 0.647058823529412f, 0f, 1f);

        /// <summary>
        /// OrangeRed color
        /// </summary>
        public static Color4 OrangeRed => new Color4(1f, 0.270588235294118f, 0f, 1f);

        /// <summary>
        /// Orchid color
        /// </summary>
        public static Color4 Orchid => new Color4(0.854901960784314f, 0.43921568627451f, 0.83921568627451f, 1f);

        /// <summary>
        /// PaleGoldenrod color
        /// </summary>
        public static Color4 PaleGoldenrod => new Color4(0.933333333333333f, 0.909803921568627f, 0.666666666666667f, 1f);

        /// <summary>
        /// PaleGreen color
        /// </summary>
        public static Color4 PaleGreen => new Color4(0.596078431372549f, 0.984313725490196f, 0.596078431372549f, 1f);

        /// <summary>
        /// PaleTurquoise color
        /// </summary>
        public static Color4 PaleTurquoise => new Color4(0.686274509803922f, 0.933333333333333f, 0.933333333333333f, 1f);

        /// <summary>
        /// PaleVioletRed color
        /// </summary>
        public static Color4 PaleVioletRed => new Color4(0.858823529411765f, 0.43921568627451f, 0.576470588235294f, 1f);

        /// <summary>
        /// PapayaWhip color
        /// </summary>
        public static Color4 PapayaWhip => new Color4(1f, 0.937254901960784f, 0.835294117647059f, 1f);

        /// <summary>
        /// PeachPuff color
        /// </summary>
        public static Color4 PeachPuff => new Color4(1f, 0.854901960784314f, 0.725490196078431f, 1f);

        /// <summary>
        /// Peru color
        /// </summary>
        public static Color4 Peru => new Color4(0.803921568627451f, 0.52156862745098f, 0.247058823529412f, 1f);

        /// <summary>
        /// Pink color
        /// </summary>
        public static Color4 Pink => new Color4(1f, 0.752941176470588f, 0.796078431372549f, 1f);

        /// <summary>
        /// Plum color
        /// </summary>
        public static Color4 Plum => new Color4(0.866666666666667f, 0.627450980392157f, 0.866666666666667f, 1f);

        /// <summary>
        /// PowderBlue color
        /// </summary>
        public static Color4 PowderBlue => new Color4(0.690196078431373f, 0.87843137254902f, 0.901960784313726f, 1f);

        /// <summary>
        /// Purple color
        /// </summary>
        public static Color4 Purple => new Color4(0.501960784313725f, 0f, 0.501960784313725f, 1f);

        /// <summary>
        /// Red color
        /// </summary>
        public static Color4 RedColor => new Color4(1f, 0f, 0f, 1f);

        /// <summary>
        /// RosyBrown color
        /// </summary>
        public static Color4 RosyBrown => new Color4(0.737254901960784f, 0.56078431372549f, 0.56078431372549f, 1f);

        /// <summary>
        /// RoyalBlue color
        /// </summary>
        public static Color4 RoyalBlue => new Color4(0.254901960784314f, 0.411764705882353f, 0.882352941176471f, 1f);

        /// <summary>
        /// SaddleBrown color
        /// </summary>
        public static Color4 SaddleBrown => new Color4(0.545098039215686f, 0.270588235294118f, 0.0745098039215686f, 1f);

        /// <summary>
        /// Salmon color
        /// </summary>
        public static Color4 Salmon => new Color4(0.980392156862745f, 0.501960784313725f, 0.447058823529412f, 1f);

        /// <summary>
        /// SandyBrown color
        /// </summary>
        public static Color4 SandyBrown => new Color4(0.956862745098039f, 0.643137254901961f, 0.376470588235294f, 1f);

        /// <summary>
        /// SeaGreen color
        /// </summary>
        public static Color4 SeaGreen => new Color4(0.180392156862745f, 0.545098039215686f, 0.341176470588235f, 1f);

        /// <summary>
        /// SeaShell color
        /// </summary>
        public static Color4 SeaShell => new Color4(1f, 0.96078431372549f, 0.933333333333333f, 1f);

        /// <summary>
        /// Sienna color
        /// </summary>
        public static Color4 Sienna => new Color4(0.627450980392157f, 0.32156862745098f, 0.176470588235294f, 1f);

        /// <summary>
        /// Silver color
        /// </summary>
        public static Color4 Silver => new Color4(0.752941176470588f, 0.752941176470588f, 0.752941176470588f, 1f);

        /// <summary>
        /// SkyBlue color
        /// </summary>
        public static Color4 SkyBlue => new Color4(0.529411764705882f, 0.807843137254902f, 0.92156862745098f, 1f);

        /// <summary>
        /// SlateBlue color
        /// </summary>
        public static Color4 SlateBlue => new Color4(0.415686274509804f, 0.352941176470588f, 0.803921568627451f, 1f);

        /// <summary>
        /// SlateGray color
        /// </summary>
        public static Color4 SlateGray => new Color4(0.43921568627451f, 0.501960784313725f, 0.564705882352941f, 1f);

        /// <summary>
        /// Snow color
        /// </summary>
        public static Color4 Snow => new Color4(1f, 0.980392156862745f, 0.980392156862745f, 1f);

        /// <summary>
        /// SpringGreen color
        /// </summary>
        public static Color4 SpringGreen => new Color4(0f, 1f, 0.498039215686275f, 1f);

        /// <summary>
        /// SteelBlue color
        /// </summary>
        public static Color4 SteelBlue => new Color4(0.274509803921569f, 0.509803921568627f, 0.705882352941177f, 1f);

        /// <summary>
        /// Tan color
        /// </summary>
        public static Color4 Tan => new Color4(0.823529411764706f, 0.705882352941177f, 0.549019607843137f, 1f);

        /// <summary>
        /// Teal color
        /// </summary>
        public static Color4 Teal => new Color4(0f, 0.501960784313725f, 0.501960784313725f, 1f);

        /// <summary>
        /// Thistle color
        /// </summary>
        public static Color4 Thistle => new Color4(0.847058823529412f, 0.749019607843137f, 0.847058823529412f, 1f);

        /// <summary>
        /// Tomato color
        /// </summary>
        public static Color4 Tomato => new Color4(1f, 0.388235294117647f, 0.27843137254902f, 1f);

        /// <summary>
        /// Turquoise color
        /// </summary>
        public static Color4 Turquoise => new Color4(0.250980392156863f, 0.87843137254902f, 0.815686274509804f, 1f);

        /// <summary>
        /// Violet color
        /// </summary>
        public static Color4 Violet => new Color4(0.933333333333333f, 0.509803921568627f, 0.933333333333333f, 1f);

        /// <summary>
        /// Wheat color
        /// </summary>
        public static Color4 Wheat => new Color4(0.96078431372549f, 0.870588235294118f, 0.701960784313725f, 1f);

        /// <summary>
        /// WhiteSmoke color
        /// </summary>
        public static Color4 WhiteSmoke => new Color4(0.96078431372549f, 0.96078431372549f, 0.96078431372549f, 1f);

        /// <summary>
        /// Yellow color
        /// </summary>
        public static Color4 Yellow => new Color4(1f, 1f, 0f, 1f);

        /// <summary>
        /// YellowGreen color
        /// </summary>
        public static Color4 YellowGreen => new Color4(0.603921568627451f, 0.803921568627451f, 0.196078431372549f, 1f);
    }
}

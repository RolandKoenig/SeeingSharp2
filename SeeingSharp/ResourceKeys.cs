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
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia
{
    public static class ResourceKeys
    {
        public static readonly NamedOrGenericKey RES_SKYBOX_VERTICES = new NamedOrGenericKey(
            "ResourceKeys.RES_SKYBOX_VERTICES");
        public static readonly NamedOrGenericKey RES_SKYBOX_INDICES = new NamedOrGenericKey(
            "ResourceKeys.RES_SKYBOX_INDICES");
        public static readonly NamedOrGenericKey RES_SKYBOX_VERTEX_SHADER = new NamedOrGenericKey(
            "ResourceKeys.RES_SKYBOX_VERTEX_SHADER");
        public static readonly NamedOrGenericKey RES_SKYBOX_PIXEL_SHADER = new NamedOrGenericKey(
            "ResourceKeys.RES_SKYBOX_PIXEL_SHADER");

        public static readonly NamedOrGenericKey RES_MATERIAL_COLORED = new NamedOrGenericKey(
            $"{nameof(ResourceKeys)}.{nameof(RES_MATERIAL_COLORED)}");
    }
}
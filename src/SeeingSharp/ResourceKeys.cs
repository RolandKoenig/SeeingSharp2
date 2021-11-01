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
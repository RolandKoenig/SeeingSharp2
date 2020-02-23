// This file contains all default constant buffers used across all
// object rendered with this graphics engine

// Reference of scalar types: http://msdn.microsoft.com/en-us/library/windows/desktop/bb509646(v=vs.85).aspx
// ConstantBuffer separation see "2010 DirectX 10 Performance.pdf"

// Textures
Texture2D     ObjectTexture        : register( t0 );
TextureCube   ObjectCubeTexture    : register( t0 );
SamplerState  ObjectTextureSampler : register( s0 );

// Buffer with per frame data
cbuffer cbPerFrame : register( b0 )
{
    int         Time;
    float3		Dummy1;
}

// Buffer with per view data
cbuffer cbPerView : register ( b1 )
{
    float4x4    ViewProj;
	float4x4    View;
    float3      CameraPosition;
    float       BorderFactor;
    float       LightPower;
    float       StrongLightFactor;
    float       Ambient;
    float       Accentuation;
    float2      ScreenPixelSize;
	float2      Dummy2;
}

// Buffer with per object data
cbuffer cbPerObject : register ( b2 )
{
    float4x4    World;
    float4      ObjectColor;
    float       Opacity;
    float       ObjectAccentuationFactor;
	float3      ObjectScaling;
    float 		SpriteScaling;
    float2      Dummy3;
}

// Buffer with per material data
cbuffer cbPerMaterial : register ( b3 )
{
	float4    MaterialDiffuseColor;
	float4    TextureReciprocal;
	float     Texture0Factor;
	float     MaxClipDistance;
	float     ClipFactor;
	float     AdjustTextureCoordinatesFactor;
	float     AddToAlpha;    // Needed for video rendering (Frames from the MF SourceReader have alpha always to zero)
	float     FadeIntensity;
	float     DiffuseColorFactor;
	float     BorderPart;
	float     BorderMultiplyer;
    float3    Dummy4;
}
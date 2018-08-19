// This file contains all default structures used across all
// objects rendered with this graphics engine

//----------------------------------------------------------------------------- Standard rendering
// VertexShader input
struct VSInputStandard
{
    float3 pos     : POSITION;
    float3 normal  : NORMAL0;
    float4 col     : COLOR0;
    float2 tex     : TEXCOORD0;
    float  texFact : TEXCOORD1;
};

// PixelShader input
struct PSInputStandard
{
    float4 pos     : SV_POSITION;
    float4 col     : COLOR0;
    float2 tex     : TEXCOORD0;
    float3 normal  : TEXCOORD1;
    float3 pos3D   : TEXCOORD2;
	float  texFact : TEXCOORD3;
};

// PixelShader output
struct PSOutput
{
	float4 color       : SV_Target0;
	float4 normalDepth : SV_Target1;
};

//----------------------------------------------------------------------------- Line rendering
//VertexShader input
struct VSInputLine
{
    float3 pos    : POSITION;
};

//PixelShader input
struct PSInputLine
{
    float4 pos    : SV_POSITION;
};
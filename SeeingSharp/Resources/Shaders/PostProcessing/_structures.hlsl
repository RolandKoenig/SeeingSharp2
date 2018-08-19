// This file contains all default structures used across all
// objects rendered with this graphics engine

//----------------------------------------------------------------------------- Standard rendering
//VertexShader output
struct PSInputPostprocess
{
	float4 pos  : SV_Position;
	float2 tex  : TexCoord;
};
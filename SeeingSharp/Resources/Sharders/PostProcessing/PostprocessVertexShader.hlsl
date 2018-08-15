//-----------------------------------------------------------------------------
//Include all common items
#include "_structures.hlsl"

// Very simple VertexShader that builds a fullscreen quad without any bound VertexBuffer
// see https://www.indiedev.de/wiki/Vier_Methoden_zur_Erzeugung_eines_Fullscreen_Quad
PSInputPostprocess main(uint VertexID: SV_VertexID)
{
	PSInputPostprocess output;
	output.tex = float2((VertexID << 1) & 2, VertexID & 2);
	output.pos = float4((output.tex * float2(2, -2) + float2(-1, 1)), 0, 1);
	return output;
}
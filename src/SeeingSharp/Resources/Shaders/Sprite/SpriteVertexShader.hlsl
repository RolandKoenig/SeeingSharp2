//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//VertexShader implementation
PSInputStandard main(VSInputStandard input) 
{
	PSInputStandard output = (PSInputStandard)0;
	
	output.pos = float4(input.pos.xyz * SpriteScaling, 1.0);
	output.col = input.col;
	output.tex = input.tex;
	output.normal = input.normal;
	output.pos3D = output.pos.xyz;

	return output;
}
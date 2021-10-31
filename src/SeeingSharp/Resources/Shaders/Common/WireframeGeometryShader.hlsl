//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//GeometryShader implementation
[maxvertexcount(6)]
void main(triangle PSInputStandard input[3], inout LineStream<PSInputStandard> lineStream)
{
	lineStream.Append(input[0]);
	lineStream.Append(input[1]);

	lineStream.Append(input[1]);
	lineStream.Append(input[2]);

	lineStream.Append(input[2]);
	lineStream.Append(input[0]);
}
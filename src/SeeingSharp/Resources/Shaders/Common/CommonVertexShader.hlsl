//-----------------------------------------------------------------------------
//Include all common items
#include "../_mainInclude.hlsl"

//-----------------------------------------------------------------------------
//VertexShader implementation
PSInputStandard main(VSInputStandard input) 
{
    PSInputStandard output = (PSInputStandard)0;

    // Calculate standard values (position and normal)
	float4x4 localWorldViewProj = mul(World, ViewProj);
    output.pos = mul(float4(input.pos.xyz, 1.0), localWorldViewProj);
    output.normal = normalize(mul(input.normal, (float3x3)World));

	// Interpolate texture coordinate based on xy-scaling
	if(AdjustTextureCoordinatesFactor > 0.0)
	{
		output.tex.xy = abs(input.tex.xy * (ObjectScaling.xy * input.normal.z + ObjectScaling.xz * input.normal.y + ObjectScaling.zy * input.normal.x));
	}
	else
	{
		output.tex.xy = input.tex.xy;
	}

	// Apply object color for all color values with low alpha
	float4 vertexColor = input.col * input.col.a + ObjectColor * (1 - input.col.a);

	// Apply output color
	float4 outputColor = (MaterialDiffuseColor * DiffuseColorFactor) + (vertexColor * (1.0 - DiffuseColorFactor));
	outputColor.a = 1.0;
	output.col = outputColor;

    // Calculate position vector in world space
    output.pos3D = mul(float4(input.pos.xyz, 1.0), World).xyz;
	output.texFact = input.texFact;
     
    return output;
}

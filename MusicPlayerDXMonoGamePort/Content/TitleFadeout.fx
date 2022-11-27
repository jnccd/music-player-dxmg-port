sampler2D samp2D;

int TexWidth = 300;
int TexHeight = 500;

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(samp2D, texCoord);
	float2 InvertedTexSize = float2(1.0 / TexHeight, 1.0 / TexWidth);

	if (texCoord.x < 0.1f)
		color.rgba *= 1 - ((0.1f - texCoord.x) * 10);
	if (texCoord.x > 0.9f)
		color.rgba *= 1 - ((texCoord.x - 0.9f) * 10);

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_1 PixelShaderFunction();
	}
}
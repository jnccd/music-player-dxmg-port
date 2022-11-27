sampler samp2D : register(s0);

int TexWidth = 300;
int TexHeight = 500;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 color = tex2D(samp2D, input.texCoord);
	float2 InvertedTexSize = float2(1.0 / TexHeight, 1.0 / TexWidth);

	if (input.texCoord.x < 0.1f)
		color.rgba *= 1 - ((0.1f - input.texCoord.x) * 10);
	if (input.texCoord.x > 0.9f)
		color.rgba *= 1 - ((input.texCoord.x - 0.9f) * 10);

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
	}
}
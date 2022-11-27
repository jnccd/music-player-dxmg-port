Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

int TexWidth = 300;
int TexHeight = 500;

float BlurWeights[15];
float2 InvTexsize;
bool horz = false;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR
{
	float4 color = 0;
	float2 coords = 0;

	for (int i = 0; i < 15; i++)
	{
		int j = i-7;
		if (horz)
			coords = float2(input.TextureCoordinates.x + InvTexsize.x * j, input.TextureCoordinates.y);
		else
			coords = float2(input.TextureCoordinates.x, input.TextureCoordinates.y + InvTexsize.y * 2 * j);
		color += tex2D(SpriteTextureSampler, coords) * BlurWeights[i];
	}

	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
	}
}
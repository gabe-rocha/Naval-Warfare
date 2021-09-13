Shader "BlueOcean/Inputs/Shore/OceanShoreWaves"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			Blend One One

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"
			#include "../../OceanNoise.hlsl"

			float _GlobalTime;

			#include "../ShoreWaves.hlsl"
            
            #define PI 3.141593

			sampler2D _MainTex;
			

			CBUFFER_START(BlueOceanPerOceanInput)
            float  _WindDirection;
            float  _DepthMax;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 worldXZ : TEXCOORD1;
			};

			Varyings Vert(Attributes input)
			{
				Varyings output;
				output.position = UnityObjectToClipPos(input.positionOS);
				output.uv = input.uv;
				float3 worldPos = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).xyz;
				output.worldXZ = worldPos.xz;
				return output;
			}
            
			half4 Frag(Varyings input) : SV_Target
			{
                float dispHeight = 0;
                float depth = tex2D(_MainTex, input.uv).r;
                
                float4 disp = float4(0, dispHeight, 0, 0);
				float3 pos = float3(input.worldXZ.x, 0, input.worldXZ.y);
				
				float3 gerstnerOffset = 0;
				AddGerstnerWaves(pos, _WindDirection, 0.2, gerstnerOffset);

				disp.xyz += gerstnerOffset;
				disp.xyz *= lerp(1, 0, saturate(depth / _DepthMax));
				disp.w = 0;
                return disp;
                
			}
			ENDCG
		}
	}
}

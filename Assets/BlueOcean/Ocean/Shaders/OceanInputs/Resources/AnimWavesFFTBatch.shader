// zos
Shader "Hidden/BlueOcean/Inputs/Animated Waves/FFT Batch Global"
{
	Properties
	{
	}

	SubShader
	{
		Pass
		{
			Blend One One
			ZWrite Off
			ZTest Always
			Cull Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			#include "../../OceanGlobals.hlsl"
			#include "../../OceanInputsDriven.hlsl"
			#include "../../OceanLODData.hlsl"

			float choppiness;
            float depthFade;
            float baseSSS;
            float dispSSSMul;

			//Texture2D<float> inputH;
			//Texture2D<float> inputDx;
			//Texture2D<float> inputDy;
			Texture2D<float4> inputHDxDy;

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 worldPosXZ : TEXCOORD0;
				float3 uv_slice : TEXCOORD1;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = float4(input.positionOS.xy, 0.0, 0.5);

#if UNITY_UV_STARTS_AT_TOP // https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
				o.positionCS.y = -o.positionCS.y;
#endif
				float2 worldXZ = UVToWorld(input.uv);
				o.worldPosXZ = worldXZ;
				o.uv_slice = float3(input.uv, _LD_SliceIndex);
				return o;
			}

			float3 SampleDisplacement(int2 coord)
			{
				//return float3(inputDx[coord], inputH[coord], inputDy[coord]);
				return float3(inputHDxDy[coord].yxz);
			}

			half4 Frag(Varyings input) : SV_Target
			{
				// sample ocean depth (this render target should 1:1 match depth texture, so UVs are trivial)
				const half depth = _LD_TexArray_SeaFloorDepth.Sample(LODData_linear_clamp_sampler, input.uv_slice).x;

                float domainSize = _LD_Params[input.uv_slice.z].x * _LD_Params[input.uv_slice.z].y;
                float2 normalizedWorldCoord = fmod(input.worldPosXZ, domainSize.xx) / domainSize;
                if (normalizedWorldCoord.x < 0) normalizedWorldCoord.x += 1;
                if (normalizedWorldCoord.y < 0) normalizedWorldCoord.y += 1;
                
				//normalizedWorldCoord / _LD_Params[input.uv_slice.z].w
				int2 coord = floor(normalizedWorldCoord * _LD_Params[input.uv_slice.z].y);
				
				//float invDomainSize = 1.0 / domainSize;
				float4 disp = float4(SampleDisplacement(coord), 0);

				float w = 1;
				w = saturate(depth / depthFade);
				disp *= w;

				disp.w = length(disp.xyz) * dispSSSMul + baseSSS;

				return disp;
			}
			ENDCG
		}
	}
}

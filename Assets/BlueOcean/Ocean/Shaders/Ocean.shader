Shader "BlueOcean/Ocean"
{
	Properties
	{
		[Header(Normal Mapping)]
		// Whether to add normal detail from a texture. Can be used to add visual detail to the water surface
		[Toggle] _ApplyNormalMapping("Enable", Float) = 1
		// Normal map texture (should be set to Normals type in the properties)
		[NoScaleOffset] _Normals("Normal Map", 2D) = "bump" {}
		// Strength of normal map influence
		_NormalsStrength("Strength", Range(0.01, 2.0)) = 0.36
		// Scale of normal map texture
		_NormalsScale("Scale", Range(0.01, 200.0)) = 40.0

		// Base light scattering settings which give water colour
		[Header(Scattering)]
		// Base colour when looking straight down into water
		_Diffuse("Diffuse", Color) = (0.0, 0.0124, 0.566, 1.0)
		// Base colour when looking into water at shallow/grazing angle
		_DiffuseGrazing("Diffuse Grazing", Color) = (0.184, 0.393, 0.519, 1)
		// Changes colour in shadow. Requires 'Create Shadow Data' enabled on OceanRenderer script.
		[Toggle] _Shadows("Shadowing", Float) = 0
		// Base colour in shadow
		_DiffuseShadow("Diffuse (Shadow)", Color) = (0.0, 0.356, 0.565, 1.0)

		[Header(Subsurface Scattering)]
		// Whether to to emulate light scattering through the water volume
		[Toggle] _SubSurfaceScattering("Enable", Float) = 1
		// Colour tint for primary light contribution
		_SubSurfaceColour("Colour", Color) = (0.0, 0.48, 0.36)
		// Amount of primary light contribution that always comes in
		_SubSurfaceBase("Base Mul", Range(0.0, 4.0)) = 1.0
		// Primary light contribution in direction of light to emulate light passing through waves
		_SubSurfaceSun("Sun Mul", Range(0.0, 10.0)) = 4.5
		// Fall-off for primary light scattering to affect directionality
		_SubSurfaceSunFallOff("Sun Fall-Off", Range(1.0, 16.0)) = 5.0

		[Header(Shallow Scattering)]
		// Enable light scattering in shallow water
		[Toggle] _SubSurfaceShallowColour("Enable", Float) = 1
		// Max depth that is considered 'shallow'
		_SubSurfaceDepthMax("Depth Max", Range(0.01, 50.0)) = 10.0
		// Fall off of shallow scattering
		_SubSurfaceDepthPower("Depth Power", Range(0.01, 10.0)) = 2.5
		// Colour in shallow water
		_SubSurfaceShallowCol("Shallow Colour", Color) = (0.552, 1.0, 1.0, 1.0)
		// Shallow water colour in shadow (see comment on Shadowing param above)
		_SubSurfaceShallowColShadow("Shallow Colour (Shadow)", Color) = (0.144, 0.226, 0.212, 1)

		// Reflection properites
		[Header(Reflection Environment)]
		// Strength of specular lighting response
		_Specular("Specular", Range(0.0, 1.0)) = 1.0
		// Smoothness of surface
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.8
		// Vary smoothness - helps to spread out specular highlight in mid-to-background. Models transfer of normal detail
		// to microfacets in BRDF.
		[Toggle] _VarySmoothnessOverDistance("Vary Smoothness Over Distance", Float) = 0
		// Material smoothness at far distance from camera
		_SmoothnessFar("Smoothness Far", Range(0.0, 1.0)) = 0.35
		// Definition of far distance
		_SmoothnessFarDistance("Smoothness Far Distance", Range(1.0, 8000.0)) = 2000.0
		// How smoothness varies between near and far distance - shape of curve.
		_SmoothnessPower("Smoothness Power", Range(0.0, 2.0)) = 0.5
		// Acts as mip bias to smooth/blur reflection
		_ReflectionBlur("Softness", Range(0.0, 7.0)) = 0.0
		// Main light intensity multiplier
		_LightIntensityMultiplier("Light Intensity Multiplier", Range(0.0, 10.0)) = 1.0
		// Controls harshness of Fresnel behaviour
		_FresnelPower("Fresnel Power", Range(1.0, 20.0)) = 5.0
		// Index of refraction of air. Can be increased to almost 1.333 to increase visibility up through water surface.
		_RefractiveIndexOfAir("Refractive Index of Air", Range(1.0, 2.0)) = 1.0
		// Index of refraction of water. Typically left at 1.333.
		_RefractiveIndexOfWater("Refractive Index of Water", Range(1.0, 2.0)) = 1.333
        
        [Header(ScreenSpaceReflection)]
        [Toggle] _ScreenSpaceReflections("Enable", Float) = 1
        
		[Header(Foam)]
		// Enable foam layer on ocean surface
		[Toggle] _Foam("Enable", Float) = 1
		// Foam texture
		[NoScaleOffset] _FoamTexture("Foam Texture", 2D) = "white" {}
		// Foam texture scale
		_FoamScale("Scale", Range(0.01, 50.0)) = 10.0
		// Scale intensity of lighting
		_WaveFoamLightScale("Light Scale", Range(0.0, 2.0)) = 1.35
		// Colour tint for whitecaps / foam on water surface
		_FoamWhiteColor("White Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)
		// Controls how gradual the transition is from full foam to no foam
		_WaveFoamFeather("Wave Foam Feather", Range(0.001, 1.0)) = 0.4
        
		

		[Header(Transparency)]
		// Whether light can pass through the water surface
		[Toggle] _Transparency("Enable", Float) = 1
		// Scattering coefficient within water volume, per channel
		_DepthFogDensity("Fog Density", Vector) = (0.33, 0.23, 0.37, 1.0)
		// How strongly light is refracted when passing through water surface
		_RefractionStrength("Refraction Strength", Range(0.0, 2.0)) = 0.1

		[Header(Debug Options)]
		// Build shader with debug info which allows stepping through the code in a GPU debugger. I typically use RenderDoc or
		// PIX for Windows (requires DX12 API to be selected).
		[Toggle] _CompileShaderWithDebugInfo("Compile Shader With Debug Info (D3D11)", Float) = 0
	}

	SubShader
	{
		Tags
		{
			// run exclusively in URP
			"RenderPipeline"="UniversalPipeline"
			"RenderType"="Transparent"
			"Queue"="Transparent-100"
			"DisableBatching"="True"
		}

		Pass
		{
			// Following URP code. Apparently this can be not defined according to https://gist.github.com/phi-lira/225cd7c5e8545be602dca4eb5ed111ba
			//Tags {"LightMode" = "UniversalForward"}

			// Need to set this explicitly as we dont rely on built-in pipeline render states anymore.
			ZWrite On

			// Culling user defined - can be inverted for under water
			Cull[_CullMode]

			HLSLPROGRAM
			// Required to compile gles 2.0 with standard SRP library
			// All shaders must be compiled with HLSLcc and currently only gles is not using HLSLcc by default
			// https://gist.github.com/phi-lira/225cd7c5e8545be602dca4eb5ed111ba
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			// For VFACE
			#pragma target 3.0

			#pragma vertex Vert
			#pragma fragment Frag
			// for VFACE
			#pragma target 3.0
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			//#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

			#pragma shader_feature _APPLYNORMALMAPPING_ON
			#pragma shader_feature _SUBSURFACESCATTERING_ON
			#pragma shader_feature _SUBSURFACESHALLOWCOLOUR_ON
			#pragma shader_feature _VARYSMOOTHNESSOVERDISTANCE_ON
			#pragma shader_feature _TRANSPARENCY_ON
			#pragma shader_feature _FOAM_ON
			#pragma shader_feature _SCREENSPACEREFLECTIONS_ON
			//#pragma shader_feature _SHADOWS_ON
			#pragma shader_feature _COMPILESHADERWITHDEBUGINFO_ON

			#if _COMPILESHADERWITHDEBUGINFO_ON
			#pragma enable_d3d11_debug_symbols
			#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            #include "OceanNoise.hlsl"
			#include "OceanGlobals.hlsl"
			#include "OceanInputsDriven.hlsl"
			#include "OceanInput.hlsl"
			#include "OceanLODData.hlsl"
			#include "OceanHelpersNew.hlsl"
			#include "OceanHelpers.hlsl"

			#include "OceanEmission.hlsl"
			#include "OceanNormalMapping.hlsl"
			#include "OceanReflection.hlsl"
			#include "OceanFoam.hlsl"
            

			struct Attributes
			{
				real3 positionOS : POSITION;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 lodAlpha_worldXZUndisplaced_oceanDepth : TEXCOORD0;
				real4 n_shadow : TEXCOORD1;
				real4 foam_screenPosXYW : TEXCOORD2;
				float4 positionWS_fogFactor : TEXCOORD3;
				#if _FLOW_ON
				real2 flow : TEXCOORD4;
				#endif

				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexPositionInputs GetOceanShadowInputs(float3 worldPos)
			{
				VertexPositionInputs input;
				input.positionWS = worldPos;
				input.positionVS = TransformWorldToView(input.positionWS);
				input.positionCS = TransformWorldToHClip(input.positionWS);

				float4 ndc = input.positionCS * 0.5f;
				input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
				input.positionNDC.zw = input.positionCS.zw;

				return input;
			}

			Varyings Vert(Attributes input)
			{
				Varyings o = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				// Move to world space
				o.positionWS_fogFactor.xyz = TransformObjectToWorld(input.positionOS);

				// Vertex snapping and lod transition
				float lodAlpha;
				SnapAndTransitionVertLayout(_InstanceData.x, o.positionWS_fogFactor.xyz, lodAlpha);
				o.lodAlpha_worldXZUndisplaced_oceanDepth.x = lodAlpha;
				o.lodAlpha_worldXZUndisplaced_oceanDepth.yz = o.positionWS_fogFactor.xz;
				o.lodAlpha_worldXZUndisplaced_oceanDepth.w = BLUE_OCEAN_OCEAN_DEPTH_BASELINE;
				// Sample shape textures - always lerp between 2 LOD scales, so sample two textures

				// Calculate sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
				const float wt_smallerLod = (1. - lodAlpha) * _LD_Params[_LD_SliceIndex].z;
				const float wt_biggerLod = (1. - wt_smallerLod) * _LD_Params[_LD_SliceIndex + 1].z;
				// Sample displacement textures, add results to current world pos / normal / foam
				const float2 positionWS_XZ_before = o.positionWS_fogFactor.xz;

				// Data that needs to be sampled at the undisplaced position
				if (wt_smallerLod > 0.001)
				{
					const float3 uv_slice_smallerLod = WorldToUV(positionWS_XZ_before);

					#if !_DEBUGDISABLESHAPETEXTURES_ON
					half sss = 0.;
					SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, o.positionWS_fogFactor.xyz, sss);
					#endif
				}
				if (wt_biggerLod > 0.001)
				{
					const float3 uv_slice_biggerLod = WorldToUV_BiggerLod(positionWS_XZ_before);

					#if !_DEBUGDISABLESHAPETEXTURES_ON
					half sss = 0.;
					SampleDisplacements(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, o.positionWS_fogFactor.xyz, sss);
					#endif
				}

				// Data that needs to be sampled at the displaced position
				if (wt_smallerLod > 0.0001)
				{
					const float3 uv_slice_smallerLodDisp = WorldToUV(o.positionWS_fogFactor.xz);

					#if _SUBSURFACESHALLOWCOLOUR_ON
					// The minimum sampling weight is lower (0.0001) than others to fix shallow water colour popping.
					SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_smallerLodDisp, wt_smallerLod, o.lodAlpha_worldXZUndisplaced_oceanDepth.w);
					#endif
				}
				if (wt_biggerLod > 0.0001)
				{
					const float3 uv_slice_biggerLodDisp = WorldToUV_BiggerLod(o.positionWS_fogFactor.xz);

					#if _SUBSURFACESHALLOWCOLOUR_ON
					// The minimum sampling weight is lower (0.0001) than others to fix shallow water colour popping.
					SampleSeaDepth(_LD_TexArray_SeaFloorDepth, uv_slice_biggerLodDisp, wt_biggerLod, o.lodAlpha_worldXZUndisplaced_oceanDepth.w);
					#endif
				}

				//#if _SHADOWS_ON
				//VertexPositionInputs vertexInput = GetOceanShadowInputs(o.positionWS_fogFactor.xyz);
				//o.n_shadow = GetShadowCoord(vertexInput);
				//#endif

				// Foam can saturate
				o.foam_screenPosXYW.x = saturate(o.foam_screenPosXYW.x);

				o.positionCS = TransformWorldToHClip(o.positionWS_fogFactor.xyz);

				o.positionWS_fogFactor.w = ComputeFogFactor(o.positionCS.z);

				o.foam_screenPosXYW.yzw = ComputeScreenPos(o.positionCS).xyw;

				return o;
			}

			

			bool IsUnderwater(const float facing)
			{
#if !_UNDERWATER_ON
				return false;
#endif
				const bool backface = facing < 0.0;
				return backface || _ForceUnderwater > 0.0;
			}

			half4 Frag(const Varyings input, const float facing : VFACE) : SV_Target
			{
				// We need this when sampling a screenspace texture.
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				const bool underwater = IsUnderwater(facing);
				const float lodAlpha = input.lodAlpha_worldXZUndisplaced_oceanDepth.x;

				real3 view = normalize(GetCameraPositionWS() - input.positionWS_fogFactor.xyz);

				float pixelZ = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
				real3 screenPos = input.foam_screenPosXYW.yzw;
				real2 uvDepth = screenPos.xy / screenPos.z;

				float sceneZ01 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uvDepth).x;
				float sceneZ = LinearEyeDepth(sceneZ01, _ZBufferParams);

				// Normal - geom + normal mapping. Subsurface scattering.
				const float3 uv_slice_smallerLod = WorldToUV(input.lodAlpha_worldXZUndisplaced_oceanDepth.yz);
				const float3 uv_slice_biggerLod = WorldToUV_BiggerLod(input.lodAlpha_worldXZUndisplaced_oceanDepth.yz);
				const float wt_smallerLod = (1. - lodAlpha) * _LD_Params[_LD_SliceIndex].z;
				const float wt_biggerLod = (1. - wt_smallerLod) * _LD_Params[_LD_SliceIndex + 1].z;
				float3 dummy = 0.;
				half3 n_geom = half3(0.0, 1.0, 0.0);
				half sss = 0.;
				
				if (wt_smallerLod > 0.001) SampleDisplacementsNormals(_LD_TexArray_AnimatedWaves, uv_slice_smallerLod, wt_smallerLod, _LD_Params[_LD_SliceIndex].w, _LD_Params[_LD_SliceIndex].x, dummy, n_geom.xz, sss);
				if (wt_biggerLod > 0.001) SampleDisplacementsNormals(_LD_TexArray_AnimatedWaves, uv_slice_biggerLod, wt_biggerLod, _LD_Params[_LD_SliceIndex + 1].w, _LD_Params[_LD_SliceIndex + 1].x, dummy, n_geom.xz, sss);
				n_geom = normalize(n_geom);

				if (underwater) n_geom = -n_geom;
				real3 n_pixel = n_geom;
				#if _APPLYNORMALMAPPING_ON
				n_pixel.xz += (underwater ? -1. : 1.) * SampleNormalMaps(input.lodAlpha_worldXZUndisplaced_oceanDepth.yz, lodAlpha);
				n_pixel = normalize(n_pixel);
				#endif

				//#if _SHADOWS_ON
				//const Light lightMain = GetMainLight(input.n_shadow);
				//const float shadow = lightMain.shadowAttenuation;
				//#else
				const Light lightMain = GetMainLight();
				const float shadow = 1;
				//#endif
				const real3 lightDir = lightMain.direction;
				const real3 lightCol = lightMain.color;
				
				// Foam - underwater bubbles and whitefoam
				real3 bubbleCol = (half3)0.;
				float foam = 0;
				
				#if _FOAM_ON
				if (wt_smallerLod > 0.001)
				{
					SampleFoam(_LD_TexArray_Foam, uv_slice_smallerLod, wt_smallerLod, foam);
				}
				if (wt_biggerLod > 0.001)
				{
					SampleFoam(_LD_TexArray_Foam, uv_slice_biggerLod, wt_biggerLod, foam);
				}
				
				real4 whiteFoamCol;
				ComputeFoam(foam, input.lodAlpha_worldXZUndisplaced_oceanDepth.yz, input.positionWS_fogFactor.xz, n_pixel, pixelZ, sceneZ, view, lightDir, lightCol, shadow, lodAlpha, bubbleCol, whiteFoamCol);
                #endif // _FOAM_ON
                
				// Compute color of ocean - in-scattered light + refracted scene
				half3 scatterCol = ScatterColour(input.lodAlpha_worldXZUndisplaced_oceanDepth.w, _WorldSpaceCameraPos, lightDir, view, shadow, underwater, true, lightCol, sss);
				real3 col = OceanEmission(view, n_pixel, lightCol, lightDir, input.foam_screenPosXYW.yzw, pixelZ, uvDepth, sceneZ, sceneZ01, bubbleCol, _Normals, underwater, scatterCol);
                     
				// Light that reflects off water surface
				// Soften reflection at intersections with objects/surfaces
				#if _TRANSPARENCY_ON
				float reflAlpha = saturate((sceneZ - pixelZ) / 0.2);
				#else
				// This addresses the problem where screenspace depth doesnt work in VR, and so neither will this. In VR people currently
				// disable transparency, so this will always be 1.0.
				float reflAlpha = 1.0;
				#endif

				ApplyReflectionSky(view, n_pixel, lightDir, shadow, input.foam_screenPosXYW.yzzw, pixelZ, reflAlpha, input.positionWS_fogFactor.xyz, col);
				            
				// Override final result with white foam - bubbles on surface
				#if _FOAM_ON
				col = lerp(col, whiteFoamCol.rgb, whiteFoamCol.a);
				#endif

				// Fog
				if (!underwater)
				{
					// Above water - do atmospheric fog. If you are using a third party sky package such as Azure, replace this with their stuff!
					col = MixFog(col, input.positionWS_fogFactor.w);
				}
				else
				{
					// underwater - do depth fog
					col = lerp(col, scatterCol, saturate(1.0 - exp(-_DepthFogDensity.xyz * pixelZ)));
				}

				return real4(col, 1.0);
			}

			ENDHLSL
		}
	}

	// If the above doesn't work then error.
	FallBack "Hidden/InternalErrorShader"
}

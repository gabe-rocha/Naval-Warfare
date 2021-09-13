#if _SCREENSPACEREFLECTIONS_ON
float UVJitter(in float2 uv)
{
	return frac((52.9829189 * frac(dot(uv, float2(0.06711056, 0.00583715)))));
}

void SSRRayConvert(float3 worldPos, out float4 clipPos, out float3 screenPos)
{
	clipPos = TransformWorldToHClip(worldPos);
	float k = ((1.0) / (clipPos.w));

	screenPos.xy = ComputeScreenPos(clipPos).xy * k;
	screenPos.z = k;

#if defined(UNITY_SINGLE_PASS_STEREO)
	screenPos.xy = UnityStereoTransformScreenSpaceTex(screenPos.xy);
#endif
}

float3 SSRRayMarch(float3 i_worldPos, half3 i_R)
{
	float4 startClipPos;
	float3 startScreenPos;

	SSRRayConvert(i_worldPos, startClipPos, startScreenPos);

	float4 endClipPos;
	float3 endScreenPos;

	SSRRayConvert(i_worldPos + i_R, endClipPos, endScreenPos);

	if (((endClipPos.w) < (startClipPos.w)))
	{
		return float3(0, 0, 0);
	}

	float3 screenDir = endScreenPos - startScreenPos;

	float screenDirX = abs(screenDir.x);
	float screenDirY = abs(screenDir.y);

	
	float dirMultiplier = lerp(1 / (_ScreenParams.y * screenDirY), 1 / (_ScreenParams.x * screenDirX), screenDirX > screenDirY) * _SSRSampleStep;

	screenDir *= dirMultiplier;

	half lastRayDepth = startClipPos.w;

	//no jitter test
	half sampleCount = 1 + UVJitter(startClipPos) * 0.1;

	float3 lastScreenMarchUVZ = startScreenPos;
	float lastDeltaDepth = 0;

#if defined (SHADER_API_OPENGL) || defined (SHADER_API_D3D11) || defined (SHADER_API_D3D12)
	[unroll(64)]
#else
	UNITY_LOOP
#endif
		
		for (int i = 0; i < _SSRMaxSampleCount; i++)
		{
			float3 screenMarchUVZ = startScreenPos + screenDir * sampleCount;

			if ((screenMarchUVZ.x <= 0) || (screenMarchUVZ.x >= 1) || (screenMarchUVZ.y <= 0) || (screenMarchUVZ.y >= 1))
			{
				break;
			}

			float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, screenMarchUVZ.xy), _ZBufferParams);

			half rayDepth = 1.0 / screenMarchUVZ.z;
			half deltaDepth = rayDepth - sceneDepth;

			if ((deltaDepth > 0) && (sceneDepth > startClipPos.w) && (deltaDepth < (abs(rayDepth - lastRayDepth) * 2)))
			{
				float samplePercent = saturate(lastDeltaDepth / (lastDeltaDepth - deltaDepth));
				samplePercent = lerp(samplePercent, 1, rayDepth >= _ProjectionParams.z);
				float3 hitScreenUVZ = lerp(lastScreenMarchUVZ, screenMarchUVZ, samplePercent);
				return float3(hitScreenUVZ.xy, 1);
			}

			lastRayDepth = rayDepth;
			sampleCount += 1;

			lastScreenMarchUVZ = screenMarchUVZ;
			lastDeltaDepth = deltaDepth;
		}

	float4 farClipPos;
	float3 farScreenPos;

	SSRRayConvert(i_worldPos + i_R * 100000, farClipPos, farScreenPos);

	if ((farScreenPos.x > 0) && (farScreenPos.x < 1) && (farScreenPos.y > 0) && (farScreenPos.y < 1))
	{

		float farDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, farScreenPos.xy), _ZBufferParams);

		if (farDepth > startClipPos.w)
		{
			return float3(farScreenPos.xy, 1);
		}
	}

	return float3(0, 0, 0);
}

float3 GetSSRUVZ(in const float3 i_WorldPos, in const float2 i_ScreenUV, in const half i_NoV, in const half3 i_R)
{
#if defined(UNITY_SINGLE_PASS_STEREO)
	half ssrWeight = 1;

	half NoV = i_NoV * 2;
	ssrWeight *= (1 - NoV * NoV);
#else
	float screenUV = i_ScreenUV * 2 - 1;
	screenUV *= screenUV;

	half ssrWeight = saturate(1 - dot(screenUV, screenUV));

	half NoV = i_NoV * 2.5;
	ssrWeight *= (1 - NoV * NoV);
#endif

	if (ssrWeight > 0.0005)
	{
		float3 uvz = SSRRayMarch(i_WorldPos, i_R);
		uvz.z *= ssrWeight;
		return uvz;
	}

	return float3(0, 0, 0);
}

void ScreenSpaceReflection(in const float3 i_WorldPos, in const half4 i_ScreenPos, in const half i_NoV, in const half3 i_R, inout half3 io_colour)
{
	float2 screenUV = i_ScreenPos.xy / i_ScreenPos.w;
	float3 uvz = GetSSRUVZ(i_WorldPos, screenUV, i_NoV, i_R);
	
	io_colour = lerp(io_colour, SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvz.xy), saturate(uvz.z * _SSRIntensity));
    
}
#endif // _SCREENSPACEREFLECTIONS_ON


float CalculateFresnelReflectionCoefficient(float cosTheta)
{
	// Fresnel calculated using Schlick's approximation
	// See: http://www.cs.virginia.edu/~jdl/bib/appearance/analytic%20models/schlick94b.pdf
	// reflectance at facing angle
	float R_0 = (_RefractiveIndexOfAir - _RefractiveIndexOfWater) / (_RefractiveIndexOfAir + _RefractiveIndexOfWater); R_0 *= R_0;
	const float R_theta = R_0 + (1.0 - R_0) * pow(max(0.,1.0 - cosTheta), _FresnelPower);
	return R_theta;
}

void ApplyReflectionSky(in const half3 i_view, in const half3 i_n_pixel, in const half3 i_lightDir, in const half i_shadow, in const half4 i_screenPos, in const float i_pixelZ, in const half i_weight, in const half3 i_worldPos, inout half3 io_col)
{
    half3 skyColour = 0;
	// Reflection
	half3 refl = reflect(-i_view, i_n_pixel);
	// Don't reflect below horizon
	refl.y = max(refl.y, 0.0);

	// Sharp reflection
	const real mip = _ReflectionBlur;

	// Unity sky
	half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, refl, mip);
#if !defined(UNITY_USE_NATIVE_HDR)
	skyColour = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
#else
	skyColour = encodedIrradiance.rbg;
#endif

#if _SCREENSPACEREFLECTIONS_ON

	half NOV = saturate(dot(i_n_pixel, i_view));
	ScreenSpaceReflection(i_worldPos, i_screenPos, NOV, refl, skyColour);
#endif

	// Specular from sun light
	
	// Surface smoothness
	float smoothness = _Smoothness;
#if _VARYSMOOTHNESSOVERDISTANCE_ON
	smoothness = lerp(smoothness, _SmoothnessFar, pow(saturate(i_pixelZ / _SmoothnessFarDistance), _SmoothnessPower));
#endif
	
	half alpha = 1.0;
	BRDFData brdfData;
	// TODO pull in code for specular highlight
	InitializeBRDFData(0, 0.0, 1.0, smoothness, alpha, brdfData);
	const Light mainLight = GetMainLight();
	// Multiply Specular here because it the BRDF doesn't seem to use it..
	//skyColour += i_shadow * _LightIntensityMultiplier * LightingPhysicallyBased(brdfData, mainLight, i_n_pixel, i_view);


	// Fresnel
	//float R_theta = CalculateFresnelReflectionCoefficient(max(dot(i_n_pixel, i_view), 0.0));
	//io_col = skyColour * R_theta + io_col * (1 - R_theta) + i_shadow * _LightIntensityMultiplier * LightingPhysicallyBased(brdfData, mainLight, i_n_pixel, i_view);
	//io_col = lerp(io_col, skyColour, R_theta * _Specular * i_weight);

	// Fresnel
	float R_theta = CalculateFresnelReflectionCoefficient(max(dot(i_n_pixel, i_view), 0.0));
	R_theta *= _Specular * i_weight;
	float3 ISky = skyColour * R_theta;
	float3 ISea = io_col * (1 - R_theta);
	float3 ISun = i_shadow * _LightIntensityMultiplier * LightingPhysicallyBased(brdfData, mainLight, i_n_pixel, i_view);
	io_col = ISea + (ISky + ISun);// *saturate(1 - foam);
    //io_col = ISky;
}

#if _UNDERWATER_ON
void ApplyReflectionUnderwater(in const half3 i_view, in const half3 i_n_pixel, in const half3 i_lightDir, in const half i_shadow, in const half4 i_screenPos, half3 scatterCol, in const half i_weight, inout half3 io_col)
{
	const half3 underwaterColor = scatterCol;
	// The the angle of outgoing light from water's surface
	// (whether refracted form outside or internally reflected)
	const float cosOutgoingAngle = max(dot(i_n_pixel, i_view), 0.);

	// calculate the amount of incident light from the outside world (io_col)
	{
		// have to calculate the incident angle of incoming light to water
		// surface based on how it would be refracted so as to hit the camera
		const float cosIncomingAngle = cos(asin(clamp( (_RefractiveIndexOfWater * sin(acos(cosOutgoingAngle))) / _RefractiveIndexOfAir, -1.0, 1.0) ));
		const float reflectionCoefficient = CalculateFresnelReflectionCoefficient(cosIncomingAngle) * i_weight;
		io_col *= (1.0 - reflectionCoefficient);
		io_col = max(io_col, 0.0);
	}

	// calculate the amount of light reflected from below the water
	{
		// angle of incident is angle of reflection
		const float cosIncomingAngle = cosOutgoingAngle;
		const float reflectionCoefficient = CalculateFresnelReflectionCoefficient(cosIncomingAngle) * i_weight;
		io_col += (underwaterColor * reflectionCoefficient);
	}
}
#endif

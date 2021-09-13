// GLOBALs - we're allowed to use these anywhere. TODO should all be prefixed by "BlueOcean"!

#ifndef BLUE_OCEAN_OCEAN_GLOBALS_H
#define BLUE_OCEAN_OCEAN_GLOBALS_H

SamplerState LODData_linear_clamp_sampler;
SamplerState LODData_point_clamp_sampler;
SamplerState sampler_BlueOcean_linear_repeat;

CBUFFER_START(BlueOceanPerFrame)
float _BlueOceanTime;
float _TexelsPerWave;
float3 _OceanCenterPosWorld;
float _SliceCount;
float _MeshScaleLerp;
float _BlueOceanClipByDefault;
float _BlueOceanLodAlphaBlackPointFade;
float _BlueOceanLodAlphaBlackPointWhitePointFade;

float3 _PrimaryLightDirection;
float3 _PrimaryLightIntensity;
CBUFFER_END

#endif

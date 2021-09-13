#ifndef SHORE_WAVES_INCLUDED
#define SHORE_WAVES_INCLUDED

uniform uint _WaveCount; // how many waves, set via the water component

half4 waveData[10]; // 0-9 amplitude, direction, wavelength

struct WaveStruct
{
	float3 position;
};

WaveStruct GerstnerWave(half2 pos, float waveCountMulti, half amplitude, half direction, half wavelength)
{
	WaveStruct waveOut;

	////////////////////////////////wave value calculations//////////////////////////
	half3 wave = 0; // wave vector
	half w = 6.28318 / wavelength; // 2pi over wavelength(hardcoded)
	half wSpeed = sqrt(9.8 * w); // frequency of the wave based off wavelength
	half peak = 1; // peak value, 1 is the sharpest peaks
	half qi = peak / (amplitude * w * _WaveCount);

	//direction = radians(direction); // convert the incoming degrees to radians, for directional waves
    direction += 3.14159*0.5;
	half2 dirWaveInput = half2(sin(direction), cos(direction));

	half2 windDir = normalize(dirWaveInput); // calculate wind direction
	half dir = dot(windDir, pos); // calculate a gradient along the wind direction

	////////////////////////////position output calculations/////////////////////////
	half calc = dir * w + -_GlobalTime * wSpeed; // the wave calculation
	half cosCalc = cos(calc); // cosine version(used for horizontal undulation)
	half sinCalc = sin(calc); // sin version(used for vertical undulation)

	// calculate the offsets for the current point
	wave.xz = qi * amplitude * windDir.xy * cosCalc;
	wave.y = ((sinCalc * amplitude)) * waveCountMulti;// the height is divided by the number of waves
	
	////////////////////////////normal output calculations/////////////////////////
	//half wa = w * amplitude;
	//// normal vector
	//half3 n = half3(-(windDir.xy * wa * cosCalc),
	//				1-(qi * wa * sinCalc));

	////////////////////////////////assign to output///////////////////////////////
	waveOut.position = wave * saturate(amplitude * 10000);
	//waveOut.normal = (n * waveCountMulti) * amplitude;

	return waveOut;
}

inline void AddGerstnerWaves(float3 position, float dir, half opacity, out float3 waveOut)
{
	half2 pos = position.xz;
	WaveStruct waves[10];
	half waveCountMulti = 1.0 / _WaveCount;
	half3 opacityMask = saturate(half3(5, 1, 5) * opacity);
	waveOut = 0;
	
	UNITY_LOOP
	for(uint i = 0; i < _WaveCount; i++)
	{
		waves[i] = GerstnerWave(pos,
        								waveCountMulti, 
        								waveData[i].x, 
        								waveData[i].y + dir, 
        								waveData[i].z); // calculate the wave

		waveOut += waves[i].position * opacityMask; // add the position
	}
}

#endif // SHORE_WAVES_INCLUDED
#ifndef OCEAN_NOISE_INCLUDED
#define OCEAN_NOISE_INCLUDED

// Simple noise from thebookofshaders.com
// 2D Random
float random(float2 st) {
    st = float2(dot(st, float2(127.1, 311.7)), dot(st, float2(269.5, 183.3)));
    return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
}

// 2D Noise based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float Noise(float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);

    float2 u = f * f*(3.0 - 2.0*f);

    return lerp(lerp(dot(random(i), f),
        dot(random(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
        lerp(dot(random(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
            dot(random(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}

#endif // OCEAN_NOISE_INCLUDED

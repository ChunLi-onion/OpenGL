
#version 330 core
out float FragColor;

in vec2 TexCoords;

uniform sampler2D gPositionDepth;
uniform sampler2D gNormal;
uniform sampler2D texNoise;



// sample kernel 参数，调整效果
uniform int kernelSize;
uniform float radius;
uniform float bias;
uniform float power;

uniform vec3 samples[64];
uniform mat4 projection;

//平铺噪声纹理
const vec2 noiseScale = vec2(800.0/4.0, 600.0/4.0); // screen 800x600

void main()
{
    // 观察空间
    vec3 fragPos = texture(gPositionDepth, TexCoords).xyz;
    vec3 normal = texture(gNormal, TexCoords).xyz;
    vec3 randomVec = texture(texNoise, TexCoords * noiseScale).xyz;

    // 创建TBN矩阵
    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;
    for (int i = 0; i < kernelSize; ++i) {
        // 将样本从切线空间变换到观察空间
        vec3 samplePos = TBN * samples[i];
        samplePos = fragPos + samplePos * radius; // 偏移后采样坐标

        // sample -> 屏幕空间
        vec4 offset = vec4(samplePos, 1.0);
        offset = projection * offset;
        offset.xyz /= offset.w; // 透视变换
        offset.xyz = offset.xyz * 0.5 + 0.5; // [0.0, 1.0]

        // 采样gbuffer获得观察空间线性深度，即第一可见
        float sampleDepth = texture(gPositionDepth, offset.xy).z;  // <0

        //去除无效半径内的深度值影响
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth)); // shading point 深度与 采样位置的深度差值是否在半球内， 0则离得远，1则离的近
        occlusion += (sampleDepth >= (samplePos.z + bias) ? 1.0 : 0.0) * rangeCheck;
    }
    occlusion = 1.0 - (occlusion / kernelSize);
    FragColor = pow(occlusion, power);
}

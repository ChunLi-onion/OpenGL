#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D scene;
uniform sampler2D blurImage;

uniform bool bloom;
uniform float exposure;

void main()
{
    const float gamma = 2.2;
    vec3 hdrColor = texture(scene, TexCoords).rgb;
    vec3 bloomColor = texture(blurImage, TexCoords).rgb;
    
    if (bloom) {
        hdrColor += bloomColor; // 简单粗暴直接相加
    }
    
    //色调映射
    vec3 result = vec3(1.0) - exp(-hdrColor * exposure);
    
    //gamma校正
    result = pow(result, vec3(1.0) / gamma);
    FragColor = vec4(result, 1.0);
}



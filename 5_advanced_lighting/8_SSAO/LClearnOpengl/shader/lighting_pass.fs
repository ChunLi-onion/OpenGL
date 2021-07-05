
#version 330 core

out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D gPositionDepth;
uniform sampler2D gNormal;
uniform sampler2D gAlbedo;
uniform sampler2D ssao;

struct Light {
    vec3 Position;
    vec3 Color;
    
    float Linear;
    float Quadratic;
};

uniform Light light;

// 观察空间中viewPos 为[0,0,0]，不需传入

void main()
{
    vec3 FragPos = texture(gPositionDepth, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Albedo = texture(gAlbedo, TexCoords).rgb;
    float AmbientOcclusion = texture(ssao, TexCoords).r;
    
    // Blinn - Phong 光照计算, 观察空间中
    //ambient
    vec3 ambient = vec3(0.3 * AmbientOcclusion); //加入遮蔽因子
    
    // 光源贡献
    vec3 viewDir = normalize(-FragPos); // viewPos = [0,0,0]
    vec3 lighting = ambient;
    //diffuse
    vec3 lightDir = normalize(FragPos - light.Position);
    float diff = max(0.0, dot(Normal, -lightDir));
    vec3 result = light.Color * diff * Albedo;
    
    //specular
    vec3 halfway = normalize(-lightDir + viewDir);
    float shininess = 64;
    float spec = pow(max(0.0, dot(Normal, halfway)), shininess);
    result += light.Color * spec;
    
    //衰减,没有开启gamma校正
    float distance = length(FragPos - light.Position);
    result *= 1.0 / (1.0 + light.Linear * distance + light.Quadratic * distance * distance);
    lighting += result;

//    FragColor = vec4(vec3(AmbientOcclusion), 1.0);
    FragColor = vec4(vec3(lighting), 1.0);
    
}

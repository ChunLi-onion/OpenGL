
#version 330 core

out vec4 FragColor;
in vec2 TexCoords;

uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D gAlbedoSpec;

struct Light {
    vec3 Position;
    vec3 Color;
    float Linear;
    float Quadratic;
};

const int NR_LIGHTS = 32;
uniform Light lights[NR_LIGHTS];
uniform vec3 viewPos;

void main()
{
    vec3 FragPos = texture(gPosition, TexCoords).rgb;
    vec3 Normal = texture(gNormal, TexCoords).rgb;
    vec3 Albedo = texture(gAlbedoSpec, TexCoords).rgb;
    float Specular = texture(gAlbedoSpec, TexCoords).a;
    
    // 光照计算
    //ambient
    vec3 ambient = 0.5 * Albedo;
    
    // 光源贡献
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 lighting = vec3(0.0);
    for (int i = 0; i < NR_LIGHTS; ++i) {
        //diffuse
        vec3 lightDir = normalize(FragPos - lights[i].Position);
        float diff = max(0.0, dot(Normal, -lightDir));
        vec3 result = lights[i].Color * diff * Albedo;
        //specular
        vec3 halfway = normalize(-lightDir + viewDir);
        float shininess = 64;
        float spec = pow(max(0.0, dot(Normal, halfway)), shininess);
        result += lights[i].Color * spec * Specular;
        
        //衰减,因没有开启gamma校正，所以这里衰减方式用双曲线衰减符合物理规律
        float distance = length(FragPos - lights[i].Position);
        result *= 1.0 / (1.0 + lights[i].Linear * distance + lights[i].Quadratic * distance * distance);
        lighting += result;
    }
    vec3 result = ambient + lighting;
    
    FragColor = vec4(result, 1.0);
    
}

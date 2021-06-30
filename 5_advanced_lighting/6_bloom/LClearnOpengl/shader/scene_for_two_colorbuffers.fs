
#version 330 core
layout (location = 0) out vec4 FragColor;
layout (location = 1) out vec4 BrightColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

struct Light {
    vec3 Position;
    vec3 Color;
};

uniform Light lights[4];
uniform sampler2D diffuseTexture;
uniform sampler2D specularTexture;
uniform vec3 viewPos;

void main()
{
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    
    //ambient
    vec3 ambient = 0.0 * color;
    
    // 光源贡献
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 lighting = vec3(0.0);
    for (int i = 0; i < 4; ++i) {
        //diffuse
        vec3 lightDir = normalize(fs_in.FragPos - lights[i].Position);
        float diff = max(0.0, dot(normal, -lightDir));
        vec3 result = lights[i].Color * diff * color;
        //specular
        vec3 halfway = normalize(-lightDir + viewDir);
        float shininess = 64;
        float spec = pow(max(0.0, dot(normal, halfway)), shininess);
        vec3 specular = texture(specularTexture, fs_in.TexCoords).rgb;
        result += lights[i].Color * spec * specular;
        
        //衰减,因开启了gamma校正，所以这里衰减方式用平方衰减符合物理规律
        float distance = length(fs_in.FragPos - lights[i].Position);
        result *= 1 / (distance * distance);
        lighting += result;
    }
    vec3 result = ambient + lighting;
    
    //检查超出阈值的明亮部分，写入第二个颜色缓冲，用于之后的高斯模糊
    float brightness = dot(result, vec3(0.2126, 0.7152, 0.0722));
    if (brightness > 1.0) {
        BrightColor = vec4(result, 1.0);
    } else {
        BrightColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
    
    FragColor = vec4(result, 1.0); //hdr colorBuffer写入正常RGB
    
}

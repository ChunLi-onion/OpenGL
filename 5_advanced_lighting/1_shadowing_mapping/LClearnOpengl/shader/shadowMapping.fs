
#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 lightSpaceFragPos;
}fs_in;

uniform sampler2D shadowMap;
uniform sampler2D diffuseTexture;

uniform vec3 lightPos;
uniform vec3 viewPos;

float ShadowCalculation(vec4 lightSpaceFragPos, vec3 normal, vec3 lightDir)
{
    float shadow = 0.0;
    float min_depth = 0.0;
    float pcfDepth = 0.0;
    // 将自行计算的剪裁空间坐标执行光透视法, 得到【-1. 1】，模拟VS 到FS 的坐标变换
    vec3 projCoords = fs_in.lightSpaceFragPos.xyz / fs_in.lightSpaceFragPos.w;
    // 将NDC坐标【-1， 1】变换到 采样坐标【0，1】进行阴影贴图的采样
    projCoords = projCoords * 0.5 + 0.5;
    float cur_depth = projCoords.z; // 由世界空间坐标值经光空间正视投影转换矩阵得到，深度依然保持线性没有改变
    //阴影偏移
    float bias  = max(0.05 * (1.0 - dot(normal, -lightDir)), 0.005);
    // 多次采样平均化，柔和硬阴影
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0); // MipMap 0 级
    for (int x = -1; x <= 1; ++x)
    {
        for (int y = -1; y <= 1; ++y) {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += cur_depth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    
    // 处理处于光空间正交视锥远平面外的点
    if (cur_depth > 1.0) {
        shadow = 0.0;
        return shadow;
    }
    
    shadow /= 9.0;

    return shadow;
}
void main()
{
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightColor = vec3(0.5);
    //环境光
    vec3 ambient = 0.15 * color;

    //漫反射
    vec3 lightDir = normalize(fs_in.FragPos - lightPos);
    float diff = max(dot(-lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    //高光
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 halfwayDir = normalize(-lightDir + viewDir);
    float spec = pow(max(dot(halfwayDir, normal), 0.0), 64.0);
    vec3 specular = spec * lightColor;

    //计算阴影
    float shadow = ShadowCalculation(fs_in.lightSpaceFragPos, normal, lightDir);
    vec3 lighting = ambient + (1.0 - shadow) * (diffuse + specular) * color;
    FragColor = vec4(lighting, 1.0);

}

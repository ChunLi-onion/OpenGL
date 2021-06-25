#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
}fs_in;

uniform sampler2D diffuseTexture;
uniform samplerCube depthMap;

uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float far_plane;
uniform bool shadows;

vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1),
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);


float ShadowCalculation(vec3 fragPos)
{
    float shadow = 0.0;
    float min_depth = 0.0;
    //计算着色点与光源在世界中的距离，与depthMap中统一
    vec3 fragToLight = fragPos - lightPos; // cubeMap的采样方向
    float cur_depth = length(fragToLight);
    float bias = 0.05;

//    // ============ 优化样本数量后，滤波柔化阴影 =================
    int samples = 20;
    float viewDistance = length(viewPos - fragPos);
    float diskRadius = (1.0 + (viewDistance / far_plane)) / 25.0; // 根据着色点与相机距离改变 filter size，越远越柔和
    for(int i = 0; i < samples; ++i)
    {
        min_depth = texture(depthMap, fragToLight + gridSamplingDisk[i] * diskRadius).r;
        min_depth *= far_plane;   // undo mapping [0;1]
        if(cur_depth - bias > min_depth)
            shadow += 1.0;
    }
    shadow /= float(samples);

    // ============= pcf soft shadow ==============
//    float samples = 4.0;
//    float offset = 0.1;
//    for(float x = -offset; x < offset; x += offset / (samples * 0.5))
//    {
//        for(float y = -offset; y < offset; y += offset / (samples * 0.5))
//        {
//            for(float z = -offset; z < offset; z += offset / (samples * 0.5))
//            {
//                float min_depth = texture(shadowMap, fragToLight + vec3(x, y, z)).r;
//                min_depth *= far_plane;   // Undo mapping [0;1]
//                if(cur_depth - bias > min_depth)
//                    shadow += 1.0;
//            }
//        }
//    }
//    shadow /= (samples * samples * samples);

    // ============ 硬阴影 =========
    //阴影偏移
//    min_depth = texture(depthMap, fragToLight).r;
//    min_depth *= far_plane;
//    shadow = cur_depth - bias > min_depth ? 1.0 : 0.0;

    return shadow;
//    return min_depth;
}
void main()
{
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);
    vec3 lightColor = vec3(0.3);
    //环境光
    vec3 ambient = 0.3 * color;

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
//    float shadow = ShadowCalculation(fs_in.FragPos);
    float shadow = shadows ? ShadowCalculation(fs_in.FragPos) : 0.0;
    vec3 lighting = ambient + (1.0 - shadow) * (diffuse + specular) * color;

    FragColor = vec4(lighting, 1.0);

}

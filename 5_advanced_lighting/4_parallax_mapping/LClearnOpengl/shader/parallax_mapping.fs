#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentFragPos;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
}fs_in;

uniform sampler2D diffuseTexture;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform float height_scale;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    //  1. 普通视差贴图方式 -------------
//    float height =  texture(depthMap, texCoords).r;
//    vec2 p =  viewDir.xy / viewDir.z * (height * heightScale)
//    return texCoords - p; // p与v同向，原纹理坐标减去v的逆向偏移

    //  2. 陡峭视差映射 ，取第一次人工设定深度低于采样深度时的偏移纹理-------------
    //
    // 深度层数，根据观察方向与（切线空间）表面法线夹角调整样本层数
//    const float minLayers = 8;
//    const float maxLayers = 32;
//    float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));
//    // 每层深度
//    float layerDepth = 1.0 / numLayers;
//    vec2 p = viewDir.xy / viewDir.z * height_scale;
//    vec2 deltaTexCoords = p / numLayers;
//
//    vec2 curTexCoords = texCoords;
//    float curDepthMapValue = texture(depthMap, curTexCoords).r;
//    float curLayerDepth = 0.0;
//    while (curLayerDepth < curDepthMapValue) {
//        curTexCoords -= deltaTexCoords;
//        curDepthMapValue = texture(depthMap, curTexCoords).r;
//        curLayerDepth += layerDepth;
//    }
//
//    return curTexCoords;

    // 3.视差遮蔽视图，偏移纹理取碰撞前后的纹理坐标的线性插值  ----------------- 
    const float minLayers = 8;
    const float maxLayers = 32;
    float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));
    // 每层深度
    float layerDepth = 1.0 / numLayers;
    vec2 p = viewDir.xy / viewDir.z * height_scale;
    // 纹理坐标单次偏移量
    vec2 deltaTexCoords = p / numLayers;

    vec2 curTexCoords = texCoords;
    float curDepthMapValue = texture(depthMap, curTexCoords).r;
    float curLayerDepth = 0.0;
    while (curLayerDepth < curDepthMapValue) {
        curTexCoords -= deltaTexCoords;
        curDepthMapValue = texture(depthMap, curTexCoords).r;
        curLayerDepth += layerDepth;
    }
    // 碰撞前后的偏移纹理，进行线性插值
    vec2 preTexCoords = curTexCoords - deltaTexCoords; // 碰撞前纹理坐标
    float beforeDepth = texture(depthMap, preTexCoords).r - curLayerDepth + layerDepth; // 前深度差
    float afterDepth = curDepthMapValue - curLayerDepth; // 后深度差
    
    float weight = afterDepth / (afterDepth - beforeDepth);
    vec2 finalTexCoords = preTexCoords * weight + curTexCoords * (1 - weight);
    
    return finalTexCoords;
}

void main()
{
    //用视差贴图获得偏移后的纹理坐标
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = ParallaxMapping(fs_in.TexCoords, viewDir);
    //处理边缘纹理坐标超过采样范围时造成的失真
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
    {
        discard;
    }
    //使用偏移后的纹理坐标进行光照计算
    vec3 color = texture(diffuseTexture, texCoords).rgb;
    vec3 normal = texture(normalMap, texCoords).rgb;
    normal = normalize(normal * 2.0 - 1.0);
    vec3 lightColor = vec3(0.5);
    //环境光
    vec3 ambient = 0.3 * color;

    //漫反射
    vec3 lightDir = normalize(fs_in.TangentFragPos - fs_in.TangentLightPos);
    float diff = max(dot(-lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    //高光
    vec3 halfwayDir = normalize(-lightDir + viewDir);
    float spec = pow(max(dot(halfwayDir, normal), 0.0), 64.0);
    vec3 specular = spec * lightColor;

    vec3 lighting = ambient + (diffuse + specular) * color;

    FragColor = vec4(lighting, 1.0);

}

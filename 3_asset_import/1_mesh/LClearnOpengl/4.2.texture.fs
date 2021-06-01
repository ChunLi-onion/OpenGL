//
//  4.2.texture.cpp
//  LClearnOpengl
//
//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.

#version 330 core
out vec4 FragColor;

struct Material {
    sampler2D diffuse; //移除环境光，因为它在任何情况下都等于漫反射颜色
    sampler2D specular;
    float shininess;
};
struct DirLight {
    vec3 direction;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

struct PointLight {
    vec3 position;
    
    float constant; // 光衰减因子： 常数项 一次项 二次项
    float linear;
    float quadratic;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;//镜面强度，不要产生过度影响
};

struct SpotLight {
    vec3 position; //定向光不再需要位置，而是用方向来表示，点光源和聚光灯使用position
    vec3 direction; //从光源出发的全局方向,定向光和聚光灯使用
    
    float cutoff; //切光角
    float outercutoff; // 外圆锥切光角
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;//镜面强度，不要产生过度影响
    
    float constant; // 光衰减因子： 常数项 一次项 二次项
    float linear;
    float quadratic;
};

#define NR_POINT_LIGHTS 4
in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

uniform DirLight dirLight;
uniform PointLight pointLights[NR_POINT_LIGHTS];
uniform SpotLight spotLight;
uniform Material material;
uniform vec3 viewPos;

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir);

void main()
{
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);
    //定向光
    vec3 result = CalcDirLight(dirLight, norm, viewDir);
    //点光源
//    vec3 result;
    for (int i = 0; i < NR_POINT_LIGHTS; i++) {
        result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);
    }
    //聚光灯
    result += CalcSpotLight(spotLight, norm, FragPos, viewDir);
    
    FragColor = vec4(result, 1.0);
}

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir) {
    vec3 lightDir = normalize(-light.direction);
    
    //diffuse
    float diff = max(dot(normal, lightDir), 0.0f);
    vec3 diffuse = diff * light.diffuse * vec3(texture(material.diffuse, TexCoords));
    
    //specular
    vec3 reflectDir = normalize(reflect(light.direction, normal));
    float spec = pow(max(dot(reflectDir, viewDir), 0), material.shininess);
    vec3 specular = spec * light.specular * vec3(texture(material.specular, TexCoords));
    
    //ambient
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    
    return (ambient + diffuse + specular);
}
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    vec3 lightDir = normalize(light.position - fragPos); //点光源，聚光灯
    
    //衰减
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    //diffuse
    float diff = max(dot(normal, lightDir), 0.0f);
    vec3 diffuse = diff * light.diffuse * vec3(texture(material.diffuse, TexCoords));
    
    //specular
    vec3 reflectDir = normalize(reflect(-lightDir, normal));
    float spec = pow(max(dot(reflectDir, viewDir), 0), material.shininess);
    vec3 specular = spec * light.specular * vec3(texture(material.specular, TexCoords));
    
    //ambient
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords));
    
    //我们可以将环境光分量保持不变，让环境光照不会随着距离减少，但是如果我们使用多于一个的光源，所有的环境光分量将会开始叠加，所以在这种情况下我们也希望衰减环境光照
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    
    return (ambient + diffuse + specular);
}
vec3 CalcSpotLight(SpotLight light, vec3 normal, vec3 fragPos, vec3 viewDir) {
    vec3 lightDir = normalize(light.position - fragPos); //点光源，聚光灯
    //聚光灯
    float theta = dot(lightDir, normalize(-light.direction));
    float gapCutoff = light.cutoff - light.outercutoff;
    float intensity = clamp((theta - light.outercutoff) / gapCutoff, 0.0f, 1.0f);
 
    //在切光角内，则执行光照运算,否则只有环境光
    //衰减
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));
    
    //环境光
    vec3 ambient = light.ambient * vec3(texture(material.diffuse, TexCoords)); // 材质属性 * 对应光照分量 = 环境光给予物体的亮度
    
    //漫反射 Diffuse
    float diff = max(dot(lightDir, normal), 0.0f); // 夹角造成的影响
    vec3 diffuse = light.diffuse * diff * vec3(texture(material.diffuse, TexCoords)); // 光源给予物体的漫反射部分能量
    
    //Specular 镜面光
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(reflectDir, viewDir), 0.0f), material.shininess); // 高光度shininess设为32，其值越高，高光点越小
    vec3 specular = light.specular * vec3(texture(material.specular, TexCoords)) * spec;
    
    //我们可以将环境光分量保持不变，让环境光照不会随着距离减少，但是如果我们使用多于一个的光源，所有的环境光分量将会开始叠加，所以在这种情况下我们也希望衰减环境光照
    // intensity 消除聚光灯光圈硬边，内外圆锥之间强度
    ambient *= attenuation;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;

    return (ambient + diffuse + specular);  // 三个分量均为每个材质属性乘上它们对应的光照分量
}

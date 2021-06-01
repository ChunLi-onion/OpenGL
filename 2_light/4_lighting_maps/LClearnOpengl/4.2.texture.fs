//
//  4.2.texture.cpp
//  LClearnOpengl
//
//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.

#version 330 core
in vec3 Normal;
in vec3 FragPos;
in vec2 TexCoords;

out vec4 FragColor;

struct Material {
    sampler2D diffuse; //移除环境光，因为它在任何情况下都等于漫反射颜色
    sampler2D specular;
    sampler2D emission;
    float shininess;
};
struct Light {
    vec3 position;
    
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;//镜面强度，不要产生过度影响
};

uniform Light light;
uniform Material material;
uniform float matrixlight;
uniform float matrixmove; // 0-1
uniform vec3 viewPos;

void main()
{
    //环境光
    vec3 ambient = vec3(texture(material.diffuse, TexCoords)) * light.ambient; // 材质属性 * 对应光照分量 = 环境光给予物体的亮度
    
    //漫反射 Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(lightDir, norm), 0); // 夹角造成的影响
    vec3 diffuse = diff * light.diffuse * vec3(texture(material.diffuse, TexCoords)); // 光源给予物体的漫反射部分能量
    
    //Specular 镜面光
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(reflectDir, viewDir), 0.0f), material.shininess); // 高光度shininess设为32，其值越高，高光点越小
    vec3 specular = vec3(texture(material.specular, TexCoords)) * spec * light.specular;
    
    //放射光贴图
    vec3 emission = vec3(texture(material.emission, vec2(TexCoords.x, matrixmove + TexCoords.y))) * matrixlight;
    vec3 result = ambient + diffuse + specular + emission;  // 三个分量均为每个材质属性乘上它们对应的光照分量
    FragColor = vec4(result, 1.0);
}

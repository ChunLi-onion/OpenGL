//
//  4.2.texture.cpp
//  LClearnOpengl
//
//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.

#version 330 core
in vec3 Normal;
in vec3 FragPos;

out vec4 FragColor;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
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
uniform vec3 viewPos;

void main()
{
    //环境光
    vec3 ambient = material.ambient * light.ambient; // 材质属性 * 对应光照分量 = 环境光给予物体的亮度
    
    //漫反射 Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - FragPos);
    float diff = max(dot(lightDir, norm), 0); // 夹角造成的影响
    vec3 diffuse = (diff * material.diffuse) * light.diffuse; // 光源给予物体的漫反射部分能量
    
    //Specular 镜面光
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(reflectDir, viewDir), 0.0f), material.shininess); // 高光度shininess设为32，其值越高，高光点越小
    vec3 specular = (material.specular * spec) * light.specular;
    
    vec3 result = ambient + diffuse + specular;  // 三个分量均为每个材质属性乘上它们对应的光照分量
    FragColor = vec4(result, 1.0);
}

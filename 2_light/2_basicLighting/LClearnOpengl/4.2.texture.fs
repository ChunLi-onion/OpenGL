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

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;

void main()
{
    //环境光
    float ambientStrenght = 0.05;
    vec3 ambient = ambientStrenght * lightColor; // 环境光给予物体的亮度
    
    //漫反射 Diffuse
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(lightDir, norm), 0); // 夹角造成的影响
    vec3 diffuse = diff * lightColor; // 光源给予物体的漫反射部分能量
    
    //Specular 镜面光
    float specularStrength = 0.5f; //镜面强度，不要产生过度影响
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(reflectDir, viewDir), 0.0f), 32); // 高光度设为32，其值越高，高光点越小
    vec3 specular = specularStrength * spec * lightColor;
    
    vec3 result = (ambient + diffuse + specular) * objectColor;
    FragColor = vec4(result, 1.0);
}

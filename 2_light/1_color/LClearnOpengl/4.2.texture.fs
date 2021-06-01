//
//  4.2.texture.cpp
//  LClearnOpengl
//
//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.

#version 330 core
out vec4 FragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
void main()
{
    FragColor = vec4(lightColor * objectColor, 1.0);
}

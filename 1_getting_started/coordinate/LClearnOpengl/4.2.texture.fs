//
//  4.2.texture.cpp
//  LClearnOpengl
//
//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.
//
//
//#version 330 core
//out vec4 FragColor;
//
//in vec3 ourColor;
//in vec2 TexCoord;
//
////纹理采样器
//uniform sampler2D texture1;
//uniform sampler2D texture2;
//
//void main() {
//    //线性插值两个纹理，80%第一个纹理，20% 第二个纹理
//    FragColor = mix(texture(texture1, TexCoord), texture(texture2, TexCoord), 0.2);
//}

#version 330 core
out vec4 FragColor;

in vec2 TexCoord;

// texture samplers
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform float rate;

void main()
{
    // linearly interpolate between both textures (80% container, 20% awesomeface)
    //    FragColor = 0.1 * tex1 + 0.9 * texture(texture2, TexCoord);
    FragColor = mix(texture(texture1, TexCoord), texture(texture2, vec2(1 - TexCoord.x, TexCoord.y)), rate);
}

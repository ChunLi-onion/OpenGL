//  Created by 李春 on 2021/5/21.
//  Copyright © 2021 李春. All rights reserved.

#version 330 core
in vec3 FragPos;
in vec2 TexCoords;

out vec4 FragColor;
uniform sampler2D texture1;

void main()
{
    FragColor = texture(texture1, TexCoords);
}

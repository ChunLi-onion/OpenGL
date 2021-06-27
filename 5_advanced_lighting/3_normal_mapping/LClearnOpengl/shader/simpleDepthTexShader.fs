#version 330 core

in vec4 FragPos; //着色点世界空间坐标

uniform vec3 lightPos;
uniform float far_plane;

void main()
{
    float lightDistance = length(FragPos.xyz - lightPos); //着色点与光源之间的世界距离
    lightDistance /= far_plane; //映射到[0, 1]
    
    gl_FragDepth = lightDistance;
    
}

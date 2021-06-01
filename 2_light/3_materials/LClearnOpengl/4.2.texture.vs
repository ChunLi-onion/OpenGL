//#version 330 core
//layout (location = 0) in vec3 aPos;
//layout (location = 1) in vec3 aColor;
//latout (location = 2) in vec2 aTexCoord;
//
//out vec3 ourColor;
//out vec2 TexCoord;
//
//void main() {
//    gl_Position = vec4(aPos, 1.0);
//    outColor = aColor;
//    TexColor = aTexCoord;
//}
#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 Normal;
out vec3 FragPos;
void main()
{
    Normal = mat3(transpose(inverse(model))) * aNormal;
    FragPos = vec3(model * vec4(aPos, 1.0)); // 顶点在世界空间中的坐标
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}

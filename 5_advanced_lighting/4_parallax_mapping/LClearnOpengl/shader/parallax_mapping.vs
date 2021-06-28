
#version 330 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 texCoords;
layout (location = 3) in vec3 tangent;
layout (location = 4) in vec3 bitangent;

out VS_OUT {
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentFragPos;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
}vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform vec3 lightPos;
uniform vec3 viewPos;

void main()
{
    vs_out.FragPos = vec3(model * vec4(position, 1.0));
    vs_out.TexCoords = texCoords;
    
    // 共享法线柔化后，解决切线与法线不再正交问题
// 1.使用model矩阵转至世界空间 ------------
//    vec3 T = normalize(vec3(model * vec4(tangent, 0.0)));
//    vec3 N = normalize(vec3(model * vec4(normal, 0.0)));
//    // re-orthogonalize T with respect to N
//    T = normalize(T - dot(T, N) * N);
//    // then retrieve perpendicular vector B with the cross product of T and N
//    vec3 B = cross(T, N);
    
    // 2. 使用法线矩阵到世界空间，更精确，去除平移和缩放变换带来的错误
    mat3 normal_matrix = transpose(inverse(mat3(model)));
    vec3 T = normalize(normal_matrix * tangent);
    vec3 N = normalize(normal_matrix *normal);
    // re-orthogonalize T with respect to N
    T = normalize(T - dot(T, N) * N);
    // then retrieve perpendicular vector B with the cross product of T and N
    vec3 B = cross(T, N);

// 3. 最简单tbn构造，不考虑柔滑不考虑缩放平移问题
//    vec3 T = normalize(vec3(model * vec4(tangent, 0.0)));
//    vec3 B = normalize(vec3(model * vec4(bitangent, 0.0)));
//    vec3 N = normalize(vec3(model * vec4(normal, 0.0)));

    mat3 TBN = transpose(mat3(T, B, N)); // 世界转切线
    vs_out.TangentFragPos = TBN * vs_out.FragPos;
    vs_out.TangentLightPos = TBN * lightPos;
    vs_out.TangentViewPos = TBN * viewPos;
    gl_Position = projection * view * model * vec4(position, 1.0);
}

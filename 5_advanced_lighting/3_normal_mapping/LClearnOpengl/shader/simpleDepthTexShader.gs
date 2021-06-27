#version 330 core
layout (triangles) in;
layout (triangle_strip, max_vertices=18) out;

uniform mat4 shadowMatrices[6];

out vec4 FragPos; //从gs到fs的着色点世界空间坐标，用来计算与光源的线性距离

void main()
{
    for (int face = 0; face < 6; ++face) {
        gl_Layer = face; // 该内建变量表示当前渲染cubemap的哪个面
        for(int i = 0; i < 3; ++i) {
            FragPos = gl_in[i].gl_Position;
            gl_Position = shadowMatrices[face] * FragPos;
            EmitVertex();
        }
        EndPrimitive();
    }
}


#version 330 core
layout (location = 0) out vec3 gPositionDepth;
layout (location = 1) out vec3 gNormal;
layout (location = 2) out vec3 gAlbedo;

in VS_OUT { //观察空间
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

void main()
{
    // 向Gbuffer颜色缓冲中存储几何数据(观察空间）
    gPositionDepth = fs_in.FragPos;
    
    gNormal = normalize(fs_in.Normal);
    
    gAlbedo.rgb = vec3(0.95);
}

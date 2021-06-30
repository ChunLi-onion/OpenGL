#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D brightImage;

uniform bool horizontal;
uniform float weight[5] = float[] (0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162);

void main()
{
    //两步高斯模糊，水平8次采样，垂直8次采样，两个fbo的colorbuffer来回交替模糊10次
    vec2 tex_offset = 1.0 / textureSize(brightImage, 0); // gets size of single texel
     vec3 result = texture(brightImage, TexCoords).rgb * weight[0];
     if(horizontal)
     {
         for(int i = 1; i < 5; ++i)
         {
            result += texture(brightImage, TexCoords + vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
            result += texture(brightImage, TexCoords - vec2(tex_offset.x * i, 0.0)).rgb * weight[i];
         }
     }
     else
     {
         for(int i = 1; i < 5; ++i)
         {
             result += texture(brightImage, TexCoords + vec2(0.0, tex_offset.y * i)).rgb * weight[i];
             result += texture(brightImage, TexCoords - vec2(0.0, tex_offset.y * i)).rgb * weight[i];
         }
     }
     FragColor = vec4(result, 1.0);
}

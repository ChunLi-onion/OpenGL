#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 Position;

uniform samplerCube skybox;
uniform vec3 cameraPos;

void main()
{
    vec3 I = normalize(Position - cameraPos); // world space
    
    // reflect 反射 -----------
//    vec3 R = reflect(I, normalize(Normal));
//    FragColor = vec4(texture(skybox, R).rgb, 1.0);

    // refract 折射 -----------
    float ratio = 1.00 / 1.52;
    vec3 R = refract(I, normalize(Normal), ratio);
    FragColor = vec4(texture(skybox, R).rgb, 1.0);
}

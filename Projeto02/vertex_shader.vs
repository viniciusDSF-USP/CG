#version 330
layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texture_coord;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform float time;
uniform bool isWater;

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    
    // Se for água, desloca a coordenada U com o tempo para simular fluxo
    if (isWater) {
        TexCoord = vec2(texture_coord.x + time * 0.05, texture_coord.y + time * 0.02);
    } else {
        TexCoord = texture_coord;
    }
}
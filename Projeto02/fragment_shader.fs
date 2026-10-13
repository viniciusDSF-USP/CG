#version 330
out vec4 FragColor;

in vec2 TexCoord;

uniform sampler2D samplerTexture;
uniform float time;
uniform bool isLantern;

void main()
{
    vec4 texColor = texture(samplerTexture, TexCoord);
    
    // Se for o pixel transparente (alpha baixo), descarta
    if (texColor.a < 0.1) {
        discard;
    }

    vec3 finalColor = texColor.rgb;

    // Se for lanterna, faz o brilho pulsar com uma onda senoidal
    if (isLantern) {
        float pulse = 0.85 + 0.15 * sin(time * 4.0);
        finalColor *= pulse;
    }

    FragColor = vec4(finalColor, texColor.a);
}

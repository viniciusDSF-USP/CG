#version 120

varying vec2 out_texture;
uniform sampler2D samplerTexture;
uniform vec3 tint;   // cor multiplicada na textura (1,1,1 = sem tinta)

void main(){
    vec4 texture = texture2D(samplerTexture, out_texture);
    if (texture.a < 0.1) discard;   // pixels transparentes (folhas, portas, ...)
    gl_FragColor = vec4(texture.rgb * tint, texture.a);
}

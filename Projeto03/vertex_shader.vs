#version 330

// Atributos de cada vértice (um VBO para cada um)
in vec3 posicao;           // Posição local (espaço do objeto)
in vec2 coord_textura;     // Coordenada (u, v) da textura
in vec3 normal;            // Normal da face (espaço do objeto)

// Dados enviados ao fragment shader (interpolados entre os vértices)
out vec2 coordTextura;
out vec3 posMundo;         // Posição do fragmento no mundo (para calcular direção da luz e do observador)
out vec3 normalMundo;      // Normal no mundo

// Transformações
uniform mat4 model;        // Objeto -> Mundo
uniform mat4 view;         // Mundo  -> Câmera
uniform mat4 projection;   // Câmera -> Tela
uniform mat3 normalMatrix; // Inversa transposta da parte 3x3 de 'model' (leva normais ao mundo sem deformá-las)

// Animação da água
uniform float tempo;       // Segundos desde o início do programa
uniform bool  ehAgua;      // Água e lava "escorrem" deslocando a textura

void main()
{
    vec4 posicaoNoMundo = model * vec4(posicao, 1.0);

    posMundo    = posicaoNoMundo.xyz;
    normalMundo = normalMatrix * normal;
    gl_Position = projection * view * posicaoNoMundo;

    // Na água, a textura desliza com o tempo para simular o fluxo
    vec2 uv = coord_textura;
    if (ehAgua) {
        uv += vec2(0.05, 0.02) * tempo;
    }
    coordTextura = uv;
}

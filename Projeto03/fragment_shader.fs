#version 330
#define MAX_LUZES 8

// Dados vindos do vertex shader
in vec2 coordTextura;
in vec3 posMundo;
in vec3 normalMundo;

out vec4 corFinal;

// Luz pontual
// 'ambiente' diz qual ambiente ela ilumina (0 = externo, 1 = interno)
struct LuzPontual {
    vec3  pos;        // posição no mundo
    vec3  cor;        // cor e intensidade
    int   ambiente;   // ambiente que ela ilumina
    bool  ligada;     // "interruptor"
    float kc;         // atenuação com a distância d:  1 / (kc + kl*d + kq*d²)
    float kl;
    float kq;
};

// Material do objeto que está sendo desenhado
struct Material {
    float ka;         // reflexão ambiente
    float kd;         // reflexão difusa
    float ks;         // reflexão especular
    float brilho;     // expoente especular (quanto maior, mais "polido")
    float emissao;    // luz própria (lanterna acesa, tocha acesa, cubo)
};

uniform sampler2D texturaObjeto;

// Iluminação da cena
uniform LuzPontual luzes[MAX_LUZES];
uniform int        numLuzes;
uniform vec3       luzAmbiente;     // cor * intensidade da luz ambiente (zero se desligada)
uniform vec3       posCamera;       // posição do observador no mundo

// Objeto atual
uniform Material material;
uniform int      ambienteObjeto;    // 0 = externo, 1 = interno

// Controles globais do teclado (multiplicam kd e ks de todos os objetos)
uniform float fatorDifusa;
uniform float fatorEspecular;

// Céu não é iluminado, só escurece/clareia junto com a luz ambiente
uniform bool  ehCeu;
uniform float brilhoCeu;

// Iluminação
// Soma a contribuição difusa e especular de uma luz pontual nos acumuladores.
void somaLuz(LuzPontual luz, vec3 normal, vec3 paraObservador, inout vec3 difusa, inout vec3 especular)
{
    vec3  paraLuz = luz.pos - posMundo;
    float dist    = length(paraLuz);
    paraLuz /= dist;

    float atenuacao = 1.0 / (luz.kc + luz.kl * dist + luz.kq * dist * dist);

    // Difusa (Lambert): depende do ângulo entre a normal e a direção da luz
    float cosDifusa = max(dot(normal, paraLuz), 0.0);
    difusa += luz.cor * cosDifusa * atenuacao;

    // Especular (Blinn-Phong): só existe se a face estiver voltada para a luz
    if (cosDifusa > 0.0) {
        vec3  meio        = normalize(paraLuz + paraObservador);
        float cosEspecular = max(dot(normal, meio), 0.0);
        especular += luz.cor * pow(cosEspecular, material.brilho) * atenuacao;
    }
}

void main()
{
    vec4 texel = texture(texturaObjeto, coordTextura);

    // Pixels transparentes da textura (alpha baixo) não são desenhados
    if (texel.a < 0.1) {
        discard;
    }

    // O céu só é escurecido/clareado, sem cálculo de luz
    if (ehCeu) {
        corFinal = vec4(texel.rgb * brilhoCeu, texel.a);
        return;
    }

    vec3 normal          = normalize(normalMundo);
    vec3 paraObservador  = normalize(posCamera - posMundo);

    // Superfícies finas (quadro, flores, água) são vistas dos dois lados:
    // Vira a normal para o lado de quem olha
    if (dot(normal, paraObservador) < 0.0) {
        normal = -normal;
    }

    // Ambiente: igual em qualquer ponto, não depende de posição nem direção
    vec3 ambiente = material.ka * luzAmbiente;

    // Difusa e especular: soma das luzes pontuais que iluminam o ambiente deste objeto
    vec3 difusa    = vec3(0.0);
    vec3 especular = vec3(0.0);

    for (int i = 0; i < numLuzes; i++) {
        // Ignora luz apagada ou de outro ambiente (o cubo do quintal não ilumina a sala)
        if (luzes[i].ligada && luzes[i].ambiente == ambienteObjeto) {
            somaLuz(luzes[i], normal, paraObservador, difusa, especular);
        }
    }

    vec3 cor = texel.rgb * (ambiente + material.kd * fatorDifusa * difusa)   // Pintam com a textura
             + material.ks * fatorEspecular * especular                      // Tem a cor da luz
             + material.emissao * texel.rgb;                                 // Luz própria do objeto

    corFinal = vec4(min(cor, vec3(1.0)), texel.a);
}

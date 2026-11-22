import numpy as np
from OpenGL.GL import *

class Shader:
    def __init__(self, caminho_vertex: str, caminho_fragment: str):
        vertex = self._compila(GL_VERTEX_SHADER, caminho_vertex)
        fragment = self._compila(GL_FRAGMENT_SHADER, caminho_fragment)

        # Linka os dois shaders em um único programa
        self.ID = glCreateProgram()
        glAttachShader(self.ID, vertex)
        glAttachShader(self.ID, fragment)
        glLinkProgram(self.ID)
        if not glGetProgramiv(self.ID, GL_LINK_STATUS):
            raise RuntimeError("Erro ao linkar o programa:\n" + glGetProgramInfoLog(self.ID).decode())

        # Depois de linkados, os objetos de shader individuais não são mais necessários
        glDeleteShader(vertex)
        glDeleteShader(fragment)

        self._locations = {}   # cache: nome do uniform -> location

    @staticmethod
    def _compila(tipo, caminho: str) -> int:
        # Lê o código-fonte de `caminho` e compila. Levanta RuntimeError se houver erro.
        with open(caminho) as arquivo:
            codigo = arquivo.read()

        shader = glCreateShader(tipo)
        glShaderSource(shader, codigo)
        glCompileShader(shader)
        if not glGetShaderiv(shader, GL_COMPILE_STATUS):
            raise RuntimeError("Erro ao compilar %s:\n%s" % (caminho, glGetShaderInfoLog(shader).decode()))
        return shader

    def getProgram(self) -> int:
        #Id do programa (usado por `glGetAttribLocation`, por exemplo).
        return self.ID

    def use(self) -> None:
        #Ativa este programa. Os uniforms enviados depois valem para ele.
        glUseProgram(self.ID)

    def attribLocation(self, nome: str) -> int:
        #Endereço de um atributo de vértice (`in`) do vertex shader.
        return glGetAttribLocation(self.ID, nome)

    def _loc(self, nome: str) -> int:
        #Endereço do uniform `nome` (consultado só na primeira vez).
        #Aceita campos de struct e de array: "material.ka", "luzes[2].pos".
        if nome not in self._locations:
            self._locations[nome] = glGetUniformLocation(self.ID, nome)
        return self._locations[nome]

    def setBool(self, nome: str, valor: bool) -> None:
        glUniform1i(self._loc(nome), int(valor))

    def setInt(self, nome: str, valor: int) -> None:
        glUniform1i(self._loc(nome), valor)

    def setFloat(self, nome: str, valor: float) -> None:
        glUniform1f(self._loc(nome), valor)

    def setVec3(self, nome: str, x: float, y: float, z: float) -> None:
        glUniform3f(self._loc(nome), x, y, z)

    def setMat3(self, nome: str, matriz, transpor: bool = True) -> None:
        #Matriz 3x3. `transpor=True`: `matriz` está linha a linha (como o numpy guarda).
        glUniformMatrix3fv(self._loc(nome), 1, GL_TRUE if transpor else GL_FALSE,
                           np.ascontiguousarray(matriz, dtype=np.float32))

    def setMat4(self, nome: str, matriz, transpor: bool = True) -> None:
        #Matriz 4x4. `transpor=True`: `matriz` está linha a linha (como o numpy guarda).
        glUniformMatrix4fv(self._loc(nome), 1, GL_TRUE if transpor else GL_FALSE,
                           np.ascontiguousarray(matriz, dtype=np.float32))

#version 460 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoords;

out vec2 TexCoords;
out vec3 FragPos;
out vec3 Normals;
uniform mat4 viewMatrix;
uniform mat4 projMatrix;
uniform mat4 modelMatrix;



void main()
{
	FragPos = vec3(modelMatrix * vec4(aPos, 1.0));
    TexCoords = aTexCoords;
	Normals = aNormal;
    gl_Position = vec4(aPos, 1.0) * modelMatrix * viewMatrix * projMatrix;
}
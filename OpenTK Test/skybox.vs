#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projMatrix;
uniform mat4 viewMatrix;

void main()
{
	TexCoords = aPos;
	vec4 MVP_Pos = vec4(aPos, 0) * viewMatrix * projMatrix;
	MVP_Pos.xy*=-1;
    gl_Position = MVP_Pos.xyww;	
} 
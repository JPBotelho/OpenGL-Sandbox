#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 proj;
uniform mat4 view;
uniform mat4 mvp;

void main()
{
	TexCoords = aPos;
	vec4 MVP_Pos = proj * view * vec4(aPos, 1.0);
	MVP_Pos.xy*=-1;
    gl_Position = MVP_Pos.xyww;	
} 
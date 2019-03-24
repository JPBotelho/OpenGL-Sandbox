#version 460 core

layout(location = 0) in vec3 vPos;

layout(location = 1) in vec2 texcoord;

out vec2 frag_texcoord;
uniform mat4 mvp;

void main(void)
{
	//Then, we further the input texture coordinate to the output one.
	//texCoord can now be used in the fragment shader.
	frag_texcoord = texcoord;

    gl_Position = vec4(vPos, 1.0) * mvp;
}
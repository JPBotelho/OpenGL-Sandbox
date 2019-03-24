#version 460 core

out vec4 outputColor;

in vec2 frag_texcoord;

uniform sampler2D texture0;
uniform sampler2D texture1;


void main()
{
    outputColor = mix(texture(texture0, frag_texcoord), texture(texture1, frag_texcoord), 0.2);
}
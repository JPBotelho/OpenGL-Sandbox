#version 460 core
layout (location = 0) in vec3 aPos;

uniform mat4 modelMatrix;
uniform mat4 viewProjMatrix;

void main()
{
    gl_Position = vec4(aPos, 1.0) * modelMatrix * viewProjMatrix;
}
#version 460 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture_diffuse1;

void main()
{    
	vec4 tex = texture(texture_diffuse1, TexCoords);
	if(tex.a < 0.5)
	{
		discard;
	}
	if(textureQueryLevels(texture_diffuse1) == 0)
	{
		FragColor = vec4(1);
	}
    FragColor = tex;
}
#version 460 core
out vec4 FragColor;

struct DirectionalLight
{
	vec3 direction;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct PointLight
{
	vec3 position;
	float constant;
	float linear;
	float quadratic;
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

struct Spotlight 
{
    vec3 position;
    vec3 direction;
    float cutoff;
    float outercutoff;
  
    float constant;
    float linear;
    float quadratic;
  
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;       
};

#define NR_POINT_LIGHTS 1


in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normals;

uniform samplerCube depthMap;
uniform mat4 cubeProjMatrix;
uniform vec3 cameraPos;

float chebyshevNorm(in vec3 dir)
{
    vec3 tmp = abs(dir);
    return max(max(tmp.x,tmp.y),tmp.z);
}

void main()
{  
	vec3 fragToLight = FragPos - vec3(0, 1, 3); 
	float closestDepth = texture(depthMap, fragToLight).r;

    float lightChebyshev = -chebyshevNorm(fragToLight); // linear depth
	vec4 postProjPos = vec4(0.0,0.0,lightChebyshev,1.0) * cubeProjMatrix;
	float NDC_z = postProjPos.z/postProjPos.w;
	float Window_z = NDC_z*0.5+0.5;
	FragColor = vec4(Window_z < closestDepth ? 1.0:0.0);
}
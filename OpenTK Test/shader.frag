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
in mat4 mvp;

uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform vec3 cameraPos;

uniform DirectionalLight directionalLights[1];
uniform Spotlight spotlights[1];
uniform PointLight pointLights[NR_POINT_LIGHTS];

vec3 CalcDirLight(DirectionalLight light, vec3 normal, vec3 viewDir);
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir);
vec3 CalcSpotLight(Spotlight light, vec3 normal, vec3 fragPos, vec3 viewDir);
float materialshininess= 32.0f;

uniform sampler2D shadowAtlases;
uniform mat4 cubeProjMatrix;

vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  1), vec3( 1, -1,  1), vec3(-1, -1,  1), vec3(-1, 1,  1), 
   vec3(1, 1, -1), vec3( 1, -1, -1), vec3(-1, -1, -1), vec3(-1, 1, -1),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  1), vec3(-1,  0,  1), vec3( 1,  0, -1), vec3(-1, 0, -1),
   vec3(0, 1,  1), vec3( 0, -1,  1), vec3( 0, -1, -1), vec3( 0, 1, -1)
);

float chebyshevNorm(in vec3 dir)
{
    vec3 tmp = abs(dir);
    return max(max(tmp.x,tmp.y),tmp.z);
}

float getCurrentDepth(vec3 fragToLight)
{
	float lightChebyshev = -chebyshevNorm(fragToLight); // linear depth
	vec4 postProjPos = vec4(fragToLight.xy, lightChebyshev,1.0) * cubeProjMatrix;
	float NDC = postProjPos.z/postProjPos.w;
	float Window = NDC*0.5+0.5;
	return Window;
}

  
vec2 getUVs(vec3 shadowCoord) {
    vec2 scale = 1.0f / textureSize(shadowAtlases, 0);
    shadowCoord.xy *= textureSize(shadowAtlases, 0);
    vec2 offset = fract(shadowCoord.xy - 0.5f);
    shadowCoord.xy -= offset*0.5f;
    return shadowCoord.xy;
}

vec2 getCorrectUVs(vec3 lightDirection)
{
	vec3 absDir = abs(lightDirection);
    vec4 project = max(absDir.x, absDir.y) > absDir.z ?
                    (absDir.x > absDir.y ?
                        vec4(lightDirection.zyx, 0.0f / 3.0f) :
                        vec4(lightDirection.xzy, 1.0f / 3.0f)) :
                    vec4(lightDirection, 2.0f / 3.0f);
    //vec4 shadowCoord = mvp * vec4(project.xy, abs(project.z), 1.0f);
	project = vec4(project.xy, abs(project.z), 1);
	vec2 scale = 1.0f / textureSize(shadowAtlases, 0);
    project.xy *= textureSize(shadowAtlases, 0);
    vec2 offset = fract(project.xy - 0.5f);
    project.xy -= offset*0.5f;
	return project.xy;
}

vec2 sampleCube(const vec3 v)
{
	vec3 vAbs = abs(v);
	float ma;
	vec2 uv;
	int faceIndex;
	if(vAbs.z >= vAbs.x && vAbs.z >= vAbs.y)
	{
		faceIndex = v.z < 0.0 ? 5 : 4;
		ma = 0.5 / vAbs.z;
		uv = vec2(v.z < 0.0 ? -v.x : v.x, -v.y);
	}
	else if(vAbs.y >= vAbs.x)
	{
		faceIndex = v.y < 0.0 ? 3 : 2;
		ma = 0.5 / vAbs.y;
		uv = vec2(v.x, v.y < 0.0 ? -v.z : v.z);
	}
	else
	{
		faceIndex = v.x < 0.0 ? 1 : 0;
		ma = 0.5 / vAbs.x;
		uv = vec2(v.x < 0.0 ? v.z : -v.z, -v.y);
	}
	vec2 faceUVS = uv * ma + 0.5;
	vec2 size = textureSize(shadowAtlases, 0);
	vec2 faceSize = vec2(size.x / 3.0, size.y / 2.0);

	faceUVS *= faceSize;
	if(faceIndex % 2 == 0)
	{
		//upper half
		faceUVS.y += size.y / 2;
	}
	else if(faceIndex % 2 != 0)
	{
		//faceUVS.y -= size.y / 2.0;
	}
	if(faceIndex == 2 || faceIndex == 3)
	{
		faceUVS.x += size.x / 3.0;
	}
	if(faceIndex == 4 || faceIndex == 5)
	{
		faceUVS.x += (size.x / 3.0) * 2.0;
	}
	return faceUVS /= size;
}

void main()
{  
	vec3 norm = normalize(Normals);
    vec3 viewDir = normalize(cameraPos - FragPos);
	vec3 result = vec3(0);

	vec3 finalResult = vec3(0);

	vec3 fragToLight = FragPos - pointLights[0].position; 

	float closestDepth = texture(shadowAtlases, sampleCube(fragToLight)).r;

	float shadow = getCurrentDepth(fragToLight) - 0.00001 < closestDepth ? 1 : 0;
	FragColor = vec4(closestDepth.xxx, 1);
	
	//for(int i = 0; i < NR_POINT_LIGHTS; i++)
	//{
	//	vec3 fragToLight = FragPos - pointLights[i].position; 

	//	vec3 result = vec3(0);
	//	result += CalcPointLight(pointLights[i], norm, FragPos, viewDir);

		//float closestDepth = texture(shadowAtlases[i], fragToLight).r;

	//	float shadow = 1;//getCurrentDepth(fragToLight) - 0.00001 < closestDepth ? 1 : 0;
	//	finalResult += max(0.0, shadow) * result;
	//}


	//FragColor = vec4(finalResult, 1);

	//FragColor = vec4(shadow.xxx, 1);
	//if(texture(texture_diffuse1, TexCoords).a < 0.5)
	//	discard;

}


vec3 CalcDirLight(DirectionalLight light, vec3 normal, vec3 viewDir)
{
    vec3 lightDir = normalize(-light.direction);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialshininess);
    // combine results
    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    return (ambient + diffuse + specular);
}

vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialshininess);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // combine results
    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    ambient *= attenuation;
    diffuse *= attenuation;
    specular *= attenuation;
    return (ambient + diffuse + specular);
}

vec3 CalcSpotLight(Spotlight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
    vec3 lightDir = normalize(light.position - fragPos);
    // diffuse shading
    float diff = max(dot(normal, lightDir), 0.0);
    // specular shading
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialshininess);
    // attenuation
    float distance = length(light.position - fragPos);
    float attenuation = 1.0 / (light.constant + light.linear * distance + light.quadratic * (distance * distance));    
    // spotlight intensity
    float theta = dot(lightDir, normalize(-light.direction)); 
    float epsilon = light.cutoff - light.outercutoff;
    float intensity = clamp((theta - light.outercutoff) / epsilon, 0.0, 1.0);
    // combine results
    vec3 ambient = light.ambient * vec3(texture(texture_diffuse1, TexCoords));
    vec3 diffuse = light.diffuse * diff * vec3(texture(texture_diffuse1, TexCoords));
    vec3 specular = light.specular * spec * vec3(texture(texture_specular1, TexCoords));
    ambient *= attenuation * intensity;
    diffuse *= attenuation * intensity;
    specular *= attenuation * intensity;
    return (ambient + diffuse + specular);
}
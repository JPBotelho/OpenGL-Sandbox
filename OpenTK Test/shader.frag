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

uniform sampler2D shadowAtlases;/*
uniform mat4 cubeProjMatrix;

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
}*/

float map(float value, float min1, float max1, float min2, float max2) 
{
	float perc = (value - min1) / (max1 - min1);

	// Do the same operation backwards with min2 and max2
	return perc * (max2 - min2) + min2;
}

float linearize(float dep)
{
	float f=300.0;
	float n = 0.1;
	float z = (2 * n) / (f + n - dep * (f - n));
	return z;
}

vec2 sampleCube(vec3 v)
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

	if(faceIndex == 0) //+X
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 0.0, 1.0/3.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, .5f, 1.0);
		return faceUVS;
	} else if(faceIndex == 1)//-X
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 0.0, 1.0/3.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, 0.0, .5);
		return faceUVS;
	} else if(faceIndex == 2)//+Y
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 1.0/3.0, 2.0/3.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, .5f, 1);
		return faceUVS;
	} else if(faceIndex == 3)//-Y
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 1.0/3.0, 2.0/3.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, 0.0, .5);
		return faceUVS;
	} else if(faceIndex == 4)//+Z
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 2.0/3.0, 1.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, .5f, 1);
		return faceUVS;
	} else if(faceIndex == 5)//-Z
	{
		faceUVS.x = map(faceUVS.x, 0.0, 1.0, 2.0/3.0, 1.0);
		faceUVS.y = map(faceUVS.y, 0.0, 1.0, 0f, .5);
		return faceUVS;
	}
	return faceUVS;
}

void main()
{  
	vec3 norm = normalize(Normals);
    vec3 viewDir = normalize(cameraPos - FragPos);
	vec3 result = vec3(0);

	vec3 finalResult = vec3(0);

	vec3 fragToLight = (FragPos - pointLights[0].position); 
	vec2 uvs = sampleCube(fragToLight);

	vec2 screnUVS = (gl_FragCoord.xy - .5) / vec2(1366.0, 768.0);
	float closestDepth = linearize(texture(shadowAtlases, sampleCube(fragToLight)).x);

	//float shadow = getCurrentDepth(fragToLight) - 0.00001 < closestDepth ? 1 : 0;
	FragColor = (closestDepth.xxxx);//vec4(vec3(uvs.x == 2), 1);
	
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
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

uniform samplerCube depthMap;
uniform mat4 cubeProjMatrix;

vec3 gridSamplingDisk[20] = vec3[]
(
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0), 
   vec3(1, 1, -0), vec3( 1, -1, -0), vec3(-1, -1, -0), vec3(-1, 1, -0),
   vec3(1, 1,  0), vec3( 1, -1,  0), vec3(-1, -1,  0), vec3(-1, 1,  0),
   vec3(1, 0,  0), vec3(-1,  0,  0), vec3( 1,  0, -0), vec3(-1, 0, -0),
   vec3(0, 1,  0), vec3( 0, -1,  0), vec3( 0, -1, -0), vec3( 0, 1, -0)
);
 
float chebyshevNorm(in vec3 dir)
{
    vec3 tmp = abs(dir);
    return max(max(tmp.x,tmp.y),tmp.z);
}

float getCurrentDepth(vec3 fragToLight)
{
	float lightChebyshev = -chebyshevNorm(fragToLight); // linear depth
	vec4 postProjPos = vec4(0.0,0.0,lightChebyshev,1.0) * cubeProjMatrix;
	float NDC_z = postProjPos.z/postProjPos.w;
	float Window_z = NDC_z*0.5+0.5;
	return Window_z;
}

void main()
{  
	vec3 norm = normalize(Normals);
    vec3 viewDir = normalize(cameraPos - FragPos);
	vec3 result = CalcPointLight(pointLights[0], norm, FragPos, viewDir);

	vec3 fragToLight = FragPos - pointLights[0].position; 
	float closestDepth = texture(depthMap, fragToLight).r;

	//

	float shadow = 0.0;
    int samples = 20;
    float diskRadius = .05;
    for(int i = 0; i < samples; ++i)
    {
        float closestDepth = texture(depthMap, fragToLight + gridSamplingDisk[i] * diskRadius).r;
        if(getCurrentDepth(fragToLight) - 0 < closestDepth)
            shadow += 1.0;
    }
    shadow = float(shadow) / float(samples);

	//shadow = getCurrentDepth(fragToLight) < closestDepth ? 1 : 0;
	
	FragColor = vec4(result.xyz * shadow, 1);
	if(texture(texture_diffuse1, TexCoords).a < 0.5)
		discard;

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
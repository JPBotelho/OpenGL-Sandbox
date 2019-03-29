#version 460 core
out vec4 FragColor;

in vec2 TexCoords;
in vec3 FragPos;
in vec3 Normals;
uniform sampler2D texture_diffuse1;
uniform sampler2D texture_specular1;
uniform vec3 cameraPos;

void main()
{    
	vec2 tCoords = TexCoords * vec2(1, -1);
	vec3 lightColor = vec3(1, 1, 1);
	float lightAmbient = 0.1;
	float lightDiffuse = 1;
	float lightSpecular = 1;
	
	vec3 materialambient = vec3(1.0f, 0.5f, 0.31f);
	vec3 materialdiffuse = vec3(1.0f, 0.5f, 0.31f);
	vec3 materialspecular = vec3(0.5f, 0.5f, 0.5f);
	float materialshininess= 32.0f;
 	
    // diffuse 
    vec3 norm = normalize(Normals);
	vec3 lightDir = normalize(vec3(-0.2f, -1.0f, -1.5));
    float diff = max(dot(norm, lightDir), 0.0);
    
    // specular
    vec3 viewDir = normalize(cameraPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), materialshininess);
	

	vec3 ambient  = lightAmbient  * vec3(texture(texture_diffuse1, tCoords));
	vec3 diffuse  = lightDiffuse  * diff * vec3(texture(texture_diffuse1, tCoords));  
	vec3 specular = lightSpecular * spec * vec3(texture(texture_specular1, tCoords));

	FragColor = vec4(ambient + diffuse + specular, 1);	
}
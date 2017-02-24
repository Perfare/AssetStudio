#version 140

in vec3 surfaceNormal;
in vec3 toLightVector;
in vec4 color;

out vec4 outputColor;

void main()
{
	vec3 lightColor = vec3(0.8, 0.8, 0.8);

	// Ambient
	float ambientStrength = 0.8;
	vec3 ambient = ambientStrength * lightColor;

	// Diffuse
	vec3 unitNormal = normalize(surfaceNormal);
	vec3 unitLightVector = normalize(toLightVector);
	float nDotProduct = dot(unitNormal, unitLightVector);
	float brightness = max(nDotProduct, 0.0);
	vec3 diffuse = brightness * lightColor;

	// Specular
	// float specularStrength = 0.5;
	// vec3 viewDirection = normalize(viewPosition - fragmentPosition);
	// vec3 reflectDirection = reflect(-lightDirection, normal);
	// float spec = pow(max(dot(viewDirection, reflectDirection), 0.0), 32);
	// vec3 specular = specularStrength * spec * lightColor;

	// Output Color
	vec4 result = color * vec4((ambient + diffuse), 0.0); // (ambient + diffuse + specular);
	outputColor = result;
}
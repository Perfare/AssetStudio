#version 140

in vec3 surfaceNormal;
in vec3 toLightVector;
in vec4 color;

out vec4 outputColor;

void main()
{
	vec3 lightColor = vec3(0.5, 0.5, 0.5);

	// Ambient
	float ambientStrength = 0.9;
	vec3 ambient = ambientStrength * lightColor;

	// Diffuse
	vec3 unitNormal = normalize(surfaceNormal);
	vec3 unitLightVector = normalize(toLightVector);
	float nDotProduct = dot(unitNormal, unitLightVector);
	float brightness = clamp(nDotProduct, 0, 1);  // max(nDotProduct, 0.0);
	vec3 diffuse = brightness * lightColor;

	// Output Color
	vec4 result = color * vec4((ambient + diffuse/2), 0.0);
	outputColor = result;
}
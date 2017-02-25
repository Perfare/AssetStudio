#version 140

in vec3 vertexPosition;
in vec3 normalDirection;
in vec4 vertexColor;
uniform mat4 viewMatrix;

out vec3 surfaceNormal;
out vec3 toLightVector;
out vec4 color;

void main()
{
	vec3 lightPosition = vec3(200.0, 200.0, 200.0);
	gl_Position = viewMatrix * vec4(vertexPosition, 1.0);
	surfaceNormal = normalDirection;
	toLightVector = lightPosition - vertexPosition;
	color = vertexColor; 
}
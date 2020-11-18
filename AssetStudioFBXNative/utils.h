#pragma once

struct Vector3 {

	float X;
	float Y;
	float Z;

	Vector3();
	Vector3(float x, float y, float z);

};

struct Quaternion {

	float X;
	float Y;
	float Z;
	float W;

	Quaternion();
	Quaternion(float x, float y, float z);
	Quaternion(float x, float y, float z, float w);

};

Vector3 QuaternionToEuler(Quaternion q);

Quaternion EulerToQuaternion(Vector3 v);

#include <fbxsdk.h>
#include <cassert>

#include "utils.h"

Vector3::Vector3()
	: X(0), Y(0), Z(0)
{
}

Vector3::Vector3(float x, float y, float z)
	: X(x), Y(y), Z(z)
{
}

Quaternion::Quaternion()
	: X(0), Y(0), Z(0), W(1)
{
}

Quaternion::Quaternion(float x, float y, float z)
	: X(x), Y(y), Z(z), W(1)
{
}

Quaternion::Quaternion(float x, float y, float z, float w)
	: X(x), Y(y), Z(z), W(w)
{
}

Vector3 QuaternionToEuler(Quaternion q) {
	FbxAMatrix lMatrixRot;
	lMatrixRot.SetQ(FbxQuaternion(q.X, q.Y, q.Z, q.W));
	FbxVector4 lEuler = lMatrixRot.GetR();
	return Vector3((float)lEuler[0], (float)lEuler[1], (float)lEuler[2]);
}

Quaternion EulerToQuaternion(Vector3 v) {
	FbxAMatrix lMatrixRot;
	lMatrixRot.SetR(FbxVector4(v.X, v.Y, v.Z));
	FbxQuaternion lQuaternion = lMatrixRot.GetQ();
	return Quaternion((float)lQuaternion[0], (float)lQuaternion[1], (float)lQuaternion[2], (float)lQuaternion[3]);
}

using UnityEngine;

public static class Matrix4x4Extensions
{
    public static Vector4 MultiplyPoint4x4(this Matrix4x4 m, Vector4 v)
    {
        Vector4 result;
        result.x = m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03 * v.w;
        result.y = m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13 * v.w;
        result.z = m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23 * v.w;
        result.w = m.m30 * v.x + m.m31 * v.y + m.m32 * v.z + m.m33 * v.w;
        return result;
    }
}
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector3 Round(this Vector3 vector)
    {
        vector.x = Mathf.Round(vector.x);
        vector.y = Mathf.Round(vector.y);
        vector.z = Mathf.Round(vector.z);
        return vector;
    }

    public static Vector3 Round(this Vector3 vector, float size)
    {
        return Round(vector / size) * size;
    }

    public static float Round(this float f, float size)
    {
        return Mathf.Round(f / size) * size;
    }
}


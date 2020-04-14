﻿using UnityEditor;
using UnityEngine;

public static class Snapper
{
    private const string k_UndoStringSnap = "Snap Objects";

    [MenuItem("Waypoint System/Snap Selected Object %&S", isValidateFunction: true)]
    public static bool SnapTheThingsValidate()
    {
        return Selection.gameObjects.Length > 0;
    }

    [MenuItem("Waypoint System/Snap Selected Object %&S")]
    public static void SnapTheThings()
    {
        foreach(GameObject gameObject in Selection.gameObjects)
        {
            Undo.RecordObject(gameObject.transform, k_UndoStringSnap);
            gameObject.transform.position = gameObject.transform.position.Round();
        }
    }

    public static Vector3 Round(this Vector3 vector)
    {
        vector.x = Mathf.Round(vector.x);
        vector.y = Mathf.Round(vector.y);
        vector.z = Mathf.Round(vector.z);
        return vector;
    }
}
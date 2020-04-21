using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

public struct SpawnData
{
    public Vector2 pointInDisc;
    public float randomAngleDegree;

    public void SetRandomValues()
    {
        pointInDisc = Random.insideUnitCircle;
        randomAngleDegree = Random.value * 360;
    }
}

public class GrimmCannon : EditorWindow
{
    [MenuItem("Tools/Grimm Cannon")]
    public static void OpenGrimm()
    {
        GetWindow<GrimmCannon>();
    }

    [Range(0f, 100f)]
    public float radius = 2f;
    [Range(0, 100)]
    public int spawnCount = 8;

    SerializedObject serializedObject;
    SerializedProperty propertyRadius;
    SerializedProperty propertySpawnCount;

    SpawnData[] spawnDataPoints;
    GameObject[] prefabs;
    GameObject spawnPrefab;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        propertyRadius = serializedObject.FindProperty("radius");
        propertySpawnCount = serializedObject.FindProperty("spawnCount");
        GenerateRandomPoints();
        SceneView.duringSceneGui += DuringSceneGUI;

        // load prefabs
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefabs" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void GenerateRandomPoints()
    {
        spawnDataPoints = new SpawnData[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            spawnDataPoints[i].SetRandomValues();
        }
    }
    
    private void OnGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(propertyRadius);
        propertyRadius.floatValue = propertyRadius.floatValue.AtLeast(1f);
        EditorGUILayout.PropertyField(propertySpawnCount);
        propertySpawnCount.intValue = propertySpawnCount.intValue.AtLeast(1);

        if (serializedObject.ApplyModifiedProperties())
        {
            GenerateRandomPoints();
            SceneView.RepaintAll();
        }

        // if you clicked left mouse button, in the editor window
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint(); // repaint on the editor window UI
        }
    }

    private void TrySpawnObjects(List<Pose> poses)
    {
        if(spawnPrefab == null)
        {
            return;
        }

        foreach (Pose pose in poses)
        {
            // spawn prefab
            GameObject spawnedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(spawnPrefab);
            Undo.RegisterCreatedObjectUndo(spawnedPrefab, "Spawn Objects");
            spawnedPrefab.transform.position = pose.position;
            spawnedPrefab.transform.rotation = pose.rotation;
        }
        GenerateRandomPoints(); // update points
    }

    private void DrawSphere(Vector3 position)
    {
        Handles.SphereHandleCap(-1, position, Quaternion.identity, 0.1f, EventType.Repaint);
    }

    bool TryRaycastFromCamera(Vector2 cameraUp, out Matrix4x4 tangentToWorld)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // setting up tangent space
            Vector3 hitNormal = hit.normal;
            Vector3 hitTangent = Vector3.Cross(hitNormal, cameraUp).normalized;
            Vector3 hitBiTangent = Vector3.Cross(hitNormal, hitTangent);
            tangentToWorld = Matrix4x4.TRS(hit.point, Quaternion.LookRotation(hitNormal, hitBiTangent), Vector3.one);
            return true;
        }
        tangentToWorld = default;
        return false;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();
        Rect rectangle = new Rect(8f, 8f, 64f, 64f);
        foreach (GameObject prefab in prefabs)
        {
            Texture icon = AssetPreview.GetAssetPreview(prefab);
            if (GUI.Toggle(rectangle, spawnPrefab == prefab, new GUIContent(icon)))
            {
                spawnPrefab = prefab;
            }
            rectangle.y += rectangle.height + 2;
        }
        Handles.EndGUI();

        Handles.zTest = CompareFunction.LessEqual;
        Transform cameraTransform = sceneView.camera.transform;

        // make sure it repaints on mouse move
        if (Event.current.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }

        // change radius
        bool holdingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;
        if (Event.current.type == EventType.ScrollWheel && holdingAlt)
        {
            float scrollDirection = Mathf.Sign(Event.current.delta.y);
            serializedObject.Update();
            propertyRadius.floatValue *= 1f + -scrollDirection * 0.05f;
            serializedObject.ApplyModifiedProperties();
            Repaint(); // updates editor window
            Event.current.Use(); // consume the event, don't let it fall through
        }

        // if the cursor is pointing on valid ground
        if (TryRaycastFromCamera(cameraTransform.up, out Matrix4x4 tangentToWorld))
        {
            // draw circle marker
            DrawCircleRegion(tangentToWorld);

            // draw all spawn positions and meshes
            List<Pose> spawnPoses = GetSpawnPoses(tangentToWorld);
            DrawSpawnPreviews(spawnPoses);

            // spawn on press
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                TrySpawnObjects(spawnPoses);
            }
        }
    }

    private void DrawSpawnPreviews(List<Pose> spawnPoses)
    {
        foreach (Pose pose in spawnPoses)
        {
            if (spawnPrefab != null)
            {
                // draw preview of all meshes in the prefab
                Matrix4x4 poseToWorld = Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
                DrawPrefab(spawnPrefab, poseToWorld);
            }
            else
            {
                // prefab missing, draw sphere and normal on surface instead
                Handles.SphereHandleCap(-1, pose.position, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.DrawAAPolyLine(pose.position, pose.position + pose.up);
            }
        }
    }

    static void DrawPrefab(GameObject prefab, Matrix4x4 poseToWorld)
    {
        MeshFilter[] filters = prefab.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in filters)
        {
            Matrix4x4 childToPose = filter.transform.localToWorldMatrix;
            Matrix4x4 childToWorld = poseToWorld * childToPose;
            Mesh mesh = filter.sharedMesh;
            Material material = filter.GetComponent<MeshRenderer>().sharedMaterial;
            material.SetPass(0);
            Graphics.DrawMeshNow(mesh, childToWorld);
        }
    }

    List<Pose> GetSpawnPoses(Matrix4x4 tangentToWorld)
    {
        List<Pose> hitPoses = new List<Pose>();
        foreach (SpawnData randomDataPoint in spawnDataPoints)
        {
            // create ray for this point
            Ray pointRay = GetCircleRay(tangentToWorld, randomDataPoint.pointInDisc);
            // raycast to find point on surface
            if (Physics.Raycast(pointRay, out RaycastHit pointHit))
            {
                // calculate rotation and assign to pose together with position
                Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomDataPoint.randomAngleDegree);
                Quaternion rotation = Quaternion.LookRotation(pointHit.normal) * (randomRotation * Quaternion.Euler(90f, 0f, 0f));
                Pose pose = new Pose(pointHit.point, rotation);
                hitPoses.Add(pose);
            }
        }
        return hitPoses;
    }

    private Ray GetCircleRay(Matrix4x4 tangentToWorld, Vector2 pointInCircle)
    {
        Vector3 normal = tangentToWorld.MultiplyVector(Vector3.forward);
        Vector3 rayOrigin = tangentToWorld.MultiplyPoint3x4(pointInCircle * radius);
        rayOrigin += normal * 2; // offset margin
        Vector3 rayDirection = -normal;
        return new Ray(rayOrigin, rayDirection);
    }

    private void DrawCircleRegion(Matrix4x4 localToWorld)
    {
        DrawAxis(localToWorld);
        // draw circle adapted to the terrain
        const int circleDetail = 128;
        Vector3[] ringPoints = new Vector3[circleDetail];
        for (int i = 0; i < circleDetail; i++)
        {
            float turn = i / ((float)circleDetail - 1); // go back to 0/1 position
            const float pi = Mathf.PI * 2;
            float angularRadius = turn * pi;
            Vector2 direction = new Vector2(Mathf.Cos(angularRadius), Mathf.Sin(angularRadius));
            Ray r = GetCircleRay(localToWorld, direction);
            if (Physics.Raycast(r, out RaycastHit rHit))
            {
                ringPoints[i] = rHit.point + rHit.normal * 0.02f;
            }
            else
            {
                ringPoints[i] = r.origin;
            }
        }
        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(ringPoints);
        Handles.color = Color.white;
    }

    private void DrawAxis(Matrix4x4 localToWorld)
    {
        Vector3 point = localToWorld.MultiplyPoint3x4(Vector3.zero);
        Handles.color = Color.red;
        Handles.DrawAAPolyLine(6, point, point + localToWorld.MultiplyVector(Vector3.right));
        Handles.color = Color.green;
        Handles.DrawAAPolyLine(6, point, point + localToWorld.MultiplyVector(Vector3.up));
        Handles.color = Color.blue;
        Handles.DrawAAPolyLine(6, point, point + localToWorld.MultiplyVector(Vector3.forward));
        Handles.color = Color.white;
    }
}
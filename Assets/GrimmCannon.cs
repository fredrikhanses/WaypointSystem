using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

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

    Vector2[] randomPoints;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        propertyRadius = serializedObject.FindProperty("radius");
        propertySpawnCount = serializedObject.FindProperty("spawnCount");
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void GenerateRandomPoints()
    {
        randomPoints = new Vector2[spawnCount];
        for (int i = 0; i < spawnCount; i++)
        {
            randomPoints[i] = Random.insideUnitCircle;
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
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint();
        }
    }

    private void DrawSphere(Vector3 position)
    {
        Handles.SphereHandleCap(-1, position, Quaternion.identity, 0.1f, EventType.Repaint);
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        Handles.zTest = CompareFunction.LessEqual;
        Transform cameraTransform = sceneView.camera.transform;
        if (Event.current.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }
        bool holdingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;
        if (Event.current.type == EventType.ScrollWheel && holdingAlt)
        {
            float scrollDirection = Mathf.Sign(Event.current.delta.y);
            serializedObject.Update();
            propertyRadius.floatValue *= 1f + -scrollDirection * 0.05f;
            serializedObject.ApplyModifiedProperties();
            Repaint();
            Event.current.Use();
        }
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 hitNormal = hit.normal;
            Vector3 hitTangent = Vector3.Cross(hitNormal, cameraTransform.up).normalized;
            Vector3 hitBiTangent = Vector3.Cross(hitNormal, hitTangent);
            Ray GetTangentRay(Vector2 tangentSpacePosition)
            {
                Vector3 rayOrigin = hit.point + (hitTangent * tangentSpacePosition.x + hitBiTangent * tangentSpacePosition.y) * radius;
                rayOrigin += hitNormal * 3;
                Vector3 rayDirection = -hitNormal;
                return new Ray(rayOrigin, rayDirection);
            }
            foreach (Vector2 point in randomPoints)
            {
                Ray pointRay = GetTangentRay(point);
                if(Physics.Raycast(pointRay, out RaycastHit pointHit))
                {
                    Handles.color = Color.black;
                    DrawSphere(pointHit.point);
                    Handles.DrawAAPolyLine(pointHit.point, pointHit.point + pointHit.normal);
                    Handles.color = Color.white;
                }
            }
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitTangent);
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitBiTangent);
            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(6, hit.point, hit.point + hitNormal);

            const int circleDetail = 128;
            Vector3[] ringPoints = new Vector3[circleDetail];
            for (int i = 0; i < circleDetail; i++)
            {
                float turn = i / ((float)circleDetail - 1);
                const float pi = Mathf.PI * 2;
                float angularRadius = turn * pi;
                Vector2 direction = new Vector2(Mathf.Cos(angularRadius), Mathf.Sin(angularRadius));
                Ray r = GetTangentRay(direction);
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
            //Handles.DrawWireDisc(hit.point, hit.normal, radius);
            Handles.color = Color.white;
        }
    }
}

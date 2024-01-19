using System.Linq;
using Procedural_Mesh;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Mesh2D shape2D;

    [Range(0, 1), SerializeField] private float tTest = 0;

    [SerializeField] private Transform[] controlPoints = new Transform[4];

    Vector3 GetPos(int i) => controlPoints[i].position;

    private void OnDrawGizmos()
    {
        var radius = 0.15f;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), radius);
        }

        Handles.DrawBezier(
            GetPos(0),
            GetPos(1),
            GetPos(2),
            GetPos(3), Color.white, EditorGUIUtility.whiteTexture, 1f);

        Gizmos.color = Color.green;

        OrientedPoint orientedPoint = GetBezierOrientedPoint(tTest);

        Handles.PositionHandle(orientedPoint.Position, orientedPoint.Rotation);


        void DrawPoint(Vector2 localPosition) =>
            Gizmos.DrawSphere(orientedPoint.LocalToWorld(localPosition), radius);

        Vector3[] verts = shape2D.Vertices.Select(v => orientedPoint.LocalToWorld(v.Point)).ToArray();
        for (int i = 0; i < shape2D.LineIndices.Length; i+=2)
        {
            Vector3 a = verts[shape2D.LineIndices[i]];
            Vector3 b = verts[shape2D.LineIndices[i+1]];

            Gizmos.DrawLine(a, b);
        }

        DrawPoint(Vector3.right * 0.2f);
        DrawPoint(Vector3.right * 0.1f);
        DrawPoint(Vector3.right * -0.1f);
        DrawPoint(Vector3.right * -0.2f);

        Gizmos.color = Color.white;
    }

    private OrientedPoint GetBezierOrientedPoint(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;
        ;

        return new OrientedPoint(pos, tangent);
    }
}
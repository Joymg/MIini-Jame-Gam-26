using System;
using System.Collections.Generic;
using System.Linq;
using Procedural_Mesh;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MeshFilter))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Mesh2D shape2D;


    [Range(2, 32)] [SerializeField] private int edgeRingCount = 8;

    [Range(0, 1), SerializeField] private float tTest = 0;

    [SerializeField] private Transform[] controlPoints = new Transform[4];

    private Mesh _mesh;
    private Vector3 GetPos(int i) => controlPoints[i].position;

    [SerializeField] private MeshCollider meshCollider;


    private void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "Segment";
        GetComponent<MeshFilter>().sharedMesh = _mesh;
        meshCollider.sharedMesh = _mesh;
    }

    private void Start()
    {
        GenerateMesh();
    }

    //private void Update() => GenerateMesh();

    private void GenerateMesh()
    {
        _mesh.Clear();

        float uSpan = shape2D.CalculateUSpan();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        //Generate Vertices
        for (int ring = 0; ring < edgeRingCount; ring++)
        {
            float t = ring / (edgeRingCount - 1f);
            OrientedPoint op = GetBezierOrientedPoint(t);
            for (int i = 0; i < shape2D.VertexCount; i++)
            {
                verts.Add(op.LocalToWorldPosition(shape2D.Vertices[i].Point));
                normals.Add(op.LocalToWorldVector(shape2D.Vertices[i].Normal));
                uvs.Add(new Vector2(shape2D.Vertices[i].U, t * GetApproximateLength() / uSpan));
            }
        }

        //Generate Triangles
        List<int> trianglesIndices = new List<int>();
        for (int ring = 0; ring < edgeRingCount - 1; ring++)
        {
            int rootIndex = ring * shape2D.VertexCount;
            int rootIndexNext = (ring + 1) * shape2D.VertexCount;

            for (int line = 0; line < shape2D.LineCount; line += 2)
            {
                int lineIndexA = shape2D.LineIndices[line];
                int lineIndexB = shape2D.LineIndices[line + 1];

                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;
                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;

                trianglesIndices.Add(currentA);
                trianglesIndices.Add(nextA);
                trianglesIndices.Add(nextB);

                trianglesIndices.Add(currentA);
                trianglesIndices.Add(nextB);
                trianglesIndices.Add(currentB);
            }
        }

        _mesh.SetVertices(verts);
        _mesh.SetUVs(0, uvs);
        _mesh.SetTriangles(trianglesIndices, 0);
        _mesh.SetNormals(normals);
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = _mesh;
    }

    private void OnDrawGizmos()
    {
        var radius = 0.15f;
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), radius);
        }

        Handles.DrawBezier(
            GetPos(0),
            GetPos(3),
            GetPos(1),
            GetPos(2), Color.white, EditorGUIUtility.whiteTexture, 1f);

        Gizmos.color = Color.green;

        OrientedPoint orientedPoint = GetBezierOrientedPoint(tTest);

        Handles.PositionHandle(orientedPoint.Position, orientedPoint.Rotation);


        void DrawPoint(Vector2 localPosition) =>
            Gizmos.DrawSphere(orientedPoint.LocalToWorldPosition(localPosition), radius);

        Vector3[] verts = shape2D.Vertices.Select(v => orientedPoint.LocalToWorldPosition(v.Point)).ToArray();
        for (int i = 0; i < shape2D.LineIndices.Length; i += 2)
        {
            Vector3 a = verts[shape2D.LineIndices[i]];
            Vector3 b = verts[shape2D.LineIndices[i + 1]];

            Gizmos.DrawLine(a, b);
        }

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
        Vector3 up = Vector3.Lerp(controlPoints[0].up, controlPoints[3].up, t).normalized;

        Quaternion rot = Quaternion.LookRotation(tangent, up);

        return new OrientedPoint(pos, rot);
    }

    private float GetApproximateLength(int precision = 8)
    {
        Vector3[] points = new Vector3[precision];

        for (int i = 0; i < precision; i++)
        {
            float t = i / (precision - 1f);
            points[i] = GetBezierOrientedPoint(t).Position;
        }

        float dist = 0;
        for (int i = 0; i < precision - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];

            dist += Vector3.Distance(a, b);
        }

        return dist;
    }
}
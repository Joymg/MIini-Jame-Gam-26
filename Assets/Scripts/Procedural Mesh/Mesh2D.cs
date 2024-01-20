using UnityEngine;

namespace Procedural_Mesh
{
    [CreateAssetMenu]
    public class Mesh2D : ScriptableObject
    {
        [System.Serializable]
        public class Vertex
        {
            public Vector2 Point;
            public Vector2 Normal;
            public float U;
        }

        public Vertex[] Vertices;
        public int[] LineIndices;

        public int VertexCount => Vertices.Length;
        public int LineCount => LineIndices.Length;

        public float CalculateUSpan()
        {
            //Calculate length "perimeter" of uvs
            float dist = 0;
            for (int i = 0; i < LineCount; i+=2)
            {
                Vector2 a = Vertices[LineIndices[i]].Point;
                Vector2 b = Vertices[LineIndices[i+1]].Point;
                dist += (a - b).magnitude;
            }

            return dist;
        }
    }
}
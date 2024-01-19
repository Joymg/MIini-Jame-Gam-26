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
    }
}
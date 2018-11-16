using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Pieces
{
    public class Piece : MonoBehaviour
    {
        public int minSegments = 3;
        public int maxSegments = 5;

        public float baseRadius = 1f;
        public float defectDeviation = 0.2f;

        // Use this for initialization
        void Start()
        {
            var vertices2D = GenerateVertices();

            BuildCollider(vertices2D);
            BuildMesh(vertices2D);
        }

        private Vector2[] GenerateVertices()
        {
            var vertices = Random.Range(minSegments, maxSegments + 1);
            var partitions = PartitionCircleInterval(vertices);

            return ConvertPartitionsTo2D(partitions);
        }

        private void BuildCollider(Vector2[] vertices2D)
        {
            var collider2d = gameObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
            collider2d.SetPath(0, vertices2D);
        }

        private void BuildMesh(Vector2[] vertices2D)
        {
            var vertices3D = System.Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);

            var triangulator = new Triangulator(vertices2D);
            var indices = triangulator.Triangulate();

            var colors = Enumerable.Range(0, vertices3D.Length)
                .Select(i => Random.ColorHSV())
                .ToArray();

            var mesh = new Mesh
            {
                vertices = vertices3D,
                triangles = indices,
                colors = colors
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));

            var filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
        }

        private static List<float> PartitionCircleInterval(int vertices)
        {
            var partitions = new List<float>(vertices);

            for (var i = 0; i < vertices; i++)
            {
                partitions.Add(Random.Range(0, 2 * Mathf.PI));
            }

            partitions.Sort();

            return partitions;
        }

        private Vector2[] ConvertPartitionsTo2D(List<float> partitions)
        {
            var points = new List<Vector2>(partitions.Capacity);

            partitions.ForEach(scalar =>
            {
                var deviation = Random.Range(-defectDeviation, defectDeviation + Mathf.Epsilon);
                var radius = baseRadius + deviation;

                points.Add(new Vector2(Mathf.Cos(scalar) * radius, Mathf.Sin(scalar) * radius));
            });

            return points.ToArray();
        }
    }
}
using System;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Assets.Pieces
{
    public class Piece : MonoBehaviour
    {
        public Material[] Materials;

        public void InitVertices(Vector2[] vertices2D)
        {
            BuildCollider(vertices2D);
            BuildMesh(vertices2D);
        }

        private void BuildCollider(Vector2[] vertices2D)
        {
            var polygonCollider2D = gameObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
            Debug.Assert(polygonCollider2D != null, nameof(polygonCollider2D) + " != null");

            polygonCollider2D.SetPath(0, vertices2D);
        }

        private void BuildMesh(Vector2[] vertices2D)
        {
            var amountVertices = vertices2D.Length;
            var vertices3D = Array.ConvertAll<Vector2, Vector3>(vertices2D, v => v);

            var triangulator = new Triangulator(vertices2D);
            var indices = triangulator.Triangulate();

            var uv = BuildCookieCutterUVs(vertices2D, amountVertices);

            var mesh = new Mesh
            {
                vertices = vertices3D,
                triangles = indices,
                uv = uv,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var meshRenderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            Debug.Assert(meshRenderer != null, nameof(meshRenderer) + " != null");
            
            meshRenderer.material = Materials[Random.Range(0,Materials.Length)];

            var filter = gameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;
        }

        private static Vector2[] BuildCookieCutterUVs(Vector2[] vertices2D, int amountVertices)
        {
            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;

            foreach (var vertex in vertices2D)
            {
                if (minX > vertex.x)
                    minX = vertex.x;
                if (maxX < vertex.x)
                    maxX = vertex.x;
                if (minY > vertex.y)
                    minY = vertex.y;
                if (maxY < vertex.y)
                    maxY = vertex.y;
            }

            var deltaX = maxX - minX;
            var midX = (maxX + minX) / 2;
            var deltaY = maxY - minY;
            var midY = (maxY + minY) / 2;

            var delta = Mathf.Max(deltaX, deltaY);

            var uv = new Vector2[amountVertices];
            for (var i = 0; i < amountVertices; i++)
            {
                var vertex = vertices2D[i];
                uv[i] = new Vector2(0.5f + ((vertex.x - midX) / delta), 0.5f + ((vertex.y - midY) / delta));
            }

            return uv;
        }
    }
}
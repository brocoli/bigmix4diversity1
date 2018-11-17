using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Pieces
{
    public class Piece : MonoBehaviour
    {
        void InitVertices(Vector2[] vertices2D)
        {
            BuildCollider(vertices2D);
            BuildMesh(vertices2D);
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
    }
}
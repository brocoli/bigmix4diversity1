using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Pieces
{
    public class PieceRandomizer : MonoBehaviour
    {
        public GameObject PiecePrefab;
        public Transform SpawnPoint;

        public int MinSegments = 4;
        public int MaxSegments = 6;
        public float BaseRadius = 3.5f;
        public float OutsetDeviation = 0f;
        public float InsetDeviation = 0f;

        public float MinWidth = 0f;
        public float MinHeight = 0f;
        public float MinTriangleAreaPerSidesMinusTwo = 0f;

        public float SpawnDistance = 5f;

        public readonly Piece[] PiecesToSelect = new Piece[3];
        public float WindowHeight;

        public float MaxReferenceY = -15f;
        private float _pieceOffset;

        public void Start()
        {
            WindowHeight = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        }

        public void Update()
        {
            for (var i = 0; i < PiecesToSelect.Length; i++)
            {
                var piece = PiecesToSelect[i];
                
                if (piece != null && !piece.IsInPlay)
                {
                    continue;
                }

                PiecesToSelect[i] = null;
                SpawnPiece(i);
            }
        }

        private void SpawnPiece(int slot)
        {
            var rotZ = Random.Range(0, 360);
            var posX = SpawnPoint.position.x + (slot - 1) * SpawnDistance;

            var position = new Vector3(posX, SpawnPoint.position.y, SpawnPoint.position.z + _pieceOffset);
            var rotation = new Quaternion(SpawnPoint.rotation.x, SpawnPoint.rotation.y, rotZ, SpawnPoint.rotation.w);

            var amountVertices = Random.Range(MinSegments, MaxSegments + 1);
            var vertices2D = GenerateGoodVertices(amountVertices);

            var newPieceObject = Instantiate(PiecePrefab, position, rotation);
            newPieceObject.tag = "Pieces";
            var newPiece = newPieceObject.GetComponent<Piece>();
            newPiece.PieceRandomizer = this;

            _pieceOffset += 0.00001f;

            newPiece.InitVertices(vertices2D);
            PiecesToSelect[slot] = newPiece;
        }

        private Vector2[] GenerateGoodVertices(int amountVertices)
        {
            Vector2[] vertices2D;
            var tries = 0;

            while (true)
            {
                vertices2D = TryGenerateGoodVertices(amountVertices);
                if (vertices2D == null)
                {
                    tries += 1;
                    if (tries <= 500)
                    {
                        continue;
                    }

                    Debug.Log("Warning! piece spawn constraints are too strict.");
                    break;
                }
                else
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
                    var deltaY = maxY - minY;

                    if (deltaX < MinWidth || deltaY < MinHeight)
                    {
                        tries += 1;
                        if (tries <= 500)
                        {
                            continue;
                        }

                        Debug.Log("Warning! piece spawn constraints are too strict.");
                        break;
                    }

                    break; 
                }
            }

            return vertices2D;
        }

        [CanBeNull]
        private Vector2[] TryGenerateGoodVertices(int amountVertices)
        {
            var vertices2D = GenerateVertices(amountVertices);

            var triangulator = new Triangulator(vertices2D.ToArray());
            var indices = triangulator.Triangulate();

            for (var i = 0; i < Mathf.RoundToInt((float)indices.Length / (float)3); i++)
            {
                var triangle = new List<Vector2>
                {
                    vertices2D[indices[i * 3 + 0]], vertices2D[indices[i * 3 + 1]], vertices2D[indices[i * 3 + 2]]
                };

                var triangleArea = Mathf.Abs(Triangulator.Area(triangle));
                if (triangleArea < MinTriangleAreaPerSidesMinusTwo/(amountVertices-2))
                {
                    return null;
                }
            }

            return vertices2D.ToArray();
        }

        private List<Vector2> GenerateVertices(int amountVertices)
        {
            var partitions = PartitionCircleInterval(amountVertices);
            return ConvertPartitionsTo2D(partitions);
        }

        private List<Vector2> ConvertPartitionsTo2D(List<float> partitions)
        {
            var points = new List<Vector2>(partitions.Capacity);

            partitions.ForEach(scalar =>
            {
                var deviation = Random.Range(-InsetDeviation, OutsetDeviation + Mathf.Epsilon);
                var radius = BaseRadius + deviation;

                points.Add(new Vector2(Mathf.Cos(scalar) * radius, Mathf.Sin(scalar) * radius));
            });

            return points;
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
    }
}

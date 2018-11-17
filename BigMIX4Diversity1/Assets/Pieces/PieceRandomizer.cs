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
        public float MinTriangleAreaPerSidesMinusTwo = 0f;

        public float SpawnDistance = 5f;

        private readonly Piece[] _pieces = new Piece[3];

        public void Start()
        {
            for (var i = 0; i < 3; i++)
            {
                SpawnPiece(i);
            }
        }

        public void Update()
        {
            for (var i = 0; i < _pieces.Length; i++)
            {
                var piece = _pieces[i];

                if (piece == null || !piece.IsInPlay())
                {
                    continue;
                }

                _pieces[i] = null;
                SpawnPiece(i);
            }
        }

        private void SpawnPiece(int slot)
        {
            var rotZ = Random.Range(0, 360);
            var posX = SpawnPoint.position.x + (slot - 1) * SpawnDistance;

            var position = new Vector3(posX, SpawnPoint.position.y, SpawnPoint.position.z);
            var rotation = new Quaternion(SpawnPoint.rotation.x, SpawnPoint.rotation.y, rotZ, SpawnPoint.rotation.w);

            var amountVertices = Random.Range(MinSegments, MaxSegments + 1);
            var vertices2D = GenerateGoodVertices(amountVertices);

            var newPieceObject = Instantiate(PiecePrefab, position, rotation);
            var newPiece = newPieceObject.GetComponent<Piece>();

            newPiece.InitVertices(vertices2D);
            _pieces[slot] = newPiece;
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace Assets.Pieces
{
    public class PieceRandomizer : MonoBehaviour
    {
        public GameObject PiecePrefab;
        public Transform SpawnPoint;

        public int MinSegments = 3;
        public int MaxSegments = 6;
        public float BaseRadius = 3.5f;
        public float OutsetDeviation = 1f;
        public float InsetDeviation = 2.5f;
        public float MinTriangleAreaPerSidesMinusTwo = 6f;

        public int MinSpawnX = -5;
        public int MaxSpawnX = 5;

        public float SpawnPeriod = 1f;
        private float _timer;

        private Random _random;

        public void Awake()
        {
            _random = new Random();
        }

        public void Update()
        {
            _timer = _timer + Time.deltaTime;
            if (!(_timer >= SpawnPeriod))
            {
                return;
            }

            var posX = _random.Next(MinSpawnX, MaxSpawnX);
            var rotZ = _random.Next(0, 360);

            SpawnPoint.position = new Vector3(posX, SpawnPoint.position.y, SpawnPoint.position.z);
            SpawnPoint.rotation =
                new Quaternion(SpawnPoint.rotation.x, SpawnPoint.rotation.y, rotZ, SpawnPoint.rotation.w);

            var amountVertices = UnityEngine.Random.Range(MinSegments, MaxSegments + 1);
            var vertices2D = GenerateGoodVertices(amountVertices);
            var piece = Instantiate(PiecePrefab, SpawnPoint.position, SpawnPoint.rotation);
            piece.SendMessage("InitVertices", vertices2D);

            _timer = 0;
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
                    if (tries > 500)
                    {
                        break;
                    }
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

            var fullArea = Triangulator.Area(vertices2D);

            for (var i = 0; i < Mathf.RoundToInt((float)indices.Length / (float)3); i++)
            {
                var triangle = new List<Vector2>
                {
                    vertices2D[indices[i * 3 + 0]], vertices2D[indices[i * 3 + 1]], vertices2D[indices[i * 3 + 2]]
                };

                var triangleArea = Mathf.Abs(Triangulator.Area(triangle));
                print(triangleArea);
                fullArea -= triangleArea;

                if (triangleArea < MinTriangleAreaPerSidesMinusTwo/(amountVertices-2))
                {
                    return null;
                }
            }

            print("The full area decremented is");
            print(fullArea);

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
                var deviation = UnityEngine.Random.Range(-InsetDeviation, OutsetDeviation + Mathf.Epsilon);
                var radius = BaseRadius + deviation;
                //var radius = BaseRadius;

                points.Add(new Vector2(Mathf.Cos(scalar) * radius, Mathf.Sin(scalar) * radius));
            });

            return points;
        }

        private static List<float> PartitionCircleInterval(int vertices)
        {
            var partitions = new List<float>(vertices);

            for (var i = 0; i < vertices; i++)
            {
                partitions.Add(UnityEngine.Random.Range(0, 2 * Mathf.PI));
            }

            partitions.Sort();

            return partitions;
        }
    }
}

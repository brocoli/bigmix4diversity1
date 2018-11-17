using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Assets.Pieces
{
    public class Piece : MonoBehaviour
    {
        public Material[] Materials;
        public float VelY = -0.5f;

        private static readonly int _amountReferencePoints = 201;
        private static readonly Vector2[] ReferencePoints = new Vector2[_amountReferencePoints];
        private static bool _hasReferencePoints = false;

        private float _pieceZ = 0f;

        private PolygonCollider2D _polygonCollider2D;
        private bool _lastMouse0Up;
        private bool _isInPlay;
        private Transform _transform;
        private float _finalY = float.NegativeInfinity;

        public void Awake()
        {
            _transform = GetComponent<Transform>();

            var pos = _transform.position;
            pos.z = _pieceZ;
            _transform.position = pos;

            _pieceZ += 0.01f;

            if (!_hasReferencePoints)
            {
                for (var i = 0; i < _amountReferencePoints; i++)
                {
                    ReferencePoints[i] = new Vector2((float)(i - (_amountReferencePoints - 1)/2) /2, -12f);
                }
                _hasReferencePoints = true;
            }
        }

        public void InitVertices(Vector2[] vertices2D)
        {
            BuildCollider(vertices2D);
            BuildMesh(vertices2D);
        }

        private void BuildCollider(Vector2[] vertices2D)
        {
            _polygonCollider2D = gameObject.AddComponent(typeof(PolygonCollider2D)) as PolygonCollider2D;
            Debug.Assert(_polygonCollider2D != null, nameof(_polygonCollider2D) + " != null");

            _polygonCollider2D.SetPath(0, vertices2D);
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
            
            meshRenderer.material = Materials[Random.Range(0, Materials.Length)];

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
            var midX = (maxX + minX)/2;
            var deltaY = maxY - minY;
            var midY = (maxY + minY)/2;

            var delta = Mathf.Max(deltaX, deltaY) + 0.1f;

            var uv = new Vector2[amountVertices];
            for (var i = 0; i < amountVertices; i++)
            {
                var vertex = vertices2D[i];
                uv[i] = new Vector2(0.5f + (vertex.x - midX)/delta, 0.5f + (vertex.y - midY)/delta);
            }

            return uv;
        }

        private void Update()
        {
            _lastMouse0Up = Input.GetKeyUp(KeyCode.Mouse0);
        }

        private void FixedUpdate()
        {
            if (_lastMouse0Up && !_isInPlay)
            {
                _lastMouse0Up = false;

                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var hit = Physics2D.Raycast(pos, Vector2.zero);

                if (hit.collider != null && hit.collider == _polygonCollider2D)
                {
                    PutIntoPlay();
                }
            }

            if (_isInPlay)
            {
                var rawPos = _transform.position;
                var nextY = rawPos.y + VelY;

                if (nextY > _finalY)
                {
                    var pos = new Vector3(rawPos.x, nextY, rawPos.z);
                    _transform.position = pos;
                }
            }
        }

        private void PutIntoPlay()
        {
            var affectedReferencePoints = new List<int>();
            var hitsFromAbove = new List<float>();

            var maxDeltaY = float.NegativeInfinity;
            
            for (var i = 0; i < _amountReferencePoints; i++)
            {
                var referencePoint = ReferencePoints[i];
                
                var hitFromBelow = Physics2D.Raycast(referencePoint, Vector2.up);
                if (hitFromBelow.collider != _polygonCollider2D)
                {
                    continue;
                }

                var hitFromAbove = Physics2D.Raycast(new Vector2(referencePoint.x, 100f), Vector2.down);
                Debug.Assert(hitFromAbove.collider != null, "wtf is this? I can hit from below but not above?");

                affectedReferencePoints.Add(i);
                hitsFromAbove.Add(hitFromAbove.point.y);

                var hitY = hitFromBelow.point.y;
                var deltaY = hitY - referencePoint.y;
                if (maxDeltaY < deltaY)
                {
                    maxDeltaY = deltaY;
                }
            }

            Debug.Assert(!float.IsNegativeInfinity(maxDeltaY));
            
            var amountAffectedPoints = affectedReferencePoints.Count;
            for (var i = 0; i < amountAffectedPoints; i++)
            {
                var j = affectedReferencePoints[i];
                var referencePoint = ReferencePoints[j];
                var hitFromAbove = hitsFromAbove[i];

                var landedReferencePoint = hitFromAbove - maxDeltaY;
                if (landedReferencePoint >= referencePoint.y)
                {
                    referencePoint.y = landedReferencePoint + 1f;
                    ReferencePoints[j] = referencePoint;
                }
            }
            
            _finalY = _transform.position.y - maxDeltaY;
            _isInPlay = true;
        }

        public bool IsInPlay()
        {
            return _isInPlay;
        }
    }
}
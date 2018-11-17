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

        private PolygonCollider2D _polygonCollider2D;
        private bool _lastMouse0Up;
        private bool _isInPlay;
        private Transform _transform;
        private float _finalY = float.NegativeInfinity;

        private Vector2 _mouseDownPointerPos;
        private Vector2 _mouseDownPiecePos;

        private static Piece _touchFocus;
        private static Piece _touchController;

        private static float _maxReferenceY = -12f;

        public PieceRandomizer PieceRandomizer;
        private static float _pieceRandomizerDelta;

        public Camera CameraRef;
        private static float _cameraDelta;

        public void Awake()
        {
            _transform = GetComponent<Transform>();
            _transform.SetAsFirstSibling();

            var pos = _transform.position;
            _transform.position = pos;

            if (!_hasReferencePoints)
            {
                for (var i = 0; i < _amountReferencePoints; i++)
                {
                    ReferencePoints[i] = new Vector2((float)(i - (_amountReferencePoints - 1)/2) /2, -12f);
                }
                _hasReferencePoints = true;
            }

            if (_touchController == null)
            {
                _touchController = this;
            }

            CameraRef = Camera.main;
            _cameraDelta = CameraRef.transform.position.y + 12f;

            _pieceRandomizerDelta = 20f;
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
            if (_touchController == this)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    var hit = Physics2D.Raycast(pos, Vector2.zero);

                    if (hit.collider != null)
                    {
                        var piece = hit.collider.gameObject.GetComponent<Piece>();
                        if (piece != null)
                        {
                            _touchFocus = piece;
                            _touchFocus.OnPieceTouchDown();
                        }
                    }
                }

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (_touchFocus)
                    {
                        _touchFocus.OnPieceTouch();
                    }
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    if (_touchFocus)
                    {
                        _touchFocus.OnPieceTouchUp();
                    }
                    _touchFocus = null;
                }
            }
        }

        private void OnPieceTouchDown()
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var piecePos = _transform.position;

            _mouseDownPointerPos = new Vector2(mousePos.x, mousePos.y);
            _mouseDownPiecePos = new Vector2(piecePos.x, piecePos.y);
        }

        private void OnPieceTouch()
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var deltaPosX = pos.x - _mouseDownPointerPos.x;
            var deltaPosY = pos.y - _mouseDownPointerPos.y;

            var piecePos = _transform.position;
            piecePos.x = _mouseDownPiecePos.x + deltaPosX;
            piecePos.y = _mouseDownPiecePos.y + deltaPosY;
            _transform.position = piecePos;
        }

        private void OnPieceTouchUp()
        {
            _lastMouse0Up = Input.GetKeyUp(KeyCode.Mouse0);
        }

        private void FixedUpdate()
        {
            if (_lastMouse0Up && !_isInPlay)
            {
                PutIntoPlay();
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

                var hitY = hitFromBelow.point.y;
                var hitFromAbove = Physics2D.Raycast(new Vector2(referencePoint.x, hitY + 30f), Vector2.down);
                Debug.Assert(hitFromAbove.collider != null, "wtf is this? I can hit from below but not above?");

                affectedReferencePoints.Add(i);
                hitsFromAbove.Add(hitFromAbove.point.y);

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
                    referencePoint.y = landedReferencePoint + 0.5f;
                    ReferencePoints[j] = referencePoint;

                    if (referencePoint.y > _maxReferenceY)
                    {
                        _maxReferenceY = referencePoint.y;

                        var cameraPos = CameraRef.transform.position;
                        cameraPos.y = (_cameraDelta + _maxReferenceY)/3f;
                        CameraRef.transform.position = cameraPos;

                        var spawnerPos = PieceRandomizer.transform.position;
                        spawnerPos.y = _pieceRandomizerDelta + _maxReferenceY;
                        PieceRandomizer.transform.position = spawnerPos;
                    }
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
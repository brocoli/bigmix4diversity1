using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Assets.Pieces
{
    public class Piece : MonoBehaviour
    {
        public Material[] Materials;
        public float TransitionDownTime = 1f;
        public float MinMeanMoveUpPerPlay = 10f;

        private GameObject _yReferences;

        private PolygonCollider2D _polygonCollider2D;
        private bool _isInPlay;

        private Vector2 _mouseDownPointerPos;
        private Vector2 _mouseDownPiecePos;

        private static float _maxReferenceY = -12f;

        public PieceRandomizer PieceRandomizer;
        public Camera CameraRef;

        public void Awake()
        {
            var p = transform.position;
            p.z = -2f;
            transform.position = p;
            transform.SetAsFirstSibling();

            var pos = transform.position;
            transform.position = pos;

            CameraRef = Camera.main;
        }

        public void Start()
        {
            _yReferences = GameObject.FindWithTag("YReferences");
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

        public void OnPieceTouchDown()
        {
            if (_isInPlay)
            {
                return;
            }

            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var piecePos = transform.position;

            _mouseDownPointerPos = new Vector2(mousePos.x, mousePos.y);
            _mouseDownPiecePos = new Vector2(piecePos.x, piecePos.y);
        }

        public void OnPieceTouch()
        {
            if (_isInPlay)
            {
                return;
            }

            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var deltaPosX = pos.x - _mouseDownPointerPos.x;
            var deltaPosY = pos.y - _mouseDownPointerPos.y;

            var piecePos = transform.position;
            piecePos.x = _mouseDownPiecePos.x + deltaPosX;
            piecePos.y = _mouseDownPiecePos.y + deltaPosY;
            transform.position = piecePos;
        }

        public void OnPieceTouchUp()
        {
            if (_isInPlay)
            {
                return;
            }


            PutIntoPlay();
        }

        private void PutIntoPlay()
        {
            var referencesTransforms = _yReferences.transform.Cast<Transform>().ToArray();
            var amountReferencePoints = referencesTransforms.Length;
            
            var affectedReferencePoints = new List<int>();
            var hitsFromAbove = new List<float>();

            var maxDeltaY = float.NegativeInfinity;
            
            for (var i = 0; i < amountReferencePoints; i++)
            {
                var referencePoint = referencesTransforms[i].position;

                var hitFromBelow = Physics2D.Raycast(new Vector2(referencePoint.x, _polygonCollider2D.bounds.min.y - float.Epsilon), Vector2.up);
                if (hitFromBelow.collider != _polygonCollider2D)
                {
                    continue;
                }

                var hitFromBelowY = hitFromBelow.point.y;
                var hitFromAbove = Physics2D.Raycast(new Vector2(referencePoint.x, _polygonCollider2D.bounds.max.y + float.Epsilon), Vector2.down);
                Debug.Assert(hitFromAbove.collider != null, "wtf is this? I can hit from below but not above?");

                affectedReferencePoints.Add(i);
                hitsFromAbove.Add(hitFromAbove.point.y);

                var deltaY = hitFromBelowY - referencePoint.y;
                if (maxDeltaY < deltaY)
                {
                    maxDeltaY = deltaY;
                }
            }

            if (float.IsNegativeInfinity(maxDeltaY))
            {
                Destroy(gameObject);
                return;
            }
            
            var amountAffectedPoints = affectedReferencePoints.Count;

            var meanRefPointMoveUp = 0f;
            for (var i = 0; i < amountAffectedPoints; i++)
            {
                var j = affectedReferencePoints[i];
                var referenceTransform = referencesTransforms[j];
                var referencePoint = referenceTransform.position;
                var hitFromAbove = hitsFromAbove[i];

                var landedReferencePoint = hitFromAbove - maxDeltaY;
                var referencePointMoveUp = landedReferencePoint - referencePoint.y;

                meanRefPointMoveUp += referencePointMoveUp;
            }
            meanRefPointMoveUp /= amountAffectedPoints;

            var amountMoveDown = maxDeltaY;
            var minMoveUpThisPlay = MinMeanMoveUpPerPlay / amountAffectedPoints;
            if (meanRefPointMoveUp < minMoveUpThisPlay)
            {
                amountMoveDown -= minMoveUpThisPlay - meanRefPointMoveUp;
            }

            for (var i = 0; i < amountAffectedPoints; i++)
            {
                var j = affectedReferencePoints[i];
                var referenceTransform = referencesTransforms[j];
                var referencePoint = referenceTransform.position;
                var hitFromAbove = hitsFromAbove[i];

                var landedReferencePoint = hitFromAbove - amountMoveDown;

                referencePoint.y = landedReferencePoint;
                referencesTransforms[j].position = referencePoint;

                if (_maxReferenceY < referencePoint.y)
                {
                    _maxReferenceY = referencePoint.y;

                    var cameraTransform = CameraRef.transform;
                    var targetY = _maxReferenceY + PieceRandomizer.WindowHeight * 1/4;
                    cameraTransform.DOMoveY(targetY, 0.4f);

                    var spawnerPos = PieceRandomizer.transform.position;
                    spawnerPos.y = targetY + PieceRandomizer.WindowHeight * 3/4;
                    PieceRandomizer.transform.position = spawnerPos;
                }
            }

            transform.DOMoveY(transform.position.y - amountMoveDown, TransitionDownTime);
            _isInPlay = true;
        }

        public bool IsInPlay()
        {
            return _isInPlay;
        }
    }
}
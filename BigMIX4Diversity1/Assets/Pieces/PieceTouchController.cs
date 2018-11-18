using UnityEngine;

namespace Assets.Pieces
{
    public class PieceTouchController : MonoBehaviour
    {
        private Piece _touchFocus;

        private void Update()
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
}

using UnityEngine;
namespace ColorBlocks
{
    public class BlockDragHandler : MonoBehaviour
    {
        private Block _block;
        private bool _isDragging = false;
        private float _minMoveDistance = 0.3f;
        private Camera _mainCamera;
        private Vector3 _lastMovePosition;
        private static int _touchIndex = 0;

        private void Awake()
        {
            _block = GetComponent<Block>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                HandleMouseInput();
            }
        }

        private void HandleTouchInput()
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (IsBlockTouched(touch.position))
                    {
                        StartDrag(GetWorldPosition(touch.position));
                        IncreaseTouchIndex();
                    }
                    break;

                case TouchPhase.Moved:
                    if (_isDragging)
                    {
                        ContinueDrag(GetWorldPosition(touch.position));
                    }
                    break;

                case TouchPhase.Ended:
                    if (_isDragging )
                    {
                        EndDrag();
                    }
                    break;
            }
        }
        private void IncreaseTouchIndex()
        {
            _touchIndex++;
            if(_touchIndex > 10000)
            {
                _touchIndex = 0;
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsBlockTouched(Input.mousePosition))
                {
                    StartDrag(GetWorldPosition(Input.mousePosition));
                    IncreaseTouchIndex();
                }
            }
            else if (Input.GetMouseButton(0))
            {
                if (_isDragging)
                {
                    ContinueDrag(GetWorldPosition(Input.mousePosition));
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (_isDragging)
                {
                    EndDrag();
                }
            }
        }

        private void StartDrag(Vector3 position)
        {
            _lastMovePosition = position;
            _isDragging = true;
        }

        private void ContinueDrag(Vector3 currentPosition)
        {
            Vector3 dragDirection = currentPosition - _lastMovePosition;
            dragDirection.y = 0;

            if (dragDirection.magnitude >= _minMoveDistance)
            {
                int direction = GetSwipeDirection(dragDirection);
                bool moved = Grid.Instance.MoveBlock(_touchIndex, _block, direction);
                if (moved)
                {
                    _lastMovePosition = currentPosition;
                }
            }
        }

        private void EndDrag()
        {
            _isDragging = false;
        }

        private bool IsBlockTouched(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                return hit.transform == transform;
            }

            return false;
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Plane plane = new Plane(Vector3.up, transform.position);

            float distance;
            if (plane.Raycast(ray, out distance))
            {
                return ray.GetPoint(distance);
            }

            return transform.position;
        }

        private int GetSwipeDirection(Vector3 swipe)
        {
            swipe.y = 0;
            float angle = Vector3.SignedAngle(Vector3.right, swipe, Vector3.up);

            if (angle >= -45f && angle < 45f) return 1;
            if (angle >= 45f && angle < 135f) return 2;
            if (angle >= 135f || angle < -135f) return 3;
            return 0;
        }
    }
}

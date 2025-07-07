using UnityEngine;

public class TouchCameraController : MonoBehaviour {
    public float panSpeed = 0.1f;
    public float minX = -10f, maxX = 10f;
    public float minY = -10f, maxY = 10f;

    private Vector2 lastTouchPos;
    private bool isDragging;

    void Update() {
        if (Input.touchCount == 1) {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase) {
                case TouchPhase.Began:
                    lastTouchPos = touch.position;
                    isDragging = true;
                    break;

                case TouchPhase.Moved:
                    if (isDragging) {
                        Vector2 delta = touch.position - lastTouchPos;
                        Vector3 move = new Vector3(-delta.x * panSpeed, -delta.y * panSpeed, 0f);

                        transform.position = ClampPosition(transform.position + move);
                        lastTouchPos = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    Vector3 ClampPosition(Vector3 pos) {
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}
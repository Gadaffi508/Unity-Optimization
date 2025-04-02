using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public Camera cam;
    public float xOffset = 0f;
    public float zOffset = 0f;
    public KeyCode activeKeyCode;

    [Range(0f, 20f)]
    public float followSpeed = 5f;

    void Update()
    {
        if (!Input.GetKey(activeKeyCode)) return;

        Vector3 mousePos = Input.mousePosition;
        float depth = cam.WorldToScreenPoint(transform.position).z + zOffset;

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, depth));
        Vector3 targetPos = new Vector3(worldPos.x + xOffset, worldPos.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
    }
}

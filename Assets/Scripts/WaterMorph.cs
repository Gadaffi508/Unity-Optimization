using UnityEngine;

public class WaterMorph : MonoBehaviour
{
    public float moveThreshold = 0.1f;
    public float morphSpeed = 5f;
    public Vector2 standingXZScale = new Vector2(1f, 1f);
    public Vector2 movingXZScale = new Vector2(2.5f, 2.5f);
    public float fixedYScale = 2f;
    public float moveSpeed = 5f;

    [Header("Camera Settings")] public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public Transform rotationTarget;

    [Header("Jump Settings")] public float jumpForce = 7f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;
    public Transform groundCheckPoint;
    public Vector3 jumpScale = new Vector3(0.5f, 3.5f, 0.5f); // zıplama şekli

    private float yaw = 0f;
    private float pitch = 0f;

    private Vector2 currentXZScale;
    private Vector2 targetXZScale;
    private Rigidbody rb;
    private Vector3 moveDir;
    private Material material;
    private float expandedValue;

    private float jumpTimer = 0f;
    private float jumpDuration = 3f; // toplam zıplama süresi

    private bool isMoving;
    private bool isJumping;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        material = GetComponent<Renderer>().material;
        targetXZScale = standingXZScale;
        currentXZScale = standingXZScale;
        expandedValue = 0f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Kamera dönüşü
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 60f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        rotationTarget.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Hareket kontrolü
        float speed = rb.linearVelocity.magnitude;
        isMoving = speed > moveThreshold;

        // Jump input
        bool jumpInput = Input.GetKeyDown(KeyCode.Space);
        bool isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, groundCheckDistance, groundMask);

        if (jumpInput && isGrounded && !isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
            jumpTimer = 0f;
        }

        // Zıplama shader efekti
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            material.SetFloat("_expanded", 0f);
            material.SetFloat("_width", 1f);

            if (jumpTimer >= jumpDuration)
            {
                isJumping = false;
                material.SetFloat("_width", 0f);
            }

            return;
        }

        else
        {
            // Normal morph işlemleri
            if (isMoving)
            {
                expandedValue = Mathf.Lerp(expandedValue, 1f, Time.deltaTime * morphSpeed);
                material.SetFloat("_expanded", expandedValue);
                if (expandedValue > 0.9f)
                    targetXZScale = movingXZScale;
            }
            else
            {
                targetXZScale = standingXZScale;
            }

            currentXZScale.x = Mathf.Lerp(currentXZScale.x, targetXZScale.x, Time.deltaTime * morphSpeed);
            currentXZScale.y = Mathf.Lerp(currentXZScale.y, targetXZScale.y, Time.deltaTime * morphSpeed);
            transform.localScale = new Vector3(currentXZScale.x, fixedYScale, currentXZScale.y);

            if (!isMoving && Mathf.Abs(currentXZScale.x - standingXZScale.x) < 0.05f)
            {
                expandedValue = Mathf.Lerp(expandedValue, 0f, Time.deltaTime * morphSpeed);
                material.SetFloat("_expanded", expandedValue);
            }
        }

        // Yön hareketi
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveDir = (cameraTransform.forward * v + cameraTransform.right * h).normalized;

        // Shader Efektleri
        float targetTransparency = isMoving ? 0.4f : 0.8f;
        float targetWave = isMoving ? 0.1f : 0.01f;
        float targetRefraction = isMoving ? 0.2f : 0.05f;
        float targetSmoothness = isMoving ? 0.2f : 0.6f;
        float t = Time.deltaTime * morphSpeed;

        material.SetFloat("_Transparency", Mathf.Lerp(material.GetFloat("_Transparency"), targetTransparency, t));
        material.SetFloat("_NoiseScale", Mathf.Lerp(material.GetFloat("_NoiseScale"), targetWave, t));
        material.SetFloat("_Refraction", Mathf.Lerp(material.GetFloat("_Refraction"), targetRefraction, t));
        material.SetFloat("_Smoothness", Mathf.Lerp(material.GetFloat("_Smoothness"), targetSmoothness, t));
    }

    void LateUpdate()
    {
        if (moveDir.magnitude > 0.01f)
        {
            Vector3 move = moveDir * moveSpeed;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
}
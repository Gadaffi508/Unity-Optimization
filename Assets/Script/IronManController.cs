using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IronManController : MonoBehaviour
{
    [Header("Man Reference")]
    public PlayerInput ýnput;
    public Animator controller;
    public ThirdPersonController thirdPersonController;
    public BasicRigidBodyPush basicRigidBodyPush;
    public StarterAssetsInputs starterAssets;

    public GameObject manModel;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("IronMan Reference")]
    public GameObject model;

    public Animator modelAnimator;

    public GameObject head, Body, rightHand, leftHand, rightLeg, leftLeg;

    public bool IsIronMan = false;

    [Header("Hand Offset")]
    public float handYOffset = 10f;

    [Header("Character Bones")]
    public Transform manHead;
    public Transform manBody;
    public Transform manRightHand;
    public Transform manLeftHand;
    public Transform manRightLeg;
    public Transform manLeftLeg;

    [Header("Iron Man Rigidbody")]
    public Rigidbody[] ýronManRb;

    [Header("Move Reference")]
    public CharacterController characterController;

    Vector3 velocity;
    float moveSpeed = 5f;
    float gravity = -9.81f;

    void Start()
    {
        ýnput = GetComponent<PlayerInput>();
        controller = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        basicRigidBodyPush = GetComponent<BasicRigidBodyPush>();
        starterAssets = GetComponent<StarterAssetsInputs>();

        characterController = GetComponent<CharacterController>();

        ChangeIronMan(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            IsIronMan = !IsIronMan;
            StartCoroutine(ChangeIronManDelay());
        }

        if (!IsIronMan) return;

        Move();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * z + right * x;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            modelAnimator.SetBool("Walk", true);
        }
        else
        {
            modelAnimator.SetBool("Walk", false);
        }
    }

    void ChangeIronMan(bool isEnable)
    {
        ýnput.enabled = isEnable;
        controller.enabled = isEnable;
        thirdPersonController.enabled = isEnable;
        basicRigidBodyPush.enabled = isEnable;
        starterAssets.enabled = isEnable;
        manModel.SetActive(isEnable);

        model.SetActive(!isEnable);

        if (model.active == false)
        {
            head.SetActive(true);
            Body.SetActive(true);
            rightHand.SetActive(true);
            leftHand.SetActive(true);
            rightLeg.SetActive(true);
            leftLeg.SetActive(true);
        }
    }

    IEnumerator ChangeIronManDelay()
    {
        ýnput.enabled = false;

        foreach (Rigidbody rb in ýronManRb)
        {
            rb.isKinematic = IsIronMan;
        }

        yield return StartCoroutine(AttachPartsToBones());

        yield return new WaitForSeconds(0.2f);

        head.SetActive(false);
        Body.SetActive(false);
        rightHand.SetActive(false);
        leftHand.SetActive(false);
        rightLeg.SetActive(false);
        leftLeg.SetActive(false);

        ChangeIronMan(!IsIronMan);
    }

    IEnumerator AttachPartsToBones()
    {
        float elapsedTime = 0f;
        float duration = 1f;

        Vector3 startHead = head.transform.position;
        Vector3 startBody = Body.transform.position;
        Vector3 startRightHand = rightHand.transform.position;
        Vector3 startLeftHand = leftHand.transform.position;
        Vector3 startRightLeg = rightLeg.transform.position;
        Vector3 startLeftLeg = leftLeg.transform.position;

        Quaternion startHeadRot = head.transform.rotation;
        Quaternion startBodyRot = Body.transform.rotation;
        Quaternion startRightHandRot = rightHand.transform.rotation;
        Quaternion startLeftHandRot = leftHand.transform.rotation;
        Quaternion startRightLegRot = rightLeg.transform.rotation;
        Quaternion startLeftLegRot = leftLeg.transform.rotation;

        Quaternion characterRotation = transform.rotation;

        Quaternion targetHeadRot = characterRotation;
        Quaternion targetBodyRot = characterRotation;
        Quaternion targetRightHandRot = characterRotation;
        Quaternion targetLeftHandRot = characterRotation;
        Quaternion targetRightLegRot = characterRotation;
        Quaternion targetLeftLegRot = characterRotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            head.transform.position = Vector3.Lerp(startHead, manHead.position, t);
            Body.transform.position = Vector3.Lerp(startBody, manBody.position, t);
            rightHand.transform.position = Vector3.Lerp(startRightHand, manRightHand.position, t);
            leftHand.transform.position = Vector3.Lerp(startLeftHand, manLeftHand.position, t);
            rightLeg.transform.position = Vector3.Lerp(startRightLeg, manRightLeg.position, t);
            leftLeg.transform.position = Vector3.Lerp(startLeftLeg, manLeftLeg.position, t);

            head.transform.rotation = Quaternion.Lerp(startHeadRot, targetHeadRot, t);
            Body.transform.rotation = Quaternion.Lerp(startBodyRot, targetBodyRot, t);
            rightHand.transform.rotation = Quaternion.Lerp(startRightHandRot, targetRightHandRot, t);
            leftHand.transform.rotation = Quaternion.Lerp(startLeftHandRot, targetLeftHandRot, t);
            rightLeg.transform.rotation = Quaternion.Lerp(startRightLegRot, targetRightLegRot, t);
            leftLeg.transform.rotation = Quaternion.Lerp(startLeftLegRot, targetLeftLegRot, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        head.transform.position = manHead.position;
        Body.transform.position = manBody.position;
        rightHand.transform.position = manRightHand.position + new Vector3(0f, -handYOffset, 0f);
        leftHand.transform.position = manLeftHand.position + new Vector3(0f, -handYOffset, 0f);
        rightLeg.transform.position = manRightLeg.position;
        leftLeg.transform.position = manLeftLeg.position;

        head.transform.rotation = targetHeadRot;
        Body.transform.rotation = targetBodyRot;
        rightHand.transform.rotation = targetRightHandRot;
        leftHand.transform.rotation = targetLeftHandRot;
        rightLeg.transform.rotation = targetRightLegRot;
        leftLeg.transform.rotation = targetLeftLegRot;
    }
}

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

    [Header("IronMan Reference")]
    public GameObject model;

    public GameObject head, Body, rightHand, leftHand, rightLeg, leftLeg;

    public bool IsIronMan = false;

    [Header("Character Bones")]
    public Transform manHead;
    public Transform manBody;
    public Transform manRightHand;
    public Transform manLeftHand;
    public Transform manRightLeg;
    public Transform manLeftLeg;

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

        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
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
    }

    IEnumerator ChangeIronManDelay()
    {
        Debug.Log("Change Started");

        yield return StartCoroutine(AttachPartsToBones());

        yield return new WaitForSeconds(1f);

        head.SetActive(false);
        Body.SetActive(false);
        rightHand.SetActive(false);
        leftHand.SetActive(false);
        rightLeg.SetActive(false);
        leftLeg.SetActive(false);

        ChangeIronMan(!IsIronMan);

        Debug.Log("Change Finished");
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

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            head.transform.position = Vector3.Lerp(startHead, manHead.position, t);
            Body.transform.position = Vector3.Lerp(startBody, manBody.position, t);
            rightHand.transform.position = Vector3.Lerp(startRightHand, manRightHand.position, t);
            leftHand.transform.position = Vector3.Lerp(startLeftHand, manLeftHand.position, t);
            rightLeg.transform.position = Vector3.Lerp(startRightLeg, manRightLeg.position, t);
            leftLeg.transform.position = Vector3.Lerp(startLeftLeg, manLeftLeg.position, t);

            head.transform.rotation = Quaternion.Lerp(startHeadRot, manHead.rotation, t);
            Body.transform.rotation = Quaternion.Lerp(startBodyRot, manBody.rotation, t);
            rightHand.transform.rotation = Quaternion.Lerp(startRightHandRot, manRightHand.rotation, t);
            leftHand.transform.rotation = Quaternion.Lerp(startLeftHandRot, manLeftHand.rotation, t);
            rightLeg.transform.rotation = Quaternion.Lerp(startRightLegRot, manRightLeg.rotation, t);
            leftLeg.transform.rotation = Quaternion.Lerp(startLeftLegRot, manLeftLeg.rotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        head.transform.position = manHead.position;
        Body.transform.position = manBody.position;
        rightHand.transform.position = manRightHand.position;
        leftHand.transform.position = manLeftHand.position;
        rightLeg.transform.position = manRightLeg.position;
        leftLeg.transform.position = manLeftLeg.position;

        head.transform.rotation = manHead.rotation;
        Body.transform.rotation = manBody.rotation;
        rightHand.transform.rotation = manRightHand.rotation;
        leftHand.transform.rotation = manLeftHand.rotation;
        rightLeg.transform.rotation = manRightLeg.rotation;
        leftLeg.transform.rotation = manLeftLeg.rotation;
    }

}

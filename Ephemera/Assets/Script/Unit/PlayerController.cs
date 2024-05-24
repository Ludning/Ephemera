using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    #region Field
    private Vector2 _input;
    private Vector3 _direction;
    private float mouseX;
    private float mouseY;
    private float gravity = -9.81f;
    private float _velocity;
    private float currentVelocity;

    private float speed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float normalSpeed;
    [SerializeField] private float gravityMultiplier = 3.0f;
    [SerializeField] private float smoothTime = 0.05f;
    [SerializeField] private float jumpforce;
    [SerializeField] private float cameraSpeed;

    [SerializeField] private Transform vCam;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Inventory inventory;

    Transform localPlayerObject;

    private bool IsGround() => characterController.isGrounded;
    #endregion

    #region Initialize Function
    public void SetActivateLocalPlayer(bool isActive)
    {
        if (isLocalPlayer)
        {
            Debug.Log("SetActivateLocalPlayer : "+ isActive);
            characterController.enabled = isActive;
            GetComponent<PlayerInput>().enabled = isActive;
            //GetComponent<CapsuleCollider>().enabled = isActive;
            this.enabled = isActive;
        }
    }
    public void SetActivateLocalController(bool isActive)
    {
        if (isLocalPlayer)
        {
            characterController.enabled = isActive;
            GetComponent<PlayerInput>().enabled = isActive;
        }
    }
    #endregion
    #region MonoBehaviour Function
    private void Update()
    {
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
        SetAnimator();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion
    #region NetworkBehaviour Function
    public override void OnStartLocalPlayer()
    {
        characterController.enabled = true;
        GetComponent<PlayerInput>().enabled = true;
        GetComponent<Collider>().enabled = true;
        this.enabled = true;
    }
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            GameManager.Instance.localPlayerController = this;
            CameraReference.Instance.RegistLocalPlayerVirtualCamera(vCam.gameObject);
            speed = normalSpeed;
        }
        else
            CameraReference.Instance.RegistPlayerVirtualCamera(netId, vCam.gameObject);
    }
    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            GameManager.Instance.localPlayerController = null;
            CameraReference.Instance.RegistLocalPlayerVirtualCamera(vCam.gameObject);
        }
        else
            CameraReference.Instance.RegistPlayerVirtualCamera(netId, vCam.gameObject);
    }
    #endregion
    #region Update Function
    void ApplyGravity()
    {
        if (IsGround() && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        _direction.y = _velocity;
    }
    void ApplyRotation()
    {
        //sda
        if (_input.sqrMagnitude == 0)
            return;
        var targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }
    void ApplyMovement()
    {
        characterController.Move(transform.TransformDirection(_direction) * speed * Time.deltaTime);
    }
    public void SetAnimator()
    {
        animator.SetFloat("Xspeed", _direction.x * speed);
        animator.SetFloat("Zspeed", _direction.z * speed);
    }
    #endregion
    #region InputAction Function
    public void OnMove(InputAction.CallbackContext context)
    {
        _direction = context.ReadValue<Vector3>();
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        Vector2 vector2 = context.ReadValue<Vector2>();
        mouseX += vector2.x * cameraSpeed * Time.deltaTime;
        mouseY -= vector2.y * cameraSpeed * Time.deltaTime;
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);
        this.transform.localEulerAngles = new Vector3(0, mouseX, 0);
        vCam.localEulerAngles = new Vector3(mouseY, 0, 0);
    }
    public void OnUseItem(InputAction.CallbackContext context)
    {
        inventory.UseItem();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGround()) return;
        _velocity += jumpforce;
    }
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
            speed = runSpeed;
        else if (context.performed) { }
        else if (context.canceled)
            speed = normalSpeed;
    }
    public void OnInteraction(InputAction.CallbackContext context)
    {

    }
    public void OnCrouch(InputAction.CallbackContext context)
    {

    }
    public void OnChangeItem(InputAction.CallbackContext context)
    {
        switch(context.control.name)
        {
            case "1":
                inventory.ChangeItemSlot(0);
                break;
            case "2":
                inventory.ChangeItemSlot(1);
                break;
            case "3":
                inventory.ChangeItemSlot(2);
                break;
            case "4":
                inventory.ChangeItemSlot(3);
                break;
        }
    }
    public void OnDetachItem(InputAction.CallbackContext context)
    {
        inventory.RemoveItem();
    }
    #endregion
    #region Network Command Function
    [Command]
    public void CmdTeleport(Transform transform, Vector3 position)
    {
        OnClientTeleport(transform, position);
    }
    #endregion
    #region Network ClientRpc Function
    [ClientRpc]
    public void OnClientTeleport(Transform transform, Vector3 position)
    {
        transform.position = position;
    }
    #endregion

    public void EnbleGameplayControls()
    {

    }
}

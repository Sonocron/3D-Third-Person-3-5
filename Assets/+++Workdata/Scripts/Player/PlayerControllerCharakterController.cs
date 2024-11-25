using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerCharakterController : MonoBehaviour
{
    private static readonly int Hash_MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int Hash_Grounded = Animator.StringToHash("Grounded");
    private static readonly int Hash_Crouched = Animator.StringToHash("Crouched");
    private static readonly int Hash_Jumped = Animator.StringToHash("Jumped");
    private static readonly int Hash_ActionTrigger = Animator.StringToHash("ActionTrigger");
    private static readonly int Hash_ActionId = Animator.StringToHash("ActionId");
    
    private static readonly int Hash_WeaponEquipTrigger = Animator.StringToHash("WeaponEquipTrigger");
    private static readonly int Hash_WeaponId = Animator.StringToHash("WeaponId");
    private static readonly int Hash_WeaponUnequipTrigger = Animator.StringToHash("WeaponUnequipTrigger");
    
    #region Inspector
    
    [FormerlySerializedAs("movementSpeed")]
    [Header("Movement")]
    
    [Min(0)]
    [Tooltip("The speed values of the player in uu/s")]
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    
    [SerializeField] private float jumpHeight = 3f;
    
    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing.")]
    [SerializeField] private float speedChangeRate = 10f;

    [SerializeField] private float rotationSpeed = 10f;

    [Header("Slope Movement")] 
    
    [SerializeField] private float pullDownForce = 5f;

    [SerializeField] private LayerMask raycastMask = 1;

    [SerializeField] private float raycastLength = 0.5f;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;

    [SerializeField] private float verticalCameraRotationMax = 70f;

    [SerializeField] private float cameraHorizontalSpeed = 200f;

    [SerializeField] private float cameraVerticalSpeed = 130f;
    
    [Header("Animator")]
    public Animator animator;

    [SerializeField] private float coyoteTime = .2f;
    
    [Header("Mouse Settings")]
    [Range(0f,2f)]
    [SerializeField] private float mouseCameraSensitivity = 1f;

    [Header("Controller Settings")]
    [Range(0f,2f)]
    [SerializeField] private float controllerCameraSensitivity = 1f;

    [SerializeField] private bool invertY = true;
    #endregion
    
    #region Private Variables
    private CharacterController characterController;
    
    public GameInput inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    private InputAction interactAction;
    
    private Interactable selectedInteractable;
    
    private Vector2 moveInput;
    private Vector2 lookInput;
    
    private Quaternion characterTargetRotation = Quaternion.identity;

    private Vector3 lastMovement;
    private Vector2 cameraRotation;

    private bool isGrounded = true;
    
    private float airTime;
    private bool isRunning;
    private bool isCrouched;
    private float currentSpeed;

    private int upperBody_AnimLayer;
    
    [SerializeField] private float gravity = -19.62f;
    private Vector3 velocity;
    
    #endregion
    
    #region Unity Event Functios
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        //animator = GetComponentInChildren<>()
        upperBody_AnimLayer = animator.GetLayerIndex("Upper Body");
        
        inputActions = new GameInput();
        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        runAction = inputActions.Player.ShiftRun;
        crouchAction = inputActions.Player.Crouch;
        jumpAction = inputActions.Player.Jump;
        interactAction = inputActions.Player.Interact;
        
        characterTargetRotation = transform.rotation;
        cameraRotation = cameraTarget.rotation.eulerAngles;

        currentSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        EnableInput();
        runAction.performed += ShiftInput;
        runAction.canceled += ShiftInput;

        crouchAction.performed += CrouchInput;
        crouchAction.canceled += CrouchInput;
        
        jumpAction.performed += JumpInput;
        
        interactAction.performed += Interact;
    }

    private void Update()
    {
        ReadInput();
        
        Rotate(moveInput);
        Move(moveInput);

        CheckGround();
        
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        RotateCamera(lookInput);
    }

    private void OnDisable()
    {
        DisableInput();
        runAction.performed -= ShiftInput;
        runAction.canceled -= ShiftInput;
        
        crouchAction.performed -= CrouchInput;
        crouchAction.canceled -= CrouchInput;
        
        jumpAction.performed -= JumpInput;
        
        interactAction.performed -= Interact;

    }
    
    #endregion
    
    #region Input

    public void EnableInput()
    {
        inputActions.Enable();
    }

    public void DisableInput()
    {
        inputActions.Disable();
    }
    
    void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
    }

    private void ShiftInput(InputAction.CallbackContext ctx)
    {
        isRunning = ctx.performed;

        currentSpeed = isRunning ? runSpeed : walkSpeed;
    }
    
    private void CrouchInput(InputAction.CallbackContext ctx)
    {
        isCrouched = ctx.performed;

        currentSpeed = isCrouched ? crouchSpeed : walkSpeed;
    }

    private void JumpInput(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool(Hash_Jumped, true);
        }
    }
    
    #endregion
    
    #region Movement
    
    private void Rotate(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;

            Vector3 worldInputDirection = cameraTarget.TransformDirection(inputDirection);
            worldInputDirection.y = 0;
            
            characterTargetRotation = Quaternion.LookRotation(worldInputDirection);
        }

        if (Quaternion.Angle(transform.rotation, characterTargetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, characterTargetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = characterTargetRotation;
        }
    }
    
    private void Move(Vector2 moveInput)
    { 
                            //(Ist mein Input == 0,0     JA . ODER...    
        float targetSpeed = moveInput == Vector2.zero ? 0 : this.currentSpeed * moveInput.magnitude;

        Vector3 currentVelocity = lastMovement;
        currentVelocity.y = 0;

        float currentSpeed = currentVelocity.magnitude;

        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
        }
        else
        {
            currentSpeed = targetSpeed;
        }

        Vector3 targetDirection = characterTargetRotation * Vector3.forward;

        Vector3 movement = targetDirection * currentSpeed;
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        movement.y = velocity.y;

        characterController.Move(movement * Time.deltaTime);

        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit,
                raycastLength, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.ProjectOnPlane(movement, hit.normal).y < 0)
            {
                characterController.Move(Vector3.down * (pullDownForce * Time.deltaTime));
            }
        }
        
        lastMovement = movement;
    }
    
    #endregion
    
    #region Ground Check

    private void CheckGround()
    {
        if (characterController.isGrounded)
        {
            airTime = 0;
        }
        else
        {
            airTime += Time.deltaTime;
        }

        isGrounded = airTime < coyoteTime;
    }
    
    #endregion
    
    #region Animator

    private void UpdateAnimator()
    {
        Vector3 velocity = lastMovement;
        velocity.y = 0;
        float speed = velocity.magnitude;
        
        animator.SetFloat(Hash_MovementSpeed, speed);
        animator.SetBool(Hash_Grounded, isGrounded);
        animator.SetBool(Hash_Crouched, isCrouched);
    }

    public void JumpEnd()
    {
        animator.SetBool(Hash_Jumped, false);
    }

    public void UpperBody_Layer(float weight)
    {
        animator.SetLayerWeight(upperBody_AnimLayer, weight);
    }
    
    public void AnimationsAction(int id)
    {
        animator.SetTrigger(Hash_ActionTrigger);
        animator.SetInteger(Hash_ActionId, id);
    }

    public void AnimationsWeaponEquip(int id)
    {
        animator.SetTrigger(Hash_WeaponEquipTrigger);
        animator.SetInteger(Hash_WeaponId, id);
    }
    
    public void AnimationsWeaponUnequip(int id)
    {
        animator.SetTrigger(Hash_WeaponUnequipTrigger);
        animator.SetInteger(Hash_WeaponId, id);
    }
    
    public void EndAction()
    {
        UpperBody_Layer(0);
        animator.SetInteger(Hash_ActionId, 0);
    }
    
    #endregion
    
    #region Physics
    private void OnTriggerEnter(Collider other)
    {
        TrySelectInteractable(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TryDeselectInteractable(other);
    }

    #endregion
    
    #region Interaction

    private void Interact(InputAction.CallbackContext ctx)
    {
        if (selectedInteractable != null)
        {
            selectedInteractable.Interact();
        }
    }

    private void TrySelectInteractable(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null){ return; }

        if (selectedInteractable != null)
        {
            selectedInteractable.Deselect();
        }
        
        selectedInteractable = interactable;
        selectedInteractable.Select();
    }
    
    private void TryDeselectInteractable(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null){ return; }

        if (interactable == selectedInteractable)
        {
            selectedInteractable.Deselect();
            selectedInteractable = null;
        }
    }
    
    
    #endregion
    
    #region Camera

    private void RotateCamera(Vector2 lookInput)
    {
        if (lookInput != Vector2.zero)
        {
            bool isMouseLook = IsMouseLook();

            float deltaTimeMultiplier = isMouseLook ? 1 : Time.deltaTime;
            //                     Bedingung     true                       false
            float sensitivity = isMouseLook ? mouseCameraSensitivity : controllerCameraSensitivity;

            lookInput *= deltaTimeMultiplier * sensitivity;
            
            cameraRotation.x += lookInput.y * cameraVerticalSpeed * (!isMouseLook && invertY ? -1 : 1);

            cameraRotation.y += lookInput.x * cameraHorizontalSpeed;

            cameraRotation.x = NormalizeAngle(cameraRotation.x);
            cameraRotation.y = NormalizeAngle(cameraRotation.y);

            cameraRotation.x = Mathf.Clamp(cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }
        
        cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360;

        if (angle < 0)
        {
            angle += 360;
        }

        if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    private bool IsMouseLook()
    {
        if (lookAction.activeControl == null)
        {
            return true;
        }

        return lookAction.activeControl.device.name == "Mouse";
    }
    
    #endregion
    
}

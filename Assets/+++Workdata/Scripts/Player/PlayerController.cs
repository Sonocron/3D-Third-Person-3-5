using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private static readonly int Hash_MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int Hash_Grounded = Animator.StringToHash("Grounded");
    private static readonly int Hash_Crouched = Animator.StringToHash("Crouched");
    private static readonly int Hash_Jumped = Animator.StringToHash("Jumped");
    
    #region Inspector
    
    [FormerlySerializedAs("movementSpeed")]
    [Header("Movement")]
    
    [Min(0)]
    [Tooltip("The speed values of the player in uu/s")]
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;
    
    [FormerlySerializedAs("jumpHeight")] [SerializeField] private float jumpForce = 3f;
    
    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing.")]
    [SerializeField] private float speedChangeRate = 10f;

    [SerializeField] private float rotationSpeed = 10f;

    [Header("Ground")]
    [SerializeField] private LayerMask raycast_GroundMask = 1;
    [SerializeField] private float raycast_GroundLength = 0.1f;
    
    [Header("Slope Movement")] 
    
    [SerializeField] private float pullDownForce = 5f;

    [SerializeField] private LayerMask raycast_SlopeMask = 1;

    [SerializeField] private float raycastLength = 0.5f;
    
    [Header("Camera")]
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;

    [SerializeField] private float verticalCameraRotationMax = 70f;

    [SerializeField] private float cameraHorizontalSpeed = 200f;

    [SerializeField] private float cameraVerticalSpeed = 130f;
    
    [Header("Animator")]
    [SerializeField] private Animator animator;

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
    private Rigidbody rb;
    
    private GameInput inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction crouchAction;
    private InputAction jumpAction;
    
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
    [SerializeField] private float gravity = -19.62f;
    private Vector3 velocity;
    
    #endregion
    
    #region Unity Event Functios
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        inputActions = new GameInput();
        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        runAction = inputActions.Player.ShiftRun;
        crouchAction = inputActions.Player.Crouch;
        jumpAction = inputActions.Player.Jump;
        
        characterTargetRotation = transform.rotation;
        cameraRotation = cameraTarget.rotation.eulerAngles;

        currentSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        runAction.performed += ShiftInput;
        runAction.canceled += ShiftInput;

        crouchAction.performed += CrouchInput;
        crouchAction.canceled += CrouchInput;
        
        jumpAction.performed += JumpInput;
    }

    private void FixedUpdate()
    {
        ReadInput();
        Rotate(moveInput);
        Move(moveInput);
        
        CheckGround();
    }

    private void LateUpdate()
    {
        UpdateAnimator();
        RotateCamera(lookInput);
    }

    private void OnDisable()
    {
        inputActions.Disable();
        runAction.performed -= ShiftInput;
        runAction.canceled -= ShiftInput;
        
        crouchAction.performed -= CrouchInput;
        crouchAction.canceled -= CrouchInput;
        
        jumpAction.performed -= JumpInput;
    }

    private void OnDestroy()
    {
        
    }
    #endregion
    
    #region Input

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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        float targetSpeed = moveInput == Vector2.zero ? 0 : this.currentSpeed * moveInput.magnitude;

        Vector3 targetDirection = characterTargetRotation * Vector3.forward;

        Vector3 movement = targetDirection * targetSpeed;
        
        Vector3 velocity = rb.velocity;
        velocity.x = movement.x;
        velocity.z = movement.z;
        
        rb.velocity = velocity;
        

        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit,
                raycastLength, raycast_SlopeMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.ProjectOnPlane(movement, hit.normal).y < 0)
            {
                //characterController.Move(Vector3.down * (pullDownForce * Time.deltaTime));
            }
        }
        
        lastMovement = movement;
    }
    
    #endregion
    
    #region Ground Check

    private void CheckGround()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit,
                raycast_GroundLength, raycast_GroundMask, QueryTriggerInteraction.Ignore))
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
    
    #endregion
    
    #region Camera

    private void RotateCamera(Vector2 lookInput)
    {
        if (lookInput != Vector2.zero)
        {
            bool isMouseLook = IsMouseLook();
            float deltaTimeMultiplier = isMouseLook ? 1 : Time.deltaTime;
            float sensitivity = isMouseLook ? mouseCameraSensitivity : controllerCameraSensitivity;

            lookInput *= deltaTimeMultiplier * sensitivity;
            
            cameraRotation.x += lookInput.y * cameraVerticalSpeed * (invertY ? -1 : 1);
            cameraRotation.y += lookInput.x * cameraHorizontalSpeed;
            
            cameraRotation.x = NormalizeAngle(cameraRotation.x);
            cameraRotation.y = NormalizeAngle(cameraRotation.y);
            
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
            
            cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180)
        {
            angle += 360;
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

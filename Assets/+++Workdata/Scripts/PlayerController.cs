using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private static readonly int Hash_MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int Hash_Grounded = Animator.StringToHash("Grounded");
    private static readonly int Hash_Crouch = Animator.StringToHash("Crouch");

    #region Inspector

    [FormerlySerializedAs("movementSpeed")]
    [Header("Movement")]
    
    [Min(0)]
    [Tooltip("The maximum speed of the player in uu/s")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    
    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing")]
    [SerializeField] private float speedChangeRate = 10f;

    [SerializeField] private float rotationSpeed = 10f;

    [Header("Slope Movement")] 
    
    [SerializeField] private float pullDownForce = 5f;

    [SerializeField] private LayerMask raycastMask;

    [SerializeField] private float raycastLength = 0.5f;
    
    [Header("Camera")] 
    
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;

    [SerializeField] private float verticalCameraRotationMax = 70f;

    [SerializeField] private float cameraHorizontalSpeed = 200f;

    [SerializeField] private float cameraVerticalSpeed = 130f;

    [Header("Animations")] 
    [SerializeField] private Animator animator;

    [SerializeField] private float coyoteTime;
    
    [Header("Mouse Settings")] 
    
    [SerializeField] private float mouseCameraSensitivity = 1f;
    
    [Header("Controller Settings")] 
    [SerializeField] private float controllerCameraSensitivity = 1f;

    [SerializeField] private bool invertY = true;

    #endregion
    
    #region Private Variables
    private CharacterController characterController;
    
    private GameInput inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction runAction;
    private InputAction crouchAction;
    
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Quaternion characterTargetRotation = Quaternion.identity;
    
    private Vector2 cameraRotation;
    
    private Vector3 lastMovement;

    private bool isGrounded;
    private bool isRunning;
    private bool isCrouching;
    private float airTime;
    private float currentSpeed = 3f;
    #endregion
    
    #region Event Functions
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
           
        inputActions = new GameInput();
        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
        runAction = inputActions.Player.ShiftRun;
        crouchAction = inputActions.Player.Crouch;

        characterTargetRotation = transform.rotation;
        cameraRotation = cameraTarget.rotation.eulerAngles;

        currentSpeed = walkSpeed;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        runAction.performed += Run;
        runAction.canceled += Run;
        
        crouchAction.performed += Crouch;
        crouchAction.canceled += Crouch;
    }

    private void Update()
    {
        ReadInput();

        Rotate(moveInput);
        Move(moveInput);

        GroundCheck();
        UpdateAnimator();
    }

    private void LateUpdate()
    {
        RotateCamera(lookInput);
    }

    private void OnDisable()
    {
        inputActions.Disable();
        runAction.performed -= Run;
        runAction.canceled -= Run;
        
        crouchAction.performed -= Crouch;
        crouchAction.canceled -= Crouch;
    }

    private void OnDestroy()
    {
        
    }
    #endregion

    #region Input

    private void ReadInput()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();
    }

    private void Run(InputAction.CallbackContext ctx)
    {
        isRunning = ctx.performed;
        currentSpeed = isRunning ? runSpeed : walkSpeed;
    }
    
    private void Crouch(InputAction.CallbackContext ctx)
    {
        isCrouching = ctx.performed;
        currentSpeed = isCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed;
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
            transform.rotation =
                Quaternion.Slerp(transform.rotation, characterTargetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = characterTargetRotation;
        }
    }
    
    private void Move(Vector2 moveInput)
    {
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
        characterController.SimpleMove(movement);

        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down,
                out RaycastHit hit, raycastLength, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.ProjectOnPlane(movement, hit.normal).y < 0)
            {
                characterController.Move(Vector3.down * (pullDownForce * Time.deltaTime));
            }
        }
        
        lastMovement = movement;
    }

    #endregion

    #region GroundCheck

    private void GroundCheck()
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
        animator.SetBool(Hash_Crouch, isCrouching);
    }

    #endregion
    
    #region Camera

    private void RotateCamera(Vector2 lookInput)
    {
        if (lookInput != Vector2.zero)
        {
            bool isMouseLook = IsMouseLook(); 
                            //   ist mein bool true ? true : false
            float deltaTimeMultiplier = isMouseLook ? 1 : Time.deltaTime;

            float sensitivity = isMouseLook ? mouseCameraSensitivity : controllerCameraSensitivity;
            
            /*       
            if (isMouseLook)
            {
                sensitivity = mouseCameraSensitivity;
            }
            else
            {
                sensitivity = controllerCameraSensitivity;
            }
            */

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

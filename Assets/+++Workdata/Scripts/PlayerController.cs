using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Inspector

    [Header("Movement")]
    
    [Min(0)]
    [Tooltip("The maximum speed of the player in uu/s")]
    [SerializeField] private float movementSpeed = 5f;

    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing")]
    [SerializeField] private float speedChangeRate = 10f;

    [Header("Camera")] 
    
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float verticalCameraRotationMin = -30f;

    [SerializeField] private float verticalCameraRotationMax = 70f;

    [SerializeField] private float cameraHorizontalSpeed = 200f;

    [SerializeField] private float cameraVerticalSpeed = 130f;

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
    
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector2 cameraRotation;
    
    private Vector3 lastMovement;
    #endregion
    
    #region Event Functions
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        inputActions = new GameInput();
        moveAction = inputActions.Player.Move;
        lookAction = inputActions.Player.Look;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void Update()
    {
        ReadInput();

        Rotate(moveInput);
        Move(moveInput);
    }

    private void LateUpdate()
    {
        RotateCamera(lookInput);
    }

    private void OnDisable()
    {
        inputActions.Disable();
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

    #endregion
    
    #region Movement

    private void Rotate(Vector2 moveInput)
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            transform.rotation = Quaternion.LookRotation(inputDirection);
        }
    }
    
    private void Move(Vector2 moveInput)
    {
        float targetSpeed = moveInput == Vector2.zero ? 0 : movementSpeed * moveInput.magnitude;
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

        Vector3 movement = transform.forward * currentSpeed;
        characterController.SimpleMove(movement);
        lastMovement = movement;
    }

    #endregion

    #region Camera

    private void RotateCamera(Vector2 lookInput)
    {
        if (lookInput != Vector2.zero)
        {
            cameraRotation.x += lookInput.y * cameraVerticalSpeed;
            cameraRotation.y += lookInput.x * cameraHorizontalSpeed;
        }
        
        cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
    }

    #endregion
}

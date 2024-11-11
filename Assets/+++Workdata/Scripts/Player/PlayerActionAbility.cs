using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionAbility : MonoBehaviour
{
    private PlayerControllerCharakterController playerController;
    private InputAction leftClickAction;

    
    private void Awake()
    {
        playerController = GetComponent<PlayerControllerCharakterController>();
    }

    private void OnEnable()
    {
        StartCoroutine(DelayReference());
    }

    private void OnDisable()
    {
        leftClickAction.performed -= ActionInput;
    }

    IEnumerator DelayReference()
    {
        yield return null;
        leftClickAction = playerController.inputActions.Player.Fire;
        leftClickAction.performed += ActionInput;
    }
    
    public void ActionInput(InputAction.CallbackContext ctx)
    {
        playerController.UpperBody_Layer(1);
        playerController.AnimationsAction(1);
    }


}

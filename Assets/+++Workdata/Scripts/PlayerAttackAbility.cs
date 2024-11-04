using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackAbility : MonoBehaviour
{
    private PlayerController playerController;
    
    private InputAction attackAction;

    public GameObject leftHand;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PlayerController.EnableAbilities += EnableAbility;
        PlayerController.DisableAbilities += DisableAbility;
    }
    
    private void OnDisable()
    {
        PlayerController.EnableAbilities -= EnableAbility;
        PlayerController.DisableAbilities -= DisableAbility;
    }

    public void EnableAbility()
    {
        StartCoroutine(DelayAbility());
    }

    IEnumerator DelayAbility()
    {
        yield return null;
        attackAction = playerController.inputActions.Player.Fire;
        attackAction.performed += AttackInput;
    }
    
    public void DisableAbility()
    {
        attackAction.performed -= AttackInput;
    }

    private void AttackInput(InputAction.CallbackContext ctx)
    {
        playerController.SetAnimator_UppderBody_LayerWeight(1);
        playerController.ActionTrigger(1);
    }
}

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
        PlayerWeaponAbility playerWeaponAbility = GetComponent<PlayerWeaponAbility>();

        if (playerWeaponAbility)
        {
            if (playerWeaponAbility.weaponState == PlayerWeaponAbility.WeaponState.TwoHandSword)
            {
                if (playerController.animator.GetInteger("ActionId") == 10)
                {
                    print("Combo 1");
                    playerController.AnimationsAction(11);
                }
                else if (playerController.animator.GetInteger("ActionId") == 11)
                {
                    print("Combo 2");
                    playerController.AnimationsAction(12);
                }
                else
                {
                    print("Normal");
                    playerController.AnimationsAction(10);
                }
                    
            }
            else if (playerWeaponAbility.weaponState == PlayerWeaponAbility.WeaponState.Unarmed)
            {
                playerController.UpperBody_Layer(1);
                playerController.AnimationsAction(1);
            }
        }
        else
        {
            playerController.UpperBody_Layer(1);
            playerController.AnimationsAction(1);
        }
        
        

    }


}

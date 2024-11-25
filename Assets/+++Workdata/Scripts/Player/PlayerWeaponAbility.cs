using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerWeaponAbility : MonoBehaviour
{
    public enum WeaponState{ Unarmed, TwoHandSword, OneHandSword}

    public WeaponState weaponState;
    
    private PlayerControllerCharakterController playerController;
    private InputAction equipUneqeuipAction;

    public GameObject twoHandSwordBack, twoHandSwordHand;
    
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
        equipUneqeuipAction.performed -= WeaponEquipUnequip;
    }

    IEnumerator DelayReference()
    {
        yield return null;
        equipUneqeuipAction = playerController.inputActions.Player.WeaponDrawn;
        equipUneqeuipAction.performed += WeaponEquipUnequip;
    }
    
    public void WeaponEquipUnequip(InputAction.CallbackContext ctx)
    {
        switch (weaponState)
        {
            case WeaponState.Unarmed:
                playerController.AnimationsWeaponEquip(1);
                break;
            
            case WeaponState.OneHandSword:

                break;
            
            case WeaponState.TwoHandSword:
                playerController.AnimationsWeaponUnequip(0);
                break;
        }
    }

    public void WeaponSwitch()
    {
        switch (weaponState)
        {
            case WeaponState.Unarmed:
                
                twoHandSwordBack.SetActive(false);
                twoHandSwordHand.SetActive(true);
                weaponState = WeaponState.TwoHandSword;
                break;
            
            case WeaponState.OneHandSword:

                break;
            
            case WeaponState.TwoHandSword:
                twoHandSwordBack.SetActive(true);
                twoHandSwordHand.SetActive(false);
                weaponState = WeaponState.Unarmed;

                break;
        }
    }

}

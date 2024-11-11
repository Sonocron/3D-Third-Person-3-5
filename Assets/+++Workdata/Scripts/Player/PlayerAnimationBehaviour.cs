using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    private PlayerControllerCharakterController playerController;
    [SerializeField] Collider leftHandCollider, leftFootCollider, rightHandCollider, rightFootCollider;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerControllerCharakterController>();
    }

    public void JumpEnd()
    {
        playerController.JumpEnd();
    }

    public void EndAction()
    {
        playerController.EndAction();
    }
    
    
    public void ActivateDmgLeftHand()
    {
        leftHandCollider.enabled = true;
    }

    public void ActivateDmgRightHand()
    {
        rightHandCollider.enabled = true;
    }

    public void ActivateDmgLeftFoot()
    {
        leftFootCollider.enabled = true;
    }

    public void ActivateDmgRightFoot()
    {
        rightFootCollider.enabled = true;
    }
    
    public void DeactivateDmgLeftHand()
    {
        leftHandCollider.enabled = false;
    }

    public void DeactivateDmgRightHand()
    {
        rightHandCollider.enabled = false;
    }

    public void DeactivateDmgLeftFoot()
    {
        leftFootCollider.enabled = false;
    }

    public void DeactivateDmgRightFoot()
    {
        rightFootCollider.enabled = false;
    }
}

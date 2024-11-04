using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationBehaviour : MonoBehaviour
{
    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    public void EndJump()
    {
        playerController.EndJump();
    }

    public void EndAttack()
    {
        playerController.SetAnimator_UppderBody_LayerWeight(0);
    }
}

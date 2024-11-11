using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private static readonly int Hash_Hit = Animator.StringToHash("Hit");
    private static readonly int Hash_Dead = Animator.StringToHash("Dead");
    private static readonly int Hash_MovementType = Animator.StringToHash("MovementType");
    private static readonly int Hash_ActionTrigger = Animator.StringToHash("ActionTrigger");

    #region Inspector

    [SerializeField] int enemyHealth;
    
    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] private float blendSpeed = 1;
    
    #endregion
    
    #region Private Variables

    #endregion
    
    
    public void GetDamage(int damage)
    {
        if (enemyHealth < 1) return;
        
        enemyHealth -= damage;

        animator.SetTrigger(Hash_ActionTrigger);
        if (enemyHealth < 1)
        {
            OnDeath();
        }
        else
        {
            OnHit();
        }
    }
    
    public void OnDeath()
    {
        animator.SetTrigger(Hash_Dead);
    }
    
    public void OnHit()
    {
        animator.SetTrigger(Hash_Hit);
    }
    
    public void OnAggroEnter(int movementTypeId)
    {
        print("check");
        StartCoroutine(AggroMovementType(1));
    }
    
    public void OnAggroExit()
    {
        StartCoroutine(AggroMovementType(0));
    }


    IEnumerator AggroMovementType(float targetValue)
    {
        while (Mathf.Abs(animator.GetFloat(Hash_MovementType) - targetValue) > 0.01)
        {
            float movementTypeValue =
                ValueInterpolator.MoveTowards(animator.GetFloat(Hash_MovementType), 
                    targetValue, blendSpeed, Time.deltaTime);
            
            animator.SetFloat(Hash_MovementType, movementTypeValue);
            yield return null;
        }
        
        animator.SetFloat(Hash_MovementType, targetValue);
    }
}

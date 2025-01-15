using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NavMeshChase : MonoBehaviour
{
 private static readonly int Hash_MovementSpeed = Animator.StringToHash("MovementSpeed");
    
    #region Inspector

    [SerializeField] private Animator anim;
    [SerializeField] private Transform player;
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private float rotationDistance = 2f;
    [SerializeField] private float attackDistance = 2f;

    
    [Header("Gizmos")] 
    [SerializeField] private bool showGizmos = true;

    #endregion
    
    #region Private Variables

    private NavMeshAgent navMeshAgent;
    private EnemyBehaviour enemyBehaviour;
    private bool chasePlayer;
    private bool attackReady;
    
    private Vector3 originPos;
    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyBehaviour = GetComponent<EnemyBehaviour>();
        
        navMeshAgent.autoBraking = true;
        originPos = transform.position;
    }

    private void Update()
    {
        anim.SetFloat(Hash_MovementSpeed, navMeshAgent.velocity.magnitude);
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (!navMeshAgent.isStopped && chasePlayer)
        {
            navMeshAgent.destination = player.position;
        }
        else
        {
            navMeshAgent.destination = originPos;
        }

        if (distanceToPlayer <= rotationDistance)
        {
            SmoothLookAtPlayer();
        }

        if (distanceToPlayer <= attackDistance && !attackReady)
        {
            attackReady = true;
            enemyBehaviour.CanAttackPlayer(true);
        }
        else if (distanceToPlayer > attackDistance && attackReady)
        {
            attackReady = false;
            enemyBehaviour.CanAttackPlayer(false);
        }
        
    }

    #endregion

    #region Navigation

    public void StopChasingPlayer()
    {
        chasePlayer = false;
        //navMeshAgent.isStopped = true;
    }

    public void StartChasePlayer()
    {
        chasePlayer = true;
        //navMeshAgent.isStopped = false;
    }

    #endregion

    #region Helper Methods

    private void SmoothLookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rotationDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
    }
}

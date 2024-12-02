using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class NavMeshPatrol : MonoBehaviour
{
    private static readonly int Hash_MovementSpeed = Animator.StringToHash("MovementSpeed");
    
    #region Inspector

    [SerializeField] private Animator anim;
    
    [Header("Waypoints")] 
    [SerializeField] private bool randomOrder;
    
    [SerializeField] private List<Transform> waypoints;
    
    [SerializeField] private bool waitAtWaypoint;

    [SerializeField] private Vector2 waitDuration = new Vector2(1, 5);

    [Header("Gizmos")] 
    [SerializeField] private bool showWaypoints = true;

    #endregion
    
    #region Private Variables

    private NavMeshAgent navMeshAgent;
    
    private int currentWaypointIndex = -1;

    private bool isWaiting;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoBraking = waitAtWaypoint;
    }

    private void Start()
    {
        SetNextWaypoint();
    }

    private void Update()
    {
        anim.SetFloat(Hash_MovementSpeed, navMeshAgent.velocity.magnitude);
        
        if (!navMeshAgent.isStopped)
        {
            CheckIfWaypointIsReached();
        }
    }

    #endregion

    #region Navigation

    public void StopPatrolForDialogue()
    {
        // trun to player
        StopPatrol();
        DialogueController.DialogueClosed += ResumePatrol;
    }

    public void StopPatrol()
    {
        navMeshAgent.isStopped = true;
    }

    public void ResumePatrol()
    {
        navMeshAgent.isStopped = false;
        DialogueController.DialogueClosed -= ResumePatrol;
    }

    private void SetNextWaypoint()
    {
        switch (waypoints.Count)
        {
            case 0:
                Debug.LogError("No waypoints set for NavMesh", this);
                return;
                break;
            
            case 1:

                if (randomOrder)
                {
                    Debug.LogError("Only one waypoint set NachMeshPatrol. Need at least 2 with randomOrder enabled", this);
                    return;
                }
                else
                {
                    Debug.LogError("Only one waypoint set NachMeshPatrol.", this);
                    return;
                }

                break;
        }

        if (randomOrder)
        {
            int newWaypointIndex;
            
            do
            {
                newWaypointIndex = Random.Range(0, waypoints.Count);
            } while (newWaypointIndex == currentWaypointIndex);

            currentWaypointIndex = newWaypointIndex;
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }

        navMeshAgent.destination = waypoints[currentWaypointIndex].position;
    }

    private void CheckIfWaypointIsReached()
    {
        if (isWaiting)
        {
            //....
            return;
        }

        if (navMeshAgent.pathPending)
        {
            return;
        }

        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.01f)
        {
            if (waitAtWaypoint)
            {
                StartCoroutine(WaitBeforeNextWaypoint(Random.Range(waitDuration.x, waitDuration.y)));
            }
            else
            {
                SetNextWaypoint();
            }
        }
        
    }

    IEnumerator WaitBeforeNextWaypoint(float duration)
    {
        isWaiting = true;
        yield return new WaitForSeconds(duration);
        isWaiting = false;
        
        SetNextWaypoint();
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (!showWaypoints) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            Transform waypoint = waypoints[i];
            Gizmos.color = currentWaypointIndex == i ? Color.green : Color.yellow;
            Gizmos.DrawSphere(waypoint.position, .3f);

            if (!randomOrder)
            {
                Gizmos.DrawLine(
                    i == 0 ? waypoints[^1].position : waypoints[i-1].position,
                    waypoints[i].position);
            }
        }
    }
}

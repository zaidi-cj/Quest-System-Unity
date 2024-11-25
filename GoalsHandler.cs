using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GoalsHandler : MonoBehaviour
{

    public GameObject itemPrefab;
    public Transform spawnLocation;
    public Transform playerTransform;
    public QuestManager questManager;
    public float playerDetectionDistance = 5f;
    public float stopDistance = 10f;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private HealthSystem healthSystem;
    private Dictionary<Goals, bool> instantiatedForGoal = new Dictionary<Goals, bool>();
    private void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        if(Vector3.Distance(playerTransform.position , transform.position) <= playerDetectionDistance)
        {
            HandleItemRetrievalGoal();
        }
        if(questManager.HasActiveEscortGoal(out Goals escortGoal))
        {
            HandleEscortGoal(escortGoal);
        }
        else
        {
            animator.SetBool("walking", false);
            navMeshAgent.isStopped = true;
        }

    }

    private void HandleItemRetrievalGoal()
    {
       
        if(questManager.HasActiveItemRetrievalGoal(out Goals itemRetrievalGoal))
        {
            if(!instantiatedForGoal.ContainsKey(itemRetrievalGoal) || !instantiatedForGoal[itemRetrievalGoal] )
            {
                for (int i = 0; i < itemRetrievalGoal.AmountToRetrieve; i++)
                {
                    Vector3 SpawnPoint = spawnLocation.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
                    Instantiate(itemPrefab, SpawnPoint, Quaternion.identity);
                }
                instantiatedForGoal[itemRetrievalGoal] = true;
            }
        }
    }

    public void HandleEscortGoal(Goals escortGoal)
    {
        if (!healthSystem.isDead)
        {
            float playerDistance = Vector3.Distance(playerTransform.position, transform.position);
            if (playerDistance <= playerDetectionDistance && !escortGoal.Completed)
            {
                navMeshAgent.SetDestination(escortGoal.destinationPos);
                animator.SetBool("walking", true);
                if (playerDistance > stopDistance)
                {
                    animator.SetBool("walking", false);
                    navMeshAgent.isStopped = true;
                }
                else
                {
                    animator.SetBool("walking", true);
                    navMeshAgent.isStopped = false;
                }
                float remainingDistance = Vector3.Distance(transform.position, escortGoal.destinationPos);
                EventHandler.NpcEscorted?.Invoke(remainingDistance);
            }
            else
            {
                animator.SetBool("walking", false);
                navMeshAgent.isStopped = true;
            }
        }
        else
        {
            escortGoal.Completed = false;
            escortGoal.failed = true;
            EventHandler.NpcEscorted?.Invoke(Vector3.Distance(playerTransform.position, transform.position));
            Debug.Log("Goal failed");
        }


    }
    private void OnFootstep()
    {
        //
    }
}

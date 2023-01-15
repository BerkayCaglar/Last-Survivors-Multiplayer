using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
public class ZombieAI : MonoBehaviour
{
    private NavMeshAgent Agent;
    [SerializeField]
    LayerMask LookingFor;
    [SerializeField]
    private float searchRadius,attackRange;
    [SerializeField]
    private Vector3 AttackRangePosition;
    public GameObject attackTo;
    public bool playerInAttackRange,dealDamage;
    private const int zombieAttackDamage = 5;
    private void Start() 
    {
        Agent = GetComponent<NavMeshAgent>();
        StartCoroutine(SearchPlayer());
    }
    private void Update() 
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
        if(!playerInAttackRange && attackTo != null)
        {
            Agent.SetDestination(attackTo.transform.position);
        }
        if(attackTo != null) 
        {
            foreach(Collider i in Physics.OverlapSphere(transform.position + AttackRangePosition,attackRange,LookingFor))
            {
                playerInAttackRange = true;
                Agent.SetDestination(transform.position);
                if(!dealDamage)
                {
                    dealDamage = true;
                    StartCoroutine(AttackPlayer(attackTo));
                }
                return;
            }
            playerInAttackRange = false;
        }
    }
    private IEnumerator AttackPlayer(GameObject Player)
    {
        yield return new WaitForSeconds(1.6f);
        if(playerInAttackRange || attackTo != null)
        {
            try
            {
                Player.GetComponent<PlayerManager>().TakenDamage(zombieAttackDamage);
            }
            catch
            {
                
            }
        }
        dealDamage = false;
    }
    private IEnumerator SearchPlayer()
    {
        GameObject nearestPlayer = null;
        float nearestDistance = Mathf.Infinity;
        foreach (Collider i in Physics.OverlapSphere(transform.position, searchRadius, LookingFor))
        {
            float distance = Vector3.Distance(transform.position, i.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlayer = i.gameObject;
            }
        }
        attackTo = nearestPlayer;
        yield return new WaitForSeconds(1f);
        StartCoroutine(SearchPlayer());
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + AttackRangePosition, attackRange);
    }
}

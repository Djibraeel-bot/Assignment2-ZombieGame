using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.Animations;

public class enemyAI : MonoBehaviour
{
    //public int health = 100;

    public Transform Player;
    public float detectionRange = 50f;
    public float attackDistance = 3f;
    public float attackinterval = 2f;

    NavMeshAgent Agent;
   public Animator anim;
    bool isDead = false;
    bool isAttacking = false;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if(Player== null)
        {
            Player= GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update()
    {
        if (isDead) return;
        float Distance = Vector3.Distance(transform.position, Player.position);
       
        if(Distance<= detectionRange)
        {
            Agent.SetDestination(Player.position);
            anim.SetBool("isWalking", true);

            if(Distance<=attackDistance && isAttacking)
            {
                StartCoroutine( PlayAttackAnimation());
            }
        }
        else
        {
            Agent.ResetPath();
            anim.SetBool("isWalking", false);
            //lost player
        }
    }


    IEnumerator PlayAttackAnimation()
    {
        isAttacking = true;
        Agent.isStopped= true;
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(attackinterval);

        Agent.isStopped = false;
        isAttacking = false;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class ZombieAnimationController : MonoBehaviour
{
    private enum ZombieState
    {
        Idle,
        RunForward,
        Attack,
        Dead
    }
    private List<string> Parameters = new List<string>()
    {
        "IsAttacking",
        "IsRunning",
        "IsDead"
    };
    private ZombieState zombieState;
    private Animator zombieAnimator;
    private ZombieAI zombieAI;
    private ZombieManager zombieManager;
    private void Start()
    {
        zombieAnimator = GetComponent<Animator>();
        zombieAI = GetComponent<ZombieAI>();
        zombieManager = GetComponent<ZombieManager>();
    }
    private void Update()
    {
        if(!NetworkManager.Singleton.IsServer) return;
        
        if(zombieManager.health < 1)
        {
            UpdateZombieState(ZombieState.Dead);
            return;
        }
        else if (zombieAI.attackTo != null && !zombieAI.playerInAttackRange)
        {
            UpdateZombieState(ZombieState.RunForward);
            return;
        }
        else if (zombieAI.attackTo != null && zombieAI.playerInAttackRange)
        {
            UpdateZombieState(ZombieState.Attack);
            return;
        }
        
        UpdateZombieState(ZombieState.Idle);
    }
    private void UpdateZombieState(ZombieState state)
    {
        zombieState = state;
        ZombieVisualState(zombieState);
    }
    private void ZombieVisualState(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Idle:
                SetBoolParameters();
                break;
            case ZombieState.RunForward:
                SetBoolParameters("IsRunning");
                break;
            case ZombieState.Attack:
                SetBoolParameters("IsAttacking");
                break;
            case ZombieState.Dead:
                SetBoolParameters("IsDead");
                break;
        }
    }
    private void SetBoolParameters(string ParameterName = "")
    {
        foreach(string i in Parameters)
        {
            if(ParameterName == i)
            {
                zombieAnimator.SetBool(i, true);
            }
            else
            {
                zombieAnimator.SetBool(i, false);
            }
        }
    }
}
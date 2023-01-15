using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
public class ZombieManager : ServerRPC
{
    public int health = 100;
    private AudioSource zombieScream;
    private bool functionCalled;
    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public Collider agentCollider;
    [HideInInspector]
    public ZombieAI zombieAI;
    [HideInInspector]
    public ZombieSpawner zombieSpawner;
    private NetworkObject zombieNetworkObject;
    [HideInInspector]
    public GameObject zombieMinimapIcon;
    public GameObject AK47AmmoPack, PistolAmmoPack;
    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        agentCollider = GetComponent<Collider>();
        zombieAI = GetComponent<ZombieAI>();
        zombieSpawner = GameObject.Find("SpawnManager").GetComponent<ZombieSpawner>();
        zombieNetworkObject = GetComponent<NetworkObject>();
        zombieMinimapIcon = transform.GetChild(4).gameObject;
    }
    private void Update() {
        if(health<1 && (IsServer || IsHost) && !functionCalled)
        {
            functionCalled = true;
            ZombieDeadServerRpc(this);
        }
    }
}
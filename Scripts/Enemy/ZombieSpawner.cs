using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class ZombieSpawner : ServerRPC
{   
    public GameObject Zombie;
    private int maxZombies = 100;
    public int zombieCount;
    [SerializeField]
    private float spawnRate;
    public override void OnNetworkSpawn()
    {
        if((IsServer || IsHost) && IsOwner)
        {
            InvokeRepeating("SpawnZombie",5f,spawnRate);
            base.OnNetworkSpawn();
        }
    }
    private void SpawnZombie()
    {
        if(zombieCount < maxZombies)
        {
            zombieCount++;
            SpawnZombieServerRpc(this);
        }
    }
}
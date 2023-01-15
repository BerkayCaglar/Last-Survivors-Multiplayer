using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class SpawnPosition : ServerRPC
{
    public override void OnNetworkSpawn()
    {
        SpawnPositionServerRpc(GetComponent<NetworkObject>());
    }
}
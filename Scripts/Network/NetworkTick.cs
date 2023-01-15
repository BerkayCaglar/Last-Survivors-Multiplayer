using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class NetworkTick : NetworkBehaviour
{
    /*
    public override void OnNetworkSpawn()
    {
        NetworkManager.NetworkTickSystem.Tick += Tick;
    }

    private void Tick()
    {
        Debug.Log($"Tick: {NetworkManager.LocalTime.Tick}");
    }

    public override void OnNetworkDespawn() // don't forget to unsubscribe
    {
        NetworkManager.NetworkTickSystem.Tick -= Tick;
    }
    */
}
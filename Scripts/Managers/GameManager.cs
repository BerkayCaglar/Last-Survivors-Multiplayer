using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        UIManager.Instance.ShowEquipmentCanvas(true);
        UIManager.Instance.ShowHealthEnvironment(true);
        UIManager.Instance.ShowMiniMap(true);
    }
}
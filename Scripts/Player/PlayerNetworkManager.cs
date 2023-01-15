using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetworkManager : NetworkBehaviour
{
    [SerializeField]
    private SpriteRenderer playerMinimapIcon;
    private class Colors
    {
        public Color Blue = new Color32(0, 173, 255, 255),
        Yellow = new Color32(255, 255, 0, 255),
        Green = new Color32(0, 255, 127, 255);
    }
    private Colors colors = new Colors();
    private void Start() 
    {
        if(!IsOwner) return;
        MinimapIconColorServerRpc();
    }
    [ServerRpc]
    private void MinimapIconColorServerRpc()
    {
        if(NetworkManager.ConnectedClients.Count == 1)
        {
            MinimapIconColorClientRpc(colors.Blue);
        }
        else if(NetworkManager.ConnectedClients.Count == 2)
        {
            MinimapIconColorClientRpc(colors.Yellow);
        }
        else if(NetworkManager.ConnectedClients.Count == 3)
        {
            MinimapIconColorClientRpc(colors.Green);
        }
    }
    [ClientRpc]
    private void MinimapIconColorClientRpc(Color color)
    {
        playerMinimapIcon.color = color;
    }
}
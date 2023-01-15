using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core.Environments;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
public class RelayConnectionManager : MonoBehaviour
{
    public static RelayConnectionManager Instance { get; private set; }
    [HideInInspector]
    public string JoinCode;
    private const int maxConnections = 3;
    public async Task<(string host, ushort port, string joinCode, byte[] allocationIDBytes, byte[] connectionData, byte[] Key)> GetJoinCode(int maxConnections)
    {
        InitializationOptions options = new InitializationOptions().SetEnvironmentName("production");
        await UnityServices.InitializeAsync(options);

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation;

        string joinCode;

        allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

        this.JoinCode = joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        StartCoroutine(UIManager.Instance.ShowJoinCode(joinCode));

        var dtls = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");

        return (dtls.Host,(ushort)dtls.Port,joinCode,allocation.AllocationIdBytes,allocation.ConnectionData,allocation.Key);
    }
    public IEnumerator StartGame()
    {
        var RelayServerUtiliy = GetJoinCode(maxConnections);

        while(!RelayServerUtiliy.IsCompleted)
        {
            yield return null;
        }

        if(RelayServerUtiliy.IsFaulted)
        {
            Debug.LogError($"{RelayServerUtiliy.Exception.Message}");
        }

        var(host, port, joinCode, allocationIDBytes, connectionData, Key) = RelayServerUtiliy.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(host,port,allocationIDBytes,Key,connectionData,true);
        
        NetworkManager.Singleton.StartHost();
        
        yield return null;
    }

    public async Task<(string host,ushort port,byte[] allocationIDBytes,byte[] connectionData,byte[] hostConnectionData,byte[] Key)> JoinWithCodeTask(string JoinCode)
    {
        InitializationOptions options = new InitializationOptions().SetEnvironmentName("production");
        await UnityServices.InitializeAsync(options);

        if(!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        JoinAllocation joinAllocation;

        joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCode.Trim());

        var dtls = joinAllocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
    
        return(dtls.Host,(ushort)dtls.Port,joinAllocation.AllocationIdBytes,joinAllocation.ConnectionData,joinAllocation.HostConnectionData,joinAllocation.Key);
    }

    public IEnumerator JoinAGame(string JoinCode)
    {
        var RelayServerUtiliy = JoinWithCodeTask(JoinCode.Trim());

        while(!RelayServerUtiliy.IsCompleted)
        {
            yield return null;
        }
        if(RelayServerUtiliy.IsFaulted)
        {
            Debug.LogError(RelayServerUtiliy.Exception.Message);
            yield break;
        }
        var(host,port,allocationIDBytes,connectionData,hostConnectionData,Key) = RelayServerUtiliy.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(host,port,allocationIDBytes,Key,connectionData,hostConnectionData,true);

        NetworkManager.Singleton.StartClient();
        yield return null;
    }
    private void Awake() {
        if (Instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        else if (Instance==null) 
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
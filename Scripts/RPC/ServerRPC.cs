using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// ServerRpc oyuncular ve server tarafından çağırılabilir ancak sadece server içerisinde işlenir. Yapılan işlemler oyuncu için değil sunucu için yapılır.

// ClientRpc sadece sunucu tarafından çağırılır ve bütün oyuncularda işlemleri yapar. Ancak bir oyuncu bu fonksiyonu çağırmak isterse çalışmaz. Sadece sunucu çalıştırabilir. 
// Spesifik bir kullanıcı üzerinde işlem yapılması gerekiyorsa ClientRpcParams ile yapılır.

[DisallowMultipleComponent]
public class ServerRPC : ClientRPC
{
    /* -------------------------------------------------------------------------- */
    /*                               ATTACK MANAGER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="Script"></param>
    /// <param name="weaponName"></param>
    [ServerRpc]
    public void PlayMuzzleEffectAndSoundServerRpc(NetworkBehaviourReference Script,string weaponName)
    {
        PlayMuzzleEffectAndSoundClientRpc(Script,weaponName);
    }
    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="Script"></param>
    /// <param name="weaponName"></param>
    [ServerRpc]
    public void SpawnAmmoServerRpc(NetworkBehaviourReference Script,string weaponName)
    {
        SpawnAmmoClientRpc(Script,weaponName);
    }
    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="Script"></param>
    /// <param name="weaponName"></param>
    [ServerRpc]
    public void UpdateWeaponStateServerRpc(NetworkBehaviourReference Script,string weaponName)
    {
        UpdateWeaponStateClientRpc(Script,weaponName);
    }
    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="Script"></param>
    /// <param name="weaponName"></param>
    [ServerRpc]
    public void PlayReloadSoundServerRpc(NetworkBehaviourReference Script,string weaponName)
    {
        PlayReloadSoundClientRpc(Script,weaponName);
    }

    /* -------------------------------------------------------------------------- */
    /*                               PLAYER MANAGER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="Script"></param>
    /// <param name="spawn"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ReSpawnOrDespawnGameObjectServerRpc(NetworkBehaviourReference Script,bool spawn)
    {
        ReSpawnOrDespawnGameObjectClientRpc(Script,spawn);
    }

    /* -------------------------------------------------------------------------- */
    /*                               SPAWN POSITION                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// ClientRpc çağırılır.
    /// </summary>
    /// <param name="NetworkObjectReference"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SpawnPositionServerRpc(NetworkObjectReference NetworkObjectReference)
    {
        SpawnPositionClientRpc(NetworkObjectReference);
    }

    /* -------------------------------------------------------------------------- */
    /*                               ZOMBIE SPAWNER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Zombilerin spawn olacağı konum server tarafından güncellenir.
    /// </summary>
    /// <param name="Script"></param>
    [ServerRpc]
    public void SpawnZombieServerRpc(NetworkBehaviourReference Script)
    {
        if(Script.TryGet(out ZombieSpawner ZombieSpawner))
        {
            GameObject Zombie = Instantiate(ZombieSpawner.Zombie, new Vector3(Random.Range(-100f, 100f), -2f, Random.Range(-100f, 100f)), Quaternion.identity);
            Zombie.GetComponent<NetworkObject>().Spawn();
        }
    }

    /* -------------------------------------------------------------------------- */
    /*                               ZOMBIE MANAGER                               */
    /* -------------------------------------------------------------------------- */
    /// <summary>
    /// Zombi öldüğü zaman çağırılır. Tüm clientlarda zombi öldürülür. 30 saniye sonra zombi yok edilir.
    /// </summary>
    /// <param name="Script"></param>
    [ServerRpc]
    public void ZombieDeadServerRpc(NetworkBehaviourReference Script)
    {
        int ammoPackSpawnChance = Random.Range(0, 100);
        int ammoPackTypeChance = Random.Range(0, 100);
        ZombieDeadClientRpc(Script,ammoPackSpawnChance,ammoPackTypeChance,Random.insideUnitSphere);
        if(Script.TryGet(out ZombieManager ZombieManager))
        {
            ZombieManager.StartCoroutine(DespawnZombie(ZombieManager.GetComponent<NetworkObject>()));
        }
    }
    /// <summary>
    /// Zombi 30 saniye sonra yok edilir.
    /// </summary>
    /// <param name="zombieNetworkObject"></param>
    /// <returns></returns>
    private IEnumerator DespawnZombie(NetworkObject zombieNetworkObject)
    {
        yield return new WaitForSeconds(30f);
        zombieNetworkObject.Despawn(true);
    }
}
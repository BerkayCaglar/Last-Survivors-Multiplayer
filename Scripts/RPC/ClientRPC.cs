using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// ServerRpc oyuncular ve server tarafından çağırılabilir ancak sadece server içerisinde işlenir. Yapılan işlemler oyuncu için değil sunucu için yapılır.

// ClientRpc sadece sunucu tarafından çağırılır ve bütün oyuncularda işlemleri yapar. Ancak bir oyuncu bu fonksiyonu çağırmak isterse çalışmaz. Sadece sunucu çalıştırabilir. 
// Spesifik bir kullanıcı üzerinde işlem yapılması gerekiyorsa ClientRpcParams ile yapılır.

[DisallowMultipleComponent]
public class ClientRPC : NetworkBehaviour
{
    /* -------------------------------------------------------------------------- */
    /*                               ATTACK MANAGER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Silahtan çıkan efekt ve sesi oynatır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    /// <param name="weaponName"> İşlemler elindeki silaha göre yapılacak. </param>
    [ClientRpc]
    public void PlayMuzzleEffectAndSoundClientRpc(NetworkBehaviourReference Script,string weaponName)
    {
        if(Script.TryGet(out AttackManager AttackManager))
        {
            if(weaponName == "AK47")
            {
                AttackManager.AK47FireSound.Play();
                AttackManager.AK47MuzzelEffect.Play(true);
            }
            else if(weaponName == "Pistol")
            {
                AttackManager.PistolFireSound.Play();
                AttackManager.PistolMuzzelEffect.Play(true);
            }
        }
        if(!IsOwner)
        {
            StartCoroutine(CameraShake.Instance.CameraShaker(4));
            return;
        }
        StartCoroutine(CameraShake.Instance.CameraShaker());
    }
    /// <summary>
    /// Oyuncu silahını reload ederken çıkan sesi oynatır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    /// <param name="weaponName"> İşlemler elindeki silaha göre yapılacak. </param>
    [ClientRpc]
    public void PlayReloadSoundClientRpc(NetworkBehaviourReference Script,string weaponName)
    {
        if(Script.TryGet(out AttackManager AttackManager))
        {
            if(weaponName == "AK47")
            {
                AttackManager.AK47ReloadSound.Play();
            }
            else if(weaponName == "Pistol")
            {
                AttackManager.PistolReloadSound.Play();
            }
        }
    }
    /// <summary>
    /// Her client üzerinde bir mermi yaratır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    /// <param name="weaponName"> İşlemler elindeki silaha göre yapılacak. </param>
    [ClientRpc]
    public void SpawnAmmoClientRpc(NetworkBehaviourReference Script,string weaponName)
    {
        if(Script.TryGet(out AttackManager AttackManager))
        {
            if(weaponName == "AK47")
            {
                Instantiate(AttackManager.AK47Ammo, AttackManager.AK47RifleBarrel.transform.position, AttackManager.AK47RifleBarrel.transform.rotation);
                GameObject BulletCase = Instantiate(AttackManager.AK47BulletCase, AttackManager.AK47BoltCarrier.transform.position, AttackManager.AK47BoltCarrier.transform.rotation);
                BulletCase.GetComponent<Rigidbody>().AddForce(AttackManager.AK47BoltCarrier.transform.right * 400f);
            }
            else if(weaponName == "Pistol")
            {
                Instantiate(AttackManager.PistolAmmo, AttackManager.PistolBarrel.transform.position, AttackManager.PistolBarrel.transform.rotation);
                GameObject BulletCase = Instantiate(AttackManager.PistolBulletCase, AttackManager.PistolBoltCarrier.transform.position, AttackManager.PistolBoltCarrier.transform.rotation);
                BulletCase.GetComponent<Rigidbody>().AddForce(AttackManager.PistolBoltCarrier.transform.right * 400f);
            }
        }
    }
    /// <summary>
    /// Oyuncu elindeki silahı değiştirdiğinde çalışır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    /// <param name="weaponName"> İşlemler elindeki silaha göre yapılacak. </param>
    [ClientRpc]
    public void UpdateWeaponStateClientRpc(NetworkBehaviourReference Script,string weaponName)
    {
        Debug.Log("UpdateWeaponStateClientRpc");
        if(Script.TryGet(out AttackManager AttackManager))
        {
            if(weaponName == "AK47")
            {
                AttackManager.AK47.SetActive(true);
                AttackManager.Pistol.SetActive(false);
                AttackManager.UpdateWeaponStateInAnimationController();
                AttackManager.UpdateWeaponStateInUIManager();
            }
            else if(weaponName == "Pistol")
            {
                AttackManager.AK47.SetActive(false);
                AttackManager.Pistol.SetActive(true);
                AttackManager.UpdateWeaponStateInAnimationController();
                AttackManager.UpdateWeaponStateInUIManager();
            }
        }
    }
    /* ------------------------------------------------------------------------- */
    /*                               PLAYER MANAGER                              */
    /* ------------------------------------------------------------------------- */

    /// <summary>
    /// Oyuncu öldüğünde çalışır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    /// <param name="spawn"> Canlansın mı canlanmasın mı? </param>
    [ClientRpc]
    public void ReSpawnOrDespawnGameObjectClientRpc(NetworkBehaviourReference Script,bool spawn)
    {
        if(Script.TryGet(out PlayerManager PlayerManager))
        {
            if (spawn)
            {
                //PlayerManager.gameObject.transform.position = new Vector3(Random.Range(-100f, 100f), 0.5f, Random.Range(-100f, 100f));
                PlayerManager.gameObject.transform.rotation = Quaternion.identity;
                PlayerManager.gameObject.SetActive(spawn);
                PlayerManager.animationController.ChangeWeaponStateOnDeath(PlayerManager.attackManager.ReturnCurrentWeapon());
            }
            else
            {
                PlayerManager.gameObject.SetActive(spawn);
            }
        }
    }
    
    /* -------------------------------------------------------------------------- */
    /*                                SPAWN MANAGER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Oyuna giriş yapan oyuncuların spawn noktasını değiştirir.
    /// </summary>
    /// <param name="NetworkObjectReference"> Pozisyonu değiştirilecek olan Player Network Objesi </param>
    [ClientRpc]
    public void SpawnPositionClientRpc(NetworkObjectReference NetworkObjectReference)
    {
        if(NetworkObjectReference.TryGet(out NetworkObject NetworkObject))
        {
            float x = NetworkObject.gameObject.transform.position.x;
            float z = NetworkObject.gameObject.transform.position.z;
            NetworkObject.gameObject.transform.position = new Vector3(x,0.5f,z);
        }
    }

    /* -------------------------------------------------------------------------- */
    /*                               ZOMBIE MANAGER                               */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Zombi öldüğünde çalışır.
    /// </summary>
    /// <param name="Script"> Script parametreleri almak için gerekli. </param>
    [ClientRpc]
    public void ZombieDeadClientRpc(NetworkBehaviourReference Script,int ammoPackSpawnChance,int ammoPackTypeChance,Vector3 angularVelocity)
    {
        if(Script.TryGet(out ZombieManager ZombieManager))
        {
            ZombieManager.agent.enabled = false;
            ZombieManager.agentCollider.enabled = false;
            ZombieManager.zombieSpawner.zombieCount -= 1;
            ZombieManager.zombieAI.enabled = false;
            ZombieManager.zombieMinimapIcon.SetActive(false);
            SpawnAmmoPackWithARandomChance(ZombieManager.AK47AmmoPack,ZombieManager.PistolAmmoPack,ZombieManager.transform.position,ammoPackSpawnChance,ammoPackTypeChance,angularVelocity);
        }
    }
    /// <summary>
    /// Rastgele bir şans ile ammo pack spawn eder. @ZombieDeadClientRpc tarafından çağırılır.
    /// </summary>
    /// <param name="AK47AmmoPack"> AK47 mermi kutusu. </param>
    /// <param name="PistolAmmoPack"> Pistol mermi kutusu. </param>
    /// <param name="zombiePosition"> Zombinin pozisyonu. </param>
    /// <param name="ammoPackSpawnChance"> Mermi kutusunun spawn olma şansı. </param>
    /// <param name="ammoPackTypeChance"> Mermi kutusunun tip şansı.(AK47Pack or Pistol) </param>
    /// <param name="angularVelocity"> Random.insideUnitSphere </param>
    private void SpawnAmmoPackWithARandomChance(GameObject AK47AmmoPack,GameObject PistolAmmoPack,Vector3 zombiePosition, int ammoPackSpawnChance, int ammoPackTypeChance,Vector3 angularVelocity)
    {
        // 30% şans ile ammo pack spawn olur.
        if(ammoPackSpawnChance <= 30)
        {
            // 50% şans ile pistol ammo pack spawn olur.
            if(ammoPackTypeChance <= 50)
            {
                GameObject _PistolAmmoPack = Instantiate(PistolAmmoPack,zombiePosition,Quaternion.identity);
                Rigidbody rb = _PistolAmmoPack.GetComponent<Rigidbody>();
                rb.angularVelocity = angularVelocity * 30f;
                rb.AddForce(angularVelocity + Vector3.up * 6f,ForceMode.Impulse);
                return;
            }
            GameObject _AK47AmmoPack = Instantiate(AK47AmmoPack,zombiePosition,Quaternion.identity);
            Rigidbody rb2 = _AK47AmmoPack.GetComponent<Rigidbody>();
            rb2.angularVelocity = angularVelocity * 30f;
            rb2.AddForce(angularVelocity + Vector3.up * 6f,ForceMode.Impulse);
        }
    }
}
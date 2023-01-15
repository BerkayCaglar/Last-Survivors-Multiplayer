using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class AmmoPackManager : MonoBehaviour
{
    private enum AmmoType
    {
        Pistol = 12,
        AK47 = 30
    }
    private AmmoType ammoType;
    private void Start() 
    {
        if(gameObject.CompareTag("Pistol Ammo Pack"))
        {
            ammoType = AmmoType.Pistol;
        }
        else if(gameObject.CompareTag("Rifle Ammo Pack"))
        {
            ammoType = AmmoType.AK47;
        }
    }
    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            AttackManager attackManager = other.GetComponent<AttackManager>();
            if(attackManager != null)
            {
                attackManager.AddAmmo(ammoType.ToString(),ammoType.GetHashCode());
                Destroy(gameObject);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    public ParticleSystem bloodEffect;
    [SerializeField]
    LayerMask LookingFor;
    [SerializeField]
    private float sphereRadius = 2.5f,bulletSpeed;
    private bool check = true;
    [SerializeField]
    private GameObject bulletMesh, Trail;
    private enum BulletDamage
    {
        Pistol = 20,
        Shotgun = 10,
        Rifle = 40,
        Sniper = 50
    }
    BulletDamage bulletDamage;
    
    private void Start() 
    {
        Destroy(gameObject, 3f);
    }
    
    void Update()
    {
        if(check)
        {
            transform.Translate(new Vector3(0f,0f,bulletSpeed));
        }
        
        foreach (Collider i in Physics.OverlapSphere(transform.position, sphereRadius, LookingFor))
        {
            if(check)
            {
                check = false;
                HitTheObject(i.gameObject);
            }
        }
    }
    private void HitTheObject(GameObject hittenObject)
    {
        bloodEffect.Play();
        bulletMesh.SetActive(false);
        Trail.SetActive(false);
        Destroy(gameObject,0.1f);
        if(hittenObject.TryGetComponent(out ZombieManager ZombieManager))
        {
            if(name == "Pistol Bullet(Clone)")
            {
                bulletDamage = BulletDamage.Pistol;
            }
            else if(name == "Rifle Bullet(Clone)")
            {
                bulletDamage = BulletDamage.Rifle;
            }
            ZombieManager.health -= bulletDamage.GetHashCode();
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
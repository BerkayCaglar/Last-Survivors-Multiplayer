using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
public class AttackManager : ServerRPC
{
    public ParticleSystem AK47MuzzelEffect,PistolMuzzelEffect;
    public AudioSource AK47FireSound,PistolFireSound,AK47ReloadSound,PistolReloadSound;

    //AK47
    public GameObject AK47;
    public GameObject AK47Ammo,AK47RifleBarrel,AK47BulletCase,AK47BoltCarrier;

    //Pistol
    public GameObject Pistol;
    public GameObject PistolAmmo,PistolBarrel,PistolBulletCase,PistolBoltCarrier;

    [SerializeField]
    private float currentTime,nextTimeToFire;
    private bool reqReload = false,isReloading  = false;
    private AnimationController animationController;
    private PlayerManager playerManager;
    private enum CurrentWeapon
    {
        AK47,
        Pistol
    }
    private class AK47Properties
    {
        public int maxCarryInMagazine = 30;
        public int roundsInTheMagazine = 30;
        public int CarryingAmmoCount = 120;
        public float fireRate = 10f;
    }
    private enum AK47FireTypes
    {
        SemiAuto,
        FullAuto
    }
    private class PistolProperties
    {
        public int maxCarryInMagazine = 12;
        public int roundsInTheMagazine = 12;
        public int CarryingAmmoCount = 48;
    }
    private AK47Properties AK47properties = new AK47Properties();
    private PistolProperties pistolProperties = new PistolProperties();
    private AK47FireTypes fireType = AK47FireTypes.FullAuto;
    private CurrentWeapon currentWeapon = CurrentWeapon.Pistol;
    private void Start() {
        animationController = GetComponent<AnimationController>();
        playerManager = GetComponent<PlayerManager>();
        UpdateBulletCountInUIManager();
        UpdateCarryingAmmoCountInUIManager();
    }
    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if(playerManager.respawnInProgress || !IsLocalPlayer)
        {
            return;
        }

        if(currentWeapon == CurrentWeapon.AK47 && !isReloading)
        {
            AK47Fire();
        }
        else if(currentWeapon == CurrentWeapon.Pistol && !isReloading)
        {
            PistolFire();
        }
        if(AK47properties.roundsInTheMagazine <= 0 && currentWeapon == CurrentWeapon.AK47  && !reqReload && !isReloading && CheckIfPlayerHasAmmo())
        {
            reqReload = true;
            StartCoroutine(Reload());
        }
        if(pistolProperties.roundsInTheMagazine <= 0 && currentWeapon == CurrentWeapon.Pistol && !reqReload && !isReloading && CheckIfPlayerHasAmmo())
        {
            reqReload = true;
            StartCoroutine(Reload());
        }
        if(Input.GetKeyDown(KeyCode.Alpha1) && !isReloading && currentWeapon != CurrentWeapon.AK47)
        {
            currentWeapon = CurrentWeapon.AK47;
            UpdateWeaponStateServerRpc(this,currentWeapon.ToString());
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2) && !isReloading && currentWeapon != CurrentWeapon.Pistol)
        {
            currentWeapon = CurrentWeapon.Pistol;
            UpdateWeaponStateServerRpc(this,currentWeapon.ToString());
        }
    }
    /// <summary>
    /// Ak47 ile ateş ederken çalışır.
    /// </summary>
    private void AK47Fire()
    {
        if(Input.GetKey(KeyCode.Mouse0))
        {
            if(AK47properties.roundsInTheMagazine > 0)
            {
                if(fireType == AK47FireTypes.SemiAuto && !isReloading)
                {
                    if(Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        AK47properties.roundsInTheMagazine -= 1;
                        Shoot();
                    }
                }
                else if(fireType == AK47FireTypes.FullAuto)
                {
                    currentTime += Time.deltaTime;

                    nextTimeToFire = 1 / AK47properties.fireRate;
                    if(currentTime >= nextTimeToFire)
                    {
                        AK47properties.roundsInTheMagazine -= 1;
                        Shoot();
                        currentTime = 0;
                    }
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.B))
        {
            ChangeAK47FireType();
        }
        else if(Input.GetKeyDown(KeyCode.R) && AK47properties.roundsInTheMagazine != AK47properties.maxCarryInMagazine && !isReloading && CheckIfPlayerHasAmmo())
        {
            StartCoroutine(Reload());
        }
    }
    /// <summary>
    /// Tabanca ile ateş ederken çalışır.
    /// </summary>
    private void PistolFire()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0) && !isReloading)
        {
            if(pistolProperties.roundsInTheMagazine > 0)
            {
                pistolProperties.roundsInTheMagazine -= 1;
                Shoot();
            }
        }
        else if(Input.GetKeyDown(KeyCode.R) && pistolProperties.roundsInTheMagazine != pistolProperties.maxCarryInMagazine && !isReloading && CheckIfPlayerHasAmmo())
        {
            StartCoroutine(Reload());
        }
    }
    /// <summary>
    /// Oyuncu ateş etmek istediği zaman çalışır.
    /// </summary>
    private void Shoot()
    {
        SpawnAmmoServerRpc(this,currentWeapon.ToString());
        PlayMuzzleEffectAndSoundServerRpc(this,currentWeapon.ToString());
        UpdateBulletCountInUIManager();
    }
    /// <summary>
    /// Oyuncu silahının mermisini değiştirmek istediği zaman çalışır.
    /// </summary>
    /// <returns>2 saniye bekler</returns>
    private IEnumerator Reload()
    {
        isReloading = true;
        ShowReloadAlertInUIManager(false);
        ShowMagazineInUIManager(false);
        ShowReloadingIconInUIManager(true);
        PlayReloadSoundServerRpc(this,currentWeapon.ToString());
        yield return new WaitForSeconds(1.5f);

        ReloadCaller();
    }
    public void ReloadCaller()
    {
        reqReload = false;
        isReloading = false;
        UpdateRoundCountInTheCarringAmmoCount();
        UpdateBulletCountInUIManager();
        UpdateCarryingAmmoCountInUIManager();
        ShowReloadingIconInUIManager(false);
        ShowMagazineInUIManager(true);
    }
    /// <summary>
    /// Oyuncu AK47'nin atış modunu değiştirmek istediği zaman çalışır.
    /// </summary>
    private void ChangeAK47FireType()
    {
        if(fireType == AK47FireTypes.FullAuto)
        {
            ShowSemiAutoIconInUIManager(true);
            ShowFullAutoIconInUIManager(false);
            fireType = AK47FireTypes.SemiAuto;
        }
        else
        {
            ShowSemiAutoIconInUIManager(false);
            ShowFullAutoIconInUIManager(true);
            fireType = AK47FireTypes.FullAuto;
        }
    }
    /// <summary>
    /// Oyuncunun taşıdığı mermi sayısı ve reload yaptığında elde edeceği mermi sayısı silahına göre güncellenir. Harcadığı mermi sayısına göre eksilir.
    /// </summary>
    private void UpdateRoundCountInTheCarringAmmoCount()
    {
        if(currentWeapon == CurrentWeapon.AK47)
        {
            // Eğer taşıdığı mermi sayısı reload yaptığı zaman elde edeceği mermi sayısından fazlaysa
            if(AK47properties.CarryingAmmoCount >= AK47properties.maxCarryInMagazine - AK47properties.roundsInTheMagazine)
            {
                // Normal bir şekilde reload yapılır.
                AK47properties.CarryingAmmoCount -= AK47properties.maxCarryInMagazine - AK47properties.roundsInTheMagazine;
                AK47properties.roundsInTheMagazine = AK47properties.maxCarryInMagazine;
            }
            // Değilse
            else
            {
                // Taşıdığı mermi sayısı reload yaptığı zaman elde edeceği mermi sayısına eşitlenir.
                AK47properties.roundsInTheMagazine += AK47properties.CarryingAmmoCount;
                AK47properties.CarryingAmmoCount = 0;
            }
        }
        else if(currentWeapon == CurrentWeapon.Pistol)
        {
            // Eğer taşıdığı mermi sayısı reload yaptığı zaman elde edeceği mermi sayısından fazlaysa
            if(pistolProperties.CarryingAmmoCount >= pistolProperties.maxCarryInMagazine - pistolProperties.roundsInTheMagazine)
            {
                // Normal bir şekilde reload yapılır.
                pistolProperties.CarryingAmmoCount -= pistolProperties.maxCarryInMagazine - pistolProperties.roundsInTheMagazine;
                pistolProperties.roundsInTheMagazine = pistolProperties.maxCarryInMagazine;
            }
            // Değilse
            else
            {
                // Taşıdığı mermi sayısı reload yaptığı zaman elde edeceği mermi sayısına eşitlenir.
                pistolProperties.roundsInTheMagazine += pistolProperties.CarryingAmmoCount;
                pistolProperties.CarryingAmmoCount = 0;
            }
        }
    }
    
    /* -------------------------------------------------------------------------- */
    /*                                   RETURNS                                  */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Oyuncunun elidenki silahın adını döndürür.
    /// </summary>
    /// <returns></returns>
    public string ReturnCurrentWeapon()
    {
        if(IsOwner)
        {
            return currentWeapon.ToString();
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// Oyuncunun elindeki mermi sayısını silahına göre kontrol eder. O dan büyükse true döner. O dan küçükse false döner.
    /// </summary>
    /// <returns></returns>
    private bool CheckIfPlayerHasAmmo()
    {
        if(currentWeapon == CurrentWeapon.AK47)
        {
            if(AK47properties.CarryingAmmoCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if(currentWeapon == CurrentWeapon.Pistol)
        {
            if(pistolProperties.CarryingAmmoCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }
    /* -------------------------------------------------------------------------- */
    /*                                    SETS                                    */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// IsReloading değişkenini set eder.
    /// </summary>
    /// <param name="set"></param>
    public void SetIsReloading(bool set)
    {
        this.isReloading = set;
    }
    public void SetDefaultAmmoToAllGuns() // Bu fonksiyon kullanılmıyor.
    {
        AK47properties.roundsInTheMagazine = 30;
        pistolProperties.roundsInTheMagazine = 12;
    }
    /// <summary>
    /// @AmmoPackManager.cs tarafından çağırılır. Pack alındığı zaman çalışır.
    /// </summary>
    public void AddAmmo(string weaponName, int ammoCount)
    {
        if(!IsOwner) return;

        if(weaponName == "AK47")
        {
            AK47properties.CarryingAmmoCount += ammoCount;
        }
        else if(weaponName == "Pistol")
        {
            pistolProperties.CarryingAmmoCount += ammoCount;
        }
        UpdateCarryingAmmoCountInUIManager();
    }
    public void ResetPlayer()
    {
        if(!IsOwner) return;
        currentWeapon = CurrentWeapon.Pistol;
        UpdateWeaponStateServerRpc(this,currentWeapon.ToString());
    }
    /* -------------------------------------------------------------------------- */
    /*                                     UI                                     */
    /* -------------------------------------------------------------------------- */

    /// <summary>
    /// Oyuncu reload yaptığında UIManager'da bu sayı güncellenir.
    /// </summary>
    private void UpdateBulletCountInUIManager()
    {
        if(currentWeapon == CurrentWeapon.Pistol)
        {
            UIManager.Instance.UpdateBulletCount(pistolProperties.roundsInTheMagazine);
        }
        else if(currentWeapon == CurrentWeapon.AK47)
        {
            UIManager.Instance.UpdateBulletCount(AK47properties.roundsInTheMagazine);
        }
    }
    /// <summary>
    /// @UpdateWeaponStateInUIManager, @Reload ve @AddAmmo tarafından çağırılır. UIManager'da oyuncunun elinde bulunan (@CarryingAmmoCount) mermi miktarını gösterir.
    /// </summary>
    private void UpdateCarryingAmmoCountInUIManager()
    {
        if(currentWeapon == CurrentWeapon.Pistol)
        {
            UIManager.Instance.UpdateMagazineCount(pistolProperties.CarryingAmmoCount);
        }
        else if(currentWeapon == CurrentWeapon.AK47)
        {
            UIManager.Instance.UpdateMagazineCount(AK47properties.CarryingAmmoCount);
        }
    }
    /// <summary>
    /// @UpdateWeaponStateInUIManager tarafından çağırılır. UIManager'da silah iconunun değişimi yapılır.
    /// </summary>
    private void UpdateWeaponInUIManager()
    {
        if(currentWeapon == CurrentWeapon.Pistol)
        {
            UIManager.Instance.ShowAK47UI(false);
            UIManager.Instance.ShowPistolUI(true);
        }
        else if(currentWeapon == CurrentWeapon.AK47)
        {
            UIManager.Instance.ShowAK47UI(true);
            UIManager.Instance.ShowPistolUI(false);
        }
    }
    /// <summary>
    /// Oyuncu silah değiştirdiğinde çalışır. UIManager'da silah değişimi yapılır.
    /// </summary>
    public void UpdateWeaponStateInUIManager()
    {
        if(!IsOwner) return;
        UpdateCarryingAmmoCountInUIManager();
        UpdateBulletCountInUIManager();
        UpdateWeaponInUIManager();
    }
    /// <summary>
    /// Oyuncu silah değiştirdiğinde çalışır. AnimationController'da silah değişimi yapılır.
    /// </summary>
    public void UpdateWeaponStateInAnimationController()
    {
        animationController.ChangeWeaponStateInAnimator();
    }
    /// <summary>
    /// Şuanlık inaktif. Oyuncunun mermisi bittiğinde çalışır. UI'da reload alert gösterilir.
    /// </summary>
    /// <param name="show">Kapat veya Aç</param>
    private void ShowReloadAlertInUIManager(bool show)
    {
        UIManager.Instance.ShowReloadAlert(show);
    }
    /// <summary>
    /// Oyuncu reload yaparken çalışır. Oyun içi UI'da sağ altta bulunan şarjördeki mermi sayısı ve elinde bulunan mermi sayısı kısmı açılır veya kapatılır.
    /// </summary>
    /// <param name="show">Kapat veya Aç</param>
    private void ShowMagazineInUIManager(bool show)
    {
        UIManager.Instance.ShowMagazine(show);
    }
    /// <summary>
    /// Oyuncu reload yaparken çalışır. Oyun içi UI'da merminin yüklenmesi için beklenirken bulunan yükleniyor iconu açılır veya kapatılır. 
    /// </summary>
    /// <param name="show">Kapat veya Aç</param>
    private void ShowReloadingIconInUIManager(bool show)
    {
        UIManager.Instance.ShowReloadingIcon(show);
    }
    /// <summary>
    /// Oyuncu AK47 kullanırken çalışır. Oyun içi UI'da AK47'nin Semi-Auto ateş modunu gösterir.
    /// </summary>
    /// <param name="show">Kapat veya Aç</param>
    private void ShowSemiAutoIconInUIManager(bool show)
    {
        UIManager.Instance.ShowSemiAutoIcon(show);
    }
    /// <summary>
    /// Oyuncu AK47 kullanırken çalışır. Oyun içi UI'da AK47'nin Full-Auto ateş modunu gösterir.
    /// </summary>
    /// <param name="show">Kapat veya Aç</param>
    private void ShowFullAutoIconInUIManager(bool show)
    {
        UIManager.Instance.ShowFullAutoIcon(show);
    }
}
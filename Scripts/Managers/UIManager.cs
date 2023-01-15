using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
public class UIManager : NetworkBehaviour
{
    [SerializeField] private  TMP_InputField joinCodeEnterance;
    [SerializeField] TMP_Text joinCodeText,bulletCountText,magazineCountText;
    public static UIManager Instance;
    [SerializeField]
    private Canvas EquipmentCanvas,MenuCanvas;
    [SerializeField]
    private GameObject reloadMagazineAlertUI,magazineUI,reloadingIcon,semiAutoIcon,fullAutoIcon,youDiedUI,respawnTimerUI,healthEnvironmentUI,AK47UI,PistolUI,miniMapUI,staminaEnvironmentUI;
    [SerializeField]
    private Slider healthBar,staminaBar;
    private void Awake() {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public override void OnNetworkSpawn()
    {
        DeactiveConnectionCanvas();
    }
    public IEnumerator ShowJoinCode(string JoinCode)
    {
        yield return joinCodeText.text = $"Your Lobby Code Is : {JoinCode}";
    }
    public void StartAGame()
    {
        StartCoroutine(RelayConnectionManager.Instance.StartGame());
    }
    public void JoinWithInputCode()
    {
        if(joinCodeEnterance.text.Length != 6)
        {
            return;
        }
        StartCoroutine(RelayConnectionManager.Instance.JoinAGame(joinCodeEnterance.text.Trim().ToUpper()));
    }
    public void DeactiveConnectionCanvas()
    {
        MenuCanvas.gameObject.SetActive(false);
    }
    public void UpdateBulletCount(int count)
    {
        bulletCountText.text = count.ToString();
    }
    public void UpdateMagazineCount(int count)
    {
        magazineCountText.text = count.ToString();
    }
    public void ShowEquipmentCanvas(bool show)
    {
        EquipmentCanvas.gameObject.SetActive(show);
    }
    public void ShowReloadAlert(bool show)
    {
        reloadMagazineAlertUI.SetActive(show);
    }
    public void ShowMagazine(bool show)
    {
        magazineUI.SetActive(show);
    }
    public void ShowReloadingIcon(bool show)
    {
        reloadingIcon.SetActive(show);
    }
    public void ShowSemiAutoIcon(bool show)
    {
        semiAutoIcon.SetActive(show);
        fullAutoIcon.SetActive(!show);
    }
    public void ShowFullAutoIcon(bool show)
    {
        fullAutoIcon.SetActive(show);
        semiAutoIcon.SetActive(!show);
    }
    public bool FullAutoIsActive()
    {
        if(fullAutoIcon.activeSelf)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void ShowAK47UI(bool show)
    {
        if(AK47UI.activeSelf != show)
        {
            AK47UI.SetActive(show);
        }
    }
    public void ShowPistolUI(bool show)
    {
        if(PistolUI.activeSelf != show)
        {
            PistolUI.SetActive(show);
        }
    }
    public void ShowYouDiedUI(bool show)
    {
        youDiedUI.SetActive(show);
    }
    public void UpdateRespawnTimer(int time)
    {
        respawnTimerUI.GetComponent<TMP_Text>().text = time.ToString();
    }
    public void UpdateHealth(int health)
    {
        healthBar.value = health/100f;
    }
    public void ShowHealthEnvironment(bool show)
    {
        healthEnvironmentUI.gameObject.SetActive(show);
    }
    public void ShowMiniMap(bool show)
    {
        miniMapUI.gameObject.SetActive(show);
    }
    public void ShowStaminaEnvironment(bool show)
    {
        staminaEnvironmentUI.gameObject.SetActive(show);
    }
    public void UpdateStamina(float stamina)
    {
        staminaBar.value = stamina / 100f;
    }
    
}
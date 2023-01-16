using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class PlayerManager : ServerRPC
{
    [SerializeField] private NetworkMovementComponent playerMovement;
    public AnimationController animationController;
    public AttackManager attackManager;
    Vector3 move;
    Ray ray;
    public int health = 100;
    private float stamina = 100;
    private int respawnTime = 5;
    private float speed = 3f;
    public float _speed { get { return speed; } private set { speed = value; } }
    public bool respawnInProgress;
    private void Start()
    {
        animationController = GetComponent<AnimationController>();
        attackManager = GetComponent<AttackManager>();
    }
    private void Update()
    {
        if (health > 0 && !respawnInProgress)
        {
            if (IsOwner)
            {
                CameraFollow.Instance.FollowPlayer(gameObject);
                MinimapController.Instance.FollowPlayer(gameObject);
            }
            move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;

            Vector3 mousePosition = Input.mousePosition;
            ray = Camera.main.ScreenPointToRay(mousePosition);
            if (IsLocalPlayer && IsClient)
            {
                if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
                {
                    speed = 6f;
                    if (move != Vector3.zero)
                    {
                        UIManager.Instance.ShowStaminaEnvironment(true);
                        stamina -= 0.2f;
                        UIManager.Instance.UpdateStamina(stamina);
                    }
                    if (stamina <= 0)
                    {
                        speed = 3f;
                        animationController.ChangePlayerMovementRunOrWalkInAnimator();
                    }
                }
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    if (stamina > 0)
                    {
                        animationController.ChangePlayerMovementRunOrWalkInAnimator();
                    }
                }
                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    speed = 3f;
                    if (stamina > 0)
                    {
                        animationController.ChangePlayerMovementRunOrWalkInAnimator();
                    }
                }

                if (!Input.GetKey(KeyCode.LeftShift) && stamina < 100)
                {
                    UpdateStaminaBarForAWhile();
                }
                playerMovement.ProcessLocalPlayerMovement(move, ray, speed);
            }
            else
            {
                playerMovement.ProcessSimulatedMovement();
            }
        }
        else
        {
            if (IsOwner && !respawnInProgress)
            {
                respawnInProgress = true;
                PrepareToRespawn();
            }
        }
    }
    private void PrepareToRespawn()
    {
        UIManager.Instance.ShowYouDiedUI(true);
        UIManager.Instance.ShowEquipmentCanvas(false);
        UIManager.Instance.ShowHealthEnvironment(false);
        UIManager.Instance.ShowMiniMap(false);
        ReSpawnOrDespawnGameObjectServerRpc(this, false);
        InvokeRepeating("RespawnTimer", 0f, 1f);
    }
    private void RespawnTimer()
    {
        if (respawnTime > 0)
        {
            respawnTime--;
            UIManager.Instance.UpdateRespawnTimer(respawnTime);
        }
        else
        {
            CancelInvoke("RespawnTimer");
            UIManager.Instance.ShowYouDiedUI(false);
            UIManager.Instance.ShowEquipmentCanvas(true);
            UIManager.Instance.ShowHealthEnvironment(true);
            Respawn();
        }
    }
    private void Respawn()
    {
        health = 100;
        stamina = 100;
        respawnTime = 5;
        speed = 3f;
        UIManager.Instance.UpdateHealth(health);
        attackManager.ReloadCaller();
        UIManager.Instance.ShowMiniMap(true);
        UIManager.Instance.ShowStaminaEnvironment(false);
        UIManager.Instance.UpdateStamina(stamina);
        ReSpawnOrDespawnGameObjectServerRpc(this, true);

        respawnInProgress = false;
        //attackManager.SetDefaultAmmoToAllGuns();

        //transform.position = new Vector3(transform.position.x,0.5f,transform.position.z);

    }
    public void TakenDamage(int damage)
    {
        if (!IsOwner) return;
        health -= damage;
        UIManager.Instance.UpdateHealth(health);
    }
    private void UpdateStaminaBarForAWhile()
    {
        stamina += 0.1f;
        UIManager.Instance.UpdateStamina(stamina);

        if (stamina >= 100f)
        {
            UIManager.Instance.ShowStaminaEnvironment(false);
        }
    }
}

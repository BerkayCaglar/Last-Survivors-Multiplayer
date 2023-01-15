using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : ServerRPC
{
    private enum PlayerState
    {
        Idle,
        RunLeft,
        RunRight,
        RunForward,
        RunForwardLeft,
        RunForwardRight,
        RunBackward,
        RunBackwardLeft,
        RunBackwardRight,
        RunForwardPistol,
        RunLeftPistol,
        RunRightPistol,
        RunBackwardPistol
    }
    private List<string> Parameters = new List<string>()
        {
            "IsRunningForward",
            "IsRunningForwardLeft",
            "IsRunningForwardRight",
            "IsRunningRight",
            "IsRunningLeft",
            "IsRunningBackward",
            "IsRunningBackwardLeft",
            "IsRunningBackwardRight",
            "IsRunningForwardPistol",
            "IsRunningLeftPistol",
            "IsRunningRightPistol",
            "IsRunningBackwardPistol"
        };
    private PlayerState playerState;
    private Animator playerAnimator;
    private PlayerManager playerManager;
    private AttackManager attackManager;

    private Transform camTransform;
    private Vector3 camForward;
    private Vector3 Move;
    private Vector3 MoveInput;
    private float forwardAmount;
    private float turnAmount;
    private void Start() 
    {
        playerAnimator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
        attackManager = GetComponent<AttackManager>();

        camTransform = Camera.main.transform;
    }
    private void Update() 
    {
        if(!IsOwner) return;
        
        if(camTransform != null)
        {
            camForward = Vector3.Scale(camTransform.up, new Vector3(1, 0, 1)).normalized;
            Move = Input.GetAxis("Vertical") * camForward + Input.GetAxis("Horizontal") * camTransform.right;
        }
        else
        {
            camForward = Vector3.forward;
            Move = Input.GetAxis("Vertical") * camForward + Input.GetAxis("Horizontal") * Vector3.right;
        }
        if(Move.magnitude > 1f)
        {
            Move.Normalize();
        }
        MoveAnimator(Move);
    }
    private void MoveAnimator(Vector3 Move)
    {
        MoveInput = Move;
        ConvertMoveInput();
        UpdateAnimator();
    }
    private void ConvertMoveInput()
    {
        Vector3 localMove = transform.InverseTransformDirection(MoveInput);
        turnAmount = localMove.x;
        forwardAmount = localMove.z;
    }
    private void UpdateAnimator()
    {
        playerAnimator.SetFloat("Forward", forwardAmount, 0.1f, 0.1f);
        playerAnimator.SetFloat("Turn", turnAmount, 0.1f, 0.1f);
    }
    public void ChangeWeaponStateInAnimator()
    {
        if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Pistol Run")) == 1)
        {
            SetAllLayerWeight("Rifle Run");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Pistol Walk")) == 1)
        {
            SetAllLayerWeight("Rifle Walk");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Rifle Run")) == 1)
        {
            SetAllLayerWeight("Pistol Run");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Rifle Walk")) == 1)
        {
            SetAllLayerWeight("Pistol Walk");
            return;
        }
    }
    /// <summary>
    /// Oyuncunun koştuğunda veya koşmadığında animasyonunun değişmesini sağlar.
    /// </summary>
    public void ChangePlayerMovementRunOrWalkInAnimator()
    {
        if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Rifle Walk")) == 1 && !CheckIfOpenLayerWeight())
        {
            SetAllLayerWeight("Rifle Run");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Rifle Run")) == 1)
        {
            SetAllLayerWeight("Rifle Walk");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Pistol Walk")) == 1)
        {
            SetAllLayerWeight("Pistol Run");
            return;
        }
        else if(playerAnimator.GetLayerWeight(playerAnimator.GetLayerIndex("Pistol Run")) == 1)
        {
            SetAllLayerWeight("Pistol Walk");
            return;
        }
    }
    public void ChangeWeaponStateOnDeath(string WeaponName)
    {   
        if(WeaponName == "AK47")
        {
            SetAllLayerWeight("Rifle Walk");
            return;
        }
        SetAllLayerWeight("Pistol Walk");
    }
    private void SetBoolParameters(string ParameterName = "")
    {
        foreach(string i in Parameters)
        {
            if(ParameterName == i)
            {
                playerAnimator.SetBool(i, true);
            }
            else
            {
                playerAnimator.SetBool(i, false);
            }
        }
    }
    private void SetAllLayerWeight(string LayerName)
    {
        for(int i = 0; i < playerAnimator.layerCount; i++)
        {
            if(playerAnimator.GetLayerName(i) == LayerName)
            {
                playerAnimator.SetLayerWeight(i, 1);
                continue;
            }
            playerAnimator.SetLayerWeight(i, 0);
        }
    }
    private bool CheckIfOpenLayerWeight()
    {
        for(int i = 0; i < playerAnimator.layerCount; i++)
        {
            if(playerAnimator.GetLayerWeight(i) == 1 && playerAnimator.GetLayerName(i) != "Rifle Walk")
            {
                return true;
            }
        }
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using System;
using System.Linq;

public class NetworkMovementComponent : NetworkBehaviour
{
    [SerializeField] private CharacterController CharacterController;
    public LayerMask RaycastGround;

    private int Tick = 0;
    public float TickRate = 1f / 60f;
    private float TickDeltaTime = 0;

    private const int BUFFER_SIZE = 1024;
    private InputState[] InputStates = new InputState[BUFFER_SIZE];
    private TransformState[] TransformStates = new TransformState[BUFFER_SIZE];

    public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
    public TransformState PreviousTransformState;

    private void OnEnable()
    {
        ServerTransformState.OnValueChanged += OnServerStateChange;
    }
    private void OnServerStateChange(TransformState previousValue, TransformState serverState)
    {
        if (!IsLocalPlayer) return;
        if (PreviousTransformState == null)
        {
            PreviousTransformState = serverState;
        }
        TransformState calculatedState = TransformStates.First(localState => localState.Tick == serverState.Tick);
        if (calculatedState.Position != serverState.Position)
        {
            Debug.Log("Teleporting player to server position");
            // Teleport the player to the server position
            TeleportPlayer(serverState);
            // Replay the inputs that happened after the server state
        }
    }

    private void TeleportPlayer(TransformState state)
    {
        CharacterController.enabled = false;
        transform.position = state.Position;
        transform.rotation = state.Rotation;
        CharacterController.enabled = true;

        for (int i = 0; i < TransformStates.Length; i++)
        {
            if (TransformStates[i].Tick == state.Tick)
            {
                TransformStates[i] = state;
                break;
            }
        }
    }

    public void ProcessLocalPlayerMovement(Vector3 movementInput, Ray ray, float speed)
    {
        TickDeltaTime += Time.deltaTime;
        if (TickDeltaTime > TickRate)
        {
            int bufferIndex = Tick % BUFFER_SIZE;

            if (!IsServer)
            {
                MovePlayerServerRpc(Tick, movementInput, ray, speed);
                MovePlayer(movementInput, speed);
                LookAtMouse(ray);
            }
            else
            {
                MovePlayer(movementInput, speed);
                LookAtMouse(ray);

                TransformState state = new TransformState()
                {
                    Tick = this.Tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                PreviousTransformState = ServerTransformState.Value;
                ServerTransformState.Value = state;
            }

            InputState inputState = new InputState()
            {
                Tick = this.Tick,
                movementInput = movementInput,
                lookInput = ray
            };

            TransformState transformState = new TransformState()
            {
                Tick = this.Tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };

            InputStates[bufferIndex] = inputState;
            TransformStates[bufferIndex] = transformState;

            TickDeltaTime -= TickRate;
            Tick++;
        }
    }
    public void ProcessSimulatedMovement()
    {
        TickDeltaTime += Time.deltaTime;
        if (TickDeltaTime > TickRate)
        {
            if (ServerTransformState.Value.HasStartedMoving)
            {
                transform.position = ServerTransformState.Value.Position;
                transform.rotation = ServerTransformState.Value.Rotation;
            }

            TickDeltaTime -= TickRate;
            Tick++;
        }
    }
    private void MovePlayer(Vector3 movementInput, float speed)
    {
        CharacterController.Move(movementInput * TickRate * speed);
    }
    private void LookAtMouse(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, RaycastGround))
        {
            transform.LookAt(new Vector3(raycastHit.point.x, 0f, raycastHit.point.z));
        }
        Quaternion newRotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
        transform.rotation = newRotation;
    }

    [ServerRpc]
    public void MovePlayerServerRpc(int Tick, Vector3 movementInput, Ray ray, float speed)
    {
        MovePlayer(movementInput, speed);
        LookAtMouse(ray);

        TransformState state = new TransformState()
        {
            Tick = this.Tick,
            Position = transform.position,
            Rotation = transform.rotation,
            HasStartedMoving = true
        };

        PreviousTransformState = ServerTransformState.Value;
        ServerTransformState.Value = state;
    }
}
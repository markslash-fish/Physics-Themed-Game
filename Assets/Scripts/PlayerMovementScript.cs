using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovementScript : NetworkBehaviour, IDamageable
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpd = 10f;
    [SerializeField] private float jumpHeight = 5f;

    // Health is now a NetworkVariable so everyone sees the same HP bar
    public NetworkVariable<int> playerCurrentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("References")]
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;

    public int playerDamage = 10;
    public int playerDefense = 35;
    public int playerMaxHealth = 100;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;

    public CinemachineCamera vcam;

    public override void OnNetworkSpawn()
    {
        playerMaxHealth = 100;
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            playerInputReader.onLightAttackStarted += PlayerLightAttack;
            playerInputReader.onMove += PlayerMove;
            playerInputReader.jumpStarted += PlayerJump;

            vcam.Priority = 100;
        }
        else
        {
            vcam.Priority = 0;
        }

        // 2. Initialize health on the server
        if (IsServer)
        {
            playerCurrentHealth.Value = playerMaxHealth;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            playerInputReader.onLightAttackStarted -= PlayerLightAttack;
            playerInputReader.onMove -= PlayerMove;
            playerInputReader.jumpStarted -= PlayerJump;
        }
    }

    private void Update()
    {
        // 3. Only the owner calculates movement direction
        if (!IsOwner) return;

        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        CheckGrounded();
    }

    private void FixedUpdate()
    {
        // 4. Only the owner moves their own Rigidbody
        if (!IsOwner) return;

        Vector3 movePosition = rb.position + moveDirection * moveSpd * Time.fixedDeltaTime;
        rb.MovePosition(movePosition);
    }

    private void CheckGrounded()
    {
        // Simple ground check logic
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void PlayerMove(Vector2 input) => moveInput = input;

    void PlayerJump()
    {
        // Add logic here to ensure isGrounded is true
        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
    }

    void PlayerLightAttack()
    {
        Debug.Log("Attack triggered by Owner");
        // Here you would call a ServerRpc to tell the server to spawn a hitbox
    }

    // 5. Health logic MUST happen on the Server
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;

        int healthDamage = (damage * damage) / (damage + playerDefense);
        playerCurrentHealth.Value -= healthDamage;

        Debug.Log(healthDamage);

        if (playerCurrentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
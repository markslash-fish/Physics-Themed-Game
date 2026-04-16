using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 8f;

    bool isBlocking = false;
    bool isDodging = false;
    float dodgeTimer = 0f;

    [Header("Movement")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] float gravity = -20f;
    float verticalVelocity;
    [SerializeField] float combo3Force = 6f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    private float groundDistance = 0.3f;

    public Vector2 movement;
    public Vector3 move;


    public bool isGrounded;
    public bool isRunning;

    Animator anim;
    Rigidbody rb;

    // ======================
    // COMBAT
    // ======================
    int comboStep = 0;
    bool canCombo = false;
    bool canMove = true;

    float comboTimer = 0f;
    float comboResetTime = 1f;

    bool isHeavyAttacking = false;

    [Header("Stats")]
    public NetworkVariable<int> playerBaseCurrentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public int playerBaseMaxHealth;
    [SerializeField] public int playerBaseMinAP;
    [SerializeField] public int playerBaseMaxAP;
    [SerializeField] public int playerBaseDamage;
    [SerializeField] public int playerBaseDefense;
    [SerializeField] public int playerDamage;
    public CinemachineCamera playerCam;



    public override void OnNetworkSpawn()
    {
      
        playerInputReader.onBlockStarted += StartBlock;
        playerInputReader.onBlockFinished += StopBlock;
        playerInputReader.onDodgeStarted += Dodge;
        playerInputReader.onSprint += SetSprint;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;

        playerInputReader.onLightAttackStarted += Attack;
        playerInputReader.onHeavyAttackStarted += HeavyAttack;
    }

    public override void OnNetworkDespawn()
    {

        playerInputReader.onBlockStarted -= StartBlock;
        playerInputReader.onBlockFinished -= StopBlock;
        playerInputReader.onDodgeStarted -= Dodge;
        playerInputReader.onSprint -= SetSprint;
        playerInputReader.onMove -= PlayerMove;
        playerInputReader.jumpStarted -= PlayerJump;

        playerInputReader.onLightAttackStarted -= Attack;
        playerInputReader.onHeavyAttackStarted -= HeavyAttack;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerInputReader = GetComponent<PlayerInputReader>();
    }

    void Update()
    {

        if (IsOwner) {
            CheckGround();
            CalculateMovement();
            HandleMovementAnimation();
            HandleComboReset();

        }

    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
            if (!canMove) return;



        if (move != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            }

            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * currentSpeed;
            velocity.y = verticalVelocity;

            rb.MovePosition(rb.position + velocity * Time.deltaTime);
        
      
    }

    // ======================
    // MOVEMENT
    // ======================
    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void CalculateMovement()
    {
        float x = movement.x;
        float z = movement.y;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        move = forward * z + right * x;
    }

    void HandleMovement()
    {
        if (move != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;


    }

    void PlayerMove(Vector2 input)
    {
        movement = input;
    }

    void PlayerJump()
    {
        if (isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            anim.SetTrigger("jumpTrigger");
        }
    }

    void SetSprint(bool value)
    {
        isRunning = value;
    }

    void HandleMovementAnimation()
    {
        float speed = move.magnitude;
        bool isMoving = speed > 0.1f;

        anim.SetBool("isIdle", !isMoving);
        anim.SetBool("isRunning", isMoving);
    }
    public void PlayerInteract()
    {

    }

    // ======================
    // LIGHT ATTACK COMBO
    // ======================
    public void Attack()
    {
        DisableMove();
        if (isHeavyAttacking) return;

        comboTimer = 0f;

        if (comboStep == 0)
        {
            comboStep = 1;
            anim.SetInteger("LightAttack", 1);
        }
        else if (comboStep == 1 && canCombo)
        {
            comboStep = 2;
            canCombo = false;
            anim.SetInteger("LightAttack", 2);
        }
        else if (comboStep == 2 && canCombo)
        {
            comboStep = 3;
            canCombo = false;
            anim.SetInteger("LightAttack", 3);
        }
    }

    // ======================
    // HEAVY ATTACK
    // ======================
    public void HeavyAttack()
    {
        if (comboStep != 0) return;
        if (isHeavyAttacking) return;

        isHeavyAttacking = true;
        anim.SetTrigger("HeavyAttack");

    }

    // ======================
    // COMBO SYSTEM
    // ======================
    void HandleComboReset()
    {
        if (comboStep > 0)
        {
            comboTimer += Time.deltaTime;

            if (comboTimer >= comboResetTime)
            {
                ResetCombo();
            }
        }
    }

    public void EnableNextCombo()
    {
        canCombo = true;
    }

    public void ResetCombo()
    {
        comboStep = 0;
        canCombo = false;
        comboTimer = 0f;

        anim.SetInteger("LightAttack", 0);
    }

    public void EndHeavyAttack()
    {
        isHeavyAttacking = false;
    }

    public void DisableMove()
    {
        canMove = false;
    }

    public void EnableMove()
    {
        canMove = true;
    }
    public void Combo3Move()
    {
        Vector3 dash = transform.forward * combo3Force;

        rb.AddForce(dash, ForceMode.VelocityChange);
    }



    public void Dodge()
    {
        if (isDodging) return;
        if (!isGrounded) return;

        isDodging = true;
        DisableMove();

        anim.SetTrigger("Dodge");

        Vector3 dodgeDir = move;

        if (dodgeDir == Vector3.zero)
        {
            dodgeDir = transform.forward;
        }

        rb.AddForce(dodgeDir * dodgeForce, ForceMode.VelocityChange);

        float dodgeTime = 0.6f * 1;
        Invoke(nameof(EndDodge), dodgeTime);
    }

    void EndDodge()
    {
        isDodging = false;
        EnableMove();
    }
    void StartBlock()
    {
        if (isBlocking) return;

        isBlocking = true;
        anim.SetBool("Block", true);
    }

    void StopBlock()
    {
        isBlocking = false;
        anim.SetBool("Block", false);
    }

    public void TakeDamage(int damage)
    {
        int healthDamage = (damage * damage) / damage + playerBaseDefense;
        playerBaseCurrentHealth.Value -= healthDamage;
        if (playerBaseCurrentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
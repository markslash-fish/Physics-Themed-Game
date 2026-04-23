using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    public enum PlayerState { None, IsJumping, IsDodging, IsAttacking, IsBlocking, IsHealing }
    public PlayerState playerState;

  public bool isBusy => playerState != PlayerState.None;

    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 12f;

    public  bool isBlocking = false;
    public bool isDodging = false;
  

    [Header("Movement")]
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] float gravity = -20f;
    float verticalVelocity;
  

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
    public int comboStep = 0;
    public bool canCombo = false;
    public bool canMove = true;

    public float comboTimer = 0f;
    public float comboResetTime = 1f;

   public bool isHeavyAttacking = false;

    [Header("Stats")]
    public NetworkVariable<int> playerBaseCurrentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] public int playerBaseMaxHealth;
    public NetworkVariable<int> playerBaseCurrentStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public int playerBaseMaxStamina;
    [SerializeField] public int playerBaseMinAP;
    [SerializeField] public int playerBaseMaxAP;
    [SerializeField] public int playerBaseDamage;
    [SerializeField] public int playerBaseDefense;
    [SerializeField] public float playerBaseSense;
    [SerializeField] public int playerDamage;
    [SerializeField] public string skillTrigger;
    [SerializeField] public int potionCount;
    public CinemachineCamera playerThirdPersonCam;
    public CinemachineCamera playerLockOnCam;



    public override void OnNetworkSpawn()
    {
        potionCount = 3;
        if(IsOwner)
        {
            playerThirdPersonCam.Priority = 100;
            
        }
        else
        {
            playerThirdPersonCam.Priority = 0;
           
        }

        playerBaseMaxHealth = 100;
        playerBaseCurrentHealth.Value = playerBaseMaxHealth;
        playerBaseMaxStamina = 100;
        playerBaseCurrentStamina.Value = playerBaseMaxStamina;
        playerBaseMinAP = 10;
        playerBaseMaxAP = 12;
        playerBaseDefense = 9;
        playerBaseSense = 0.25f;

        playerInputReader.onBlockStarted += StartBlock;
        playerInputReader.onBlockFinished += StopBlock;
        playerInputReader.onDodgeStarted += Dodge;
        playerInputReader.onSprint += SetSprint;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;
        playerInputReader.onHeal += PlayerHeal;
        playerInputReader.onLightAttackStarted += PlayerLightAttack;
        playerInputReader.onHeavyAttackStarted += PlayerHeavyAttack;
    }

    public override void OnNetworkDespawn()
    {

        playerInputReader.onBlockStarted -= StartBlock;
        playerInputReader.onBlockFinished -= StopBlock;
        playerInputReader.onDodgeStarted -= Dodge;
        playerInputReader.onSprint -= SetSprint;
        playerInputReader.onMove -= PlayerMove;
        playerInputReader.jumpStarted -= PlayerJump;
        playerInputReader.onHeal -= PlayerHeal;

        playerInputReader.onLightAttackStarted -= PlayerLightAttack;
        playerInputReader.onHeavyAttackStarted -= PlayerHeavyAttack;
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
            HandleComboReset();

        }

        if(playerState == PlayerState.IsJumping)
        {
            playerInputReader.jumpAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
        }
        else if (playerState == PlayerState.IsAttacking)
        {
            playerInputReader.dodgeAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.moveAction.Disable();
            playerInputReader.jumpAction.Disable();
        }
        else if (playerState == PlayerState.IsDodging) 
        {
            playerInputReader.moveAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
        }
        else if (playerState == PlayerState.IsBlocking)
        {
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
        }
        else if (playerState == PlayerState.IsHealing)
        {
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();

        }
        else 
        {
            playerInputReader.jumpAction.Enable();
            playerInputReader.lAttackAction.Enable();
            playerInputReader.hAttackAction.Enable();
            playerInputReader.dodgeAction.Enable();
            playerInputReader.blockAction.Enable();
            playerInputReader.healAction.Enable();
            playerInputReader.moveAction.Enable();
        }


    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        
            if (!canMove && isBusy) return;

      

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
        if (isBusy) return;

       
        if (isGrounded)
            {
             playerState = PlayerState.IsJumping;
             verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            }
        
       
    }

    void SetSprint(bool value)
    {
        isRunning = value;
    }
    public void PlayerInteract()
    {

    }
    public void PlayerHeal()
    {
        if (isBusy && playerBaseCurrentHealth.Value == playerBaseMaxHealth || potionCount == 0) return;
        playerState = PlayerState.IsHealing;
    }
    

    // ======================
    // LIGHT ATTACK COMBO
    // ======================
    public void PlayerLightAttack()
    {
        if (isBusy && playerState != PlayerState.IsAttacking) return;
        if (isHeavyAttacking) return;

        playerState = PlayerState.IsAttacking;

        comboTimer = 0f;

        if (comboStep == 0)
        {
            comboStep = 1;
           
        }
        else if (comboStep == 1 && canCombo)
        {
            comboStep = 2;
            canCombo = false;
            
        }
        else if (comboStep == 2 && canCombo)
        {
            comboStep = 3;
            canCombo = false;
            
        }
    }

    // ======================
    // HEAVY ATTACK
    // ======================
    public void PlayerHeavyAttack()
    {
        if (isBusy) return;
        playerState = PlayerState.IsAttacking;
        if (comboStep != 0) return;
        if (isHeavyAttacking) return;

        isHeavyAttacking = true;
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
        playerState = PlayerState.None;
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
   



    public void Dodge()
    {
        if (isBusy) return;

        playerState = PlayerState.IsDodging;
        if (!isGrounded) return;

        anim.applyRootMotion = false;
        isDodging = true;
        Vector3 dodgeDir = move;

        if (dodgeDir == Vector3.zero)
        {
            dodgeDir = transform.forward;
        }

        rb.AddForce(dodgeDir * dodgeForce, ForceMode.Impulse);

       
       
    }

    void StartBlock()
    {
        if (isBusy) return;
        if (isBlocking) return;
        playerState = PlayerState.IsBlocking;
        isBlocking = true;
      
    }

    void StopBlock()
    {
        playerState = PlayerState.None;
        isBlocking = false;
    }

    public void TakeDamage(int damage)
    {
        int healthDamage = (damage * damage) / damage + playerBaseDefense;
        playerBaseCurrentHealth.Value -= healthDamage;
        
        anim.SetTrigger("Hit");
        if (playerBaseCurrentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
    public void PlayerCameraLockOn()
    {
       
      
    }
    void ResetState()
    {
        anim.applyRootMotion = true;
        playerState = PlayerState.None;
    }
}
using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("After Image")]
public GameObject ghostPrefab;
public float ghostSpawnDelay = 0.05f;
float ghostTimer = 0f;
    
    public enum PlayerState { None, IsJumping, IsDodging, IsAttacking, IsBlocking, IsHealing, IsHurt }
    public PlayerState playerState;

  public bool isBusy => playerState != PlayerState.None;

    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] PlayerCamLockOn camLock;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 12f;

    public  bool isBlocking = false;
    public bool isDodging = false;


    [Header("Movement")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float baseWalkSpeed;
    [SerializeField] private float currentWalkSpeed;
    [SerializeField] private float runSpeed;
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
    public NetworkVariable<int> playerBaseCurrentHealth = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] public int playerBaseMaxHealth;
    public NetworkVariable<int> playerBaseCurrentStamina = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public int playerBaseMaxStamina;
    [SerializeField] public int playerBaseMinAP;
    [SerializeField] public int playerBaseMaxAP;
    [SerializeField] public int playerBaseDamage;
    [SerializeField] public int playerBaseDefense;
    [SerializeField] public float playerBaseSense;
    [SerializeField] public int playerDamage;
    [SerializeField] public string skillTrigger;
    [SerializeField] public int potionCount;
    public CinemachineStateDrivenCamera stateCam;
   
  

    public event Action onHurt;


    public override void OnNetworkSpawn()
    {
        jumpHeight = 2.2f;
        baseWalkSpeed = 4f;
        currentWalkSpeed = 2f;
        runSpeed = 5.8f;
        potionCount = 3;
        if(IsOwner)
        {
            stateCam.Priority = 100;
            gameObject.tag = "Player";
        }
        else
        {
            stateCam.Priority = 0;
          

        }
        playerBaseCurrentHealth.OnValueChanged += (oldVal, newVal) => {
            if (newVal < oldVal)
            {
                onHurt?.Invoke();
            }
        };
        playerBaseMaxHealth = 200;
        playerBaseCurrentHealth.Value = playerBaseMaxHealth;
        playerBaseMaxStamina = 100;
        playerBaseCurrentStamina.Value = playerBaseMaxStamina;
        playerBaseMinAP = 10;
        playerBaseMaxAP = 12;
        playerBaseDefense = 50;
        playerBaseSense = 0.25f;

        playerInputReader.onBlockStarted += StartBlock;
        playerInputReader.onBlockFinished += StopBlock;
        playerInputReader.onDodgeStarted += Dodge;
        playerInputReader.onSprintStarted += SetSprint;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;
        playerInputReader.onSprintFinished += EndSprint;
        playerInputReader.onHeal += PlayerHeal;
        playerInputReader.onLightAttackStarted += PlayerLightAttack;
        playerInputReader.onHeavyAttackStarted += PlayerHeavyAttack;
        playerInputReader.onLockOn += PlayerCameraLockOn;
    }

    public override void OnNetworkDespawn()
    {

        playerInputReader.onBlockStarted -= StartBlock;
        playerInputReader.onBlockFinished -= StopBlock;
        playerInputReader.onDodgeStarted -= Dodge;
        playerInputReader.onSprintStarted -= SetSprint;
        playerInputReader.onSprintFinished -= EndSprint;
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

if (!IsOwner) return;

if (isDodging)
{
    ghostTimer += Time.deltaTime;

    if (ghostTimer >= ghostSpawnDelay)
    {
        SpawnGhostServerRpc(transform.position, transform.rotation);
        ghostTimer = 0f;
    }
}

        }

        if(playerState == PlayerState.IsJumping)
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
        }
        else if (playerState == PlayerState.IsAttacking)
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.moveAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.hAttackAction.Disable();
        }
        else if (playerState == PlayerState.IsDodging)
        {
            playerInputReader.sprintAction.Disable();
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
            playerInputReader.sprintAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
        }
        else if (playerState == PlayerState.IsHealing)
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();

        }
        else if (playerState == PlayerState.IsHurt)
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.moveAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();

        }
        else 
        {
            playerInputReader.sprintAction.Enable();
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

        if (playerState == PlayerState.IsHurt) return;

        if (!canMove && isBusy) return;

      

        if (move != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 10f);
            }

            float currentSpeed = isRunning ? runSpeed : baseWalkSpeed;

            if (isGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = isBlocking? move * currentWalkSpeed : move * currentSpeed;
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
        if (camLock.currentEnemy != null) // You'll need to expose these variables
        {
            // Direction from player to enemy
            forward = (camLock.currentEnemy.position - transform.position).normalized;
            right = Quaternion.Euler(0, 90, 0) * forward;
        }
        else
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
        }
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

        float currentSpeed = isRunning ? runSpeed : baseWalkSpeed;
        

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = isBlocking? move * currentSpeed : move * currentWalkSpeed;
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
        value = true;
        isRunning = value;
   
    }
    void EndSprint(bool value)
    {
        value = false;
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
        playerDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
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
        int baseDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
        playerDamage = baseDamage * 3;
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

    public void TakeDamage(int damage, Vector3 hitDir)
    {
        if (isDodging) return;
        int healthDamage = (damage * damage) / (damage + playerBaseDefense);
        float staminaDamage = (healthDamage / 4);
        float knockbackForce = (damage <20)? 8f: 12f;
        playerBaseCurrentHealth.Value -= healthDamage;
        playerState = Player.PlayerState.IsHurt;

     
        ApplyKnockbackClientRpc(knockbackForce, hitDir);
        if (playerBaseCurrentHealth.Value <= 0)
        {
            GetComponent<NetworkObject>().Despawn();
        }
      
       
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ApplyKnockbackClientRpc(float force, Vector3 dir)
    {
       
        ApplyPlayerKnockback(force, dir);
    }


    public void ApplyPlayerKnockback(float knockbackForce, Vector3 hitDir)
    {
        playerState = PlayerState.IsHurt; // Ensure state is set for everyone
        anim.applyRootMotion = false;

        // CRITICAL: Clear current velocity so the knockback isn't fighting old movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply the force
        rb.AddForce(hitDir * knockbackForce, ForceMode.Impulse);

        Debug.Log("Knockback Applied");
    }
    public void PlayerCameraLockOn()
    {
        if (!IsOwner) return;

     
        if (camLock != null)
        {
            camLock.ToggleLockOn();
        }
    }
    void ResetState()
    {
        anim.applyRootMotion = true;
        playerState = PlayerState.None;
    }

// =========================
// MULTIPLAYER AFTER IMAGE
// =========================

[ServerRpc]
void SpawnGhostServerRpc(Vector3 pos, Quaternion rot)
{
    SpawnGhostClientRpc(pos, rot);
}

[ClientRpc]
void SpawnGhostClientRpc(Vector3 pos, Quaternion rot)
{
    GameObject ghost = Instantiate(ghostPrefab, pos, rot);

    Animator ghostAnim = ghost.GetComponent<Animator>();
    Animator myAnim = GetComponent<Animator>();

    if (ghostAnim != null && myAnim != null)
    {
        ghostAnim.Play(myAnim.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
        ghostAnim.speed = 0f;
    }
}
public void EndDodge()
{
    isDodging = false;
    ghostTimer = 0f;
}
}
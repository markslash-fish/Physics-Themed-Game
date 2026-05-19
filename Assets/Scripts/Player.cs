using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IDamageable
{
    [Header("After Image")]
    public GameObject ghostPrefab;
    public float ghostSpawnDelay = 0.05f;
    float ghostTimer = 0f;

    public enum PlayerState { None, IsJumping, IsDodging, IsAttacking, IsBlocking, IsHealing, IsHurt, IsExhausted, IsDead }
    public PlayerState playerState;

    public bool isBusy => playerState != PlayerState.None;

    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] PlayerCamLockOn camLock;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 12f;

    public bool isBlocking = false;
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

    public bool HasReadied { get; private set; } = false;

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
    [SerializeField] public int currentPotionCount;
    [SerializeField] public int maxPotionCount;
    public CinemachineStateDrivenCamera stateCam;
    public GameObject playerHUD;

    public NetworkVariable<bool> IsReadySynced = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action onHurt;
    public event Action onDeath;
    public Coroutine staminaRegenCoroutine;

    public override void OnNetworkSpawn()
    {
        GameManager.Instance.LockCursor();
        jumpHeight = 2.2f;
        baseWalkSpeed = 4f;
        currentWalkSpeed = 2f;
        runSpeed = 5.8f;
        maxPotionCount = 3;
        currentPotionCount = maxPotionCount;

        if (IsOwner)
        {
            stateCam.Priority = 100;
            gameObject.tag = "Player";
            playerHUD.SetActive(true);
        }
        else
        {
            stateCam.Priority = 0;
            playerHUD.SetActive(false);


        }
        playerBaseCurrentHealth.OnValueChanged += (oldVal, newVal) => {
            if (newVal < oldVal)
            {
                if(newVal <= 0)
                {
                    onDeath?.Invoke();
                }
                if (playerState == PlayerState.IsHealing) return;
                onHurt?.Invoke();
            }
        };
        playerBaseMaxHealth = 200;
        playerBaseCurrentHealth.Value = playerBaseMaxHealth;
        playerBaseMaxStamina = 100;
        playerBaseCurrentStamina.Value = playerBaseMaxStamina;
        playerBaseMinAP = 11;
        playerBaseMaxAP = 13;
        playerBaseDefense = 13;
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
        GameManager.Instance.UnlockCursor();
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

        if (playerState == PlayerState.IsJumping)
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
        else if(playerState == PlayerState.IsExhausted)
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.moveAction.Disable();
        }
        else if (playerState == PlayerState.IsDead || anim.GetCurrentAnimatorStateInfo(1).IsName("Death"))
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.moveAction.Disable();
        }
        else if (PauseManager.Instance.isPaused() || GameManager.Instance.isInConfirmation())
        {
            playerInputReader.sprintAction.Disable();
            playerInputReader.jumpAction.Disable();
            playerInputReader.lAttackAction.Disable();
            playerInputReader.hAttackAction.Disable();
            playerInputReader.dodgeAction.Disable();
            playerInputReader.blockAction.Disable();
            playerInputReader.healAction.Disable();
            playerInputReader.moveAction.Disable();
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
        if (playerState != PlayerState.IsAttacking &&
          playerState != PlayerState.IsDodging &&
          playerBaseCurrentStamina.Value < playerBaseMaxStamina)
        {
            if (staminaRegenCoroutine == null)
            {
                staminaRegenCoroutine = StartCoroutine(StaminaRegen());
            }
        }
        else
        {
            // Stop regenerating if they attack, dodge, or hit max stamina
            if (staminaRegenCoroutine != null)
            {
                StopCoroutine(staminaRegenCoroutine);
                staminaRegenCoroutine = null; // Reset the reference safely
            }
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

        Vector3 velocity = isBlocking ? move * currentWalkSpeed : move * currentSpeed;
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

        Vector3 velocity = isBlocking ? move * currentSpeed : move * currentWalkSpeed;
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
        if (isBusy && playerBaseCurrentHealth.Value == playerBaseMaxHealth || currentPotionCount == 0) return;
        playerState = PlayerState.IsHealing;
    }


    // ======================
    // LIGHT ATTACK COMBO
    // ======================
    public void PlayerLightAttack()
    {
        int staminaConsumption = 8;

     
        if (isBusy && playerState != PlayerState.IsAttacking) return;

      
        if (playerState == PlayerState.IsAttacking && !canCombo) return;

      
        if (isHeavyAttacking) return;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;

       
        playerDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
        playerState = PlayerState.IsAttacking;
        comboTimer = 0f;

      
        if (comboStep == 0)
        {
            comboStep = 1;
            canCombo = false;
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

      
        playerBaseCurrentStamina.Value -= staminaConsumption;
    }

    // ======================
    // HEAVY ATTACK
    // ======================
    public void PlayerHeavyAttack()
    {
        int baseDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
        int staminaConsumption = 17;
        playerDamage = baseDamage * 3;
        if (isBusy) return;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;
        playerState = PlayerState.IsAttacking;
        if (comboStep != 0) return;
        if (isHeavyAttacking) return;
        playerBaseCurrentStamina.Value -= staminaConsumption;
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
        int staminaConsumption = 15;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;
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
        playerBaseCurrentStamina.Value -= staminaConsumption;



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
        Debug.Log("Blocking Stopped");
        playerState = PlayerState.None;
        isBlocking = false;
    }
    void PlayerOnDeath()
    {
       
        playerBaseCurrentHealth.Value = 0;
        Cursor.lockState = CursorLockMode.None;
       
    }
    public void TakeDamage(int damage, Vector3 hitDir)
    {
        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Death") || isDodging) return;
    
       
        int healthDamage = (damage * damage) / (damage + playerBaseDefense);
        int staminaDamage = (healthDamage * (5/2));
        float knockbackForce = (damage <20)? 5f: 7f;
        ApplyKnockbackClientRpc(knockbackForce, hitDir);
        if (playerState == PlayerState.IsBlocking && isBlocking)
        {
            playerBaseCurrentHealth.Value -= healthDamage / 5;
            playerBaseCurrentStamina.Value -= staminaDamage;
            return;

        }
        if(playerState == PlayerState.IsBlocking && isBlocking && playerBaseCurrentStamina.Value <= 0)
        {
            playerBaseCurrentStamina.Value = 0;
            playerState = PlayerState.IsExhausted;
        }
        playerBaseCurrentHealth.Value -= healthDamage;

    

       
        if (playerBaseCurrentHealth.Value <= 0)
        {
            PlayerOnDeath();
          
        }
        else if(playerBaseCurrentStamina.Value <= 0)
        {
            playerBaseCurrentStamina.Value = 0;
            playerState = PlayerState.IsExhausted;

        }
      
       
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void ApplyKnockbackClientRpc(float force, Vector3 dir)
    {
       
        ApplyPlayerKnockback(force, dir);
    }


    public void ApplyPlayerKnockback(float knockbackForce, Vector3 hitDir)
    {
      
        anim.applyRootMotion = false;

       
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

     
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
        Debug.Log("Nonee");
    }
    void DecreasePotionCount()
    {
        currentPotionCount--;
    }
    void HealPlayer()
    {
        int healValue = 45;
        int damageToHealth = playerBaseMaxHealth - playerBaseCurrentHealth.Value;
        playerBaseCurrentHealth.Value += healValue;
        if(healValue > damageToHealth)
        {
            playerBaseCurrentHealth.Value = playerBaseMaxHealth;
        }

    }
    void SetPlayerDeath()
    {
        playerState = PlayerState.IsDead;
        GameManager.Instance.NotifyPlayerDeathRpc();
    }
    [Rpc(SendTo.Server)]
    public void SetReadyRpc()
    {
        IsReadySynced.Value = true;
        GameManager.Instance.SetPlayerReadyRpc(true);
    }

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
    private IEnumerator StaminaRegen()
    {

        while (playerBaseCurrentStamina.Value < playerBaseMaxStamina)
        {
            yield return new WaitForSeconds(0.08f);
            playerBaseCurrentStamina.Value += 1;
        }
        staminaRegenCoroutine = null;
    }
}   
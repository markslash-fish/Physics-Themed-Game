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

    public enum PlayerState { None, IsJumping, IsDodging, IsAttacking, IsBlocking, IsHealing, IsHurt, IsExhausted, IsDead, }
    public PlayerState playerState;

    public bool isBusy => playerState != PlayerState.None;

    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] PlayerCamLockOn camLock;
    [SerializeField] PlayerAnimationController animationController;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

    [Header("Dodge")]
    [SerializeField] float dodgeForce = 12f;

    public bool isBlocking = false;
    public bool isDodging = false;

    [Header("Movement")]
    [SerializeField] private float jumpHeight;
    [SerializeField] public float baseWalkSpeed;
    [SerializeField] public float currentWalkSpeed;
    [SerializeField] public float runSpeed;
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
    public int healValue;
    public int heavyDamage;
    public int uniqueSkillDamage;
    public CinemachineStateDrivenCamera stateCam;
    public GameObject playerHUD;
    public float skillCooldown;
    public bool skillInCooldown;
    public float cooldownTimer;
    public NetworkVariable<bool> IsReadySynced = new NetworkVariable<bool>(false,
         NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public event Action onHurt;
    public event Action onDeath;
    public event Action onExhaust;
    public Coroutine staminaRegenCoroutine;

    public override void OnNetworkSpawn()
    {
        healValue = 45;
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
                if (newVal <= 0) onDeath?.Invoke();
                if (playerState == PlayerState.IsHealing) return;
                onHurt?.Invoke();
            }
        };

        playerBaseCurrentStamina.OnValueChanged += (oldVal, newVal) => {
            if (newVal < oldVal)
            {
                if (playerState == PlayerState.IsExhausted) return;
                if (newVal <= 0 && playerState == PlayerState.IsBlocking)
                {
                    onExhaust?.Invoke();
                }
            }
        };

        // FIX: Only server writes NetworkVariables
        if (IsServer)
        {
            playerBaseMaxHealth = 200;
            playerBaseCurrentHealth.Value = playerBaseMaxHealth;
            playerBaseMaxStamina = 100;
            playerBaseCurrentStamina.Value = playerBaseMaxStamina;
        }
        else
        {
            playerBaseMaxHealth = 200;
            playerBaseMaxStamina = 100;
        }

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
        playerInputReader.onUniqueSkillStarted += PlayerUniqueSkill;
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
        playerInputReader.onUniqueSkillStarted -= PlayerUniqueSkill;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerInputReader = GetComponent<PlayerInputReader>();
        animationController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        if (IsOwner)
        {
            CheckGround();
            CalculateMovement();
            HandleComboReset();

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
        else if (playerState == PlayerState.IsAttacking || anim.GetCurrentAnimatorStateInfo(4).IsName(skillTrigger))
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
        else if (playerState == PlayerState.IsExhausted)
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
        else if (playerState == PlayerState.IsDead)
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
        else if (PauseManager.Instance.isPaused() || GameManager.Instance.isInConfirmation() || anim.GetCurrentAnimatorStateInfo(0).IsName("Player_Kneel"))
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
            playerState != PlayerState.IsExhausted &&
            playerBaseCurrentStamina.Value < playerBaseMaxStamina)
        {
            if (staminaRegenCoroutine == null)
                staminaRegenCoroutine = StartCoroutine(StaminaRegen());
        }
        else
        {
            if (staminaRegenCoroutine != null)
            {
                StopCoroutine(staminaRegenCoroutine);
                staminaRegenCoroutine = null;
            }
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (playerState == PlayerState.IsHurt) return;
        if (!canMove && isBusy) return;

        if (camLock != null && camLock.currentEnemy != null)
        {
            Vector3 dirToEnemy = (camLock.currentEnemy.position - transform.position).normalized;
            dirToEnemy.y = 0;
            if (dirToEnemy != Vector3.zero || playerState != PlayerState.IsDodging)
                transform.rotation = Quaternion.LookRotation(dirToEnemy);
        }
        else
        {
            if (move != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 10f);
            }
        }

        float currentSpeed = isRunning ? runSpeed : baseWalkSpeed;

        if (isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.fixedDeltaTime;

        Vector3 velocity = isBlocking ? move * currentWalkSpeed : move * currentSpeed;
        velocity.y = verticalVelocity;
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
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

        Vector3 forward;
        Vector3 right;

        if (camLock != null && camLock.currentEnemy != null)
        {
            forward = (camLock.currentEnemy.position - transform.position).normalized;
            forward.y = 0;
            forward.Normalize();
            right = new Vector3(forward.z, 0f, -forward.x);
        }
        else
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
        }

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
        if (isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = isBlocking ? move * currentSpeed : move * currentWalkSpeed;
        velocity.y = verticalVelocity;
    }

    void PlayerMove(Vector2 input) { movement = input; }

    void PlayerJump()
    {
        if (isBusy) return;
        if (isGrounded)
        {
            playerState = PlayerState.IsJumping;
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void SetSprint(bool value) { value = true; isRunning = value; }
    void EndSprint(bool value) { value = false; isRunning = value; }
    public void PlayerInteract() { }

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
        if (!IsOwner) return;
        int staminaConsumption = 12;

        if (isBusy && playerState != PlayerState.IsAttacking) return;
        if (playerState == PlayerState.IsAttacking && !canCombo) return;
        if (isHeavyAttacking) return;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;

        playerDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
        playerState = PlayerState.IsAttacking;
        comboTimer = 0f;

        if (comboStep == 0) { comboStep = 1; canCombo = false; }
        else if (comboStep == 1 && canCombo) { comboStep = 2; canCombo = false; }
        else if (comboStep == 2 && canCombo) { comboStep = 3; canCombo = false; }

        // FIX: Hindi na direkta ang write — ServerRpc na para gumana sa Player 2
        DeductStaminaServerRpc(staminaConsumption);
    }

    // ======================
    // HEAVY ATTACK
    // ======================
    public void PlayerHeavyAttack()
    {
        if (!IsOwner) return;
        int staminaConsumption = 23;

        if (isBusy && playerState != PlayerState.IsAttacking) return;
        if (isHeavyAttacking) return;
        if (comboStep != 0) return;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;

        int baseDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP);
        heavyDamage = baseDamage * 2;
        playerDamage = heavyDamage;
        playerState = PlayerState.IsAttacking;
        isHeavyAttacking = true;
        comboTimer = 0f;

        // FIX: Hindi na direkta ang write — ServerRpc na para gumana sa Player 2
        DeductStaminaServerRpc(staminaConsumption);
    }

    public void PlayerUniqueSkill()
    {
        playerDamage = UnityEngine.Random.Range(playerBaseMinAP, playerBaseMaxAP) + uniqueSkillDamage;
        if (skillInCooldown || isBusy) return;
        if (!isGrounded) return;
        animationController.PlaySkill(skillTrigger);
        StartCoroutine(StartSkillCooldown());
        cooldownTimer = skillCooldown;

        if (skillTrigger == "isSpeedSkill")
        {
            playerState = PlayerState.IsDodging;
            anim.applyRootMotion = false;
            isDodging = true;
            Vector3 dodgeDir = move == Vector3.zero ? transform.forward : move;
            rb.AddForce(dodgeDir * dodgeForce, ForceMode.Impulse);
        }
        else if (skillTrigger == "isTimeSkill")
        {
            playerState = PlayerState.IsHealing;
        }
        else if (skillTrigger == "isForceSkill" || skillTrigger == "isMassSkill")
        {
            playerState = PlayerState.IsAttacking;
            anim.SetBool("isAttacking", true);
        }
    }

    // ======================
    // COMBO SYSTEM
    // ======================
    void HandleComboReset()
    {
        if (comboStep > 0)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboResetTime) ResetCombo();
        }
    }

    public void EnableNextCombo() { canCombo = true; }

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

    public void DisableMove() { canMove = false; }
    public void EnableMove() { canMove = true; }

    public void Dodge()
    {
        if (!IsOwner) return;
        if (isBusy) return;
        int staminaConsumption = 19;
        if (playerBaseCurrentStamina.Value < staminaConsumption) return;
        playerState = PlayerState.IsDodging;
        if (!isGrounded) return;

        anim.applyRootMotion = false;
        isDodging = true;
        Vector3 dodgeDir = move == Vector3.zero ? transform.forward : move;
        rb.AddForce(dodgeDir * dodgeForce, ForceMode.Impulse);

        // FIX: Hindi na direkta ang write — ServerRpc na para gumana sa Player 2
        DeductStaminaServerRpc(staminaConsumption);
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
        playerState = PlayerState.IsDead;
        Cursor.lockState = CursorLockMode.None;
    }

    // ======================
    // TAKE DAMAGE
    // ======================
    public void TakeDamage(int damage, Vector3 hitDir)
    {
        // FIX: Server lang ang mag-process ng damage — para consistent sa Player 1 at Player 2
        if (!IsServer) return;

        if (playerState == PlayerState.IsDead || isDodging || anim.GetCurrentAnimatorStateInfo(1).IsName("HeavyHit")) return;

        int healthDamage = (damage * damage) / (damage + playerBaseDefense);
        int staminaDamage = Mathf.RoundToInt(healthDamage * (5f / 2f));
        float knockbackForce = anim.GetCurrentAnimatorStateInfo(1).IsName("Player_Exhaust") ? 16f : 5f;

        ResetCombo();

        if (isHeavyAttacking || playerState == PlayerState.IsAttacking)
        {
            isHeavyAttacking = false;
            canMove = true;
        }

        // BLOCKING: bawasan stamina, maliit na health damage
        if (playerState == PlayerState.IsBlocking && isBlocking)
        {
            healthDamage = 8;
            knockbackForce = 6f;

            // FIX: Stamina reduction habang nagbo-block — dati gumagana na ito pero kailangan ng IsServer check
            playerBaseCurrentStamina.Value -= staminaDamage;
            if (playerBaseCurrentStamina.Value < 0) playerBaseCurrentStamina.Value = 0;

            ApplyKnockbackClientRpc(knockbackForce, hitDir);
            playerBaseCurrentHealth.Value -= healthDamage;
            if (playerBaseCurrentHealth.Value < 0) playerBaseCurrentHealth.Value = 0;

            if (playerBaseCurrentStamina.Value <= 0)
            {
                onExhaust?.Invoke();
                StartCoroutine(Flinch());
            }
            return;
        }

        ApplyKnockbackClientRpc(knockbackForce, hitDir);

        playerBaseCurrentHealth.Value -= healthDamage;
        if (playerBaseCurrentHealth.Value < 0) playerBaseCurrentHealth.Value = 0;

       
     
        if (playerBaseCurrentStamina.Value < 0) playerBaseCurrentStamina.Value = 0;

        playerState = PlayerState.IsHurt;

        if (playerBaseCurrentHealth.Value <= 0) PlayerOnDeath();
    }

    // FIX: Bagong ServerRpc para sa stamina deduction ng player actions
    // Kailangan ito kasi Server write permission na ang stamina, hindi na Owner
    [ServerRpc(RequireOwnership = false)]
    private void DeductStaminaServerRpc(int amount)
    {
        playerBaseCurrentStamina.Value -= amount;
        if (playerBaseCurrentStamina.Value < 0) playerBaseCurrentStamina.Value = 0;
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
        if (camLock != null) camLock.ToggleLockOn();
    }

    void ResetState()
    {
        anim.applyRootMotion = true;
        playerState = PlayerState.None;
        isHeavyAttacking = false;
        animationController.ResetAttacking();
        Debug.Log("Nonee");
    }

    void ResetPlayerStamina()
    {
        // FIX: Server lang ang makakawrite
        if (!IsServer) return;
        playerBaseCurrentStamina.Value = playerBaseMaxStamina;
    }

    void DecreasePotionCount() { currentPotionCount--; }

    void HealPlayer()
    {
        // FIX: Server lang ang makakawrite ng health
        if (!IsServer) return;
        int damageToHealth = playerBaseMaxHealth - playerBaseCurrentHealth.Value;
        playerBaseCurrentHealth.Value += healValue;
        if (healValue > damageToHealth) playerBaseCurrentHealth.Value = playerBaseMaxHealth;
    }

    void TriggerTimeSkillEffects()
    {
        // FIX: Server lang ang makakawrite ng health
        if (!IsServer) return;
        healValue *= 2;
        int damageToHealth = playerBaseMaxHealth - playerBaseCurrentHealth.Value;
        playerBaseCurrentHealth.Value += healValue;
        if (healValue > damageToHealth) playerBaseCurrentHealth.Value = playerBaseMaxHealth;
        if (currentPotionCount != maxPotionCount) currentPotionCount += 1;
    }

    void SetPlayerDeath()
    {
        if (!IsOwner) return;
        GameManager.Instance.NotifyPlayerDeathRpc();
        StartCoroutine(DespawnDelay());
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
        // FIX: Server lang ang mag-regen ng stamina dahil Server write permission na
        if (!IsServer) yield break;

        while (playerBaseCurrentStamina.Value < playerBaseMaxStamina)
        {
            yield return new WaitForSeconds(0.08f);
            playerBaseCurrentStamina.Value += 1;
        }
        staminaRegenCoroutine = null;
    }

    [ServerRpc]
    public void RequestDamageWindowServerRpc(float duration)
    {
        EnemyAttackHitboxScript[] hitboxes = GetComponentsInChildren<EnemyAttackHitboxScript>();
        foreach (var hitbox in hitboxes)
            hitbox.StartDamageWindow(duration);
    }

    private IEnumerator StartSkillCooldown()
    {
        skillInCooldown = true;
        yield return new WaitForSeconds(skillCooldown);
        skillInCooldown = false;
        Debug.Log("Skill ready!");
    }

    private IEnumerator Flinch()
    {
        playerInputReader.moveAction.Disable();
        yield return new WaitForSeconds(0.8f);
        playerInputReader.moveAction.Enable();
    }
    private IEnumerator DespawnDelay()
    {
        yield return new WaitForSeconds(3f);
        GetComponent<NetworkObject>().Despawn();
    }
}
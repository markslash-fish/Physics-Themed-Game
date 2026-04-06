using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform groundCheck;

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

    void OnEnable()
    {
        playerInputReader.onSprint += SetSprint;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;

        playerInputReader.onLightAttackStarted += Attack;
        playerInputReader.onHeavyAttackStarted += HeavyAttack;
    }

    void OnDisable()
    {
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

    }

    void Update()
    {
        
        CheckGround();
        CalculateMovement();
        HandleMovementAnimation();
        HandleComboReset();
       
    }

void FixedUpdate()
{
    if (!canMove) 
    {
        return;
    }
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

public void AddBootStats(float speedBonus, float jumpBonus)
{
    walkSpeed += speedBonus;
    runSpeed += speedBonus;
    jumpHeight += jumpBonus;
}
}
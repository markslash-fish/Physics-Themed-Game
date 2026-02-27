using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour

{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpd = 10f;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float accelerationSpd = 20f;
    [SerializeField] private float decelerationSpd = 20f;


    public bool isWalking, isGrounded, isIdle;
    [Header("Camera Settings")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private float verticalClamp = 90f;
    [SerializeField] private float cameraPitch = 0f;
    [Header("References")]

    [Header("Combo")]
    public float lastClickedTime;
    public float comboResetTime = 0.5f;
    public int comboStep;
    [SerializeField] private float attackDelay = 0.2f;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool isAttacking;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private Transform groundCheck;
    private PlayerInput playerInput;
    private InputAction moveAction, jumpAction, lookAction, attackAction;

    Rigidbody rb;


    private Vector2 moveInput, lookInput;
    private Vector3 currentVelocity;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        jumpHeight = 5f;

    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        lookAction = playerInput.actions["Look"];
        attackAction = playerInput.actions["Attack"];
    }


    void Update()
    {
        OnPlayerJump();
        OnPlayerIdle();
        OnPlayerAttack();
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        PlayerLook();
        if (moveAction.triggered && canAttack)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

    }
    private void FixedUpdate()
    {
        OnPlayerMove();


    }
    public void OnPlayerIdle()
    {
        isIdle = moveInput.magnitude == 0;

    }
    public void OnPlayerMove()
    {
        isWalking = moveInput.magnitude > 0;


        PlayerMove();

    }
    public void OnPlayerJump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (jumpAction.triggered && isGrounded)
        {
            PlayerJump();
            Debug.Log("Jumped");
        }
    }
    public void OnPlayerAttack()
    {




        if (attackAction.triggered && canAttack)
        {
            PlayerAttack();
            Debug.Log(comboStep);
            canAttack = false;
            isAttacking = true;
        }


    }

    public void PlayerMove()
    {

        Vector3 movementVector = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        Vector3 moveVelocity = movementVector * moveSpd;
        float lerpSpd = (moveVelocity.magnitude > 0) ? accelerationSpd : decelerationSpd;

        currentVelocity = Vector3.Lerp(currentVelocity, moveVelocity, lerpSpd * Time.fixedDeltaTime);

        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }
    public void PlayerJump()
    {

        rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        isGrounded = false;
    }
    public void PlayerLook()
    {
        float lookX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float lookY = lookInput.y * mouseSensitivity * Time.deltaTime;

        cameraPitch = Mathf.Clamp(cameraPitch, -verticalClamp, verticalClamp);
        cameraPitch -= lookY;

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

    }
    public void PlayerAttack()
    {
        comboStep++;
        comboStep = Mathf.Clamp(comboStep, 0, 3);






    }
    public void ResetComboStep()
    {
        comboStep = 0;

    }
    public void AttackEnable()
    {
        canAttack = true;

    }

}


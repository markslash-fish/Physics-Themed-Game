
using UnityEngine;

public class Player : MonoBehaviour
{
    
    [SerializeField] PlayerInputReader playerInputReader;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    public UnityEngine.Vector2 movement;
    private Rigidbody rb;
    public UnityEngine.Vector3 move;
    public bool isGrounded;
    public bool isRunning;

    [SerializeField] private Transform groundCheck;
    public LayerMask groundMask;
    private float groundDistance = 0.03f;

    void OnEnable()
    {
        playerInputReader.onSprint += SetSprint;
        playerInputReader.onLightAttackStarted += PlayerAttack;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;
    }
    void OnDisable()
    {
        playerInputReader.onSprint -= SetSprint;
        playerInputReader.onMove -= PlayerMove;
        playerInputReader.jumpStarted -= PlayerJump;
        playerInputReader.onLightAttackStarted -= PlayerAttack;
    }
    void Start()
    {
        
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        float x = movement.x;
        float z = movement.y;
        
        move = new UnityEngine.Vector3(x, 0f, z);
    }
    void FixedUpdate()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveposition = rb.position + move * currentSpeed * Time.deltaTime;
        rb.MovePosition(moveposition);
    }
        void PlayerMove(UnityEngine.Vector2 input)
        {
            movement = input;
        }
        void PlayerJump()
        {
            if (isGrounded)
            {
                rb.AddForce(UnityEngine.Vector3.up * jumpHeight, ForceMode.Impulse);
            }
        }
        void PlayerAttack()
        {
        
        }   
        void SetSprint(bool value)
    {
        isRunning = value;
    }
    

}
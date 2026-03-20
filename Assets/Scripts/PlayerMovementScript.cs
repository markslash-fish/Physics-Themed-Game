using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour

{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpd = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private bool isGrounded;




    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;

    [SerializeField] private float attackDelay = 0.2f;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool isAttacking;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private Transform groundCheck; 

    Rigidbody rb;


    private Vector2 movement;
    private Vector3 move;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;
    }
    private void OnDisable()
    {
        playerInputReader.onMove -= PlayerMove;
        playerInputReader.jumpStarted -= PlayerJump;
    }
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        float x = movement.x;
        float z = movement.y;

        move = new Vector3(x, 0f, z).normalized;
    }
    private void FixedUpdate()
    {
        Vector3 moveposition = rb.position + move * moveSpd * Time.deltaTime; ;

        rb.MovePosition(moveposition);
    }
    void PlayerMove(Vector2 input)
    {
        movement = input;
    }
    void PlayerJump()
    {
        if(isGrounded)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
      
    }


}
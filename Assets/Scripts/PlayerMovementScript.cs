using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementScript : MonoBehaviour

{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpd = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private bool isGrounded;


    public Transform enemy;

    [Header("References")]
    [SerializeField] PlayerInputReader playerInputReader;

    [SerializeField] private float attackDelay = 0.2f;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool isAttacking;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundDistance = 0.1f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] public float playerDamage;
    Rigidbody rb;


    private Vector2 movement;
    private Vector3 move;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void OnEnable()
    {

        playerDamage = 10f;
        playerInputReader.onLightAttackStarted += PlayerLightAttack;
        playerInputReader.onMove += PlayerMove;
        playerInputReader.jumpStarted += PlayerJump;
    }
    private void OnDisable()
    {
        
    }
    private void Update()
    {

       
        float x = movement.x;
        float z = movement.y;

        move = new Vector3(x, 0f, z);
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
  void PlayerLightAttack()
    {
        Debug.Log("Attack");
    }


}
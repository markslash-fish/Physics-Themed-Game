using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [SerializeField]  InputActionAsset inputActionAsset;
    public UnityAction<Vector2> onMove;
    public UnityAction jumpStarted;
    public UnityAction onDodgeStarted;
    public UnityAction onDodgeFinished;
    public UnityAction onBlockStarted;
    public UnityAction onBlockFinished;
    public UnityAction onHeal;
    public UnityAction onUniqueSkillStarted;
    public UnityAction onLightAttackStarted;
    public UnityAction onLightAttackFinished;
    public UnityAction onHeavyAttackStarted;
    public UnityAction onHeavyAttackFinished;
    public UnityAction onSprint;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dodgeAction;
    private InputAction blockAction;
    private InputAction skillAction;
    private InputAction healAction;
    private InputAction lAttackAction;
    private InputAction hAttackAction;
    private InputAction sprintAction;
    void OnEnable()
    {
        sprintAction = inputActionAsset.FindAction("Sprint");
        moveAction = inputActionAsset.FindAction("Move");
        jumpAction = inputActionAsset.FindAction("Jump");
        dodgeAction = inputActionAsset.FindAction("Dodge");
        blockAction = inputActionAsset.FindAction("Block");
        healAction = inputActionAsset.FindAction("Heal");
        skillAction = inputActionAsset.FindAction("Unique Skill");
        lAttackAction = inputActionAsset.FindAction("Light Attack");
        hAttackAction = inputActionAsset.FindAction("Heavy Attack");

        moveAction.started += OnMove;
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;


        jumpAction.started += OnJump;
        jumpAction.performed += OnJump;
        jumpAction.canceled += OnJump;


        dodgeAction.started += OnDodge;
        dodgeAction.performed += OnDodge;
        dodgeAction.canceled += OnDodge;


        blockAction.started += OnBlock;
        blockAction.performed += OnBlock;
        blockAction.canceled += OnBlock;


        healAction.started += OnHeal;
        healAction.performed += OnHeal;
        healAction.canceled += OnHeal;

        skillAction.started += OnUniqueSkill;
        skillAction.performed += OnUniqueSkill;
        skillAction.canceled += OnUniqueSkill;

        lAttackAction.started += OnLightAttack;
        lAttackAction.performed += OnLightAttack;
        lAttackAction.canceled += OnLightAttack;

        hAttackAction.started += OnHeavyAttack;
        hAttackAction.performed += OnHeavyAttack;
        hAttackAction.canceled += OnHeavyAttack;


        sprintAction.started += OnMove;
        sprintAction.performed += OnMove;
        sprintAction.canceled += OnMove;


        moveAction.Enable();
        jumpAction.Enable();
        dodgeAction.Enable();
        blockAction.Enable();
        healAction.Enable();
        skillAction.Enable();
        lAttackAction.Enable();
        hAttackAction.Enable();
    }

    void OnDisable()
    {
        moveAction.started -= OnMove;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;


        jumpAction.started -= OnJump;
        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJump;


        dodgeAction.started -= OnDodge;
        dodgeAction.performed -= OnDodge;
        dodgeAction.canceled -= OnDodge;


        blockAction.started -= OnBlock;
        blockAction.performed -= OnBlock;
        blockAction.canceled -= OnBlock;


        healAction.started -= OnHeal;
        healAction.performed -= OnHeal;
        healAction.canceled -= OnHeal;

        skillAction.started -= OnUniqueSkill;
        skillAction.performed -= OnUniqueSkill;
        skillAction.canceled -= OnUniqueSkill;

        lAttackAction.started -= OnLightAttack;
        lAttackAction.performed -= OnLightAttack;
        lAttackAction.canceled -= OnLightAttack;

        hAttackAction.started -= OnHeavyAttack;
        hAttackAction.performed -= OnHeavyAttack;
        hAttackAction.canceled -= OnHeavyAttack;

        sprintAction.started -= OnMove;
        sprintAction.performed -= OnMove;
        sprintAction.canceled -= OnMove;

    }
    public void OnBlock(InputAction.CallbackContext context)
    {
        if (onBlockStarted != null && context.started) onBlockStarted.Invoke();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (onDodgeStarted != null && context.started) onDodgeStarted.Invoke();
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (onHeal != null && context.started) onHeal.Invoke();
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (onHeavyAttackStarted != null && context.performed) onHeavyAttackStarted.Invoke();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (jumpStarted != null && context.started) jumpStarted.Invoke();
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (onLightAttackStarted != null && context.canceled) onLightAttackStarted.Invoke();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        onMove?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnUniqueSkill(InputAction.CallbackContext context)
    {
        if (onUniqueSkillStarted != null && context.started) onUniqueSkillStarted.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (onSprint != null && context.started) onSprint.Invoke();
    }


}
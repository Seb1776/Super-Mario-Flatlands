using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    None, Super, Fire, Ice, Mega, Hammer, Turtle,
    Boomerang, Leaf, Tanooki, SuperTanooki
}

public class Player : MonoBehaviour
{
    public PowerUpType currentPowerUp = PowerUpType.Super;
    public float moveSpeed;
    public float runSpeed;
    public float transitionSpeed;
    public float jumpForce;
    public float fallMultiplier;
    public float crouchSpeedDecremental;
    public float requiredTimeToPWing;
    public float groundCheckLength;
    public LayerMask ground;
    public Transform groundCheck;
    public KeyCode jumpKey;
    public KeyCode runKey;
    public KeyCode crouchKey;
    public bool canMove;
    public bool isWalking;
    public bool pWingJumpStart;
    public Collider bigCollider;
    [SerializeField] float currentTimeToPWing;
    [SerializeField] bool onGround;
    [SerializeField] bool pWing;
    [Header ("Leaf Powers")]
    public float leafFloatDuration;
    public float leafFloatFallMultiplier;
    public bool onLeafFloat;

    Animator playerAnimator;
    Animator spriteAnimator;
    float currentSpeed;
    float inintialFallMultiplier;
    bool crouching;
    bool running;
    bool falling;
    Vector2 moveInput;
    Rigidbody rb;
    float currentLeafFloat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        spriteAnimator = transform.GetChild(0).GetComponent<Animator>();
    }

    void Start()
    {
        inintialFallMultiplier = fallMultiplier;
    }

    void Update()
    {
        GetPlayerInput();
        GetAnimations();
    }

    void FixedUpdate()
    {
        if (canMove)
            MovePlayer();
    }

    void GetPlayerInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        bigCollider.enabled = !crouching;

        RaycastHit floor;
        onGround = Physics.Raycast(groundCheck.position, Vector3.down, out floor, groundCheckLength, ground);
        
        if (rb.velocity.y < 0f)
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

        else if (rb.velocity.y > 0f && !Input.GetKey(jumpKey))
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpForce - 1f) * Time.deltaTime;

        if (Input.GetKeyDown(jumpKey))
            if (onGround || pWingJumpStart && PowerUpIs(PowerUpType.Leaf, PowerUpType.Tanooki, PowerUpType.SuperTanooki))
                rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        
        if (Input.GetKeyDown(jumpKey) && !onGround && pWing && !pWingJumpStart && PowerUpIs(PowerUpType.Leaf, PowerUpType.Tanooki, PowerUpType.SuperTanooki))
        {
            pWingJumpStart = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        }

        if (falling && PowerUpIs(PowerUpType.Leaf, PowerUpType.Tanooki, PowerUpType.SuperTanooki) && Input.GetKeyDown(jumpKey) && !pWingJumpStart)
        {
            if (!onLeafFloat)
            {
                onLeafFloat = true;
                fallMultiplier = leafFloatFallMultiplier;
            }

            else currentLeafFloat = 0f;
        }

        if (onLeafFloat)
        {
            currentLeafFloat += Time.deltaTime;

            if (currentLeafFloat >= leafFloatDuration)
            {
                currentLeafFloat = 0f;
                fallMultiplier = inintialFallMultiplier;
                onLeafFloat = false;
            }
        }
        
        if (Input.GetKeyDown(crouchKey))
            crouching = true;
            
        else if (Input.GetKeyUp(crouchKey))
            crouching = false;

        if (Input.GetKeyDown(runKey))
            running = true;
            
        else if (Input.GetKeyUp(runKey))
            running = false;

        if (onGround)
        {   
            if (crouching)
            {
                if (currentSpeed > 0f && rb.velocity.magnitude > 0.001f)
                    currentSpeed -= Time.deltaTime * crouchSpeedDecremental;
                
                if (Input.GetKeyUp(crouchKey))
                    currentSpeed = moveSpeed;
            }
        }

        isWalking = rb.velocity.magnitude > 0.001f;

        if (rb.velocity.y < 0f)
            falling = true;

        if (onGround)
            falling = false;
        
        if (crouching && pWing)
            currentTimeToPWing = 0;
        
        if (running && !crouching && onGround)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, transitionSpeed * Time.deltaTime);
            spriteAnimator.speed = Mathf.Lerp(spriteAnimator.speed, 2f, transitionSpeed * Time.deltaTime);

            if (currentTimeToPWing < requiredTimeToPWing && !pWingJumpStart)
            {
                currentTimeToPWing += Time.deltaTime;
            }
        }
        
        else if (!running && !crouching && currentSpeed != moveSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, transitionSpeed * Time.deltaTime);

            if (currentSpeed <= 5.1f)
            {
                currentSpeed = moveSpeed;
                spriteAnimator.speed = 1f;
            }

            spriteAnimator.speed = Mathf.Lerp(spriteAnimator.speed, 1f, transitionSpeed * Time.deltaTime);
        }

        if ((!running && currentTimeToPWing > 0f) || pWingJumpStart)
        {
            currentTimeToPWing -= Time.deltaTime;

            if (currentTimeToPWing <= 0f || PowerUpIs(PowerUpType.Leaf, PowerUpType.Tanooki, PowerUpType.SuperTanooki))
            {
                pWingJumpStart = false;
                pWing = false;
                currentTimeToPWing = 0f;
            }
        }

        if (currentTimeToPWing >= requiredTimeToPWing && !pWingJumpStart)
            pWing = true;
        
        else if (currentTimeToPWing < requiredTimeToPWing && !pWingJumpStart)
            pWing = false;
    }

    public void ChangePowerUp(PowerUpType put)
    {
        currentPowerUp = put;
    }

    public bool PowerUpIs(params PowerUpType[] compareTo)
    {   
        if (compareTo.Length > 1)
        {
            for (int i = 0; i < compareTo.Length; i++)
                if (currentPowerUp == compareTo[i])
                    return true;
            
            return false;
        }

        return currentPowerUp == compareTo[0];
    }

    void GetAnimations()
    {
        if (!crouching)
        {
            spriteAnimator.SetBool("onGround", onGround);
            spriteAnimator.SetBool("pRun", pWing);
            spriteAnimator.SetFloat("moveSpeed", moveInput.magnitude);
        }

        spriteAnimator.SetBool("crouching", crouching);

        if (!pWing) spriteAnimator.SetBool("jumpFall", falling);

        if (moveInput.x < 0 && !playerAnimator.GetBool("isBackwards"))
            playerAnimator.SetBool("isBackwards", true);
        
        else if (moveInput.x > 0 && playerAnimator.GetBool("isBackwards"))
            playerAnimator.SetBool("isBackwards", false);
    }

    void MovePlayer()
    {
        rb.velocity = new Vector3(moveInput.x * currentSpeed, rb.velocity.y, moveInput.y * currentSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(groundCheck.position, Vector3.down * groundCheckLength);
    }
}

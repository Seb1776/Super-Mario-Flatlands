using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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

    Animator playerAnimator;
    Animator spriteAnimator;
    float currentSpeed;
    [SerializeField]
    float currentTimeToPWing;
    bool crouching;
    bool onGround;
    bool running;
    bool falling;
    [SerializeField]
    bool pWing;
    Collider bigCollider;
    Collider smallCollider;
    Vector2 moveInput;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        spriteAnimator = transform.GetChild(0).GetComponent<Animator>();
        bigCollider = GetComponent<Collider>();
        smallCollider = transform.GetChild(3).GetComponent<Collider>();
    }

    void Start()
    {
        currentSpeed = moveSpeed;
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

        /*smallCollider.enabled = crouching;
        bigCollider.enabled = !crouching;*/

        RaycastHit floor;

        if (Physics.Raycast(groundCheck.position, Vector3.down, out floor, groundCheckLength, ground))
            onGround = true;
        
        else
            onGround = false;
        
        if (rb.velocity.y < 0f)
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;

        else if (rb.velocity.y > 0f && !Input.GetKey(jumpKey))
            rb.velocity += Vector3.up * Physics.gravity.y * (jumpForce - 1f) * Time.deltaTime;

        if (Input.GetKeyDown(jumpKey) && onGround)
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        
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
                if (currentSpeed > 0f)
                    currentSpeed -= Time.deltaTime * crouchSpeedDecremental;
                
                if (Input.GetKeyUp(crouchKey))
                    currentSpeed = moveSpeed;
            }
        }
        
        if (rb.velocity.y < 0f)
            falling = true;
        
        else
            falling = false;
        
        if (running && !crouching)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, runSpeed, transitionSpeed * Time.deltaTime);
            spriteAnimator.speed = Mathf.Lerp(spriteAnimator.speed, 2f, transitionSpeed * Time.deltaTime);

            if (currentTimeToPWing < requiredTimeToPWing)
                currentTimeToPWing += Time.deltaTime;
        }
        
        else if (!running && !crouching)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, transitionSpeed * Time.deltaTime);
            spriteAnimator.speed = Mathf.Lerp(spriteAnimator.speed, 1f, transitionSpeed * Time.deltaTime);

            if (currentTimeToPWing > 0f)
                currentTimeToPWing -= Time.deltaTime;
        }

        if (currentTimeToPWing >= requiredTimeToPWing)
            pWing = true;
        
        else
            pWing = false;
    }

    void GetAnimations()
    {
        spriteAnimator.SetBool("onGround", !onGround);
        spriteAnimator.SetBool("pRun", pWing);
        spriteAnimator.SetFloat("moveSpeed", rb.velocity.magnitude);
        spriteAnimator.SetBool("jumpFall", falling);
        spriteAnimator.SetBool("crouching", crouching);

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

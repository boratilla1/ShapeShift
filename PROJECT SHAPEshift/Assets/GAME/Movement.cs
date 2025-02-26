using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private float horizontal;
    private float speed = 5f;
    private float jumpingPower = 13f;
    private bool isFacingRight = true;

    private bool isWallSliding;
    private float wallSlidingSpeed = 5f;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.1f;
    private Vector2 wallJumpingPower = new Vector2(7f, 14f);

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 12f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.5f;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] public GameObject afterimagePrefab; // Afterimage için prefab
    [SerializeField] private float afterimageSpawnRate = 0.01f; // Afterimage oluşma süresi
    [SerializeField] private float afterimageLifetime = 0.5f; // Afterimage'in kaybolma süresi

    private float lastAfterimageTime;
    private void Update()
    {
        if (isDashing)
        {
            return;
        }

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0f)
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isIdle", false);
        }
        else
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isIdle", true);
        }

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            animator.SetBool("isRising", true);
            animator.SetBool("isFalling", false);
            coyoteTimeCounter = 0;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        if (rb.linearVelocity.y < 0f && !IsGrounded())
        {
            animator.SetBool("isFalling", true);
            animator.SetBool("isRising", false);
        }
        else if (rb.linearVelocity.y == 0f && IsGrounded())
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isRising", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            float slideSpeed = Input.GetKey(KeyCode.S) ? wallSlidingSpeed * 2f : wallSlidingSpeed; // Aşağı basıldığında hızlanır

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -slideSpeed, float.MaxValue));
            animator.SetBool("isWallSliding", true);
        }
        else
        {
            isWallSliding = false;
            animator.SetBool("isWallSliding", false);
        }
    }


    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            animator.SetBool("isWallSliding", false);
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
    private void CreateAfterimage()
    {
        if (afterimagePrefab == null) return; // Eğer prefab yoksa çık

        GameObject afterimage = Instantiate(afterimagePrefab, transform.position, Quaternion.identity);
        SpriteRenderer afterimageRenderer = afterimage.GetComponent<SpriteRenderer>();
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();

        if (afterimageRenderer != null && playerRenderer != null)
        {
            afterimageRenderer.sprite = playerRenderer.sprite; // Karakterin mevcut sprite'ını al
            afterimageRenderer.color = new Color(1f, 1f, 1f, 0.5f); // Hafif transparan yap
            afterimage.transform.localScale = transform.localScale; // Karakterin ölçeğini kopyala
        }

        Destroy(afterimage, afterimageLifetime); // Belirlenen sürede sil
    }



    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.SetBool("isDashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDirection = new Vector2(horizontal, Input.GetAxisRaw("Vertical")).normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        }

        rb.velocity = dashDirection * dashingPower;
        tr.emitting = true;

        float dashEndTime = Time.time + dashingTime;
        while (Time.time < dashEndTime)
        {
            CreateAfterimage();
            yield return new WaitForSeconds(afterimageSpawnRate);
        }

        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("isDashing", false);
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }



}

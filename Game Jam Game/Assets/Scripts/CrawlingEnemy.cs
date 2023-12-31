using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrawlingEnemy : MonoBehaviour
{
    //stop at edge stuff
    public LayerMask enemyMask;
    public float speed;
    Rigidbody2D rb;
    float width;

    //Chase stuff
    public GameObject pixie;
    private Transform pixieTransform;
    public float pixieAggroDistance;

    public GameObject player;
    private Transform playerTransform;
    public float playerAggroDistance;

    // jump stuff
    public float jumpForce;
    public float jumpHeight;
    [Range(0,1)]
    public float jumpDistanceProportion;
    public float jumpCooldown;
    private bool canJump = true;
    private float jumpDistance;
    private float lastJumped;

    // rigidbody movement
    Vector3 v = Vector3.zero; 
    float smoothTime = 0.3f;

    // animation
    public Animator animator;
    

    void Start()
    {
        pixieTransform = pixie.GetComponent<Transform>();
        playerTransform = player.GetComponent<Transform>();

        rb = this.GetComponent<Rigidbody2D>();
        width = GetComponent<SpriteRenderer>().bounds.extents.x;

        jumpDistance = pixieAggroDistance * jumpDistanceProportion;
    }

    void FixedUpdate() {

        if(isGrounded()) animator.SetBool("isJumping", false);

        //go towards pixie if in range
        if (targetIsWithin(pixieTransform, pixieAggroDistance) || (targetIsWithin(playerTransform, playerAggroDistance))) {
            
            if(!isGrounded()) return;

            // prioritize pixie (this is only really noticable for jumping)
            Transform targetTransform = targetIsWithin(pixieTransform, pixieAggroDistance) ? pixieTransform : playerTransform;

            // Move the character by finding the target velocity
            Vector3 dir = targetTransform.position - transform.position;
            dir = Vector3.Normalize(dir);
			Vector3 targetVelocity = new Vector2(dir.x * 10f, rb.velocity.y);

            if (isFacingWrongWay(targetVelocity.x)) {
                flip();
            }
			
            // And then smoothing it out and applying it to the character
			rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref v, smoothTime);

            // if in closer dist, jump
            // can't jump again until pixie reenters said closer dist
            if (targetIsWithin(targetTransform, jumpDistance)) {
                if(canJump && isGrounded()) {
                    jumpAtTarget(targetTransform.position);
                    canJump = false;
                    lastJumped = Time.time;
                    animator.SetBool("isJumping", true);
                }
            }

            if (lastJumped + jumpCooldown < Time.time) canJump = true;

        } else {

            // once you drop aggro can jump again
            canJump = true;

            //Debug.Log(isGrounded() + ", " + isHittingWall());

            //if no ground or is hitting wall, turn around
            if (!isGrounded() || isHittingWall()) {
                flip();
            }

            //always move forward
            Vector2 targetVelocity = rb.velocity;
            targetVelocity.x = transform.right.x * speed;
            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref v, smoothTime);
        }
    }

    bool isFacingWrongWay(float xVelocity) {
        Vector3 rotation = transform.eulerAngles;

        if (xVelocity > 0 && rotation.y == 180) {
            return true;
        } else if (xVelocity < 0 && rotation.y == 0) {
            return true;
        }

        return false;
    }

    void flip() {
        Vector3 rotation = transform.eulerAngles;
        rotation.y = (rotation.y == 0 ? 180 : 0);
        transform.eulerAngles = rotation;
    }

    void jumpAtTarget(Vector3 targetPos) {
        Vector3 force = (targetPos - transform.position) * jumpForce;
        force.y += jumpHeight; // aim for the head
        rb.AddForce(force);
        canJump = false;
    }

    bool isGrounded() {
        Vector2 lineCastPos = transform.position + (transform.right * width);
        return Physics2D.Linecast(lineCastPos, lineCastPos + Vector2.down * 2f, enemyMask);
    }

    bool isHittingWall() {
        Vector2 lineCastPos = transform.position + (transform.right * width);

        Vector3 rotation = transform.eulerAngles;
        Vector2 dir = (rotation.y == 0 ? 1 : -1) * Vector2.right * 0.05f;

        return Physics2D.Linecast(lineCastPos, lineCastPos + dir, enemyMask);
    }

    bool targetIsWithin(Transform t, float d) {
        return Vector2.Distance(transform.position, t.position) < d;
    }
}

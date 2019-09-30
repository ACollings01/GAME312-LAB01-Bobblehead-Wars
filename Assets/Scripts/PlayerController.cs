using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float[] hitForce;    // Used in determining camera shake intensity

    // Used in giving the player i-frames when hit
    public float timeBetweenHits = 2.5f;
    private bool isHit = false;
    private float timeSinceHit = 0;
    private int hitNumber = -1;

    public LayerMask layerMask;
    private Vector3 currentLookTarget = Vector3.zero;
    public Rigidbody head;
    public float moveSpeed = 50.0f;
    private CharacterController characterController;

    public Animator bodyAnimator;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"),
                                            0,
                                            Input.GetAxis("Vertical"));
        characterController.SimpleMove(moveDirection * moveSpeed);

        // Reset iFrames after a short time
        if (isHit)
        {
            timeSinceHit += Time.deltaTime;
            if (timeSinceHit > timeBetweenHits)
            {
                isHit = false;
                timeSinceHit = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        // Sets the direction of movement to that which is input using WASD or the arrow keys
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"),
                                            0,
                                            Input.GetAxis("Vertical"));
        if (moveDirection == Vector3.zero)
        {
            // Boolean to control walking/idle animation
            bodyAnimator.SetBool("IsMoving", false);
        }
        else
        {
            // Boolean to control walking/idle animation
            bodyAnimator.SetBool("IsMoving", true);

            // This makes the marine's head bobble
            head.AddForce(transform.right * 150, ForceMode.Acceleration);
        }

        // Creates a ray from the mouse pointer on the screen to the floor of the arena
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.green);   // Shows the ray in green in the scene view

        // Sets the rotation of the marine so that it points to the mouse pointer's ray where it intersects the floor of the arena
        if (Physics.Raycast(ray, out hit, 1000, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.point != currentLookTarget)
            {
                currentLookTarget = hit.point;
            }

            Vector3 targetPosition = new Vector3(hit.point.x,
                                                    transform.position.y,
                                                    hit.point.z);
            Quaternion rotation = Quaternion.LookRotation(targetPosition - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation,
                                                    rotation,
                                                    Time.deltaTime * 10.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If an existing alien hits the player, increment hitNumber and shake the camera accordingly, or kill the player if necessary
        Alien alien = other.gameObject.GetComponent<Alien>();
        if (alien != null)
        {
            if (!isHit)
            {
                hitNumber += 1;
                CameraShake cameraShake = Camera.main.GetComponent<CameraShake>();
                if (hitNumber < hitForce.Length)
                {
                    cameraShake.intensity = hitForce[hitNumber];
                    cameraShake.Shake();
                }
                else
                {
                    // death TODO
                }
                isHit = true;   // Will be used to give the play iFrames
                SoundManager.Instance.PlayOneShot(SoundManager.Instance.hurt);  // Plays the hurst sound
            }
            alien.Die();    // Kills the alien after it hits the player.
        }
    }
}

using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //Movement variables
    public float moveSpeed = 5;
    public float sprintFactor = 1.5f;

    //A 'helper variable' to store the calculation of current speed +
    //sprint
    private float sprintSpeed;

    //Jumping! The numbers are a bit smaller than with rigidbodies!
    public float jumpHeight = 1.5f;
    public float lowJumpFraction = 0.5f;    //Tap vs hold. 1/2 height for tap!
    public float fastFallMultiplier = 1.6f; //Speed up on descent
    public Vector3 gravity; //We are going to use Unity's default gravity!

    //Another helper to calculate velocity!
    private Vector3 velocity;

    //========CAMERA STUFF========//
    public Transform cameraTransform;
    public float mouseSensitivity = 5;

    //Helper to store rotation
    private float lookRotation = 0;

    //CharacterController Component
    private CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {
        //Set our gravity variable to Unity's gravity.
        gravity = Physics.gravity;

        controller = GetComponent<CharacterController>();

        //Set how to calculate sprint speed.
        sprintSpeed = moveSpeed * sprintFactor;

        //Make the cursor disappear. ESC to bring it back
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Once per frame, call these two functions
        MouseLook();
        Movement();  
    }

    void GenerateRandom()
    {
        
    }

    void MouseLook()
    {
        //Input has built-in Mouse X and Mouse Y.
        //Which are -1, 0, or 1 (too slow), so we multiply by mouseSensitivity
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        //Only the camera can move up and down
        lookRotation -= mouseY;

        //Clamp lookRotation so the camera can't rotate below -85 or past 85
        lookRotation = Mathf.Clamp(lookRotation, -85f, 85f);

        //REMEMBER: rotation is the axis it is rotating around, not the direction
        //of the rotation!!!!!!!!!!!!!
        //Camera is moving up and down, therefore rotate around x!
        cameraTransform.localRotation = Quaternion.Euler(lookRotation, 0, 0);

        //Moving left and right is easier because the player can rotate that way
        //Vector3.up = (0, 1, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Movement()
    {
        //Our own variable; just to save on typing time.
        bool isGrounded = controller.isGrounded;

        //This is to make sure the player sticks on the ground. This is a failsafe.
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        //Store horizontal and vertical movement as floats (-1, 0, 1)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Calculate the target move direction
            //The player's right transform * whatever x is
            //Plus the forward transform * whatever z is
        Vector3 move = transform.right * x + transform.forward * z;

        //Store currentSpeed first
        float currentSpeed = moveSpeed;

        //For the sprint button
        if (Input.GetKey(KeyCode.T) && Mathf.Abs(z) > 0.1f)
        {
            currentSpeed = sprintSpeed;
        }

        //We now have direction and speed, which is what we need to move
        //So, we can move there. Yay!
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            //This is a kinematic formula (AKA Suvat formula)
            //"What does this value need to be to get to that point?"
            //v^2 = u^2 + 2(as) where v = final velocity
            //u = initial velocity, a = acceleration and s = speed
            //We have our current velocity, jump height and gravity already
            //Flip it, and you use a square root to get the final value
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity.y);
        }

        //If the player taps the jump button vs holding it, it is 50%
        if (velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            //Use the lowJumpFraction
            velocity.y += gravity.y * lowJumpFraction * Time.deltaTime;
        }

        //Speed up on descent
        if (velocity.y < 0)
        {
            //Use the fast fall multiplier. -1 because it is 1.6
            velocity.y += gravity.y * (fastFallMultiplier - 1) * Time.deltaTime;
        }

        //After the math is done, apply that to velocity
        velocity.y += gravity.y * Time.deltaTime;

        //Helper variable for movement on x and z, and gravity
        Vector3 moveCalc = move * currentSpeed + Vector3.up * velocity.y;

        //Apply all of that to the CharacterController
        //???
        //Profit.
        controller.Move(moveCalc * Time.deltaTime);
    }
}

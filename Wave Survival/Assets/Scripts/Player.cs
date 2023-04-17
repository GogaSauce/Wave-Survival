using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    CharacterController controller;
    InputMaster input;
    public float speed;
    public Transform cam;
    Rigidbody rb;
    Vector3 moveDir;
    public float smoothTime;
    float smoothVelocity;
    Vector3 velocity;
    float gravity = -9.81f;
    Animator anim;
    bool isAiming;
    public bool inHand = true;
    public GameObject axe;
    public float axeSpeed;
    public float rotation;
    public Transform target, centerPoint, aimObj;
    Vector3 oldPos;
    bool isReturning = false;
    Rigidbody axeRig;
    public float time = 0f;
    public float angle;
    public GameObject mainCM, aimCM, canvas;
    GameObject hitObj;
    public float comboInterval;
    public float clicks = 0;
    AnimatorStateInfo stateInfo;
    public float lastClickedTime;
    // Start is called before the first frame update
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = new InputMaster();
        rb = GetComponent<Rigidbody>();
        input.Player.Movement.performed += ctx => Movement(ctx.ReadValue<Vector2>());
        input.Player.Movement.canceled += ctx =>
        {
            moveDir = Vector3.zero;
            anim.SetBool("walking", false);
        };
        input.Player.Aim.performed += ctx =>
        {
            isAiming = true;
            anim.SetBool("isAiming", true);

        };
        input.Player.Aim.canceled += ctx =>
        {
            isAiming = false;
            anim.SetBool("isAiming", false);

        };
        input.Player.Throw.performed += ctx =>
        {
            if (isAiming && inHand)
            {
                anim.SetTrigger("isThrow");
            }
        };
        input.Player.Recall.performed += ctx => RecallAxe();
        Cursor.lockState = CursorLockMode.Locked;
        anim = GetComponent<Animator>();
        axeRig = axe.GetComponent<Rigidbody>();
        input.Player.Attack.performed += ctx => ComboAttack();
    }


    //Creates an attack sequence similar to God Of War
    public void ComboAttack(){ 
       
        if (!isAiming)
        {
            clicks++;
            clicks = Mathf.Clamp(clicks, 0f, 3f);
            
             if (clicks == 0)
             {
                 anim.SetBool("hit1", false);
                 anim.SetBool("hit2", false);
                 anim.SetBool("hit3", false);

             }

             if (clicks == 1)
             {
                anim.SetTrigger("hit1");
                 lastClickedTime = Time.time;
                 

             }
             if (clicks >= 2)
             {
                 if (stateInfo.IsName("firstAttack") && stateInfo.normalizedTime >= 0.1f)
                 {
                     anim.SetTrigger("hit2");
                     lastClickedTime = Time.time;
                 }

             } 
             if(clicks == 3)
            {
                
                anim.SetTrigger("hit3");
                lastClickedTime = Time.time;
                clicks = 0;
            }

        }
    }

    public void ThrowAxe()
    {

        if (isAiming && inHand)
        {
            axe.GetComponent<Axe>().activated = true;
            axe.transform.SetParent(null);
            axeRig.isKinematic = false;
            Vector3 throwDirection = Camera.main.transform.forward;

            axeRig.AddForce(throwDirection * axeSpeed, ForceMode.Impulse);

            inHand = false;
        }
    }

    void Movement(Vector2 dir)
    {
        moveDir = new Vector3(dir.x, 0f, dir.y);
        anim.SetBool("walking", true);
    }
    void RecallAxe()
    {
        oldPos = axe.transform.position;
        isReturning = true;
        axeRig.velocity = Vector3.zero;
        axeRig.isKinematic = true;
    }
    private void OnEnable()
    {
        input.Player.Enable();
    }
    private void OnDisable()
    {
        input.Player.Disable();

    }
    private void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (Time.time - lastClickedTime > comboInterval)
        {
            clicks = 0f;
            anim.SetBool("isIdle", true);
           
        }
        
            if (moveDir.magnitude >= 0.1f)
            {
                //Get target angle to rotate towards
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                //Apply target angle to object rotation
                float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetAngle, ref smoothVelocity, smoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 dir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(dir.normalized * speed * Time.deltaTime);
            }
            if (isAiming)
            {
                //Get target angle to rotate towards
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                //Apply target angle to object rotation
                float angle = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetAngle, ref smoothVelocity, smoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            if (isReturning)
            {
                if (time < 1f)
                {
                    //as time increases, the bezier curve is more pronounced and the axe's position
                    //will be set to all of the points on that path until it returns(time == one)
                    axe.transform.position = getBQCPoint(time, oldPos, centerPoint.position, target.position);
                    time += Time.deltaTime;
                    axe.transform.Rotate(Vector3.right * rotation, angle);
                }
            }
            if (time >= 1)
            {
                isReturning = false;
                axe.transform.SetParent(target);
                inHand = true;
                axe.transform.localPosition = Vector3.zero;
                axe.transform.rotation = target.rotation;
                time = 0f;

            }

            if (isAiming)
            {

                mainCM.SetActive(false);
                aimCM.SetActive(true);
                canvas.SetActive(true);

            }
            else
            {
                mainCM.SetActive(true);
                aimCM.SetActive(false);
                canvas.SetActive(false);

            }

            if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsName("firstAttack"))
            {
                anim.SetBool("hit1", false);
            }
            else if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsName("secondAttack"))
            {
                anim.SetBool("hit2", false);
            }
            else if (stateInfo.normalizedTime >= 1.0f && stateInfo.IsName("lastAttack"))
            {
                anim.SetBool("hit3", false);
            }


        }
        Vector3 getBQCPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // "t" is always between 0 and 1, so "u" is other side of t
            // If "t" is 1, then "u" is 0
            float u = 1 - t;
            // "t" square
            float tt = t * t;
            // "u" square
            float uu = u * u;
            // this is the formula in one line
            // (u^2 * p0) + (2 * u * t * p1) + (t^2 * p2)
            Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);
            return p;
        }
 }


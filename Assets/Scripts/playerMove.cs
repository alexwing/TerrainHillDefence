using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class playerMove : MonoBehaviour
{
    //NavMeshAgent agent;
    [SerializeField]

    public float speed = 115;
    public float maxSpeed = 10;
    public float minSpeed = 0.5f;
    public float acceleration = 5;
    public float deceleration = 5;
    public float maxDeceleration = 10;
    public float maxAcceleration = 10;
    public float maxAngularSpeed = 10;


    [SerializeField]
    public static bool is_visible_zproia = false;
    private Ray ray;
    private RaycastHit hit;
    private Animator animator;
    private Vector3 movePosition;
    [SerializeField]
    private AudioClip clipAtaka;

    private Rigidbody rb;
    [SerializeField]
    private AudioClip clipStep;
    // Start is called before the first frame update
    void Start()
    {
   //     agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {            
        if (Input.GetMouseButtonDown(0))
        {

            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f))
            {
                //walks to the position of the hit
                movePosition = hit.point;
                rb.velocity = Vector3.zero;
                rb.AddForce(transform.forward * speed, ForceMode.Impulse);                

                animator.SetBool("is_run", (int) rb.velocity.magnitude >0);



            }

            movePosition = Input.mousePosition;
            ray = Camera.main.ScreenPointToRay(movePosition);
            print(ray);


          
        }
      //  animator.SetBool("is_run", (int)agent.velocity.magnitude > 0);
      
        if (Input.GetMouseButtonDown(1))
        {
            //print(clipAtaka);
         //   audio_control.playClip(clipAtaka);
            animator.SetTrigger("is_ataka");
        }

      
    }
    void Step()
    {
     //   audio_control.playClip(clipStep);
    }
}
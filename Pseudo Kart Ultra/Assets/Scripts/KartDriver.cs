using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartDriver : MonoBehaviour
{
    Rigidbody rb;
    SphereCollider sc;

    [Header("Suspension")]
    [SerializeField] private float riseLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;
    [SerializeField] private float springConstant;
    [SerializeField] private float damperConstant;

    private float[] lastLengths = new float[4], springLengths = new float[4];
    private Vector3[] rayOrigin = new Vector3[4];
    private Vector3 suspensionForce;

    private float springForce, springVel, damperForce;
    private float maxLength, minLength, dim;

    [Header("Driving & Steering")]
    [SerializeField] private float maxSpeed;
    [SerializeField] private float steerConst;

    private Vector3 driveVel, steerVect;

    private float steerDir, steerVal;
    private float currentSpeed = 0;
    private float realSpeed;

    [Header("Drifting & Boosting")]
    [SerializeField] private float minDriftSpeed;
    [SerializeField] private float centriForce;

    private bool rightDrift, leftDrift, isDrifting;
    private float driftTime, boostTime, boostSpeed;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sc = GetComponent<SphereCollider>();

        dim = sc.radius;

        riseLength = riseLength >= dim ? riseLength : dim / 2f + riseLength;
        minLength = riseLength - springTravel;
        maxLength = riseLength + springTravel;

        rightDrift = leftDrift = isDrifting = false;
        boostSpeed = maxSpeed + 20;
    }

    void FixedUpdate()
    {
        suspensionPhysics();

        realSpeed = transform.InverseTransformDirection(rb.velocity).z;
        steerDir = Input.GetAxisRaw("Horizontal");

        steerVal = realSpeed / (realSpeed > 30 ? 4 : 1.5f) * steerDir * steerConst;
        steerVect = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + steerVal, transform.eulerAngles.z);

        if(Input.GetKey(KeyCode.UpArrow))
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime / 2f);
        else if(Input.GetKey(KeyCode.DownArrow))
            currentSpeed = Mathf.Lerp(currentSpeed, -maxSpeed / 1.75f, 1f * Time.deltaTime);
        else
            currentSpeed = Mathf.Lerp(currentSpeed, 0, Time.deltaTime * 1.5f);
        
        driveVel = transform.forward * currentSpeed;
        driveVel.y = rb.velocity.y;
        rb.velocity = driveVel;

        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, steerVect, 3f * Time.deltaTime);
    }

    void suspensionPhysics()
    {
        rayOrigin[0] = transform.position + (transform.right + transform.forward) * dim;
        rayOrigin[1] = transform.position - (transform.right - transform.forward) * dim;
        rayOrigin[2] = transform.position + (transform.right - transform.forward) * dim;
        rayOrigin[3] = transform.position - (transform.right + transform.forward) * dim;

        for (int i = 0; i < rayOrigin.Length; i++)
        {
            RaycastHit hit;
            Physics.Raycast(rayOrigin[i], -transform.up, out hit, maxLength + wheelRadius);
            if (hit.collider != null)
            {
                lastLengths[i] = springLengths[i];
                springLengths[i] = Mathf.Clamp(hit.distance - wheelRadius, minLength, maxLength);

                springVel = (lastLengths[i] - springLengths[i]) / Time.fixedDeltaTime;
                springForce = (riseLength - springLengths[i]) * springConstant;
                damperForce = damperConstant * springVel;

                suspensionForce = (springForce + damperForce) * transform.up;
                rb.AddForceAtPosition(suspensionForce, rayOrigin[i]);

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2f, hit.normal) 
                    * transform.rotation, 7.5f * Time.deltaTime);
                isGrounded = true;
            }
            else
                isGrounded = false;
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        for (int i = 0; i < rayOrigin.Length; i++)
        {
            Gizmos.DrawRay(rayOrigin[i], -transform.up * (maxLength + wheelRadius));
        }
    }
}

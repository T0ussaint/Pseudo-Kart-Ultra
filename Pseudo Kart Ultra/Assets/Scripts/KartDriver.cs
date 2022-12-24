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

        if(leftDrift && !rightDrift)
        {
            steerDir = Input.GetAxis("Horizontal") < 0 ? -1.5f : -0.5f;
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, 
                Quaternion.Euler(0, -20f, 0), 8f * Time.deltaTime);
            
            if(isDrifting && isGrounded)
                rb.AddForce(transform.right * centriForce * Time.deltaTime, ForceMode.Acceleration);
        }
        else if(rightDrift && !leftDrift)
        {
            steerDir = Input.GetAxis("Horizontal") > 0 ? 1.5f : 0.5f;
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation, 
                Quaternion.Euler(0, 20f, 0), 8f * Time.deltaTime);
            
            if(isDrifting && isGrounded)
                rb.AddForce(transform.right * -centriForce * Time.deltaTime, ForceMode.Acceleration);
        }
        else
            transform.GetChild(0).localRotation = Quaternion.Lerp(transform.GetChild(0).localRotation,
                Quaternion.Euler(0, 0, 0), 8f * Time.deltaTime);

        driftMechanics();

        steerVal = realSpeed / (realSpeed > 30 ? 4 : 1.5f) * steerDir * steerConst;
        steerVect = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + steerVal, transform.eulerAngles.z);

        if (Input.GetKey(KeyCode.UpArrow))
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, Time.deltaTime / 2f);
        else if (Input.GetKey(KeyCode.DownArrow))
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

    void driftMechanics()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded && steerDir != 0f)
        {
            Debug.Log("Drifting");
            leftDrift = !(rightDrift = steerDir > 0 ? true : false);
            isDrifting = true;

            if(currentSpeed > minDriftSpeed && Input.GetAxis("Horizontal") != 0)
                driftTime += Time.deltaTime;
        }

        if(!Input.GetKey(KeyCode.Space) || realSpeed < minDriftSpeed)
        {
            leftDrift = rightDrift = isDrifting = false;

            if(driftTime > 1.5 && driftTime < 4)
                boostTime = 0.75f;
            if(driftTime >= 4 && driftTime < 7)
                boostTime = 1.5f;
            if(driftTime >= 7)
                boostTime = 2.5f;
        }
    }

    public float getBoostTime()
    {
        return boostTime;
    }
}

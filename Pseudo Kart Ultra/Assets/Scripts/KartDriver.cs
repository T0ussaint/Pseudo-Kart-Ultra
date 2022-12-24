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

    private float springForce, springVel, damperForce;
    private float maxLength, minLength, dim;
    private Vector3 suspensionForce;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sc = GetComponent<SphereCollider>();

        dim = sc.radius;

        riseLength = riseLength > dim ? riseLength : dim / 2f + riseLength;
        minLength = riseLength - springTravel;
        maxLength = riseLength + springTravel;
    }

    void FixedUpdate(){
        suspensionPhysics();
    }

    void suspensionPhysics(){
        rayOrigin[0] = transform.position + (transform.right + transform.forward) * dim;
        rayOrigin[1] = transform.position - (transform.right - transform.forward) * dim;
        rayOrigin[2] = transform.position + (transform.right - transform.forward) * dim;
        rayOrigin[3] = transform.position - (transform.right + transform.forward) * dim;

        for (int i = 0; i < rayOrigin.Length; i++)
        {
            RaycastHit hit;
            Physics.Raycast(rayOrigin[i], -transform.up, out hit, maxLength + wheelRadius);
            if(hit.collider != null)
            {
                Debug.Log(hit.collider.name);
                lastLengths[i] = springLengths[i];
                springLengths[i] = Mathf.Clamp(hit.distance - wheelRadius, minLength, maxLength);

                springVel = (lastLengths[i] - springLengths[i]) / Time.fixedDeltaTime;
                springForce = (riseLength - springLengths[i]) * springConstant;
                damperForce = damperConstant * springVel;

                suspensionForce = (springForce + damperForce) * transform.up;
                rb.AddForceAtPosition(suspensionForce, rayOrigin[i]);

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(transform.up * 2f, hit.normal) * transform.rotation, 7.5f * Time.deltaTime);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public Vector3 offset;
    public Transform player;

    private KartDriver kd;

    public Vector3 origCamPos;
    public Vector3 boostCamPos;

    // Start is called before the first frame update
    void Start()
    {
        kd = player.GetComponent<KartDriver>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.position + offset;

        //if (!kd.GLIDER_FLY)
            transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, 3 * Time.deltaTime); //normal
       // else
           // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, player.eulerAngles.y, 0), 3 * Time.deltaTime); //we only want the camera rotate on the y axis. Not the x or z axis since when the player is turning in the air while gliding, the player is tilting a lot, so we do not want the camera to tilt sideways too.

        if (kd.getBoostTime() > 0)
        {
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, boostCamPos, 3 * Time.deltaTime);
        }
        else
        {
            transform.GetChild(0).localPosition = Vector3.Lerp(transform.GetChild(0).localPosition, origCamPos, 3 * Time.deltaTime);
        }
    }
}
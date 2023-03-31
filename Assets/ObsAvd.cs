using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsAvd : MonoBehaviour
{
    //Vector3 Motion = Vector3.zero;
    //Vector3 LastPos = Vector3.zero;
    Vector3 Target = new Vector3(1, 1, -2);
    DroneAction MyDrone;
    bool Avoiding = false;

    // Start is called before the first frame update
    void Start()
    {
        MyDrone = GetComponent<DroneAction>();
    }

    // Update is called once per frame
    void Update()
    {
#if true
        var toTgt = Target - transform.position;
        if (toTgt.sqrMagnitude > 0.001f)
        {
            Debug.DrawRay(transform.position, toTgt);
            if (Physics.Raycast(transform.position, toTgt, 1.5f))
            {
                var quat = Quaternion.AngleAxis(5, Vector3.up);
                do
                {
                    toTgt = quat * toTgt;
                } while (Physics.Raycast(transform.position, toTgt, 1.5f));
                var vel = toTgt.normalized * 0.5f;
                MyDrone.SetVelcoty(vel);
                Avoiding = true;
                Debug.DrawRay(transform.position, vel, Color.red);
            }
            else if (Avoiding)
            {
                Avoiding = false;
                MyDrone.SetTarget(Target);
            }
        }        
#else
        Motion = transform.position - LastPos;

        if (Motion.sqrMagnitude > 0.00001f)
        {
            Debug.DrawRay(transform.position, Motion.normalized, Color.red);
            
            if (Physics.Raycast(transform.position, Motion, 1.5f))
            {
                //Debug.Log("obstacle " + transform.position);
                var quat = Quaternion.AngleAxis(5, Vector3.up);
                do
                {
                    Motion = quat * Motion;
                } while (Physics.Raycast(transform.position, Motion, 1.5f));
                //Debug.Log("avoid motion " + Motion);
                var vel = Motion.normalized * 0.5f;
                MyDrone.SetVelcoty(-vel.x, vel.z);
            }
        }

        LastPos = transform.position;
#endif
    }
}

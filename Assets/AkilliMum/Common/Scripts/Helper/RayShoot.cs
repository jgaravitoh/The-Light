using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace AkilliMum
{
    public class RayShoot : MonoBehaviour
    {
        public Vector3 RayDirection = Vector3.forward;
        public GameObject PutOnHit;
        private Vector3 _startAngle;
        public Vector3 HitToFitPosition = new Vector3(0, 0, 0);
        public bool IsDebug = false;

        void Start()
        {
            if(PutOnHit != null)
                _startAngle = PutOnHit.transform.eulerAngles;
        }

        // See Order of Execution for Event Functions for information on FixedUpdate() and Update() related to physics queries
        void LateUpdate()
        {
            if (PutOnHit == null)
                return;
            //// Bit shift the index of the layer (8) to get a bit mask
            //int layerMask = 1 << 8;

            //// This would cast rays only against colliders in layer 8.
            //// But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
            //layerMask = ~layerMask;

            RaycastHit hit1;
            RaycastHit hit2;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(RayDirection), out hit1,
                    100))
            {
                if (IsDebug)
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(RayDirection) * hit1.distance,
                        Color.yellow);
                    Debug.Log("Did Hit on 1: " + hit1.point);
                }

                //float3 newPos = new float3(
                //    HitToFitPosition.x > 0 ? hit.point.x : PutOnHit.transform.position.x,
                //    HitToFitPosition.y > 0 ? hit.point.y : PutOnHit.transform.position.y,
                //    HitToFitPosition.z > 0 ? hit.point.z : PutOnHit.transform.position.z);
                //PutOnHit.transform.position = newPos;

                //move back as the difference of the rayShoot object and the real object and fire a hit again
                var distance = Mathf.Abs(2 * (transform.position.z - PutOnHit.transform.position.z));
                var moveALittleBack = new Vector3(transform.position.x, transform.position.y, transform.position.z - distance);

                if (Physics.Raycast(moveALittleBack, transform.TransformDirection(RayDirection), out hit2,
                        100))
                {
                    if (IsDebug)
                    {
                        Debug.DrawRay(moveALittleBack, transform.TransformDirection(RayDirection) * hit2.distance,
                            Color.yellow);
                        Debug.Log("Did Hit on 2: " + hit2.point);
                    }

                    //find the angle via atan
                    var zDifference = hit1.point.z - hit2.point.z;
                    var yDifference = hit1.point.y - hit2.point.y;
                    var angle = Mathf.Atan(zDifference / yDifference);

                    if (IsDebug)
                    {
                        Debug.Log("Angle between hits: " + angle);
                    }

                    float3 newPos = new float3(
                        HitToFitPosition.x > 0 ? (hit1.point.x + hit2.point.x) / 2 : PutOnHit.transform.position.x,
                        HitToFitPosition.y > 0 ? (hit1.point.y + hit2.point.y) / 2 : PutOnHit.transform.position.y,
                        HitToFitPosition.z > 0 ? (hit1.point.z + hit2.point.z) / 2 : PutOnHit.transform.position.z);
                    PutOnHit.transform.position = newPos;
                    if(zDifference > 0)
                        PutOnHit.transform.eulerAngles = _startAngle + new Vector3(-angle, 0, 0);
                    else
                        PutOnHit.transform.eulerAngles = _startAngle + new Vector3(angle, 0, 0);
                }

            }
            else
            {
                if (IsDebug)
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(RayDirection) * 1000, Color.white);
                    Debug.Log("Did not Hit");
                }
            }
        }
    }
}
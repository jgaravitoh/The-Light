using UnityEngine;

namespace AkilliMum
{
    public class CameraFollow : MonoBehaviour
    {

        public Transform Target;
        public float distance = 6.4f;
        public float height = 1.4f;
        public float rotationDamping = 3.0f;
        public float heightDamping = 2.0f;
        public float zoomRatio = 0.5f;
        //public float defaultFOV = 60f;

        private Vector3 rotationVector;

        void LateUpdate()
        {
            float wantedAngle = rotationVector.y;
            float wantedHeight = Target.position.y + height;
            float myAngle = transform.eulerAngles.y;
            float myHeight = transform.position.y;

            myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.deltaTime);
            myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.deltaTime);

            Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);
            transform.position = Target.position;
            transform.position -= currentRotation * Vector3.forward * distance;
            Vector3 temp = transform.position; //temporary variable so Unity doesn't complain
            temp.y = myHeight;
            transform.position = temp;
            transform.LookAt(Target);
        }

        void FixedUpdate()
        {
            Vector3 localVelocity = Target.InverseTransformDirection(Target.GetComponent<Rigidbody>().linearVelocity);
            if (localVelocity.z < -0.1f)
            {
                Vector3 temp = rotationVector; //because temporary variables seem to be removed after a closing bracket "}" we can use the same variable name multiple times.
                temp.y = Target.eulerAngles.y + 180;
                rotationVector = temp;
            }
            else
            {
                Vector3 temp = rotationVector;
                temp.y = Target.eulerAngles.y;
                rotationVector = temp;
            }
            //float acc = Target.GetComponent<Rigidbody>().linearVelocity.magnitude;
            //GetComponent<Camera>().fieldOfView = defaultFOV + acc * zoomRatio * Time.deltaTime;  //he removed * Time.deltaTime but it works better if you leave it like this.
        }
    }
}
using UnityEngine;
using System.Collections;

namespace AkilliMum
{
    public class TimeRotater : MonoBehaviour
    {
        public Vector3 direction = Vector3.right;
        public float Speed = 5;
        
        void Update()
        {
            var speed = Speed;

            gameObject.transform.RotateAround(gameObject.transform.position,
                direction,
                speed * Time.frameCount);
        }
    }
}

using UnityEngine;

namespace AkilliMum
{
    public class SyncPosition : MonoBehaviour
    {

        public Transform Target;
        public bool SyncX;
        public bool SyncY;
        public bool SyncZ;

        void Update()
        {
            var position = transform.position;
            if (SyncX)
                position.x = Target.transform.position.x;
            if (SyncY)
                position.y = Target.transform.position.y;
            if (SyncZ)
                position.z = Target.transform.position.z;
            transform.position = position;
        }
    }
}
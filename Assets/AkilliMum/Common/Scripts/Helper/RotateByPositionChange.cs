using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

namespace AkilliMum
{
    public class RotateByPositionChange : MonoBehaviour
    {
        public float Speed;
        public RotateByPositionChangeObject[] ToRotate;

        private Vector3 _previousPosition;
        private float _distance;

        void Start()
        {
            //get the start point as the start position
            _previousPosition = gameObject.transform.position;
        }

        void Update()
        {
            var currentPosition = gameObject.transform.position;
            _distance = currentPosition.z - _previousPosition.z;
            _previousPosition = gameObject.transform.position;

            foreach (var obj in ToRotate)
            {
                obj.ToRotate.transform.Rotate(obj.Direction, _distance * Speed);
            }
        }
    }

    [Serializable]
    public class RotateByPositionChangeObject
    {
        public GameObject ToRotate;
        public Vector3 Direction;
    }
}

using AkilliMum.Touches;
using UnityEngine;

namespace AkilliMum.SRP.CarPaint
{
    public class SimpleTouchRotater : MonoBehaviour
    {
        public GameObject ToRotate;
        public bool RotateObject = false;

        private static TKMovementRecognizer _recognizer;

        void Start()
        {
            RegisterTouchEvents();
        }

        protected void RegisterTouchEvents()
        {
            if (ToRotate == null)
                return;

            if (_recognizer != null)
                TouchKit.removeGestureRecognizer(_recognizer);

            //add touch to selected car
            _recognizer = new TKMovementRecognizer();
            _recognizer.boundaryFrame = TouchKit.instance.getRect();
            //var startAngle = car.gameObject.transform.eulerAngles;
            //recognizer.targetPosition = Camera.main.WorldToScreenPoint(_selectedCar.transform.position);
            _recognizer.gestureMovingEvent += (r) =>
            {
                var distance = (r.LastPosition.x - r.FirstPosition.x);
                r.FirstPosition = r.LastPosition;
                if (RotateObject)
                {
                    ToRotate.transform.Rotate(Vector3.up, -distance * 0.1f);
                }
                else
                {
                    Camera.main.transform.RotateAround(ToRotate.transform.position,
                        Vector3.up, distance * 0.1f);
                }

            };
            _recognizer.gestureCompleteEvent += (r) =>
            {
                //r.FirstPosition = r.LastPosition;
            };
            TouchKit.addGestureRecognizer(_recognizer);
        }
    }
}
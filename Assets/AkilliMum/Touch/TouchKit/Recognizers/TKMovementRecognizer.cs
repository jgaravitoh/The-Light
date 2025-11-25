using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace AkilliMum.Touches
{
    /// <summary>
    /// detects a movement continuesly with a single finger
    /// </summary>
    public class TKMovementRecognizer : TKAbstractGestureRecognizer
    {
        public event Action<TKMovementRecognizer> gestureRecognizedEvent;
        public event Action<TKMovementRecognizer> gestureMovingEvent;
        public event Action<TKMovementRecognizer> gestureCompleteEvent;

        public Vector2 FirstPosition { get; set; }
        public Vector2 LastPosition { get; set; }

        internal override void fireRecognizedEvent()
        {
            if (gestureRecognizedEvent != null)
                gestureRecognizedEvent(this);
        }


        internal override bool touchesBegan(List<TKTouch> touches)
        {
            if (state == TKGestureRecognizerState.Possible)
            {
                _trackingTouches.Add(touches[0]);

                FirstPosition = _trackingTouches[0].position;
                LastPosition = _trackingTouches[0].position;
                
                state = TKGestureRecognizerState.Began;
            }

            return false;
        }


        internal override void touchesMoved(List<TKTouch> touches)
        {
            if (state == TKGestureRecognizerState.RecognizedAndStillRecognizing ||
                state == TKGestureRecognizerState.Began)
            {
                LastPosition = _trackingTouches[0].position;
                state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
                if (gestureMovingEvent != null)
                    gestureMovingEvent(this);
            }
        }


        internal override void touchesEnded(List<TKTouch> touches)
        {
            // if we had previously been recognizing fire our complete event
            if (state == TKGestureRecognizerState.RecognizedAndStillRecognizing)
            {
                if (gestureCompleteEvent != null)
                    gestureCompleteEvent(this);
            }

            state = TKGestureRecognizerState.FailedOrEnded;
        }

    }
}
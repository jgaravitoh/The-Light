using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
namespace AkilliMum
{
    [RequireComponent(typeof(Button))]
    public class ButtonPressListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private AudioSource _clickSoundAudioSource;
        public AudioClip ClickSoundAudioClip;

        public UnityEvent onPressDown;
        public UnityEvent onPressUp;
        
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();

            _clickSoundAudioSource = gameObject.AddComponent<AudioSource>();
            _clickSoundAudioSource.clip = ClickSoundAudioClip;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button.interactable)
            {
                if (onPressDown != null)
                {
                    onPressDown.Invoke();
                    
                    _clickSoundAudioSource.Stop();
                    _clickSoundAudioSource.Play();
                }
            }
        }


        public void OnPointerUp(PointerEventData eventData)
        {
            if (button.interactable)
                onPressUp?.Invoke();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using AkilliMum.SRP.Mirror;
using UnityEngine.UI;

namespace AkilliMum
{
    public class ReflectionChanger : MonoBehaviour
    {
        public MirrorManager mirrorManager;
        public Toggle reflectionToggle;
        public GameObject ground;

        void Start()
        {
            reflectionToggle.onValueChanged.AddListener(delegate {
               OnChange(); 
            });
        }

        public void OnChange()
        {
            if (reflectionToggle.isOn)
            {
                mirrorManager.IsEnabled = true;
                ground.GetComponent<Renderer>().material.SetFloat("_ReflectionIntensity", 1);
            }
            else
            {
                mirrorManager.IsEnabled = false;
                ground.GetComponent<Renderer>().material.SetFloat("_ReflectionIntensity", 0);
            }
        }
    }
}
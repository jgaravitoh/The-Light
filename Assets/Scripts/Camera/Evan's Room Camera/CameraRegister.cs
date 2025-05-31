using UnityEngine;
using Unity.Cinemachine;

public class CameraRegister : MonoBehaviour
{
    private void OnEnable()
    {
        CameraSwitcher.Register(GetComponent<CinemachineCamera>());
    }

    private void OnDisable()
    {
        CameraSwitcher.Unregister(GetComponent<CinemachineCamera>());
    }
}

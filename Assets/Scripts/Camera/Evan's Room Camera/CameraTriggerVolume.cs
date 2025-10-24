using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using UnityEngine.Rendering;
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class CameraTriggerVolume : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private Vector3 boxSize;
    [SerializeField] private bool restrictMovement;

    BoxCollider box;
    Rigidbody rb;
    private void Awake()
    {
        box = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        box.isTrigger = true;
        box.size = boxSize;

        rb.isKinematic = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered Camera Trigger Volume");
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Entered Camera Trigger Volume");
            if (CameraSwitcher.activeCamera != cam) CameraSwitcher.SwitchCamera(cam); Debug.Log(cam.Name); 
            if (restrictMovement) PlayerMovement.sharedInstancePlayerMovement.allowMovement = false;
        }
    }
}

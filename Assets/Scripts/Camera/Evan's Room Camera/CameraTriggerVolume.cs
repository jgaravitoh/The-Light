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
        if (other.gameObject.CompareTag("Player"))
        {
            if (CameraSwitcher.activeCamera != cam) CameraSwitcher.SwitchCamera(cam);
            if (restrictMovement) PlayerMovement.sharedInstancePlayerMovement.allowMovement = false;
        }
    }
}

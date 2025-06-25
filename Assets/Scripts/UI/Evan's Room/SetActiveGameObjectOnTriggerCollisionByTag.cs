using UnityEngine;

public class SetActiveGameObjectOnTriggerCollisionByTag : MonoBehaviour
{
    [SerializeField] private GameObject _gameObject;
    [SerializeField] private string _targetTag;
    [SerializeField] private bool _setActiveBool;
    [SerializeField] private float _activationTime = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_targetTag))
        {
            Invoke(nameof(InvokeActivation), _activationTime);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_targetTag))
        {
            Invoke(nameof(InvokeActivation), _activationTime);
        }
    }

    public void InvokeActivation()
    {
        _gameObject.SetActive(_setActiveBool);
    }

}

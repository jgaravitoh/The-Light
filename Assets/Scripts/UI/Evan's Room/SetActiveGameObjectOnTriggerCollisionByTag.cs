using UnityEngine;

public class SetActiveGameObjectOnTriggerCollisionByTag : MonoBehaviour
{
    [SerializeField] private GameObject _gameObject;
    [SerializeField] private string _targetTag;
    [SerializeField] private bool _setActiveBool;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_targetTag))
        {
            _gameObject.SetActive(_setActiveBool);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_targetTag))
        {
            _gameObject.SetActive(_setActiveBool);
        }
    }
}

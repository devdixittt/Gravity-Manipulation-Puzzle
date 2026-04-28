using UnityEngine;

public class CollectibleCube : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private float rotateSpeed = 90f;
    //[SerializeField] private GameObject collectFX; // Optional particle effect

    private bool _collected = false;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;
        GameManager.Instance?.RegisterCubeCollected();

        /*if (collectFX != null)
            Instantiate(collectFX, transform.position, Quaternion.identity);*/

        Destroy(gameObject);
    }
}

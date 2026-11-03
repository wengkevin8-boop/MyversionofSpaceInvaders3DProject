using UnityEngine;

public class Dart : MonoBehaviour
{
    [SerializeField] float speed = 16f;
    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        transform.position += Vector3.up * speed * Time.deltaTime;
        if (transform.position.y > 12f) Destroy(gameObject);
    }
}

using UnityEngine;

public class AlienBullet : MonoBehaviour
{
    [Header("Inscribed")]
    [SerializeField] float speed = 8f;
    [SerializeField] float destroyBelowY = -10f;

    bool hasHit;

    void Update()
    {
        GameManager game = GameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        transform.position += Vector3.down * speed * Time.deltaTime;

        if (transform.position.y < destroyBelowY)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;

        Spaceship ship = collision.gameObject.GetComponent<Spaceship>();

        if (ship == null)
            return;

        GameManager game = GameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        hasHit = true;
        game.LoseLife();

        Destroy(gameObject);
    }
}

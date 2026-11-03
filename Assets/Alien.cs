using UnityEngine;

public class Alien : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public AlienFleet fleet;

    [SerializeField] private float minimumShotDelay = 5f;
    [SerializeField] private float maximumShotDelay = 16f;

    private float nextShotTime;
    private bool wasPlaying;

    void Update()
    {
        GameManager game = GameManager.Instance;

        if (game == null || !game.IsPlaying)
        {
            wasPlaying = false;
            return;
        }

        if (!wasPlaying)
        {
            wasPlaying = true;
            ScheduleNextShot();
            return;
        }

        if (bulletPrefab == null) return;
        if (Time.time < nextShotTime) return;

        Instantiate(
            bulletPrefab,
            transform.position,
            Quaternion.identity
        );

        ScheduleNextShot();
    }

    void ScheduleNextShot()
    {
        float minDelay = Mathf.Min(minimumShotDelay, maximumShotDelay);
        float maxDelay = Mathf.Max(minimumShotDelay, maximumShotDelay);

        nextShotTime = Time.time + Random.Range(minDelay, maxDelay);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Dart>() == null)
            return;

        if (fleet != null)
        {
            fleet.AlienDestroyed(gameObject);
        }

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }
}


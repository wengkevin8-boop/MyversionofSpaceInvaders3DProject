using System.Collections.Generic;
using UnityEngine;

public class AlienFleet : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject alienPrefab;
    public AlienCounter alienCounter;
    public List<GameObject> aliens = new List<GameObject>();

    [Header("Fleet Movement")]
    [SerializeField, Min(0f)] float startingSpeed = 2f;
    [SerializeField, Min(0f)] float speedPerAlienDestroyed = 0.12f;
    [SerializeField] float leftBound = -16f;
    [SerializeField] float rightBound = 16f;
    [SerializeField, Min(0f)] float edgePadding = 0.25f;
    [SerializeField] float lossLine = -6f;
    [SerializeField, Min(0f)] float stepDown = 0.6f;
    [SerializeField, Min(0f)] float stepPause = 0.12f;

    [Header("Wave Layout")]
    [SerializeField, Min(1)] int columns = 10;
    [SerializeField, Min(1)] int rows = 3;
    [SerializeField, Min(0f)] float alienSpacing = 2f;
    [SerializeField] float firstRowY = 10f;
    [SerializeField] Camera playfieldCamera;

    float direction = 1f;
    float pauseTimer;
    float alienHalfWidth = 0.5f;

    int wave = 1;
    int startingAlienCount;

    bool movingDown;
    GameManager game;

    void Awake()
    {
        if (playfieldCamera == null)
            playfieldCamera = Camera.main;
    }

    void Start()
    {
        game = GameManager.Instance;

        if (alienCounter == null)
            alienCounter = FindObjectOfType<AlienCounter>();

        SpawnWave();
    }

    void Update()
    {
        RemoveDestroyedAliens();

        if (alienCounter != null)
            alienCounter.score = aliens.Count;

        if (game == null || !game.IsPlaying || aliens.Count == 0)
            return;

        if (HasReachedLossLine())
        {
            game.EndGame(false);
            return;
        }

        if (movingDown)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0f)
                movingDown = false;

            return;
        }

        float speed = Mathf.Max(
            0f,
            startingSpeed
            + (wave - 1) * 0.45f
            + (startingAlienCount - aliens.Count)
            * speedPerAlienDestroyed
        );

        float horizontalStep = direction * speed * Time.deltaTime;

        GetFleetHorizontalExtents(out float left, out float right);
        GetAllowedCenterBounds(out float minCenterX, out float maxCenterX);

        bool reachesEdge = direction > 0f
            ? right + horizontalStep >= maxCenterX
            : left + horizontalStep <= minCenterX;

        if (!reachesEdge)
        {
            MoveFleet(Vector3.right * horizontalStep);
            return;
        }

        // Stop exactly at the boundary.
        float distanceToEdge = direction > 0f
            ? maxCenterX - right
            : minCenterX - left;

        MoveFleet(Vector3.right * distanceToEdge);

        // Reverse direction.
        direction *= -1f;

        // Move the formation downward.
        MoveFleet(Vector3.down * stepDown);

        movingDown = true;
        pauseTimer = stepPause;

        if (HasReachedLossLine())
            game.EndGame(false);
    }

    public void AlienDestroyed(GameObject alien)
    {
        if (!aliens.Remove(alien))
            return;

        if (alienCounter != null)
            alienCounter.score = aliens.Count;

        if (aliens.Count == 0 && game != null)
            game.WaveCleared();
    }

    public void AdvanceWave()
    {
        RemoveDestroyedAliens();

        if (aliens.Count > 0)
            return;

        wave++;

        ResetMovement();

        SpawnWave();
    }

    public void ResetFleet()
    {
        wave = 1;
        ResetMovement();
        startingAlienCount = 0;
    }

    void ResetMovement()
    {
        direction = 1f;
        movingDown = false;
        pauseTimer = 0f;
    }

    public void SpawnWave()
    {
        RemoveDestroyedAliens();

        if (aliens.Count > 0)
        {
            Debug.LogWarning(
                "Cannot spawn a new wave while aliens are alive.",
                this
            );

            return;
        }

        if (alienPrefab == null)
        {
            Debug.LogError(
                "AlienFleet requires an alien prefab.",
                this
            );

            return;
        }

        int safeColumns = Mathf.Max(1, columns);
        int safeRows = Mathf.Max(1, rows);

        startingAlienCount = safeColumns * safeRows;
        alienHalfWidth = MeasureAlienHalfWidth(alienPrefab);

        for (int column = 0; column < safeColumns; column++)
        {
            for (int row = 0; row < safeRows; row++)
            {
                float x = (column - (safeColumns - 1) * 0.5f)
                          * alienSpacing;

                float y = firstRowY - row * alienSpacing;

                GameObject alien = Instantiate(
                    alienPrefab,
                    new Vector3(x, y, 0f),
                    Quaternion.identity
                );

                Alien component = alien.GetComponent<Alien>();

                if (component != null)
                    component.fleet = this;

                aliens.Add(alien);
            }
        }

        if (alienCounter != null)
            alienCounter.score = aliens.Count;
    }

    void RemoveDestroyedAliens()
    {
        for (int i = aliens.Count - 1; i >= 0; i--)
        {
            if (aliens[i] == null)
                aliens.RemoveAt(i);
        }
    }

    void MoveFleet(Vector3 offset)
    {
        foreach (GameObject alien in aliens)
        {
            if (alien != null)
                alien.transform.position += offset;
        }
    }

    void GetFleetHorizontalExtents(
        out float left,
        out float right
    )
    {
        left = float.PositiveInfinity;
        right = float.NegativeInfinity;

        foreach (GameObject alien in aliens)
        {
            if (alien == null)
                continue;

            float x = alien.transform.position.x;

            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x);
        }
    }

    void GetAllowedCenterBounds(
        out float minX,
        out float maxX
    )
    {
        minX = Mathf.Min(leftBound, rightBound) + alienHalfWidth;
        maxX = Mathf.Max(leftBound, rightBound) - alienHalfWidth;

        if (playfieldCamera == null)
            playfieldCamera = Camera.main;

        if (CameraUtils.TryGetHorizontalBounds(
                playfieldCamera,
                aliens[0].transform.position.z,
                out float cameraLeft,
                out float cameraRight))
        {
            cameraLeft += alienHalfWidth + edgePadding;
            cameraRight -= alienHalfWidth + edgePadding;
            minX = Mathf.Max(minX, cameraLeft);
            maxX = Mathf.Min(maxX, cameraRight);
        }

        if (minX > maxX)
        {
            float center = (minX + maxX) * 0.5f;

            minX = center;
            maxX = center;
        }
    }

    float MeasureAlienHalfWidth(GameObject alien)
    {
        float width = 0f;

        Renderer[] renderers =
            alien.GetComponentsInChildren<Renderer>();

        foreach (Renderer item in renderers)
        {
            if (item == null || !item.enabled)
                continue;

            width = Mathf.Max(
                width,
                Mathf.Abs(item.bounds.min.x - alien.transform.position.x),
                Mathf.Abs(item.bounds.max.x - alien.transform.position.x)
            );
        }

        if (width > 0f)
            return width;

        Collider[] colliders =
            alien.GetComponentsInChildren<Collider>();

        foreach (Collider item in colliders)
        {
            if (item == null || !item.enabled)
                continue;

            width = Mathf.Max(
                width,
                Mathf.Abs(item.bounds.min.x - alien.transform.position.x),
                Mathf.Abs(item.bounds.max.x - alien.transform.position.x)
            );
        }

        return width;
    }

    bool HasReachedLossLine()
    {
        foreach (GameObject alien in aliens)
        {
            if (alien != null &&
                alien.transform.position.y <= lossLine)
                return true;
        }

        return false;
    }
}

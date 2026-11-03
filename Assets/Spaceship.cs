using UnityEngine;
using UnityEngine.InputSystem;

public class Spaceship : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject dartPrefab;

    [SerializeField] private float shotCooldown = 0.20f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float horizontalLimit = 14.5f;
    [SerializeField] private float screenEdgePadding = 0.2f;
    [SerializeField] private Camera playfieldCamera;

    private float nextShotTime;

    private float halfWidth;

    void Awake()
    {
        if (playfieldCamera == null)
            playfieldCamera = Camera.main;

        halfWidth = CalculateHalfWidth();
    }

    void Update()
    {
        GameManager game = GameManager.Instance;

        if (game == null || !game.IsPlaying)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        float input = 0f;

        if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed)
            input += 1f;

        if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed)
            input -= 1f;

        Vector3 position = transform.position;

        GetHorizontalBounds(out float minX, out float maxX);

        position.x += input * speed * Time.deltaTime;

        position.x = Mathf.Clamp(position.x, minX, maxX);

        transform.position = position;

        if (keyboard.spaceKey.wasPressedThisFrame &&
            Time.time >= nextShotTime &&
            dartPrefab != null)
        {
            Instantiate(
                dartPrefab,
                transform.position,
                Quaternion.identity
            );

            nextShotTime = Time.time + shotCooldown;
        }
    }

    float CalculateHalfWidth()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        foreach (Renderer item in renderers)
        {
            if (item == null || !item.enabled)
                continue;

            Bounds bounds = item.bounds;

            minX = Mathf.Min(minX, bounds.min.x);
            maxX = Mathf.Max(maxX, bounds.max.x);
        }

        if (float.IsInfinity(minX))
            return 0f;

        return Mathf.Max(
            transform.position.x - minX,
            maxX - transform.position.x
        );
    }

    void GetHorizontalBounds(out float minX, out float maxX)
    {
        minX = -horizontalLimit;
        maxX = horizontalLimit;

        if (playfieldCamera == null)
            playfieldCamera = Camera.main;

        if (playfieldCamera == null)
            return;

        if (!CameraUtils.TryGetHorizontalBounds(
            playfieldCamera,
            transform.position.z,
            out float cameraLeft,
            out float cameraRight))
        {
            return;
        }

        float cameraMinX =
            cameraLeft + halfWidth + screenEdgePadding;

        float cameraMaxX =
            cameraRight - halfWidth - screenEdgePadding;

        minX = Mathf.Max(-horizontalLimit, cameraMinX);
        maxX = Mathf.Min(horizontalLimit, cameraMaxX);

        if (minX > maxX)
        {
            float midpoint = (cameraLeft + cameraRight) * 0.5f;

            minX = midpoint;
            maxX = midpoint;
        }
    }
}

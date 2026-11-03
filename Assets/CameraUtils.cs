using UnityEngine;

public static class CameraUtils
{
    public static float HalfWidth => GetHalfWidth(Camera.main);

    public static float GetHalfWidth(Camera camera)
    {
        if (camera == null || !camera.orthographic)
            return 0f;

        return camera.orthographicSize * camera.aspect;
    }

    public static bool TryGetHorizontalBounds(
        Camera camera,
        float planeZ,
        out float minX,
        out float maxX)
    {
        minX = 0f;
        maxX = 0f;

        if (camera == null)
            return false;

        Plane targetPlane = new Plane(
            Vector3.forward,
            new Vector3(0f, 0f, planeZ)
        );

        Vector3[] viewportCorners =
        {
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(1f, 0f, 0f),
            new Vector3(1f, 1f, 0f)
        };

        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;

        foreach (Vector3 corner in viewportCorners)
        {
            Ray ray = camera.ViewportPointToRay(corner);

            if (!targetPlane.Raycast(ray, out float distance))
            {
                minX = 0f;
                maxX = 0f;
                return false;
            }

            float x = ray.GetPoint(distance).x;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
        }

        return true;
    }
}

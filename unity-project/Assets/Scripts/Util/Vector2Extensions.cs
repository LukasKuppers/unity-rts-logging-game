using UnityEngine;

public static class Vector2Extensions
{
    /// <summary>
    /// Returns two points on the circle centered on this point with radius 'radius'.
    /// For each point, a line drawn through the origin (0, 0) and the point will be tangent to the circle.
    /// Both returned points will be the center point if the distance between the origin and the point is less than or equal to the radius
    /// </summary>
    /// <param name="point">The center of the circle</param>
    /// <param name="radius">The radius of the Circle</param>
    /// <returns>A typle with two vectors: left and right - the points relative to the origin</returns>
    public static (Vector2 left, Vector2 right) OriginToCircleTangent(this Vector2 point, float radius)
    {
        float d = point.magnitude;

        if (d < radius)
            return (point, point);

        float rho = radius / d;
        float A = Mathf.Pow(rho, 2f);
        float B = rho * Mathf.Sqrt(1 - A);

        float leftX = point.x - (A * point.x) - (B * point.y);
        float leftY = point.y - (A * point.y) + (B * point.x);

        float rightX = point.x - (A * point.x) + (B * point.y);
        float rightY = point.y - (A * point.y) - (B * point.x);

        return (new Vector2(leftX, leftY), new Vector2(rightX, rightY));
    }
}

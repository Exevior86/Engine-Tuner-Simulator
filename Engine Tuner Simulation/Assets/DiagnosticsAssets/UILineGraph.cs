using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineGraph : MonoBehaviour
{
    public Color lineColor = Color.white;
    public RectTransform graphContainer;
    public Sprite dotSprite; // Optional: Assign a round circle sprite for data dots

    // Keep track of active segments for this specific line
    private List<GameObject> activeSegments = new List<GameObject>();

    public void DrawGraphLine(List<Vector2> points)
    {
        // 1. Clean up any previous segments belonging to this specific line
        ClearLine();

        if (points == null || points.Count < 2) return;

        float width = graphContainer.rect.width;
        float height = graphContainer.rect.height;

        // 2. Loop and connect the normalized coordinate points
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 startPos = new Vector2(points[i].x * width, points[i].y * height);
            Vector2 endPos = new Vector2(points[i + 1].x * width, points[i + 1].y * height);

            // Create and parent the line segment
            GameObject segment = CreateLineSegment(startPos, endPos);
            if (segment != null)
            {
                activeSegments.Add(segment);
            }
        }
    }

    /// <summary>
    /// Instantiates and configures a single UI image segment between two coordinates
    /// </summary>
    private GameObject CreateLineSegment(Vector2 start, Vector2 end)
    {
        // Create a new empty UI GameObject
        GameObject segmentObj = new GameObject("lineSegment", typeof(Image));

        // ➔ THE CRITICAL FIX: Parent it to THIS line's transform immediately!
        segmentObj.transform.SetParent(this.transform, false);

        // Style the line segment color
        Image image = segmentObj.GetComponent<Image>();
        image.color = lineColor;

        // Calculate size, position, and rotation to stretch between the two points
        RectTransform rect = segmentObj.GetComponent<RectTransform>();
        Vector2 direction = (end - start).normalized;
        float distance = Vector2.Distance(start, end);

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.sizeDelta = new Vector2(distance, 2f); // 2f is the line thickness
        rect.anchoredPosition = start + (direction * distance * 0.5f);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.localRotation = Quaternion.Euler(0, 0, angle);

        return segmentObj; // Returning the GameObject so the caller can track it!
    }

    public void ClearLine()
    {
        // Destroy all child segments immediately
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        activeSegments.Clear();
    }

    private void OnDestroy()
    {
        ClearLine();
    }
}
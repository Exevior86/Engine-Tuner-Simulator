using System.Collections.Generic;
using UnityEngine;

public class GraphVisualTest : MonoBehaviour
{
    // Reference to your UILineGraph component
    public UILineGraph lineGraph;

    private void Start()
    {
        // Generate a simple dummy line that climbs from bottom-left to top-right
        List<Vector2> testPoints = new List<Vector2>();

        // ➔ FIXED: Capitalized "Add"
        testPoints.Add(new Vector2(0.0f, 0.1f)); // Start low on the left
        testPoints.Add(new Vector2(0.2f, 0.4f));
        testPoints.Add(new Vector2(0.4f, 0.3f)); // Little dip
        testPoints.Add(new Vector2(0.6f, 0.8f)); // Climb up
        testPoints.Add(new Vector2(0.8f, 0.7f));
        testPoints.Add(new Vector2(1.0f, 0.9f)); // End high on the right

        // Tell the graph to draw these points immediately on play!
        if (lineGraph != null)
        {
            lineGraph.DrawGraphLine(testPoints);
            Debug.Log("[Test] Successfully sent dummy data points to the graph!");
        }
        else
        {
            Debug.LogError("[Test] Line Graph reference is missing!");
        }
    }
}
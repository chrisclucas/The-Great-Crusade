using UnityEngine;



public class GraphicRoutines : MonoBehaviour
{

    /// <summary>
    /// Draws a line between the two points passed
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    public static void DrawLineBetweenTwoPoints(Vector3 point1, Vector3 point2, float width, UnityEngine.Color color)
    {
        Material lineMaterial = Resources.Load("LineMaterial", typeof(Material)) as Material;
        GameObject line = new GameObject("DrawLineBetweenTwoPoints");

        if (lineMaterial == null)
            GlobalDefinitions.WriteToLogFile("DrawLineBetweenTwoPoints: ERROR - Material returned null from Resources");

        line.layer = LayerMask.NameToLayer("Lines");
        line.transform.SetParent(GameObject.Find("Lines").transform);

        Vector3[] linePositions = new Vector3[2];
        linePositions[0] = point1;
        linePositions[1] = point2;

        line.AddComponent<LineRenderer>();
        line.GetComponent<LineRenderer>().useWorldSpace = true;
        line.GetComponent<LineRenderer>().startColor = color;
        line.GetComponent<LineRenderer>().endColor = color;
        line.GetComponent<LineRenderer>().positionCount = 2;
        line.GetComponent<LineRenderer>().startWidth = 0.5f;
        line.GetComponent<LineRenderer>().endWidth = 0.5f;
        line.GetComponent<LineRenderer>().numCapVertices = 10;
        line.GetComponent<LineRenderer>().material = lineMaterial;
        line.GetComponent<LineRenderer>().SetPositions(linePositions);
        line.GetComponent<LineRenderer>().sortingLayerName = "Lines";
        line.GetComponent<LineRenderer>().startWidth = width;
        line.GetComponent<LineRenderer>().endWidth = width;
    }

    /// <summary>
    /// Draws a square using the four points passed.  The assumption is that the square can be drawn using the points in the order passed
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <param name="point3"></param>
    /// <param name="point4"></param>
    /// <param name="color"></param>
    public static void DrawSquare(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float width, UnityEngine.Color color)
    {
        DrawLineBetweenTwoPoints(point1, point2, width, color);
        DrawLineBetweenTwoPoints(point2, point3, width, color);
        DrawLineBetweenTwoPoints(point3, point4, width, color);
        DrawLineBetweenTwoPoints(point4, point1, width, color);
    }

    /// <summary>
    /// Puts the invasion tables onto the board
    /// </summary>
    public static void PlaceInvasionTables()
    {
        // The locations and the graphics are all hardcoded


    }
}
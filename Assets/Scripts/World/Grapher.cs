using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grapher : MonoBehaviour
{
    public int numberOfPoints;
    public int maxX;
    public int maxY;

    public GameObject pointsParent;
    public Sprite dot;

    public GameObject linesParent;
    public GameObject line;

	void Start()
    {
        List<Vector2> points = GenerateRandomGridPoints(numberOfPoints, maxX, maxY);
        points = ScalePoints(points, 1f / maxX, 1f / maxY);
        InstantiatePoints(points);
        ConnectAllPoints(points);
	}

    /// <summary>
    /// Builds a list of random points. The points have coordinates between -1 and 1.
    /// </summary>
    /// <param name="numberOfPoints">The number of points to create.</param>
    /// <param name="increment">The minimum distance between points. A non-zero value will essentially put the points on a grid.</param>
    /// <returns>The list of random points.</returns>
    List<Vector2> GenerateRandomPoints(int numberOfPoints)
    {
        List<Vector2> randomPoints = new List<Vector2>();
        while (randomPoints.Count < numberOfPoints)
        {
            Vector2 newPoint = new Vector2();
            newPoint.Set(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            randomPoints.Add(newPoint);
        }
        return randomPoints;
    }

    /// <summary>
    /// Builds a list of random points. The points have integer coordinates between (-maxX, -maxY) and (maxX, maxY).
    /// </summary>
    /// <param name="numberOfPoints">The number of points to create.</param>
    /// <param name="maxX">The maximum x coordiante.</param>
    /// <param name="maxY">The maximum y coordinate.</param>
    /// <returns>The list of random points.</returns>
    List<Vector2> GenerateRandomGridPoints(int numberOfPoints, int maxX, int maxY)
    {
        // Make sure we don't try to create too many points
        int maxPointsForGrid = (2 * maxX + 1) * (2 * maxY + 1);
        numberOfPoints = Mathf.Min(numberOfPoints, maxPointsForGrid);

        List<Vector2> randomPoints = new List<Vector2>();
        while (randomPoints.Count < numberOfPoints)
        {
            Vector2 newPoint = new Vector2();
            int x = Random.Range(-maxX, maxX + 1);
            int y = Random.Range(-maxY, maxY + 1);
            newPoint.Set(x, y);

            // Filter duplicates
            if (randomPoints.FindAll(p => p.x == x && p.y == y).Count != 0)
            {
                continue;
            }

            randomPoints.Add(newPoint);
        }
        return randomPoints;
    }

    List<Vector2> ScalePoints(List<Vector2> points, float scaleX, float scaleY)
    {
        List<Vector2> scaledPoints = new List<Vector2>();
        foreach (var point in points)
        {
            scaledPoints.Add(new Vector2(point.x * scaleX, point.y * scaleY));
        }
        return scaledPoints;
    }

    /// <summary>
    /// Instantiates the given points into the game world.
    /// </summary>
    /// <param name="points">The points to instantiate.</param>
    void InstantiatePoints(List<Vector2> points)
    {
        foreach (var point in points)
        {
            GameObject newPoint = new GameObject("Point");
            newPoint.AddComponent<SpriteRenderer>();
            newPoint.GetComponent<SpriteRenderer>().sprite = dot;
            newPoint.transform.localPosition = point;
            newPoint.transform.parent = pointsParent.transform;
        }
    }

    /// <summary>
    /// Connects all points together with a line.
    /// </summary>
    /// <param name="points">The points to connect.</param>
    void ConnectAllPoints(List<Vector2> points)
    {
        for (int ii = 0; ii < points.Count - 1; ii++)
        {
            for (int jj = ii + 1; jj < points.Count; jj++)
            {
                GameObject newLine = Instantiate(line);
                newLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
                {
                    points[ii],
                    points[jj]
                });
                newLine.transform.parent = linesParent.transform;
            }
        }
    }
}

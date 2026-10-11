using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class HoverOutlineFromCollider2D : MonoBehaviour
{
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 0.05f;

    private PolygonCollider2D polygon;
    private LineRenderer line;

    void Awake()
    {
        polygon = GetComponent<PolygonCollider2D>();
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = false;
        line.loop = true;
        line.startColor = outlineColor;
        line.endColor = outlineColor;
        line.startWidth = outlineWidth;
        line.endWidth = outlineWidth;

        CreateOutline();

        line.enabled = false;
    }

    void CreateOutline()
    {
        if (polygon.pathCount == 0)
        {
            return;
        }

        Vector2[] points =
            polygon.GetPath(0);

        line.positionCount =
            points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(
                i,
                new Vector3(
                    points[i].x,
                    points[i].y,
                    -0.1f
                )
            );
        }
    }

    public void SetVisible(bool visible)
    {
        line.enabled = visible;
    }
}
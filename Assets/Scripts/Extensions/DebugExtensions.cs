using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugExtensions
{
    public static void DrawCircle(Vector3 position, float radius, Color debugColor, float maxAngle = 360, int steps = 20) {
        float angle = 0;
        Vector3 begin = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized * radius;
        Vector3 end;
        for (int j = 0; j <= steps; j++) {
            angle = (float)j / (float)steps * maxAngle * Mathf.Deg2Rad;
            end = position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized * radius;
            Debug.DrawLine(begin, end, debugColor);
            begin = end;
        }
    }

    public static void DrawBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2) {
        int numberOfPoints = 50;
        Gizmos.DrawWireSphere(point0, .5f);
        Gizmos.DrawWireSphere(point1, .5f);
        Gizmos.DrawWireSphere(point2, .5f);
        Vector3 beginPos = point0;
        if (point0 != null && point1 != null && point2 != null) {
            for (int i = 1; i < numberOfPoints + 1; i++){
                float t = i / (float)numberOfPoints;
                Vector3 newPos =  Extensions.CalculateQuadraticBezierPoint(t, point0, point1, point2);
                Debug.DrawLine(beginPos, newPos);
                beginPos = newPos;
            }
        }
    }
}

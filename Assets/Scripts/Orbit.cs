using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes a gameobject orbit around its original position.
/// </summary>
public class Orbit : MonoBehaviour
{

    public float speed = 1f;
    public float radius = 1f;
    [Range(-1, 1)]
    public float startAngle;
    public Vector3 orbitDirection;

    private Vector3 from;
    private Vector3 to;

    private float angle = 0;
    private const float twoPi = Mathf.PI * 2;
    private float angleChangePerFrame;
    private Vector3 originalPosition;
    private Vector3 originalWorldPosition;

    private float half = 1;

    void Start()
    {
        angleChangePerFrame = speed / twoPi;
        originalPosition = transform.localPosition;
        originalWorldPosition = transform.position;
        to = orbitDirection * radius;
        from = -orbitDirection * radius;
        if (startAngle < 0)
        {
            startAngle += 1;
            half = -1;
        }
    }

    void Update()
    {
        if (angle >= 1)
        {
            angle -= 1;
            half *= -1;
        }
        Vector3 newPos = Vector3.Slerp(from, to, angle);
        transform.localPosition = originalPosition + half * newPos;
        transform.LookAt(originalWorldPosition);
        angle += angleChangePerFrame;

    }
}

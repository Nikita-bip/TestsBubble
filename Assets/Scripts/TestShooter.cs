using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShooter : MonoBehaviour
{
    public Vector3 Initial_Position;
    public int Bird_Speed;

    public void OnMouseUp()
    {
        Vector2 Spring_force = Initial_Position - transform.position;
        GetComponent<Rigidbody2D>().gravityScale = 1;
        GetComponent<Rigidbody2D>().AddForce(Bird_Speed*Spring_force);
    }

    public void OnMouseDrag()
    {
        Vector3 DragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(DragPosition.x, DragPosition.y);
    }
}

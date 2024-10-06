using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    private float _raycastRange = 18f;
    private float _raycastOffset = 19f;

    public bool IsFixed;
    public bool IsConnected;
    public BubbleColor bubbleColor;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((collision.gameObject.tag == "Bubble" && collision.gameObject.GetComponent<Bubble>().IsFixed) || collision.gameObject.tag == "Limit")
        {
            if (!IsFixed)
                HasCollided();
        }
        // добавить удаление шариков 
    }

    private void HasCollided()
    {
        Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
        Destroy(rigidbody);
        IsFixed = true;
        LevelManager.instance.SetAsBubbleAreaChild(transform);
        GameManager.Instance.ProcessTurn(transform);
    }

    public List<Transform> GetNeighbours()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        List<Transform> neighbours = new List<Transform>();

        hits.Add(Physics2D.Raycast(new Vector2(transform.position.x - _raycastOffset, transform.position.y), Vector3.left, _raycastRange));
        hits.Add(Physics2D.Raycast(new Vector2(transform.position.x + _raycastOffset, transform.position.y), Vector3.right, _raycastRange));
        hits.Add(Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y + _raycastOffset), Vector3.up, _raycastRange));
        hits.Add(Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - _raycastOffset), Vector3.down, _raycastRange));

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.transform.tag.Equals("Bubble"))
            {
                neighbours.Add(hit.transform);
            }
        }

        return neighbours;
    }

    public enum BubbleColor
    {
        Blue, Yellow, Red, Purple
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        foreach (Transform tr in GetNeighbours())
        {
            Gizmos.DrawLine(transform.position, tr.position);
        }
    }
}
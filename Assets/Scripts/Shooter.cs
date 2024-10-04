using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private float _speed = 400f;
    [SerializeField] private GameObject _bottomShootPoint;
    [SerializeField] private Transform _nextBubblePosition;
    [SerializeField] private LineRenderer lineRenderer; // Добавление LineRenderer

    public bool canShoot;
    private GameObject _currentBubble;
    private GameObject _nextBubble;
    private Vector2 _lookDirection;
    private float _lookAngle;
    private GameObject _limit;
    private Vector2 _gizmosPoint = new Vector2(0f, -11.73333f);

    private void Awake()
    {
        _limit = GameObject.FindGameObjectWithTag("Limit");
    }

    private void Update()
    {
        _gizmosPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        _lookAngle = Mathf.Atan2(_lookDirection.y, _lookDirection.x) * Mathf.Rad2Deg;

        if (canShoot && Input.GetMouseButton(0))
        {
            CastRay(transform.position, _gizmosPoint);
        }

        if (canShoot && Input.GetMouseButtonUp(0)
            && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > _bottomShootPoint.transform.position.y)
            && (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < _limit.transform.position.y))
        {
            canShoot = false;
            Shoot();
        }
    }

    public void Shoot()
    {
        if (_currentBubble == null)
            CreateNextBubble();

        ScoreManager.GetInstance().ReduceThrows();
        transform.rotation = Quaternion.Euler(0f, 0f, _lookAngle - 90f);
        _currentBubble.transform.rotation = transform.rotation;
        _currentBubble.GetComponent<CircleCollider2D>().enabled = true;
        Rigidbody2D rb = _currentBubble.GetComponent<Rigidbody2D>();
        rb.AddForce(_currentBubble.transform.up * _speed, ForceMode2D.Impulse);
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;
        _currentBubble = null;
    }

    private void CastRay(Vector2 pos, Vector2 dir)
    {
        int RayCount = 2;
        lineRenderer.positionCount = RayCount;

        lineRenderer.SetPosition(0, pos);

        for (int i = 1; i < RayCount; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, dir - (Vector2)transform.position, 300);

            if (hit.collider != null && hit.transform.tag.Equals("Wall"))
            {
                lineRenderer.SetPosition(i, hit.point);

                pos = hit.point - dir * 0.01f;
                dir = Vector2.Reflect(dir, hit.normal);

                if (RayCount < 5)
                {
                    RayCount += 1;
                    lineRenderer.positionCount = RayCount;
                }
            }
            else if (hit.collider != null && hit.transform.tag.Equals("Bubble"))
            {
                lineRenderer.SetPosition(i, hit.point);
                break;
            }
        }
    }

    public void CreateNewBubbles()
    {
        if (_nextBubble != null)
            Destroy(_nextBubble);

        if (_currentBubble != null)
            Destroy(_currentBubble);

        _nextBubble = null;
        _currentBubble = null;
        CreateNextBubble();
        canShoot = true;
    }

    public void CreateNextBubble()
    {
        List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
        List<string> colors = LevelManager.instance.colorsInScene;

        if (bubblesInScene.Count < 1)
            return;

        if (_nextBubble == null)
        {
            _nextBubble = InstantiateNewBubble(bubblesInScene);
        }

        if (_currentBubble == null)
        {
            _currentBubble = _nextBubble;
            _currentBubble.transform.position = transform.position;
            _nextBubble = InstantiateNewBubble(bubblesInScene);
        }
    }

    private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
    {
        if (bubblesInScene.Count > 0)
        {
            GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)]);
            newBubble.transform.position = _nextBubblePosition.position;
            newBubble.GetComponent<Bubble>().isFixed = false;
            newBubble.GetComponent<CircleCollider2D>().enabled = false;
            Rigidbody2D rb2d = newBubble.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0f;
            return newBubble;
        }
        else
        {
            return null;
        }
    }
}

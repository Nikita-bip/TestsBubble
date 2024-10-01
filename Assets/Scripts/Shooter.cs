using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    [SerializeField] private float _speed = 400f;
    [SerializeField] private GameObject _bottomShootPoint;
    [SerializeField] private Transform _nextBubblePosition;

    public bool canShoot; //надо чекнуть и исправить
    private GameObject _currentBubble;
    private GameObject _nextBubble;
    private Vector2 _lookDirection;
    private float _lookAngle;
    private GameObject _limit;
    private Vector2 _gizmosPoint;

    public void Awake()
    {
        _limit = GameObject.FindGameObjectWithTag("Limit");
    }

    public void Update()
    {
        _gizmosPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        _lookAngle = Mathf.Atan2(_lookDirection.y, _lookDirection.x) * Mathf.Rad2Deg;

        if (canShoot
            && Input.GetMouseButtonUp(0)
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
            Rigidbody2D rb2d = newBubble.AddComponent(typeof(Rigidbody2D)) as Rigidbody2D;
            rb2d.gravityScale = 0f;
            return newBubble;
        }
        else
        {
            return null;
        }
    }
}
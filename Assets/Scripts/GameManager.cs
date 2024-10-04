using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _winMenu;
    [SerializeField] private GameObject _loseMenu;
    [SerializeField] private GameObject _winScore;
    [SerializeField] private Transform _bottomLimit;

    public static GameManager Instance;
    public Shooter shootScript; // чекнуть и исправить

    private const int SEQUENCE_SIZE = 2;
    private const string StrDissolve = "_DissolveAmount";
    private List<Transform> _sequenceBubbles;
    private List<Transform> _connectedBubbles;
    private List<Transform> _bubblesToDrop;
    private List<Transform> _bubblesToDissolve;
    private float _dropSpeed = 50f;
    private bool _isDissolving = false;
    private float _dissolveSpeed = 2f;
    private float _rayDistance = 200f;

    private void Update()
    {
        if (_isDissolving)
        {
            foreach (Transform bubble in _bubblesToDissolve)
            {

                if (bubble == null)
                {
                    if (_bubblesToDissolve.IndexOf(bubble) == _bubblesToDissolve.Count - 1)
                    {
                        _isDissolving = false;
                        EmptyDissolveList();
                        break;
                    }
                    else continue;
                }

                SpriteRenderer spriteRenderer = bubble.GetComponent<SpriteRenderer>();
                float dissolveAmount = spriteRenderer.material.GetFloat(StrDissolve);

                if (dissolveAmount >= 0.99f)
                {
                    _isDissolving = false;
                    EmptyDissolveList();
                    break;
                }
                else
                {
                    float newDissolve = dissolveAmount + _dissolveSpeed * Time.deltaTime;
                    spriteRenderer.material.SetFloat(StrDissolve, newDissolve);
                }
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        _winMenu.SetActive(false);
        _loseMenu.SetActive(false);
        _sequenceBubbles = new List<Transform>();
        _connectedBubbles = new List<Transform>();
        _bubblesToDrop = new List<Transform>();
        _bubblesToDissolve = new List<Transform>();
        DontDestroyOnLoad(gameObject);
    }

    private void EmptyDissolveList()
    {
        foreach (Transform bubble in _bubblesToDissolve)
        { 
            if (bubble != null)
                Destroy(bubble.gameObject);
        }

        _bubblesToDissolve.Clear();
    }

    IEnumerator CheckSequence(Transform currentBubble)
    {
        yield return new WaitForSeconds(0.1f);

        _sequenceBubbles.Clear();
        CheckBubbleSequence(currentBubble);

        if (_sequenceBubbles.Count >= SEQUENCE_SIZE)
        {
            ProcessBubblesInSequence();
            ProcessDisconectedBubbles();
        }

        _sequenceBubbles.Clear();
        LevelManager.instance.UpdateListOfBubblesInScene();

        if (LevelManager.instance.BubblesInScene.Count == 0)
        {
            ScoreManager man = ScoreManager.GetInstance();
            _winScore.GetComponent<Text>().text = man.GetScore().ToString();
            _winMenu.SetActive(true);
        }
        else
        {
            shootScript.CreateNextBubble();
            shootScript.CanShoot = true;
        }

        ProcessBottomLimit();
        CheckingTheThrows();
    }

    public void ProcessTurn(Transform currentBubble)
    {
        StartCoroutine(CheckSequence(currentBubble));
    }

    private void ProcessBottomLimit()
    {
        foreach (Transform bubble in LevelManager.instance.BubblesArea)
        {
            if (bubble.GetComponent<Bubble>().IsConnected && bubble.position.y < _bottomLimit.position.y)
            {
                _loseMenu.SetActive(true);
                shootScript.CanShoot = false;
                break;
            }
        }
    }

    private void CheckingTheThrows()
    {
        ScoreManager scoreManager = ScoreManager.GetInstance();
        int currentThrows = scoreManager.GetThrows();

        if(currentThrows == 0)
        {
            _loseMenu.SetActive(true);
            shootScript.CanShoot = false;
        }
    }

    private void CheckBubbleSequence(Transform currentBubble)
    {
        _sequenceBubbles.Add(currentBubble);

        Bubble bubbleScript = currentBubble.GetComponent<Bubble>();
        List<Transform> neighbours = bubbleScript.GetNeighbours();

        foreach (Transform bubble in neighbours)
        {
            if (!_sequenceBubbles.Contains(bubble))
            {
                Bubble bScript = bubble.GetComponent<Bubble>();

                if (bScript.bubbleColor == bubbleScript.bubbleColor)
                {
                    CheckBubbleSequence(bubble);
                }
            }
        }
    }

    private void ProcessBubblesInSequence()
    {
        foreach (Transform bubble in _sequenceBubbles)
        {
            if (!_bubblesToDissolve.Contains(bubble))
            {
                ScoreManager.GetInstance().AddScore(1);
                bubble.tag = "Untagged";
                bubble.SetParent(null);
                bubble.GetComponent<CircleCollider2D>().enabled = false;
                _bubblesToDissolve.Add(bubble);
            }
        }
        _isDissolving = true;
    }

    #region Drop Disconected Bubbles

    private void ProcessDisconectedBubbles()
    {
        SetAllBubblesConnectionToFalse();
        SetConnectedBubblesToTrue();
        CheckDisconectedBubbles();
        DropAll();
    }

    private void SetAllBubblesConnectionToFalse()
    {
        foreach (Transform bubble in LevelManager.instance.BubblesArea)
        {
            bubble.GetComponent<Bubble>().IsConnected = false;
        }
    }

    private void SetConnectedBubblesToTrue()
    {
        _connectedBubbles.Clear();

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.right, _rayDistance);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.gameObject.tag.Equals("Bubble"))
                SetNeighboursConnectionToTrue(hits[i].transform);
        }
    }

    private void SetNeighboursConnectionToTrue(Transform bubble)
    {
        _connectedBubbles.Add(bubble);

        Bubble bubbleScript = bubble.GetComponent<Bubble>();
        bubbleScript.IsConnected = true;

        foreach (Transform t in bubbleScript.GetNeighbours())
        {
            if (!_connectedBubbles.Contains(t))
            {
                SetNeighboursConnectionToTrue(t);
            }
        }
    }

    private void CheckDisconectedBubbles()
    {
        foreach (Transform bubble in LevelManager.instance.BubblesArea)
        {
            Bubble bubbleScript = bubble.GetComponent<Bubble>();

            if (!bubbleScript.IsConnected)
            {
                if (!_bubblesToDrop.Contains(bubble))
                {
                    ScoreManager.GetInstance().AddScore(2);
                    bubble.tag = "Untagged";
                    _bubblesToDrop.Add(bubble);
                }
            }
        }
    }

    private void DropAll()
    {
        foreach (Transform bubble in _bubblesToDrop)
        {
            bubble.SetParent(null);
            bubble.gameObject.GetComponent<CircleCollider2D>().enabled = false;

            if (!bubble.GetComponent<Rigidbody2D>())
            {
                Rigidbody2D rig = (Rigidbody2D)bubble.gameObject.AddComponent(typeof(Rigidbody2D));
                rig.gravityScale = _dropSpeed;
            }
        }
        _bubblesToDrop.Clear();
    }

    #endregion
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.right * _rayDistance);
    }
}
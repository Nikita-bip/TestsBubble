using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Grid))]
public class LevelManager : MonoBehaviour
{
    #region Singleton
    public static LevelManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [SerializeField] private Vector3 _startPosition;
    [SerializeField] private List<GameObject> _bubblesPrefabs;
    [SerializeField] private GameObject _ballPrefab1;
    [SerializeField] private GameObject _ballPrefab2;
    [SerializeField] private GameObject _ballPrefab3;
    [SerializeField] private GameObject _ballPrefab4;
    [SerializeField] private GameObject _winMenu;
    [SerializeField] private GameObject _winScore;

    public Transform BubblesArea;
    public List<GameObject> BubblesInScene;
    public List<string> ColorsInScene;

    private string _filePath = "Assets/Resources/Level.txt";
    private Dictionary<char, GameObject> _prefabDictionary;
    private Grid _grid;

    private void Start()
    {
        _prefabDictionary = new Dictionary<char, GameObject>
    {
        { '1', _ballPrefab1 },
        { '2', _ballPrefab2 },
        { '3', _ballPrefab3 },
        { '4', _ballPrefab4 },
    };

        _grid = GetComponent<Grid>();
        StartCoroutine(LoadLevel());
    }

    private IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(0.1f);

        GenerateField(_startPosition);

        SnapChildrensToGrid(BubblesArea);
        UpdateListOfBubblesInScene();
        GameManager.Instance.shootScript.CreateNewBubbles();
    }

    private void GenerateField(Vector3 startPosition)
    {
        string[] lines = File.ReadAllLines(_filePath);

        for (int y = 0; y < lines.Length; y++)
        {
            string line = lines[y];

            for (int x = 0; x < line.Length; x++)
            {
                char currentChar = line[x];

                if (_prefabDictionary.ContainsKey(currentChar))
                {
                    Vector3 position = new Vector3(x * 25, -y * 25, 0) + startPosition;
                    GameObject prefab = _prefabDictionary[currentChar];
                    Instantiate(prefab, position, Quaternion.identity, BubblesArea);
                }
            }
        }
    }

    private void SnapChildrensToGrid(Transform BubbleArea)
    {
        foreach (Transform bubble in BubbleArea)
        {
            SnapToNearestGripPosition(bubble);
        }
    }

    private void SnapToNearestGripPosition(Transform bubble)
    {
        Vector3Int cellPosition = _grid.WorldToCell(bubble.position);
        bubble.position = _grid.GetCellCenterWorld(cellPosition);
        bubble.rotation = Quaternion.identity;

    }

    public void UpdateListOfBubblesInScene()
    {
        List<string> colors = new List<string>();
        List<GameObject> newListOfBubbles = new List<GameObject>();

        foreach (Transform bubble in BubblesArea)
        {
            Bubble bubbleScript = bubble.GetComponent<Bubble>();

            if (colors.Count < _bubblesPrefabs.Count && !colors.Contains(bubbleScript.bubbleColor.ToString()))
            {
                string color = bubbleScript.bubbleColor.ToString();

                foreach (GameObject prefab in _bubblesPrefabs)
                {
                    if (color.Equals(prefab.GetComponent<Bubble>().bubbleColor.ToString()))
                    {
                        colors.Add(color);
                        newListOfBubbles.Add(prefab);
                    }
                }
            }
        }

        ColorsInScene = colors;
        BubblesInScene = newListOfBubbles;

        CheckWinCondition();
    }

    public void SetAsBubbleAreaChild(Transform bubble)
    {
        SnapToNearestGripPosition(bubble);
        bubble.SetParent(BubblesArea);
    }

    private void CheckWinCondition()
    {
        List<Transform> firstRowBubbles = new List<Transform>();
        int totalBubbles = 0;
        string[] lines = File.ReadAllLines(_filePath);
        string firstLine = lines[0];

        for (int x = 0; x < firstLine.Length; x++)
        {
            char currentChar = firstLine[x];

            if (_prefabDictionary.ContainsKey(currentChar))
            {
                totalBubbles++;
                Vector3 position = new Vector3((x * 25 + _startPosition.x + 2.5f), 137.5f, 0.5f);
                Transform bubble = FindBubbleAtPosition(position);

                if (bubble != null)
                {
                    firstRowBubbles.Add(bubble);
                }
            }
        }

        if (firstRowBubbles.Count < totalBubbles * 0.3f)
        {
            RemoveRemainingBubbles();
        }
    }

    private Transform FindBubbleAtPosition(Vector3 position)
    {
        foreach (Transform bubble in BubblesArea)
        {
            if (bubble.position == position)
            {
                return bubble;
            }
        }
        
        return null;
    }

    private void RemoveRemainingBubbles()
    {
        foreach (Transform bubble in BubblesArea)
        {
            Destroy(bubble.gameObject);
        }

        _winMenu.SetActive(true);
        ScoreManager scoreManager = ScoreManager.GetInstance();
        _winScore.GetComponent<Text>().text = scoreManager.GetScore().ToString();
    }
}
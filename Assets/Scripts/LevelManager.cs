using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public Transform bubblesArea;
    public List<GameObject> bubblesInScene;
    public List<string> colorsInScene;

    private string _filePath = "Assets/Resources/Level.txt"; // Путь к текстовому файлу
    private Dictionary<char, GameObject> prefabDictionary;
    private Grid _grid;

    private void Start()
    {
        prefabDictionary = new Dictionary<char, GameObject>
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

        SnapChildrensToGrid(bubblesArea);
        UpdateListOfBubblesInScene();
        GameManager.instance.shootScript.CreateNewBubbles();
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

                if (prefabDictionary.ContainsKey(currentChar))
                {
                    Vector3 position = new Vector3(x * 25, -y * 25, 0) + startPosition;
                    GameObject prefab = prefabDictionary[currentChar];
                    Instantiate(prefab, position, Quaternion.identity, bubblesArea);
                }
            }
        }
    }

    #region Snap to Grid
    private void SnapChildrensToGrid(Transform bubbleArea)
    {
        foreach (Transform bubble in bubbleArea)
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
    #endregion

    public void UpdateListOfBubblesInScene()
    {
        List<string> colors = new List<string>();
        List<GameObject> newListOfBubbles = new List<GameObject>();

        foreach (Transform bubble in bubblesArea)
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

        colorsInScene = colors;
        bubblesInScene = newListOfBubbles;
    }

    public void SetAsBubbleAreaChild(Transform bubble)
    {
        SnapToNearestGripPosition(bubble);
        bubble.SetParent(bubblesArea);
    }
}
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

        CheckWinCondition();
    }

    public void SetAsBubbleAreaChild(Transform bubble)
    {
        SnapToNearestGripPosition(bubble);
        bubble.SetParent(bubblesArea);
    }

    private void CheckWinCondition()
    {
        // Здесь получаем количество шариков в первом ряду
        List<Transform> firstRowBubbles = new List<Transform>();
        int totalBubbles = 0;

        // Получаем строки уровня из файла
        string[] lines = File.ReadAllLines(_filePath);

        // Определяем индекс первого ряда (с нуля)
        string firstLine = lines[0];
        Debug.Log($"Позиция {firstLine}");

        for (int x = 0; x < firstLine.Length; x++)
        {
            char currentChar = firstLine[x];

            if (prefabDictionary.ContainsKey(currentChar))
            {
                totalBubbles++;
                // Вместо _grid.CellCountY используем lines.Length
                Vector3 position = new Vector3((x * 25 + _startPosition.x + 2.5f), 137.5f, 0.5f);
                Transform bubble = FindBubbleAtPosition(position);
                Debug.Log($"Позиция {position}");

                if (bubble != null)
                {
                    firstRowBubbles.Add(bubble);
                }
            }
        }

        Debug.Log($"{firstRowBubbles.Count}");
        Debug.Log($"{totalBubbles}");
        // Проверяем соотношение
        if (firstRowBubbles.Count < totalBubbles * 0.3f)
        {
            // Условие выигрыша выполнено
            Debug.Log("Выиграл");
            RemoveRemainingBubbles();
        }
    }

    private Transform FindBubbleAtPosition(Vector3 position)
    {
        // Этот метод ищет шарик по указанной позиции
        foreach (Transform bubble in bubblesArea)
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
        // Удаляем все остальные шарики в сцене
        foreach (Transform bubble in bubblesArea)
        {
            Destroy(bubble.gameObject);
        }

        // Можно здесь вызвать дополнительное событие, например
        // GameManager.instance.WinGame();
    }


}
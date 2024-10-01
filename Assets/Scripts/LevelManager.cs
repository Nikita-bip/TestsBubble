using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

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

    public Vector3 startPosition;
    public Grid grid;
    public List<GameObject> bubblesPrefabs;
    public Transform bubblesArea;
    public GameObject ballPrefab1;
    public GameObject ballPrefab2;
    public GameObject ballPrefab3;
    public GameObject ballPrefab4;

    public string filePath = "Assets/Resources/Level.txt"; // Путь к текстовому файлу
    public Dictionary<char, GameObject> prefabDictionary;
    public List<GameObject> bubblesInScene;
    public List<string> colorsInScene;


    private void Start()
    {
        prefabDictionary = new Dictionary<char, GameObject>
    {
        { '1', ballPrefab1 },
        { '2', ballPrefab2 },
        { '3', ballPrefab3 },
        { '4', ballPrefab4 },
    };

        grid = GetComponent<Grid>();
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(0.1f);

        GenerateField(startPosition);

        SnapChildrensToGrid(bubblesArea);
        UpdateListOfBubblesInScene();
        GameManager.instance.shootScript.CreateNewBubbles();
    }

    public void GenerateField(Vector3 startPosition)
    {
        string[] lines = File.ReadAllLines(filePath);

        for (int y = 0; y < lines.Length; y++)
        {
            string line = lines[y];

            for (int x = 0; x < line.Length; x++)
            {
                char currentChar = line[x];
                if (prefabDictionary.ContainsKey(currentChar)) // Проверка наличия префаба для текущего символа
                {
                    Vector3 position = new Vector3(x * 25, -y * 25, 0) + startPosition; // Позиция объекта
                    GameObject prefab = prefabDictionary[currentChar]; // Получение префаба из словаря
                    Instantiate(prefab, position, Quaternion.identity, bubblesArea);
                    Debug.Log($"Spawned {currentChar} at {position}");
                }
            }
        }
    }

    #region Snap to Grid
    private void SnapChildrensToGrid(Transform parent)
    {
        foreach (Transform t in parent)
        {
            SnapToNearestGripPosition(t);
        }
    }

    public void SnapToNearestGripPosition(Transform t)
    {
        Vector3Int cellPosition = grid.WorldToCell(t.position);
        t.position = grid.GetCellCenterWorld(cellPosition);
        t.rotation = Quaternion.identity;

    }
    #endregion

    public void UpdateListOfBubblesInScene()
    {
        List<string> colors = new List<string>();
        List<GameObject> newListOfBubbles = new List<GameObject>();

        foreach (Transform t in bubblesArea)
        {
            Bubble bubbleScript = t.GetComponent<Bubble>();
            if (colors.Count < bubblesPrefabs.Count && !colors.Contains(bubbleScript.bubbleColor.ToString()))
            {
                string color = bubbleScript.bubbleColor.ToString();

                foreach (GameObject prefab in bubblesPrefabs)
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
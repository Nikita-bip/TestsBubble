using UnityEngine;
using UnityEngine.UI;

public class ScoreManager
{
	private int _score = 0;
	private int _throws = 10;

	public static ScoreManager Instance;

	public static ScoreManager GetInstance()
	{
		if (Instance == null)
			Instance = new ScoreManager();

		return Instance;
	}

    private void UpdateScoreUI()
	{
		Text _score = GameObject.FindWithTag("Score").GetComponent<Text>();
		_score.text = $"Score: {this._score}";
	}

    private void UpdateThrowsUI()
    {
        Text _throws = GameObject.FindWithTag("Throws").GetComponent<Text>();
        _throws.text = $"{this._throws}";
    }

    public void AddScore(int score)
	{
		this._score += score;
		UpdateScoreUI();
	}

	public int GetScore()
	{
		return _score;
	}

	public void ReduceThrows()
	{
		_throws--;
		UpdateThrowsUI();
    }

	public int GetThrows()
	{
		return _throws;
	}
}
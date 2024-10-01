using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
	public bool canShoot;
	public float speed = 25f;

	public Transform nextBubblePosition;
	public GameObject currentBubble;
	public GameObject nextBubble;
	public GameObject bottomShootPoint;

	private Vector2 lookDirection;
	private float lookAngle;
	private GameObject line;
	private GameObject limit;
	private LineRenderer lineRenderer;
	private Vector2 gizmosPoint;

	public void Awake()
	{
		line = GameObject.FindGameObjectWithTag("Line");
		limit = GameObject.FindGameObjectWithTag("Limit");
	}

	public void Update()
	{
			gizmosPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

			if (canShoot
				&& Input.GetMouseButtonUp(0)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y > bottomShootPoint.transform.position.y)
				&& (Camera.main.ScreenToWorldPoint(Input.mousePosition).y < limit.transform.position.y))
			{
				canShoot = false;
				Shoot();
			}
	}

	public void Shoot()
	{
		if (currentBubble == null) CreateNextBubble();
		ScoreManager.GetInstance().AddThrows();
		transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);
		currentBubble.transform.rotation = transform.rotation;
		currentBubble.GetComponent<CircleCollider2D>().enabled = true;
		Rigidbody2D rb = currentBubble.GetComponent<Rigidbody2D>();
		rb.AddForce(currentBubble.transform.up * speed, ForceMode2D.Impulse);
		rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		rb.gravityScale = 0;
		currentBubble = null;
	}

	public void CreateNewBubbles()
	{
		if (nextBubble != null)
			Destroy(nextBubble);

		if (currentBubble != null)
			Destroy(currentBubble);

		nextBubble = null;
		currentBubble = null;
		CreateNextBubble();
		canShoot = true;
	}

	public void CreateNextBubble()
	{
		List<GameObject> bubblesInScene = LevelManager.instance.bubblesInScene;
		List<string> colors = LevelManager.instance.colorsInScene;

		if (bubblesInScene.Count < 1) return;

		if (nextBubble == null)
		{
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}

		if (currentBubble == null)
		{
			currentBubble = nextBubble;
			currentBubble.transform.position = transform.position;
			nextBubble = InstantiateNewBubble(bubblesInScene);
		}
	}

	private GameObject InstantiateNewBubble(List<GameObject> bubblesInScene)
	{
		if (bubblesInScene.Count > 0)
		{
			GameObject newBubble = Instantiate(bubblesInScene[Random.Range(0, bubblesInScene.Count)]);
			newBubble.transform.position = nextBubblePosition.position;
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
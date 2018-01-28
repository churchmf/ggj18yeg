using UnityEngine;
using System.Collections;

public class BackgroundScroller : MonoBehaviour
{
	public float scrollSpeed;

	private Vector3 startPosition;
	private float spriteWidth;

	void Start ()
	{
		startPosition = transform.position;
		SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		spriteWidth = spriteRenderer.bounds.size.x;
	}

	void Update ()
	{
		float newPosition = Mathf.Repeat(Time.time * scrollSpeed, spriteWidth*1.5f);
		transform.position = startPosition + Vector3.left * newPosition;
	}
}
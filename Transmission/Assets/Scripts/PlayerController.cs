using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float speed;

    private Rigidbody2D rb;

    void Start () {
        rb = GetComponent<Rigidbody2D>();
    }
	
	void FixedUpdate() {
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(0, moveVertical);
        rb.AddForce(movement * speed);
    }
}

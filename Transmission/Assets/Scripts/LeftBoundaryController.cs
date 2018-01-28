using UnityEngine;

public class LeftBoundaryController : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D other)
    {
        other.gameObject.SetActive(false);
    }
}

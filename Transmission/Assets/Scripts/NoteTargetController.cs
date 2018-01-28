using UnityEngine;

public class NoteTargetController : MonoBehaviour {
    public bool hit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag.Equals("Player"))
        {
            hit = true;
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
    }
}

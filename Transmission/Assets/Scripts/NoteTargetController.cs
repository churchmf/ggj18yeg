using UnityEngine;

public class NoteTargetController : MonoBehaviour {
    public bool hit = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag.Equals("Player"))
        {
            hit = true;
        }
    }

    void Update()
    {
        if(hit)
        {
            gameObject.transform.localScale /= 2;
        }
    }
}

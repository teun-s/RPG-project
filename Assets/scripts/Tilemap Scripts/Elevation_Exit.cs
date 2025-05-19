using UnityEngine;

public class Elevation_Exit : MonoBehaviour
{
    public Collider2D[] mountainColliders;
    public Collider2D[] boundaryColliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("triggerentertest");
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("PlayerDetected");
            foreach (Collider2D mountain in mountainColliders)
            {
                mountain.enabled = true;
            }

            foreach (Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = false;
            }

            Debug.Log("test");
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
    }
}

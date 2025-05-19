using TMPro;
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatSpeed = 1f; // Speed of upward movement
    public float fadeDuration = 4f; // Duration of the fade effect

    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private float timer = 0f;


    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetText(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }
    }

    void Update()
    {
        // Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Fade out
        timer += Time.deltaTime;
        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - (timer / fadeDuration);
        }
    }
}

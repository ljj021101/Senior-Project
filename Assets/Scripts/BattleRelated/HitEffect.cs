using UnityEngine;
using System.Collections;

public class HitEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.white;
    public float flashDuration = 0.2f;

    private Color originalColor;
    private Coroutine flashRoutine;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashAndFade());
    }

    IEnumerator FlashAndFade()
    {
        spriteRenderer.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            spriteRenderer.color = Color.Lerp(flashColor, originalColor, t);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }
}

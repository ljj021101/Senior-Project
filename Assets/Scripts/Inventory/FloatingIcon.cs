using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    public float floatSpeed = 0.5f;
    public float fadeSpeed = 1f;
    private SpriteRenderer spriteRenderer;
    private float lifetime = 1.5f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        Color color = spriteRenderer.color;
        color.a -= fadeSpeed * Time.deltaTime;
        spriteRenderer.color = color;
    }

    public void SetIcon(Sprite sprite, Color tint)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        //spriteRenderer.color = tint;
    }
}
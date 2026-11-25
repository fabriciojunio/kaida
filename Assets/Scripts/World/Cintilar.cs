using UnityEngine;

/// <summary>Faz uma tocha tremular (cor e escala), como fogo de verdade.</summary>
public class Cintilar : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 escala;
    float fase;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        escala = transform.localScale;
        fase = transform.position.x;
    }

    void Update()
    {
        float t = Time.time * 9f + fase;
        float k = 0.5f + 0.25f * Mathf.Sin(t) + 0.25f * Mathf.Sin(t * 2.3f + 1f);
        if (sr != null) sr.color = Color.Lerp(new Color(1f, 0.75f, 0.5f), Color.white, k);
        transform.localScale = new Vector3(escala.x, escala.y * (0.96f + 0.04f * k), escala.z);
    }
}

using UnityEngine;

/// <summary>
/// Ponto de luz flutuando no ar (vaga-lume na mata, esporo de lúmen na
/// caverna). Só decoração: dá vida ao cenário sem pedir nada do jogador.
/// O sprite é gerado aqui, então não depende de nenhum asset.
/// </summary>
public class Vagalume : MonoBehaviour
{
    public Color cor = new Color(1f, 0.9f, 0.5f);
    public float raio = 1.2f;

    Vector3 centro;
    float fase, vel;
    SpriteRenderer sr;

    void Awake()
    {
        centro = transform.position;
        fase = (centro.x * 1.31f + centro.y * 0.77f) % 6.28f;
        vel = 0.35f + Mathf.Abs(Mathf.Sin(centro.x * 3.1f)) * 0.4f;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = Ponto();
        sr.sortingOrder = 6;
    }

    void Update()
    {
        float t = Time.time * vel + fase;
        transform.position = centro + new Vector3(Mathf.Sin(t) * raio, Mathf.Sin(t * 1.7f) * raio * 0.5f, 0f);
        float brilho = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(t * 2.3f));
        sr.color = new Color(cor.r, cor.g, cor.b, brilho);
    }

    static Sprite ponto;
    static Sprite Ponto()
    {
        if (ponto != null) return ponto;
        const int n = 16;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float c = (n - 1) * 0.5f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a * a + (d < 0.25f ? 0.6f : 0f)));
            }
        tex.Apply();
        ponto = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 40f);
        return ponto;
    }
}

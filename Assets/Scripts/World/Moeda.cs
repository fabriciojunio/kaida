using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Moeda de lúmen espalhada pelo mapa. Recompensa quem explora: fica nos
/// cantos, em cima das plataformas e nos caminhos alternativos.
///
/// O sprite é desenhado aqui mesmo (o pacote de arte não tem moeda) e gira
/// achatando na horizontal, como uma moeda de verdade. Pegar uma é discreto
/// de propósito - som curto e um brilho -, para não competir com o anúncio
/// de habilidade, que é o grande momento de cada região.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class Moeda : MonoBehaviour
{
    public string moedaId = "";
    public int valor = 1;

    /// <summary>Total de moedas mudou (HUD escuta isso).</summary>
    public static event Action<int> TotalMudou;

    public static int Total => SaveSystem.Instance != null ? SaveSystem.Instance.Data.moedas : totalSemSave;
    static int totalSemSave;

    SpriteRenderer sr;
    Vector3 basePos;
    float fase;
    bool pega;

    void Awake()
    {
        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteDaMoeda();
        sr.sortingOrder = 8;

        basePos = transform.position;
        // cada moeda gira num tempo diferente, senão o mapa pisca em sincronia
        fase = (transform.position.x * 0.37f + transform.position.y * 0.61f) % 6.28f;
    }

    void Start()
    {
        if (SaveSystem.Instance != null && !string.IsNullOrEmpty(moedaId)
            && SaveSystem.Instance.IsCollected(moedaId))
            gameObject.SetActive(false);
    }

    void Update()
    {
        if (pega) return;
        float t = Time.time * 3f + fase;
        transform.position = basePos + Vector3.up * Mathf.Sin(t * 0.7f) * 0.1f;
        transform.localScale = new Vector3(Mathf.Max(0.15f, Mathf.Abs(Mathf.Cos(t))), 1f, 1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pega || other.GetComponent<PlayerController>() == null) return;
        pega = true;

        if (SaveSystem.Instance != null)
        {
            // não grava em disco a cada moeda: o marco de descanso salva
            if (!string.IsNullOrEmpty(moedaId)) SaveSystem.Instance.MarkCollected(moedaId);
            SaveSystem.Instance.Data.moedas += valor;
        }
        else totalSemSave += valor;

        TotalMudou?.Invoke(Total);
        EfeitosSonoros.Tocar(Som.Moeda);
        StartCoroutine(Sumir());
    }

    IEnumerator Sumir()
    {
        GetComponent<Collider2D>().enabled = false;
        Vector3 inicio = transform.position;
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float k = t / 0.3f;
            transform.position = inicio + Vector3.up * k * 0.8f;
            transform.localScale = Vector3.one * (1f + k * 0.6f);
            sr.color = new Color(1f, 1f, 1f, 1f - k);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    static Sprite sprite;
    public static Sprite SpriteDaMoeda()
    {
        if (sprite != null) return sprite;
        const int n = 16;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;   // combina com a pixel art do jogo
        var borda = new Color(0.55f, 0.33f, 0.05f);
        var miolo = new Color(1f, 0.78f, 0.22f);
        var brilho = new Color(1f, 0.96f, 0.7f);
        float c = (n - 1) * 0.5f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                Color cor = Color.clear;
                if (d <= 7.2f) cor = borda;
                if (d <= 5.9f) cor = miolo;
                if (d <= 5.9f && x - y > 3 && x - y < 6) cor = brilho;        // reflexo
                if (d <= 2.2f && d > 1.2f) cor = Color.Lerp(miolo, borda, 0.5f); // marca central
                tex.SetPixel(x, y, cor);
            }
        tex.Apply();
        sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 24f);
        return sprite;
    }
}

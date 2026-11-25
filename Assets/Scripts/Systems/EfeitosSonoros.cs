using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Efeitos sonoros sintetizados em tempo de execução (o pacote de arte não
/// traz áudio). Cria-se sozinho na primeira chamada e atravessa as cenas,
/// então não precisa ser colocado em nenhuma cena.
///
/// Uso: EfeitosSonoros.Tocar(Som.Pulo);
/// </summary>
public enum Som { Pulo, PuloAereo, Ataque, Dash, Dano, Morte, AcertoInimigo, MorteInimigo, Coleta, Checkpoint, Habilidade, DanoChefe, Moeda }

public class EfeitosSonoros : MonoBehaviour
{
    const int Taxa = 44100;
    const float VolumeBase = 0.9f;

    static EfeitosSonoros instancia;
    readonly Dictionary<Som, AudioClip> clipes = new Dictionary<Som, AudioClip>();
    AudioSource[] fontes;
    int proxima;

    public static void Tocar(Som som)
    {
        if (instancia == null)
        {
            var go = new GameObject("EfeitosSonoros");
            DontDestroyOnLoad(go);
            instancia = go.AddComponent<EfeitosSonoros>();
            instancia.Iniciar();
        }
        instancia.TocarInterno(som);
    }

    void Iniciar()
    {
        // várias fontes para sons poderem se sobrepor (ataque + acerto, etc.)
        fontes = new AudioSource[6];
        for (int i = 0; i < fontes.Length; i++)
        {
            fontes[i] = gameObject.AddComponent<AudioSource>();
            fontes[i].playOnAwake = false;
            fontes[i].spatialBlend = 0f;
        }
    }

    void TocarInterno(Som som)
    {
        if (!clipes.TryGetValue(som, out var clip))
        {
            clip = Gerar(som);
            clipes[som] = clip;
        }
        var f = fontes[proxima];
        proxima = (proxima + 1) % fontes.Length;
        f.pitch = Random.Range(0.95f, 1.05f);   // variação para não soar repetitivo
        f.PlayOneShot(clip, VolumeBase * GameSettings.Volume);
    }

    // ------------------------------------------------------------------ síntese

    static AudioClip Gerar(Som som)
    {
        switch (som)
        {
            case Som.Pulo:          return Varredura("pulo", 0.14f, 300f, 620f, Onda.Quadrada, 0.5f);
            case Som.PuloAereo:     return Varredura("puloAereo", 0.16f, 450f, 900f, Onda.Quadrada, 0.45f);
            case Som.Ataque:        return Ruido("ataque", 0.10f, 0.6f, 2500f, 600f);
            case Som.Dash:          return Ruido("dash", 0.18f, 0.5f, 900f, 3000f);
            case Som.Dano:          return Varredura("dano", 0.25f, 380f, 110f, Onda.Serra, 0.7f, ruido: 0.25f);
            case Som.Morte:         return Varredura("morte", 0.9f, 330f, 55f, Onda.Serra, 0.7f, ruido: 0.15f);
            case Som.AcertoInimigo: return Varredura("acerto", 0.09f, 220f, 90f, Onda.Quadrada, 0.6f, ruido: 0.5f);
            case Som.MorteInimigo:  return Varredura("morteInimigo", 0.35f, 500f, 80f, Onda.Quadrada, 0.55f, ruido: 0.35f);
            case Som.DanoChefe:     return Varredura("danoChefe", 0.18f, 160f, 60f, Onda.Serra, 0.75f, ruido: 0.4f);
            case Som.Moeda:         return Arpejo("moeda", new[] { 988f, 1319f }, 0.045f, 0.32f);
            case Som.Coleta:        return Arpejo("coleta", new[] { 660f, 880f, 1320f }, 0.08f, 0.45f);
            case Som.Checkpoint:    return Arpejo("checkpoint", new[] { 440f, 554f, 659f, 880f }, 0.11f, 0.45f);
            case Som.Habilidade:    return Arpejo("habilidade", new[] { 392f, 523f, 659f, 784f, 1047f }, 0.12f, 0.5f);
        }
        return Varredura("vazio", 0.05f, 440f, 440f, Onda.Seno, 0f);
    }

    enum Onda { Seno, Quadrada, Serra }

    static float Amostra(Onda o, float fase)
    {
        float x = fase - Mathf.Floor(fase);   // fase em ciclos
        switch (o)
        {
            case Onda.Quadrada: return x < 0.5f ? 0.6f : -0.6f;
            case Onda.Serra:    return (x * 2f - 1f) * 0.7f;
            default:            return Mathf.Sin(x * 2f * Mathf.PI);
        }
    }

    /// <summary>Tom que desliza de uma frequência a outra, com queda de volume.</summary>
    static AudioClip Varredura(string nome, float dur, float f0, float f1, Onda onda, float ganho, float ruido = 0f)
    {
        int n = Mathf.RoundToInt(dur * Taxa);
        var d = new float[n];
        var rnd = new System.Random(nome.GetHashCode());
        float fase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float f = Mathf.Lerp(f0, f1, t);
            fase += f / Taxa;
            float env = Envelope(i, n);
            float s = Amostra(onda, fase) * (1f - ruido) + ((float)rnd.NextDouble() * 2f - 1f) * ruido;
            d[i] = s * env * ganho;
        }
        return Criar(nome, d);
    }

    /// <summary>Ruído filtrado (whoosh) com corte que varia no tempo.</summary>
    static AudioClip Ruido(string nome, float dur, float ganho, float corte0, float corte1)
    {
        int n = Mathf.RoundToInt(dur * Taxa);
        var d = new float[n];
        var rnd = new System.Random(nome.GetHashCode());
        float y = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float corte = Mathf.Lerp(corte0, corte1, t);
            float a = 1f - Mathf.Exp(-2f * Mathf.PI * corte / Taxa);   // passa-baixa de 1 polo
            y += a * (((float)rnd.NextDouble() * 2f - 1f) - y);
            d[i] = y * Envelope(i, n) * ganho * 2.5f;
        }
        return Criar(nome, d);
    }

    /// <summary>Notas curtas em sequência (sons de recompensa).</summary>
    static AudioClip Arpejo(string nome, float[] notas, float durNota, float ganho)
    {
        int porNota = Mathf.RoundToInt(durNota * Taxa);
        int cauda = porNota * 2;
        int n = porNota * notas.Length + cauda;
        var d = new float[n];
        for (int k = 0; k < notas.Length; k++)
        {
            int ini = k * porNota;
            int len = porNota + cauda;
            for (int i = 0; i < len && ini + i < n; i++)
            {
                float t = (float)i / Taxa;
                float s = Mathf.Sin(2f * Mathf.PI * notas[k] * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * notas[k] * t);
                d[ini + i] += s * Envelope(i, len) * ganho * 0.6f;
            }
        }
        return Criar(nome, d);
    }

    /// <summary>Ataque rápido (sem clique) e queda exponencial.</summary>
    static float Envelope(int i, int n)
    {
        int ataque = Mathf.Max(1, Mathf.Min(220, n / 10));
        if (i < ataque) return (float)i / ataque;
        float x = (float)(i - ataque) / Mathf.Max(1, n - ataque);
        return Mathf.Pow(1f - x, 2f);
    }

    static AudioClip Criar(string nome, float[] d)
    {
        for (int i = 0; i < d.Length; i++) d[i] = Mathf.Clamp(d[i], -1f, 1f);
        var c = AudioClip.Create("sfx_" + nome, d.Length, 1, Taxa, false);
        c.SetData(d, 0);
        return c;
    }
}

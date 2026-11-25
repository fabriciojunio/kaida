using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Anúncio de habilidade nova. Pegar uma habilidade é o marco de cada região,
/// então não pode passar como uma mensagem qualquer (igual pegar uma moeda):
///
///   1. o tempo desacelera por um instante;
///   2. um clarão dourado cobre a tela e anéis de lúmen saem da Kaida;
///   3. faixas de cinema entram e o nome da habilidade aparece grande,
///      com a descrição e a tecla que a usa.
///
/// Não trava o jogo: some sozinho depois de alguns segundos.
/// </summary>
public class HabilidadeDesbloqueadaUI : MonoBehaviour
{
    static HabilidadeDesbloqueadaUI instancia;

    CanvasGroup grupo;
    Image clarao, faixaCima, faixaBaixo;
    Text rotulo, nome, descricao, tecla;
    Coroutine rodando;

    public static void Mostrar(PlayerController jogador, string titulo, string texto, string teclaDeUso)
    {
        if (instancia == null)
        {
            var go = new GameObject("HabilidadeDesbloqueadaUI");
            instancia = go.AddComponent<HabilidadeDesbloqueadaUI>();
            instancia.Montar();
        }
        if (instancia.rodando != null) instancia.StopCoroutine(instancia.rodando);
        instancia.rodando = instancia.StartCoroutine(instancia.Sequencia(jogador, titulo, texto, teclaDeUso));
    }

    void OnDestroy() { if (instancia == this) instancia = null; }

    IEnumerator Sequencia(PlayerController jogador, string titulo, string texto, string teclaDeUso)
    {
        EfeitosSonoros.Tocar(Som.Habilidade);

        nome.text = titulo.ToUpper();
        descricao.text = texto;
        tecla.text = string.IsNullOrEmpty(teclaDeUso) ? "" : $"[ {teclaDeUso} ]";
        tecla.gameObject.SetActive(!string.IsNullOrEmpty(teclaDeUso));

        if (jogador != null) StartCoroutine(EfeitoNaPersonagem(jogador));

        // câmera lenta curta: o jogador sente que algo mudou
        Time.timeScale = 0.25f;

        grupo.alpha = 1f;
        float t = 0f;
        const float entrada = 0.45f;
        while (t < entrada)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / entrada);
            float suave = 1f - (1f - k) * (1f - k);

            clarao.color = new Color(1f, 0.86f, 0.45f, Mathf.Lerp(0.75f, 0.15f, suave));
            AjustarFaixas(suave);
            nome.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, suave);
            nome.color = Cor(UIKit.CorLumen, suave);
            rotulo.color = Cor(UIKit.CorTexto, suave);
            yield return null;
        }

        // devolve o tempo (se o jogador não pausou nesse meio-tempo)
        if (Mathf.Approximately(Time.timeScale, 0.25f)) Time.timeScale = 1f;

        // descrição entra depois do nome
        t = 0f;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / 0.4f);
            descricao.color = Cor(UIKit.CorTexto, k);
            tecla.color = Cor(UIKit.CorLumen, k);
            clarao.color = new Color(1f, 0.86f, 0.45f, Mathf.Lerp(0.15f, 0f, k));
            yield return null;
        }

        // pulsa o nome enquanto fica na tela
        t = 0f;
        while (t < 3.6f)
        {
            t += Time.unscaledDeltaTime;
            float brilho = 0.85f + 0.15f * Mathf.Sin(t * 5f);
            nome.color = UIKit.CorLumen * brilho + new Color(0, 0, 0, 1f - brilho);
            yield return null;
        }

        t = 0f;
        while (t < 0.6f)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / 0.6f);
            grupo.alpha = 1f - k;
            AjustarFaixas(1f - k);
            yield return null;
        }
        grupo.alpha = 0f;
        rodando = null;
    }

    static Color Cor(Color c, float a) => new Color(c.r, c.g, c.b, a);

    void AjustarFaixas(float k)
    {
        float altura = 46f * k;
        faixaCima.rectTransform.sizeDelta = new Vector2(0f, altura);
        faixaBaixo.rectTransform.sizeDelta = new Vector2(0f, altura);
    }

    // ------------------------------------------------------------ no mundo
    IEnumerator EfeitoNaPersonagem(PlayerController jogador)
    {
        // três anéis de lúmen, um atrás do outro
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(Anel(jogador.transform, 0.9f + i * 0.25f));
            float espera = 0f;
            while (espera < 0.18f) { espera += Time.unscaledDeltaTime; yield return null; }
        }

        // a Kaida brilha dourado e volta à cor normal
        var sr = jogador.spriteRenderer;
        if (sr == null) yield break;
        var original = Color.white;
        float t = 0f;
        while (t < 1.4f && sr != null)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.PingPong(t * 2.5f, 1f) * (1f - t / 1.4f);
            sr.color = Color.Lerp(original, new Color(1f, 0.85f, 0.35f), k);
            yield return null;
        }
        if (sr != null) sr.color = original;
    }

    IEnumerator Anel(Transform alvo, float duracao)
    {
        var go = new GameObject("AnelDeLumen");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteDoAnel();
        sr.sortingOrder = 50;

        float t = 0f;
        while (t < duracao && alvo != null)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duracao;
            go.transform.position = alvo.position + Vector3.up * 0.9f;
            go.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 5f, 1f - (1f - k) * (1f - k));
            sr.color = new Color(1f, 0.88f, 0.5f, 1f - k);
            yield return null;
        }
        Destroy(go);
    }

    static Sprite anel;
    static Sprite SpriteDoAnel()
    {
        if (anel != null) return anel;
        const int n = 128;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float c = (n - 1) * 0.5f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - Mathf.Abs(d - 0.88f) / 0.1f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        tex.Apply();
        anel = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        return anel;
    }

    // ------------------------------------------------------------- montagem
    void Montar()
    {
        DontDestroyOnLoad(gameObject);
        UIKit.ConfigurarCanvas(gameObject, 400, comCliques: false);

        var raiz = new GameObject("Painel", typeof(RectTransform));
        raiz.transform.SetParent(transform, false);
        Esticar(raiz.GetComponent<RectTransform>());
        grupo = raiz.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;

        clarao = Imagem(raiz.transform, "Clarao", new Color(1f, 0.86f, 0.45f, 0f));
        Esticar(clarao.rectTransform);

        faixaCima = Imagem(raiz.transform, "FaixaCima", new Color(0f, 0f, 0f, 0.85f));
        var rc = faixaCima.rectTransform;
        rc.anchorMin = new Vector2(0f, 1f); rc.anchorMax = new Vector2(1f, 1f); rc.pivot = new Vector2(0.5f, 1f);

        faixaBaixo = Imagem(raiz.transform, "FaixaBaixo", new Color(0f, 0f, 0f, 0.85f));
        var rb = faixaBaixo.rectTransform;
        rb.anchorMin = new Vector2(0f, 0f); rb.anchorMax = new Vector2(1f, 0f); rb.pivot = new Vector2(0.5f, 0f);

        // fundo escuro atrás do texto, para ler em qualquer cenário
        var tarja = Imagem(raiz.transform, "Tarja", new Color(0.04f, 0.04f, 0.07f, 0.72f));
        var rt = tarja.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1000f, 118f);
        rt.anchoredPosition = new Vector2(0f, 52f);

        rotulo = UIKit.Titulo(raiz.transform, "— NOVA HABILIDADE —", 12, new Vector2(0f, 96f));
        nome = UIKit.Titulo(raiz.transform, "", 32, new Vector2(0f, 64f));
        nome.rectTransform.sizeDelta = new Vector2(620f, 46f);
        var sombra = nome.gameObject.AddComponent<Outline>();
        sombra.effectColor = new Color(0.35f, 0.2f, 0f, 0.8f);
        sombra.effectDistance = new Vector2(2f, -2f);

        descricao = UIKit.Titulo(raiz.transform, "", 13, new Vector2(0f, 30f));
        descricao.rectTransform.sizeDelta = new Vector2(620f, 22f);
        tecla = UIKit.Titulo(raiz.transform, "", 14, new Vector2(0f, 8f));
    }

    static Image Imagem(Transform pai, string nome, Color cor)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        var img = go.AddComponent<Image>();
        img.color = cor;
        img.raycastTarget = false;
        return img;
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}

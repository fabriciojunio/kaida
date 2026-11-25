using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Abertura do Novo Jogo: conta a história do vale em poucos quadros, com uma
/// frase curta em cada um.
///
///   o lúmen guardava as lembranças → a luz se apagou → o povo esqueceu
///   → Kaida acorda sem memória → os fragmentos devolvem o que se perdeu
///   → o Guardião espera → título
///
/// Cada quadro é uma ilustração montada com a arte do jogo, com movimento
/// lento de câmera (aproximação suave) e mudanças de luz - nada de gente
/// correndo ou pulando: é narração, não gameplay.
///
/// ESPAÇO / ENTER / clique avançam; ESC pula tudo.
/// </summary>
public class CutsceneDeAbertura : MonoBehaviour
{
    static readonly string[] Frases =
    {
        "No Vale de Myrrhen, o lúmen guardava as lembranças de todos.",
        "Até que, numa noite, a luz se apagou.",
        "E o povo esqueceu quem era.",
        "Kaida acordou na orla, também sem memória.",
        "Cada fragmento de lúmen devolve um pouco do que foi esquecido.",
        "No coração do vale, o Guardião do Lúmen espera.",
        "",
    };
    static readonly float[] Duracoes = { 5.5f, 5f, 5f, 5f, 5.5f, 5f, 4.5f };

    const float PalcoL = 600f, PalcoA = 250f, Chao = -100f;

    Action aoTerminar;
    CanvasGroup grupoTela, grupoTudo, grupoFrase;
    Text frase;
    RectTransform palco, cena;
    RecursosDaAbertura arte;
    bool avancar, pular;

    public static CutsceneDeAbertura Iniciar(Action fim)
    {
        var go = new GameObject("CutsceneDeAbertura");
        var c = go.AddComponent<CutsceneDeAbertura>();
        c.aoTerminar = fim;
        c.arte = FindObjectOfType<RecursosDaAbertura>();
        c.Montar();
        c.StartCoroutine(c.Rodar());
        return c;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) pular = true;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0))
            avancar = true;
    }

    IEnumerator Rodar()
    {
        yield return Fade(grupoTudo, 0f, 1f, 0.6f);

        for (int i = 0; i < Frases.Length; i++)
        {
            if (pular) break;
            avancar = false;
            frase.text = Frases[i];
            grupoFrase.alpha = 0f;

            MontarQuadro(i);
            yield return Fade(grupoTela, 0f, 1f, 0.8f);

            // a frase entra um pouco depois da imagem
            float tempo = 0f;
            while (tempo < Duracoes[i] && !avancar && !pular)
            {
                tempo += Time.unscaledDeltaTime;
                grupoFrase.alpha = Mathf.Clamp01((tempo - 0.4f) / 0.8f);
                yield return null;
            }

            yield return Fade(grupoTela, 1f, 0f, pular ? 0.2f : 0.6f);
        }

        grupoTela.alpha = 0f;
        yield return Fade(grupoTudo, grupoTudo.alpha, 1f, 0.2f);
        aoTerminar?.Invoke();

        // a região carrega por baixo do preto; depois o preto se abre
        yield return null;
        yield return null;
        yield return Fade(grupoTudo, 1f, 0f, 0.6f);
        Destroy(gameObject);
    }

    static IEnumerator Fade(CanvasGroup g, float de, float para, float duracao)
    {
        float t = 0f;
        while (t < duracao)
        {
            t += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(de, para, Mathf.Clamp01(t / duracao));
            yield return null;
        }
        g.alpha = para;
    }

    // ------------------------------------------------------------- quadros
    static readonly Color Dourado = new Color(1f, 0.85f, 0.45f);
    static readonly Color CeuQuente = new Color(0.95f, 0.8f, 0.6f);
    static readonly Color MataQuente = new Color(0.5f, 0.6f, 0.42f);
    static readonly Color CeuNoite = new Color(0.22f, 0.2f, 0.35f);
    static readonly Color MataNoite = new Color(0.12f, 0.12f, 0.2f);

    void MontarQuadro(int i)
    {
        if (cena != null) Destroy(cena.gameObject);
        cena = new GameObject("Cena", typeof(RectTransform)).GetComponent<RectTransform>();
        cena.SetParent(palco, false);
        Esticar(cena);
        var camera = cena.gameObject.AddComponent<Aproximar>();

        if (arte == null) return;   // cena de teste sem arte: só a frase

        switch (i)
        {
            case 0: // a vila sob a luz do lúmen
            {
                Fundo(arte.ceu, CeuQuente);
                Fundo(arte.mata, MataQuente);
                Solo(new Color(0.2f, 0.28f, 0.16f));
                Vila(Color.white);
                Lumen(new Vector2(0f, 40f), 1f);
                Particulas(24, Dourado, sobe: true);
                break;
            }
            case 1: // a mesma vila: a luz se apaga
            {
                var ceu = Fundo(arte.ceu, CeuQuente);
                var mata = Fundo(arte.mata, MataQuente);
                Solo(new Color(0.14f, 0.18f, 0.14f));
                var casas = Vila(Color.white);
                var (frasco, halo) = Lumen(new Vector2(0f, 40f), 1f);
                var apagar = cena.gameObject.AddComponent<Anoitecer>();
                apagar.ceu = ceu; apagar.mata = mata; apagar.casas = casas;
                apagar.frasco = frasco; apagar.halo = halo;
                camera.de = 1.06f; camera.para = 1.0f;   // afasta, em vez de aproximar
                break;
            }
            case 2: // o povo esquecido: cascas paradas no escuro
            {
                Fundo(arte.ceu, CeuNoite);
                Fundo(arte.mata, MataNoite);
                Solo(new Color(0.07f, 0.07f, 0.11f));
                Vila(new Color(0.3f, 0.3f, 0.4f));
                var sombra = new Color(0.55f, 0.45f, 0.6f);
                Ator(arte.javaliParado, new Vector2(-150f, Chao), 2.2f, 4f, olharDireita: true).color = sombra;
                Ator(arte.caracol, new Vector2(150f, Chao), 2.2f, 0f).color = sombra;
                var abelha = Ator(arte.abelha, new Vector2(30f, 20f), 1.6f, 10f);
                abelha.color = sombra;
                abelha.gameObject.AddComponent<Flutuar>().amplitude = 8f;
                Particulas(10, new Color(0.6f, 0.5f, 0.8f), sobe: false);
                break;
            }
            case 3: // Kaida acorda na orla
            {
                Fundo(arte.ceu, new Color(0.55f, 0.6f, 0.8f));
                Fundo(arte.mata, new Color(0.25f, 0.32f, 0.38f));
                Solo(new Color(0.12f, 0.16f, 0.18f));
                Brilho(new Vector2(0f, -40f), 220f, new Color(0.8f, 0.9f, 1f, 0.25f));
                var k = Ator(arte.kaidaParada, new Vector2(0f, Chao), 2.2f, 5f, olharDireita: true);
                k.gameObject.AddComponent<Surgir>();
                break;
            }
            case 4: // os fragmentos voltam para ela
            {
                Fundo(arte.ceu, new Color(0.45f, 0.55f, 0.65f));
                Fundo(arte.mata, new Color(0.28f, 0.38f, 0.36f));
                Solo(new Color(0.13f, 0.18f, 0.16f));
                var halo = Brilho(new Vector2(0f, -30f), 200f, new Color(1f, 0.85f, 0.45f, 0.0f));
                halo.gameObject.AddComponent<Acender>().alvo = 0.45f;
                Ator(arte.kaidaParada, new Vector2(0f, Chao), 2.2f, 5f, olharDireita: true);
                for (int n = 0; n < 16; n++)
                {
                    var p = Ponto(Dourado, UnityEngine.Random.Range(6f, 11f));
                    var c = p.gameObject.AddComponent<Convergir>();
                    c.alvo = new Vector2(0f, -20f);
                    c.atraso = n * 0.25f;
                }
                break;
            }
            case 5: // o Guardião no coração do vale
            {
                Fundo(arte.cavernaFundo != null ? arte.cavernaFundo : arte.caverna, new Color(0.35f, 0.3f, 0.5f));
                Brilho(new Vector2(110f, 30f), 260f, new Color(0.5f, 0.6f, 1f, 0.35f));
                var g = Ator(arte.abelha, new Vector2(110f, -30f), 3.1f, 5f);
                g.color = new Color(0.6f, 0.72f, 1f, 0.9f);
                g.gameObject.AddComponent<Flutuar>().amplitude = 6f;
                var k = Ator(arte.kaidaParada, new Vector2(-200f, Chao), 1.5f, 5f, olharDireita: true);
                k.color = new Color(0.4f, 0.4f, 0.5f);
                Particulas(12, new Color(0.6f, 0.75f, 1f), sobe: true);
                break;
            }
            default: // título
            {
                Particulas(30, Dourado, sobe: true);
                var t1 = UIKit.Titulo(cena, "KAIDA", 46, new Vector2(0f, 26f));
                t1.gameObject.AddComponent<Outline>().effectColor = new Color(0.3f, 0.18f, 0f, 0.8f);
                var t2 = UIKit.Subtitulo(cena, "Raízes do Esquecimento", new Vector2(0f, -14f), 16);
                t2.color = UIKit.CorTexto;
                var t3 = UIKit.Subtitulo(cena, "Recupere a memória do vale.", new Vector2(0f, -58f), 11);
                t3.color = UIKit.CorApagada;
                camera.de = 1.0f; camera.para = 1.04f;
                break;
            }
        }
    }

    // --------------------------------------------------- peças do quadro
    RawImage Fundo(Sprite s, Color cor)
    {
        if (s == null) return null;
        var img = new GameObject("Fundo", typeof(RectTransform)).AddComponent<RawImage>();
        img.transform.SetParent(cena, false);
        Esticar(img.rectTransform);
        // um pouco maior que o palco: a aproximação de câmera não mostra borda
        img.rectTransform.offsetMin = new Vector2(-30f, -20f);
        img.rectTransform.offsetMax = new Vector2(30f, 20f);
        var tex = s.texture;
        tex.wrapMode = TextureWrapMode.Repeat;
        img.texture = tex;
        img.color = cor;
        img.raycastTarget = false;
        float aspectoTex = (float)tex.width / tex.height;
        img.uvRect = new Rect(0f, 0f, ((PalcoL + 60f) / (PalcoA + 40f)) / aspectoTex, 1f);
        return img;
    }

    /// <summary>Faixa de chão onde tudo se apoia, com uma borda mais clara de grama.</summary>
    void Solo(Color cor)
    {
        var img = new GameObject("Solo", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(cena, false);
        img.color = cor;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f); rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(80f, PalcoA * 0.5f + Chao + 30f);
        rt.anchoredPosition = new Vector2(0f, -30f);

        var borda = new GameObject("Grama", typeof(RectTransform)).AddComponent<Image>();
        borda.transform.SetParent(img.transform, false);
        borda.color = Color.Lerp(cor, new Color(0.55f, 0.7f, 0.3f), 0.35f);
        borda.raycastTarget = false;
        var b = borda.rectTransform;
        b.anchorMin = new Vector2(0f, 1f); b.anchorMax = new Vector2(1f, 1f);
        b.pivot = new Vector2(0.5f, 1f);
        b.sizeDelta = new Vector2(0f, 4f);
        b.anchoredPosition = Vector2.zero;
    }

    /// <summary>Casas e árvores da vila. Devolve as imagens para poder escurecer depois.</summary>
    Image[] Vila(Color cor)
    {
        var verde = Color.Lerp(cor, new Color(0.8f, 0.9f, 0.75f), cor.r > 0.9f ? 1f : 0f);
        return new[]
        {
            Enfeite(arte.arvore, new Vector2(-250f, Chao), 0.55f, verde * cor),
            Enfeite(arte.casa, new Vector2(-150f, Chao), 1.5f, cor),
            Enfeite(arte.casa, new Vector2(150f, Chao), 1.5f, cor, espelhar: true),
            Enfeite(arte.arvore, new Vector2(255f, Chao), 0.5f, verde * cor),
        };
    }

    (Image, Image) Lumen(Vector2 pos, float intensidade)
    {
        var halo = Brilho(pos, 170f, new Color(1f, 0.85f, 0.45f, 0.45f * intensidade));
        // o lúmen: um núcleo de luz quente, não um objeto
        var f = Ponto(new Color(1f, 0.97f, 0.85f, 1f), 30f);
        f.rectTransform.anchoredPosition = pos;
        f.gameObject.AddComponent<Flutuar>().amplitude = 5f;
        halo.gameObject.AddComponent<Flutuar>().amplitude = 5f;
        return (f, halo);
    }

    Image Ator(Sprite[] quadros, Vector2 pos, float escala, float fps, bool olharDireita = false, bool olhaDireitaNativo = false)
    {
        var img = new GameObject("Ator", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(cena, false);
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;

        // A Kaida é desenhada olhando para a direita; os inimigos, para a
        // esquerda. Espelha só quando o lado pedido não é o da arte.
        bool nativoDireita = quadros == arte.kaidaParada || quadros == arte.kaidaCorrendo || olhaDireitaNativo;
        if (olharDireita != nativoDireita) rt.localScale = new Vector3(-1f, 1f, 1f);

        var anim = img.gameObject.AddComponent<AnimarQuadros>();
        anim.quadros = quadros;
        anim.fps = fps;
        anim.escala = escala;
        anim.Aplicar(0);
        return img;
    }

    Image Enfeite(Sprite s, Vector2 pos, float escala, Color cor, bool espelhar = false)
    {
        if (s == null) return null;
        var img = Ator(new[] { s }, pos, escala, 0f, olhaDireitaNativo: true, olharDireita: !espelhar);
        img.color = cor;
        return img;
    }

    Image Brilho(Vector2 pos, float tamanho, Color cor)
    {
        var img = Ponto(cor, tamanho);
        img.name = "Brilho";
        img.rectTransform.anchoredPosition = pos;
        return img;
    }

    Image Ponto(Color cor, float tamanho)
    {
        var img = new GameObject("Luz", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(cena, false);
        img.sprite = SpriteDeBrilho();
        img.color = cor;
        img.raycastTarget = false;
        img.rectTransform.sizeDelta = Vector2.one * tamanho;
        return img;
    }

    void Particulas(int n, Color cor, bool sobe)
    {
        for (int i = 0; i < n; i++)
        {
            var p = Ponto(new Color(cor.r, cor.g, cor.b, 0.7f), UnityEngine.Random.Range(4f, 9f));
            p.gameObject.AddComponent<ParticulaDeLumen>().velocidade = sobe ? 1f : 0.35f;
        }
    }

    static Sprite brilho;
    static Sprite SpriteDeBrilho()
    {
        if (brilho != null) return brilho;
        const int n = 64;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        float c = (n - 1) * 0.5f;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        tex.Apply();
        brilho = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        return brilho;
    }

    // -------------------------------------------------------------- montagem
    void Montar()
    {
        DontDestroyOnLoad(gameObject);
        UIKit.ConfigurarCanvas(gameObject, 900, comCliques: false);

        var raiz = new GameObject("Raiz", typeof(RectTransform));
        raiz.transform.SetParent(transform, false);
        Esticar(raiz.GetComponent<RectTransform>());
        grupoTudo = raiz.AddComponent<CanvasGroup>();
        grupoTudo.alpha = 0f;

        var fundo = new GameObject("Fundo", typeof(RectTransform)).AddComponent<Image>();
        fundo.transform.SetParent(raiz.transform, false);
        fundo.color = new Color(0.02f, 0.02f, 0.04f, 1f);
        Esticar(fundo.rectTransform);

        var tela = new GameObject("Tela", typeof(RectTransform));
        tela.transform.SetParent(raiz.transform, false);
        Esticar(tela.GetComponent<RectTransform>());
        grupoTela = tela.AddComponent<CanvasGroup>();
        grupoTela.alpha = 0f;

        palco = new GameObject("Palco", typeof(RectTransform)).GetComponent<RectTransform>();
        palco.SetParent(tela.transform, false);
        palco.anchoredPosition = new Vector2(0f, 28f);
        palco.sizeDelta = new Vector2(PalcoL, PalcoA);
        palco.gameObject.AddComponent<RectMask2D>();

        // bordas esfumaçadas nas laterais do palco, para a imagem "nascer" do escuro
        foreach (int lado in new[] { -1, 1 })
        {
            var v = new GameObject("Vinheta", typeof(RectTransform)).AddComponent<Image>();
            v.transform.SetParent(tela.transform, false);
            v.sprite = SpriteDeBrilho();
            v.color = new Color(0.02f, 0.02f, 0.04f, 1f);
            v.raycastTarget = false;
            v.rectTransform.anchoredPosition = new Vector2(lado * (PalcoL * 0.5f + 40f), 28f);
            v.rectTransform.sizeDelta = new Vector2(160f, PalcoA * 1.6f);
        }

        var caixaFrase = new GameObject("Frase", typeof(RectTransform));
        caixaFrase.transform.SetParent(tela.transform, false);
        Esticar(caixaFrase.GetComponent<RectTransform>());
        grupoFrase = caixaFrase.AddComponent<CanvasGroup>();
        frase = UIKit.Titulo(caixaFrase.transform, "", 16, new Vector2(0f, -124f));
        frase.rectTransform.sizeDelta = new Vector2(640f, 26f);
        frase.color = UIKit.CorTexto;
        frase.fontStyle = FontStyle.Italic;

        var dica = UIKit.Subtitulo(raiz.transform, "ESPAÇO avança  ·  ESC pula", Vector2.zero, 9);
        var rd = dica.rectTransform;
        rd.anchorMin = rd.anchorMax = new Vector2(0.5f, 0f);
        rd.anchoredPosition = new Vector2(0f, 10f);
    }

    static void Esticar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // ------------------------------------------------- pequenos animadores
    class AnimarQuadros : MonoBehaviour
    {
        public Sprite[] quadros;
        public float fps = 10f, escala = 1f;
        Image img; float t; int atual = -1;

        public void Aplicar(int i)
        {
            if (quadros == null || quadros.Length == 0) return;
            if (img == null) img = GetComponent<Image>();
            i %= quadros.Length;
            if (i == atual) return;
            atual = i;
            var s = quadros[i];
            if (s == null) return;
            img.sprite = s;
            var rt = img.rectTransform;
            rt.sizeDelta = s.rect.size * escala;
            rt.pivot = new Vector2(s.pivot.x / s.rect.width, s.pivot.y / s.rect.height);
        }

        void Update()
        {
            if (fps <= 0f) return;
            t += Time.unscaledDeltaTime;
            Aplicar((int)(t * fps));
        }
    }

    /// <summary>Câmera lenta sobre a ilustração: aproxima e desliza de leve.</summary>
    class Aproximar : MonoBehaviour
    {
        public float de = 1.0f, para = 1.07f, duracao = 6f;
        float t; RectTransform rt;
        void Start() => rt = (RectTransform)transform;
        void Update()
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracao));
            rt.localScale = Vector3.one * Mathf.Lerp(de, para, k);
            rt.anchoredPosition = new Vector2(Mathf.Lerp(8f, -8f, k), 0f);
        }
    }

    /// <summary>A luz do lúmen se apaga e a vila escurece.</summary>
    class Anoitecer : MonoBehaviour
    {
        public RawImage ceu, mata;
        public Image[] casas;
        public Image frasco, halo;
        float t;
        Color[] originaisCasas;

        void Start()
        {
            originaisCasas = new Color[casas.Length];
            for (int i = 0; i < casas.Length; i++) if (casas[i] != null) originaisCasas[i] = casas[i].color;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            // segura a luz um instante, ela tremula e apaga
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 1.2f) / 2.2f));
            float tremor = t > 0.6f && t < 1.6f ? (Mathf.Sin(t * 40f) > 0.3f ? 0.4f : 1f) : 1f;

            if (ceu != null) ceu.color = Color.Lerp(CeuQuente, CeuNoite, k);
            if (mata != null) mata.color = Color.Lerp(MataQuente, MataNoite, k);
            for (int i = 0; i < casas.Length; i++)
                if (casas[i] != null) casas[i].color = Color.Lerp(originaisCasas[i], new Color(0.3f, 0.3f, 0.4f), k);
            if (halo != null) halo.color = new Color(1f, 0.85f, 0.45f, 0.45f * (1f - k) * tremor);
            if (frasco != null) frasco.color = new Color(1f, 0.97f, 0.85f, (1f - k) * tremor);
        }
    }

    class Flutuar : MonoBehaviour
    {
        public float amplitude = 6f;
        RectTransform rt; float baseY, fase;
        void Start() { rt = (RectTransform)transform; baseY = rt.anchoredPosition.y; fase = UnityEngine.Random.value * 6f; }
        void Update()
        {
            var p = rt.anchoredPosition;
            p.y = baseY + Mathf.Sin(Time.unscaledTime * 1.6f + fase) * amplitude;
            rt.anchoredPosition = p;
        }
    }

    /// <summary>Surge do escuro, devagar (a Kaida acordando).</summary>
    class Surgir : MonoBehaviour
    {
        Image img; float t;
        void Start() { img = GetComponent<Image>(); img.color = new Color(0f, 0f, 0f, 0f); }
        void Update()
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 3f));
            img.color = new Color(k, k, k, Mathf.Clamp01(k * 2f));
        }
    }

    class Acender : MonoBehaviour
    {
        public float alvo = 0.4f;
        Image img; float t;
        void Start() => img = GetComponent<Image>();
        void Update()
        {
            t += Time.unscaledDeltaTime;
            var c = img.color;
            c.a = Mathf.Lerp(0f, alvo, Mathf.Clamp01(t / 4f)) * (0.85f + 0.15f * Mathf.Sin(t * 3f));
            img.color = c;
        }
    }

    /// <summary>Faísca de lúmen que vem das bordas e entra na Kaida.</summary>
    class Convergir : MonoBehaviour
    {
        public Vector2 alvo;
        public float atraso;
        RectTransform rt; Image img; Vector2 inicio; float t, dur;

        void Start()
        {
            rt = (RectTransform)transform;
            img = GetComponent<Image>();
            Renascer();
            t = -atraso;
        }

        void Renascer()
        {
            float ang = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            inicio = alvo + new Vector2(Mathf.Cos(ang) * 300f, Mathf.Sin(ang) * 130f);
            dur = UnityEngine.Random.Range(1.4f, 2.2f);
            t = 0f;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            if (t < 0f) { img.enabled = false; return; }
            img.enabled = true;
            float k = Mathf.Clamp01(t / dur);
            float suave = k * k;
            // trajetória curva, não reta
            var meio = Vector2.Lerp(inicio, alvo, 0.5f) + new Vector2(0f, 60f);
            var a = Vector2.Lerp(inicio, meio, suave);
            var b = Vector2.Lerp(meio, alvo, suave);
            rt.anchoredPosition = Vector2.Lerp(a, b, suave);
            var c = img.color; c.a = Mathf.Sin(k * Mathf.PI) * 0.9f + 0.1f; img.color = c;
            if (k >= 1f) Renascer();
        }
    }

    class ParticulaDeLumen : MonoBehaviour
    {
        public float velocidade = 1f;
        RectTransform rt;
        float vel, oscila, fase;

        void Start()
        {
            rt = (RectTransform)transform;
            Renascer(UnityEngine.Random.Range(-PalcoA * 0.5f, PalcoA * 0.5f));
        }

        void Renascer(float y)
        {
            rt.anchoredPosition = new Vector2(UnityEngine.Random.Range(-PalcoL * 0.5f, PalcoL * 0.5f), y);
            vel = UnityEngine.Random.Range(8f, 20f) * velocidade;
            oscila = UnityEngine.Random.Range(4f, 14f);
            fase = UnityEngine.Random.Range(0f, 6.28f);
        }

        void Update()
        {
            var p = rt.anchoredPosition;
            p.y += vel * Time.unscaledDeltaTime;
            p.x += Mathf.Sin(Time.unscaledTime * 0.8f + fase) * oscila * Time.unscaledDeltaTime;
            rt.anchoredPosition = p;
            if (p.y > PalcoA * 0.5f + 10f) Renascer(-PalcoA * 0.5f - 10f);
        }
    }
}

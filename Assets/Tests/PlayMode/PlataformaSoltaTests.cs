using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// A laje que se solta ao ser pisada, com a física rodando de verdade.
///
/// Os testes usam durações curtas de propósito: o que precisa ser medido é que
/// ela anda pelo tempo configurado e para no fim dele, e esperar cinco segundos
/// em cada caso só faria a bateria demorar. Que a laje do jogo está configurada
/// com cinco segundos é conferido nos testes de EditMode, que leem a cena.
/// </summary>
public class PlataformaSoltaTests
{
    [SetUp]
    public void Antes()
    {
        CenarioDeTeste.PrepararAmbiente();
        CenarioDeTeste.CriarSistemas();
    }

    [TearDown]
    public void Depois() => CenarioDeTeste.Limpar();

    /// <summary>Laje montada como a do prefab: a posição é a superfície.</summary>
    static PlataformaSolta CriarLaje(Vector2 superficie, int direcao = 1,
                                     float velocidade = 3f, float duracao = 1f)
    {
        var go = new GameObject("PlataformaSolta") { layer = CenarioDeTeste.LayerGround };
        go.SetActive(false);                  // segura o Awake até tudo estar no lugar
        go.transform.position = superficie;

        go.AddComponent<Rigidbody2D>();
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(3f, 1f);
        col.offset = new Vector2(0f, -0.5f);

        var laje = go.AddComponent<PlataformaSolta>();
        laje.direcao = direcao;
        laje.velocidade = velocidade;
        laje.duracao = duracao;

        go.SetActive(true);
        return laje;
    }

    static IEnumerator Esperar(float segundos)
    {
        for (float t = 0f; t < segundos; t += Time.fixedDeltaTime)
            yield return new WaitForFixedUpdate();
    }

    /// <summary>Derruba a Kaida em cima da laje e espera ela encostar.</summary>
    static IEnumerator PisarNaLaje(PlataformaSolta laje, PlayerController kaida)
    {
        kaida.transform.position = laje.transform.position + Vector3.up * 1.5f;
        kaida.SetVelocity(0f, 0f);
        yield return Esperar(0.4f);
    }

    [UnityTest]
    public IEnumerator SemNinguemEncostar_FicaOndeEstaSemCair()
    {
        var laje = CriarLaje(new Vector2(0f, 5f));
        yield return null;

        yield return Esperar(1.5f);

        Assert.AreEqual(PlataformaSolta.Fase.Parada, laje.Estado);
        Assert.AreEqual(0f, laje.transform.position.x, 0.001f, "andou sem ninguém pisar");
        Assert.AreEqual(5f, laje.transform.position.y, 0.001f,
            "caiu sozinha: o corpo devia estar sem gravidade até a hora");
    }

    [UnityTest]
    public IEnumerator SoOJogadorASolta_OutrosCorposNao()
    {
        var laje = CriarLaje(new Vector2(0f, 5f));

        // uma caixa qualquer despencando em cima dela
        var caixa = new GameObject("Caixa") { layer = CenarioDeTeste.LayerGround };
        caixa.transform.position = new Vector2(0f, 8f);
        caixa.AddComponent<BoxCollider2D>();
        caixa.AddComponent<Rigidbody2D>().gravityScale = 3f;

        yield return Esperar(1.5f);

        Assert.AreEqual(PlataformaSolta.Fase.Parada, laje.Estado,
            "qualquer coisa que encoste está soltando a laje, não só o jogador");
    }

    [UnityTest]
    public IEnumerator AoSerPisada_AndaNaDirecaoConfigurada()
    {
        var laje = CriarLaje(new Vector2(0f, 5f), direcao: 1, duracao: 3f);
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);

        Assert.AreEqual(PlataformaSolta.Fase.Andando, laje.Estado, "não saiu do lugar");
        Assert.Greater(laje.transform.position.x, 0.2f, "devia estar indo para a direita");
    }

    [UnityTest]
    public IEnumerator ParaEsquerda_AndaParaOOutroLado()
    {
        var laje = CriarLaje(new Vector2(0f, 5f), direcao: -1, duracao: 3f);
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);

        Assert.Less(laje.transform.position.x, -0.2f, "devia estar indo para a esquerda");
    }

    [UnityTest]
    public IEnumerator Anda_PeloTempoConfigurado_ENaoAlemDele()
    {
        const float velocidade = 3f, duracao = 1f;
        var laje = CriarLaje(new Vector2(0f, 5f), velocidade: velocidade, duracao: duracao);

        // De onde ela parte, antes de qualquer contato. Medir a partir daqui
        // dispensa saber em que instante exato a Kaida encostou: parada ela
        // não anda, então o percurso inteiro é o dos segundos configurados.
        float partida = laje.transform.position.x;

        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);
        yield return Esperar(duracao + 0.3f);

        Assert.AreEqual(PlataformaSolta.Fase.Caindo, laje.Estado,
            $"depois de {duracao}s ela devia ter parado e começado a cair");
        Assert.AreEqual(duracao, laje.TempoAndando, Time.fixedDeltaTime * 2f,
            "andou por um tempo diferente do configurado");

        // margem de um passo de física de cada lado
        float andou = laje.transform.position.x - partida;
        Assert.AreEqual(velocidade * duracao, andou, velocidade * Time.fixedDeltaTime * 2f,
            "a distância não bate com velocidade x tempo");
    }

    [UnityTest]
    public IEnumerator NoFimDoTempo_ParaNaHorizontalECaiPelaGravidade()
    {
        var laje = CriarLaje(new Vector2(0f, 5f));
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);
        yield return Esperar(1.2f);

        float xAoParar = laje.transform.position.x;
        float yAoParar = laje.transform.position.y;
        var rb = laje.GetComponent<Rigidbody2D>();

        Assert.AreEqual(RigidbodyType2D.Dynamic, rb.bodyType, "sem corpo dinâmico ela não cai");

        yield return Esperar(0.5f);

        Assert.Less(laje.transform.position.y, yAoParar - 0.5f, "não caiu");
        Assert.AreEqual(xAoParar, laje.transform.position.x, 0.2f,
            "continuou andando de lado enquanto caía");
    }

    [UnityTest]
    public IEnumerator DuranteAQueda_EncostarDeNovo_NaoReiniciaOMovimento()
    {
        CenarioDeTeste.CriarChao(new Vector2(0f, -1f), new Vector2(60f, 2f));

        var laje = CriarLaje(new Vector2(0f, 5f));
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);

        // anda, para e desce até pousar no chão
        yield return Esperar(3f);
        Assert.AreEqual(PlataformaSolta.Fase.Caindo, laje.Estado);

        // tira a Kaida de perto e traz de volta, para o contato ser novo
        kaida.transform.position = new Vector2(30f, 3f);
        yield return Esperar(0.3f);

        float onde = laje.transform.position.x;
        yield return PisarNaLaje(laje, kaida);
        yield return Esperar(0.5f);

        Assert.AreEqual(PlataformaSolta.Fase.Caindo, laje.Estado,
            "voltou a andar depois de já ter caído");
        Assert.AreEqual(onde, laje.transform.position.x, 0.3f,
            "saiu do lugar de novo com o segundo contato");
    }

    [UnityTest]
    public IEnumerator LevaJuntoQuemEstaEmCima()
    {
        const float velocidade = 3f, duracao = 1f;
        var laje = CriarLaje(new Vector2(0f, 5f), velocidade: velocidade, duracao: duracao);
        var kaida = CenarioDeTeste.CriarKaida(new Vector2(0f, 6.5f));
        yield return PisarNaLaje(laje, kaida);

        float partidaDaKaida = kaida.transform.position.x;
        yield return Esperar(duracao);

        float andou = kaida.transform.position.x - partidaDaKaida;
        // A Kaida não é empurrada por atrito: o PlayerController reescreve a
        // velocidade dela a cada passo. Quem carrega é a laje, de propósito.
        Assert.Greater(andou, velocidade * duracao * 0.6f,
            "a laje escorregou por baixo dos pés e deixou a Kaida plantada no ar");
    }
}

using UnityEngine;

/// <summary>
/// Laje que se solta quando pisada: fica parada no ar, desliza em linha reta
/// por um tempo fixo e então trava e cai.
///
/// <para>O corpo nasce <b>cinemático</b>, e não dinâmico com a gravidade
/// desligada. Os dois ficam parados no ar, mas o dinâmico é empurrado por
/// quem encosta: bastaria chegar correndo para a laje sair do lugar antes da
/// hora, e o requisito é que ela só se mova pelo relógio dela.</para>
///
/// <para>A queda no fim não é uma animação: o corpo vira dinâmico e passa a
/// obedecer à gravidade do mundo, então ela bate no chão, pousa em cima de
/// outra plataforma ou afunda no lago como qualquer outra coisa do cenário.</para>
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlataformaSolta : MonoBehaviour
{
    public enum Fase { Parada, Andando, Caindo }

    [Header("Movimento")]
    [Tooltip("Para onde ela desliza: 1 vai para a direita, -1 para a esquerda.")]
    public int direcao = 1;
    [Tooltip("Unidades por segundo. Uma unidade é um tile.")]
    public float velocidade = 2f;
    [Tooltip("Quanto tempo ela anda antes de travar e cair.")]
    public float duracao = 5f;

    [Header("Queda")]
    [Tooltip("Multiplicador da gravidade depois que o tempo acaba.")]
    public float gravidadeDaQueda = 2.5f;

    [Header("Depois da queda")]
    [Tooltip("Some e volta ao lugar de origem depois de cair. Sem isso ela fica no chão " +
             "para sempre e vira um obstáculo que trava os inimigos.")]
    public bool voltarAoLugar = false;
    [Tooltip("Segundos parada no chão antes de sumir e voltar.")]
    public float esperaParaVoltar = 2.5f;

    /// <summary>Em que ponto do ciclo ela está. Parada, Andando ou Caindo.</summary>
    public Fase Estado { get; private set; } = Fase.Parada;

    /// <summary>Segundos já andados. Serve para conferir a duração de fora.</summary>
    public float TempoAndando { get; private set; }

    Rigidbody2D rb;
    BoxCollider2D caixa;
    PlayerController passageiro;
    Vector3 origem;
    float tempoCaindo;
    bool assentada;
    bool voltando;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        caixa = GetComponent<BoxCollider2D>();

        // Vale tanto para a laje montada pelo gerador quanto para uma colocada
        // à mão na Scene View: de qualquer jeito ela começa parada e sem peso.
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.velocity = Vector2.zero;
        origem = transform.position;
    }

    void OnCollisionEnter2D(Collision2D contato)
    {
        Soltar(contato.collider);
        AnotarPassageiro(contato.collider);
    }

    void OnCollisionStay2D(Collision2D contato) => AnotarPassageiro(contato.collider);

    void OnCollisionExit2D(Collision2D contato)
    {
        if (passageiro != null && contato.collider.GetComponentInParent<PlayerController>() == passageiro)
            passageiro = null;
    }

    /// <summary>
    /// Primeiro contato do jogador, e só o primeiro. Encostar de novo enquanto
    /// ela cai não faz nada: sem essa guarda a laje voltaria a deslizar no meio
    /// da queda, que é justamente o que não pode acontecer.
    /// </summary>
    void Soltar(Collider2D quem)
    {
        if (Estado != Fase.Parada) return;
        if (quem.GetComponentInParent<PlayerController>() == null) return;

        Estado = Fase.Andando;
        TempoAndando = 0f;
        rb.velocity = new Vector2(direcao < 0 ? -velocidade : velocidade, 0f);
    }

    void FixedUpdate()
    {
        if (Estado == Fase.Caindo) { DepoisDaQueda(); return; }
        if (Estado != Fase.Andando) return;

        // Carrega quem está em cima pelo mesmo tanto que ela anda no passo.
        // O PlayerController escreve a velocidade dele direto todo FixedUpdate,
        // então o atrito da física não empurra ninguém: sem isto a laje
        // escorregaria por baixo dos pés da Kaida e a deixaria plantada no ar.
        if (passageiro != null)
            passageiro.rb.position += rb.velocity * Time.fixedDeltaTime;

        // A contagem é no passo da física, que é o mesmo que move a laje.
        // Contar no Update deixaria a parada meio passo fora do lugar, e com o
        // passo padrão de 0,02 os 5 segundos caem exatos em 250 passos.
        TempoAndando += Time.fixedDeltaTime;
        if (TempoAndando < duracao) return;

        Cair();
    }

    void Cair()
    {
        Estado = Fase.Caindo;
        passageiro = null;

        rb.velocity = Vector2.zero;               // para de vez na horizontal
        rb.bodyType = RigidbodyType2D.Dynamic;    // e a partir daqui tem peso
        rb.gravityScale = gravidadeDaQueda;
        // trava o X: caindo, ela não pode ser empurrada de lado por quem encosta
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        tempoCaindo = 0f;
        assentada = false;
    }

    /// <summary>
    /// Depois de pousar, a laje vira cinemática: um corpo dinâmico parado no
    /// chão era empurrado pela Kaida e pelos inimigos, e acabava prendendo os
    /// javalis contra ela. Se configurada, some e volta ao lugar de origem.
    /// </summary>
    void DepoisDaQueda()
    {
        tempoCaindo += Time.fixedDeltaTime;

        if (!assentada && tempoCaindo > 0.25f && Mathf.Abs(rb.velocity.y) < 0.05f)
        {
            assentada = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            tempoCaindo = 0f;
        }

        if (!voltarAoLugar || voltando) return;

        // pousada há um tempo, ou caiu no vazio / na água e nunca pousou
        bool hora = assentada ? tempoCaindo >= esperaParaVoltar : tempoCaindo >= 6f;
        if (hora) StartCoroutine(VoltarAoLugar());
    }

    System.Collections.IEnumerator VoltarAoLugar()
    {
        voltando = true;
        var sprites = GetComponentsInChildren<SpriteRenderer>();

        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            foreach (var sr in sprites) { var c = sr.color; c.a = 1f - t / 0.5f; sr.color = c; }
            yield return null;
        }

        caixa.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        rb.position = origem;
        transform.position = origem;
        passageiro = null;
        Estado = Fase.Parada;
        TempoAndando = 0f;
        assentada = false;
        yield return new WaitForSeconds(0.2f);
        caixa.enabled = true;

        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            foreach (var sr in sprites) { var c = sr.color; c.a = t / 0.5f; sr.color = c; }
            yield return null;
        }
        foreach (var sr in sprites) { var c = sr.color; c.a = 1f; sr.color = c; }
        voltando = false;
    }

    /// <summary>Marca quem está pisando em cima. Quem encosta de lado ou por
    /// baixo continua andando por conta própria.</summary>
    void AnotarPassageiro(Collider2D quem)
    {
        var p = quem.GetComponentInParent<PlayerController>();
        if (p == null) return;

        passageiro = p.transform.position.y >= caixa.bounds.max.y - 0.1f ? p : null;
    }

    void OnDrawGizmosSelected()
    {
        // O caminho que ela vai fazer, para dar para conferir no editor se a
        // laje termina em cima de algo aproveitável.
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        float distancia = (direcao < 0 ? -velocidade : velocidade) * duracao;
        Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.9f);
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(distancia, 0f, 0f));
        Gizmos.DrawWireCube(transform.position + new Vector3(distancia, 0f, 0f), col.size);
    }
}

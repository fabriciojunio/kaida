using UnityEngine;

/// <summary>
/// Sprites que a abertura usa. Fica na cena do menu, preenchido pelo
/// SceneBuilder, porque a cutscene é montada em tempo de execução e sozinha
/// não teria como achar os assets.
/// </summary>
public class RecursosDaAbertura : MonoBehaviour
{
    [Header("Kaida")]
    public Sprite[] kaidaParada;
    public Sprite[] kaidaCorrendo;

    [Header("Inimigos")]
    public Sprite[] javali;
    public Sprite[] javaliParado;
    public Sprite[] abelha;
    public Sprite[] caracol;

    [Header("Cenário")]
    public Sprite ceu;
    public Sprite mata;
    public Sprite caverna;
    public Sprite cavernaFundo;
    public Sprite casa;
    public Sprite arvore;
    public Sprite arvoreVermelha;
    public Sprite frascoLumen;
}

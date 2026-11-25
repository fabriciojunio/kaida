using UnityEngine;

/// <summary>
/// Teclas do jogo num lugar só. Os estados perguntam aqui em vez de cada um
/// ler o Input direto, então mudar um atalho é mudar uma linha.
///
///   Andar    setas  ou  A / D
///   Pular    ESPAÇO, W  ou  seta para cima
///   Atacar   ENTER, J, X, Ctrl esquerdo  ou  clique
///   Descer   S  ou  seta para baixo (atravessa a plataforma)
///   Impulso  Shift (esquerdo ou direito)  ou  K
/// </summary>
public static class Controles
{
    public static bool PularApertou =>
        Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

    public static bool PularSoltou =>
        (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        && !PularSegurando;

    public static bool PularSegurando =>
        Input.GetButton("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

    public static bool AtacarApertou =>
        Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)
        || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.X);

    public static bool DescerApertou =>
        Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);

    public static bool ImpulsoApertou =>
        Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.K);
}

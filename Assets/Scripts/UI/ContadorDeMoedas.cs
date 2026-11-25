using UnityEngine;
using UnityEngine.UI;

/// <summary>Contador de moedas no canto superior direito do HUD.</summary>
public class ContadorDeMoedas : MonoBehaviour
{
    Text rotulo;
    RectTransform icone;
    float pulso;

    void Start()
    {
        Montar();
        Atualizar(Moeda.Total);
        Moeda.TotalMudou += AoMudar;
    }

    void OnDestroy() { Moeda.TotalMudou -= AoMudar; }

    void AoMudar(int total)
    {
        Atualizar(total);
        pulso = 1f;
    }

    void Atualizar(int total)
    {
        if (rotulo != null) rotulo.text = $"x {total}";
    }

    void Update()
    {
        if (pulso <= 0f || icone == null) return;
        pulso = Mathf.Max(0f, pulso - Time.unscaledDeltaTime * 4f);
        icone.localScale = Vector3.one * (1f + pulso * 0.4f);
    }

    void Montar()
    {
        var canvasGO = new GameObject("CanvasMoedas");
        canvasGO.transform.SetParent(transform, false);
        UIKit.ConfigurarCanvas(canvasGO, 100, comCliques: false);

        var img = new GameObject("IconeMoeda", typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(canvasGO.transform, false);
        img.sprite = Moeda.SpriteDaMoeda();
        img.raycastTarget = false;
        icone = img.rectTransform;
        icone.anchorMin = icone.anchorMax = new Vector2(1f, 1f);
        icone.pivot = new Vector2(1f, 1f);
        icone.sizeDelta = new Vector2(16f, 16f);
        icone.anchoredPosition = new Vector2(-62f, -16f);

        rotulo = new GameObject("Total", typeof(RectTransform)).AddComponent<Text>();
        rotulo.transform.SetParent(canvasGO.transform, false);
        rotulo.font = UIKit.Fonte;
        rotulo.fontSize = 14;
        rotulo.color = UIKit.CorLumen;
        rotulo.alignment = TextAnchor.MiddleLeft;
        rotulo.raycastTarget = false;
        var rt = rotulo.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(44f, 18f);
        rt.anchoredPosition = new Vector2(-14f, -15f);
        rotulo.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);
    }
}

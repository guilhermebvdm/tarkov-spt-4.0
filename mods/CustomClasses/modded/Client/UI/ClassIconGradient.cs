using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomClasses.Client;

/// <summary>
///     (015 · 06-fix-02) Gradiente vertical numa <see cref="Image"/> (ícone da classe), espelhando o
///     gradiente dos nomes (<see cref="ClassIdentityView.ApplyGradient(TMPro.TextMeshProUGUI, Color)"/>):
///     topo levemente clareado, base = cor da classe. Como o ícone é uma silhueta BRANCA, colorir os
///     vértices com o degradê tinge a forma com o gradiente — o aspecto "premium" dos ícones de edição
///     do EFT (Unheard/EOD). O Unity UI <c>Image</c> não tem gradiente nativo → usamos um BaseMeshEffect.
/// </summary>
[DisallowMultipleComponent]
internal sealed class ClassIconGradient : BaseMeshEffect
{
    public Color TopColor = Color.white;
    public Color BottomColor = Color.white;

    private readonly List<UIVertex> _verts = new();

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }

        _verts.Clear();
        vh.GetUIVertexStream(_verts);

        float minY = float.MaxValue, maxY = float.MinValue;
        for (var i = 0; i < _verts.Count; i++)
        {
            var y = _verts[i].position.y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        var range = maxY - minY;
        if (Mathf.Approximately(range, 0f))
        {
            range = 1f;
        }

        for (var i = 0; i < _verts.Count; i++)
        {
            var v = _verts[i];
            var t = (v.position.y - minY) / range;     // 0 = base, 1 = topo
            v.color = Color.Lerp(BottomColor, TopColor, t);   // silhueta branca × degradê = ícone com gradiente
            _verts[i] = v;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(_verts);
    }
}

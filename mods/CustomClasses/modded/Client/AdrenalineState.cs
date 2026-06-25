using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.2 — state-machine da Adrenaline (🔧 Fuzileiro).
///     Gatilho (causar/receber dano) abre uma janela de N s; durante a janela, recuo/recarga/ADS melhoram.
///     Re-gatilhar durante a janela RENOVA (renovável). Depois da janela há um cooldown antes de reativar.
///     Usa <c>Time.time</c> (tempo desde o boot do jogo, monotônico) → janelas de raids antigas já expiraram
///     numa raid nova (timestamp no passado), então não precisa de reset explícito entre raids.
/// </summary>
internal static class AdrenalineState
{
    private static float _windowEnd = -9999f;
    private static float _cooldownEnd = -9999f;

    internal static bool IsActive => Time.time < _windowEnd;

    // Diagnóstico (overlay 052)
    internal static float SecondsLeft => Mathf.Max(0f, _windowEnd - Time.time);
    internal static bool OnCooldown => !IsActive && Time.time < _cooldownEnd;

    internal static void Trigger()
    {
        var now = Time.time;

        // Em cooldown (janela acabou e o cd ainda não passou): não reativa.
        if (now >= _windowEnd && now < _cooldownEnd)
        {
            return;
        }

        // Ativa (cd já passou) ou renova (ainda dentro da janela).
        var dur = PerksConfig.AdrenalineDuration?.Value ?? 25f;
        var cd = PerksConfig.AdrenalineCooldown?.Value ?? 120f;
        _windowEnd = now + dur;
        _cooldownEnd = _windowEnd + cd;
    }
}

using System;
using System.Collections;
using Comfort.Common;
using EFT;
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
    private static Coroutine? _watcher;   // 085: handle do watcher de re-sync (1 por janela; parável no Reset)

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

    /// <summary>
    ///     085 — aplica/restaura o reload speed nas TRANSIÇÕES da janela. O <see cref="ReloadSpeedPatch"/> é
    ///     push-based (<c>SetAnimatorAndProceduralValues</c>); o watcher garante que a mudança valha IMEDIATAMENTE ao
    ///     abrir/fechar a janela — sem ele, o efeito só entraria no próximo saque/início de recarga e uma recarga
    ///     iniciada logo após o fechamento poderia sair ainda acelerada. O watcher (1 por janela — renovações
    ///     estendem <c>_windowEnd</c> e o loop segue) re-sincroniza na abertura e de novo quando <c>IsActive</c> vira
    ///     false. Barato (uma checagem por frame durante a janela). NÃO afeta recuo (por-tiro) nem ADS (re-avaliado
    ///     todo frame no <c>UpdateWeaponVariables</c>) — só o reload precisa do empurrão.
    ///     <para>(CR#1) Só arranca com a janela JÁ ATIVA (o <see cref="Trigger"/> roda antes desta chamada) — assim
    ///     um dano em COOLDOWN, onde <c>Trigger</c> é no-op, não dispara <c>SetAnimatorAndProceduralValues</c> à toa.
    ///     (CR#2) Guarda o handle p/ o <see cref="Reset"/> matar um watcher órfão entre raids.</para>
    /// </summary>
    internal static void EnsureReloadResync()
    {
        if (!IsActive || _watcher != null || Plugin.Instance == null)
        {
            return;
        }

        _watcher = Plugin.Instance.StartCoroutine(WatchWindow());
    }

    private static IEnumerator WatchWindow()
    {
        ForceReloadResync();       // abertura: re-empurra o reload speed já acelerado (o patch escala)
        while (IsActive)
        {
            yield return null;     // renovações estendem _windowEnd → o loop segue esperando
        }

        ForceReloadResync();       // janela fechou: IsActive=false → o patch NÃO escala → reload volta ao normal
        _watcher = null;
    }

    private static void ForceReloadResync()
    {
        try
        {
            if (Singleton<GameWorld>.Instance?.MainPlayer?.HandsController is Player.FirearmController fc)
            {
                fc.SetAnimatorAndProceduralValues();   // re-empurra o reload speed → o ReloadSpeedPatch re-avalia IsActive
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] (085) adrenaline reload re-sync falhou: {ex.Message}");
        }
    }

    /// <summary>Reseta janela+cooldown (chamado no início da raid — evita cooldown de raid anterior atravessar).</summary>
    internal static void Reset()
    {
        _windowEnd = -9999f;
        _cooldownEnd = -9999f;

        // (CR#2) mata um watcher órfão de raid anterior (extração no meio da janela) e zera o handle — sem isso o
        // flag latcharia e a 1ª janela da nova raid perderia o force-resync de abertura.
        if (_watcher != null && Plugin.Instance != null)
        {
            Plugin.Instance.StopCoroutine(_watcher);
        }

        _watcher = null;
    }
}

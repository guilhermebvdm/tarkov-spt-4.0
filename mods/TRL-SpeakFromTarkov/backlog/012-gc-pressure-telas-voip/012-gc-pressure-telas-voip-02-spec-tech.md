# 012 — GC Pressure nas Telas de VOIP · Spec Técnica

**Mod:** TRL-SpeakFromTarkov
**Spec funcional:** [012-gc-pressure-telas-voip-01-spec.md](012-gc-pressure-telas-voip-01-spec.md)
**Criado:** 2026-09-09

> Nenhuma referência ao Assembly do EFT é necessária neste item — todo o código tocado é UI própria do mod (`UnityEngine.GUIStyle`/`GUI`/`ConcurrentDictionary`, APIs padrão do Unity/.NET, não do jogo).

## 1. Estratégia

Três correções independentes de GC pressure/IO, todas em `MonoBehaviour.OnGUI()` de telas próprias do mod:

1. **AUD-03-06 (GUIStyle):** aplicar o mesmo padrão de cache lazy já usado em `VoipHUD.cs`/`InRaidVoipHUD.cs` desde a Review 02 (`if (_xStyle == null) _xStyle = new GUIStyle(...)`) em `PlayerVolumeMixerHUD.cs` e `VoiceCalibrationHUD.cs`.
2. **AUD-03-06 (List):** remover os dois `.ToList()` de `MenuVoipHUD.cs` — `ConcurrentDictionary.Values` e `HashSet<string>` já são seguros para `foreach` direto no contexto single-thread do `OnGUI` (packet handling e `OnGUI` rodam ambos na main thread; não há a concorrência que motivaria o snapshot defensivo).
3. **AUD-03-07 + corner case da revisão da spec funcional (flush no fechamento abrupto):** `SetPlayerVolume` para de chamar `SaveConfig()` (I/O síncrono) a cada frame de drag; passa a marcar um flag "dirty" com throttle de 1s, e adiciona um flush garantido em `Close()`, `OnDestroy()` e `OnApplicationQuit()`.

**Alternativa descartada para o flush:** persistir só em `OnApplicationQuit()`. Descartada porque não cobre o caso comum de "ajustei o volume, fechei o mixer com Alt+P, mas a raid continua" — sem o flush em `Close()`, o valor ajustado ficaria só em memória até o jogo fechar de verdade, e um crash nesse meio tempo perderia o ajuste mesmo sem ter sido um "fechamento abrupto" de fato.

## 2. Pontos de patch

N/A — nenhum patch Harmony, nenhuma referência ao Assembly do EFT.

## 3. Novas propriedades F12 (BepInEx)

Nenhuma.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `UI/PlayerVolumeMixerHUD.cs` | MODIFICAR | Cacheia os 6 `GUIStyle` do escopo da janela + os 3 de `DrawPlayerRow` (linhas 275-431 atuais); troca `SaveConfig()` síncrono por dirty-flag com throttle + flush em `Close()`/`OnDestroy()`/`OnApplicationQuit()`. |
| `UI/VoiceCalibrationHUD.cs` | MODIFICAR | Cacheia os 8 `GUIStyle` (linhas 199,207,214,223,268,301,325,419 atuais). |
| `UI/MenuVoipHUD.cs` | MODIFICAR | Remove `.ToList()` em `curChannel.Members` (linha 615) e `activeChannels.Values` (linha 650). |

## 5. Stubs de código

```csharp
// UI/PlayerVolumeMixerHUD.cs — campos de cache (mesmo padrão de VoipHUD.cs/InRaidVoipHUD.cs)
private GUIStyle? _headerStyle, _closeBtnStyle, _subHeaderStyle, _emptyStyle, _resetAllStyle, _doneStyle;
private GUIStyle? _nameStyle, _volLabelStyle, _muteBtnStyle; // usados dentro de DrawPlayerRow

void OnGUI()
{
    if (!IsOpen) return;

    // ANTES: var headerStyle = new GUIStyle(GUI.skin.label) { ... };
    // AGORA:
    if (_headerStyle == null) _headerStyle = new GUIStyle(GUI.skin.label) { /* mesmos ajustes de antes */ };
    if (_closeBtnStyle == null) _closeBtnStyle = new GUIStyle(GUI.skin.button) { /* ... */ };
    if (_subHeaderStyle == null) _subHeaderStyle = new GUIStyle(GUI.skin.label) { /* ... */ };
    if (_emptyStyle == null) _emptyStyle = new GUIStyle(GUI.skin.label) { /* ... */ };
    if (_resetAllStyle == null) _resetAllStyle = new GUIStyle(GUI.skin.button) { /* ... */ };
    if (_doneStyle == null) _doneStyle = new GUIStyle(GUI.skin.button) { /* ... */ };

    // ... resto do corpo de OnGUI usa _headerStyle/_closeBtnStyle/etc. em vez das variáveis locais ...
}

private void DrawPlayerRow(/* assinatura existente */)
{
    if (_nameStyle == null) _nameStyle = new GUIStyle(GUI.skin.label) { /* ... */ };
    if (_volLabelStyle == null) _volLabelStyle = new GUIStyle(GUI.skin.label) { /* ... */ };
    if (_muteBtnStyle == null) _muteBtnStyle = new GUIStyle(GUI.skin.button) { /* ... */ };
    // ... resto do corpo usa _nameStyle/_volLabelStyle/_muteBtnStyle ...
}
```

```csharp
// UI/PlayerVolumeMixerHUD.cs — debounce de SaveConfig com flush garantido

private static volatile bool _saveDirty = false;
private static float _lastSaveRealtime = 0f;
private const float SaveThrottleSeconds = 1.0f;

public static void SetPlayerVolume(string profileId, float volume)
{
    if (string.IsNullOrEmpty(profileId)) return;
    volume = Mathf.Clamp(volume, 0.0f, 2.0f);
    _playerVolumes[profileId] = volume;
    ApplyToSpeaker(profileId); // continua imediato — é o áudio em tempo real, não a persistência

    // ANTES: SaveConfig(); (síncrono, todo frame de drag)
    // AGORA: marca dirty; o Update() do componente drena no máximo 1x/segundo.
    _saveDirty = true;
}

// Corrigido na Review 01 (PA-01-01): Update() já existe (PlayerVolumeMixerHUD.cs:237-252) com
// toda a lógica atual dentro de um único `if (IsOpen)` — o throttle novo entra no MESMO bloco,
// não em um Update() vazio.
void Update()
{
    if (IsOpen)
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        // NOVO — throttle de persistência, ver FlushPendingSave()
        if (_saveDirty && Time.realtimeSinceStartup - _lastSaveRealtime >= SaveThrottleSeconds)
        {
            FlushPendingSave();
        }
    }
}

/// <summary>Ponto único de persistência real. Chamado pelo throttle do Update() e, como
/// garantia contra perda de dado (revisão da spec funcional), em todo caminho de fechamento.</summary>
private static void FlushPendingSave()
{
    if (!_saveDirty) return;
    _saveDirty = false;
    _lastSaveRealtime = Time.realtimeSinceStartup;
    SaveConfig(); // método privado existente, sem mudança no corpo
}

public void Close()
{
    IsOpen = false;
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    SetGameInputBlocked(false);
    FlushPendingSave(); // NOVO — garante persistência ao fechar o mixer, não só ao sair do jogo
}

private void OnDestroy()
{
    // ... limpeza de texturas já existente (linha 79 atual) ...
    FlushPendingSave(); // NOVO
}

private void OnApplicationQuit()
{
    FlushPendingSave(); // NOVO — melhor esforço contra Alt+F4/fechamento abrupto
}
```

```csharp
// UI/MenuVoipHUD.cs — remoção dos .ToList() (linhas 615 e 650 atuais)

// ANTES: foreach (var memberId in curChannel.Members.ToList())
foreach (var memberId in curChannel.Members) // HashSet<string>, mutado só na main thread (packet handler), sem concorrência real com OnGUI
{
    // ... corpo sem mudança ...
}

// ANTES: var channelsList = activeChannels.Values.ToList();
// AGORA: ConcurrentDictionary.Values já é seguro para enumeração direta (snapshot momento-a-momento
// garantido pelo próprio tipo, sem exceção de modificação concorrente).
var channelsCount = activeChannels.Count;
if (channelsCount == 0)
{
    GUI.Label(new Rect(posX + 15, currentY, width - 30, 20), "Nenhum canal ativo no momento.", subStyle);
}
else
{
    float contentHeight = channelsCount * 26f;
    // ... o restante do bloco que usava channelsList.Count itera direto sobre activeChannels.Values ...
    foreach (var ch in activeChannels.Values)
    {
        // ... corpo de desenho de cada canal, sem mudança de lógica ...
    }
}
```

## 6. Fluxo de dados

```
[Drag do slider — OnGUI, várias vezes por frame durante o arraste]
  SetPlayerVolume() → _playerVolumes[id]=vol (imediato) + ApplyToSpeaker() (áudio em tempo real, imediato)
                     → _saveDirty = true (sem I/O aqui)

[Update() do componente, 1x/frame, throttle de 1s]
  se _saveDirty e passou 1s desde o último save → FlushPendingSave() → SaveConfig() → File.WriteAllText

[Fechamento do mixer / destruição do componente / saída do jogo]
  Close() / OnDestroy() / OnApplicationQuit() → FlushPendingSave() (imediato, ignora o throttle)
```

## 7. Riscos e dependências

- **Nenhum patch Harmony envolvido.**
- **`OnApplicationQuit` não é garantia absoluta:** em crash real ou `taskkill` forçado, nenhum hook do Unity roda. O flush em `Close()` cobre o caso de uso mais comum (fechar o mixer ainda dentro do jogo); os outros dois hooks são melhor-esforço, conforme já esperado pela spec funcional (marcado como resolução aceitável do corner case, não como garantia 100%).
- **`DrawPlayerRow` precisa ser um método de instância** (não `static`) pra guardar `_nameStyle`/`_volLabelStyle`/`_muteBtnStyle` como campos de instância — confirmar que já é assim antes de aplicar o stub (a spec funcional já assume isso, mas vale checar na implementação).
- **Nenhuma dependência com os itens 010/011/014/015** — este item é isolado nas 3 telas de UI, sem sobreposição de arquivo com os demais itens desta rodada de auditoria.

## 8. Checklist de implementação

- [x] `PlayerVolumeMixerHUD.cs`: adicionar os 9 campos de `GUIStyle` cacheado, substituir as 9 declarações locais por checagem lazy `if (_x == null)`. **Detalhe além do stub original:** 3 dos 9 estilos (`_nameStyle`, `_volLabelStyle`, `_muteBtnStyle` em `DrawPlayerRow`) têm cor/background dinâmicos por linha (dependem de `isMuted`/`currentVol`) — cacheados sem essas propriedades no bloco `normal`, com a cor/background atualizada por escrita simples a cada chamada (não realocando o `GUIStyle`).
- [x] `PlayerVolumeMixerHUD.cs`: adicionar `_saveDirty`/`_lastSaveRealtime`, trocar a chamada direta a `SaveConfig()` em `SetPlayerVolume` por `_saveDirty = true`, adicionar `FlushPendingSave()` e chamá-la de `Update()` (throttle), `Close()`, `OnDestroy()` e `OnApplicationQuit()`.
- [x] `VoiceCalibrationHUD.cs`: adicionar os 8 campos de `GUIStyle` cacheado, substituir as 8 declarações locais por checagem lazy. `_recStatusStyle` também tem cor dinâmica (`_isRecordingPhase`) — mesmo tratamento do item acima.
- [x] `MenuVoipHUD.cs`: remover os 2 `.ToList()` (linhas 615 e 650), ajustar o cálculo de `contentHeight` pra usar `activeChannels.Count` direto.
- [ ] Testar: abrir e manter aberto cada uma das 3 telas por >30s sem alocação perceptível (profiler ou contagem de `GC.CollectionCount`); arrastar o slider de volume por 5s contínuos e confirmar só 1 escrita em disco por segundo no máximo; fechar o mixer com Alt+P imediatamente após um ajuste e confirmar que o JSON reflete o novo valor. *(Requer teste em jogo.)*
- [ ] Compilar via `dotnet build` com 0 erros/0 avisos. *(Requer `/compile-mod`.)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Flush adicionado em `OnDestroy()` (teardown do componente) além de `Close()`/`OnApplicationQuit()` — nenhum dado pendente sobrevive à destruição do componente. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | UI local, sem interação com `Player`/`GameWorld`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Nenhum patch Harmony, nenhum método virtual do EFT. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Nenhuma API do EFT tocada; só `UnityEngine.GUIStyle`/`System.IO.File`/`ConcurrentDictionary`. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `_saveDirty`/`_lastSaveRealtime` são estáticos mas resetados pela lógica do próprio throttle (não herdam "sujeira" perigosa: no pior caso, uma raid seguinte só teria um save pendente de antes, que o flush resolve de qualquer forma). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo ou alterado. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Nenhum patch Harmony. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | Cache de `GUIStyle` é lazy e nunca invalidado incorretamente — mesmo padrão já validado em produção desde a Review 02 (`VoipHUD.cs`/`InRaidVoipHUD.cs`). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | N/A | Nenhuma referência ao Assembly do EFT neste item. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não se aplica. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + ... — AP-11 | N/A | Nenhum pacote de rede novo. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-09 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-09 | Review 01 aplicada — stub de `Update()` mostrado completo com o bloco `if (IsOpen)` já existente |

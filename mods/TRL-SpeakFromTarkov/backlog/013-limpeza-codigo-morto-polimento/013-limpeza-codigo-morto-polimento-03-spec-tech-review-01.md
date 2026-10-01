# 013 — Limpeza de Código Morto e Polimento · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [013-limpeza-codigo-morto-polimento-02-spec-tech.md](013-limpeza-codigo-morto-polimento-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Local real de `.Enable()` dos patches mortos (AUD-03-15) não é onde a spec supôs | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟡 Importante | Opção B do AUD-03-11 não mostra onde `_voipCaptureStarted` é resetado — sem isso, quebra a 2ª raid | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟡 Importante

**Local real de `.Enable()` dos patches mortos (AUD-03-15) não é onde a spec supôs**

**Problema:** O stub da seção 5 (AUD-03-15) diz: *"REMOVER as 2 chamadas correspondentes em `Init()` (não citadas na spec funcional, mas devem existir junto de `new PlayerInitPatch().Enable();` etc. — confirmar na implementação)"*. Conferi agora: `FikaVoipSendPatch`/`FikaVoipReceivePatch` **não são habilitados em `GameSessionPatcher.Init()`** — são habilitados em `VOIPPlugin.cs:328-329` (`new GameSessionPatcher.FikaVoipSendPatch().Enable();` e a linha seguinte), num ponto de inicialização diferente do `GameSessionPatcher.cs`.

**Por que importa:** Se quem implementar confiar só na suposição da spec e procurar essas chamadas dentro de `GameSessionPatcher.Init()`, não vai achar nada lá — as classes seriam removidas mas as 2 linhas de `.Enable()` em `VOIPPlugin.cs` ficariam órfãs, causando erro de compilação (referência a tipo que não existe mais).

**Sugestão:** Atualizar o stub da seção 5 e o item correspondente no checklist (seção 8) para: *"Remover `FikaVoipSendPatch`/`FikaVoipReceivePatch` de `GameSessionPatcher.cs` (linhas 76-104 atuais) **e** as 2 chamadas `new GameSessionPatcher.FikaVoipSendPatch().Enable();`/`new GameSessionPatcher.FikaVoipReceivePatch().Enable();` em `VOIPPlugin.cs:328-329`."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Stub (§5) e checklist (§8) corrigidos para citar `VOIPPlugin.cs:328-329` como local real das chamadas `.Enable()` a remover.

---

### PA-01-02 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟡 Importante

**Opção B do AUD-03-11 não mostra onde `_voipCaptureStarted` é resetado — sem isso, quebra a 2ª raid**

**Problema:** O stub da Opção B (seção 5) adiciona `_voipCaptureStarted` como guard one-shot em `VoipController.Update()`, com o comentário: *"`_voipCaptureStarted` precisa voltar a false no fim da raid (mesmo ponto onde o resto do estado de raid já é limpo, ex.: `SetGameStateChannel(false)` em `GameWorldDisposePatch`)"* — mas **não mostra o código desse reset**, só descreve onde ele deveria ir.

**Por que importa:** Sem esse reset, `_voipCaptureStarted` fica `true` pra sempre depois da 1ª raid (é um campo de instância de `VoipController`, que — pelo padrão já confirmado em `Core/VoipController.cs:15` — é um singleton que sobrevive entre raids, `Instance { get; private set; }`). Na 2ª raid em diante, `StartVoipCapture()`/`SetGameStateChannel(true)` nunca mais seriam chamados — quebrando a captura de voz completamente a partir da segunda raid. Isso é exatamente o tipo de regressão que o item pretende evitar (AP-01), mas a Opção B, como escrita, introduziria um bug novo se implementada sem esse detalhe.

**Sugestão:** Adicionar ao stub da Opção B, dentro do `Prefix` de `GameWorldDisposePatch` (`GameSessionPatcher.cs:57-72`, que já chama `Core.VoipController.Instance.SetGameStateChannel(false);` na linha 69):
```csharp
internal class GameWorldDisposePatch : ModulePatch
{
    // ... GetTargetMethod() sem mudança ...
    [PatchPrefix]
    static void Prefix()
    {
        if (Core.VoipController.Instance != null)
        {
            Core.VoipController.Instance.SetGameStateChannel(false);
            Core.VoipController.Instance.ResetVoipCaptureStartedFlag(); // NOVO — só necessário se a Opção B for a usada
        }
    }
}
```
E adicionar em `VoipController.cs` um método `internal void ResetVoipCaptureStartedFlag() => _voipCaptureStarted = false;` (ou tornar o campo internamente acessível o suficiente pro patch resetar). Se a Opção A (`MethodType.Async`) funcionar, este ponto todo fica moot — mas a spec precisa estar completa pro caminho B mesmo assim, já que é o fallback documentado.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `ResetVoipCaptureStartedFlag()` adicionado ao stub de `VoipController.cs`, chamado a partir de `GameWorldDisposePatch.Prefix()` (stub novo adicionado à seção 5 da spec técnica).

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).

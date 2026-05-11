# 001 — Stamina e Velocidade por Postura · Review Técnica 05

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [001-stamina-e-velocidade-02-spec-tech.md](001-stamina-e-velocidade-02-spec-tech.md)
**Spec funcional referência:** [001-stamina-e-velocidade-01-spec.md](001-stamina-e-velocidade-01-spec.md)
**Reviews anteriores:** [01](001-stamina-e-velocidade-03-spec-tech-review-01.md) · [02](001-stamina-e-velocidade-03-spec-tech-review-02.md) · [03](001-stamina-e-velocidade-03-spec-tech-review-03.md) · [04](001-stamina-e-velocidade-03-spec-tech-review-04.md)
**Data:** 2026-05-08

> Análise crítica após resolução das 6 PAs da review-04 (resolução por candidatos, cache de `AimDrainRate`, log de reflection, `HandleExpiration`, tolerância 0.0001f, imports). Foco desta rodada: pontos finos de hardening e edge cases que podem aparecer em uso prolongado.
>
> Skills aplicadas: `spt-mod-best-practices` + `csharp-mod-best-practices`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 (✅ **1 resolvido**) · 🟢 Menores: 3 (✅ **3 resolvidos**) · Total: **4 — todos resolvidos** em 2026-05-08
>
> ✅ **Status:** todos os 4 PAs aplicados na spec técnica. **Spec finalizada e pronta para `/build-item`** — esta foi a última rodada de review antes do build.

## Reviews anteriores resolvidas

Todas as 6 PAs da review-04 confirmadas resolvidas na spec atual:

- ✅ PA-04-01 resolvido — `ResolveBackingFieldByCandidates` tenta nome do evento público (`OnValueChanged`/`OnThresholdPass`) primeiro, fallback para `action_3`/`action_1`.
- ✅ PA-04-02 resolvido — `_cachedAimDrainRate` populado em `OnRaidStart`.
- ✅ PA-04-03 resolvido — `HasMissingReflection(out missing)` chamado no `Plugin.Awake` com `LogWarning` listando os campos não resolvidos.
- ✅ PA-04-04 resolvido — `hands.HandleExpiration()` chamado quando drain leva `Current` para 0.
- ✅ PA-04-05 resolvido — Trocado `Mathf.Abs(...) < float.Epsilon` por early-exit `drain < 0.0001f`.
- ✅ PA-04-06 resolvido — `using System.Reflection;` + outros imports adicionados ao `StanceManager`.

## Índice de novos pontos

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-05-01](#pa-05-01) | C — Lógica | 🟡 | `_raidEnded` default `false` permite OnRaidEnd antes de qualquer OnRaidStart — cleanup espúrio | ✅ Resolvido |
| [PA-05-02](#pa-05-02) | B — Edge | 🟢 | `hands.ForceMode == true` é ignorado pelo drain manual — diverge do contrato de `Consume()` | ✅ Resolvido |
| [PA-05-03](#pa-05-03) | A — Gap | 🟢 | Cycle voltar a `Stance.Default` re-arma drain — comportamento correto mas não documentado em §1 | ✅ Resolvido |
| [PA-05-04](#pa-05-04) | A — Gap | 🟢 | `ResolveBackingFieldByCandidates` sem comentário explicando por que tentar nome público antes do `action_N` | ✅ Resolvido |

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

### PA-05-01 · C — Lógica · 🟡 Importante {#pa-05-01}

**`_raidEnded` default `false` permite `OnRaidEnd()` antes de qualquer `OnRaidStart()`**

**Problema:** O stub de `StanceManager` em §5 declara:

```csharp
private static bool _raidEnded;     // default false
```

E `OnRaidEnd` faz:

```csharp
public static void OnRaidEnd()
{
    if (_raidEnded) return;          // idempotente
    _raidEnded = true;
    // ... cleanup ...
}
```

Como o default de `bool` é `false`, na primeiríssima vez que `OnRaidEnd` é chamado (sem nenhum `OnRaidStart` antes), o guard **deixa passar** e o cleanup roda. Isso pode acontecer em cenários reais:

1. **Plugin é recarregado dentro do hideout** (BepInEx F4 reload, raro mas existe). `Awake` registra patches. Se `BaseLocalGame.Stop` for chamado em algum fluxo de saída do hideout/menu, `OnRaidEnd` dispara sem nunca ter havido `OnRaidStart`.
2. **Bug de ordem de patches**: se algum mod externo dispara `BaseLocalGame.Stop` antes do nosso `OnGameStarted` postfix executar, mesma situação.

O cleanup espúrio chama `RemoveStateSpeedLimit` (no-op se nada registrado), `StanceStaminaState.Reset()` (no-op no default), `ResetState()` (efetivo — resetar `CurrentStance` e tac sprint flags do mod existente). Isso pode interromper inicialização do mod.

**Por que importa:** `ResetState()` resetando `CurrentStance` e flags de tac sprint quando NÃO houve raid pode causar bug visual (player no menu/hideout perde stance configurada, dirty-flags zeradas no momento errado). Pequena chance de ocorrer, mas não-zero.

**Sugestão:** Inverter a semântica — `_raidEnded` default `true` ("nenhuma raid começou"), `OnRaidStart` zera, `OnRaidEnd` checa:

```csharp
private static bool _raidEnded = true;     // default: "nenhuma raid começou"

public static void OnRaidStart()
{
    try
    {
        _raidEnded = false;        // raid agora ativa
        // ... resto
    }
    ...
}

public static void OnRaidEnd()
{
    if (_raidEnded) return;        // só executa se raid estava ativa
    _raidEnded = true;
    // ... cleanup ...
}
```

Mais claro e defende contra OnRaidEnd-antes-de-OnRaidStart.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** `_raidEnded` declarado como `private static bool _raidEnded = true;` em [§5 (StanceManager additions)](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código). `OnRaidStart` zera (`_raidEnded = false`); `OnRaidEnd` checa o flag e só executa cleanup se houve raid ativa. Comentário inline explica a semântica.

---

### PA-05-02 · B — Edge · 🟢 Menor {#pa-05-02}

**`hands.ForceMode == true` é ignorado pelo drain manual — diverge do contrato de `Consume()` vanilla**

**Problema:** O stub `TickStanceStamina` faz drain via mutação direta sem checar `ForceMode`. Olhando [`GClass774.Consume` em GClass774.cs:241-275](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L241-L275):

```csharp
public float Consume(...)
{
    if (Multiplier <= 0f) return Current;       // ✓ honramos
    float result = 0f;
    if (!ForceMode)                              // ← honrar
    {
        // ... consume ...
    }
    InvokeChangedAction();
    return result;
}
```

Quando `ForceMode == true`, `Consume` **pula a redução do Current** mas ainda invoca `InvokeChangedAction()`. Nosso drain manual ignora `ForceMode` e sempre muta `Current`.

**Por que importa:** `ForceMode` é raríssimo — provavelmente usado em modo dev/cheat ou em algum power-up temporário do EFT. Mas se outro mod habilita `ForceMode = true` para desativar drenagem temporariamente, **nosso drain ainda roda**, contradizendo intenção do flag.

**Sugestão:** Adicionar guard ao `TickStanceStamina`:

```csharp
if (hands.Multiplier <= 0f) return;
// Honra o ForceMode do GClass774 — Consume pula redução quando ForceMode == true
if (hands.ForceMode) return;
```

Verificar antes se `ForceMode` é `public` (provavelmente sim, está acessível no decompilado). Se for privado, ignorar este PA — risco baixo.

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Confirmado em [`GClass774.cs:91`](../../../../references/eft-decompiled/Assembly-CSharp/GClass774.cs#L91): `public bool ForceMode`. Adicionado guard `if (hands.ForceMode) return;` em [§5 (TickStanceStamina)](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código), logo após o guard do `Multiplier`. Honra contrato vanilla de `Consume()`.

---

### PA-05-03 · A — Gap · 🟢 Menor {#pa-05-03}

**Cycle voltando a `Stance.Default` re-arma o drain — correto, mas não documentado**

**Problema:** O cycle nativo do mod ([StanceManager.cs:140-144](../../modded/StanceManager.cs#L140-L144)) tem transição:

```csharp
Stance.Default => Stance.Stance1,
Stance.Stance1 => Stance.Stance2,
Stance.Stance2 => Stance.Stance3,
Stance.Stance3 => Stance.Default,
```

Quando o jogador completa o cycle (`Stance3 → Default`), o setter da property dispara `OnStanceChanged(Stance3, Default)`, que chama `ApplyStaminaStance(Default)` — **carregando a config de "Stance 0"** (Drain `0.50` por default) e aplicando o speed limit de 90%.

Isso é **correto por design** — Stance 0 é a posição de pronto-de-tiro com drain leve. Mas a spec não menciona explicitamente que cycle volta a re-armar drain. Um leitor casual pode achar que "voltar ao Default" significa "desativar nossa feature".

**Por que importa:** clareza e expectativa. Se um jogador esperar que "passar pelo Default no cycle" desliga drain, vai estranhar a barra continuar caindo. A spec funcional explica que Stance 0 = vanilla, mas a relação com o cycle não está explícita.

**Sugestão:** Adicionar parágrafo curto em §1 ("Mapeamento Stance 0 ↔ Default do mod existente"):

```markdown
**Cycle e Stance 0:** o cycle nativo (`_EnableStance1/2/3 in Cycle`) transiciona
Default → Stance1 → Stance2 → Stance3 → Default. Quando o cycle volta a `Stance.Default`,
nosso `OnStanceChanged` dispara e aplica a config da Stance 0 (drain leve, velocidade 90%
por default) — não é "desligar a feature", é simplesmente reentrar na configuração base.
Para desativar drain, o jogador deve setar `Stance 0 Stamina Mode = None` no F12.
```

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** Parágrafo "Cycle e Stance 0" adicionado em [§1 "Modelo de stance"](001-stamina-e-velocidade-02-spec-tech.md#1-estratégia), explicitando que cycle voltando a Default re-arma drain e como desativar via F12.

---

### PA-05-04 · A — Gap · 🟢 Menor {#pa-05-04}

**`ResolveBackingFieldByCandidates` sem comentário explicando a estratégia**

**Problema:** O helper em §5 tem assinatura clara mas o **porquê** dos candidatos múltiplos não está no código:

```csharp
private static FieldInfo ResolveBackingFieldByCandidates(Type t, params string[] candidates)
{
    foreach (var name in candidates)
    {
        var f = AccessTools.Field(t, name);
        if (f != null && f.FieldType == typeof(Action)) return f;
    }
    return null;
}
```

E o uso:

```csharp
private static readonly FieldInfo _onValueChangedBacking =
    ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnValueChanged), "action_3");
```

Quem ler o código sem ter visto a discussão no review pode pensar:
- "Por que `OnValueChanged` (que é nome de evento, não field)?"
- "Por que `action_3`?"
- "Qual ordem importa?"

**Por que importa:** manutenibilidade. Próximo dev fazendo update do mod para nova versão do EFT precisa entender por que essa estratégia existe — caso contrário pode "simplificar" (ex.: remover o `nameof(...)`) e introduzir fragilidade.

**Sugestão:** Adicionar comentário XMLDoc ao helper:

```csharp
/// <summary>
/// Resolve um backing field privado de event Action tentando uma lista ordenada
/// de candidatos. Estratégia: passar primeiro o nome do **evento público** (estável
/// se BSG mantiver a API), depois nomes do **decompilador ILSpy** (action_N, podem
/// mudar entre patches do EFT). Retorna o primeiro Action field encontrado, ou null
/// se nenhum candidato bater. Falha gera log warning no Awake (ver HasMissingReflection).
/// </summary>
private static FieldInfo ResolveBackingFieldByCandidates(Type t, params string[] candidates)
{
    foreach (var name in candidates)
    {
        var f = AccessTools.Field(t, name);
        if (f != null && f.FieldType == typeof(Action)) return f;
    }
    return null;
}
```

**Decisão:** `[x]` **Aceitar sugestão** · ✅ Resolvido em 2026-05-08
**Resolução:** XMLDoc adicionada em [§5 (StanceManager additions)](001-stamina-e-velocidade-02-spec-tech.md#5-stubs-de-código) explicitando estratégia de candidatos: nome do evento público primeiro (estável), `action_N` do decompilador como fallback, falha logada via `HasMissingReflection` no Awake.

---

## Próximos passos

✅ **Todas as 4 PAs aplicadas em 2026-05-08.** Esta foi a **última rodada de review antes do build**.

Próximo passo: executar `/build-item mods\stancesAndCameraPositionSPT4.0.11\backlog\001-stamina-e-velocidade\` para implementar a feature em `mods/stancesAndCameraPositionSPT4.0.11/modded/`.

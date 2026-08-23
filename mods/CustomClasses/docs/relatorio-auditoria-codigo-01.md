---
title: "Relatório de Auditoria Técnica de Código — CustomClasses v0.16.8 (Review 01, --perf)"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (auditoria preventiva de performance 2026-08-22)
---

# Relatório de Auditoria Técnica de Código — CustomClasses v0.16.8 (Review 01, --perf)

> ⚠️ **Revisão 01 (2026-08-22)** — este relatório passou por revisão adversarial antes de qualquer Decisão ser marcada: [relatorio-auditoria-codigo-01-review-01.md](relatorio-auditoria-codigo-01-review-01.md). Resultado: **1 achado não detectado** (`AUD-01-08`, acrescentado abaixo), 2 afirmações corrigidas, 2 achados reclassificados como fora do escopo de performance, evidência recontada e o plano de instrumentação corrigido. As anotações `⚠️ Revisão 01` ao longo do texto marcam cada ponto; o texto original foi preservado (relatórios são imutáveis).

## 1. Resumo Executivo da Auditoria

Auditoria de performance do **CustomClasses v0.16.8** (client [modded/Client/](../modded/Client/) 9.444 linhas · server [modded/Server/](../modded/Server/) 6.467 linhas), commit base `8732d47c`, branch `perf-customclasses-optimize`. **Modo preventivo**: diferente das outras frentes desta rodada, aqui **não há achado prévio de investigação** — na raid-baseline de 2026-08-22 o mod emitiu apenas 9 linhas de log e não apareceu em nenhum spike. A auditoria é estática, cruzada com o decompile do EFT 0.16.9 (lido do checkout principal `tarkov-spt-4.0/references/eft-decompiled/`, ausente neste worktree) e com o `spt-source` 4.0.

**Escopo priorizado conforme o direcionamento da sessão:** superfícies **client em raid** (patches de perk, gates por jogador, hooks de som/dano/movimento). O editor web Blazor (`Server/Web`, `Server/wwwroot`) ficou de fora; do server foram auditadas apenas as rotas que o client consome no raid-start.

### Veredicto

> ⚠️ **Revisão 01 — ressalva de cobertura (RV-03/RV-08).** O veredicto abaixo vale para **CPU em raid**, que é onde a varredura foi integral. A lente de **retenção/VRAM** (`spt-memory-leak-analysis`) rodou só como grep, e contra superfícies de raid — foi ali que escapou o `AUD-01-08`. Cobertura real: ~55% das 9.444 linhas do client lidas por inteiro (todos os patches de raid, hubs de estado, cache de ícones e as rotas de servidor consumidas em raid); o resto por greps direcionados da taxonomia. Recomendação: `/analyze-memory-leak CustomClasses` como frente própria.

**O client não tem ofensor de performance em raid.** As duas únicas superfícies verdadeiramente por-frame do mod (`Plugin.OnGUI` e `FadeIn.Update`) estão desligadas por default ou se auto-desligam; **todos** os 33 patches Harmony têm early-return barato no topo, e o gate de identidade de instância (`ReferenceEquals(__instance, MainPlayer…)` — regra 075) está íntegro em 100% dos patches que rodam para bots/peers. Não há LINQ, alocação, `GameObject.Find` nem reflection não cacheada em nenhum caminho de raid. Isso é coerente com a baseline de 9 linhas de log.

Os achados abaixo são, em ordem: **um** desperdício real (mas na superfície de **menu**, não em raid) e **cinco melhorias sistêmicas/de higiene** de custo unitário baixo. Nenhum deles justifica sozinho uma rodada de otimização — a decisão natural é agrupar AUD-01-01 (o único com ganho perceptível) com os 🔵 mais baratos numa única rodada, ou classificar tudo como dívida anotada.

### Tabela Resumo de Severidade

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 0 | — |
| 🟠 **Alto** | 0 | — |
| 🟡 **Médio** | **2** | Polling de `GameObject.Find` global (até 60 frames) a cada abertura do menu, sem short-circuit quando o Menu-Overhaul está ausente · **[Revisão 01] cache de textura tingida sem teto, alimentado pelo picker de cor do F12 (`AUD-01-08`)** |
| 🔵 **Baixo** | 5 | Comparação de string no gate mais chamado do mod; ~~multiplicidade de patches no mesmo alvo~~ᴬ; reflection crua no funil de som; log de diagnóstico sem gate; ~~type-name por disparo~~ᴬ |
| 💡 **Otimização** | 1 | Bloco agregado de 4 melhorias preventivas |

ᴬ ⚠️ **Revisão 01 (RV-04):** `AUD-01-03` e `AUD-01-06` **não são achados de performance** pelo critério deste próprio relatório (ganho de CPU declaradamente desprezível; benefício real é manutenção/conformidade). Ficam registrados para rastreabilidade, mas **saem da recomendação de rodada** — encaminhar por `/code-review`.

~~**Nível de evidência:** Forte 6 · Suspeita 0 · Melhoria preventiva 1. Não há Suspeita porque nenhum eixo de custo ficou em aberto na leitura — o que falta medir é a **magnitude** de AUD-01-01 no setup do usuário (ver Instrumentação).~~

> ⚠️ **Revisão 01 (RV-05) — evidência recontada: Forte 4 · Suspeita 2 · Preventiva 1.** A declaração original confundia **mecanismo** (provado por leitura — Forte em todos) com **magnitude** (o produto real frequência × entidades no setup do usuário). `AUD-01-01` e `AUD-01-08` têm magnitude **desconhecida** e o próprio relatório propõe instrumentação para eles — pela definição da skill, isso é **Suspeita**, não Forte. Registre-se também que este relatório **não contém uma única medição**: é 100% estático.

---

## 2. Tabela Geral de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟡 Médio | `Client/Patches/MenuClassIdentityPatch.cs:99-161` | UNITY + FREQ | Coroutine faz `GameObject.Find` global por frame, até 60 frames, a cada `MenuScreen.Show`; sem Menu-Overhaul instalado paga o máximo sempre, para nada |
| `AUD-01-02` | 🔵 Baixo | `Client/SkillMultipliers.cs:60-63` | Custo unitário do gate | `IsLocalClass` resolve a classe por `string.Equals(OrdinalIgnoreCase)` em 42 call-sites, vários em superfície per-frame |
| `AUD-01-03` | 🔵 Baixo | `Client/Patches/ClassWeaponPatches.cs`, `BulwarkPatch.cs`, `ClassCombatHealthPatches.cs`, `AdrenalineTriggerPatch.cs` | PATCH (multiplicidade) | 4 patches em `Player.ApplyDamageInfo`, 4 em `PWA.Shoot`, 3 em `SetAnimatorAndProceduralValues`, 2 em `TotalErgonomics` — cada um repete o mesmo gate |
| `AUD-01-04` | 🔵 Baixo | `Client/Patches/SilentKnifePatch.cs:84-85` | IO (reflection crua) | `FieldInfo.GetValue` + `PropertyInfo.GetValue` por clip de som não-arma; o mod já usa delegate compilado no `SainSoundPatch` |
| `AUD-01-05` | 🔵 Baixo | `Client/Patches/SkillsClassTabPatch.cs:435-445` | LOG | Diagnósticos `[053-tabicon]`/`[053-tabtext]` em `LogInfo` sem gate de config — sobra de um fix já fechado |
| `AUD-01-06` | 🔵 Baixo | `Client/Patches/WeaponMasteryPatches.cs:57` | Custo unitário | `p.GetType().Name.IndexOf("Hideout", …)` por disparo de underbarrel, onde `is HideoutPlayer` resolve |
| `AUD-01-07` | 💡 Otimização | (4 pontos — ver §3) | Preventivas agregadas | Subscrição de bala sempre ativa · poll por frame da janela de Adrenalina · string de tooltip por linha no scroll · lista de perks por Repaint com diag ligado |
| `AUD-01-08` | 🟡 Médio | `Client/UI/ClassIconCache.cs:74-137` | GROW + UNITY + ALLOC | **[Revisão 01]** `TintedCache` guarda uma textura 256×256 por **cor** e nunca libera; o picker de cor do F12 gera uma entrada por evento de mudança, em dois consumidores |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Polling de `GameObject.Find` global no menu (até 60 frames por abertura)

- **Severidade:** 🟡 Médio
- **Evidência:** ~~Forte~~ → ⚠️ **Revisão 01 (RV-05): Suspeita** — mecanismo provado por leitura, **magnitude não medida**. Pela definição da skill, eixo em aberto que exige instrumentação antes de virar refactor é Suspeita.
- **O que refutaria este achado** *(Revisão 01, RV-10)*: se no setup do usuário o painel do Menu-Overhaul aparecer em 1–2 quadros, o custo real é quase zero e a correção **não paga** o ciclo de build + reinstalar + reiniciar o EFT + validar. Medir (INSTR-1) **antes** de decidir.
- **Execução:** per-frame por até 60 frames × 1 entidade × por `MenuScreen.Show` (toda volta ao menu principal, ou seja ≥1× por raid) e por `RefreshColors` (cada evento do picker de cor no F12). Custo unitário **alto**: `GameObject.Find` percorre a hierarquia inteira da cena por nome, e a cena do menu principal é pesada.
- **Localização no Mod:** [MenuClassIdentityPatch.cs:99-161](../modded/Client/Patches/MenuClassIdentityPatch.cs#L99-L161) (o `for` de 60 iterações em `:103-116`, o `GameObject.Find` em `:105`, os 90 frames ociosos em `:155-158`, o segundo `Find` em `:173`)
- **Referência Cruzada:** `MenuOverhaulBridge.IsPresent` — [MenuOverhaulBridge.cs:72](../modded/Client/UI/MenuOverhaulBridge.cs#L72) — já existe, é um `Chainloader.PluginInfos.ContainsKey` (O(1)) e **não é consultado** por este patch.
- **Causa Raiz:** a coroutine espera o painel que o **Menu-Overhaul** monta de forma assíncrona (`MainMenuPlayerModelView/BottomField/NicknameText`) e faz isso com um `GameObject.Find` global **a cada frame**, até 60 vezes. O próprio XMLdoc do patch declara a premissa: *"Sem o Menu-Overhaul o painel não existe → no-op"*. Só que esse no-op **não é gratuito**: sem o mod, `nick` nunca deixa de ser null, o loop roda as 60 iterações completas, e ainda assim a coroutine segue para 90 frames de `yield return null` + mais um `GameObject.Find("Environment UI")` em `FixTopGlow`.
  - **Regime A (Menu-Overhaul ausente):** 60 varreduras globais + 90 frames de coroutine viva + 1 `Find` final, **toda vez**, entregando zero.
  - **Regime B (Menu-Overhaul presente — o caso do usuário):** o loop sai assim que o painel aparece; o custo é o número real de frames até isso acontecer (desconhecido — pode ser 1, pode ser dezenas, porque o MO monta o painel de forma assíncrona). Os 90 frames de espera do `FixTopGlow` acontecem nos dois regimes.
- **Impacto Técnico Real:** hitch na abertura do menu principal, proporcional ao tamanho da hierarquia da cena. Não afeta FPS em raid. É o maior produto `custo unitário × frequência` que a auditoria encontrou no mod inteiro.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem atual:* `GameObject.Find` global por frame, sem short-circuit e sem cache.
  - *Abordagem otimizada:* (1) bail imediato quando o MO não está carregado; (2) cachear o `Transform` achado enquanto a instância viver (o `==` do Unity detecta destruição); (3) espaçar o poll (a cada 3 frames em vez de todo frame) — 60 frames de janela viram 20 varreduras no pior caso; (4) trocar os 90 frames fixos do `FixTopGlow` por um `WaitForSeconds` equivalente (não precisa de granularidade de frame).

```csharp
private static Transform? _cachedPmv;   // válido enquanto a instância viver (== do Unity detecta destruição)

private static IEnumerator ApplyToMenu(MenuScreen menu)
{
    // (1) sem Menu-Overhaul o painel NUNCA existe → 60 varreduras globais da cena para nada.
    if (!MenuOverhaulBridge.IsPresent)
    {
        yield break;
    }

    TextMeshProUGUI? nick = null;
    for (var i = 0; i < 60 && nick == null; i++)
    {
        // (2) cache + (3) poll espaçado: 1 varredura a cada 3 frames, mesma janela de espera.
        if (_cachedPmv == null)
        {
            _cachedPmv = GameObject.Find("MainMenuPlayerModelView")?.transform;
        }

        nick = _cachedPmv?.Find("BottomField/NicknameText")?.GetComponent<TextMeshProUGUI>();
        if (nick == null)
        {
            yield return null;
            yield return null;
            yield return null;
            i += 2;
        }
    }
    // … resto inalterado …
```

- **Como validar:** contador `// PERF-INSTR AUD-01-01` de iterações do loop + `Stopwatch` da coroutine inteira, 1 linha por `Show`. Cenário pareado: abrir o menu principal 5× (voltando de raid) antes e depois, com e sem o Menu-Overhaul carregado. Critério: iterações do `Find` caem de ≤60 para ≤20 (regime B) e para **0** (regime A); tempo total da coroutine cai proporcionalmente. Não-regressão: ícone + linha de classe + AccentColor + glow do topo continuam corretos no menu com o MO instalado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · `IsLocalClass` compara string com `OrdinalIgnoreCase` — 42 call-sites, vários per-frame

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Execução:** per-frame × 1 entidade (o gate de instância já barrou bots antes) × raid inteira · sem acúmulo. Custo unitário **baixo mas não nulo**: `OrdinalIgnoreCase` faz dobra de caixa caractere a caractere (`"Combat Medic"` = 12 chars). Volume: `ClassMoveSpeed.Apply` avalia até 3 `IsLocalClass` por leitura de `MaxSpeed`, e `MaxSpeed` é lido 3× por `UpdateCharacterControllerSpeedLimit` (decompile `MovementContext.cs:4181` → `SetCharacterMovementSpeed` lê em `:2375` e `:2377`, mais `UpdateCovertEfficiency` em `:2368`), que roda por frame de movimento → **~9 comparações/frame** só nesse patch, mais 2/frame no `StancesArmStaminaBridge.Factor`.
- **Localização no Mod:** [SkillMultipliers.cs:60-63](../modded/Client/SkillMultipliers.cs#L60-L63) (a comparação) · [ClassMovementPatches.cs:53-77](../modded/Client/Patches/ClassMovementPatches.cs#L53-L77) e [StancesArmStaminaBridge.cs:91-99](../modded/Client/StancesArmStaminaBridge.cs#L91-L99) (os call-sites per-frame)
- **Referência Cruzada:** `MovementContext.MaxSpeed` — decompile EFT 0.16.9, `EFT/MovementContext.cs:910`; origem por-frame provada em `BotMover.cs:930`/`:985` → `Player.ChangeSpeed` → `MovementState.ChangeSpeed` (`MovementState.cs:248`).
  > ⚠️ **Revisão 01 (RV-09):** as referências ao decompile são citadas **em texto, sem link**, porque o dump é gitignored e **não existe neste worktree** (a auditoria o leu do checkout principal `tarkov-spt-4.0/`). Para gerá-lo aqui: `bash scripts/decompile-eft.sh` (exige o jogo instalado).
- **Causa Raiz:** a classe do perfil é um dado **imutável durante a raid** (só muda em `Apply()`, no fetch), mas todo gate a re-resolve comparando strings. O mod tem 6 classes + vanilla; um id numérico resolvido uma única vez no `Apply` transformaria cada um dos 42 gates num compare de `int`.
- **Impacto Técnico Real:** na ordem de dezenas de µs por segundo. Não é o que trava o jogo — é o item de maior **frequência** do mod e o de melhor razão ganho/risco entre os 🔵, porque a mudança é local a uma classe e mecânica.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem atual:* `string.Equals(_classNameEn, nameEn, OrdinalIgnoreCase)` a cada gate.
  - *Abordagem otimizada:* `enum EClass { None, CombatMedic, Rifleman, Hunter, Stealth, Scavenger, Tank }` resolvido 1× no `Apply()`/`Reset()`; `IsLocalClass(EClass)` vira `_classId == id`. Manter o overload de string (`IsClass(string?, string)`) para o caminho de **peer** (`ClassIdentities.ClassNameEnOf` devolve string do server), mas cacheando o `EClass` também na `Identity` no `Commit()` — assim os patches de som também comparam int.
  - Migração segura: manter a assinatura antiga como wrapper (`IsLocalClass(string) => IsLocalClass(Parse(nameEn))`) e converter os call-sites per-frame primeiro.
- **Como validar:** contador + `Stopwatch` amostrado (1 em 1024) em `ClassMoveSpeed.Apply`, mesmo mapa/rota antes e depois. Critério: custo médio por chamada cai; contagem de chamadas **não muda** (a mudança é de custo unitário, não de frequência). Não-regressão: cada perk continua ativando exatamente para a sua classe — checklist das 6 classes + vanilla, com o overlay 052 ligado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · Multiplicidade de patches no mesmo alvo — o gate é refeito N vezes

> ⚠️ **Revisão 01 (RV-04) — FORA DO ESCOPO DE PERFORMANCE.** O texto abaixo admite que o ganho de CPU é "pequeno", que o benefício real é **estrutural** (legibilidade da ordem de composição do recuo) e que esta é a mudança de **maior risco de regressão de balance** do relatório. Ganho não medido + benefício de manutenção + maior risco da rodada não formam um achado `--perf`. **Retirado da recomendação do §4**; mantido aqui por rastreabilidade. Encaminhar por `/code-review` se quiser tratá-lo.

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Execução:** per-event médio (dano, tiro) × N players + bots × raid inteira.
- **Localização no Mod:**
  - `Player.ApplyDamageInfo` — **4 patches**: [BulwarkPatch.cs:39](../modded/Client/Patches/BulwarkPatch.cs#L39) · [ExecutionMeleePatch (ClassCombatHealthPatches.cs:24)](../modded/Client/Patches/ClassCombatHealthPatches.cs#L24) · [AdrenalineTriggerPatch.cs:22](../modded/Client/Patches/AdrenalineTriggerPatch.cs#L22) · [LocalHitTypePatch (ClassWeaponPatches.cs:530)](../modded/Client/Patches/ClassWeaponPatches.cs#L530)
  - `ProceduralWeaponAnimation.Shoot` — **4 patches**: `RecoilFloorCapturePatch` · `WeaponMasteryRecoilPatch` · `ShootRecoilPatch` · `RecoilFloorApplyPatch`
  - `FirearmController.SetAnimatorAndProceduralValues` — **3 patches**: `ReloadSpeedPatch` · `HolsterDrawResetPatch` · `ShotgunReloadPatch`
  - `FirearmController.TotalErgonomics` — **2 patches**: `HeavyWeaponErgoPatch` · `WeaponMasteryErgoPatch`
- **Causa Raiz:** cada patch é uma unidade independente e, por isso, refaz do zero o mesmo trabalho de gate: `Singleton<GameWorld>.Instance?.MainPlayer` (2 leituras de campo), o `ReferenceEquals` de instância e os derefs de `ConfigEntry`. Em `ApplyDamageInfo` isso acontece **4×** por evento de dano de qualquer entidade do mapa. Nenhum patch está errado; a soma é que é redundante.
- **Impacto Técnico Real:** pequeno e proporcional à intensidade do combate. O valor real do achado é **estrutural**: hoje a ordem de execução em `PWA.Shoot` depende de três `[HarmonyPriority]` coordenados (`First` → `High` → `Normal` → `Last`) e de um campo estático compartilhado (`RecoilFloorCapturePatch.StrBefore`) — um arranjo correto, mas frágil a refactor.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem atual:* N patches independentes por alvo, cada um com gate próprio.
  - *Abordagem otimizada:* **1 patch por alvo** que resolve o gate uma vez e chama os branches em ordem explícita no corpo. Para `PWA.Shoot` o ganho extra é grande: elimina o par capture/apply e o estático `StrBefore` (o `str` original vira variável local) e torna a ordem legível em vez de emergente das prioridades.
  - Ordem de preferência: começar por `PWA.Shoot` (maior ganho estrutural) e `ApplyDamageInfo` (maior frequência). Os outros dois só se a rodada tiver folga — mexer no trio de `SetAnimatorAndProceduralValues` é o de maior risco de regressão (`__state` por patch, restauração de `BuffInfo.ReloadSpeed`).
  - ⚠️ **Risco a declarar na spec técnica:** consolidar patches muda a ordem de composição dos multiplicadores. O piso de recuo B15 e a ordem maestria→perks precisam ser preservados **literalmente** e re-validados com o overlay 052.
- **Como validar:** contadores por alvo (chamadas totais × chamadas que passam do gate) antes/depois, mesma raid/mapa. Critério: contagem de execuções de gate cai de 4→1 em `ApplyDamageInfo` e de 4→1 em `Shoot`; **valores finais idênticos**. Não-regressão: `Recoil str` no overlay 052 mostra o mesmo antes→depois para Tanque+LMG+maestria e Fuzileiro na janela de Adrenalina (os dois piores casos do Anexo C do balance board).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-04 · Reflection crua (não compilada) no funil de som `BaseSoundPlayer.PlayClip`

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Execução:** per-event médio (evento de animação de som **não-arma**: faca, granada, meds, quick-use) × N players + bots × raid inteira.
- **Localização no Mod:** [SilentKnifePatch.cs:84-85](../modded/Client/Patches/SilentKnifePatch.cs#L84-L85)
- **Referência Cruzada:** `BaseSoundPlayer.PlayClip` — `BaseSoundPlayer.cs:395`, alimentado por `PlayRandomClip` ← `SoundEventHandler`/`EventReceiver` (eventos de animação).
- **Causa Raiz:** `BridgeField` e `IPlayerProp` são resolvidos 1× (correto — `csharp-mod-best-practices` §3), mas a **invocação** continua reflexiva: `FieldInfo.GetValue(obj)` seguido de `PropertyInfo.GetValue(bridge)`, que passa por `MethodInfo.Invoke`. Isso é ~1 ordem de grandeza mais caro que um acesso direto. O gate (1) — `__instance is WeaponSoundPlayer` — já descarta a maioria das chamadas (todo som de arma de fogo) **antes** da reflection, que é o que mantém a severidade baixa.
  - **A inconsistência é o ponto:** o `SainSoundPatch`, no mesmo arquivo de domínio, já resolve isso do jeito certo — compila um `Expression.Lambda<Func<object,string>>` uma vez ([ClassSoundPatches.cs:352-355](../modded/Client/Patches/ClassSoundPatches.cs#L352-L355)) justamente "para tirar o reflection do hot-path (review)". O padrão existe no mod e não foi aplicado aqui.
- **Impacto Técnico Real:** algumas centenas de nanossegundos por som de faca/granada/meds de qualquer entidade. Invisível isoladamente; é higiene de hot path.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem atual:* `BridgeField.GetValue(...)` + `IPlayerProp.GetValue(...)` por chamada.
  - *Abordagem otimizada:* um único `Func<BaseSoundPlayer, object?>` compilado no static ctor, encadeando o campo e a property — mantendo o mesmo `try/catch` de resolução e o mesmo fail-open (`return true`) quando o delegate for null.

```csharp
// Delegate compilado 1× no static ctor: sound => ((IObserverToPlayerBridge)sound.playersBridge).iPlayer
private static readonly Func<BaseSoundPlayer, object?>? EmitterOf = BuildEmitterAccessor();

private static Func<BaseSoundPlayer, object?>? BuildEmitterAccessor()
{
    try
    {
        var field = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
        var prop = field != null ? AccessTools.Property(field.FieldType, "iPlayer") : null;
        if (field == null || prop == null) return null;

        var p = Expression.Parameter(typeof(BaseSoundPlayer), "sp");
        var body = Expression.Convert(Expression.Property(Expression.Field(p, field), prop), typeof(object));
        return Expression.Lambda<Func<BaseSoundPlayer, object?>>(body, p).Compile();
    }
    catch (Exception ex)
    {
        Plugin.Log?.LogWarning($"[CustomClasses] (083) accessor do emissor não compilado — Morte Silenciosa inerte: {ex.Message}");
        return null;
    }
}
```

- **Como validar:** `Stopwatch` amostrado (1 em 256) no Prefix, mesma raid, comparando o custo médio por chamada que passa do gate (1). Critério: custo médio cai; contagem de chamadas inalterada. Não-regressão: faca do Furtivo continua muda (sacar/golpear/acertar), som de arma de fogo intocado, granada/meds/quick-use continuam audíveis, peer Fika Furtivo continua mudo no seu cliente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-05 · Log de diagnóstico `[053-tabicon]`/`[053-tabtext]` em `LogInfo`, sem gate de config

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Execução:** one-shot por sessão (flag `_loggedTabImages`) × 1 — mas com custo de `LINQ` + `string.Join` + N linhas de log, e **sem** o gate de config que o resto do mod usa.
- **Localização no Mod:** [SkillsClassTabPatch.cs:435-445](../modded/Client/Patches/SkillsClassTabPatch.cs#L435-L445)
- **Causa Raiz:** essas linhas nasceram na "rodada 3" de fixes da aba CLASS (o comentário diz explicitamente *"se ainda falhar, colar o log"* — HANDOFF.md pendência #2). O fix foi fechado; o instrumento ficou. O irmão `DumpNativeTexts` (`:463`) faz a coisa certa: primeira linha é `if (!PerkDiag.Enabled … ) return;`.
- **Impacto Técnico Real:** nenhum em FPS. O custo é de **higiene**: polui o console do BepInEx de todo usuário na primeira abertura da tela de Skills, e a string é montada com `Select` + `string.Join` **antes** de qualquer verificação de nível — exatamente o padrão que a skill `spt-performance-analysis` §2/LOG manda evitar.
- **Alternativa de Melhor Lógica / Proposta de Correção:** gatear em `PerkDiag.Enabled` (como o `DumpNativeTexts`) ou rebaixar para `LogDebug`. Preferência: `PerkDiag.Enabled`, para ser consistente com o vizinho e manter o instrumento disponível se a aba CLASS voltar a dar problema.
- **Como validar:** abrir a tela de Skills com `Perk Diagnostics` **off** e conferir que `grep '053-tab' LogOutput.log` volta vazio; com **on**, que as linhas voltam. Não-regressão: a aba CLASS renderiza igual nos dois casos.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-06 · `GetType().Name.IndexOf("Hideout")` por disparo de underbarrel

> ⚠️ **Revisão 01 (RV-04) — FORA DO ESCOPO DE PERFORMANCE.** O próprio texto diz "desprezível em CPU" e se justifica por "desvio de padrão". Isso é conformidade, não performance. **Retirado da recomendação do §4**; mantido por rastreabilidade. Encaminhar por `/code-review`.

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Execução:** per-event (1× por disparo de GP-25/M203) × 1 entidade (gate de instância acima já barrou bots).
- **Localização no Mod:** [WeaponMasteryPatches.cs:57](../modded/Client/Patches/WeaponMasteryPatches.cs#L57)
- **Causa Raiz:** `p.GetType().Name` resolve o `Type` e devolve o nome; `IndexOf(…, OrdinalIgnoreCase)` faz busca de substring com dobra de caixa. A forma canônica do repo para essa mesma pergunta é o teste de tipo — `spt-mod-best-practices` §2 ("Robust guard: `if (gameWorld.MainPlayer is HideoutPlayer) return;`"), com a ressalva explícita de **não** usar checagem por string.
- **Impacto Técnico Real:** desprezível em CPU. Entra no relatório porque é **desvio de padrão em superfície por-evento**, e o padrão certo já é o usado no resto do mod. (O mesmo idioma aparece em `RaidPerksNotificationPatch.cs:46`, mas ali roda 1× por raid — não vale mexer.)
- **Alternativa de Melhor Lógica / Proposta de Correção:** `var inHideout = p is HideoutPlayer;` (mesmo namespace `EFT`, já referenciado).
- **Como validar:** disparar o GP-25 no shooting range do hideout (não deve creditar XP, mas o efeito de recuo por nível deve valer) e numa raid (deve creditar XP) — exatamente o checklist já escrito no `058-05-asbuild`. Não-regressão: comportamento idêntico nos dois contextos.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-07 · 💡 Melhorias preventivas (bloco agregado)

- **Severidade:** 💡 Otimização · **Evidência:** Melhoria preventiva (nenhuma explica problema atual)
- Entram juntas e curtas, conforme a regra do modo `--perf`. Nenhuma vira item de backlog sozinha.

| # | Ponto | Situação | Sugestão |
|---|---|---|---|
| a | [MedrosoPatch.cs:47](../modded/Client/Patches/MedrosoPatch.cs#L47) — `GClass897.OnShoot += OnBulletFlyBy` | A subscrição é feita no `Awake` e vale para **toda bala com estampido sônico do mapa**, mesmo quando o jogador não é Saqueador. O handler já sai na 3ª verificação (ordem barato→caro está correta), então o custo é ~2 derefs de config + 1 compare de string por bala. | Assinar/desassinar no raid-start conforme a classe local, ou fundir num único `bool` estático `_medrosoArmed` calculado no `OnGameStarted` (fecha em O(1) sem tocar na subscrição). Ganha mais depois do AUD-01-02. |
| b | [AdrenalineState.cs:67-77](../modded/Client/AdrenalineState.cs#L67-L77) — `WatchWindow` | `yield return null` a cada frame durante a janela inteira (25 s default) só para detectar o fechamento. ~1.500–3.600 retomadas de coroutine por janela, cada uma fazendo um compare de float. | `yield return new WaitForSeconds(SecondsLeft)` num loop (renovação estende `_windowEnd` → o loop re-espera o resto). Reduz de milhares para unidades de retomadas por janela, com o mesmo comportamento. |
| c | [SkillPanelPatch.cs:63-64](../modded/Client/Patches/SkillPanelPatch.cs#L63-L64) | `MultiplierFormat.TooltipText(...)` monta uma string nova a cada refresh de linha; a lista de Skills recicla `SkillPanel` durante o scroll → alocação por linha por frame de scroll. | Cachear o texto por `(ESkillId, fator)` num dicionário estático pequeno (o par muda só quando o fetch muda). |
| d | [PerkDiagnostics.cs:129](../modded/Client/PerkDiagnostics.cs#L129) → `AppendPerkList` | Com o overlay ligado, `PerksCatalog.LocalGroups()` (LINQ + `ToArray`) roda a cada Repaint. | Cachear os grupos por classe enquanto o overlay estiver aberto. Só afeta quem liga o diag — por isso é preventiva, não achado. |

- **Decisão (bloco):**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão (indicar quais: ____)
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-08 · Cache de textura tingida cresce sem teto, alimentado pelo picker de cor do F12

> ⚠️ **Achado acrescentado pela Revisão 01 (RV-01)** — a auditoria original leu este arquivo, mas o examinou pela lente de CPU-em-raid e não pela de retenção/VRAM.

- **Severidade:** 🟡 Médio — mesma régua do `AUD-01-01` (superfície de menu, acionada por ação específica do usuário). **Sobe para 🟠 Alto** se a medição confirmar emissão por quadro de arrasto: retenção sem teto é o que a régua do repo reserva para 🟠.
- **Evidência:** **Suspeita** — mecanismo provado por leitura; **magnitude depende da cadência de `SettingChanged` do ConfigurationManager**, que não dá para provar estaticamente.
- **Execução:** per-event × **2 consumidores** × **acúmulo sem teto** × vida = **a sessão inteira** (não a raid). Custo unitário **alto**.
- **Localização no Mod:** [ClassIconCache.cs:74-137](../modded/Client/UI/ClassIconCache.cs#L74-L137) (`GetTinted`) · [ClassIconCache.cs:140-154](../modded/Client/UI/ClassIconCache.cs#L140-L154) (`Dispose` — o **único** ponto de liberação)
- **Causa Raiz:** a chave do cache é `nome|corTopo|corBase`, e a cor vem de um `ConfigEntry<Color>` do F12 (item 067). Cada chave nova custa:
  - `new Texture2D(256, 256, RGBA32)` → **256 KB de VRAM** (ícones confirmados 256×256 no disco);
  - `tex.GetPixels32()` → `Color32[65536]` = **256 KB gerenciados por chamada** — acima do limiar de 85 KB, portanto **Large Object Heap**, que não é compactado e só é recolhido em coleta de geração 2;
  - 65.536 operações de pixel + `SetPixels32` + `Apply` (upload à GPU) + `Sprite.Create`.

  **Nada disso é liberado:** `DestroySprite` só roda em `Dispose()`, chamado apenas no `Plugin.OnDestroy` (fechar o jogo). Não há substituição, teto nem invalidação.

  A cadeia que torna o crescimento ilimitado:

```
F12: arrasta o picker de cor de uma classe
  → ConfigEntry<Color>.SettingChanged                (PerksConfig.cs:682)
  → PerksConfig.ClassColorsChanged                   (PerksConfig.cs:54)
  ├→ MenuClassIdentityPatch.RefreshColors            (Plugin.cs:88)
  │    → StartCoroutine(ApplyToMenu) → ApplyClassIcon
  │      → ClassIconCache.GetTinted(cor NOVA)        (ClassIdentityView.cs:134)  ← textura nova
  └→ SkillsClassTabPatch.OnColorsChanged             (SkillsClassTabPatch.cs:30)
       → rebuild da aba CLASS → PerksPanelView
         → ClassIconCache.GetTinted(cor NOVA)        (PerksPanelView.cs:242)     ← outra textura nova
```

- **Impacto Técnico Real:** dois regimes.
  - **Uso normal** (não mexe no picker): 1–2 cores por classe na sessão → um punhado de texturas. Inofensivo.
  - **Arrastando o picker:** uma textura por evento. Se o ConfigurationManager emitir por quadro de arrasto — comportamento típico de slider —, alguns segundos produzem **dezenas de MB de VRAM presos até fechar o jogo**, o mesmo volume de lixo no LOH, e travamento visível durante o arrasto. Soma-se ao `AUD-01-01`, que **compartilha exatamente o mesmo gatilho** (cada evento também reinicia a busca de 60 quadros no menu).
- **O que refutaria este achado:** se o ConfigurationManager emitir `SettingChanged` só ao **soltar** o controle (um evento por cor escolhida, não por quadro), o crescimento fica na casa de unidades por sessão e o achado morre como preventiva. **É a primeira coisa a medir** (INSTR-3) — responde em 10 segundos.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem atual:* uma entrada permanente por cor distinta, sem teto.
  - *Abordagem otimizada (recomendada = a + b):*
    - **(a)** manter no máximo **uma variante tingida viva por ícone**: ao inserir uma chave nova do mesmo `iconFile`, destruir a anterior (`DestroySprite`). Ninguém precisa do histórico de cores.
    - **(b)** **quantizar a cor na chave** (arredondar cada canal para múltiplos de 8) — corta a cardinalidade em ~32× sem diferença visível.
    - *(c) alternativa descartada:* voltar ao `ClassIconGradient` (o `BaseMeshEffect` que já existe e **não aloca por cor**). É o melhor custo/benefício em tese, mas foi justamente o caminho abandonado no 06-fix-02 por falhar em `Image` criada em runtime — reintroduzi-lo reabriria um bug já fechado.
- **Como validar:** logar `TintedCache.Count` e a VRAM estimada (`Count × 256 KB`) ao abrir e ao fechar o F12 (INSTR-3). Cenário pareado: arrastar o picker de uma classe por ~5 s, antes e depois. Critério: `Count` deixa de crescer com o arrasto (fica ≤ nº de ícones). Não-regressão: o ícone da classe mantém o gradiente correto no menu, no chat, na tela de deploy e na aba CLASS, e trocar a cor no F12 continua refletindo ao vivo.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Panorama de execução

Toda superfície periódica ou frequente do mod. Patches Harmony entram com a **frequência estimada do alvo** (método da skill `spt-performance-analysis` §3 — decompile lido, cadeia de callers provada).

### Client — em raid

| Superfície | Alvo / gatilho | Classe de frequência | Multiplicador de entidades | Gate de contexto (custo) | Quem para / quando |
|---|---|---|---|---|---|
| `Plugin.OnGUI` → `PerkDiagnostics.Draw` | `OnGUI` do Unity | **per-frame** (≥2 eventos/frame) | ×1 | `DiagnosticsEnabled` (**default off**) na 1ª linha; depois filtro `EventType.Repaint` | nunca (vive com o plugin) — com diag off, o custo é 1 branch |
| `MaxSpeedPatch` | Postfix `MovementContext.MaxSpeed` (`:910`); lido 3× por `UpdateCharacterControllerSpeedLimit` (`:4181`, `:2375`, `:2377`, `:2368`) | **per-frame** | **×N players + bots** (`BotMover.cs:930/:985` → `Player.ChangeSpeed`) | `ReferenceEquals(ctx, MainPlayer.MovementContext)` — 2 leituras de campo (`GameWorld.MainPlayer` é **campo**, `:572`) | patch global; corpo sai no gate |
| `SprintingSpeedPatch` | Postfix `MovementContext.SprintingSpeed` (`:912`) → `SprintAcceleration` (`:2547`) | **per-frame** (em sprint) | ×N players + bots | idem | idem |
| `AiSoundPatch` | Prefix `BotEventHandler.PlaySound` ← `MovementContext.cs:1756/:1769` (passo, throttle `STEP_NOISE_DELTA`) e `:1629/:3096` (porta) | per-event frequente (~1/0,2–0,3 s por entidade em movimento) | **×N players + bots** | `type != step` → `person is not Player` → `ClassNameEnOf` (`IsAI` na 1ª linha) | patch global |
| `SoundRadiusPatch` | Postfix `Player.method_67` ← `PlayStepSound`/`method_66` (`Player.cs:28105-28223`) + som de gear (`:28163`) | per-event frequente | ×N players + bots | `ClassNameEnOf` (`IsAI`) | patch global |
| `SainSoundPatch` | Prefix `PlayerComponent.PlayAISound` (SAIN) | per-event frequente | ×N players | `GetAlivePlayerByProfileID` + `ClassNameEnOf`; getter de `ProfileId` **compilado** | só registrado se o SAIN estiver carregado |
| `SilentKnifePatch` | Prefix `BaseSoundPlayer.PlayClip` (`:395`) | per-event médio (só sons **não-arma**) | ×N players + bots | config → `is WeaponSoundPlayer` → **2 reflections cruas** (AUD-01-04) | patch global |
| `Medroso.OnBulletFlyBy` | evento estático `GClass897.OnShoot` | per-event frequente (rajadas) | ×tiros do mapa | `dist>0` → config → `IsLocalClass` (string) | subscrição 1× no `Awake`, idempotente, nunca removida |
| `ApplyDamageInfo` (**4 patches**) | Prefix/Postfix `Player.ApplyDamageInfo` | per-event médio | ×N players + bots | cada patch refaz seu gate (AUD-01-03) | patch global |
| `PWA.Shoot` (**4 patches**) | `ProceduralWeaponAnimation.Shoot` | per-event (rajada) | ×N (gate deixa só o local) | `ReferenceEquals` com o PWA do MainPlayer em cada | patch global |
| `OnTriggerPatch` | Prefix `AbstractSkillClass.OnTrigger` | per-event frequente | ×1 (perfil local) | `Plugin.Enabled` + `val <= 0`; corpo é 1 lookup de dicionário | patch global |
| `MedrosoDamagePatch` | Postfix `Player.ReceiveDamage` | per-event médio | ×N players + bots | `IsYourPlayer` | patch global |
| `StancesArmStaminaBridge.Factor` | delegate chamado pelo Tick do stances (só no dreno) | **per-frame de dreno** | ×1 | 2 `IsLocalClass` (string) + `HeavyWeapon.InHand` | delegate acoplado 1×; função pura, sem estado |
| `AdrenalineState.WatchWindow` | coroutine, `yield return null` | **per-frame durante a janela** (25 s) | ×1 | só arranca com janela ativa; 1 watcher por janela | `Reset()` no raid-start mata órfã; loop sai quando `IsActive` cai |
| `MalfunctionChancePatch` · `MedUseTimePatch` · `SurgeryPenaltyPatch` · `ChangeEnergy/Hydration` | funis de jam / meds / cirurgia / metabolismo | per-event raro (o de cirurgia também é chamado por regen a cada 3 s — `ActiveHealthController.cs:2244-2256`) | ×N players + bots | config/instância/`Armed`; corpos O(1) | patch global |
| `TotalErgonomics` (**2 patches**) · `SetAnimatorAndProceduralValues` (**3**) · `UpdateWeaponVariables` · `UpdateSwayFactors` · `SetAimingSlowdown` · `GClass2175.method_1` · `HoldBreath` · `Spawn`/`Drop` | funis de ergo/animação/mira | per-event raro (**verificado: nenhum é per-frame**) | ×N controllers | `ReferenceEquals` de instância em todos | patch global |
| `PackMulePatch` · `QuickHandsPatch` | getters de `SkillManager` (`CarryingWeightRelativeModifier`, `IsSearchDouble`) | per-event raro (`UpdateWeightLimits` é event-driven — `BasePhysicalClass.cs:929`) | ×1 | classe + `ReferenceEquals` com `MainPlayer.Skills` | patch global |

### Client — menu / loading

| Superfície | Gatilho | Frequência | Gate | Observação |
|---|---|---|---|---|
| `MenuClassIdentityPatch.ApplyToMenu` | coroutine por `MenuScreen.Show` e por `RefreshColors` (evento do picker F12) | **per-frame por até 60 frames** + 90 frames ociosos | **nenhum** short-circuit de Menu-Overhaul ausente | 🟡 **AUD-01-01** |
| `ChatSpecialIconPatch` · `PartyPlayerItemPatch` · `SkillPanelPatch` · `SkillIconBorderPatch` | por linha/ícone renderizado | per-event (por linha, com reciclagem de célula) | `Plugin.ShowOnUi`/`ShowClassOnPlayerName` + null-guards | sprites vêm de `ClassIconCache` (cache de `Sprite` + `Sprite` tingido, com `Dispose` no `OnDestroy`) |
| `FadeIn.Update` | fade da aba CLASS | per-frame por 0,22 s | `enabled = false` ao terminar | auto-desliga (CR-03-03) |
| `PartyInfoPanelPrefetchPatch` | Prefix `PartyInfoPanel.Show` | 1× por tela de deploy | guard nenhum | 2 GETs síncronos (documentado, CR-057F3-04) |
| `RaidPerksNotificationPatch` | Postfix `GameWorld.OnGameStarted` | 1× por raid | guard de hideout **antes** dos prefetches | 2 GETs síncronos na tela de loading; no **headless** não há tela de loading — são 2 GETs bloqueantes no raid-start (pequenos) |

### Server

| Superfície | Gatilho | Frequência | Custo |
|---|---|---|---|
| `SkillMultipliersRouter` | GET `/customclasses/skill-multipliers` | 1× por raid-start + 1× por tela de deploy | `SaveServer.GetProfile(sessionId)` + serialize — O(1) |
| `ClassIdentitiesRouter` | GET `/customclasses/class-identities` | idem | LINQ + `OrderBy` sobre `SaveServer.GetProfiles()`; **`GetProfiles()` é cópia RASA do dicionário** (`spt-source SaveServer.cs:147-150`), N = número de perfis (unidades) → desprezível |
| `CatalogService` | editor web | fora do escopo | ✅ **a lição do 022 já está aplicada**: `_localeCache` (`ConcurrentDictionary.GetOrAdd`) envolve `localeService.GetLocaleDb` em [CatalogService.cs:219-231](../modded/Server/CatalogService.cs#L219-L231), com o motivo documentado no comentário. **Não há outro re-materializador remanescente** nos caminhos auditados |

---

## Configuração

Auditoria das chaves que alimentam **frequência, raio, quantidade, logging e limpeza** (skill §2/CFG). Total: **97 `ConfigEntry`**, todas ligadas no `Awake` (`PerksConfig.Bind`) e lidas por `.Value` no apply-time — nenhuma re-parseia arquivo em runtime.

| Chave (F12) | Default atual | Default proposto | Onde entra no código |
|---|---|---|---|
| `0 · General → Perk Diagnostics overlay` | `false` (marcada `advanced`) | **manter `false`** | Portão de todo o overlay ([PerkDiagnostics.cs:71](../modded/Client/PerkDiagnostics.cs#L71)), das escritas de `PerkDiag.*` nos patches de som/recuo/jam e do `LogPeer` (throttle 3 s por canal+nickname). É a única chave com alavanca real de custo — e já está no valor certo. |
| `6 · Scavenger → Nervous — Suppression distance (m)` | `4` | manter `4` | Única chave que liga trabalho **por bala do mapa** ([MedrosoPatch.cs:174](../modded/Client/Patches/MedrosoPatch.cs#L174)). `0` desliga a geometria de near-miss (fica só o gatilho de dano) — é a saída para quem quiser custo zero nesse canal. |
| `5 · Stealth → Silent Knife — Enabled` | `true` | manter `true` | Gate 1 do `SilentKnifePatch`; com `false`, zero reflection por clip (relacionado a AUD-01-04). |
| `0 · General → Raid-start perks notification` | `true` | manter | 1 coroutine + 1 string por raid. |
| Demais 93 chaves | — | — | São **valores de gameplay** (multiplicadores, tempos, toggles de perk), não parâmetros de custo: nenhuma controla intervalo de timer, raio de varredura, cap de entidades ou verbosidade de log. |

**Conclusão da auditoria de configuração:** este mod **não tem alavanca de CFG**. Não existe timer periódico, poller, raio de scan ou logging verboso configurável — o custo é todo determinado pela arquitetura de patches, não pelos defaults. Também **não há timers sincronizados** a dessincronizar (o mod não tem nenhum).

---

## Instrumentação proposta

Nenhum achado ficou em nível "Suspeita", então a instrumentação **não é pré-requisito** para decidir. Ela existe para dois fins: fechar a **magnitude** de AUD-01-01 no setup real do usuário e estabelecer o **baseline pareado** que a Fase 4 vai precisar. Toda ela é gated por `PerksConfig.DiagnosticsEnabled` (já existe, default `false`), sem alocação no caminho quente, e marcada para remoção.

### INSTR-1 — iterações e tempo da coroutine do menu (fecha AUD-01-01)

```csharp
// PERF-INSTR AUD-01-01 — temporary, remove after validation
private static readonly System.Diagnostics.Stopwatch _menuSw = new();
// no topo de ApplyToMenu:
if (PerkDiag.Enabled) { _menuSw.Restart(); }
int finds = 0;
// dentro do for, junto do GameObject.Find:  finds++;
// depois do loop:
if (PerkDiag.Enabled)
{
    Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-01] menu apply: finds={finds} mo={MenuOverhaulBridge.IsPresent} ms={_menuSw.Elapsed.TotalMilliseconds:F1}");
}
```

Responde: quantas varreduras globais o setup do usuário paga de fato por abertura de menu, e quanto tempo a coroutine fica viva. **1 linha por `Show`** — sem risco de flood.

### INSTR-2 — censo das superfícies mais quentes (baseline para AUD-01-02/03)

```csharp
// PERF-INSTR AUD-01-02/03 — temporary, remove after validation
internal static class PerfCount
{
    internal static long MoveSpeedCalls, MoveSpeedPassed;   // ClassMoveSpeed.Apply
    internal static long StepAiCalls, StepAiPassed;         // AiSoundPatch
    internal static long RolloffCalls, RolloffPassed;       // SoundRadiusPatch
    internal static long DamageCalls;                       // LocalHitTypePatch (1 dos 4 = proxy do alvo)
    internal static readonly System.Diagnostics.Stopwatch Sw = new();
    internal static long MoveSpeedTicks;                    // amostrado 1 em 1024
}
```

- Incremento: `long++` sem alocação, dentro do `if (PerkDiag.Enabled)` **antes** de qualquer formatação de string.
- `Stopwatch` amostrado só em `ClassMoveSpeed.Apply`: `if ((_n++ & 0x3FF) == 0) { … }`.
- ~~**Dump agregado 1× no raid-end** (`GameWorld.OnDestroy` ou o mesmo hook do `AdrenalineState.Reset`), nunca por chamada:~~

> ⚠️ **Revisão 01 (RV-06) — o ponto de despejo original não existe.** (1) `AdrenalineState.Reset` roda no raid-**START**, não no end: despejar ali reportaria os contadores da raid **anterior**, silenciosamente deslocados em uma raid. (2) **O mod não tem hook de raid-end** (nenhum patch em `GameWorld.OnDestroy` / `BaseLocalGame.Stop`), então a proposta exigia adicionar um patch novo — mais invasivo do que "instrumentação temporária" sugere.
>
> **Corrigido:** despejo **periódico**, sem hook novo — uma corrotina no `Plugin` que, a cada 60 s, emite uma linha **enquanto `Singleton<GameWorld>.Instantiated && PerkDiag.Enabled`, e zera os contadores**. Serve melhor ao propósito: mostra a evolução ao longo da raid (responde "o custo cresce?") em vez de um total no fim.

```
[CustomClasses][perf] t=<s> moveSpeed=<calls>/<passed> (avg <µs>) · stepAI=<calls>/<passed> · rolloff=<calls>/<passed> · damage=<calls>
```

Responde as duas perguntas que a estática não fecha: **qual o N real** de bots × frames que essas superfícies pagam numa raid do usuário, e **qual fração** passa do gate (deveria ser ~1/N).

### INSTR-3 — crescimento do cache de textura (fecha AUD-01-08) · *acrescentada pela Revisão 01*

```csharp
// PERF-INSTR AUD-01-08 — temporary, remove after validation
// em ClassIconCache.GetTinted, logo após TintedCache[key] = sprite;
if (PerkDiag.Enabled)
{
    Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-08] tintedCache={TintedCache.Count} (~{TintedCache.Count * 256} KB VRAM) key={key}");
}
```

Responde a única pergunta que decide o achado: **quantas entradas um arrasto do picker de cor gera?** Se for 1–2 por cor escolhida, `AUD-01-08` vira preventiva; se for dezenas por arrasto, sobe para 🟠 e justifica a rodada sozinho. Uma linha por **inserção** (não por consulta) — o cache é justamente o que impede o flood.

### Regras aplicadas

- Gate por `ConfigEntry<bool>` checado **antes** de qualquer `$"..."`.
- Zero alocação no caminho quente mesmo com o diag ligado.
- Todo bloco leva `// PERF-INSTR AUD-NN-MM — temporary, remove after validation`.
- Roda também no **headless Fika** — o dump agregado no raid-end é seguro lá.
- A build de instrumentação é **client-only**, bump de versão **patch** via `/compile-mod`, e agrupa INSTR-1 + INSTR-2 numa build só.

---

## Plano de validação

Cenário pareado obrigatório para todos: **mesmo mapa, mesmo ponto de spawn, contagem de bots semelhante, mesma duração**, com `Perk Diagnostics` ligado nas duas medições. Sem o par, a comparação não vale (`spt-performance-analysis` §7).

| Achado | Métrica | Cenário | Critério de sucesso | Não-regressão a conferir in-game |
|---|---|---|---|---|
| `AUD-01-01` | `finds` + ms da coroutine (INSTR-1) | Abrir o menu principal 5× (voltando de raid), com e sem o Menu-Overhaul carregado | `finds` cai de ≤60 para ≤20 (com MO) e para **0** (sem MO); tempo da coroutine cai proporcionalmente | Ícone + linha da classe + `AccentColor` + glow do topo (PvE) corretos no menu com MO instalado; sem MO, menu vanilla intacto |
| `AUD-01-02` | `MoveSpeedCalls` + custo médio amostrado (INSTR-2) | Raid de ~10 min, mapa com bots, andando e correndo | Custo médio por chamada cai; **contagem inalterada** | Cada perk ativa só na sua classe — percorrer as 6 classes + vanilla com o overlay 052: Heavy Frame, Execution, Lebre, Rooted, Steady/Tireless Arms |
| `AUD-01-03` | Contagem de execuções de gate por alvo (INSTR-2) | Mesma raid, com combate | 4→1 em `ApplyDamageInfo`; 4→1 em `PWA.Shoot`; **valores finais idênticos** | `Recoil str` no overlay 052 idêntico antes→depois em: Tanque+LMG+maestria 51 e Fuzileiro na janela de Adrenalina (piores casos do Anexo C do balance board); piso B15 continua mordendo; Bulwark/Execution/Adrenalina/Rattled disparando |
| `AUD-01-04` | `Stopwatch` amostrado no Prefix | Raid com uso de faca/granada/meds | Custo médio por chamada que passa do gate (1) cai; contagem inalterada | Faca do Furtivo muda (sacar+golpear+acertar); som de arma de fogo intocado; granada/meds/quick-use audíveis; peer Fika Furtivo mudo no seu cliente |
| `AUD-01-05` | `grep '053-tab' LogOutput.log` | Abrir a tela de Skills | Vazio com diag off; presente com diag on | Aba CLASS renderiza igual nos dois casos |
| `AUD-01-06` | — (mudança de forma, não de custo) | GP-25 no shooting range × GP-25 em raid | — | Range: **sem** XP, **com** efeito de recuo por nível. Raid: **com** XP. (checklist do `058-05-asbuild`) |
| `AUD-01-07` | Conforme o item aceito | — | — | (a) tremor do Saqueador dispara igual sob fogo · (b) janela da Adrenalina abre/renova/fecha nos mesmos tempos e o reload re-sincroniza · (c) marcadores ±X% corretos ao rolar a lista de Skills · (d) overlay 052 mostra a mesma lista de perks |

### Matriz de lifecycle (obrigatória na Fase 4)

| Cenário | O que conferir |
|---|---|
| Morte / despawn de bots | Contadores de `stepAI`/`rolloff` acompanham a queda do número de bots |
| Múltiplas ondas | 2ª onda custa como a 1ª. ~~(nenhuma coleção do mod cresce — não há GROW neste mod)~~ ⚠️ **Revisão 01 (RV-02): a afirmação entre parênteses é FALSA.** `ClassIconCache.TintedCache` cresce sem teto — ver `AUD-01-08`. As coleções que a auditoria de fato conferiu (`ClassIdentities.ByNickname` substituída no `Commit`, `PerkDiag.LastLog` limpa por raid, `SeenNetIds` com `Clear`, `PerksConfig.ClassColors` fixa) estão corretas; o cache de ícones nunca entrou na lista. **Nenhuma delas cresce por onda de bots**, então o critério desta linha continua válido — o que caiu foi a generalização |
| Raid longa (>20 min) | Custo estável; `LastLog` do `PerkDiag` (dicionário de throttle de peer) não cresce além do roster |
| raid1 → extract → raid2 | Nova raid não herda custo: `AdrenalineState.Reset`, `Medroso.ResetRaid`, `HolsterDrawSpeedPatch.BoostedDraw`, `PerkDiag.ResetPeerLog` já rodam no `OnGameStarted` — reconferir com os contadores zerados.<br>⚠️ **Revisão 01 (RV-06):** **o mod não tem hook de raid-END nenhum** — não há patch em `GameWorld.OnDestroy` nem em `BaseLocalGame.Stop`. Todo o reset acontece no **start** da raid seguinte. É design legítimo (nada do mod roda entre raids), mas é um fato de lifecycle que condiciona qualquer instrumentação futura |
| alt-F4 / morte / MIA | Idem (o reset é no **start** da raid seguinte, então cobre todos os caminhos de saída) |
| Headless Fika | 2 GETs bloqueantes no raid-start; dump agregado sai no raid-end; nenhuma superfície de menu roda lá |

---

## 4. Plano de Ação e Recomendações

> ⚠️ **Revisão 01 (RV-07) — §4 reescrito.** A versão original abria dizendo que não havia o que priorizar e emendava propondo um item de backlog de 4 achados; um leitor apressado lê a segunda parte. Faltava também o **custo da própria rodada**. Texto original preservado no bloco riscado ao fim desta seção.

**Recomendação default: NÃO abrir rodada de otimização agora.** Rodar só a mini-rodada de instrumentação e decidir com números.

1. **Não há 🔴 nem 🟠.** É o resultado legítimo de uma auditoria preventiva sobre código que já passou por várias rodadas de code-review com consciência de hot path (gates da regra 075 íntegros em 100% dos patches; lição do `GetLocaleDb` do item 022 aplicada no server).
2. **Os dois 🟡 dependem de uma medição que ainda não existe.** `AUD-01-01` e `AUD-01-08` são **Suspeita**: o mecanismo está provado, a magnitude não. Ambos são acionados pelo **mesmo gatilho** (o picker de cor do F12 / a abertura do menu) e vivem em arquivos vizinhos — se um justificar a correção, o outro entra junto de graça.
3. **Passo seguinte recomendado — mini-rodada de instrumentação** (prevista no passo 2 da Fase 1 do command; não exige item de backlog): INSTR-1 + INSTR-3 (+ INSTR-2 se quiser o baseline de raid). **Uma build, client-only, bump de versão *patch*.**
4. **Custo do ciclo, para a decisão ser informada:** cada correção neste mod exige compilar → bumpar SemVer → reinstalar → **reiniciar o EFT** (plugin BepInEx só recarrega no boot) → validar in-game com gate humano. Para um mod com 0 🔴 e 0 🟠, esse custo plausivelmente **supera** o ganho. A comparação é do usuário.
5. **Se, depois de medir, a rodada se justificar**, o agrupamento é: **AUD-01-08 + AUD-01-01** (mesmo gatilho) + **AUD-01-02** + **AUD-01-05**. Todos de baixo risco e localizados.
6. **Fora da rodada de performance:** `AUD-01-03` e `AUD-01-06` (RV-04 — não são achados de performance; encaminhar por `/code-review`); `AUD-01-04` e `AUD-01-07` (dívida anotada).
7. **Frente separada:** `/analyze-memory-leak CustomClasses` — a lente de retenção/VRAM que este `--perf` só tangenciou (RV-03) e que produziu `AUD-01-08` quase por acidente.
8. **Não regredir** o que já está certo: gates de identidade de instância (regra 075 / auditoria 0 vazamentos para bots), a ordem `First → High → Normal → Last` do recuo, o `Prefetch()` não-destrutivo e o `_localeCache` do `CatalogService`.

<details><summary>Texto original do §4 (preservado — relatórios são imutáveis)</summary>

> ~~1. **Não há 🔴 nem 🟠 para priorizar.** …~~
> ~~2. **Se houver rodada de otimização**, o agrupamento natural para um único item de backlog é: **AUD-01-01** (o único com ganho perceptível) + **AUD-01-02** + **AUD-01-05** + **AUD-01-06** — todos de baixo risco e localizados. **AUD-01-03** merece decisão separada…~~
> ~~3. **AUD-01-04 e AUD-01-07** são dívida anotada — só entram se a rodada tiver folga.~~
> ~~4. **Não regredir** o que já está certo…~~

</details>

### Observação fora do escopo de performance (registrada, não é achado `AUD`)

`PartyInfoPanelPrefetchPatch` (a classe mora no mesmo arquivo do `PartyPlayerItemPatch` — [PartyPlayerItemPatch.cs:137-139](../modded/Client/Patches/PartyPlayerItemPatch.cs#L137-L139)) ainda usa `Reset()` + `EnsureLoaded()` — exatamente a forma **destrutiva** que o code-review B20/F1 substituiu por `Prefetch()` em todos os outros call-sites, e cujo motivo está documentado em [SkillMultipliers.cs:92-95](../modded/Client/SkillMultipliers.cs#L92-L95): *"um GET falho no raid-start deixaria `_classNameEn=null` + `Factors` vazio marcados como carregados"*. Aqui o risco é menor (é a tela de deploy, não a raid), mas é a mesma armadilha. **Sem impacto de performance** — por isso fica como nota, não como achado. Vale um follow-up de correção junto de qualquer mexida nessa área.

---

**Memória consultada:** [`mods/CustomClasses/memory/sessions.md`](../memory/sessions.md) (snapshot + pendências P-7.x a P-16.2) e [`mods/CustomClasses/HANDOFF.md`](../HANDOFF.md) (2026-07-03 — **desatualizado**: descreve os itens 051–062 como pendentes, mas o backlog já vai até 088; usado só para contexto histórico das pendências #1/#2, que motivaram o AUD-01-05). Nenhuma pendência 🔴 aberta é de performance — as duas (`P-10.1`, `P-16.1`) são de **validação funcional in-game** dos perks. Não existe `relatorio-auditoria-codigo-*.md` nem `MEMORY-LEAK-review-*.md` anterior para este mod: este é o `NN = 01`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-22 | Claude | Criação — auditoria preventiva `--perf` do CustomClasses v0.16.8 (commit `8732d47c`): 7 achados (0 🔴 · 0 🟠 · 1 🟡 · 5 🔵 · 1 💡), panorama de execução, auditoria de configuração, instrumentação e plano de validação. |
| 2026-08-22 | Claude | Anotações da **Revisão 01** ([relatorio-auditoria-codigo-01-review-01.md](relatorio-auditoria-codigo-01-review-01.md)): `AUD-01-08` acrescentado (cache de textura sem teto — achado não detectado, RV-01); afirmação "não há GROW neste mod" corrigida (RV-02); ressalva de cobertura de retenção/VRAM no §1 (RV-03/RV-08); `AUD-01-03` e `AUD-01-06` marcados fora do escopo de performance (RV-04); evidência recontada para Forte 4 · Suspeita 2 (RV-05); `INSTR-2` corrigido (não existe hook de raid-end) e `INSTR-3` acrescentada (RV-06); §4 reescrito com "não abrir rodada" como default e o custo do ciclo explicitado (RV-07); links do decompile trocados por citação em texto (RV-09); linhas "O que refutaria" acrescentadas (RV-10). Nenhum texto original removido. |

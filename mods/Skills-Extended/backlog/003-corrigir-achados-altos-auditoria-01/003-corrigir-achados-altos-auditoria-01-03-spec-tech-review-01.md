# 003 — Corrigir achados altos da auditoria 01 · Review Técnica 01

**Mod:** Skills-Extended
**Spec técnica revisada:** [003-corrigir-achados-altos-auditoria-01-02-spec-tech.md](003-corrigir-achados-altos-auditoria-01-02-spec-tech.md)
**Data:** 2026-09-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** [mods/Skills-Extended/memory/sessions.md](../../memory/sessions.md) — Sessão 1 (snapshot 2026-09-07). Pendências que afetam este item: `P-1.1` (este próprio item, sem bloqueio adicional).

**Docs técnicos lidos (gatilho disparado):** [spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md) (obrigatório) — nenhum outro doc disparado, mesma conclusão da spec técnica.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 Importante | Buff de arma pode não se aplicar automaticamente no início da raid | `[x]` Aceitar sugestão |
| PA-01-02 | A — Gap | 🟡 Importante | Fix de `AUD-01-12` não cobre o corner case obrigatório da spec funcional (janela de transição de cena) | `[x]` Aceitar sugestão |
| PA-01-03 | C — Erro de Lógica | 🟢 Menor | Stub de `OnGameEndedPatch` (§5.7) referencia membros que não existem — não compila como está escrito | `[x]` Aceitar sugestão |
| PA-01-04 | B — Edge Case | 🟢 Menor | Troca de `HealthEffectsComponent.IHealthEffect` por wrapper não verificada contra comparação por referência em outro lugar do Assembly | `[x]` Aceitar sugestão |

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

### PA-01-01 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-07

**Buff de Ergonomia/Recuo das armas rastreadas pode não se aplicar automaticamente no início da raid**

**Problema:** A spec técnica (§1.3, `AUD-01-09`) faz o `UpdateWeaponsPatch.Prefix` só disparar quando `GameUtils.IsInRaid()` for `true` — correto para o objetivo do achado (não reprocessar fora de raid). Mas isso muda um comportamento colateral que existia **antes** desta correção: hoje, navegar telas no **hideout antes de entrar em raid** já dispara a coroutine (já que o Prefix rodava sem gate nenhum), que muta `weapon.Template.Ergonomics`/`RecoilForce*` — como essa mutação é **compartilhada por `TemplateId`**, o efeito já estava "pré-cozido" no template no exato momento em que a raid começava, independente de qualquer interação de tela dentro da raid.

Com a mudança proposta (gate por raid **+** Ergonomia migrada para `ErgonomicsTotalPatch`, que só aplica o bônus a instâncias já registradas em `UsecWeaponInstanceIds`), o bônus de Ergonomia só passa a existir depois que a coroutine rodar **pelo menos uma vez dentro da raid**. `MenuTaskBar.OnScreenChanged` é disparado por mudança de tela do `CurrentScreenSingletonClass` ([EFT.UI/MenuTaskBar.cs:473,484,494](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/MenuTaskBar.cs#L473)) — a spec técnica não verificou (nem eu, nesta review, dentro do orçamento disponível) se a transição menu→raid **em si** dispara esse evento de forma confiável antes do jogador atirar o primeiro tiro, ou se depende do jogador abrir alguma tela (inventário, etc.) já dentro da raid.

**Por que importa:** Se a transição de entrada em raid não disparar `OnScreenChanged` de forma confiável, um jogador com NATO/Eastern Weapons habilitado passaria os primeiros segundos/minutos da raid **sem** o bônus de ergonomia da sua arma (usando a arma "crua"), até a primeira vez que abrir uma tela in-raid — uma regressão de comportamento sutil que nenhum critério de aceite da spec funcional cobre explicitamente (ela testa "10 telas de hideout fora de raid" e "reprocessa quando o nível muda dentro de raid", mas não "o bônus já está ativo desde o primeiro frame da raid").

**Sugestão:** Validar em teste manual (entrar em raid sem abrir nenhuma tela, verificar se `Weapon.ErgonomicsTotal` já reflete o bônus assim que a raid carrega). Se não refletir, adicionar uma chamada direta a `UpdateUsecWeapons()`/`UpdateEasternWeapons()` a partir do `OnGameStartedPatch.Postfix` já existente ([`OnGameStarted.cs:40-72`](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs#L40-L72)) — mesmo hook já usado para assinar os eventos de skill — garantindo que o cache seja populado uma vez no início da raid, independente de qualquer interação de tela.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicado diretamente na spec técnica em vez de deixar como validação pendente — `UpdateWeaponsPatch` ganha `internal static void TriggerRaidStart()` (mesmo corpo do Prefix, sem o gate de raid, já que o caller garante o contexto) e `OnGameStartedPatch.Postfix` passa a chamá-lo. Ver [003-...-02-spec-tech.md](003-corrigir-achados-altos-auditoria-01-02-spec-tech.md) §1.3/§5.3/§5.7 atualizados.

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-07

**O fix de `AUD-01-12` não adiciona nenhuma guarda de nulidade para `Player`, deixando descoberto o corner case que a própria spec funcional exige cobrir**

**Problema:** A spec funcional deste item (`003-...-01-spec.md`, seção Corner cases, adicionada durante o `/review-spec`) exige explicitamente: *"O manipulador de evento de experiência médica (achado do 'campo já validado ignorado') dispara numa janela de transição de cena (fim de partida, troca de perfil) — a correção não deve introduzir uma falha não tratada nesse instante."*

A spec técnica (§1.6, §5.7) resolve `AUD-01-12` trocando `GameUtils.GetPlayer()!.Skills.FirstAid.IsEliteLevel` por `Player!.Skills.FirstAid.IsEliteLevel` — mas o `Player!` (null-forgiving) já era usado **incondicionalmente na primeira linha do método**, antes de qualquer branch: `var skillMgrExt = Player!.Skills.SkillManagerExtended;` ([OnGameStarted.cs:76](../../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs#L76), preservado sem mudança no stub §5.7). Ou seja: o fix elimina a chamada **redundante e insegura** (o objetivo literal de `AUD-01-12`), mas não adiciona nenhuma proteção nova contra o cenário de janela de transição que a spec funcional pede — o método continua exatamente tão vulnerável a `Player` ser inválido no momento da invocação quanto antes.

**Por que importa:** Este item introduz, no mesmo arquivo, `OnGameEndedPatch` (§5.7) que zera `OnGameStartedPatch.Player = null` no `GameWorld.OnDestroy` — ou seja, a partir de agora existe um caminho de código NOVO que pode deixar `Player` nulo enquanto o handler `ApplyMedicalXp` ainda está tecnicamente inscrito (a desinscrição e o `Player = null` acontecem na mesma chamada, mas não há garantia formal de que nenhum evento residual dispare entre a saída de raid e a desinscrição completa). Sem um guard de nulidade, `Player!.Skills` nesse cenário lançaria uma exceção não tratada dentro de um event handler — exatamente o sintoma que motivou a introdução de guards `IsYourPlayer`/null-check em outros achados desta mesma auditoria (itens 001/002).

**Sugestão:** No stub de `ApplyMedicalXp` (§5.7), trocar a primeira linha por um guard explícito:
```csharp
private static void ApplyMedicalXp(IEffect effect)
{
    if (Player == null)
    {
        return;
    }

    var skillMgrExt = Player.Skills.SkillManagerExtended;
    // ...resto do método sem alteração adicional, usando `Player` em vez de `Player!`...
}
```
Isso também casa melhor com `OnGameEndedPatch` zerando `Player` no fim da raid: qualquer invocação tardia/residual do evento passa a ser um no-op seguro, em vez de depender só da ordem exata de desinscrição.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicado — `ApplyMedicalXp` ganha o guard `if (Player == null) return;` na primeira linha; `Player!` trocado por `Player` (sem null-forgiving) no resto do método. Ver spec técnica §5.7 atualizada.

---

### PA-01-03 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-07

**Stub de `OnGameEndedPatch` (§5.7) referencia `ApplyMedicalXpPublic`/`ApplyNatoRifleXpPublic`/`ApplyEasternRifleXpPublic` — membros que não existem em lugar nenhum da spec**

**Problema:** O bloco de código em §5.7 escreve:
```csharp
OnGameStartedPatch.Player.ActiveHealthController.EffectStartedEvent -= OnGameStartedPatch.ApplyMedicalXpPublic;
OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyNatoRifleXpPublic;
OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyEasternRifleXpPublic;
```
Esses três nomes (`...Public`) não são declarados em nenhum lugar do documento — a nota logo abaixo do bloco explica que são "placeholder" e que o nome real deve ser `ApplyMedicalXp`/etc. com visibilidade `internal`. A regra do `/create-technical-spec` ("Stubs devem compilar se copiados num projeto vazio") não é satisfeita por este bloco especificamente, mesmo com a ressalva em prosa logo abaixo.

**Por que importa:** Baixo impacto — a intenção está clara e documentada em texto — mas um stub que não compila como está escrito é inconsistente com o resto da spec (todos os outros 8 blocos de código compilam de fato) e pode causar um copy-paste acidental do nome errado durante o `/code-mod` se o revisor não ler a nota logo abaixo com atenção.

**Sugestão:** Reescrever o bloco usando os nomes reais diretamente:
```csharp
OnGameStartedPatch.Player.ActiveHealthController.EffectStartedEvent -= OnGameStartedPatch.ApplyMedicalXp;
OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyNatoRifleXp;
OnGameStartedPatch.Player.Skills.OnMasteringExperienceChanged -= OnGameStartedPatch.ApplyEasternRifleXp;
```
E no stub principal de `OnGameStartedPatch` (mesma seção), trocar a visibilidade de `ApplyMedicalXp`, `ApplyNatoRifleXp` e `ApplyEasternRifleXp` de `private static` para `internal static` diretamente na assinatura, em vez de deixar essa mudança só na nota em prosa.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicado — os 3 métodos viram `internal static` diretamente na assinatura do stub, e `OnGameEndedPatch` referencia os nomes reais (`ApplyMedicalXp`/`ApplyNatoRifleXp`/`ApplyEasternRifleXp`), sem placeholder. Ver spec técnica §5.7 atualizada.

---

### PA-01-04 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-07

**Troca de `HealthEffectsComponent.IHealthEffect` por um wrapper não foi verificada contra possível comparação por referência em outro código do Assembly**

**Problema:** A §1.1 documenta corretamente que `HealthEffectsComponent.IHealthEffect` é um campo público settable e que trocá-lo por `PerInstanceHealthEffect` resolve o compartilhamento de `Cost`. O que a spec não verifica (e registra implicitamente como não verificado, já que não há nenhuma citação de Assembly sobre isso) é se **algum outro código do EFT** compara `component.IHealthEffect` por referência contra o `IHealthEffect` original cacheado na tabela de templates do jogo (um padrão comum em código que usa templates como chave de cache/dedup, ex.: `if (component.IHealthEffect == cachedTemplateForId)`). Se esse padrão existir em algum lugar não localizado nesta sessão, trocar a referência por um wrapper quebraria essa comparação silenciosamente.

**Por que importa:** Risco baixo (o campo é `[NonSerialized]` e usado majoritariamente através da property `DamageEffects`/`UseTime`/etc., não por igualdade direta, pelo padrão observado no restante de `HealthEffectsComponent.cs` e `Item.cs` lidos nesta sessão) — mas não foi **exaustivamente** descartado, diferente dos outros 8 pontos de patch desta spec, que tiveram seus consumidores confirmados.

**Sugestão:** Durante o `/code-mod`, rodar um Grep rápido por `.IHealthEffect ==` / `.IHealthEffect !=` fora de `HealthEffectComponentPatch.cs` antes de considerar o achado fechado — ou simplesmente validar em teste manual (usar um item médico várias vezes seguidas, checar que não há comportamento anômalo de UI/estado). Não bloqueante; registrar o resultado no asbuild.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aceito como item de verificação durante o `/code-mod` (Grep + teste manual) — adicionado ao checklist de implementação da spec técnica (§8). Não exige mudança de design nesta rodada.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-07 | Review técnica 01 criada via `/review-technical-spec` |
| 2026-09-07 | Usuário aceitou as 4 sugestões; spec técnica atualizada (`002-spec-tech.md` §1.3/§1.6/§5.3/§5.7/§8) |

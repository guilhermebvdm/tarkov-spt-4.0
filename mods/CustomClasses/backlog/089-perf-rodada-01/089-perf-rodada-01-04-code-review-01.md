# 089 — perf — Rodada 01 de otimização · Code Review 01

**Mod:** CustomClasses
**Spec técnica:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-08-24

> Revisão do **código implementado** (commits `ecce1ee5`, `d79b7847`, `6f97e82b`), não da spec. Cada achado recebe um ID `CR-01-MM`. Escopo: os 8 achados `AUD-01-*` mais os 32 pontos `PA-*` das quatro reviews técnicas.

## Resumo

> 🔴 Bloqueadores: 1 · 🟡 Importantes: 2 · 🟢 Menores: 3 · ✅ Verificados sem achado: 9 · Total: 6 achados
>
> **✅ TODOS OS 6 APLICADOS em 2026-09-01** (commit de correção). `CR-01-01` corrigido (não documentado): `__state` virou o par `SyncState(Original, MayHaveScaled)`, com o flag decidido ANTES dos branches — preserva o PA-03-02 e fecha a fronteira com terceiros. `CR-01-02`: teste de config+classe movido para antes do `MainPlayer` no Postfix de dano. `CR-01-03`: `SyncPerfDump` blindado com try/catch. `CR-01-04`/`CR-01-06`: registrados no `05-asbuild`. `CR-01-05`: comentário do alpha no `Quantize`.

**Build:** `dotnet build --no-incremental` → **0 erros, 1 warning** (`CS8602` em `ClassMovementPatches.cs:108` — **pré-existente**, era a linha 95 antes das inserções de instrumentação; não é regressão, e a 01-spec já previa que ele podia sobreviver).

⚠️ **Nota de método:** uma passagem intermediária reportou "0 warnings". Era artefato de **build incremental** — o arquivo não foi recompilado naquele passo. O rebuild completo mostra o warning pré-existente intacto. Toda contagem deste review usa `--no-incremental`.

**Ambiente de build:** este worktree não tinha `.spt-path` nem `References/`. Ambos foram criados a partir de `D:/SPT` para permitir compilar **sem instalar** (o `/compile-mod` instalaria e destruiria a linha de base). A DLL de baseline em [`builds/pre-089-2026-08-23/`](../../builds/pre-089-2026-08-23/) está intacta.

## Verificações objetivas (passaram)

| # | Invariante | Resultado |
|---|---|---|
| 1 | Os 5 `Enable()` novos registrados (PA-03-05) | `ShootCapturePatch`, `ShootApplyPatch`, `ClassDamagePatch`, `FirearmSyncPatch`, `TotalErgoPatch` — **1 cada** |
| 2 | Nenhum `Enable()` órfão das 13 classes removidas | **0** |
| 3 | `PWA.Shoot` = **2** patches (não 1, não 4) — PA-01-01 | ✅ `Priority.First` + `Priority.Last` presentes |
| 4 | `ApplyDamageInfo` · `SetAnimatorAndProceduralValues` · `TotalErgonomics` = **1 classe cada** | ✅ |
| 5 | `try/catch` por branch nos 4 consolidados (PA-02-01) | 4 · 3 · 2 · 3 branches isolados; **nenhum catch externo único** |
| 6 | `ClassNameEnOf` extinto (PA-03-01) | 1 ocorrência = o comentário histórico que explica a remoção |
| 7 | Literais de nome de classe em gates (AUD-01-02) | **0** |
| 8 | `ClassIdOf` é o único resolvedor nos 5 call-sites | ✅ |
| 9 | `Local()` preenche `ClassId` (PA-04-04) · `ClassChanged` disparado em `Apply` **e** `Reset` (PA-04-03) · `ValidateClassLists()` chamada (PA-04-02) | ✅ |
| 10 | Versão `0.16.9` nos **4** arquivos (PA-02-02) | `grep 0.16.8` só acha `obj/` (regenerado) e um comentário histórico |
| 11 | Gates de instância da regra 075 | 11 arquivos de patch mantêm `ReferenceEquals` |
| 12 | `PERF-INSTR` marcados | 17 blocos |

---

## Achados

### CR-01-01 · 🔴 Bloqueador — `FirearmSyncPatch` restaura `ReloadSpeed` incondicionalmente e pode clobberar escrita de terceiros

**Onde:** [ClassWeaponPatches.cs — `FirearmSyncPatch.Postfix`](../../modded/Client/Patches/ClassWeaponPatches.cs)

**Problema.** O `PA-03-02` (aceito) exigiu capturar o `__state` **incondicionalmente antes dos branches**, e implementei exatamente isso. Consequência não prevista: o **Postfix agora restaura sempre**, enquanto o código original restaurava **só quando havia escalado** (`__state` ficava `NaN` caso contrário).

Verifiquei o alvo no decompile (`EFT/Player.cs:12634-12664`) e a boa notícia é que `SetAnimatorAndProceduralValues` **só LÊ** `gclass2250_0.ReloadSpeed` — nunca escreve. A escrita mora em `SyncWithCharacterSkills` (`:12678`), que grava e **depois** chama o método patcheado; então minha captura pega o valor fresco e a restauração o devolve idêntico. **Não há regressão no caminho vanilla.**

O que **mudou** é a exposição a terceiros: se outro mod tiver um Prefix neste mesmo alvo que rode **depois** do meu e escreva `ReloadSpeed`, o meu Postfix agora **sobrescreve** essa escrita. Antes, isso só acontecia quando um branch nosso tinha escalado; agora acontece sempre que o gate passa.

**Por que é 🔴 e não 🟢:** não pelo tamanho do risco (nenhum mod conhecido patcha este alvo), mas porque é **exatamente a classe de erro que o `PA-01-01` identificou e que esta rodada existe para não cometer** — alterar uma fronteira de composição com terceiros como efeito colateral de uma consolidação. Deixar passar sem decisão explícita contradiz o critério que a própria rodada estabeleceu.

**Correção proposta.** `__state` vira um par (valor original + "podem ter mexido"), preservando o PA-03-02:

```csharp
// A flag é setada ANTES de chamar os branches — não por eles. Um branch que lance no meio de uma
// mutação continua coberto (é a garantia do PA-03-02), e um sync em que nenhum branch podia atuar
// não toca no campo (fecha o CR-01-01).
private readonly struct SyncState
{
    internal readonly float Original;
    internal readonly bool MayHaveScaled;
    internal SyncState(float original, bool mayHaveScaled) { Original = original; MayHaveScaled = mayHaveScaled; }
}
```

No Prefix: capturar `original` incondicionalmente; setar `MayHaveScaled = true` imediatamente antes do bloco de branches **apenas se algum branch pode agir** (config ligada + classe casa) — a checagem já existe dentro de cada branch, então basta um `if` barato antes. No Postfix: restaurar só se `MayHaveScaled`.

**Alternativa aceitável (menor diff):** manter como está e **documentar a mudança de fronteira** num comentário, aceitando o trade-off. Só não vale deixar implícito.

**Como validar:** recarregar escopeta como Tanque e recarregar na janela de Adrenalina como Fuzileiro — os dois devem manter o ganho de velocidade; e um perfil de classe **sem** perk de recarga não deve ter `ReloadSpeed` alterado em nenhum momento.

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Aceitar alternativa (documentar) · `[ ]` Caminho alternativo: ______

---

### CR-01-02 · 🟡 Importante — `ClassDamagePatch.Postfix` resolve o gate antes do teste de classe, invertendo a ordem barato→caro

**Onde:** [ClassCombatHealthPatches.cs — `ClassDamagePatch.Postfix`](../../modded/Client/Patches/ClassCombatHealthPatches.cs)

**Problema.** O `AdrenalineTriggerPatch` original testava `AdrenalineEnabled` + `IsLocalClass(Rifleman)` **primeiro** e só então resolvia o `MainPlayer`. Meu Postfix consolidado resolve `Singleton<GameWorld>.Instance?.MainPlayer` **antes**, para passar `mp` ao branch.

Para quem **não** é Fuzileiro — a maioria dos perfis — isso troca "1 deref de config + 1 compare" por "acesso ao Singleton + leitura de campo + deref + compare", em **todo evento de dano de qualquer entidade do mapa**. É pequeno (o `MainPlayer` é campo, não busca — verificado em `GameWorld.cs:572`), mas é a superfície onde a rodada mais se importa com ordem de gate, e a mudança vai na direção errada.

**Correção proposta.** Mover o teste barato para o topo do Postfix, antes de resolver o `MainPlayer`:

```csharp
if (PerksConfig.AdrenalineEnabled?.Value != true || !SkillMultipliers.IsLocalClass(EClassId.Rifleman)) return;
var mp = Singleton<GameWorld>.Instance?.MainPlayer;
if (mp == null) return;
```

O contador `DamageGates` do INSTR-2 passa a ser incrementado depois — o que continua correto para a métrica 4→2 (o Postfix segue contando 1 gate quando executa).

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: ______

---

### CR-01-03 · 🟡 Importante — `SyncPerfDump` chama `StartCoroutine` de dentro de um handler de `SettingChanged`

**Onde:** [Plugin.cs — `SyncPerfDump` / `PerfDumpLoop`](../../modded/Client/Plugin.cs)

**Problema.** O `PA-01-10` pedia que a corrotina de dump só existisse com o diagnóstico ligado, e implementei ligando-a ao `SettingChanged` do toggle. `StartCoroutine`/`StopCoroutine` são APIs Unity **restritas à main thread**; o `ConfigEntry.SettingChanged` do BepInEx dispara no contexto de quem escreveu o valor. Pelo F12 (ConfigurationManager) isso é a main thread — mas um recarregamento de arquivo de config ou outro mod escrevendo a entrada pode não ser, e aí a chamada lança.

**Por que importa.** É instrumentação temporária: uma exceção aqui poluiria o log e, pior, no caminho de `Awake` poderia interromper o registro dos patches seguintes. O custo de blindar é uma linha.

**Correção proposta.** Envolver o corpo de `SyncPerfDump` em `try/catch` com log único, e/ou trocar a corrotina por uma checagem de tempo dentro do `OnGUI`/`Update` já existente (que é garantidamente main thread). Preferência: o `try/catch` — mantém o desenho do PA-01-10 (nada roda com o toggle off) e custa quase nada.

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: ______

---

### CR-01-04 · 🟢 Menor — `ShootCapturePatch` conta um "gate" mesmo quando a arma é de bot

**Onde:** [ClassWeaponPatches.cs — `ShootCapturePatch.Prefix`](../../modded/Client/Patches/ClassWeaponPatches.cs)

**Problema.** O contador `ShootGates` é incrementado no Capture **antes** de saber se é a arma do player local; o `ShootApplyPatch` incrementa de novo quando executa. Para um tiro de bot, o resultado é `ShootGates = 1` — o que é defensável (o gate FOI resolvido), mas a métrica de aceite fala em "4 → 2 execuções de gate **por evento**", e um leitor pode interpretar o 1 como meia consolidação.

**Correção proposta.** Manter o incremento (é o número honesto) e acrescentar ao `05-asbuild` a nota de leitura: *"`shoot=(N) gates=(M)`: M ≈ 2N num tiro do player local e ≈ N para tiros de bot, porque o Capture resolve o gate para todos e o Apply só para o local."*

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: ______

---

### CR-01-05 · 🟢 Menor — a quantização de cor descarta o alpha da chave sem dizer por quê

**Onde:** [ClassIconCache.cs — `Quantize`](../../modded/Client/UI/ClassIconCache.cs)

**Problema.** A chave antiga usava `ToHtmlStringRGBA` (com alpha); a nova fixa `alpha = 255`. Está **correto** — o laço de tingimento multiplica só RGB e preserva o alpha da textura de origem, então o alpha da cor nunca influenciou o resultado, e duas cores que só diferem em alpha produziriam texturas idênticas. Mas o código não registra esse raciocínio, e a próxima pessoa a ler pode achar que é um bug de colisão de chave.

**Correção proposta.** Uma linha de comentário no `Quantize`: *"alpha fixo em 255 na chave: o laço de tingimento multiplica só RGB e preserva o alpha da textura de origem — duas cores que só diferem em alpha geram a MESMA textura, então colidir é correto."*

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: ______

---

### CR-01-06 · 🟢 Menor — desvio deliberado da spec na chave do cache de tooltip

**Onde:** [SkillPanelPatch.cs — `TooltipCache`](../../modded/Client/Patches/SkillPanelPatch.cs)

**Problema (registro, não defeito).** A spec técnica (§5.7, após o `PA-01-03`) previa a chave `(ESkillId, float, string?)`. A assinatura real é `MultiplierFormat.TooltipText(float factor, string? className)` (`MultiplierFormat.cs:55`) — **não recebe o skill id**. Incluí-lo criaria N entradas idênticas para o mesmo texto. Implementei `(float, string?)`, com o desvio documentado no XMLdoc do campo.

O requisito que o `PA-01-03` realmente protegia — o `className` na chave — **está cumprido**, e a invalidação por `ClassChanged` (PA-04-03) cobre os dois cenários de mudança.

**Ação:** nenhuma no código. Registrar o desvio no `05-asbuild` para a spec e a implementação não divergirem sem rastro.

**Decisão:** `[ ]` Pendente · `[x]` Registrar e seguir · `[ ]` Reverter para a chave da spec

---

## O que foi verificado e está correto

Registrado para a Fase 4 saber o que **não** precisa reabrir:

- **`PA-01-01` honrado.** `PWA.Shoot` tem exatamente 2 patches, com `Priority.First` e `Priority.Last`. O estático `ShootRecoilState.StrBefore` foi mantido (dois patches não compartilham `__state`) e a ordem interna do Apply é maestria → perks → piso → diag, copiada 1:1 das prioridades antigas.
- **`PA-02-01` honrado nos quatro alvos** — nenhum tem `try/catch` externo único; `BranchFailLog` deduplica por nome de branch.
- **`PA-03-02` honrado** — `__state` capturado antes de qualquer branch (e o `CR-01-01` é sobre o *outro lado* dessa mudança).
- **AP-09 respeitado.** O compilador rejeitou `GClass2250` numa assinatura e a correção foi passar o `FirearmController` e ler `BuffInfo` internamente — como o código original fazia. Nenhum tipo ofuscado é nomeado em assinatura nova.
- **Regra 075 intacta.** Os gates de instância continuam em todos os patches que rodam para bots/peers; a consolidação **compartilha** o gate, não o afrouxa (os branches que precisam de gate adicional — Couraça, Execution — mantêm o seu).
- **`AUD-01-08`** — `Quantize` clampa depois de multiplicar (PA-01-02), `Touch` faz move-to-end (PA-03-03), `EvictIfNeeded` remove do início com guard de mesmo-frame, `Dispose` limpa as duas estruturas novas.
- **`BuildTinted`** preserva o `try/catch`, o aviso de arquivo ausente e o `Destroy(tex)` do ramo de `LoadImage` falho (PA-01-09).
- **`Bulwark` → `BulwarkArmor`**: o helper foi renomeado porque `Bulwark` colidia com o método `DamageBranches.Bulwark`. O novo nome descreve melhor o que a classe faz (detecção de armadura de tronco) e o consumidor do overlay foi atualizado.

## Pendências que este review NÃO cobre

- **Passo 0b — raid de linha de base** (gate humano, `PA-01-04`). Sem ela, os ACs de perks que P-10.1/P-16.1 marcam como nunca validados continuam indecidíveis. A DLL do backup permite executá-la a qualquer momento.
- **`/update-mod-graph CustomClasses`** (`PA-02-09`) — 13 classes de patch removidas, 5 criadas, `IsLocalClass` com assinatura nova. O hook de pre-commit já avisou duas vezes.
- **Validação in-game de tudo** — nada aqui foi executado no jogo. Compilar ≠ funcionar (AP-06).
- **Remoção da instrumentação** (`PA-01-10`) — 17 blocos `PERF-INSTR` a remover na Fase 4.

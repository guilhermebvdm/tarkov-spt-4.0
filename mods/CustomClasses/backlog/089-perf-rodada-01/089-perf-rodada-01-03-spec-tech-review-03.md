# 089 — perf — Rodada 01 de otimização · Review Técnica 03

**Mod:** CustomClasses
**Spec técnica revisada:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-08-23

> Terceira rodada, sobre a spec já corrigida pelas reviews [01](089-perf-rodada-01-03-spec-tech-review-01.md) e [02](089-perf-rodada-01-03-spec-tech-review-02.md). Ângulos novos: **o que a própria correção da review 02 deixou pela metade** e **o que acontece nos call-sites que a spec descreve em prosa mas não em código**. Cada ponto recebe um ID `PA-03-MM`.

## Resumo

> 🔴 Bloqueadores: 2 · 🟡 Importantes: 3 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 7

**Memória consultada:** snapshot de 2026-08-03 · pendências que afetam: 🔴 P-10.1, 🔴 P-16.1 (endereçadas em `PA-01-04` — não voltam).

**Verificação das reviews anteriores:** 10/10 da review 01 e 9/9 da review 02 aplicados na spec. Nenhum é reaberto. Dois pontos desta rodada (`PA-03-01`, `PA-03-02`) são **consequências não fechadas** de correções aceitas — não contradizem as anteriores, completam-nas.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-03-01 | C — Erro de Lógica | 🔴 Bloqueador | Migrar o gate para `ClassIdOf` sem tratar o `LogPeer` deixa **dois lookups por passo** — o hot path fica pior, não melhor | Pendente |
| PA-03-02 | C — Erro de Lógica | 🔴 Bloqueador | O `try/catch` por branch do `PA-02-01` **não** resolve o `BuffInfo.ReloadSpeed` sujo: falta capturar o `__state` antes de qualquer mutação | Pendente |
| PA-03-03 | A — Gap | 🟡 Importante | `Touch()` — o mecanismo LRU inteiro do `AUD-01-08` — é chamado três vezes e nunca definido | Pendente |
| PA-03-04 | C — Erro de Lógica | 🟡 Importante | O critério de aceite do `AUD-01-08` (≤1 por classe) contradiz o desenho da spec técnica (cap 4 por ícone) | Pendente |
| PA-03-05 | A — Gap | 🟡 Importante | A lista final de `Enable()` não é enumerada — e **esquecer de registrar** um patch consolidado é silencioso (sem erro de compilação) | Pendente |
| PA-03-06 | B — Edge Case | 🟢 Menor | A checagem de boot do `PA-02-03` consome o warn-once do `Parse`, silenciando o aviso real de classe desconhecida | Pendente |
| PA-03-07 | A — Gap | 🟢 Menor | Manter `RecoilFloorPatch.cs` só com XMLdoc cria um arquivo sem código — decidir entre mover o doc ou assumir o arquivo-lápide | Pendente |

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

### PA-03-01 · C — Erro de Lógica · 🔴 Bloqueador

**Migrar o gate para `ClassIdOf` sem tratar o `LogPeer` deixa dois lookups por passo — o hot path fica pior, não melhor**

**Problema:** a §5.2 diz que "os helpers de som (`QuietStep.MultFor`, `LoudOperator.MultFor`, `SilentLooter.MultFor`) passam a receber `EClassId`". Mas os dois patches que os chamam **também usam a string**, e a spec não diz o que acontece com ela:

```csharp
// ClassSoundPatches.cs:290 — AiSoundPatch.Prefix (hoje)
var emitterClass = ClassIdentities.ClassNameEnOf(p);      // 1 lookup de dicionário
if (emitterClass is null) return;
power *= QuietStep.MultFor(emitterClass);
power *= LoudOperator.MultFor(emitterClass);
…
PerkDiag.LogPeer("AI hear power", p.Profile?.Nickname ?? "?", emitterClass, p0, power);   // ← usa a STRING
```

O mesmo em `SoundRadiusPatch.Postfix` (`:168` e `:206`). Seguindo a spec ao pé da letra, a implementação natural é acrescentar `ClassIdOf(p)` **sem remover** `ClassNameEnOf(p)` — porque o log precisa do nome. Resultado: **duas resoluções de classe por passo de cada player/bot**, cada uma com o seu `IsAI` + `Profile?.Nickname` + `TryGetValue`.

**Por que importa:** `AiSoundPatch` e `SoundRadiusPatch` são as duas superfícies de **maior frequência** do mod inteiro (per-passo × N players+bots — Panorama do relatório 01). O `AUD-01-02` existe para baratear o gate; implementado assim, ele **dobra** o custo exatamente onde mais dói. Um achado de performance que piora a performance é a pior falha possível desta rodada, e passaria despercebida: nada quebra, nenhum teste falha, o overlay não muda.

**Sugestão:** especificar o contrato explicitamente na §5.2 — **`ClassIdOf` é o único resolvedor no caminho quente; o nome só é resolvido dentro do gate de diagnóstico**:

```csharp
var emitterId = ClassIdentities.ClassIdOf(p);          // ref: AUD-01-02 — ÚNICO lookup do hot path
if (emitterId == EClassId.None) return;

var p0 = power;
power *= QuietStep.MultFor(emitterId);
power *= LoudOperator.MultFor(emitterId);

if (PerkDiag.Enabled)   // ⚠️ PA-03-01: o NOME só é resolvido aqui dentro — nunca no caminho quente
{
    if (p.IsYourPlayer) { PerkDiag.AiPowerBefore = p0; PerkDiag.AiPowerAfter = power; }
    else if (power != p0)
    {
        PerkDiag.LogPeer("AI hear power", p.Profile?.Nickname ?? "?",
                         SkillMultipliers.NameOf(emitterId), p0, power);   // enum → string, sem dicionário
    }
}
```

Isso exige um helper novo `SkillMultipliers.NameOf(EClassId)` (o inverso do `Parse`, um `switch` puro) — acrescentar ao §5.1. E acrescentar ao §8: *"`ClassNameEnOf` não pode ser chamado em `AiSoundPatch`, `SoundRadiusPatch`, `SainSoundPatch` nem `SilentKnifePatch` — só `ClassIdOf`; `grep -n 'ClassNameEnOf' modded/Client/Patches/` deve voltar vazio"*.

Decidir também o destino de `ClassNameEnOf`: ou vira `private`/removido, ou fica com um XMLdoc marcando que **não** deve ser usado em hot path. Preferência: **remover** — o compilador então garante que ninguém o reintroduza.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-02 · C — Erro de Lógica · 🔴 Bloqueador

**O `try/catch` por branch do `PA-02-01` não resolve o `BuffInfo.ReloadSpeed` sujo — falta capturar o `__state` antes de qualquer mutação**

**Problema:** o `PA-02-01` (aceito) identificou corretamente que um branch que lança não pode derrubar os irmãos, e a correção foi `try/catch` por branch. Mas ele descreveu o caso do `SetAnimatorAndProceduralValues` assim: *"se o branch de Adrenalina lançar depois de escalar `BuffInfo.ReloadSpeed` mas antes de gravar o `__state`, o Postfix não restaura e o campo fica sujo"* — e **o `try/catch` por branch não conserta isso**. Ele contém a exceção; não desfaz a mutação nem preenche o `__state`.

A raiz é a ordem dentro do branch. Hoje, cada patch faz o certo por acidente de escrita:

```csharp
__state = buff.ReloadSpeed;   // captura ANTES
buff.ReloadSpeed /= t;        // muta DEPOIS
```

Mas a spec consolidada (§5.6, prosa) diz apenas que *"o `__state` guarda o `BuffInfo.ReloadSpeed` original **uma única vez**"* — sem fixar **quando**. Um implementador que escreva `if (branchAdrenalina) { buff.ReloadSpeed /= t; __state = original; }` reproduz o bug, e o `try/catch` por branch o mascara (a exceção some no log 1×/sessão do `BranchFailLog`, e o campo fica escalado pela raid inteira: recarga permanentemente acelerada).

**Por que importa:** é o único ponto da rodada que **escreve estado persistente do EFT**. Um campo sujo aqui não se auto-corrige — sobrevive até o próximo `SetAnimatorAndProceduralValues` que passe pelo caminho de restauração, e como o Prefix é quem arma a restauração, um Prefix que falhou nunca a arma.

**Sugestão:** tornar a ordem **estrutural**, não uma convenção. O Prefix consolidado captura o `__state` **incondicionalmente e antes de qualquer branch**:

```csharp
[PatchPrefix]
private static void Prefix(Player.FirearmController __instance, out float __state)
{
    __state = float.NaN;

    var buff = __instance.BuffInfo;
    if (buff == null) return;
    if (!ReferenceEquals(__instance, Singleton<GameWorld>.Instance?.MainPlayer?.HandsController)) return;

    // ⚠️ PA-03-02 — captura INCONDICIONAL e ANTES de qualquer branch. Se um branch lançar no meio de uma
    // mutação, o Postfix ainda tem o valor original para restaurar. Um try/catch por branch (PA-02-01)
    // contém a exceção mas NÃO desfaz a escrita — só esta ordem garante que o campo nunca fique sujo.
    __state = buff.ReloadSpeed;

    try { ReloadBranches.Adrenaline(buff); } catch (Exception ex) { BranchFailLog.Once("reload/adren", ex); }
    try { ReloadBranches.Shotgun(__instance, buff); } catch (Exception ex) { BranchFailLog.Once("reload/shotgun", ex); }
}
```

O Postfix restaura sempre que `__state` não for `NaN` — inclusive quando nenhum branch escalou (restaurar o mesmo valor é no-op inofensivo, e é mais barato que rastrear "mudei ou não"). Acrescentar ao §8: *"em `SetAnimatorAndProceduralValues`, o `__state` é capturado incondicionalmente antes dos branches; nenhum branch grava `__state`"*.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-03 · A — Gap · 🟡 Importante

**`Touch()` — o mecanismo LRU inteiro do `AUD-01-08` — é chamado três vezes e nunca definido**

**Problema:** o stub da §5.3 chama `Touch(name, key)` em três lugares (hit de cache, após inserir, antes de `EvictIfNeeded`) e **nunca a define**. É exatamente o mesmo tipo de lacuna que a review 01 pegou no `BuildTinted` (`PA-01-09`) — e é mais grave aqui, porque `Touch` **é** a política LRU: ela decide qual variante morre.

Sem definição, a implementação fica ambígua em pontos que mudam o comportamento:
- `Touch` cria a lista em `VariantsByIcon` quando o ícone é novo, ou isso é responsabilidade do chamador?
- No hit de cache, ela **move** a chave para o fim (LRU de verdade) ou só registra (que degeneraria para FIFO)?
- Em FIFO, uma variante **em uso constante** (o brasão com gradiente, redesenhado a cada `Show`) seria evicta antes de uma variante velha e parada — o oposto do desejado, e reintroduziria o risco de sprite destruído em uso que o cap 4 existe para evitar.

**Por que importa:** a diferença entre LRU e FIFO aqui é a diferença entre "o cap 4 protege" e "o cap 4 destrói o sprite que está na tela".

**Sugestão:** definir na §5.3, explicitando o move-to-end:

```csharp
/// <summary>ref: AUD-01-08 · PA-03-03 — LRU de verdade: usar uma variante a manda para o FIM da fila.
/// Sem o move-to-end isto degenera em FIFO e o brasão em uso (redesenhado a cada Show) seria evicto
/// antes de uma variante velha e parada — exatamente o sprite que não pode morrer.</summary>
private static void Touch(string name, string key)
{
    if (!VariantsByIcon.TryGetValue(name, out var keys))
    {
        keys = new List<string>(MaxVariantsPerIcon + 1);
        VariantsByIcon[name] = keys;
    }

    var at = keys.IndexOf(key);
    if (at >= 0) keys.RemoveAt(at);   // já existia → tira da posição atual
    keys.Add(key);                    // …e recoloca no fim (mais recente)
}
```

`keys.IndexOf` é O(n) com n ≤ 5 — irrelevante, e a lista mantém a ordem de recência sem estrutura extra. `EvictIfNeeded` continua removendo do **início** (o menos recente).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-04 · C — Erro de Lógica · 🟡 Importante

**O critério de aceite do `AUD-01-08` contradiz o desenho da spec técnica**

**Problema:** a 01-spec (critérios B) diz:

> *"arrastando o picker de cor de uma classe por ~5 s, `tintedCache` **para de crescer** — fica ≤ número de ícones distintos usados na tela (**esperado: 1 por classe visível**)"*

A spec técnica (§5.3) projeta `MaxVariantsPerIcon = 4`, justamente porque **o mesmo ícone precisa de duas variantes vivas** (brasão com gradiente + marca d'água chapada) mais uma geração de folga. O limite real é `ícones × 4`, não `ícones × 1`.

**Por que importa:** o AC seria **reprovado por um resultado correto**. Numa aba CLASS aberta, o esperado é `tintedCache` estabilizar em ~2 (as duas formas do ícone da classe) e, durante um arrasto, oscilar até 4 antes de evictar. Quem validar seguindo a 01-spec vai ver "2 a 4" onde o critério pede "1", reportar falha e mandar investigar um comportamento que está certo.

**Sugestão:** alinhar o AC ao desenho, com o número derivado em vez de chutado:

> *"arrastando o picker de cor de uma classe por ~5 s, `tintedCache` **para de crescer** e estabiliza em **≤ 4 entradas por ícone distinto em uso** (o cap `MaxVariantsPerIcon`). Numa aba CLASS aberta o valor típico é **2** (brasão com gradiente + marca d'água chapada), podendo tocar 4 durante o arrasto antes da eviction. O que **reprova** é crescimento monotônico — qualquer número que continue subindo com o arrasto."*

O critério verdadeiro é "para de crescer", não um valor absoluto; a redação atual trocou um pelo outro.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-05 · A — Gap · 🟡 Importante

**A lista final de `Enable()` não é enumerada — e esquecer de registrar um patch consolidado é silencioso**

**Problema:** a §4 diz que `Plugin.cs` deve "ajustar os `Enable()`" e a §7 alerta que "a remoção dos `Enable()` dos patches consolidados tem de ser **exata**". Mas a spec **nunca lista o conjunto final**. O risco não é simétrico:

- **Remover** um `Enable()` de classe que deixou de existir → **erro de compilação**. O compilador protege.
- **Esquecer de adicionar** o `Enable()` de um patch consolidado novo → **compila perfeitamente** e o alvo inteiro fica sem patch. Todos os perks daquele alvo morrem em silêncio.

Esse segundo caso é concreto: se `ShootApplyPatch().Enable()` faltar, o jogo perde **de uma vez** maestria de recuo (058), Shaky Hands, Adrenalina-recuo, Bunker **e** o piso B15 — e o único sintoma é "o recuo parece diferente". Com a linha de base desconhecida do `PA-01-04`, ninguém consegue afirmar que é regressão.

**Por que importa:** é a falha mais provável de uma refatoração que mexe em ~13 classes de patch, e a única sem rede de proteção do compilador.

**Sugestão:** enumerar na §5.6 (ou numa §5.9 nova) o **diff exato** do bloco de registro no `Plugin.Awake`:

```
REMOVER (classes deixam de existir — o compilador acusa):
  new ShootRecoilPatch().Enable()            new RecoilFloorCapturePatch().Enable()
  new RecoilFloorApplyPatch().Enable()       new WeaponMasteryRecoilPatch().Enable()
  new WeaponMasteryErgoPatch().Enable()      new HeavyWeaponErgoPatch().Enable()
  new BulwarkPatch().Enable()                new ExecutionMeleePatch().Enable()
  new AdrenalineTriggerPatch().Enable()      new LocalHitTypePatch().Enable()
  new ReloadSpeedPatch().Enable()            new ShotgunReloadPatch().Enable()
  new HolsterDrawResetPatch().Enable()

ACRESCENTAR (nada acusa se faltar — conferir um a um):
  new ShootCapturePatch().Enable()      // Priority.First
  new ShootApplyPatch().Enable()        // Priority.Last
  new ClassDamagePatch().Enable()       // ApplyDamageInfo (Prefix + Postfix na mesma classe)
  new FirearmSyncPatch().Enable()       // SetAnimatorAndProceduralValues (Prefix + Postfix)
  new TotalErgoPatch().Enable()         // TotalErgonomics (Postfix)
```

E acrescentar ao §8 um AC de fumaça barato: *"com `Perk Diagnostics` ligado, o overlay 052 mostra `Recoil str` mudando ao atirar (prova o `ShootApplyPatch`), `Ergo (weapon)` refletindo o Bunker com arma pesada (prova o `TotalErgoPatch`) e `Malfunction%` preenchido"* — três leituras num único frame que provam que os patches consolidados estão registrados.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-06 · B — Edge Case · 🟢 Menor

**A checagem de boot do `PA-02-03` consome o warn-once do `Parse`, silenciando o aviso real**

**Problema:** o `Parse` (§5.1) tem um `_warnedUnknownClass` que garante **um aviso por sessão** para classe desconhecida. A checagem de boot do `PA-02-03` chama `Parse(key)` para cada chave de `ClassColors`. Se uma delas for desconhecida, o `Parse` gasta o warn-once **no boot** — e o aviso de uma classe desconhecida que apareça depois, no fetch de um peer ou na troca de perfil (o cenário que o warn-once existe para cobrir), sai **silencioso**.

O inverso também incomoda: numa divergência, o usuário vê dois registros do mesmo problema (o `LogWarning` do `Parse` e o `LogError` da checagem).

**Por que importa:** pouco — só atrapalha diagnóstico, não comportamento. Mas o `Parse` é novo nesta rodada e a interação é fácil de eliminar agora.

**Sugestão:** dar ao `Parse` um parâmetro de supressão e usá-lo na checagem de boot:

```csharp
internal static EClassId Parse(string? nameEn, bool warnUnknown = true)   // ref: PA-03-06
```

A checagem de boot chama `Parse(key, warnUnknown: false)` — ela já emite o próprio `LogError`, mais específico. O warn-once fica reservado para o caminho de runtime.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-03-07 · A — Gap · 🟢 Menor

**Manter `RecoilFloorPatch.cs` só com XMLdoc cria um arquivo sem código**

**Problema:** o `PA-02-07` decidiu que `RecoilBranches` mora em `ClassWeaponPatches.cs` e que `RecoilFloorPatch.cs` "permanece no repo contendo apenas o XMLdoc histórico do B15 com um ponteiro para o novo local". Um `.cs` sem tipo nenhum é um arquivo que o compilador inclui e ignora, e que a próxima pessoa a abrir vai tratar como resíduo de refatoração incompleta — o oposto de preservar contexto.

**Por que importa:** baixo, mas é decisão de arquitetura que a spec toma pela metade. O contexto de balance do B15 **vale** preservar (é a justificativa numérica do piso, com os casos do Anexo C); a questão é onde ele fica legível.

**Sugestão:** mover o XMLdoc do B15 para cima de `RecoilBranches.ApplyFloor`, em `ClassWeaponPatches.cs` — que é onde alguém investigando o piso vai procurar —, e **deletar** `RecoilFloorPatch.cs` (o git preserva o histórico; a spec técnica e este review preservam o rastro). Alternativa aceitável se preferir minimizar o diff: manter o arquivo, mas com um comentário de cabeçalho `// ARQUIVO-LÁPIDE (089/PA-02-07): as classes de patch migraram para ClassWeaponPatches.cs; este arquivo guarda só o histórico do B15.` — o que ao menos remove a ambiguidade.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

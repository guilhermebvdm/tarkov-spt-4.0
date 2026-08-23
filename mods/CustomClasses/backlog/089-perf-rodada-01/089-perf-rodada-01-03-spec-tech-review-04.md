# 089 — perf — Rodada 01 de otimização · Review Técnica 04

**Mod:** CustomClasses
**Spec técnica revisada:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-08-23

> Quarta rodada, sobre a spec já corrigida pelas reviews [01](089-perf-rodada-01-03-spec-tech-review-01.md), [02](089-perf-rodada-01-03-spec-tech-review-02.md) e [03](089-perf-rodada-01-03-spec-tech-review-03.md). Ângulos novos: **a sequência de execução do plano** (não só o conteúdo dele), **as fronteiras entre camadas** que as correções criaram, e **o que os pontos anteriores contaram errado**. Cada ponto recebe um ID `PA-04-MM`.

## Resumo

> 🔴 Bloqueadores: 1 · 🟡 Importantes: 3 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 6

**Memória consultada:** snapshot de 2026-08-03 · pendências que afetam: 🔴 P-10.1, 🔴 P-16.1 — e desta vez **de forma nova**: elas são a razão do `PA-01-04`, cuja execução o `PA-04-01` mostra estar agendada tarde demais.

**Verificação das reviews anteriores:** 10/10 (r01), 9/9 (r02) e 7/7 (r03) aplicados. Nenhum é reaberto. `PA-04-02` **estende** o `PA-02-03` (que contou duas fontes de verdade onde há três) e `PA-04-03` critica a **forma** da correção do `PA-01-03`, não o seu mérito.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-04-01 | C — Erro de Lógica | 🔴 Bloqueador | A raid de baseline está agendada para a Fase 4, mas o `/compile-mod` **instala a DLL** no fim da Fase 3 — a oportunidade some antes da hora marcada | Pendente |
| PA-04-02 | A — Gap | 🟡 Importante | A lista de classes tem **três** fontes de verdade, não duas: o `PA-02-03` esqueceu `PerksCatalog.ByClass` — e as três têm cardinalidades diferentes | Pendente |
| PA-04-03 | A — Gap | 🟡 Importante | A invalidação de cache do `PA-01-03` inverte camadas: `SkillMultipliers` passa a conhecer um patch de UI e o overlay de diagnóstico | Pendente |
| PA-04-04 | B — Edge Case | 🟡 Importante | `ClassIdentities.Local()` monta uma `Identity` **sem** preencher o `ClassId` novo — objeto meio-inicializado à espera de um consumidor | Pendente |
| PA-04-05 | A — Gap | 🟢 Menor | A assimetria deliberada `IsLocalClass` (chama `EnsureLoaded`) × `LocalClassId` (não chama) fica sem documentação | Pendente |
| PA-04-06 | A — Gap | 🟢 Menor | O AC de fumaça da §5.9 cobre 4 dos 5 patches novos; falta dizer que ele também prova o `ShootCapturePatch` | Pendente |

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

### PA-04-01 · C — Erro de Lógica · 🔴 Bloqueador

**A raid de baseline está agendada para a Fase 4, mas o `/compile-mod` instala a DLL no fim da Fase 3**

**Problema:** o `PA-01-04` (aceito, e a base do contrato de não-regressão) estabeleceu:

> *"Fase 4 passa a exigir **raid de baseline na DLL atual antes de instalar a nova**"*

e o item foi parar no bloco **"Gates de validação que a implementação precisa deixar preparados (executados na Fase 4)"** do §8. Só que o ciclo do repo é `/code-mod` → `/code-review` → `/apply-code-review` → **`/compile-mod`**, e o `/compile-mod` **compila e instala**: client em `BepInEx/plugins/CustomClasses/`, server em `SPT/user/mods/` (`AGENTS.md`, e o HANDOFF confirma: *"Compilar+instalar: `bash .agents/scripts/compile-mod.sh CustomClasses`"*).

Ou seja: quando a Fase 4 começa, **a DLL atual já foi sobrescrita**. A raid de baseline não é "difícil" nesse ponto — é **impossível**, e de forma irreversível (a DLL instalada não está versionada; `mods/CustomClasses/builds/` pode ou não ter uma cópia, e a spec não verifica).

**Por que importa:** o `PA-01-04` existe porque **P-10.1** e **P-16.1** dizem que ~21 efeitos nunca foram validados in-game. Sem a baseline, todo AC de não-regressão do tipo "o perk X continua funcionando" fica indecidível: se X não funcionar depois, ninguém sabe se a rodada quebrou ou se já estava quebrado. Isso não degrada a validação — **anula** a premissa central da rodada inteira, que é justamente "o contrato funcional é o comportamento atual".

É um erro de **sequência**, não de conteúdo: o passo certo, no lugar errado.

**Sugestão:** mover a baseline para **antes da Fase 3**, como pré-condição do `/code-mod`, e proteger a DLL atual:

1. **Passo 0 da Fase 3 (antes de qualquer edição):**
   - copiar a DLL instalada para `mods/CustomClasses/builds/pre-089-<data>/`, anotando tamanho e data;
   - anotar a versão que aparece no log de boot (é o `BepInPlugin` de `Plugin.cs:13` — hoje `0.16.8`);
   - rodar **uma raid** percorrendo a matriz de perks das 6 classes + vanilla com `Perk Diagnostics` ligado, registrando o resultado por perk em `089-perf-rodada-01-05-asbuild.md` (seção "Linha de base pré-089").
2. Reclassificar o item no §8: sai de "gates de Fase 4" e entra como **pré-condição bloqueante da Fase 3**.
3. Acrescentar ao §7 o risco de perda: *"o `/compile-mod` instala automaticamente; sem o passo 0, a linha de base é irrecuperável"*.

Se o usuário preferir não gastar uma raid antes de implementar, a alternativa honesta é **rebaixar o contrato**: declarar na 01-spec que os ACs de perks não validados são "melhor esforço, sem linha de base" e aceitar que uma eventual falha ficará ambígua. Não é a recomendação — mas é uma escolha legítima, desde que explícita em vez de descoberta na Fase 4.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-04-02 · A — Gap · 🟡 Importante

**A lista de classes tem três fontes de verdade, não duas — e as três têm cardinalidades diferentes**

**Problema:** o `PA-02-03` (aceito) identificou que `SkillMultipliers.Parse` e `PerksConfig.ClassColors` são "as duas faces da mesma lista" e criou uma checagem bidirecional de boot. **Contei errado: são três.**

A terceira é `PerksCatalog.ByClass` (`PerksCatalog.cs:243-251`), um `Dictionary<string, string[]>` chaveado pelos **mesmos nomes EN** e que define a composição de perks de cada classe — consultado por `GroupsFor(classNameEn)` (`:256`), que alimenta a aba CLASS, o overlay 052 e a notificação de raid-start.

E as cardinalidades **não batem**, o que torna uma checagem ingênua pior que nenhuma:

| Fonte | Chaves | Inclui `Naked`? |
|---|---|---|
| `SkillMultipliers.Parse` (novo) | 7 | ✅ sim |
| `PerksConfig.ClassColors` | 7 (`:313, 365, 421, 465, 548, 633, 638`) | ✅ sim |
| `PerksCatalog.ByClass` | **6** (`:245-250`) | ❌ **não** — e está **correto**: o Peladão não tem perks |

**Por que importa:** dois efeitos, ambos ruins. (1) A checagem do `PA-02-03`, como especificada, **não cobre** a terceira lista: acrescentar uma classe ao `Parse` e ao `ClassColors` e esquecer o `ByClass` produz uma classe com perks ativos no gameplay e **aba CLASS vazia** — sem erro nenhum. (2) Se alguém "melhorar" a checagem para exigir as três iguais, ela vai **acusar `Naked` falsamente** em todo boot, e o ruído faz o alarme ser ignorado — que é como uma checagem de consistência morre.

**Sugestão:** estender a checagem do `PA-02-03` para as três listas, com a assimetria do `Naked` **codificada e explicada**, não descoberta:

```csharp
// ref: PA-04-02 — TRÊS listas, não duas. E `Naked` é legitimamente ausente da terceira: o Peladão tem
// identidade (cor/ícone) mas nenhum perk, então não tem composição em PerksCatalog.ByClass.
foreach (SkillMultipliers.EClassId id in Enum.GetValues(typeof(SkillMultipliers.EClassId)))
{
    if (id == SkillMultipliers.EClassId.None) continue;

    var name = SkillMultipliers.NameOf(id);
    if (!ClassColors.ContainsKey(name!))
        Plugin.Log?.LogError($"[CustomClasses] (PA-04-02) EClassId.{id} sem entrada em ClassColors — cor do F12 nunca se aplica.");

    if (id != SkillMultipliers.EClassId.Naked && PerksCatalog.GroupsFor(name) == null)
        Plugin.Log?.LogError($"[CustomClasses] (PA-04-02) EClassId.{id} sem composição em PerksCatalog.ByClass — aba CLASS vazia.");
}
```

Isso também **simplifica** a versão do `PA-02-03`: iterando o enum (a lista canônica) contra as outras duas, some o laço inverso e some a necessidade do `Parse(k, warnUnknown: false)` num dos sentidos. Atualizar a §5.1 para descrever **três** faces e nomear `EClassId` como o eixo canônico.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-04-03 · A — Gap · 🟡 Importante

**A invalidação de cache do `PA-01-03` inverte camadas: `SkillMultipliers` passa a conhecer um patch de UI e o overlay de diagnóstico**

**Problema:** a correção do `PA-01-03`/`AUD-01-07d` ficou assim na §5.7:

> *"`SkillMultipliers.Apply()` e `SkillMultipliers.Reset()` … passam a chamar `SkillPanelPatch.ClearTooltipCache()` e `PerkDiagnostics.ClearGroupCache()`"*

Funciona, e a correção é necessária. Mas a **forma** faz o componente mais baixo do mod (o cache de dados da classe, que não sabe nada de UI) depender de um **patch Harmony de tela de Skills** e do **overlay de diagnóstico**. Consequências práticas: um terceiro cache no futuro exige lembrar de fiar aqui, e o esquecimento é silencioso (tooltip velho, lista de perks velha); e `SkillMultipliers`, que hoje é testável e independente, passa a arrastar dois tipos de UI.

**Por que importa:** o mod **já tem o padrão certo para isto**, criado no item 067: `PerksConfig.ClassColorsChanged` é um `event Action` que `MenuClassIdentityPatch` (`Plugin.cs:88`) e `SkillsClassTabPatch` (`:30`) assinam, sem que `PerksConfig` conheça nenhum dos dois. A correção do `PA-01-03` reinventa a fiação na direção oposta à convenção já estabelecida no próprio arquivo vizinho.

**Sugestão:** espelhar o padrão do 067:

```csharp
// modded/Client/SkillMultipliers.cs
/// <summary>ref: PA-01-03 · PA-04-03 — disparado sempre que a classe/idioma resolvidos mudam (Apply/Reset).
/// Quem cacheia algo derivado da classe assina isto; SkillMultipliers não conhece nenhum consumidor.
/// Molde: PerksConfig.ClassColorsChanged (item 067).</summary>
internal static event Action? ClassChanged;
```

Disparado no fim de `Apply()` e de `Reset()`. Os dois caches assinam **uma vez no `Plugin.Awake`** (`SkillMultipliers.ClassChanged += SkillPanelPatch.ClearTooltipCache;` e `+= PerkDiagnostics.ClearGroupCache;`) e **nunca desassinam** — são estáticos de vida-do-plugin, exatamente como o `ClassColorsChanged`. Registrar essa decisão na §7 para não virar achado de AP-01 num code-review futuro: *"assinatura estática↔estática, uma vez no Awake, sem unsubscribe — mesmo contrato do `ClassColorsChanged`"*.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-04-04 · B — Edge Case · 🟡 Importante

**`ClassIdentities.Local()` monta uma `Identity` sem preencher o `ClassId` novo**

**Problema:** a §5.2 acrescenta `internal SkillMultipliers.EClassId ClassId` à `Identity` e manda preenchê-la **em `TryFetch`** (o caminho do mapa de peers). Mas há um **segundo** construtor de `Identity` que a spec não menciona: `ClassIdentities.Local()` (`ClassIdentities.cs:144-159`), que monta uma `Identity` a partir do `SkillMultipliers` para o fallback local — e que, seguindo a spec ao pé da letra, deixaria `ClassId` no default `None`.

Hoje isso é **latente, não um bug**: os consumidores de `Local()` (`ChatSpecialIconPatch:52`, `PartyPlayerItemPatch:75`) usam só `NameEn`, `IconFile`, `NameColor`, `DisplayName` e `Description` — nenhum lê `ClassId`.

**Por que importa:** é um objeto meio-inicializado esperando um consumidor. O primeiro código que fizer `identity.ClassId` sem saber de onde a `Identity` veio recebe `None` silenciosamente — que é o valor de "vanilla/desconhecido". Um perk que devesse disparar simplesmente não dispara, sem erro. E o custo de fechar agora é uma linha.

**Sugestão:** preencher no `Local()`:

```csharp
// ClassIdentities.Local() — ref: PA-04-04: TODO construtor de Identity preenche ClassId, senão o campo
// mente com o valor de "vanilla" e o consumidor errado nunca descobre.
ClassId = SkillMultipliers.LocalClassId,
```

E acrescentar ao §8: *"conferir que **todos** os pontos que constroem `ClassIdentities.Identity` preenchem `ClassId` — hoje são dois (`TryFetch` e `Local`)"*. Alternativa mais forte, se quiser garantia estrutural: transformar `ClassId` em propriedade derivada (`internal EClassId ClassId => SkillMultipliers.Parse(NameEn, warnUnknown: false);`) — mas isso reintroduz uma comparação de string por acesso e o acesso é per-passo no hot path. **Descartada** pelo mesmo motivo que o `AUD-01-02` existe; o campo preenchido nos dois lugares é o caminho certo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-04-05 · A — Gap · 🟢 Menor

**A assimetria deliberada `IsLocalClass` × `LocalClassId` fica sem documentação**

**Problema:** depois da migração, dois acessores vizinhos se comportam de forma diferente de propósito:

- `IsLocalClass(EClassId)` chama `EnsureLoaded()` — com o comentário anti-remoção do `PA-02-08`;
- `LocalClassId` (a propriedade) **não** chama, e é o que `ClassIdOf` usa no caminho de `IsYourPlayer` — deliberadamente, porque `ClassIdOf` roda por passo de cada player/bot e um `EnsureLoaded()` ali poderia disparar o GET síncrono que o B14 tirou do hot path (`ClassIdentities.cs:131-135`).

A spec não registra que a diferença é intencional. O `PA-02-08` documentou **um** dos lados.

**Por que importa:** a próxima pessoa que ler os dois lado a lado vai enxergar inconsistência e "consertar" — em qualquer direção. Acrescentar `EnsureLoaded()` ao `LocalClassId` reabre o freeze do B14; remover do `IsLocalClass` mata o carregamento preguiçoso. Ambos silenciosos.

**Sugestão:** comentário no `LocalClassId`, espelhando o do `PA-02-08`:

```csharp
/// <summary>ref: AUD-01-02 — id da classe local, CRU. ⚠️ NÃO chama EnsureLoaded, ao contrário do
/// IsLocalClass: este acessor é usado pelo ClassIdOf, que roda a cada passo de cada player/bot
/// (BotEventHandler.PlaySound), e um GET síncrono ali seria freeze no meio da raid — foi exatamente
/// o que o achado 4 do code-review B14 tirou do hot path (ClassIdentities.cs:131-135).</summary>
internal static EClassId LocalClassId => _classId;
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-04-06 · A — Gap · 🟢 Menor

**O AC de fumaça da §5.9 cobre 4 dos 5 patches novos**

**Problema:** o `PA-03-05` criou um AC de fumaça para provar que os cinco `Enable()` novos estão registrados — o cenário cujo esquecimento **não** gera erro de compilação. Ele nomeia explicitamente `ShootApplyPatch`, `TotalErgoPatch`, `ClassDamagePatch` e `FirearmSyncPatch`. **`ShootCapturePatch` não é mencionado.**

Ele **é** coberto, mas por acidente: se o `ShootCapturePatch` faltar, `ShootRecoilState.StrBefore` fica sempre `NaN`, o `ShootApplyPatch` sai no early-return e o overlay 052 mostra `Recoil str: -` em vez de um valor. O mesmo sintoma de "o `ShootApplyPatch` faltou".

**Por que importa:** um AC que cobre por acidente não ensina o validador a distinguir. Os dois sintomas são idênticos e as causas são opostas — e como os dois patches vivem no mesmo arquivo e no mesmo bloco de registro, esquecer **um** dos dois é o erro mais provável de todos.

**Sugestão:** completar o AC da §5.9: *"`Recoil str` mostrando `a→b` com **dois números** prova que os **dois** patches de `Shoot` estão registrados: o `ShootCapturePatch` fornece o valor `antes` e o `ShootApplyPatch` o `depois`. Um traço (`-`) significa que **um dos dois** faltou — conferir os dois no `Plugin.Awake`, não só o Apply."*

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

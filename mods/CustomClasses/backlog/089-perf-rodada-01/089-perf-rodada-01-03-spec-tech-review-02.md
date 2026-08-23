# 089 — perf — Rodada 01 de otimização · Review Técnica 02

**Mod:** CustomClasses
**Spec técnica revisada:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-08-23

> Segunda rodada de análise crítica, sobre a spec **já corrigida** pela [review 01](089-perf-rodada-01-03-spec-tech-review-01.md). Não é conferência de aplicação — é revisão nova, com ângulos que a primeira não cobriu. Cada ponto recebe um ID `PA-02-MM`.

## Resumo

> 🔴 Bloqueadores: 2 · 🟡 Importantes: 3 · 🟢 Menores: 4 · ✅ Resolvidos: 0 · Total: 9

**Memória consultada:** snapshot de 2026-08-03 · pendências que afetam: 🔴 P-10.1, 🔴 P-16.1 (já endereçadas por `PA-01-04` — não voltam aqui).

**Verificação dos 10 pontos da review 01:** todos aplicados na spec. ✅ `PA-01-01` (Shoot 4→2 com `First`/`Last` preservados) · ✅ `PA-01-02` (clamp) · ✅ `PA-01-03` (`className` na chave + invalidação) · ✅ `PA-01-04` (nota de linha de base + raid de baseline) · ✅ `PA-01-05` (cadeia provada e registrada) · ✅ `PA-01-06` (fronteira do ICM + `internal`) · ✅ `PA-01-07` (`AUD-01-07b` dropado e registrado como ❌ no relatório) · ✅ `PA-01-08` (bump — **mas incompleto, ver `PA-02-02`**) · ✅ `PA-01-09` (extração explícita) · ✅ `PA-01-10` (condição de saída). Nenhum é reaberto.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Erro de Lógica | 🔴 Bloqueador | Consolidar patches destrói o **isolamento de falha**: um branch que lança leva os irmãos junto — inclusive o piso de recuo | Pendente |
| PA-02-02 | A — Gap | 🔴 Bloqueador | A versão vive em **4 lugares**, não 2 — e o gate de validação do `PA-01-08` lê justamente os dois que a spec não cita | Pendente |
| PA-02-03 | A — Gap | 🟡 Importante | `Parse` cria uma **segunda** fonte de verdade para a lista de classes; a primeira (`BindClassColor`) pode divergir em silêncio | Pendente |
| PA-02-04 | B — Edge Case | 🟡 Importante | `_cachedPmv` pode fixar um painel **inativo**: `GameObject.Find` só acha ativos, então hoje o caso se auto-corrige e com cache não | Pendente |
| PA-02-05 | A — Gap | 🟡 Importante | INSTR-2 define os contadores mas não diz **onde** incrementar — e a razão "passou/total" do AC depende da posição exata | Pendente |
| PA-02-06 | A — Gap | 🟢 Menor | A ausência de `[HarmonyPriority]` nos outros 3 alvos consolidados é premissa da spec, mas não está registrada como evidência | Pendente |
| PA-02-07 | A — Gap | 🟢 Menor | `RecoilBranches` é chamado mas a spec nunca diz onde a classe mora | Pendente |
| PA-02-08 | B — Edge Case | 🟢 Menor | `EnsureLoaded()` dentro do gate quente parece removível — sem comentário, alguém "otimiza" e reabre o CR-F5 | Pendente |
| PA-02-09 | A — Gap | 🟢 Menor | A rodada muda o inventário de patches, mas nada manda atualizar o grafo do mod | Pendente |

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

### PA-02-01 · C — Erro de Lógica · 🔴 Bloqueador

**Consolidar patches destrói o isolamento de falha — um branch que lança leva os irmãos junto, inclusive o piso de recuo**

**Problema:** hoje, cada um dos patches que a rodada consolida é uma unidade Harmony independente com o **seu próprio `try/catch`**. Se o corpo do `BulwarkPatch` lançar, o `catch` dele engole a exceção e os outros três patches de `ApplyDamageInfo` **continuam rodando normalmente**. Esse isolamento é estrutural: são métodos separados, embrulhados separadamente pelo Harmony.

A spec consolida cada alvo num método único com **um `try/catch` externo** (§5.6: `try { ApplyMastery(); ApplyPerks(); ApplyFloor(); … } catch { LogError }`). A partir daí, **a primeira exceção aborta todos os branches seguintes**.

O caso perigoso é o do recuo:

```
ApplyMastery(p, ref str);      // se ISTO lançar…
ApplyPerks(p, ref str);        // …não roda
RecoilBranches.ApplyFloor(...) // …e o PISO B15 NÃO É APLICADO
```

**Por que importa:** o piso B15 existe para impedir que o produto maestria × perks passe do limite (Anexo C: Fuzileiro em Adrenalina chegava a ×0.56). Se um branch anterior lançar — e `ApplyMastery` chama `WeaponMastery.SkillForHeld(p.Skills, …)` e lê `skill.Level`, ou seja, toca objetos do EFT que podem ser nulos numa transição de arma —, o tiro sai **sem piso nenhum**. Hoje isso é impossível: o `RecoilFloorApplyPatch` é um patch separado e roda mesmo que o de maestria tenha explodido.

O mesmo padrão vale para os outros três alvos: em `SetAnimatorAndProceduralValues`, se o branch de Adrenalina lançar **depois** de escalar `BuffInfo.ReloadSpeed` mas **antes** de gravar o `__state`, o Postfix não restaura e o campo fica sujo pela raid inteira. Hoje cada patch tem o seu par Prefix/Postfix e a falha é contida.

**Por que a review 01 não pegou:** a review 01 olhou a **ordem** dos branches (PA-01-01) e não o **modo de falha** deles.

**Sugestão:** `try/catch` **por branch**, não um externo, em todos os quatro alvos consolidados. O ganho da consolidação (gate resolvido uma vez) é preservado; o isolamento também:

```csharp
[HarmonyPriority(Priority.Last)]
[PatchPrefix]
private static void Prefix(ProceduralWeaponAnimation __instance, ref float str)
{
    var str0 = ShootRecoilState.StrBefore;
    if (float.IsNaN(str0)) return;

    var p = Singleton<GameWorld>.Instance?.MainPlayer;
    if (p == null) return;                                   // GATE ÚNICO — o ganho do AUD-01-03

    // ref: PA-02-01 — isolamento por branch. Consolidar o GATE não pode consolidar a FALHA:
    // um branch que lança não pode impedir o piso B15 de rodar.
    try { RecoilBranches.ApplyMastery(p, ref str); } catch (Exception ex) { LogOnce("mastery", ex); }
    try { RecoilBranches.ApplyPerks(p, ref str); }   catch (Exception ex) { LogOnce("perks", ex); }
    try { RecoilBranches.ApplyFloor(str0, ref str); } catch (Exception ex) { LogOnce("floor", ex); }

    if (PerkDiag.Enabled) { PerkDiag.RecoilBefore = str0; PerkDiag.RecoilAfter = str; }
}
```

`LogOnce(string branch, Exception ex)` = helper com um `HashSet<string>` de branches já logados, para não inundar o console num hot path (hoje cada patch loga a cada ocorrência — o que já é um risco de flood que a consolidação é a oportunidade de corrigir).

Acrescentar à §7 a linha de risco e ao §8 o item de checklist *"nenhum alvo consolidado tem `try/catch` externo único — cada branch é isolado"*.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-02 · A — Gap · 🔴 Bloqueador

**A versão vive em 4 lugares, não 2 — e o gate de validação do `PA-01-08` lê justamente os dois que a spec não cita**

**Problema:** o `PA-01-08` (aceito) manda bumpar `<Version>` "em **ambos** os csproj". Mas a versão do mod está em **quatro** arquivos:

| # | Arquivo | Valor | Quem lê |
|---|---|---|---|
| 1 | `modded/Client/CustomClasses.Client.csproj:9` | `0.16.8` | build (metadados do assembly) |
| 2 | **`modded/Client/Plugin.cs:13`** — `[BepInPlugin("customclasses.mdj.client", "CustomClasses", "0.16.8")]` | `0.16.8` | **o log de boot do BepInEx** |
| 3 | `modded/Server/CustomClasses.Server.csproj:10` | `0.16.8` | build |
| 4 | **`modded/Server/CustomClassesMetadata.cs:19`** — `new SemanticVersioning.Version("0.16.8")` | `0.16.8` | **o log de boot do SPT.Server** |

A spec cita **só o 1 e o 3**. Os que aparecem em log são o **2** e o **4**.

**Por que importa:** o próprio `PA-01-08` estabeleceu o gate *"confirmar no log de boot que a DLL carregada é a 0.16.9"* — e esse log lê o `BepInPlugin`, não o csproj. Bumpando só os csproj, o gate **falharia sempre**, e o diagnóstico natural ("o launcher reverteu a DLL de novo" — `feedback_server_launcher_sync_builds`) mandaria o usuário caçar um problema que não existe. Pior no sentido inverso: se o launcher **de fato** reverter a build, a versão idêntica no log torna impossível perceber.

**Sugestão:** corrigir o item de checklist do `PA-01-08` para nomear os **quatro** pontos, marcando quais são os observáveis:

> Bumpar `0.16.8` → `0.16.9` em: `Client/CustomClasses.Client.csproj:9`, **`Client/Plugin.cs:13` (`BepInPlugin` — é o que sai no log do BepInEx)**, `Server/CustomClasses.Server.csproj:10` e **`Server/CustomClassesMetadata.cs:19` (é o que sai no log do SPT.Server)**. Os quatro em lockstep, mesmo com o server inalterado.

E acrescentar ao `05-asbuild` a instrução de conferir `grep -rn '0\.16\.8' modded/` vazio após o bump — que é o teste que pega os quatro de uma vez.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-03 · A — Gap · 🟡 Importante

**`Parse` cria uma segunda fonte de verdade para a lista de classes; a primeira pode divergir em silêncio**

**Problema:** a spec declara `SkillMultipliers.Parse` como "fonte única" dos nomes de classe (§5.1). Não é — passa a ser a **segunda**. A primeira já existe: `PerksConfig.BindClassColor(config, secao, "<nome>", "<hex>")`, chamada 7 vezes (`PerksConfig.cs:313, 365, 421, 465, 548, 633, 638`) e que popula `PerksConfig.ClassColors`, o dicionário que o `ClassColorOverride.Resolve(classNameEn)` consulta **por string** — e que a rodada **não** migra para enum (corretamente: é caminho frio de render).

Depois desta rodada, a lista canônica de 7 nomes existe em dois lugares que ninguém obriga a concordar.

**Por que importa:** o cenário de divergência é concreto e silencioso. Alguém acrescenta uma classe nova (o editor web permite): se registrar só o `BindClassColor`, a classe ganha cor customizável no F12 mas **nenhum perk**; se registrar só no `Parse`, ganha perks mas a cor do F12 nunca se aplica. Nos dois casos não há erro — só um comportamento pela metade, exatamente o tipo de bug que sobrevive meses.

Hoje o acoplamento também existe (literais espalhados em 42 call-sites), então **isto não é regressão** — mas a rodada é a oportunidade de fechá-lo, e a spec afirma tê-lo fechado quando não fechou.

**Sugestão:** validação de boot barata, no fim de `PerksConfig.Bind(config)` (caminho frio, roda 1×):

```csharp
// ref: PA-02-03 — Parse e ClassColors têm de conhecer exatamente as MESMAS classes.
// Divergência = classe com cor e sem perk (ou o inverso), sem nenhum erro visível.
foreach (var key in ClassColors.Keys)
{
    if (SkillMultipliers.Parse(key) == SkillMultipliers.EClassId.None)
    {
        Plugin.Log?.LogError($"[CustomClasses] (PA-02-03) classe '{key}' tem cor no F12 mas não existe em EClassId — perks não vão disparar p/ ela.");
    }
}
```

E o inverso (todo membro de `EClassId` exceto `None` tem entrada em `ClassColors`) via `Enum.GetValues`. Acrescentar ao §5.1 a frase: *"`Parse` e `BindClassColor` são as duas faces da mesma lista; a checagem de boot é o que impede a divergência"*, e um item no §8.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-04 · B — Edge Case · 🟡 Importante

**`_cachedPmv` pode fixar um painel inativo — hoje o caso se auto-corrige, com cache não**

**Problema:** o `AUD-01-01` cacheia o `Transform` de `MainMenuPlayerModelView` num estático e só refaz a busca quando `_cachedPmv == null`. A premissa é que o `==` do Unity cobre o objeto destruído — verdade, mas **incompleta**: `GameObject.Find` **só encontra objetos ativos** (documentado no Unity), enquanto uma referência cacheada continua válida para um objeto **desativado**.

Cenário: o Menu-Overhaul desativa o painel numa transição (ir para o inventário e voltar, entrar/sair de raid) e depois **instancia um novo** em vez de reativar o antigo — `PlayerProfileFeaturesPatch.cs:302` renomeia a instância clonada, e nada garante que a antiga seja destruída no mesmo frame. Com o cache, `_cachedPmv` aponta para o painel velho (inativo, não destruído) e `Find("BottomField/NicknameText")` devolve o TMP **dele**: escreveríamos o ícone e a linha de classe num painel invisível, e o painel visível ficaria sem identidade.

Hoje isso não acontece porque `GameObject.Find` roda todo frame e **só enxerga o ativo**.

**Por que importa:** é uma regressão visual intermitente (a identidade "às vezes some do menu") e do tipo que não reproduz sob demanda — o pior custo/benefício de diagnóstico.

**Sugestão:** validar a atividade antes de confiar no cache — uma linha, mantendo todo o ganho:

```csharp
// ref: AUD-01-01 · PA-02-04 — GameObject.Find só acha ATIVOS; uma referência cacheada sobrevive à
// desativação. Sem este check, um painel velho desativado sequestraria a identidade do painel novo.
if (_cachedPmv == null || !_cachedPmv.gameObject.activeInHierarchy)
{
    _cachedPmv = GameObject.Find("MainMenuPlayerModelView")?.transform;
    finds++;
}
```

Acrescentar ao AC de não-regressão da 01-spec: *"entrar no inventário e voltar ao menu principal, e sair de uma raid para o menu — a identidade da classe continua no painel **visível** nas duas transições"*.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-05 · A — Gap · 🟡 Importante

**INSTR-2 define os contadores mas não diz onde incrementar — e a razão "passou/total" do AC depende da posição exata**

**Problema:** a §5.8 declara `PerfCount` com 10 campos e o formato do dump, e o §8 diz "INSTR-1/2/3 nos pontos previstos". **Quais pontos?** Os nomes sugerem, mas a 01-spec cobra um critério que depende da **posição** dos incrementos:

> *"a fração de chamadas que **passa** do gate permanece ~1/N (o gate não afrouxou)"*

Para essa fração existir, `MoveSpeedCalls` tem de ser incrementado **na primeira linha** de `ClassMoveSpeed.Apply` (antes do `ReferenceEquals`) e `MoveSpeedPassed` **depois** do gate. Se ambos forem incrementados depois, a razão é sempre 1 e o AC não mede nada. O mesmo vale para `StepAiCalls/Passed` (antes/depois do `IsAI`) e `RolloffCalls/Passed`.

E há uma armadilha extra: os incrementos ficam dentro de `if (PerkDiag.Enabled)`. Se o usuário ligar o diagnóstico **no meio** de uma janela de 60 s, o primeiro dump sai com uma amostra parcial e a razão fica distorcida. Não é erro, mas precisa estar dito para ninguém interpretar o primeiro dump como medição.

**Por que importa:** instrumentação mal posicionada produz um número que **parece** válido. É pior que não medir — e a rodada inteira já abriu mão da medição de baseline (decisão do usuário), então o pouco que se mede tem de estar certo.

**Sugestão:** acrescentar à §5.8 uma tabela explícita:

| Contador | Arquivo | Posição |
|---|---|---|
| `MoveSpeedCalls` / `MoveSpeedPassed` | `ClassMovementPatches.cs` → `ClassMoveSpeed.Apply` | **1ª linha do try** / logo após o `ReferenceEquals(ctx, p.MovementContext)` |
| `StepAiCalls` / `StepAiPassed` | `ClassSoundPatches.cs` → `AiSoundPatch.Prefix` | após o `type != step` / após `emitterClass is null` |
| `RolloffCalls` / `RolloffPassed` | `ClassSoundPatches.cs` → `SoundRadiusPatch.Postfix` | 1ª linha / após `emitterClass is null` |
| `DamageCalls` / `DamageGates` | patch consolidado de `ApplyDamageInfo` | 1× por invocação / 1× por resolução de gate |
| `ShootCalls` / `ShootGates` | `ShootCapturePatch` + `ShootApplyPatch` | 1× por invocação no Capture / 1 por patch que resolve gate |

Mais a nota: *"o primeiro dump após ligar o diagnóstico é parcial — descartar e usar do segundo em diante"*.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-06 · A — Gap · 🟢 Menor

**A ausência de `[HarmonyPriority]` nos outros 3 alvos consolidados é premissa, mas não está registrada como evidência**

**Problema:** depois do `PA-01-01`, a spec sabe que consolidar patches com prioridade explícita é perigoso. Mas ela consolida `ApplyDamageInfo` (4→2), `SetAnimatorAndProceduralValues` (3→2) e `TotalErgonomics` (2→1) **sem dizer** que conferiu se algum deles tem prioridade.

**Verificação feita nesta review** — `grep -rn "HarmonyPriority" modded/Client/` devolve exatamente 3 usos em patches:
- `RecoilFloorPatch.cs:41` — `Priority.First` (tratado pelo `PA-01-01`)
- `RecoilFloorPatch.cs:68` — `Priority.Last` (tratado pelo `PA-01-01`)
- `WeaponMasteryPatches.cs:116` — `Priority.High` (absorvido pela ordem interna do `ShootApplyPatch`)
- (`ClassMedicPatches.cs:186` — `Priority.First`, mas em `ObservedMedsControllerClass.method_5`, que **não** é alvo desta rodada)

Ou seja: **os outros três alvos não têm prioridade explícita**, e a consolidação deles é segura. A conclusão está certa; falta o registro.

**Por que importa:** sem a evidência escrita, a próxima pessoa que mexer aqui repete a análise — ou, pior, assume que "consolidar é seguro" como regra geral, que é justamente o que o `PA-01-01` provou ser falso.

**Sugestão:** acrescentar à §2, abaixo da tabela: *"Verificado (review 02): `grep HarmonyPriority` no client devolve prioridade explícita apenas em `RecoilFloorPatch.cs:41/:68` e `WeaponMasteryPatches.cs:116` — todos em `PWA.Shoot`, tratados pelo PA-01-01 — e em `ClassMedicPatches.cs:186` (alvo fora desta rodada). `ApplyDamageInfo`, `SetAnimatorAndProceduralValues` e `TotalErgonomics` não têm prioridade explícita: consolidá-los não move nenhuma fronteira."*

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-07 · A — Gap · 🟢 Menor

**`RecoilBranches` é chamado mas a spec nunca diz onde a classe mora**

**Problema:** a §5.6 chama `RecoilBranches.ApplyMastery`, `.ApplyPerks` e `.ApplyFloor`, e a §4 lista `ClassWeaponPatches.cs`, `RecoilFloorPatch.cs` e `WeaponMasteryPatches.cs` como modificados — mas nenhuma linha diz em **qual** deles a classe `RecoilBranches` é declarada, nem se os três branches vivem juntos ou cada um no seu arquivo de origem.

**Por que importa:** os três branches vêm de três arquivos diferentes. Sem a decisão tomada, a implementação pode espalhar (`ApplyMastery` em `WeaponMasteryPatches.cs`, `ApplyFloor` em `RecoilFloorPatch.cs`) e aí `RecoilBranches` vira uma classe `partial` ou três classes — e o objetivo declarado do `AUD-01-03` (ordem legível num lugar só) se perde.

**Sugestão:** decidir na spec: **`RecoilBranches` é uma classe estática única em `ClassWeaponPatches.cs`**, logo acima de `ShootCapturePatch`/`ShootApplyPatch`, com os três métodos movidos para lá (cada um mantendo um comentário `// ref: origem WeaponMasteryPatches.cs:118-145` etc.). `RecoilFloorPatch.cs` fica **apenas** com o XMLdoc histórico do B15 apontando para o novo lugar, ou é removido — decidir também isso (a favor de manter o arquivo com o doc: o B15 tem contexto de balance que vale preservar onde está).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-08 · B — Edge Case · 🟢 Menor

**`EnsureLoaded()` dentro do gate quente parece removível — sem comentário, alguém "otimiza" e reabre o CR-F5**

**Problema:** o novo `IsLocalClass(EClassId)` mantém `EnsureLoaded()` na primeira linha (§5.1). Depois da migração, isso vai parecer sobra: "o `_classId` já foi resolvido no `Apply`, por que checar de novo a cada frame?". A resposta é que `EnsureLoaded()` é o que dispara o **fetch preguiçoso** quando nenhum `Prefetch` rodou (menu, hideout, primeira raid depois de um restart do server).

E há um perigo específico já documentado no código: `CalmSightsPatch.cs:51-53` avisa que o `EnsureLoaded()` dentro do `IsLocalClass` pode fazer **um GET HTTP síncrono** com cache frio, e é por isso que ali o gate de identidade (`ReferenceEquals`) vem **antes** do gate de classe (achado CR-F5).

**Por que importa:** remover o `EnsureLoaded()` quebraria o carregamento preguiçoso em silêncio (classe fica `None`, nenhum perk dispara, nenhum erro). Reordenar os gates de um patch reabriria o CR-F5 (freeze no meio da raid). Nenhum dos dois é óbvio olhando só o código novo.

**Sugestão:** comentário explícito no `IsLocalClass(EClassId)`:

```csharp
public static bool IsLocalClass(EClassId id)
{
    // ⚠️ NÃO remover o EnsureLoaded: é o fetch PREGUIÇOSO para quando nenhum Prefetch rodou (menu, hideout,
    // 1ª raid pós-restart do server). Com cache frio ele faz um GET HTTP SÍNCRONO — por isso todo patch que
    // roda para bots/peers coloca o gate de INSTÂNCIA antes deste (ref: CalmSightsPatch CR-F5).
    EnsureLoaded();
    return id != EClassId.None && _classId == id;
}
```

E um item no §8: *"conferir que nenhum patch passou a chamar `IsLocalClass` **antes** do seu gate de instância durante a migração"* — a migração mecânica dos 42 call-sites é exatamente onde essa ordem pode ser invertida por descuido.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-02-09 · A — Gap · 🟢 Menor

**A rodada muda o inventário de patches, mas nada manda atualizar o grafo do mod**

**Problema:** existe um grafo de código do mod em `references/graphs/mods/CustomClasses/`, usado pela skill `graph-code-navigation` para achar alvos e chamadores. Esta rodada **remove** classes de patch (`RecoilFloorCapturePatch`, `RecoilFloorApplyPatch`, `WeaponMasteryRecoilPatch`, `WeaponMasteryErgoPatch`, `ShootRecoilPatch`, `HeavyWeaponErgoPatch`, `BulwarkPatch`, `ExecutionMeleePatch`, `AdrenalineTriggerPatch`, `LocalHitTypePatch`, `ReloadSpeedPatch`, `ShotgunReloadPatch`, `HolsterDrawResetPatch`) e **cria** outras (`ShootCapturePatch`, `ShootApplyPatch`, `RecoilBranches`, os consolidados dos outros 3 alvos), além de trocar a assinatura de `IsLocalClass`/`IsClass`.

O checklist da §8 não menciona regenerar o grafo, e o `/optimize-mod-performance` só sugere `/update-mod-graph` na **Fase 4**.

**Por que importa:** um grafo desatualizado é pior que nenhum — a próxima sessão que consultar "quem chama `IsLocalClass`" recebe 42 call-sites que não existem mais, e a skill `graph-code-navigation` prescreve "grafo aponta, leitura prova" justamente porque o grafo pode mentir. Com uma refatoração desta amplitude, ele mente muito.

**Sugestão:** acrescentar ao §8, no bloco de gates de Fase 4: *"`/update-mod-graph CustomClasses` — a rodada remove ~13 classes de patch e cria 4; o grafo fica inconsistente até ser regenerado"*. E mencionar no `05-asbuild` a lista de classes removidas/criadas, para quem for ler o histórico sem regenerar nada.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

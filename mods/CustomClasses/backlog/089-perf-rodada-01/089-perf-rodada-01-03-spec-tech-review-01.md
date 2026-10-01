# 089 — perf — Rodada 01 de otimização · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [089-perf-rodada-01-02-spec-tech.md](089-perf-rodada-01-02-spec-tech.md)
**Data:** 2026-08-23

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 3 · 🟡 Importantes: 4 · 🟢 Menores: 3 · **✅ Resolvidos: 10** · Total: 10
>
> **Todos os 10 pontos aceitos pelo usuário em 2026-08-23** e aplicados na spec técnica / spec funcional. Resolução ponto a ponto na seção [Resolução](#resolução) ao fim deste arquivo. Os títulos e o impacto de cada ponto são preservados como registrados (review é imutável); o que muda é o Status.

**Memória consultada:** snapshot de `mods/CustomClasses/memory/sessions.md` (última sessão registrada: 2026-08-03) · **pendências que afetam esta tarefa: 🔴 P-10.1 e 🔴 P-16.1** — ver `PA-01-04`.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | Consolidar `Shoot` em 1 patch destrói a garantia `Priority.Last` do piso de recuo contra mods externos | ✅ Resolvido 2026-08-23 |
| PA-01-02 | C — Erro de Lógica | 🔴 Bloqueador | `Quantize` estoura o byte: canal 1.0 vira 0 (branco → preto) | ✅ Resolvido 2026-08-23 |
| PA-01-03 | C — Erro de Lógica | 🔴 Bloqueador | Chave do cache de tooltip omite `className` → tooltip errado após troca de perfil/idioma | ✅ Resolvido 2026-08-23 |
| PA-01-04 | A — Gap | 🟡 Importante | O contrato de não-regressão pressupõe comportamento atual conhecido — mas P-10.1/P-16.1 dizem que grande parte nunca foi validada in-game | ✅ Resolvido 2026-08-23 |
| PA-01-05 | B — Edge Case | 🟡 Importante | `AUD-01-01`: o bail por `IsPresent` também desliga o `FixTopGlow` — confirmar se o alvo é objeto do EFT ou do Menu-Overhaul | ✅ Resolvido 2026-08-23 |
| PA-01-06 | A — Gap | 🟡 Importante | `AUD-01-02` toca a fachada pública consumida pelo ICM por reflexão — a spec não declara o contrato preservado | ✅ Resolvido 2026-08-23 |
| PA-01-07 | C — Erro de Lógica | 🟡 Importante | `AUD-01-07b` é micro-otimização de código frio: ganho de ~30 µs por janela, contra a regra §8 da própria skill | ✅ Resolvido 2026-08-23 |
| PA-01-08 | A — Gap | 🟢 Menor | Falta o bump de SemVer no checklist (regra do repo para toda compilação) | ✅ Resolvido 2026-08-23 |
| PA-01-09 | A — Gap | 🟢 Menor | `BuildTinted` é citado como "corpo atual" mas não existe — a extração precisa ser explícita | ✅ Resolvido 2026-08-23 |
| PA-01-10 | B — Edge Case | 🟢 Menor | `PerfDumpLoop` é um `while (true)` perpétuo introduzido numa rodada de performance | ✅ Resolvido 2026-08-23 |

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

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador

**Consolidar `PWA.Shoot` em 1 patch destrói a garantia `Priority.Last` do piso de recuo contra mods externos**

**Problema:** a spec (§2 e §5.6) propõe trocar os 4 Prefixes de `ProceduralWeaponAnimation.Shoot` por **um** patch, argumentando que "a ordem vira sequência de statements". Isso é verdade para a ordem **entre os patches deste mod**, mas ignora a razão de existir do `[HarmonyPriority(Priority.Last)]` em `RecoilFloorPatch.cs:68`: `Priority.Last` ordena o prefixo **depois dos prefixos de todos os outros mods** no mesmo método, não só depois dos nossos. O piso B15 existe justamente para clampar o **produto final** de multiplicadores de recuo. Consolidado num único patch de prioridade `Normal`, o piso passa a ser aplicado **antes** de qualquer multiplicador de um mod externo que rode em prioridade mais baixa — e o RealRecoil/Realism, que o usuário roda, patcha recuo.

Simétrico e igualmente perdido: `RecoilFloorCapturePatch` é `Priority.First` (`RecoilFloorPatch.cs:41`) — captura o `str` **antes** de qualquer mod tocá-lo. Consolidado em `Normal`, o "original" capturado já viria multiplicado por um mod externo de prioridade mais alta, e o piso passaria a ser calculado sobre uma base errada.

**Por que importa:** o `AUD-01-03` é justificado no relatório como ganho pequeno de CPU + ganho estrutural. Trocar uma garantia de correção de composição (que hoje funciona) por legibilidade é o oposto do contrato de não-regressão desta rodada. O sintoma seria silencioso: o recuo simplesmente sai diferente quando o RealRecoil está ativo, e o overlay 052 (que só mede a nossa cadeia) **não pegaria**.

**Sugestão:** consolidar `Shoot` **4 → 2**, não 4 → 1:
- **Patch A** (`[HarmonyPriority(Priority.First)]`): captura o `str` original em `RecoilFloorCapturePatch.StrBefore` (o campo estático **fica**) — gate resolvido aqui.
- **Patch B** (`[HarmonyPriority(Priority.Last)]`): maestria → perks → piso → diag, em sequência explícita no corpo, com gate resolvido uma vez.

Isso já entrega o ganho real (4 gates → 2, ordem interna explícita, some a coordenação por 3 prioridades) e **preserva as duas garantias de fronteira**. O campo estático `StrBefore` continua sendo necessário porque dois patches distintos não compartilham `__state` — corrigir a §5.6 e a §2, e remover da §7 a afirmação de que o estático "some". Atualizar a meta do `AUD-01-03` na 01-spec de "4 → 1" para "**4 → 2**" em `Shoot` (as demais consolidações não têm prioridade explícita e seguem como estão).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-02 · C — Erro de Lógica · 🔴 Bloqueador


**`Quantize` estoura o byte: canal em 1.0 vira 0 — branco vira preto**

**Problema:** o stub da §5.3:

```csharp
static byte Q(float v) => (byte)(Mathf.RoundToInt(Mathf.Clamp01(v) * 255f / ColorQuantum) * ColorQuantum);
```

Com `v = 1.0` e `ColorQuantum = 8`: `1.0 * 255 / 8 = 31.875` → `RoundToInt` = `32` → `32 * 8 = 256` → `(byte)256` em contexto unchecked (o default do C#) = **0**.

**Por que importa:** o topo do gradiente do ícone é `Color.Lerp(baseColor, Color.white, IconGradientLighten)` (`ClassIdentityView.cs:98`) — quanto mais claro, mais perto de 1.0 em cada canal. Uma classe de cor clara teria canais estourando para 0: o ícone renderiza com a cor **invertida** em vez de clareada. Pior, é intermitente por canal (só o canal que passa de 251), o que produziria tons aleatórios difíceis de diagnosticar. E cairia direto no critério de aceite visual do `AUD-01-08`.

**Por que a auditoria não pegaria:** é um bug introduzido **por esta spec**, não pelo código existente.

**Sugestão:** clampar depois da quantização, não antes:

```csharp
static byte Q(float v)
{
    var q = Mathf.RoundToInt(Mathf.Clamp01(v) * 255f / ColorQuantum) * ColorQuantum;
    return (byte)Mathf.Min(q, 255);   // ref: PA-01-02 — 1.0 → 256 estouraria o byte para 0
}
```

Acrescentar ao checklist da §8 um teste manual explícito: **abrir a aba CLASS de uma classe de cor clara** (ex.: Saqueador `#c4ad45`, cujo topo do gradiente passa de 251 no canal R) e confirmar que o brasão não inverte.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · C — Erro de Lógica · 🔴 Bloqueador

**Chave do cache de tooltip (`AUD-01-07c`) omite `className` → tooltip errado após troca de perfil ou idioma**

**Problema:** a §5.7 propõe `Dictionary<(ESkillId, float), string>` como cache de `MultiplierFormat.TooltipText(f, SkillMultipliers.ClassName)`. A assinatura real é `TooltipText(float factor, string? className)` (`MultiplierFormat.cs:55`) — o texto **depende do nome da classe**, que a chave ignora.

`SkillMultipliers.ClassName` muda em dois cenários reais:
1. **Troca de perfil sem reiniciar o cliente** — `Reset()` + refetch (`PartyInfoPanelPrefetchPatch`, `Prefetch()` no raid-start). Perfil novo, classe nova, tooltip antigo em cache.
2. **Idioma do EFT** — `ClassName` resolve por `GameLocale.IsPortuguese` (`SkillMultipliers.cs:27-30`); trocar o idioma no menu muda o texto sem mudar o fator.

**Por que importa:** o tooltip passaria a afirmar a classe errada na tela de Skills — um bug de correção introduzido para economizar uma alocação de string por linha durante o scroll, que é o item de menor ganho da rodada inteira.

**Sugestão:** duas opções, ambas aceitáveis:
- **(a) — recomendada, mais simples:** incluir a classe na chave — `Dictionary<(ESkillId, float, string?), string>` — e **limpar o cache** em `SkillMultipliers.Reset()`/`Apply()` (o mesmo ponto que já invalida tudo o mais). Uma linha de `Clear()` num caminho frio.
- **(b):** manter a chave `(ESkillId, float)` e guardar junto o `className` usado; em miss de classe, limpar tudo e reconstruir.

Se nenhuma parecer valer o esforço, a resposta honesta é **dropar o `AUD-01-07c`** — é uma preventiva 💡 cujo ganho (uma string por linha visível por frame de scroll, em tela de menu) não justifica introduzir uma chave de cache com invalidação.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-04 · A — Gap · 🟡 Importante

**O contrato de não-regressão pressupõe comportamento atual conhecido — a memória diz que grande parte nunca foi validada in-game**

**Problema:** a 01-spec declara que "o contrato funcional **é o comportamento atual**" e lista critérios do tipo "o perk X continua funcionando". Mas a memória do mod tem duas pendências **🔴 abertas** que dizem o contrário:

- **P-10.1** (aberta 2026-06-23) — "Validação in-game 050.0–050.4 (~21 efeitos)" ainda pendente. São exatamente os perks que esta rodada refatora (Bulwark, Pack Mule, Adrenaline, Ghost Step, Bunker, Sharpshooter, Iron Lungs, Execution, Rattled, Cool Under Fire…).
- **P-16.1** (aberta 2026-07-15) — validar in-game os 2 fixes de movimento (v0.2.4) e os perks 072, "em cliente Fika, não só solo".

**Por que importa:** se um perk **já está quebrado hoje** e o `/code-review` desta rodada encontrar que ele não funciona, não haverá como distinguir regressão introduzida de defeito pré-existente. Pior: se ele estiver quebrado e a refatoração o **consertar** por acidente, isso conta como mudança perceptível não declarada. O contrato de não-regressão precisa de uma linha de base conhecida — e ela não existe para boa parte da matriz de perks.

**Sugestão:** não bloquear a implementação, mas **registrar a limitação explicitamente** e ajustar o que a validação pode afirmar:
1. Acrescentar à 01-spec, na seção de critérios A, a nota: *"Onde P-10.1/P-16.1 indicam que o comportamento atual nunca foi validado, o critério é **'idêntico ao build anterior'**, não **'funciona'** — um perk que já estava inerte deve continuar inerte, e isso não é regressão desta rodada."*
2. Na Fase 4, rodar a matriz de perks **na build anterior primeiro** (a DLL atual já está instalada) anotando o que funciona, e só depois instalar a nova. Custa uma raid e transforma "não sei" em linha de base.
3. Marcar no `05-asbuild` quais ACs foram verificados contra base conhecida e quais contra base desconhecida.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-05 · B — Edge Case · 🟡 Importante

**O bail por `IsPresent` (`AUD-01-01`) também desliga o `FixTopGlow` — confirmar de quem é o objeto alvo**

**Problema:** a §5.4 faz `if (!MenuOverhaulBridge.IsPresent) yield break;` no topo de `ApplyToMenu`. Isso pula **todo** o corpo, inclusive `FixTopGlow(baseColor)` (`MenuClassIdentityPatch.cs:169-192`), que procura `"Environment UI" → Common/Glow Canvas → TopGlowPve`. A spec assume que esse objeto pertence ao Menu-Overhaul, mas **não prova**: `Environment UI` tem cara de objeto **nativo do EFT**, e o XMLdoc do próprio `FixTopGlow` descreve o `TopGlowPve` como "SPRITE azul/ciano (tema PvE)" — linguagem de asset do jogo, não de mod.

**Por que importa:** se o glow for nativo do EFT, hoje ele é tingido com a cor da classe **mesmo sem o Menu-Overhaul**, e o bail removeria esse efeito visual para quem não usa o MO — uma **mudança perceptível não declarada**, exatamente o que a 01-spec proíbe.

O argumento a favor do bail continua plausível (o `baseColor` vem de `ClassIdentityView.ResolveColor(SkillMultipliers.NameColor, …)`, que não depende do MO; mas `FixTopGlow` só é alcançado **depois** de `nick` ser encontrado, e `nick` é do MO — ou seja, **hoje já não roda sem o MO**). Se isso se confirmar, o bail é seguro e o ponto fecha sozinho.

**Por que importa mesmo assim:** a spec afirma o resultado sem mostrar o raciocínio, e é a diferença entre "otimização segura" e "remoção silenciosa de feature".

**Sugestão:** antes de implementar, confirmar por leitura que `FixTopGlow` é **inalcançável** hoje sem o Menu-Overhaul — o caminho é: `ApplyToMenu` só chega em `FixTopGlow` após o `for` encontrar `nick`; `nick` é `MainMenuPlayerModelView/BottomField/NicknameText`, criado pelo MO (`PlayerProfileFeaturesPatch`). Se confirmado, **registrar essa cadeia como comentário no código** junto do `yield break` (`// ref: AUD-01-01 — sem MO, nick nunca resolve → FixTopGlow já era inalcançável hoje`) e acrescentar o AC "sem Menu-Overhaul, o menu segue idêntico ao build anterior" (já está na 01-spec, mas sem esta justificativa). Se **não** se confirmar, mover o `FixTopGlow` para fora do bail.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-06 · A — Gap · 🟡 Importante

**`AUD-01-02` toca a fachada pública consumida pelo ICM por reflexão — a spec não declara o contrato preservado**

**Problema:** a §4 lista `CombatMedicSurgery.cs` e `CombatMedicAllyPerks.cs` como arquivos a modificar pelo `AUD-01-02`. Ambos expõem **API pública consumida por outro mod** (TRL-ImmersiveCombatMedicine) **por reflexão**, o que significa que o compilador **não** protege contra quebra:

- `CombatMedicSurgery.Adjust(Player? doctor, float penalty)` — `public static`, chamada pelo ICM no `ApplySurgery` (documentado no XMLdoc do arquivo).
- `CombatMedicSurgery.SetExternalHandling(bool)` — `public static`, chamada pelo ICM.
- `CombatMedicAllyPerks.AllyHealTimeMult(bool isSurgery)` e `AllyMobileSurgeon()` — `public static`.

A spec não diz que essas assinaturas são intocáveis. Um dev seguindo o §5.2 ("`CombatMedicSurgery.Adjust` troca `string.Equals(cls, "Combat Medic", …)` por `ClassIdOf(doctor) == EClassId.CombatMedic`") pode, com boa intenção, também trocar o **tipo do parâmetro** ou renomear — e a quebra só apareceria in-game, como cirurgia de aliado sem o perk, sem nenhum erro no log do CustomClasses.

Agrava: `EClassId` é declarado `internal` no stub da §5.1, mas `Identity.ClassId` é `public` na §5.2 — **um campo público de tipo internal não compila** fora do assembly e é, no mínimo, inconsistente.

**Por que importa:** quebra silenciosa de integração entre mods é o pior tipo — não há stack trace, só um perk que "às vezes não funciona".

**Sugestão:**
1. Acrescentar à §7 (Riscos) uma linha explícita: **"assinaturas públicas consumidas pelo ICM por reflexão são intocáveis nesta rodada"**, listando as quatro acima. Só o **corpo** muda.
2. Resolver a inconsistência de visibilidade: declarar `EClassId` como `internal` e o campo `Identity.ClassId` também `internal` (a `Identity` é `internal sealed class`, então `public` nos membros já era decorativo).
3. Acrescentar ao checklist da §8: *"conferir que `Adjust`, `SetExternalHandling`, `AllyHealTimeMult` e `AllyMobileSurgeon` mantêm assinatura byte a byte"*.
4. Acrescentar ao AC de não-regressão da 01-spec (seção A, Fika): **cirurgia de aliado com o ICM** — médico opera um aliado e o HP máximo do membro é preservado conforme o perk.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-07 · C — Erro de Lógica · 🟡 Importante

**`AUD-01-07b` é micro-otimização de código frio: o ganho é ~30 µs por janela**

**Problema:** a §5.7 troca o `yield return null` do `AdrenalineState.WatchWindow` por `WaitForSeconds(SecondsLeft)`. Dimensionando o ganho: a janela dura 25 s (default), o loop resume ~1.500–3.600 vezes, e cada resumo é um `MoveNext` de máquina de estado + uma comparação de float — na casa de **20 ns**. Ganho total: **~30–70 µs por janela de Adrenalina**, e só para a classe Fuzileiro.

Em troca, introduz três mudanças de comportamento:
1. O fechamento da janela deixa de ser detectado no frame exato e passa a ter até `0.05 s` de atraso (o `Mathf.Max(0.05f, …)` do próprio stub) — justamente o que o `ForceReloadResync` existe para evitar ("uma recarga iniciada logo após o fechamento poderia sair ainda acelerada", XMLdoc de `EnsureReloadResync`).
2. `WaitForSeconds` usa **tempo escalado**; `yield return null` não. Comportamentos divergem se o `timeScale` mudar.
3. Aloca um `WaitForSeconds` por iteração (hoje: zero alocação).

Isso é exatamente o que a skill `spt-performance-analysis` §8 proíbe: *"Micro-otimização de código frio ilegível não entra — o custo de manutenção supera o ganho inexistente"*.

**Por que importa:** trocar uma garantia de timing (o motivo pelo qual o watcher existe) por dezenas de microssegundos é um mau negócio, e contraria a régua que este mesmo relatório usou para rejeitar outras coisas.

**Sugestão:** **dropar o `AUD-01-07b`** e registrar no relatório 01 como `❌ sem ganho — rejeitado na review técnica PA-01-07`, com a razão (ganho ~30 µs/janela contra risco de timing no re-sync do reload). Se o usuário quiser mantê-lo mesmo assim, a forma mínima-risco é `yield return null` mantido e apenas **sair mais cedo** quando `Plugin.Instance == null` — o que não muda nada de mensurável, ou seja, reforça o argumento de dropar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-08 · A — Gap · 🟢 Menor

**Falta o bump de SemVer no checklist**

**Problema:** o checklist da §8 termina em "Build client 0 erros" e `05-asbuild`. A regra do repo é que **toda compilação exige bump de SemVer** — e a versão atual (`0.16.8`) está em **dois** csproj (`modded/Client/CustomClasses.Client.csproj:9` e `modded/Server/CustomClasses.Server.csproj:10`).

Como esta rodada é **client-only** (declarado na 01-spec), há uma decisão a tomar que a spec não toma: bumpar só o client (versões divergem entre os dois lados) ou os dois em lockstep (como está hoje).

**Por que importa:** `/compile-mod` pode recusar ou gerar uma DLL com versão repetida, e o launcher do usuário já demonstrou reverter DLL no sync (`feedback_server_launcher_sync_builds` na memória) — conferir a versão instalada é parte do gate.

**Sugestão:** acrescentar ao checklist: *"bumpar `<Version>` para `0.16.9` em **ambos** os csproj (manter lockstep, como hoje), mesmo sem mudança no server — evita divergência de versão entre DLLs de um mesmo release"*. E ao gate de validação: *"confirmar no log de boot que a DLL carregada é a 0.16.9"*.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-09 · A — Gap · 🟢 Menor

**`BuildTinted` é citado como "corpo atual, inalterado" mas não existe**

**Problema:** o stub da §5.3 chama `BuildTinted(name, (Color)qTop, (Color)qBottom)` com o comentário "corpo atual, inalterado". No código atual não existe esse método — o corpo (carregar PNG, `GetPixels32`, laço de tingimento, `SetPixels32`, `Apply`, `Sprite.Create`) está **inline** dentro de `GetTinted` (`ClassIconCache.cs:88-134`).

**Por que importa:** ambiguidade de implementação. Um dev pode achar que existe e procurar; outro pode extrair de um jeito que perca o `try/catch` ou o `Destroy(tex)` do caminho de falha (`:122`), que é o que evita vazar uma textura quando o `LoadImage` falha.

**Sugestão:** dizer explicitamente na §5.3: *"extrair o corpo atual de `GetTinted` (`ClassIconCache.cs:88-134`) para `private static Sprite? BuildTinted(string name, Color top, Color bottom)`, **preservando o `try/catch` e o `UnityEngine.Object.Destroy(tex)` do ramo de `LoadImage` falho**; `GetTinted` passa a ser só a camada de chave/LRU."*

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

### PA-01-10 · B — Edge Case · 🟢 Menor

**`PerfDumpLoop` é um `while (true)` perpétuo introduzido numa rodada de performance**

**Problema:** a §5.8 adiciona uma corrotina `while (true)` iniciada no `Awake`, viva pelo processo inteiro, inclusive no **Fika headless** (que fica de pé por horas hospedando raid após raid). O corpo é barato (um `WaitForSeconds(60f)` + dois checks), mas: (1) é o padrão que a própria skill marca como FREQ/LIFE; (2) é instrumentação temporária que precisa ser removida na Fase 4 — e uma corrotina no `Awake` é fácil de esquecer; (3) roda mesmo com o diagnóstico desligado (só não loga).

**Por que importa:** não é um problema de custo — é de higiene e de risco de a instrumentação virar permanente por esquecimento, que é exatamente o que a regra `// PERF-INSTR` existe para evitar.

**Sugestão:** duas mudanças pequenas:
1. Trocar `while (true)` por `while (PerksConfig.DiagnosticsEnabled?.Value == true)` e (re)iniciar a corrotina no `SettingChanged` do toggle — com o diagnóstico desligado (o default), **a corrotina nem existe**.
2. Acrescentar à §8 e ao `05-asbuild` um item explícito de Fase 4: *"`grep -rn 'PERF-INSTR' modded/Client/` deve voltar vazio após a validação"* — a corrotina inteira é um dos blocos a remover, não só as linhas de log.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Resolução

Todos os 10 pontos **aceitos pelo usuário em 2026-08-23**. O que mudou, ponto a ponto:

| ID | Resolução | Onde |
|---|---|---|
| **PA-01-01** | `Shoot` consolidado **4 → 2**, não 4 → 1: `ShootCapturePatch` (`Priority.First`) + `ShootApplyPatch` (`Priority.Last`). O estático `RecoilFloorCapturePatch.StrBefore` **fica** (dois patches não compartilham `__state`) e foi renomeado para `ShootRecoilState.StrBefore`. As garantias de fronteira contra mods externos (RealRecoil) são preservadas literalmente. | spec-tech §2, §5.6, §7, §8 · 01-spec (meta `AUD-01-03`) |
| **PA-01-02** | `Q()` passa a clampar **depois** da quantização (`Mathf.Min(q, 255)`). Teste visual de classe clara (Saqueador `#c4ad45`) acrescentado ao checklist e ao AC. | spec-tech §5.3, §8 · 01-spec (AC A) |
| **PA-01-03** | Chave do cache de tooltip vira `(ESkillId, float, string?)` incluindo `className`, e o cache é limpo em `SkillMultipliers.Apply()`/`Reset()`. | spec-tech §5.7, §8 |
| **PA-01-04** | 01-spec ganha a nota de linha de base: onde P-10.1/P-16.1 indicam comportamento nunca validado, o critério é **"idêntico ao build anterior"**, não "funciona". Fase 4 passa a exigir **raid de baseline na DLL atual antes de instalar a nova**. | 01-spec (AC A + nota) · spec-tech §8 |
| **PA-01-05** | **Confirmado por evidência:** `MainMenuPlayerModelView` é criado e nomeado pelo Menu-Overhaul em `mods/SPT-Menu-Overhaul/modded/Patches/PlayerProfileFeaturesPatch.cs:302`. Sem o MO, `nick` nunca resolve e `ApplyToMenu` já sai no guard atual **antes** do `FixTopGlow` → o bail preserva o comportamento. `Environment UI`/`Glow Canvas`/`TopGlowPve` são objetos **do EFT** que o MO apenas muta (`MenuVisibilityController.cs:14-15`), mas isso é irrelevante porque o caminho já era inalcançável. Cadeia registrada como comentário no código. | spec-tech §5.4, §7 |
| **PA-01-06** | §7 ganha linha declarando **intocáveis** as 4 assinaturas públicas consumidas pelo ICM por reflexão; `EClassId` e `Identity.ClassId` passam a `internal` (consistência de visibilidade); checklist ganha a conferência de assinatura; 01-spec ganha AC de cirurgia de aliado via ICM. | spec-tech §5.1, §5.2, §7, §8 · 01-spec (AC A/Fika) |
| **PA-01-07** | **`AUD-01-07b` DROPADO.** Registrado no relatório de auditoria como `❌ Rejeitado` com a razão (ganho ~30 µs/janela contra atraso de 50 ms no re-sync do reload, alocação nova e divergência sob `timeScale`). Removido de §4, §5.7 e §8 da spec técnica e da 01-spec. | spec-tech §1, §4, §5.7, §8 · 01-spec · relatório 01 (`AUD-01-07`) |
| **PA-01-08** | Bump de `<Version>` 0.16.8 → **0.16.9** em **ambos** os csproj (lockstep) acrescentado ao checklist, mais a conferência da versão carregada no log de boot. | spec-tech §8 |
| **PA-01-09** | §5.3 passa a instruir a extração explícita do corpo atual (`ClassIconCache.cs:88-134`) para `BuildTinted`, **preservando o `try/catch` e o `Destroy(tex)` do ramo de `LoadImage` falho**. | spec-tech §5.3 |
| **PA-01-10** | `PerfDumpLoop` passa a `while (PerksConfig.DiagnosticsEnabled?.Value == true)`, iniciada/reiniciada no `SettingChanged` do toggle — com o default (`false`) a corrotina nem existe. Item de Fase 4 acrescentado: `grep -rn 'PERF-INSTR' modded/Client/` tem de voltar vazio. | spec-tech §5.8, §8 |

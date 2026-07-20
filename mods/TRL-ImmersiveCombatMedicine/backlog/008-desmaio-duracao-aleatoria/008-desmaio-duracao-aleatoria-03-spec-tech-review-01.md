# 008 — Desmaio: duração aleatória min–max · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [008-desmaio-duracao-aleatoria-02-spec-tech.md](008-desmaio-duracao-aleatoria-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, snapshot 2026-07-19 ~22h) + pendências P-2.13/P-2.14/P-2.15 (bugs históricos do "relógio único" do desmaio — recalcular com a config ao vivo durante um desmaio em curso deslocava o wake). Nenhuma pendência aberta bloqueia este item; P-2.13/P-2.15 já foram a motivação correta citada pela própria spec técnica (§3, piso de 5s herdado) e a spec preserva integralmente a garantia de relógio único (verificado abaixo, achados C confirmam).

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Parse do valor legado usa a cultura CORRENTE em vez de invariant — migração pode corromper silenciosamente valores fracionários | ✅ Resolvido |
| PA-01-02 | A — Gap de citação | 🟢 Menor | Corner case `min == max` (inclusividade de `Random.Range(float,float)`) sem evidência citada na spec | ✅ Resolvido |

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-07-19

**Parse do valor legado usa a cultura CORRENTE em vez de invariant — migração pode corromper silenciosamente valores fracionários**

**Problema:** O stub §5.3 usa `float.TryParse(entry.Value as string, out legacyDurationValue)` — a sobrecarga SEM `NumberStyles`/`IFormatProvider`, que usa `NumberStyles.Float | NumberStyles.AllowThousands` combinado com `NumberFormatInfo.CurrentInfo` (a cultura do THREAD atual do processo), não invariant.

Decompilei `BepInEx.dll` (a partir de `mods/TRL-ImmersiveCombatMedicine/modded/References/BepInEx.dll`, a MESMA DLL referenciada pelo mod) via `ilspycmd -t BepInEx.Configuration.TomlTypeConverter` e confirmei que o conversor de `float` do PRÓPRIO BepInEx faz o oposto — sempre grava e lê com cultura invariante:
```csharp
[typeof(float)] = new TypeConverter
{
    ConvertToString = (object obj, Type type) => ((float)obj).ToString(NumberFormatInfo.InvariantInfo),
    ConvertToObject = (string str, Type type) => float.Parse(str, NumberFormatInfo.InvariantInfo)
}
```
Ou seja: o valor órfão salvo no `.cfg` para `Duracao do Desmaio` está SEMPRE em formato invariante (ex.: `"47.5"`, ponto decimal), independente da cultura do sistema — mas o stub §5.3 da spec lê esse mesmo valor com a sobrecarga que usa a cultura CORRENTE do processo.

Isso é uma cultura NOVA de risco para este item: nenhuma migração existente no código hoje faz parse de `float` — a única migração que copia um valor (`Sistema de Braços`, mojibake) usa `bool.TryParse`, que não é sensível a separador decimal. `float.TryParse` é o PRIMEIRO parse numérico de config órfão neste mod (confirmado por grep: `float\.(TryParse|Parse)` em `modded/` = 0 ocorrências antes deste item) — o "mesmo padrão já usado" que a spec cita (§3, "Alternativa descartada") vale para a MECÂNICA de iterar `orphans` e comparar `section`/`key`, mas NÃO cobre esta classe de bug (culture-sensitive numeric parsing), que é específica de `float`.

Em uma cultura corrente com separador decimal `,` e separador de milhar `.` (ex.: `pt-BR`, `de-DE` — plausível para este usuário, cujo perfil e comentários no próprio código são em português) e SEM nenhuma evidência de que o EFT/BepInEx force `CultureInfo.InvariantCulture` no thread principal (grep por `CurrentCulture`/`InvariantCulture` em `references/eft-decompiled/` = 0 ocorrências), `NumberStyles.AllowThousands` permite que o parser trate o `.` de `"47.5"` como separador de milhar (sem validar agrupamento de 3 dígitos — comportamento documentado do .NET) e o REMOVA antes de interpretar os dígitos restantes como inteiro — produzindo `475` em vez de `47.5`. O `TryParse` retornaria `true` (não `false`) com um valor 10× maior, silenciosamente.

**Por que importa:** Diferente de uma falha de parse óbvia (que a spec já cobre implicitamente ao deixar `legacyDurationDef` nulo se `TryParse` falhar), este é um parse que **retorna sucesso com o valor ERRADO**. Como `ConfigBlackoutDurationMin`/`Max` têm `AcceptableValueRange<float>(5f, 120f)`, o valor distorcido (`475`) seria automaticamente CLAMPADO para `120` no `.Value =` (confirmei via decompile de `ConfigEntryBase.ClampValue`) — ou seja, o usuário que tinha customizado a duração do desmaio para um valor fracionário (ex.: `47.5s`, plausível dado que P-2.13 documenta tuning ao vivo desse campo com valores como "3-5s") acordaria da migração com `Min = Max = 120` (o TETO da faixa), o OPOSTO do objetivo explícito da spec (§3.2: "reproduz EXATAMENTE o comportamento fixo anterior... usuários existentes não percebem NENHUMA mudança de comportamento"). Valores legados SEM parte fracionária (`"20"`, `"35"`) não disparam o bug (não há `.` para confundir com separador de milhar) — o que torna o teste de validação sugerido no checklist §8 ("testar com um `.cfg` pré-existente contendo `Duracao do Desmaio` com valor `35`") insuficiente para pegar este caso, porque usa um valor inteiro.

**Sugestão:** No stub §5.3, trocar:
```csharp
float.TryParse(entry.Value as string, out legacyDurationValue)
```
por:
```csharp
float.TryParse(entry.Value as string, System.Globalization.NumberStyles.Float,
    System.Globalization.CultureInfo.InvariantCulture, out legacyDurationValue)
```
— espelhando exatamente como o `TomlTypeConverter` do próprio BepInEx grava/lê floats (`NumberFormatInfo.InvariantInfo`), garantindo round-trip correto independente da cultura do processo. Complementarmente, no checklist §8, trocar o valor de teste sugerido de `35` para algo com parte fracionária (ex.: `47.5`) rodando com a cultura do SO em `pt-BR` — esse é o caso que exercitaria o bug se o fix não for aplicado.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Stub §5.3 corrigido com `NumberStyles.Float`/`CultureInfo.InvariantCulture`; comentário inline explicando o risco.

---

### PA-01-02 · A — Gap de citação · 🟢 Menor · ✅ Resolvido em 2026-07-19

**Corner case `min == max` (inclusividade de `Random.Range(float,float)`) sem evidência citada na spec**

**Problema:** O §3 item 5 e o stub §5.4 afirmam que, com `Mínimo == Máximo`, o resultado do sorteio é "idêntico a uma duração fixa (caso degenerado do sorteio, não um caso especial)" — isso só é verdade se `UnityEngine.Random.Range(float, float)` for inclusivo em AMBOS os extremos (ao contrário da sobrecarga `int`, que é exclusiva no máximo). A spec usa essa API sem citar evidência de que a sobrecarga `float` tem esse contrato — apesar de ser um critério de aceite EXPLÍCITO da spec funcional (item 2 dos Critérios de aceite: "Com `min == max`, o comportamento é idêntico ao fixo de hoje").

Verifiquei isso decompilando `UnityEngine.CoreModule.dll` (a partir de `mods/TRL-ImmersiveCombatMedicine/modded/References/UnityEngine.CoreModule.dll`) via `ilspycmd -t UnityEngine.Random`: os PRÓPRIOS nomes de parâmetro no assembly confirmam o contrato —
```csharp
public static extern float Range(float minInclusive, float maxInclusive);
public static int Range(int minInclusive, int maxExclusive)
```
Confirma que a afirmação da spec está CORRETA — não é um bloqueador, é uma lacuna de citação (a spec não referencia essa prova em lugar nenhum, apesar de citar `MedicalLogic.cs:366` como precedente de USO da API, não como prova de CONTRATO).

**Por que importa:** Não bloqueia — a afirmação está certa. Mas é o tipo de asserção "endurecida" (crítica para um critério de aceite explícito) que a lição da Sessão 4 do item 007 (memória, `2026-07-19 22:00`) recomenda documentar com prova formal: "quando uma prova é adicionada especificamente para endurecer uma decisão crítica, vale re-verificar a prova em si". Sem a citação, uma rodada futura de review/manutenção não tem como confirmar rapidamente que a suposição continua válida (ex.: numa atualização de engine Unity) sem repetir o decompile do zero.

**Sugestão:** Adicionar ao §3 item 5 (ou ao stub §5.4) da spec técnica a citação:
> `UnityEngine.Random.Range(float minInclusive, float maxInclusive)` — nomes de parâmetro confirmados via `ilspycmd -t UnityEngine.Random UnityEngine.CoreModule.dll`: ambos os extremos INCLUSIVOS (ao contrário da sobrecarga `int Range(int minInclusive, int maxExclusive)`, que exclui o máximo).

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Citação adicionada no stub §5.4 confirmando `Random.Range(float,float)` inclusivo nos dois extremos.

---

## Confirmações desta rodada (sem achado — verificado e correto)

- **Call sites de `ConfigBlackoutDuration`:** grep exaustivo próprio (`grep -rn "ConfigBlackoutDuration" modded/`) confirma exatamente os 3 sites que a spec lista para migração (`HealthPatches.cs:95`, `TRLImmersiveCombatMedicinePlugin.cs:546`, `FikaBridge.cs:30`) + a declaração/bind (linhas 30/100 do Plugin) — nenhum quinto site esquecido.
- **Tipo de `entry.Value`:** confirmado via decompile de `BepInEx.Configuration.ConfigFile` que `OrphanedEntries` é `private Dictionary<ConfigDefinition, string>` — `entry.Value as string` no stub §5.3 é um cast correto (não é o tipo que está errado; é a CULTURA do parse — ver PA-01-01).
- **Sorteio único por desmaio:** confirmado por leitura de `HealthPatches.cs` (stub substitui exatamente as linhas 91-97 atuais, dentro do `if (shouldFaint)`) e por grep de TODOS os leitores/escritores de `BlackoutTimers`/`BlackoutStartTimes` no mod (`TRLImmersiveCombatMedicinePlugin.cs`, `FikaBridge.cs`, `MovementPatches.cs`, `BandAidNetworkHandler.cs`) — nenhum outro ponto chama `Random.Range` ou relê `ConfigBlackoutDurationMin/Max`; todos os demais leitores comparam contra o deadline já gravado (`Time.time < BlackoutTimers[id]`), de forma opaca.
- **Fallbacks "comprovadamente mortos":** confirmado por grep que os ÚNICOS 2 pontos que ESCREVEM `BlackoutTimers` (`HealthPatches.cs:96` e `BandAidNetworkHandler.cs:135`) sempre escrevem `BlackoutStartTimes` na linha seguinte (97/136) — o ramo defensivo em `TRLImmersiveCombatMedicinePlugin.cs` (`Update()`, `else` sem `BlackoutStartTimes`) e o fallback em `FikaBridge.cs` (`isFainted==true` sem `BlackoutTimers`) são de fato inalcançáveis em operação normal, confirmando a afirmação da spec.
- **`BandAidNetworkHandler.cs:132` (literal `20f`):** confirmado que a linha existe exatamente como citada (`float duration = packet.DurationSeconds > 0f ? packet.DurationSeconds : 20f;`), com o comentário explícito na linha 129 ("Duração vem do PACOTE... nunca da config local deste processo") — o raciocínio da spec para NÃO tocar esta linha está correto.
- **Ângulo adicional investigado (sem achado):** verifiquei se o Postfix reusado (`DamageTriggerPatch`, alvo `Player.ApplyDamageInfo`, virtual) poderia rodar redundantemente em processos que apenas OBSERVAM outro jogador (mirror `ObservedPlayer` do Fika), o que geraria sorteios especulativos divergentes antes do pacote de rede sobrescrever o deadline. `docs/trauma-primitives.md` §P7 (evidência já registrada nesta base, tabela de evidências) já prova que dano de peer HUMANO não invoca `ApplyDamageInfo` no processo do atirador (`SimulatedApplyShot` retorna `null` para bullets de peer humano — só bots/próprio dono processam localmente) — o Postfix roda exatamente uma vez, no processo DONO, confirmando (não contradizendo) o N/A do check 2 (`AP-02`) da spec técnica.

## Próximo passo

Nenhum bloqueador (🔴) encontrado. Recomendo aplicar PA-01-01 (fix de 1 linha, baixo risco, alta clareza de causa) e PA-01-02 (citação, custo zero) diretamente na spec técnica antes do `/code-mod`, sem necessidade de uma 2ª rodada de review — ambos os achados têm sugestão acionável e verificação própria já concluída nesta rodada.

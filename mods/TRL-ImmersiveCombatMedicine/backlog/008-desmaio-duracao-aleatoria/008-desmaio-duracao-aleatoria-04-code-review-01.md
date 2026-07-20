# 008 — Desmaio: duração aleatória min–max · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [008-desmaio-duracao-aleatoria-01-spec.md](008-desmaio-duracao-aleatoria-01-spec.md)
**Spec técnica:** [008-desmaio-duracao-aleatoria-02-spec-tech.md](008-desmaio-duracao-aleatoria-02-spec-tech.md)
**Asbuild:** [008-desmaio-duracao-aleatoria-05-asbuild.md](008-desmaio-duracao-aleatoria-05-asbuild.md)
**Data:** 2026-07-19

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Deferido: 1 · Total: 1

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, snapshot 2026-07-19 ~22h) + pendências P-2.13/P-2.14/P-2.15 (bugs históricos do "relógio único" do desmaio — recalcular com a config ao vivo durante um desmaio em curso deslocava o wake; piso de 5s existe por esses bugs). Nenhuma pendência bloqueia este item. P-2.13/P-2.15 são a motivação documentada do piso de 5s herdado por `ConfigBlackoutDurationMin`/`Max` (§3 da spec técnica) — preservado no código real (`AcceptableValueRange<float>(5f, 120f)` em ambos os campos, `TRLImmersiveCombatMedicinePlugin.cs:106-113`). Nenhuma lição registrada reapareceu como bug no diff.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟢 Menor | 6ª cópia quase-idêntica do bloco de busca de órfão em `MigrateOrphanedConfigKeys()` | Deferido (010) |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · D — Arquitetura · 🟢 Menor · Deferido em 2026-07-19

**6ª cópia quase-idêntica do bloco de busca de órfão em `MigrateOrphanedConfigKeys()`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs:389-411`](../../modded/TRLImmersiveCombatMedicinePlugin.cs#L389-L411)

**Problema:** O bloco de migração do item 008 repete, pela 6ª vez no mesmo método, o idioma "declarar `object legacyXDef = null`, `foreach (DictionaryEntry entry in orphans)` comparando `section`/`key` via `AccessTools.Property(...).GetValue(...)`, `break` no match, `if (legacyXDef != null) { ...; orphans.Remove(...); Config.Save(); ModLogger.LogWarning(...); }`" — já presente para `Sistema de Braços` (bool, linhas 350-375) e para os 4 rename-at-delivery de 003/004/005/006/007 (linhas 418-526). O bloco do item 008 (§5.3 da spec técnica) é o único que faz parse de `float` em vez de `bool`/descarte puro, mas a estrutura de busca em si (declarar var, iterar, comparar 2 strings via reflection, break) é idêntica às outras 5 cópias.

```csharp
object legacyDurationDef = null;
float legacyDurationValue = 20f;
foreach (System.Collections.DictionaryEntry entry in orphans)
{
    var def = entry.Key;
    string section = AccessTools.Property(def.GetType(), "Section")?.GetValue(def) as string;
    string key = AccessTools.Property(def.GetType(), "Key")?.GetValue(def) as string;
    if (section == "3. Balanceamento (Trauma)" && key == "Duracao do Desmaio" &&
        float.TryParse(entry.Value as string, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out legacyDurationValue))
    { ... }
}
```

**Por que importa:** Não é um bug deste item (a lógica está correta — confirmado pela review técnica PA-01-01/02 e por leitura direta acima). É débito de manutenibilidade que cresce a cada item que adiciona uma migração: o item 010 (spec já prevista em `mod-backlog.md`, "Migração de configs + release") vai remover `Sistema de Pernas/Braços/Estomago`, quase certamente adicionando uma 7ª/8ª/9ª cópia do mesmo padrão. Quanto mais cópias, maior a chance de um erro de copy-paste (ex.: comparar a `section`/`key` errada, ou esquecer o `orphans.Remove` + `Config.Save` de uma delas — a própria lição CR-03-01 já documentada no código é sobre esse exato tipo de erro). O mod já tem um precedente registrado de dívida análoga não resolvida: P-4.1 (memória, `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`) defere a extração de um helper compartilhado para um boilerplate diferente (`Update()` de 4 consumidores) pelo mesmo motivo.

**Sugestão:** Extrair um helper privado reaproveitável, por exemplo:
```csharp
private static bool TryFindOrphan(System.Collections.IDictionary orphans, string section, string key, out object def, out string rawValue)
{
    foreach (System.Collections.DictionaryEntry entry in orphans)
    {
        var d = entry.Key;
        string s = AccessTools.Property(d.GetType(), "Section")?.GetValue(d) as string;
        string k = AccessTools.Property(d.GetType(), "Key")?.GetValue(d) as string;
        if (s == section && k == key) { def = d; rawValue = entry.Value as string; return true; }
    }
    def = null; rawValue = null; return false;
}
```
e reduzir cada um dos 6 blocos a um `if (TryFindOrphan(orphans, section, key, out var def, out var raw)) { ...parse específico...; orphans.Remove(def); Config.Save(); ModLogger.LogWarning(...); }`. Não é urgente para fechar o item 008 (o código atual está correto); recomendo deferir para o item 010, que já vai mexer nesse método de qualquer forma para as remoções de `Sistema de Pernas/Braços/Estomago`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): registrado como premissa para o item 010 (Migração de configs + release), que já vai mexer em `MigrateOrphanedConfigKeys()` para remover as keys legadas de Pernas/Braços/Estômago — mesmo momento natural para extrair o helper `TryFindOrphan`.

<!-- Após /apply-code-review: marcar a opção escolhida, trocar título para ✅ Aplicado em YYYY-MM-DD e adicionar **Resolução:** ... + **Aplicação:** descrição do que foi feito + paths -->

---

## Confirmações desta rodada (sem achado — verificado e correto)

- **PA-01-01 (parse com `CultureInfo.InvariantCulture`) implementado de fato, não só na spec:** confirmado em [`TRLImmersiveCombatMedicinePlugin.cs:396-398`](../../modded/TRLImmersiveCombatMedicinePlugin.cs#L396-L398) — `float.TryParse(entry.Value as string, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out legacyDurationValue)`, literal e idêntico ao stub §5.3 corrigido pela review técnica.
- **`BandAidNetworkHandler.cs:132` (literal `20f`) permanece intocado:** `git diff HEAD -- .../BandAidNetworkHandler.cs` não produz nenhum hunk (arquivo idêntico ao commit `fb57f2b5`); leitura direta confirma a linha `float duration = packet.DurationSeconds > 0f ? packet.DurationSeconds : 20f;` ainda presente, com o comentário de contexto na linha 129 ("Duração vem do PACOTE... nunca da config local deste processo") também intocado.
- **Migração por CÓPIA em ambos os campos:** `TRLImmersiveCombatMedicinePlugin.cs:406-407` grava `ConfigBlackoutDurationMin.Value = legacyDurationValue; ConfigBlackoutDurationMax.Value = legacyDurationValue;` — os DOIS campos recebem o valor antigo, não só um.
- **Sorteio único, ponto único:** grep de `Random\.Range` em `modded/**/*.cs` mostra só uma ocorrência ligada ao desmaio — [`HealthPatches.cs:105`](../../modded/Patches/Trauma/HealthPatches.cs#L105). O outro hit (`MedicalLogic.cs:366`, `BandAidNetworkHandler.cs:318`) é a penalidade de cirurgia, subsistema não relacionado. Grep de `ConfigBlackoutDurationMax` mostra uso único, no próprio ponto do roll (`HealthPatches.cs:99`) — nenhum fallback usa `Max`. Os 2 fallbacks (`TRLImmersiveCombatMedicinePlugin.cs:596` em `Update()`, `FikaBridge.cs:34` em `SyncFaintStatus`) usam somente `ConfigBlackoutDurationMin.Value`, sem chamar `Random.Range` de novo — confirmado por leitura direta e grep. Ambos os fallbacks continuam confirmadamente inalcançáveis em operação normal: os únicos 2 escritores de `BlackoutTimers` (`HealthPatches.cs:106`, `BandAidNetworkHandler.cs:135`) sempre gravam `BlackoutStartTimes` no mesmo bloco, e `FikaBridge.SyncFaintStatus(__instance, true)` só é chamado logo após `BlackoutTimers[id]` já ter sido escrito (`HealthPatches.cs:106→130`).
- **Versão 1.9.0 em todos os pontos:** `TRL-ImmersiveCombatMedicine.csproj` (`<Version>1.9.0</Version>`), `[BepInPlugin(..., "1.9.0")]` (`TRLImmersiveCombatMedicinePlugin.cs:17`) e o log do `Awake()` (`"TRL-ImmersiveCombatMedicine Plugin v1.9.0 carregado."`, linha 87) — os 3 pontos batem (regra do repo, `feedback_version_increment_on_release`).
- **Zero regressão no restante de `HealthPatches.cs`:** `git diff HEAD` produz um ÚNICO hunk no arquivo, cobrindo exatamente o trecho "Configura Timers" (linhas 88-116 do arquivo atual). A condição de entrada `shouldFaint` (linhas 81-84, incl. o filtro de domínio Chest/Head e o gate `ConfigConsumerBlackout2`/`TraumaBlackoutTrigger.Evaluate` que o item 007 entregou em v1.8.0) não aparece no diff — confirmado intocado.
- **`PROPRIEDADES.md`:** seção 3 substitui a linha única por 2 linhas novas com faixa/tooltip idênticos ao stub da spec técnica; tabela "Renomeadas" ganha a entrada documentando a migração por CÓPIA (distinta do padrão rename-at-delivery dos itens 003-007); Histórico de Alterações ganha a linha do item 008.
- **`mod-backlog.md`:** status do item 008 atualizado de ⚪ para 🟢.
- **Ângulo adicional investigado (sem achado) — corner case `min > max` até o ponto de escrita:** os 2 fallbacks (`Update()`, `FikaBridge`) leem `ConfigBlackoutDurationMin.Value` bruto (sem normalizar contra `Max`), mas isso só importaria se o fallback fosse alcançável com `isFainted==true` — não é (ver acima). No caminho `isFainted==false` (wake), o valor de `duration` computado é descartado pelo receptor (`BandAidNetworkHandler.OnTraumaFaintReceived`, ramo `else`, não lê `packet.DurationSeconds`/`GraceSeconds`) — confirmado por leitura de `BandAidNetworkHandler.cs:139-144`. Não é um achado.

## Próximo passo

Nenhum bloqueador. O único achado (CR-01-01) é 🟢, opcional e explicitamente recomendado para o item 010 (que já vai mexer no mesmo método para remover as keys legadas de Pernas/Braços/Estômago) — não impede fechar o item 008 agora.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Code review 01 criada via `/code-review` |

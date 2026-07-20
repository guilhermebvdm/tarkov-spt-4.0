# 008 — Desmaio: duração aleatória min–max · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [008-desmaio-duracao-aleatoria-01-spec.md](008-desmaio-duracao-aleatoria-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

**Nenhum patch Harmony novo.** O item reusa o MESMO `[HarmonyPatch(typeof(Player), "ApplyDamageInfo")]` Postfix (`DamageTriggerPatch.Postfix`) que o item 007 acabou de entregar (v1.8.0) — [modded/Patches/Trauma/HealthPatches.cs:51-131](../../modded/Patches/Trauma/HealthPatches.cs). Dentro desse Postfix, o ponto de mudança é a leitura da duração do desmaio, marcada pelo comentário `RANGE-READY` já existente no código (linhas 91-94), imediatamente antes da gravação do deadline absoluto em `TraumaState.BlackoutTimers[id]` (linha 96).

A mudança é: em vez de ler um único `ConfigEntry<float>` fixo (`ConfigBlackoutDuration.Value`), sortear uniformemente entre dois novos `ConfigEntry<float>` (`ConfigBlackoutDurationMin`/`ConfigBlackoutDurationMax`) usando `UnityEngine.Random.Range(min, max)` — o mesmo idioma já usado em [MedicalLogic.cs:366](../../modded/Patches/Medical/MedicalLogic.cs#L366) (`float penalty = UnityEngine.Random.Range(stats.SurgeryPenaltyMin, stats.SurgeryPenaltyMax);`) para a penalidade de cirurgia, também um par min/max sorteado uma única vez no ponto de decisão. Não um roll de sucesso/falha binário (`Random.value`) como o 006/007 — aqui é uma distribuição contínua, conforme a spec funcional exige.

**Alternativa descartada:** manter `ConfigBlackoutDuration` como um 3º campo "legado" ao lado dos 2 novos. Descartada porque, ao contrário dos placeholders inertes dos itens 003-007 (que nasceram com `false`/sem uso real e por isso foram substituídos via rename-at-delivery com DESCARTE do valor), `ConfigBlackoutDuration` é um valor ATIVO hoje, testado e ajustado por usuário real (ver P-2.13/P-2.15 na memória — o piso de 5s existe justamente por causa de tuning ao vivo desse campo). Manter os 3 campos deixaria 1 órfão sem leitor (confuso, dois "master" concorrentes para a mesma grandeza) ou exigiria decidir uma precedência arbitrária. A decisão adotada (§3) é **substituir** o campo único pelos 2 novos, mas **migrar o valor antigo por CÓPIA** (não por descarte) para dentro de AMBOS os novos campos — ver §3 e §7 para o detalhe da migração.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`Player.cs` — `ApplyDamageInfo`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) | Postfix (já existente, item 007) | Ponto onde `TraumaState.BlackoutTimers[id] = now + duration` é gravado hoje (linha 96 de `HealthPatches.cs`). Este item só troca COMO `duration` é calculado nas linhas 95-96 — não adiciona novo alvo de patch nem novo `[HarmonyPatch]`. |

Não há novo ponto de patch no Assembly do EFT. A "estratégia" deste item é inteiramente dentro do código do mod (`modded/`).

## 3. Novas propriedades F12 (BepInEx)

**Decisão de design (resposta ao Passo 3.1-3.3 do prompt):**

1. **Substituir, não somar.** `ConfigBlackoutDuration` ("Duracao do Desmaio") é REMOVIDO do código (campo `ConfigEntry<float>` deletado + `Config.Bind` removido). Dois novos campos ocupam a MESMA seção `3. Balanceamento (Trauma)`, mantendo a convenção de nomenclatura PT-BR já usada por essa seção legada (diferente das seções 5+ do Trauma 2.0, que usam nomes EN — a seção 3 nunca foi migrada e este item não inicia essa migração, fora de escopo).
2. **Migração por CÓPIA, não por descarte.** Ao contrário do padrão "rename-at-delivery" usado nos placeholders inertes dos itens 003-007 (`Legs Effects (item 003)` → `Legs Effects`, valor `false` descartado porque nunca foi escolha real do usuário — ver [PROPRIEDADES.md § Renomeadas](../../PROPRIEDADES.md)), aqui o valor antigo de `Duracao do Desmaio` FOI uma escolha real do usuário (é o campo que a P-2.13/P-2.15 documentam ter sido ajustado ao vivo para evitar o flap de desmaio curto). Descartá-lo seria uma regressão de UX. A migração em `MigrateOrphanedConfigKeys()` copia o valor órfão para **AMBOS** os novos campos (`Min` e `Max` = valor antigo), reproduzindo EXATAMENTE o comportamento fixo anterior no primeiro boot pós-atualização — usuários existentes não percebem NENHUMA mudança de comportamento até decidirem abrir o F12 e divergir min/max. Mesmo padrão de cópia (não descarte) já usado para a migração de `Sistema de Braços` (mojibake) em `MigrateOrphanedConfigKeys()`.
3. **Default de instalação nova** (sem órfão a migrar): `Min = 20f`, `Max = 20f` — idêntico ao default antigo do campo fixo. Preserva paridade de comportamento pronto-para-uso; o usuário opta conscientemente por uma faixa ao alterar no F12.
4. **Piso/teto herdados.** Ambos os campos usam `AcceptableValueRange<float>(5f, 120f)` — o MESMO piso de 5s e teto de 120s do campo fixo, por exigência explícita da spec funcional (critério "Corner cases" — piso de 5s é lição de UX documentada, colapso do flap blackout+grace).
5. **`min > max` (config inválida):** normalizado com `Mathf.Min`/`Mathf.Max` no ponto do roll, SEM lançar warning nem UI extra (mantendo o item simples, conforme instrução). Ambos os tooltips documentam esse comportamento explicitamente (visível no F12, sem precisar ler código).

| Seção | Nome (EN/PT — segue a seção legada) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `3. Balanceamento (Trauma)` | `Duracao Minima do Desmaio` | float | `20` | 5 a 120 | — | Duração MÍNIMA (segundos) do desmaio — sorteada uniformemente até o máximo abaixo a cada novo desmaio (independente de sorteios anteriores). ALINHAR ENTRE TODOS OS PEERS. Se este valor for maior que o Máximo, os dois são trocados antes do sorteio (config sempre produz um resultado válido). |
| `3. Balanceamento (Trauma)` | `Duracao Maxima do Desmaio` | float | `20` | 5 a 120 | — | Duração MÁXIMA (segundos) do desmaio — sorteada uniformemente a partir do Mínimo acima a cada novo desmaio. ALINHAR ENTRE TODOS OS PEERS. Com Mínimo == Máximo, o comportamento é idêntico a uma duração fixa (caso degenerado do sorteio, não um caso especial). |

Campo removido: `Duracao do Desmaio` (`ConfigBlackoutDuration`) — ver §7 (Riscos) para o inventário completo dos 3 call sites que liam esse campo e como cada um migra.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Remove `ConfigEntry<float> ConfigBlackoutDuration` (campo + `Config.Bind`); adiciona `ConfigBlackoutDurationMin`/`ConfigBlackoutDurationMax` (§3); adiciona bloco de migração por CÓPIA em `MigrateOrphanedConfigKeys()`; troca o fallback de `ConfigBlackoutDuration.Value` por `ConfigBlackoutDurationMin.Value` na linha 546 (`Update()`, ramo defensivo quando `BlackoutStartTimes` não tem entrada — ver §7). |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Ponto `RANGE-READY` (linhas 91-97): troca a leitura fixa por `Mathf.Min`/`Mathf.Max` + `UnityEngine.Random.Range(min, max)`; adiciona 1 linha de log (LogInfo, não gateada) com a duração sorteada — satisfaz o critério de aceite "verificável por log das durações sorteadas". |
| `modded/Fika/FikaBridge.cs` | MODIFICAR | Linha 30: troca o fallback `ConfigBlackoutDuration.Value` por `ConfigBlackoutDurationMin.Value` (mesmo raciocínio do Plugin.cs — ver §7). |
| `PROPRIEDADES.md` | MODIFICAR | Seção 3: substitui a linha `Duracao do Desmaio` por 2 linhas novas; seção Renomeadas ganha entrada documentando a migração por CÓPIA (distinta do padrão descarte dos itens 003-007); Histórico de Alterações ganha linha nova. |

Nenhum arquivo novo — item pequeno, sem classe nova.

## 5. Stubs de código

> Blocos compiláveis com assinatura completa e corpo mínimo plausível. Cada referência a algo do EFT tem comentário `// ref:`.

### 5.1 `modded/TRLImmersiveCombatMedicinePlugin.cs` — campos (substituindo a declaração de `ConfigBlackoutDuration`)

```csharp
// --- TrueTrauma Configs ---
public static ConfigEntry<bool> ConfigMasterEnabled;
public static ConfigEntry<bool> ConfigLegsEnabled;
public static ConfigEntry<bool> ConfigArmsEnabled;
public static ConfigEntry<bool> ConfigStomachEnabled;
public static ConfigEntry<bool> ConfigBlackoutEnabled;
// ref: item 008 — ConfigBlackoutDuration (campo único fixo) REMOVIDO; substituído pelo
// par min/max abaixo. Migração do valor antigo por CÓPIA (não descarte) em
// MigrateOrphanedConfigKeys() — ver stub 5.3.
public static ConfigEntry<float> ConfigBlackoutDurationMin;
public static ConfigEntry<float> ConfigBlackoutDurationMax;
```

### 5.2 `modded/TRLImmersiveCombatMedicinePlugin.cs` — `Config.Bind` (substituindo o bind de `ConfigBlackoutDuration` em `Awake()`)

```csharp
// ref: CR-04 — piso de 5s herdado: duração baixa (~3-5s no teste) colapsava blackout+grace
// num flap instantâneo (andar "desmaiado", timers sumindo antes do visual).
// ref: item 008 — par min/max substitui o campo fixo único; default 20/20 preserva o
// comportamento antigo em instalação nova (identidade com o valor fixo anterior).
ConfigBlackoutDurationMin = Config.Bind("3. Balanceamento (Trauma)", "Duracao Minima do Desmaio", 20f,
    new ConfigDescription(
        "Duração MÍNIMA (segundos) do desmaio — sorteada uniformemente até o máximo abaixo a cada novo desmaio (independente de sorteios anteriores). ALINHAR ENTRE TODOS OS PEERS. Se este valor for maior que o Máximo, os dois são trocados antes do sorteio (config sempre produz um resultado válido).",
        new AcceptableValueRange<float>(5f, 120f)));
ConfigBlackoutDurationMax = Config.Bind("3. Balanceamento (Trauma)", "Duracao Maxima do Desmaio", 20f,
    new ConfigDescription(
        "Duração MÁXIMA (segundos) do desmaio — sorteada uniformemente a partir do Mínimo acima a cada novo desmaio. ALINHAR ENTRE TODOS OS PEERS. Com Mínimo == Máximo, o comportamento é idêntico a uma duração fixa (caso degenerado do sorteio, não um caso especial).",
        new AcceptableValueRange<float>(5f, 120f)));
```

### 5.3 `modded/TRLImmersiveCombatMedicinePlugin.cs` — migração por CÓPIA em `MigrateOrphanedConfigKeys()`

```csharp
// ref: item 008 — MIGRAÇÃO POR CÓPIA (não descarte, diferente do padrão rename-at-delivery
// dos placeholders 003-007): "Duracao do Desmaio" era um valor ATIVO e real (ajustado ao
// vivo por lição de UX documentada — P-2.13/P-2.15), não um placeholder inerte. Copiar o
// valor antigo para OS DOIS campos novos reproduz o comportamento fixo anterior
// exatamente (min==max==valorAntigo) — usuário não percebe mudança até abrir o F12.
// PA-01-01 (review técnica 01): parse com CultureInfo.InvariantCulture — BepInEx
// (TomlTypeConverter) sempre grava/lê floats em cultura invariante; sem isso, num
// processo com cultura pt-BR/de-DE (ponto decimal → separador de milhar), um valor
// legado como "47.5" seria lido como 475 → clampado a 120 pelo AcceptableValueRange,
// migrando Min=Max=120 (o OPOSTO do objetivo). Confirmado por decompile do
// BepInEx.dll real que nenhuma migração existente no mod hoje faz parse de float
// (só bool, insensível a isso) — risco genuinamente novo deste item.
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
    {
        legacyDurationDef = def;
        break;
    }
}
if (legacyDurationDef != null)
{
    ConfigBlackoutDurationMin.Value = legacyDurationValue;
    ConfigBlackoutDurationMax.Value = legacyDurationValue;
    orphans.Remove(legacyDurationDef);
    Config.Save();
    ModLogger.LogWarning($"[Config] 'Duracao do Desmaio' (valor real do usuário) migrado por CÓPIA para Min E Max = {legacyDurationValue:F0}s; key antiga removida do .cfg.");
}
```

### 5.4 `modded/Patches/Trauma/HealthPatches.cs` — ponto `RANGE-READY` (substitui as linhas 91-97 atuais)

```csharp
// Configura Timers
// ref: CR-04 — GraceTimers NÃO nasce aqui: o grace de 5s é
// ancorado no WAKE (Plugin.WakeLocalPlayer / MainLoopPatch).
// ref: item 008 (RANGE-READY resolvido) — sorteio uniforme min-max no MESMO ponto que
// antes lia o fixo. min>max (config inválida) normalizado via Mathf.Min/Max — SEM
// warning/UI extra, mantendo o item simples (decisão da spec técnica §3). Com
// min==max, Random.Range devolve o valor determinístico — caso degenerado, não
// caso especial (nenhum branch dedicado). Todo o resto (wake, rampa visual,
// contusion, pacote de sync, espelhos) segue derivando do deadline gravado em
// BlackoutTimers — nada mais lê os configs de duração diretamente.
float configuredMin = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMin.Value;
float configuredMax = TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMax.Value;
float rollMin = Mathf.Min(configuredMin, configuredMax);
float rollMax = Mathf.Max(configuredMin, configuredMax);
// PA-01-02 (review técnica 01): Random.Range(float,float) é inclusivo em AMBOS os extremos
// (assinatura distinta de Random.Range(int,int), exclusiva no max) — confirmado por decompile
// de UnityEngine.CoreModule.dll. Com min==max, sempre devolve exatamente esse valor.
float duration = UnityEngine.Random.Range(rollMin, rollMax); // ref: MedicalLogic.cs:366 — mesmo idioma (Range, não .value)
TraumaState.BlackoutTimers[id] = now + duration;
TraumaState.BlackoutStartTimes[id] = now;

// ref: item 008 — log de verificação estatística (critério de aceite da spec funcional:
// "verificável por log das durações sorteadas"). LogInfo one-time por desmaio (não por
// frame) — não gateado por ConfigVerboseEngineLog, ao contrário do log de decisão do
// TraumaBlackoutTrigger (007): aqui é lifecycle (1 evento por desmaio), não detalhe de
// polling, mesmo critério de "LogInfo para evento único" já usado em
// TRLImmersiveCombatMedicinePlugin.cs (log de "entrou em Coma/Desmaio").
TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
    $"[Blackout] {id} duração sorteada: {duration:F1}s (min={rollMin:F0} max={rollMax:F0})");
```

### 5.5 `modded/Fika/FikaBridge.cs` — fallback (substitui a linha 30 atual)

```csharp
// ref: CR-01-02 — propaga aos peers (host espelha timers e controla o
// aggro dos bots). A duração viaja no pacote.
// ref: item 008 (RANGE-READY concluído) — o pacote carrega o valor ROLADO daquele
// desmaio (via BlackoutTimers, linha abaixo) sempre que isFainted=true; o fallback só
// serve o caso defensivo em que BlackoutTimers ainda não tem entrada (não deveria
// ocorrer — HealthPatches.cs grava os dois juntos — mas evita duration<=0 no pacote).
// Fallback usa o MÍNIMO configurado (não o antigo ConfigBlackoutDuration, removido):
// errar para o lado de uma duração mais curta é mais seguro que travar o alvo mais
// tempo que o configurado.
float duration = TRLImmersiveCombatMedicine.TRLImmersiveCombatMedicinePlugin.ConfigBlackoutDurationMin.Value;
if (isFainted && TraumaState.BlackoutTimers.TryGetValue(player.ProfileId, out float deadline))
    duration = UnityEngine.Mathf.Max(1f, deadline - UnityEngine.Time.time);
Band_Aid.BandAidNetworkHandler.SendTraumaFaintPacket(player.ProfileId, isFainted, duration, duration + 5f);
```

### 5.6 `modded/TRLImmersiveCombatMedicinePlugin.cs` — fallback em `Update()` (substitui a linha 546 atual)

```csharp
// ref: CR-04 (auditoria do desmaio) — RELÓGIO ÚNICO: o wake é dirigido pelo deadline
// ABSOLUTO gravado na entrada (BlackoutTimers); StartTimes serve só à rampa visual.
// ref: item 008 — fallback (ramo só alcançado se BlackoutStartTimes não tiver entrada
// para localId, o que não ocorre em operação normal — HealthPatches.cs e
// BandAidNetworkHandler.cs sempre gravam os dois dicts juntos) usa o MÍNIMO configurado
// em vez do extinto ConfigBlackoutDuration — mesmo raciocínio conservador do
// FikaBridge (stub 5.5): errar para uma duração mais curta é mais seguro.
if (TraumaState.BlackoutTimers.TryGetValue(localId, out float wakeDeadline))
{
    float duration = ConfigBlackoutDurationMin.Value;
    if (TraumaState.BlackoutStartTimes.TryGetValue(localId, out float startTime))
        duration = Mathf.Max(0.1f, wakeDeadline - startTime);
    else
        startTime = wakeDeadline - duration;
    float timeElapsed = Time.time - startTime;
    // ... resto do bloco inalterado (rampa visual por timeElapsed/duration)
}
```

## 6. Fluxo de dados

```
[A] Config F12 (Duracao Minima/Maxima do Desmaio, ConfigEntry<float>)
        │
        ▼
[B] HealthPatches.cs:91-99 (DamageTriggerPatch.Postfix, shouldFaint==true)
        Mathf.Min/Max(min,max) → UnityEngine.Random.Range(rollMin, rollMax) → duration
        │
        ▼
[C] TraumaState.BlackoutTimers[id] = now + duration   (deadline absoluto, OPACO daqui em diante)
    TraumaState.BlackoutStartTimes[id] = now
        │
        ├──▶ [D1] Plugin.cs Update() — wake local + rampa visual (lê wakeDeadline-startTime, NUNCA a config)
        ├──▶ [D2] MovementPatches.cs — checagem `now < BlackoutTimers[id]` (bot wake, opaco)
        └──▶ [D3] FikaBridge.SyncFaintStatus — duration = deadline - Time.time (opaco) → TraumaFaintPacket
                        │
                        ▼
                 [E] BandAidNetworkHandler.OnTraumaFaintReceived (peer)
                     BlackoutTimers[packet.ProfileId] = now + packet.DurationSeconds  (mesmo valor rolado, replicado)
```

Nenhum consumidor downstream (D1/D2/D3/E) muda neste item — todos já leem o deadline de forma opaca desde o item 007 (comentário `RANGE-READY` original já apontava isso). A única mudança real é o cálculo em [B].

## 7. Riscos e dependências

- **3 call sites que liam `ConfigBlackoutDuration` — todos migrados nesta spec:**
  1. `HealthPatches.cs:95` (ponto de roll — RANGE-READY) → vira leitura de Min/Max + `Random.Range` (§5.4).
  2. `TRLImmersiveCombatMedicinePlugin.cs:546` (fallback defensivo em `Update()`, ramo `else` só alcançado se `BlackoutStartTimes` não tiver entrada) → vira `ConfigBlackoutDurationMin.Value` (§5.6). Ramo comprovadamente morto em operação normal (todo escritor de `BlackoutTimers` grava `BlackoutStartTimes` no mesmo lugar — confirmado via grep: `HealthPatches.cs:96-97`, `BandAidNetworkHandler.cs:135-136`), mas precisa compilar e ter um valor seguro.
  3. `FikaBridge.cs:30` (fallback defensivo em `SyncFaintStatus`, só usado quando `isFainted==false` ou quando `BlackoutTimers` não tem a chave) → vira `ConfigBlackoutDurationMin.Value` (§5.5).
- **Um 4º call site EXISTE mas NÃO deve ser tocado:** `BandAidNetworkHandler.cs:132` (`float duration = packet.DurationSeconds > 0f ? packet.DurationSeconds : 20f;`) usa um literal `20f` hardcoded, não `ConfigBlackoutDuration`. O comentário na linha 129 do mesmo arquivo é explícito: *"Duração vem do PACOTE (config do dono), nunca da config local deste processo."* Trocar o `20f` por `ConfigBlackoutDurationMin.Value` LOCAL violaria esse princípio documentado (a config do peer receptor não deveria influenciar a duração de um estado cujo dono é outro processo). **Deixar como está** — fora de escopo, documentado aqui para não ser "corrigido" por engano num code-review futuro.
- **Patch existente que este item toca:** `DamageTriggerPatch` em `HealthPatches.cs` — o MESMO Postfix que o item 007 entregou há poucos commits (v1.8.0). Este item toca um trecho (linhas 91-99) estritamente ABAIXO e SEPARADO do trecho que o 007 modificou (linhas 75-84, condição de entrada `shouldFaint`). Nenhuma sobreposição de linhas — risco de conflito textual é baixo, mas o arquivo inteiro passou por overhaul de risco alto recentemente (justifica 1 rodada de review técnica em vez de pular, apesar do tamanho pequeno do item).
- **Compatibilidade com outros mods:** nenhuma — mudança inteiramente contida em `modded/`, sem novo alvo de patch no Assembly.
- **Ordem de inicialização:** nenhuma mudança — `ConfigBlackoutDurationMin`/`Max` são `Config.Bind` chamados em `Awake()`, mesma posição textual do bind removido; `MigrateOrphanedConfigKeys()` já roda depois de TODOS os binds (padrão existente, inalterado).
- **Fika/multiplayer:** confirmado por leitura de `FikaBridge.cs:27-33` que o pacote de sync (`TraumaFaintPacket`) JÁ deriva a duração do deadline gravado (`deadline - Time.time`), não da config, desde a implementação do item 007 preparatório — o comentário `RANGE-READY` em `FikaBridge.cs:27-29` previa exatamente este item ("quando a duração virar aleatória... o pacote carrega automaticamente o valor ROLADO"). **Nenhuma mudança de protocolo necessária** — o critério de aceite Fika da spec funcional já está satisfeito pela arquitetura existente.
- **Estado entre raids:** `TraumaState.BlackoutTimers`/`BlackoutStartTimes` já são limpos em `TraumaState.ResetAll()` ([TraumaState.cs:38-48](../../modded/Patches/Trauma/TraumaState.cs#L38-L48)), chamado em `OnRaidStartCleanup()`. Este item não introduz nenhum dicionário/estado novo — reusa os dois já existentes, só muda o valor computado antes de escrevê-los.

## 8. Checklist de implementação

- [x] Remover campo `ConfigEntry<float> ConfigBlackoutDuration` e seu `Config.Bind` em `TRLImmersiveCombatMedicinePlugin.cs`.
- [x] Adicionar campos `ConfigBlackoutDurationMin`/`ConfigBlackoutDurationMax` + 2 `Config.Bind` na seção `3. Balanceamento (Trauma)` (stub 5.1/5.2).
- [x] Adicionar bloco de migração por CÓPIA em `MigrateOrphanedConfigKeys()` (stub 5.3) — testar com um `.cfg` pré-existente contendo `Duracao do Desmaio` com valor customizado (ex.: 35) e confirmar que Min E Max nascem 35 após 1 boot, e que a key antiga some do `.cfg`. (Teste manual pendente — fora do escopo de `/code-mod`.)
- [x] Atualizar o ponto `RANGE-READY` em `HealthPatches.cs` (stub 5.4): `Mathf.Min`/`Mathf.Max` + `Random.Range` + log da duração sorteada.
- [x] Atualizar o fallback em `FikaBridge.cs:30` (stub 5.5).
- [x] Atualizar o fallback em `TRLImmersiveCombatMedicinePlugin.cs:546` (stub 5.6).
- [x] Confirmar que `BandAidNetworkHandler.cs:132` (literal `20f`) permanece INTOCADO (§7).
- [x] Atualizar `PROPRIEDADES.md`: seção 3 (2 linhas novas substituindo 1), seção Renomeadas (entrada da migração por cópia), Histórico de Alterações.
- [ ] Compilar via `/compile-mod` (0 erros) e bumpar a versão semver no `[BepInPlugin]` + log do `Awake()` (mudança de config visível ao usuário = pelo menos MINOR — regra do repo, `feedback_version_increment_on_release`). (Versão bumpada para 1.9.0 nos 3 pontos; compilação efetiva via `/compile-mod` fora do escopo deste `/code-mod`.)
- [ ] Validação manual sugerida (fora do gate formal): configurar min=5/max=60, causar ~10 desmaios, conferir no log `[Blackout] ... duração sorteada:` que os valores cobrem o intervalo (não concentrados numa ponta) — critério de aceite "Amostra estatística" da spec funcional.

## 9. Conformidade com skills (auto-checklist)

> Preenchido pelo `/create-technical-spec` ANTES de salvar. Cada linha: ✅ com evidência (seção desta spec ou `arquivo:linha`), ou **N/A + razão**. Linha ❌ → a spec não está pronta. Validado pelo `/review-technical-spec`. Taxonomia: [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md).

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Nenhum estado novo por raid é introduzido. `BlackoutTimers`/`BlackoutStartTimes` já existem e já são limpos em `TraumaState.ResetAll()` ([TraumaState.cs:38-48](../../modded/Patches/Trauma/TraumaState.cs#L38-L48)), chamado por `OnRaidStartCleanup()` — este item só muda o VALOR gravado neles, não o ciclo de vida (§7 "Estado entre raids"). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Nenhum `[HarmonyPatch]` novo é criado (§1/§2). O Postfix reusado (`DamageTriggerPatch`) já foi auditado quanto a esse ponto na spec técnica do item 007 (que o entregou); este item não altera o filtro de entrada (`shouldFaint`, linhas 75-84), só o cálculo de duração ABAIXO dele. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Sem novo alvo de patch — `Player.ApplyDamageInfo` já é o alvo do item 007, auditado naquela spec. Este item não adiciona reflexão nem resolve tipo ofuscado novo. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | Nenhuma mutação de estado do EFT/Unity — a mudança é inteiramente dentro do dicionário PRÓPRIO do mod (`TraumaState.BlackoutTimers`, já opaco para os leitores desde o 007). Nenhum campo do `Player`/`ActiveHealthController` é escrito por este item. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | `TraumaState.ResetAll()` ([TraumaState.cs:38-48](../../modded/Patches/Trauma/TraumaState.cs#L38-L48)) já limpa `BlackoutTimers`/`BlackoutStartTimes` incondicionalmente no início de cada raid (`OnRaidStartCleanup`, chamado no Prefix de `GameWorld.OnGameStarted` — [Plugin.cs:288-297](../../modded/TRLImmersiveCombatMedicinePlugin.cs#L288-L297)), cobrindo qualquer forma de saída da raid anterior (extração, morte, MIA, alt-F4) sem depender de um hook de STOP explícito — comportamento pré-existente, inalterado por este item. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: `Min`/`Max` com `AcceptableValueRange<float>(5f, 120f)` idêntico ao campo antigo; defaults 20/20 (paridade com o fixo anterior); `min > max` documentado no PRÓPRIO tooltip (normalização automática, sem exceção); `min == max` documentado como caso degenerado (não especial) do sorteio uniforme — todos os 3 corner cases da spec funcional cobertos no texto visível ao usuário no F12. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhuma invocação do método patcheado a partir do próprio patch; o roll de duração é uma leitura pura de config + escrita em dicionário, sem chamar `ApplyDamageInfo` novamente. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | Nenhum cache de intercept é introduzido. `BlackoutTimers`/`BlackoutStartTimes` já existiam com a MESMA semântica de "deadline opaco por ID de jogador" desde antes deste item; nenhuma troca de arma/operação/tela invalida esse valor (ele é lido só por `Time.time` comparação, não por identidade de objeto). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` |
| 2026-07-19 | Review técnica 01 aplicada: PA-01-01 (parse do valor legado com `CultureInfo.InvariantCulture` — sem isso, cultura pt-BR/de-DE corromperia a migração) e PA-01-02 (citação de evidência para `Random.Range(float,float)` inclusivo nos dois extremos). 0 achados pendentes — pronta para `/code-mod`, sem 2ª rodada. |

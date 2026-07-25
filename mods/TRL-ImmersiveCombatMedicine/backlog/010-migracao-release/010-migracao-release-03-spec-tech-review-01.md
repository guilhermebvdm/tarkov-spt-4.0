# 010 — Migração de configs + release · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [010-migracao-release-02-spec-tech.md](010-migracao-release-02-spec-tech.md)
**Data:** 2026-07-25

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 7 · Total: 7

**Memória consultada:** snapshot de 2026-07-25 (Sessão 5) · pendências que afetam: nenhuma bloqueadora específica do item 010 ([P-4.4], validação in-game do overhaul completo, é pendência geral pré-existente e não bloqueia esta revisão de spec).

**Achados da própria spec técnica — confirmados por leitura independente (não recontados como novos pontos, ambos corretos e sem ação pendente):**
- Achado crítico (§1): remover `ConfigArmsEnabled` exige remover também o bloco do mojibake em `MigrateOrphanedConfigKeys()`. Confirmado: o bloco vai de `TRLImmersiveCombatMedicinePlugin.cs:339` (doc comment) a `:375` (fecha o `if (orphanDef != null)`), escreve `ConfigArmsEnabled.Value` só em `:371`, e os 5 blocos restantes do método (`:377-526`, duração do desmaio + 4 rename-at-delivery) usam variáveis locais próprias (`legacyDurationDef`, `legacyLegsDef`, `legacyFallDef`, `legacyArmsDef`, `legacyStomachDef`, `legacyBlackoutDef`) sem tocar nenhum dos 3 campos removidos. Grep exaustivo (`grep -rn "ConfigLegsEnabled\|ConfigArmsEnabled\|ConfigStomachEnabled" modded/`) confirma zero outros usos além de declaração+Bind+este bloco. Resolução da spec está correta.
- Achado de design (§4): título/rótulos de membro do `BandAidUI` fixados em `BuildUI()` (Awake) precisam ser revalidados em `ShowUI()`. Confirmado: `BuildUI()` chama `_canvasObj.SetActive(false)` logo após construir o painel (`BandAidUI.cs:371`), tornando o texto inicial de fato inconsequente; `ShowUI(Player target)` está em `:643` e já recomputa o footer dinâmico (`:649-661`, texto idêntico ao citado na spec). O fix proposto (mover título + `_limbViews[...].NameText` para dentro de `ShowUI()`) é estruturalmente válido — `_limbViews` é `Dictionary<EBodyPart, LimbUI>` e `LimbUI.NameText` existe (`:569`). Resolução da spec está correta.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | `MedicLocale.GetDenyReasonText` não compila — falta `using Band_Aid` | ✅ Aplicado |
| PA-01-02 | C — Erro de Lógica | 🔴 Bloqueador | `BandAidNetworkHandler.cs` não compila — `MedicDenyReasonId` sem using/qualificação | ✅ Aplicado |
| PA-01-03 | C — Erro de Lógica | 🔴 Bloqueador | Terceiro ponto de leitura de `DenyReason` não mapeado (`OnHealCheckResponseReceived:930`) | ✅ Aplicado |
| PA-01-04 | A — Gap | 🟢 Menor | Handler duplicado/morto em `TRLImmersiveCombatMedicinePlugin.cs:333` nunca auditado | ✅ Aplicado |
| PA-01-05 | C — Erro de Lógica | 🟡 Importante | Citações de linha desatualizadas em `BandAidNetworkHandler.cs` | ✅ Aplicado |
| PA-01-06 | A — Gap | 🟡 Importante | `MedicLocale` perde os ícones ⚠/☠ das notificações de necrose do torniquete | ✅ Aplicado |
| PA-01-07 | A — Gap | 🟡 Importante | Stub do `Update()` do Plugin omite o bloco `IsFikaInstalled` sem marcador de elisão | ✅ Aplicado |

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

**`MedicLocale.GetDenyReasonText` não compila — falta `using Band_Aid` (ou qualificação) para `ItemDatabase`**

**Problema:** O stub 4 (§5, `MedicLocale.cs`, arquivo novo) declara `namespace TRLImmersiveCombatMedicine` com usings `System.Collections.Generic` e `EFT` apenas. O método `GetDenyReasonText` chama:
```csharp
var stats = ItemDatabase.GetStats(itemTemplateId);
```
`ItemDatabase` é `public static class` definida em `modded/Helpers/ItemDatabase.cs:3,29` sob `namespace Band_Aid`. Sem `using Band_Aid;` (ou `Band_Aid.ItemDatabase.GetStats(...)` qualificado), o compilador rejeita com CS0103 ("The name 'ItemDatabase' does not exist in the current context"). Confirmado lendo o arquivo real: `Helpers/ItemDatabase.cs` não tem nenhuma outra classe `ItemDatabase` em `TRLImmersiveCombatMedicine` que pudesse resolver a chamada por engano.

**Por que importa:** o stub é apresentado como "classe nova, completa" (§5 "Stub 4 — MedicLocale.cs (classe nova, completa)"). Implementado literalmente, o Bloco C inteiro não compila — o mod não builda.

**Sugestão:** adicionar `using Band_Aid;` à lista de usings do stub de `MedicLocale.cs` (junto com `System.Collections.Generic`/`EFT`), no mesmo padrão que `BandAidController.cs:20` já usa (`using Band_Aid;` num arquivo de namespace `TRLImmersiveCombatMedicine`). Alternativa equivalente: qualificar a chamada como `Band_Aid.ItemDatabase.GetStats(itemTemplateId)`.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** `using Band_Aid;` adicionado ao stub 4 (`MedicLocale.cs`) em `010-migracao-release-02-spec-tech.md`.

---

### PA-01-02 · C — Erro de Lógica · 🔴 Bloqueador

**`BandAidNetworkHandler.cs` não compila — `MedicDenyReasonId` referenciado sem `using` nem qualificação**

**Problema:** o stub 5 (§5, mudança de wire format) mostra o "DEPOIS" de `OnHealCheckReceived` assim:
```csharp
var denyReasonId = MedicDenyReasonId.UnknownItem;
...
denyReasonId = approved ? MedicDenyReasonId.None : MedicDenyReasonId.NoCompatibleWound;
```
e a nota final diz "(mesma mudança se aplica em `TryAnswerForLocalBot`, ~linha 833)" — implicando o mesmo padrão de código não-qualificado nos dois pontos. `MedicDenyReasonId` é declarado `internal enum` dentro de `namespace TRLImmersiveCombatMedicine` (stub 4). Já `BandAidNetworkHandler.cs` inteiro está em `namespace Band_Aid` (linha 12 do arquivo real), com usings `BepInEx.Logging`, `Comfort.Common`, `EFT`, `EFT.HealthSystem`, `EFT.Communications`, `Fika.Core.Networking`, `Fika.Core.Networking.LiteNetLib`, `System`, `System.Linq`, `System.Reflection` — nenhum deles é `TRLImmersiveCombatMedicine`. Sem `using TRLImmersiveCombatMedicine;` (ou qualificação `TRLImmersiveCombatMedicine.MedicDenyReasonId`), o mesmo CS0103 ocorre — desta vez no arquivo que carrega a mudança de wire format mais arriscada do item inteiro (Prioridade 1 desta revisão).

**Por que importa:** exatamente a classe de problema que a Prioridade 1 desta revisão pediu para investigar ("ordem/uso simétrico dos campos" e "outro serializador não mapeado") — aqui é um erro de compilação garantido nos DOIS pontos de escrita do handshake (`OnHealCheckReceived` e `TryAnswerForLocalBot`), não um bug de runtime, mas igualmente bloqueador para `/code-mod`.

**Sugestão:** adicionar `using TRLImmersiveCombatMedicine;` ao topo de `BandAidNetworkHandler.cs` (mesmo padrão de `BandAidController.cs:18`, que já importa esse namespace num arquivo com tipos de outro namespace), ou qualificar toda referência a `MedicDenyReasonId` como `TRLImmersiveCombatMedicine.MedicDenyReasonId` nos dois métodos.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** nota adicionada ao stub 5 em `010-migracao-release-02-spec-tech.md` instruindo o `using TRLImmersiveCombatMedicine;`; item também adicionado ao checklist §8 Bloco C.2.

---

### PA-01-03 · C — Erro de Lógica · 🔴 Bloqueador

**Terceiro ponto de leitura de `DenyReason` não mapeado pela spec — `BandAidNetworkHandler.OnHealCheckResponseReceived:930` quebra a build**

**Problema:** grep exaustivo (`grep -rn "DenyReason\|BandAidHealCheckResponsePacket" modded/`) revela um ponto de leitura do campo que a spec técnica não menciona em nenhum lugar (nem §4, nem stub 5, nem checklist §8): `BandAidNetworkHandler.cs:930`, dentro de `OnHealCheckResponseReceived` (o handler REAL registrado via `currentManager.RegisterPacket<BandAidHealCheckResponsePacket>(OnHealCheckResponseReceived)` em `:63` — distinto de `BandAidController.OnHealCheckResponseHandler`, que só recebe via evento `OnHealCheckResponse` depois de `OnHealCheckResponseReceived` disparar `OnHealCheckResponse?.Invoke(packet)` em `:933`). A linha exata:
```csharp
Logger.LogInfo($"HealCheck Response recebido | Approved: {packet.Approved} | Reason: {packet.DenyReason}");
```
O checklist §8 (Bloco C.2 "migrar `BandAidHealCheckResponsePacket`... e os 2 pontos que a preenchem (`OnHealCheckReceived`, `TryAnswerForLocalBot`)" e C.3 "migrar `BandAidController.OnHealCheckResponseHandler`") só cobre os 2 pontos de ESCRITA e 1 ponto de EXIBIÇÃO — nenhum cobre este terceiro ponto de LEITURA (um log de diagnóstico). Depois do rename `DenyReason`(string)→`DenyReasonId`(byte), esta linha para de compilar: CS1061 "'BandAidHealCheckResponsePacket' does not contain a definition for 'DenyReason'".

**Por que importa:** é exatamente o que a Prioridade 1(a) desta revisão pediu para verificar — "o pacote realmente só é usado nesses pontos, ou há outro serializador/handler em algum outro arquivo que a spec técnica não mapeou?". Resposta: há um handler a mais, e ele quebra a build mesmo depois de resolver PA-01-01/02.

**Sugestão:** atualizar o stub 5 e o checklist §8 (Bloco C.2 ou C.3) para incluir esta linha — trocar para `Reason: {packet.DenyReasonId}` (loga o enum cru, suficiente para diagnóstico) ou remover o segmento `| Reason: ...` do log.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** stub novo adicionado ao final do Stub 5 (`010-migracao-release-02-spec-tech.md`) trocando `packet.DenyReason`→`packet.DenyReasonId` em `OnHealCheckResponseReceived:930`; item adicionado ao checklist §8 como Bloco C.2b.

---

### PA-01-04 · A — Gap · 🟢 Menor

**Handler duplicado/morto em `TRLImmersiveCombatMedicinePlugin.cs:333` nunca auditado pela spec**

**Problema:** além de `BandAidController.OnHealCheckResponseHandler` (o handler real, migrado no stub 5), existe um SEGUNDO método com o MESMO nome e a MESMA assinatura, inscrito no MESMO evento:
```csharp
// TRLImmersiveCombatMedicinePlugin.cs:325
BandAidNetworkHandler.OnHealCheckResponse += OnHealCheckResponseHandler;
...
// TRLImmersiveCombatMedicinePlugin.cs:333-336
private void OnHealCheckResponseHandler(BandAidHealCheckResponsePacket response)
{
    // O tratamento disso ficará na classe dedicada ou adaptaremos o código de BandAidPlugin aqui.
}
```
Corpo vazio — não lê `.DenyReason`, então NÃO quebra a compilação com a mudança de wire format (diferente de PA-01-01/02/03). Mas é um segundo assinante inerte do mesmo evento que a spec técnica nunca menciona nem no §2 (pontos de patch) nem na auditoria de "grep exaustivo" citada no achado crítico do §1 — a spec verificou exaustivamente `ConfigArmsEnabled` mas não fez o mesmo grep para `OnHealCheckResponseHandler`/`BandAidHealCheckResponsePacket` como um todo (o que teria revelado também PA-01-03).

**Por que importa:** não bloqueia o build, mas é código morto do mesmo domínio que este item já está limpando (Bloco A é justamente sobre remover vestígios). Deixar passar despercebido enfraquece a confiança de que o "grep exaustivo" cobriu de fato todo o escopo do pacote.

**Sugestão:** mencionar este achado no §7 (Riscos) ou remover o método morto como parte do Bloco A (comentário explicativo já indica que era um placeholder nunca preenchido: "O tratamento disso ficará na classe dedicada..." — a classe dedicada, `BandAidController`, já existe e já faz o trabalho real). Se optar por manter, ao menos documentar que é intencionalmente inerte.

**Decisão:** `[x]` Aceitar sugestão (remover, não só documentar)

**Resolução:** novo Stub 2b adicionado ao Bloco A removendo o subscribe (`:325`) e o método morto (`:333-336`) por inteiro; item adicionado ao checklist §8 como Bloco A.7.

---

### PA-01-05 · C — Erro de Lógica · 🟡 Importante

**Citações de linha desatualizadas em `BandAidNetworkHandler.cs` (tabela de rastreio + stub 5)**

**Problema:** a "Tabela de rastreio" (§5) e o stub 5 citam linhas que não batem com o arquivo atual (texto confere, número da linha não):

| Citação da spec | Texto citado | Linha real |
|---|---|---|
| `BandAidNetworkHandler.cs:399-400` | "Você foi tratado por um aliado." | `:419` |
| `BandAidNetworkHandler.cs:614-615` | "Você recebeu um toque no ombro de {nickname}" | `:637` |
| "~linha 833" (`TryAnswerForLocalBot`, resposta em nome de bot) | construção do `response` com `DenyReason` | assinatura do método em `:857`, construção do `response` em `:886-894` |
| "~linhas 679-687" (`OnHealCheckReceived`, "lado que GERA a resposta") | `var stats = ItemDatabase.GetStats(...)` / `denyReason` | método começa em `:676`, `denyReason` declarado em `:711`, usado em `:709-716` |

O desvio (~20 a ~55 linhas) é consistente com citações herdadas sem reconferência da spec funcional (que já cravava os mesmos números `:399-400`/`:614-615`/`:681,686,833`) para um arquivo que evidentemente cresceu desde então.

**Por que importa:** `BandAidNetworkHandler.cs` tem 941 linhas com dois blocos de handshake quase idênticos (`OnHealCheckReceived`/`TryAnswerForLocalBot`) — um implementador que confie cegamente no número de linha citado (em vez de reler o arquivo real) arrisca editar o bloco errado. Não bloqueia (o texto é único e greppável), mas reduz a confiabilidade da spec como guia linha-a-linha justamente no arquivo de maior risco do item (a mudança de wire format).

**Sugestão:** atualizar as citações de linha na tabela de rastreio e no stub 5 para os números reais acima antes do `/code-mod`.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** citações de linha corrigidas no stub 5 (`010-migracao-release-02-spec-tech.md`) para os 3 pontos mapeados (`OnHealCheckReceived` :676-716, `TryAnswerForLocalBot` :857-894). As duas citações da tabela de rastreio (`:399-400`/`:614-615`) ficam como referência de TEXTO (greppável, único no arquivo) — o `/code-mod` deve localizar pelo texto citado, não confiar cegamente no número de linha em nenhum ponto deste arquivo de 941 linhas.

---

### PA-01-06 · A — Gap · 🟡 Importante

**`MedicLocale` perde os ícones ⚠/☠ das notificações de necrose do torniquete**

**Problema:** `TourniquetManager.cs:174` e `:182` (código atual) usam prefixos Unicode nas notificações de risco/destruição por necrose:
```csharp
$"⚠ Torniquete em {GetBodyPartName(bodyPart)}: risco de necrose! Remova agora!"   // :174
$"☠ {GetBodyPartName(bodyPart)} destruído por necrose do torniquete!"             // :182
```
As entradas correspondentes em `EnTexts`/`PtTexts` do stub 4 (`TourniquetNecrosisWarning`, `TourniquetDestroyed`) omitem os dois glifos:
```csharp
/* TourniquetNecrosisWarning*/ "Tourniquet on {0}: necrosis risk! Remove now!",     // EN, sem ⚠
/* TourniquetDestroyed      */ "{0} destroyed by tourniquet necrosis!",            // EN, sem ☠
```
(mesma omissão nos textos PT). A omissão já vinha da tabela de inventário da spec funcional (`01-spec.md` linhas 62-63), que a spec técnica herdou sem reconferir contra o texto-fonte real.

**Por que importa:** regressão visual observável em produção — perda do indicador visual de alerta/perigo nessas duas notificações específicas, sem nenhuma nota nos riscos (§7) ou no achado de design (§4) reconhecendo a perda como intencional.

**Sugestão:** incluir os prefixos nos templates EN/PT: `"⚠ Tourniquet on {0}: necrosis risk! Remove now!"` / `"⚠ Torniquete em {0}: risco de necrose! Remova agora!"` e `"☠ {0} destroyed by tourniquet necrosis!"` / `"☠ {0} destruído por necrose do torniquete!"`.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** ícones ⚠/☠ adicionados aos 4 templates (`TourniquetNecrosisWarning`/`TourniquetDestroyed`, EN e PT) no stub 4.

---

### PA-01-07 · A — Gap · 🟡 Importante

**Stub do `Update()` do Plugin omite o bloco `IsFikaInstalled`/`EnsurePacketsRegistered()` sem marcador de elisão**

**Problema:** o código real de `TRLImmersiveCombatMedicinePlugin.Update()` (linhas 554-572) começa assim:
```csharp
private void Update()
{
    if (IsFikaInstalled)
    {
        Band_Aid.BandAidNetworkHandler.EnsurePacketsRegistered();
    }

    // [DEBUG-ICM] roda ANTES de qualquer early-return: Plugin.Update comprovadamente vive em raid
    if (Time.time >= _debugNextBeat) { ... }
    ...
```
O stub 1 (§5, terceiro bloco "ANTES/DEPOIS" do Bloco A — sondas de heartbeat) mostra a versão "ANTES" do método já começando direto no `if (Time.time >= _debugNextBeat)`, sem o bloco `if (IsFikaInstalled) { ... }` acima, e sem nenhum comentário `/* ... inalterado ... */` sinalizando que algo foi omitido — diferente do padrão usado em outros trechos do mesmo stub (que sempre marcam elisões explicitamente, ex.: `// ... (demais AddComponent inalterados)`). A versão "DEPOIS" também omite o bloco, dando a impressão de que `Update()` do Plugin começa vazio antes da lógica master.

**Por que importa:** um implementador que aplique o diff de forma literal (copiar "ANTES" → substituir por "DEPOIS") em vez de editar o arquivo real linha a linha corre risco real de apagar por engano o registro de pacotes Fika deste método — que é um caminho de registro DISTINTO do `BandAidController.Update() → CheckInit()` (ambos chamam `EnsurePacketsRegistered()`, mas por guards diferentes: um gateado por `IsFikaInstalled`, outro incondicional a cada frame do controller). Perder esse registro quebraria o handshake de rede em qualquer sessão onde o `BandAidController` demore a inicializar.

**Sugestão:** no stub, incluir o bloco `if (IsFikaInstalled) { Band_Aid.BandAidNetworkHandler.EnsurePacketsRegistered(); }` na versão "ANTES" com a nota `// ... inalterado ...` logo antes do comentário `[DEBUG-ICM] roda ANTES...`, deixando explícito que ele permanece intocado na "DEPOIS".

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** bloco `IsFikaInstalled`/`EnsurePacketsRegistered()` incluído explicitamente nas versões ANTES e DEPOIS do stub do `Update()` (Bloco A, stub 1), com comentário deixando claro que é preservado sem alteração.

---

## Status

**Todos os 7 achados aplicados** em `010-migracao-release-02-spec-tech.md` (diretiva do usuário: aplicar todos os achados de review por padrão).

## Próximo passo

Dado o risco desta spec (mudança de wire format Fika + escopo de 4 blocos tocando ~10 arquivos), está prevista uma **rodada 2 de review técnica** antes do `/code-mod`, focada em confirmar que as 7 correções foram aplicadas sem introduzir nova inconsistência e em varrer ângulos ainda não cobertos pela rodada 1.

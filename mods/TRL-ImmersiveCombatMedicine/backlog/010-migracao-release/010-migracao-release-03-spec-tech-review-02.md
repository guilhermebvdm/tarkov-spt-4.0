# 010 — Migração de configs + release · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [010-migracao-release-02-spec-tech.md](010-migracao-release-02-spec-tech.md)
**Data:** 2026-07-25

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-02-MM` (review 02, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

**Memória consultada:** snapshot de 2026-07-25 (Sessão 5) · pendências que afetam: nenhuma bloqueadora específica do item 010 ([P-4.4], validação in-game do overhaul completo, é pendência geral pré-existente e não bloqueia esta revisão de spec).

## Confirmação das 7 correções da rodada 1

Releitura independente (Assembly/código real, não a narrativa da spec) confirma as 7 correções de `010-migracao-release-03-spec-tech-review-01.md` aplicadas corretamente, sem novo erro de compilação introduzido:

| ID | Confirmação |
|---|---|
| PA-01-01 | `using Band_Aid;` correto — `ItemDatabase` é o único tipo desse nome no repo (`Helpers/ItemDatabase.cs:3,29`); nenhum tipo de `Band_Aid` colide com identificadores usados no stub de `MedicLocale.cs` (`EBodyPart`/`EFT`/`Dictionary`). |
| PA-01-02 | `using TRLImmersiveCombatMedicine;` correto — `BandAidNetworkHandler.cs` real (linha 1-10) de fato não importava esse namespace; nenhum dos 5 tipos hoje declarados sob `namespace TRLImmersiveCombatMedicine` (`DebugBotInvisibility`, `BandAidController`, `MedicActionsPatch`, `MedicInteractable`, `TRLImmersiveCombatMedicinePlugin`) é referenciado sem qualificação no arquivo — zero risco de CS0104. |
| PA-01-03 | `OnHealCheckResponseReceived:930` é de fato o único 3º ponto de leitura — grep exaustivo (`grep -n "DenyReason" BandAidNetworkHandler.cs`) retorna só 3 ocorrências: os 2 pontos de escrita já cobertos (`:736`, `:892` reais) + este de leitura (`:930`). Nenhum 4º ponto existe. |
| PA-01-04 | Handler morto confirmado — `grep -rn "OnHealCheckResponse\b" modded/` mostra exatamente 2 assinantes hoje (`Plugin.cs:325`/`:330` morto, `BandAidController.cs:64`/`:75` real); removendo o par do Plugin sobra só o do `BandAidController`, com `+=`/`-=` pareados em `Awake()`/`OnDestroy()`. |
| PA-01-05 | Citações corrigidas batem com o real: `OnHealCheckReceived` começa em `:676` (real), `denyReason` declarado `:711` (real); `TryAnswerForLocalBot` começa em `:857` (real), resposta construída `:886-894` (real). Ver nuance em **PA-02-02** abaixo. |
| PA-01-06 | Ícones ⚠ (`TourniquetManager.cs:174` real) e ☠ (`:182` real) reproduzidos corretamente nos 4 templates EN/PT do stub. Ver **PA-02-01** abaixo — a mesma classe de regressão (ícone perdido) reaparece num 3º texto que a rodada 1 não cobriu. |
| PA-01-07 | Bloco `IsFikaInstalled`/`EnsurePacketsRegistered()` confirmado no `Update()` real (`TRLImmersiveCombatMedicinePlugin.cs:556-559`, antes do heartbeat `[DEBUG-ICM]` em `:561-569`) — stub ANTES/DEPOIS preserva a mesma posição relativa nas duas versões. |

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
| PA-02-01 | A — Gap | 🟡 Importante | `MedicLocale` perde o ícone ✈ da notificação `ShoulderTapReceived` (mesma classe de PA-01-06, não coberta na rodada 1) | ✅ Aplicado |
| PA-02-02 | C — Erro de Lógica | 🟢 Menor | Citação `OnHealCheckReceived :676-716` não cobre o ponto real de escrita do response (`:730-738`) — assimetria com `TryAnswerForLocalBot` | ✅ Aplicado |
| PA-02-03 | C — Erro de Lógica | 🟢 Menor | `package-release.sh`: `VER="$(pipeline)"` sob `set -euo pipefail` aborta silenciosamente antes do diagnóstico "ERRO: não achei `<Version>`" | ✅ Aplicado |
| PA-02-04 | A — Gap | 🟢 Menor | Checklist §8 não declara a cadeia de dependência implícita C.1→C.2→C.2b→C.3 nem a de D.1 sobre A-C (só A.1/A.2 recebe aviso explícito de ordem) | ✅ Aplicado |
| PA-02-05 | C — Erro de Lógica | 🟢 Menor | Comentário do stub `MedicLocale.cs` ("Common nunca chega aqui") é factualmente errado para `BandAidUI.ShowTreatment` | ✅ Aplicado |
| PA-02-06 | A — Gap | 🟢 Menor | Checklist D.1 pede "dar permissão de execução se necessário" para um script que só é invocado via `bash script.sh` | ✅ Aplicado |

---

## Pontos

### PA-02-01 · A — Gap · 🟡 Importante

**`MedicLocale` perde o ícone ✈ da notificação `ShoulderTapReceived` — mesma classe de regressão de PA-01-06, não coberta na rodada 1**

**Problema:** o texto real de `BandAidNetworkHandler.cs:637` é:
```csharp
$"✈ Você recebeu um toque no ombro de {packet.SenderNickname}",
```
(`✈` = ✈). A entrada correspondente do stub 4 (`010-migracao-release-02-spec-tech.md`, `EnTexts`/`PtTexts` índice 13, `ShoulderTapReceived`) é:
```csharp
/* ShoulderTapReceived      */ "Você recebeu um toque no ombro de {0}",        // PT, sem ✈
/* ShoulderTapReceived      */ "You received a shoulder tap from {0}",         // EN, sem ✈
```
A omissão já vinha da tabela de inventário da spec funcional (`01-spec.md:54`, "`:614-615` — 'Você recebeu um toque no ombro de {nickname}'" — sem menção ao ícone), herdada sem reconferência pela spec técnica — exatamente o mesmo padrão de causa raiz que gerou PA-01-06 (ícones ⚠/☠ do torniquete), só que num terceiro texto que nem a spec funcional nem a rodada 1 desta review verificaram contra o texto-fonte real.

**Por que importa:** regressão visual observável em produção — o toque no ombro é uma das poucas notificações com um indicador visual próprio (✈, distinto dos ícones de alerta ⚠/☠ já corrigidos); perder o glifo silenciosamente reduz a distinguibilidade do toast na tela sem nenhuma nota de risco em §7 ou no achado de design §4.

**Sugestão:** adicionar `✈ ` (ou o literal `✈ `) como prefixo em `PtTexts[13]`/`EnTexts[13]` (`ShoulderTapReceived`) no stub 4: `"✈ Você recebeu um toque no ombro de {0}"` / `"✈ You received a shoulder tap from {0}"`, no mesmo padrão de correção já aplicado a `TourniquetNecrosisWarning`/`TourniquetDestroyed` em PA-01-06.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** ícone ✈ adicionado a `EnTexts[13]`/`PtTexts[13]` (`ShoulderTapReceived`) no stub 4.

---

### PA-02-02 · C — Erro de Lógica · 🟢 Menor

**Citação `OnHealCheckReceived :676-716` não cobre o ponto real de escrita do response (`:730-738`) — assimetria com a citação de `TryAnswerForLocalBot`**

**Problema:** a rodada 1 (PA-01-05) corrigiu a citação de `TryAnswerForLocalBot` para `:857-894`, faixa que inclui corretamente a construção do `response` (escrita de `DenyReason`, real `:892`). A citação paralela de `OnHealCheckReceived`, corrigida para `:676-716`, para ANTES do ponto real onde o `response` é de fato construído — confirmado por leitura direta do arquivo real:
```csharp
// BandAidNetworkHandler.cs:709-717 (real, dentro da faixa citada :676-716):
var stats = ItemDatabase.GetStats(packet.ItemTemplateId);
bool approved = false;
string denyReason = "Item desconhecido.";
if (stats != null) { approved = MedicalLogic.CanUseItem(mainPlayer, stats); denyReason = approved ? "" : $"{stats.Name}: Sem ferimento compatível."; }
...
// :730-738 (real, FORA da faixa citada) — construção do response:
var response = new BandAidHealCheckResponsePacket
{
    ...
    DenyReason = denyReason,   // :736 — o ponto de escrita real, fora de :676-716
    ...
};
```
A faixa `:676-716` cobre a lógica de decisão, mas não a instanciação do `response` que efetivamente materializa `DenyReasonId` no pacote — o mesmo tipo de gap que a rodada 1 tinha corrigido no lado espelhado (`TryAnswerForLocalBot`).

**Por que importa:** não bloqueia (o bloco C.2 do checklist já cobre "migrar `BandAidHealCheckResponsePacket`... e os 2 pontos que a preenchem", e o texto é único/greppável dentro do mesmo método curto), mas mantém a mesma classe de inconsistência que a rodada 1 already flagged como reduzindo a confiabilidade da spec como guia linha-a-linha no arquivo de maior risco do item.

**Sugestão:** ampliar a citação de `OnHealCheckReceived` de `:676-716` para `:676-738` no stub 5 e no checklist §8 Bloco C.2, mantendo simetria com a citação já corrigida de `TryAnswerForLocalBot` (`:857-894`).

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** citação ampliada para `:676-738` no stub 5 e no checklist §8 Bloco C.2.

---

### PA-02-03 · C — Erro de Lógica · 🟢 Menor

**`package-release.sh`: `VER="$(pipeline)"` sob `set -euo pipefail` aborta silenciosamente antes do diagnóstico "ERRO: não achei `<Version>`"**

**Problema:** o stub 7 (script completo) tem:
```bash
set -euo pipefail
...
VER="$(grep -oE '<Version>[0-9][0-9.]*</Version>' "$MOD/modded/$MOD_NAME.csproj" | grep -oE '[0-9][0-9.]*' | head -1)"
[[ -n "$VER" ]] || { echo "ERRO: não achei <Version> no csproj do mod"; exit 1; }
```
Sob `set -e`, uma atribuição de topo `VAR="$(...)"` herda o exit status da substituição de comando: se o `.csproj` não tiver `<Version>...</Version>` (ou o formato divergir da regex), o pipeline `grep | grep | head -1` retorna código de saída não-zero via `pipefail` (o `head -1` sozinho retornaria 0 mesmo com entrada vazia, mas `pipefail` propaga a falha dos `grep`s intermediários) — e a própria linha de atribuição `VER="$(...)"` (não protegida por `if`/`&&`/`||`/negação) dispara `errexit` **imediatamente**, antes de a linha seguinte (`[[ -n "$VER" ]] || {...}`) sequer executar. O diagnóstico customizado nunca aparece; o script simplesmente morre sem mensagem.

Não é uma regressão nova desta spec — o mesmo padrão já existe, idêntico, no precedente `tools/trl-items-management/scripts/package-release.sh:35`. Mas como este item CRIA um script novo do zero, é uma oportunidade barata de não herdar o defeito.

**Por que importa:** não afeta a execução normal (o `.csproj` real tem `<Version>1.9.1</Version>` limpo, sem sufixo de prerelease, então o pipeline sempre terá sucesso hoje) — só se manifesta se o formato do `.csproj` mudar no futuro (ex.: alguém adicionar `-beta`/`-rc` à versão). Quando acontecer, o script falha sem explicar por quê, custando tempo de diagnóstico.

**Sugestão:** trocar para `VER="$(grep ... | grep ... | head -1 || true)"` (ou envolver a atribuição com `set +e`/`set -e`), preservando o guard `[[ -n "$VER" ]] || { echo "ERRO..."; exit 1; }` como o único ponto de saída com mensagem.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** `|| true` adicionado à atribuição de `VER` no stub 7, com comentário explicando o motivo.

---

### PA-02-04 · A — Gap · 🟢 Menor

**Checklist §8 não declara a cadeia de dependência implícita entre C.1→C.2→C.2b→C.3 nem a de D.1 sobre A-C — só A.1/A.2 recebe aviso explícito de ordem**

**Problema:** o checklist marca explicitamente "Bloco A.2 (ordem importa — fazer JUNTO com A.1, não depois)", mas não sinaliza duas outras dependências reais e igualmente presentes na lista:
- **C.2/C.2b/C.3 dependem de C.1** — `BandAidHealCheckPacket.cs`/`BandAidNetworkHandler.cs` (C.2) referenciam `MedicDenyReasonId`, que só existe depois de `MedicLocale.cs` (C.1) ser criado; `BandAidController.OnHealCheckResponseHandler` (C.3) depende tanto de `MedicLocale.GetDenyReasonText` (C.1) quanto do campo renomeado `response.DenyReasonId` (C.2). A ordem listada (C.1→C.2→C.2b→C.3) já é topologicamente válida, mas isso nunca é dito — diferente do aviso explícito em A.2.
- **D.1 depende implicitamente de A-C estarem completos** — o script criado em D.1 chama `compile-mod.sh`, que builda o ESTADO ATUAL do mod; se D.1 for executado (não só criado) antes de A-C terminarem, o zip gerado reflete um build parcial. A posição de D.1 como penúltimo item sugere isso, mas não é dito.

**Por que importa:** não bloqueia — a ordem como listada já está correta — mas a ausência de justificativa cria um sinal assimétrico: um implementador que note o aviso de A.2 e não veja aviso equivalente em C/D pode inferir (incorretamente) que só A.1/A.2 tem restrição de ordem, e reordenar C.3 antes de C.1/C.2 por preferência pessoal, quebrando a build no meio do processo (ainda que o estado final, se corrigido, compile).

**Sugestão:** adicionar uma nota curta no preâmbulo do §8 (ou inline em C.1) equivalente à de A.2: "C.2/C.2b/C.3 dependem de C.1 já existir (referenciam `MedicDenyReasonId`/`MedicLocale.GetDenyReasonText`)" e uma nota em D.1: "executar (não só criar) depois de A-C completos — o script builda o estado atual do mod".

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** notas de dependência de ordem adicionadas inline em C.1 e D.1 no checklist §8.

---

### PA-02-05 · C — Erro de Lógica · 🟢 Menor

**Comentário do stub `MedicLocale.cs` ("Common nunca chega aqui") é factualmente errado para `BandAidUI.ShowTreatment`**

**Problema:** o stub 4 traz o comentário:
```csharp
// ref: Assembly-CSharp/EBodyPart.cs:1-11 — 8 valores (Head..Common); Common nunca chega
// aqui (BandAidUI/TourniquetManager só chamam para membros concretos).
```
Isso é verdade para `TourniquetManager.cs` (só opera sobre membros que efetivamente têm torniquete), mas é **falso** para `BandAidUI`. Leitura real confirma 3 caminhos intencionais que alcançam `EBodyPart.Common`:
```csharp
// BandAidUI.cs:703-726 (real) — ShowTreatment chama PartLabel(part) INCONDICIONALMENTE
// na linha 711, ANTES do guard `if (part == EBodyPart.Common)` na linha 721:
public void ShowTreatment(EBodyPart part, string itemName)
{
    ...
    string label = PartLabel(part);        // :711 — Common chega aqui
    ...
    if (part == EBodyPart.Common) { ... }  // :721 — só depois
}
```
`BandAidController.cs:375` inicializa `_expectedTreatmentPart = EBodyPart.Common` como valor default, e `:581` chama `ShowTreatment(_expectedTreatmentPart, ...)` — se o membro-alvo nunca for resolvido (ex.: `MedicHealPatch.cs` linha ~390-394, `try { appliedPart = result.BodyPart; } catch { }` falhando silenciosamente), `Common` flui até `PartLabel`/`MedicLocale.BodyPartShort`. O fallback atual (`"..."`, preservado idêntico na migração) absorve isso sem quebrar — **não há bug de comportamento** — mas o comentário do stub, se copiado literalmente para o código novo, induz um futuro mantenedor a tratar o fallback como código morto e removê-lo, quebrando esse caminho intencional de UX.

**Por que importa:** não bloqueia nem muda comportamento hoje (o fallback `"..."` é idêntico ao `PartLabel` atual), mas é um risco de manutenção futura — comentário incorreto sobrevivendo à migração como "documentação enganosa".

**Sugestão:** corrigir o comentário no stub 4 para: `// Common CHEGA via BandAidUI.ShowTreatment (membro-alvo ainda não resolvido) — resolvido pelo fallback "..." abaixo, preservando o comportamento atual de PartLabel. TourniquetManager nunca passa Common (só opera sobre membros com torniquete ativo).`

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** comentário corrigido no stub 4, explicando o caminho real via `BandAidUI.ShowTreatment`.

---

### PA-02-06 · A — Gap · 🟢 Menor

**Checklist D.1 pede "dar permissão de execução se necessário" para um script que só é invocado via `bash script.sh`**

**Problema:** o item do checklist "Bloco D.1" diz: "Criar `mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` (stub 7 completo), dar permissão de execução se necessário." O próprio script, no seu comentário de uso (stub 7), documenta `Uso: bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh [OUTDIR]` — sempre invocado explicitamente via `bash`, nunca como executável direto (`./package-release.sh`). O bit de execução é irrelevante para esse modo de invocação.

**Por que importa:** não bloqueia, é só uma instrução supérflua no checklist que pode gerar um passo manual desnecessário (`chmod +x`) sem efeito prático.

**Sugestão:** remover a cláusula "dar permissão de execução se necessário" do item D.1, ou deixá-la explicitamente marcada como opcional/conveniência (não requisito).

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** cláusula removida do checklist §8 Bloco D.1 — script sempre invocado via `bash`, sem necessidade de bit de execução.

---

## Status

**Todos os 6 achados aplicados** em `010-migracao-release-02-spec-tech.md` (diretiva do usuário: aplicar todos os achados de review por padrão). 0 bloqueadores em nenhuma das 2 rodadas.

## Próximo passo

Spec técnica **pronta para `/code-mod`** — 2 rodadas de review técnica sem nenhum bloqueador remanescente (13 achados totais entre as 2 rodadas, todos aplicados).

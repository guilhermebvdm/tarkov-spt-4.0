# 023 — Coop-sync hardening (Fika PVE) · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./023-coop-sync-hardening-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) · [01-spec](./023-coop-sync-hardening-01-spec.md)<br>

---

## Abordagem por frente

### Frente A — Allowlist coop-safe no motor de sync

**Raiz confirmada no código.** `ScanExtras` resolve a regra por prefixo (`SyncPlanner.cs:229`) e, para `MirrorMoveDisabled`, gera `MoveToDisabled` com destino `<prefixo>-disabled/<remainder>` (`SyncPlanner.cs:253-262`, `BuildDisabledTarget` em `:316-321`). Os únicos escapes antes disso são os skips de `SyncPlanner.cs:222-227` (`manifestPaths`, `IsIgnored`, `IsExcludedFromCleanup`, `_protectedNormalized`, `ContainsDisabledSegment`). O fallback que classifica `plugins`/`patchers` como mirror está em `SyncRuleResolver.cs:32-35`. O manifesto do server é varredura recursiva de `mods_repo` (`ModUpdater.cs:319-329`): se o Fika client-only não está lá, é "extra" → quarentena.

**Design.** Novo predicado embutido `SyncCoopSafe.IsCoopEssentialPlugin(normalizedPath)` e mais um skip em `ScanExtras`, **imediatamente antes** do bloco `MirrorMoveDisabled` (logo após a resolução da regra em `SyncPlanner.cs:229`, junto aos demais skips), com um `Warning` no plano:

```
if (rule == SyncFolderRule.MirrorMoveDisabled
    && SyncCoopSafe.IsCoopEssentialPlugin(normalized))
{
    plan.Warnings.Add($"coop-safe: preservado plugin essencial fora do manifesto: {relative}");
    continue; // nunca vira MoveToDisabled
}
```

Colocar depois do `manifestPaths.Contains` (linha 223) garante RN-2: Fika no manifesto segue o fluxo normal (download/update). Colocar só para `MirrorMoveDisabled` garante CA-A3 (extras não-Fika continuam sendo movidos) e não toca `MirrorDelete`/`Default`.

**Matching (RN-1/CC-3).** `IsCoopEssentialPlugin` casa quando: o path normalizado está sob um prefixo mirror de plugins/patchers **e** o nome do arquivo (`Path.GetFileName`) casa a família Fika — proposta concreta: `startsWith("fika.", OrdinalIgnoreCase) && endsWith(".dll")`, mais uma lista explícita opcional de assemblies confirmados no G-023.2. Casamento por **nome de assembly**, não substring de caminho, para não pegar DLL de terceiros. A lista de padrões fica pública (`SyncCoopSafe.Patterns`) para os testes.

### Frente B — Op destrutiva ciente de coop

**Raiz confirmada.** Botões gated só por `CanStartGame` (`ProfileView.axaml:150,156,162`), que é `LauncherSettingsProvider.Instance.CanStartGame` = `!GameRunning && !IsUpdating` (`ProfileViewModel.cs:298`). `GameRunning` só vira `true` quando **este** launcher lança o EFT (`ProfileViewModel.cs:977`) — não enxerga clientes remotos. Logo o gate local é insuficiente por construção (01-spec CC-5).

**Design (2 camadas).**
1. **Aviso de coop (sempre entregável).** Acrescentar linha de aviso nas confirmações destrutivas:
   - EXCLUIR: `DeleteAccountDialogViewModel` (instanciado em `ProfileViewModel.cs:1101`) — já pede digitar o username; adicionar texto de coop.
   - WIPE / MUDAR EDIÇÃO: `ConfirmationDialogViewModel` (`WipeConfirmCommand` em `ProfileViewModel.cs:1082-1088`; ChangeEdition análogo) — passar a mensagem com o aviso de coop.
   - Alinhar o guard de retorno de Wipe/ChangeEdition ao padrão forte `is not bool confirmed || !confirmed` (já usado no delete, `ProfileViewModel.cs:1104`) para CA-B3 — o Wipe hoje usa `if (result is bool b && !b) return;` (`:1088`), que deixa `null`/não-bool prosseguir.
2. **Pré-check de sessão (condicional — G-023.3).** Se o server expõe raid/peers ativos, adicionar um request pré-flight em `DeleteAccountCommand`/`WipeProfile`/`ChangeEdition` **antes** do `RemoveAsync` (`ProfileViewModel.cs:1118`); com peers > 0, abortar com notificação. Sem endpoint, essa camada não entra e o item fica só na camada 1.

### Frente C — Erro de authkey distinguível + gate de reusabilidade

**Raiz confirmada.** `RunTailscaleUp` já captura `stderr` (`TailscaleHelper.cs:190,201,206`) e retorna `bool`; `EnsureTailscaleConnected` retorna `bool` (`:24,163`); `ConnectServerViewModel` mostra uma única mensagem genérica em qualquer falha (`ConnectServerViewModel.cs:90-99`). `--unattended` já está no comando (`TailscaleHelper.cs:179`) — o gap de RN-6 é a **propriedade da chave** (reusável?), não o comando.

**Design.**
- Introduzir `enum TailscaleConnectResult { Connected, AuthKeyRejected, NetworkFailure, NotInstalled }`. `RunTailscaleUp` classifica pela assinatura do `stderr` (tokens tipo `authkey`, `expired`, `single-use`, `already been used`, `invalid key`, `unauthorized`) → `AuthKeyRejected`; demais falhas → `NetworkFailure`. `EnsureTailscaleConnected` passa a devolver esse enum.
- `ConnectServerViewModel.cs:77-99`: mapear `AuthKeyRejected` → mensagem específica (RN-7), o resto mantém a mensagem atual (CA-C2). Sucesso = `Connected`; manter idêntico o pós-conexão (CA-C4).
- RN-6 (reusabilidade) permanece gate operacional (G-023.4) — não há verificação em código.

## Arquivos a tocar

| Arquivo | Mudança | Frente |
|---|---|---|
| `SPT.Launcher.Base/Sync/SyncCoopSafe.cs` *(novo)* | `IsCoopEssentialPlugin(normalizedPath)` + `Patterns` público | A |
| `SPT.Launcher.Base/Sync/SyncPlanner.cs` | novo skip em `ScanExtras` (~após `:229`, antes de `:253`) + `Warning` | A |
| `SPT.Launcher/ViewModels/ProfileViewModel.cs` | aviso de coop nas confirmações; guard forte no Wipe/ChangeEdition (`:1082-1090`); pré-check condicional antes de `RemoveAsync` (`:1118`) | B |
| `SPT.Launcher/Views/ProfileView.axaml` | tooltip/copy de coop nos 3 botões CONTA (`:147-166`) se necessário | B |
| `SPT.Launcher/ViewModels/DeleteAccountDialogViewModel.cs` | texto de aviso de coop | B |
| `SPT.Launcher/ViewModels/ConfirmationDialogViewModel.cs` (ou a string passada) | aviso de coop no Wipe/ChangeEdition | B |
| `SPT.Launcher/Helpers/TailscaleHelper.cs` | `enum TailscaleConnectResult`; classificação de `stderr` em `RunTailscaleUp` (`:170-217`); retorno de `EnsureTailscaleConnected` (`:24,163`) | C |
| `SPT.Launcher/ViewModels/ConnectServerViewModel.cs` | mapear resultado → mensagem (`:77-99`) | C |
| `SPT.Launcher.Tests/Sync/SyncCoopSafeTests.cs` *(novo)* + casos em `SyncPlannerTests.cs` | cobertura A | A |

## Contratos / DTOs

- **`SyncCoopSafe`** (novo, `SPT.Launcher.Base/Sync/`, mesmo namespace `SPT.Launcher.Sync`):
  - `public static bool IsCoopEssentialPlugin(string normalizedPath)` — recebe path já normalizado (usar `SyncPathUtil.Normalize`/`Path.GetFileName`); só considera paths sob prefixo de plugins/patchers.
  - `public static IReadOnlyList<string> Patterns { get; }` — expõe os padrões p/ teste e log.
- **`TailscaleConnectResult`** (novo enum em `SPT.Launcher.Helpers` ou junto ao helper): `Connected | AuthKeyRejected | NetworkFailure | NotInstalled`.
  - `EnsureTailscaleConnected()` muda de `Task<bool>` → `Task<TailscaleConnectResult>`. **Breaking** para o único caller (`ConnectServerViewModel.cs:77`); atualizar a checagem `if (tailscaleConnected)` → `if (result == Connected)`.
- **Plano/relatório:** reusa `plan.Warnings` (já existente, ex. `SyncPlanner.cs:167,249`) e o pipeline de `last-update.json` (`SyncReport`) — sem DTO novo. Sem baseline novo, sem ação nova de engine (a allowlist só **suprime** uma ação; não cria `SyncActionKind`).

## Riscos

- **R-1 (allowlist muito estreita).** Se um assembly Fika essencial não casar o padrão, o coop ainda quebra. Mitiga: G-023.2 confirma a lista exata inspecionando o `mods_repo`/instalação real; padrão de família `Fika.*` cobre o grosso.
- **R-2 (allowlist muito larga).** Preservaria lixo que deveria ser limpo. Mitiga: casar por nome de assembly Fika sob prefixo mirror, não substring de caminho; extras não-Fika intactos (CA-A3).
- **R-3 (quarentena pré-existente).** O fix não desfaz `plugins-disabled` já criado (CC-2) — precisa de passo manual único, documentado no gate.
- **R-4 (contrato do Tailscale muda o login).** Trocar `bool`→enum toca o caminho crítico de conexão; risco de regressão no login normal. Mitiga: CA-C4 + revisar o único caller.
- **R-5 (classificação de stderr frágil).** Tokens de erro do `tailscale` podem mudar entre versões; classificação errada volta a mensagem genérica (degradação segura, não quebra). Não parsear locale-dependente além dos tokens ASCII conhecidos.
- **R-6 (pré-check de coop sem endpoint).** Se o server não expõe presença, CA-B4 não é implementável agora — cai no aviso (RN-4). Registrar em G-023.3 para não virar dívida silenciosa.
- **R-7 (coop-gap residual).** Mesmo com RN-4, um host determinado ainda pode excluir a conta durante uma raid (é aviso, não trava). Só o pré-check (RN-5) fecha de verdade — por isso G-023.3 é decisão de produto, não cosmético.

## Plano de teste

**Unit (`SPT.Launcher.Tests/Sync/`, xUnit — projeto já existe):**
- `SyncCoopSafeTests.cs` (novo): `Fika.Core.dll` sob `BepInEx/plugins` → true; `Fika.Dedicated.dll` → true; `SomeOther.dll` → false; `Fika.Core.dll` sob `user/mods` (fora de prefixo mirror) → false; casing misto (`FIKA.core.DLL`) → true.
- `SyncPlannerTests.cs` (novos casos, no estilo de `Plugins_extra_is_planned_for_move_to_disabled` em `:143-153`):
  - `Fika_plugin_extra_is_never_quarantined` — escreve `BepInEx/plugins/Fika.Core.dll` local, manifesto sem ele → **nenhum** `MoveToDisabled` para o arquivo + `plan.Warnings` contém "coop-safe".
  - `NonFika_plugin_extra_still_quarantined` — regressão de CA-A3 (mantém `MoveToDisabled`).
  - `Fika_in_manifest_is_downloaded_not_preserved` — Fika no manifesto com hash divergente → `Download` (RN-2 / CA-A4).
  - `DevMode_preserves_fika_without_coopsafe_warning_conflict` — Dev Mode + Fika extra → `PreserveDevMode`, sem dupla ação (CA-A5).
- **Frente C:** teste da classificação de `stderr` — extrair a lógica de tokens para um método `static` puro testável (ex. `TailscaleHelper.ClassifyUpFailure(string stderr, int exitCode)`), com casos `authkey expired`→`AuthKeyRejected`, `control plane unreachable`→`NetworkFailure`, string vazia→`NetworkFailure`. (Sem tocar o `Process`.)
- **Frente B:** a lógica de dialog é UI-bound; cobrir com teste só se a decisão de confirmação for extraível para método puro. Caso contrário, validar por gate manual (G-023.5).

**Gates de build (nunca rodar o exe):** `dotnet build SPT.Launcher.csproj -c Release`, `dotnet test SPT.Launcher.Tests.csproj -c Release`, `dotnet build TarkovRedLine.Server.csproj -c Release`.

**Gates humanos (in-game / produção):** G-023.1 a G-023.5 da [01-spec](./023-coop-sync-hardening-01-spec.md) — escrita/movimento em `BepInEx/plugins` e remoção de `{id}.json` são efeitos em arquivos SPT: exigem validação no jogo com **segundo cliente real** (solo=host mascara), não só build verde.

## Nota de paralelismo (arquivos compartilhados com outros itens)

- **`ProfileViewModel.cs` — hub de 019–023.** Frente B edita `DeleteAccountCommand`/`WipeConfirmCommand`/`ChangeEdition` e o bloco de confirmação. Alta contenção: sequenciar com os outros itens que tocam o mesmo VM; preferir edições localizadas nesses comandos e evitar reflow do arquivo.
- **`ProfileView.axaml` — tela compartilhada (019–023).** Frente B mexe só no box CONTA (`:142-168`); manter o diff cirúrgico.
- **`TailscaleHelper.cs` + `ConnectServerViewModel.cs` — compartilhados com 006 (login-tailscale-sem-navegador).** Frente C sobrepõe diretamente o trabalho de 006 (mesmo helper, mesmo caller de conexão). **Coordenar/sequenciar com 006**: idealmente a mudança de contrato `bool`→`TailscaleConnectResult` entra junto com 006 para não haver dois refactors concorrentes no mesmo método.
- **Motor de sync (`SyncPlanner.cs`, `SyncRuleResolver.cs`, `SyncPlannerOptions.cs`) — base de 007/008/017 (já entregues).** Frente A é **aditiva** (novo arquivo + um skip em `ScanExtras`); baixa contenção hoje, mas `ScanExtras` é o mesmo método que 017 alterou — revisar que o novo skip não colide com o `continue` de seed/preserve em `SyncPlanner.cs:231-237`.
- **Dialog VMs (`DeleteAccountDialogViewModel`, `ConfirmationDialogViewModel`)** — compartilhados com 005/010 (senha/exclusão). Frente B só adiciona copy; não mudar a semântica de retorno que 010 depende.
- **Sem sobreposição com 024/025** (`Legacy.axaml`) nem com `OptionalModsHelper` (019/021) — 023 não toca esses.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec técnica das 3 frentes (allowlist coop-safe, op destrutiva ciente de coop, erro de authkey) com âncoras file:line verificadas. |

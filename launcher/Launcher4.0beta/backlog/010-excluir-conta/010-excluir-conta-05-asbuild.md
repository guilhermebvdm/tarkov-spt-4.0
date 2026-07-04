# 010 — Botão "Excluir conta" · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Insumo:** [01-spec](./010-excluir-conta-01-spec.md)

## Entregue

### Exclusão de conta

| Arquivo | Mudança |
|---|---|
| `project/SPT.Launcher/ViewModels/Dialogs/DeleteAccountDialogViewModel.cs` | **Novo.** `Username` (alvo), `TypedUsername` (two-way), `CanConfirm` = match case-sensitive com `Trim()` nas pontas. |
| `project/SPT.Launcher/Views/Dialogs/DeleteAccountDialogView.axaml` (+`.cs`) | **Novo.** `TrlDialogChrome` Title="EXCLUIR CONTA" + ✕ no head (`HeaderContent`); aviso `.trl-danger` + explicação excluir≠wipe; TextBox do username; ações à direita: CANCELAR `.ghost` + EXCLUIR DEFINITIVAMENTE `.danger` (habilita só com `CanConfirm`). Todos os fechamentos passam `Boolean` explícito ao `CloseDialogCommand` (✕/cancelar=False) — nunca `null`. |
| `project/SPT.Launcher/ViewModels/ProfileViewModel.cs` | + `DeleteAccountCommand`: dialog → `AccountManager.RemoveAsync()` → OK: limpa `AutoLoginCreds`+`SaveSettings`, `Logout()`, notificação sucesso, navega `LoginView` (`new LoginViewModel(HostScreen, true)`, padrão do logout) · NoConnection: notificação erro + `ConnectServerView` · demais: notificação erro, permanece. |
| `project/SPT.Launcher/CustomControls/TrlDialogChrome.cs` + `Assets/Theme/Controls/TrlCustomControls.axaml` | + `HeaderContent` (slot no head, docked à direita do título) — aditivo; usos existentes do chrome inalterados. Reusado pelo 005L p/ o ✕ dos dialogs legados. |

### Restyle da ProfileView (`Views/ProfileView.axaml`)

- **Sidebar** → `TrlSidebarNav` (280px do tema). Estilos inline `SidebarMenu`/`PanelCard`/`ActionButton`/`AltButton`/`PanelButton` (hex `#1A1A1A`/`#F2111111`/etc.) **removidos**; ficou apenas um bloco local token-pure `Button.nav` (gêmeo command-based do `ListBox.trl-nav`: barra 2px accent no ativo, wash tan no hover) — `trl-nav` do tema é selection-based (ListBox) e a sidebar é de comandos.
- **Painéis** Versão/Mods Opcionais/Conta → `TrlPanel` com títulos uppercase. Versão virou par de kv-rows (`trl-label` + valor `trl-mono trl-accent`); consome `ServerVersion` (dinâmico desde o 013L) e `LauncherUpdateHelper.CurrentVersion`.
- **Painel Conta**: MUDAR EDIÇÃO (outlined base) · RESETAR PROGRESSO (WIPE) `.danger` · **EXCLUIR CONTA `.danger`** (novo) + caption `trl-faint` explicando wipe≠excluir. Distinção visual wipe/excluir ficou **textual + caption** (prompt fixou `.danger` para ambos).
- **Barra inferior**: JOGAR = `.primary` (tan — R1, vermelho não é fill de trabalho), VERIFICAR ARQUIVOS = outlined base; XP e update usam `ProgressBar` do tema (overrides de cor removidos); textos → classes `trl-*`.
- Overlay `TrlPhotoOverlayBrush` adicionado sobre o BG (padrão das views já migradas).
- Bindings/commands preservados 1:1 (inclusive os dois primeiros botões da sidebar apontando para `OpenSettingsCommand`, como estava — não corrigi comportamento fora de escopo); único acréscimo: `DeleteAccountCommand`.

## Decisões / assunções

- **Gate de jogo em execução**: EXCLUIR CONTA desabilita via `IsEnabled={Binding CanStartGame}` (mesmo gate do JOGAR — cobre jogo rodando e update). Sessão coop remota (Fika) fora do alcance do launcher.
- **Confirmação**: username case-sensitive com `Trim()`; sem duplo diálogo.
- ✕/cancelar mandam `False` (bool) — `null` seria interpretado como "confirmado" por callers no padrão `if (result is bool b && !b) return;`.
- `AccountManager.Remove()` já anula `SelectedAccount`; `Logout()` é chamado mesmo assim (idempotente, explicita a intenção).

## Build

`dotnet build project/SPT.Launcher/SPT.Launcher.csproj` → **0 erros** (warnings pré-existentes; alguns de `ClassSelectionViewModel` são do item 004L em curso paralelo).

Validação visual/runtime (dialog, restyle, fluxo excluir→login) fica com o orquestrador.

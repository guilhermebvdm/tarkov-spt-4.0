# 010 — Botão "Excluir conta" · Spec (funcional + técnica)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [010-excluir-conta-00-kickoff.md](./010-excluir-conta-00-kickoff.md)

## Objetivo

Excluir a conta **definitivamente** no server a partir da ProfileView, com confirmação forte (digitar o username). Distinto do wipe: wipe reseta o progresso e mantém a conta; excluir remove conta + perfil do server.

## Backend (já existe — nada a fazer)

- `POST /launcher/profile/remove` (SPT core) — remove o perfil; resposta `true/false`.
- Cliente: `RequestHandler.RequestRemove()` → `AccountManager.Remove()/RemoveAsync()` (já zera `SelectedAccount` no sucesso).

## Fluxo

1. ProfileView, painel **Conta** (ao lado do wipe): botão `EXCLUIR CONTA` (`.danger`).
2. Abre `DeleteAccountDialogView` (novo, `TrlDialogChrome`): explica a diferença excluir≠wipe, exige digitar o **username exato** num TextBox; o botão confirmar (`.danger`) só habilita com match.
3. Confirmado → `AccountManager.RemoveAsync()`:
   - **OK** → limpa `AutoLoginCreds` + `SaveSettings()` (senão o auto-login tentaria uma conta inexistente), `AccountManager.Logout()` (idempotente — `Remove()` já anula), notificação de sucesso, navega para `LoginView` (`new LoginViewModel(HostScreen, true)`, mesmo padrão do logout).
   - **NoConnection** → notificação de erro + navega para `ConnectServerView` (padrão do `RemoveProfileCommand` existente).
   - **UpdateFailed/qualquer outro** → notificação de erro, permanece na ProfileView (conta intacta).
4. Cancelar/fechar o dialog → nada acontece.

## Confirmação forte (decisões)

- **Digitar o username** (não duplo diálogo): fricção proporcional, padrão da indústria p/ ação irreversível.
- Match **case-sensitive**, com `Trim()` nas pontas (espaço acidental não pune; caixa errada não passa).
- Botão confirmar `.danger` com texto inequívoco (`EXCLUIR DEFINITIVAMENTE`); cancelar `.ghost`.
- Fechar pelo ✕ do chrome = cancelar (CommandParameter `False` — **nunca** `null`, que os callers de `ConfirmationDialog` interpretam como "não-cancelou").

## Corner cases

| Caso | Comportamento |
|---|---|
| Jogo em execução (conta logada em jogo) | Botão `EXCLUIR CONTA` desabilitado via `IsEnabled={Binding CanStartGame}` (mesmo gate do JOGAR — cobre jogo rodando e update em andamento). Sessão coop/Fika remota: fora de escopo do launcher; o server responde o que responder. |
| Falha de rede | `NoConnection` → notificação + ConnectServerView. Conta permanece no server. |
| Server responde `false` | `UpdateFailed` → notificação de erro, fica na tela. |
| Auto-login configurado | Credenciais limpas no sucesso (ver fluxo 3). |
| Username com espaços nas pontas | `Trim()` no campo digitado antes do match. |

## UI — restyle da ProfileView (escopo deste item)

- Sidebar → `TrlSidebarNav` (chrome do tema; remove os estilos inline `SidebarMenu`/hex `#1A1A1A` duplicados com a SettingsView). Botões de nav com estilo local **token-pure** espelhando `ListBox.trl-nav` (barra de 2px à esquerda no ativo, wash tan no hover).
- Painéis Versão/Mods Opcionais/Conta → `TrlPanel` (substitui `Border.PanelCard` `#F2111111`).
- Barra inferior: `JOGAR` = `.primary` (tan — regra R1, vermelho nunca é fill de trabalho), `Verificar arquivos` = outlined base; Wipe/Excluir = `.danger` no painel Conta (distinção textual + caption explicativa).
- XP/update → `ProgressBar` do tema (remove overrides de cor).
- `TrlDialogChrome` ganha `HeaderContent` (slot no head, à direita do título) para hospedar o botão ✕ — aditivo, sem quebrar usos existentes; o 005L reusa nos dialogs legados.
- Nenhum binding/command existente muda além do novo `DeleteAccountCommand`.

## Arquivos

| Arquivo | Mudança |
|---|---|
| `project/SPT.Launcher/Views/Dialogs/DeleteAccountDialogView.axaml` (+`.cs`) | **Novo** — dialog de confirmação forte. |
| `project/SPT.Launcher/ViewModels/Dialogs/DeleteAccountDialogViewModel.cs` | **Novo** — `Username`, `TypedUsername`, `CanConfirm`. |
| `project/SPT.Launcher/ViewModels/ProfileViewModel.cs` | + `DeleteAccountCommand`. |
| `project/SPT.Launcher/Views/ProfileView.axaml` | Botão novo + restyle (acima). |
| `project/SPT.Launcher/CustomControls/TrlDialogChrome.cs` + `Assets/Theme/Controls/TrlCustomControls.axaml` | + `HeaderContent` (slot p/ ✕). |

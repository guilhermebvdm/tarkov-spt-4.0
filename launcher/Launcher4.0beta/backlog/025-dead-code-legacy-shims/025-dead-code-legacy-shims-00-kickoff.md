# 025 — Aposentar código morto + fechar shims Legacy · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (DS órfãos + código-client) · **Severidade:** 🟡 · **Deps:** 014, 024

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Achados
- **5 custom controls órfãos** (código morto, não instanciados por nenhuma view): `ProfileCard`, `DetailedProfileCard`, `TotalModsCard`, `GameLaunchBar`, `LoginBox` (com literais tipo `IndianRed`/`Gray`).
- **Código morto com risco próprio:** `WireGuardHelper` (bypass TLS `:153`, `WaitForExit` bloqueante), `FikaConfigHelper`, `ProfileViewModel.GameVersionCheck`.
- **`ModInfoView`/`ModInfoCard`/`TotalModsCard`** presos em classes legadas `.card/.acc/.alt` (alcançáveis via `OpenModsInfoCommand`) → sustentam os shims do `Legacy.axaml` que o item 014 deveria ter removido.
- **Cores da notificação** são nomes crus do Avalonia (`SPTNotificationViewModel.cs:22,27,32,37,42`).

## Correlatos 🟢
`ImageSourceConverter` decodifica bitmap na UI thread e não descarta os antigos (`:29`); `ModUpdateView.axaml:47` radius 4; falta token `TrlFgOnDanger` (literal `White` em `Button.axaml`/`TitleBar.axaml`); `GetExistingProfiles` popula coleção que a LoginView não renderiza.

## Critérios de aceite (seed)
- Controls órfãos e helpers mortos removidos (ou migrados se realmente planejados).
- `ModInfoView` migrada p/ `cc:TrlPanel` + tokens; shims `.card/.acc/.alt` do `Legacy.axaml` removidos (**fecha o débito do 014**).
- Cores de notificação via tokens; radius 0 em `ModUpdateView`; `ImageSourceConverter` com dispose/off-thread.

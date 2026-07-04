# 025 — Aposentar código morto + fechar shims Legacy · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./025-dead-code-legacy-shims-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Objetivo

Reduzir a superfície do launcher removendo **código comprovadamente morto** e **fechar o débito do item 014**: migrar os últimos consumidores das classes legadas `.card` / `.acc` / `.alt` para o Design System (DS) token-puro e apagar os shims correspondentes no `Assets/Theme/Controls/Legacy.axaml`. No mesmo passe, resolver os correlatos DS que a auditoria marcou como 🟢 e que dependem dos mesmos arquivos (cores de notificação, radius, converter de imagem, texto on-danger).

Regra de ouro: **nada é removido por suposição** — só o que grep prova ter zero instanciação/call-site fora da própria definição.

## Inventário (fonte da verdade — file:line conferidos no código)

| Item | Onde | Prova de morte | Ação |
|---|---|---|---|
| `ProfileCard` | `CustomControls/ProfileCard.axaml(.cs)` | grep só acha auto-definição + `build_*.txt` | **Deletar** |
| `DetailedProfileCard` | `CustomControls/DetailedProfileCard.axaml(.cs)` | idem | **Deletar** |
| `TotalModsCard` | `CustomControls/TotalModsCard.axaml(.cs)` | idem; único portador de `OpenModsInfoCommand` fora da VM | **Deletar** |
| `GameLaunchBar` | `CustomControls/GameLaunchBar.axaml(.cs)` | idem | **Deletar** |
| `LoginBox` | `CustomControls/LoginBox.axaml(.cs)` (literais `Gray`) | idem | **Deletar** |
| `WireGuardHelper` | `Helpers/WireGuardHelper.cs` | 0 call-sites (`WireGuardHelper.` = 0); bypass TLS + `WaitForExit` bloqueante | **Deletar** |
| `FikaConfigHelper` | `Helpers/FikaConfigHelper.cs` | 0 call-sites (`FikaConfigHelper.` = 0) | **Deletar** |
| `GameVersionCheck` | `ViewModels/ProfileViewModel.cs:388-410` | método `private`, 0 chamadas (só a definição) | **Deletar** |
| `ModInfoView` | `Views/ModInfoView.axaml:22,28,36,60,66,74` | usa `.card/.acc/.alt` | **Migrar** p/ `cc:TrlPanel` + tokens |
| `ModInfoCard` | `CustomControls/ModInfoCard.axaml:8,79` | usa `.card/.acc` | **Migrar** p/ tokens |
| Shims `.card/.acc/.alt` | `Legacy.axaml:11-16,19-26,70-78` | consumidores acima | **Remover** após migração |

Correlatos DS (mesmos arquivos, entram no escopo):

| Correlato | Onde | Ação |
|---|---|---|
| Cores de notificação = nomes crus Avalonia | `ViewModels/Notifications/SPTNotificationViewModel.cs:22,27,32,37,42` | mapear p/ tokens `Trl*` |
| Radius ≠ 0 em view migrada | `Views/ModUpdateView.axaml:47` (`CornerRadius="4"`) | → `0` |
| Texto on-danger literal `White` | `Assets/Theme/Controls/Button.axaml:70,76,81` + `CustomControls/TitleBar.axaml:90` | novo token `TrlFgOnDanger` |
| `ImageSourceConverter` decodifica na UI thread e não reaproveita bitmaps | `Converters/ImageSourceConverter.cs:29` | memoizar por path (ver corner case) |

## Critérios de aceite (Given/When/Then — testáveis)

### Grupo A — Código morto removido

- [ ] **AC-1** — **Dado** que os 5 custom controls órfãos não são instanciados por nenhuma view (grep dos nomes retorna só `x:Class`/`AvaloniaProperty.Register` das próprias definições + logs de build), **quando** os pares `.axaml` + `.axaml.cs` forem apagados, **então** `dotnet build SPT.Launcher.csproj -c Release` fica verde **e** abrir Login/Register/Profile/Settings/ClassSelection não emite nenhum XAML binding/parse error no console.
- [ ] **AC-2** — **Dado** `WireGuardHelper` e `FikaConfigHelper` sem call-sites, **quando** removidos, **então** build verde **e** grep por `WireGuardHelper`/`FikaConfigHelper` retorna 0 hits fora de `versions-git/`, `build_*.txt` e `.archived/`.
- [ ] **AC-3** — **Dado** `ProfileViewModel.GameVersionCheck()` (`:388-410`) sem chamadas, **quando** o método for removido, **então** build verde **e** grep por `GameVersionCheck` = 0.

### Grupo B — Shims Legacy fechados (débito do 014)

- [ ] **AC-4** — **Dado** `ModInfoView.axaml`, **quando** migrada, **então** não contém mais `Classes="card"|"acc"|"alt"`; os dois cabeçalhos usam `cc:TrlPanel` e os textos usam `trl-accent`/`trl-muted` (ou tokens `Trl*` equivalentes); a tela continua listando mods ativos/inativos com o mesmo conteúdo.
- [ ] **AC-5** — **Dado** `ModInfoCard.axaml`, **quando** migrado, **então** o card raiz não usa `Border.card` (usa `cc:TrlPanel` ou `Border` com `TrlBgPanelBrush`/`TrlEdgeBrush`/radius 0) e o rótulo de versão não usa `Classes="acc"` (usa `trl-accent`/`TrlAccentBrush`).
- [ ] **AC-6** — **Dado** que os consumidores de `.card/.acc/.alt` sumiram, **quando** os seletores `Border.card` (`Legacy.axaml:19-26`), `TextBlock.acc`/`TextBlock.alt` (`:11-16`), `Label.acc`/`Label.alt` (`:70-75`) e `Label.versionMismatch` (`:76-78`) forem removidos, **então** grep global por `Classes="card"`, `Classes="acc"`, `Classes="alt"`, `Classes.acc`, `Classes.versionMismatch` = 0 hits em `project/` (só sobrevivem `trl-*`).
- [ ] **AC-7** — **Dado** que shims **vivos** não fazem parte deste item, **quando** o Legacy.axaml for podado, **então** permanecem intactos e funcionais: `WindowNotificationManager`/`NotificationCard` (`:29-52`), `cc|TitleBar` (`:55-64`), o seletor base `Label` (`:67-69`) e `Separator` (`:81-84`) — e `LoginView.axaml:56` / `RegisterView.axaml:55` continuam exibindo o separador.

### Grupo C — Correlatos DS

- [ ] **AC-8** — **Dado** `SPTNotificationViewModel`, **quando** as 5 cores forem trocadas, **então** `BarColor` vem de tokens do tema (mapa: Information→`TrlAccentBrush`, Warning→`TrlWarningBrush`, Success→`TrlSuccessBrush`, Error→`TrlDangerStrongBrush`, default→`TrlFgMutedBrush`) e nenhum `Colors.DodgerBlue/Gold/ForestGreen/IndianRed/Gray` permanece no arquivo.
- [ ] **AC-9** — **Dado** `ModUpdateView.axaml:47`, **quando** editado, **então** `CornerRadius="0"`.
- [ ] **AC-10** — **Dado** o novo token `TrlFgOnDanger` em `Tokens.axaml`, **quando** referenciado, **então** `Button.axaml:70,76,81` e `TitleBar.axaml:90` usam `{DynamicResource TrlFgOnDanger}` e não o literal `White`; os botões `.danger` continuam com texto legível (contraste ≥ AA para texto bold).
- [ ] **AC-11** — **Dado** `ImageSourceConverter`, **quando** memoizado por path, **então** duas avaliações do mesmo path retornam a **mesma** instância de `Bitmap` (sem re-decodificar) e trocar o fundo em Settings não gera hitch perceptível.

## Regras de negócio

- **RN-1 — Prova antes de deletar.** "Morto" = zero instanciação/call-site fora da própria definição, confirmado por grep no commit em que se deleta. Se aparecer 1 consumidor vivo, o item NÃO deleta — reclassifica.
- **RN-2 — Ordem migrar→podar.** Um shim do Legacy.axaml só é removido **depois** que seu último consumidor foi migrado/deletado no mesmo diff (senão binding quebra em runtime, não em build).
- **RN-3 — Shim vivo fica.** O item fecha **apenas** `.card/.acc/.alt` (+ `versionMismatch`, órfão). Notification chrome, TitleBar, base `Label` e `Separator` são shims vivos e permanecem.
- **RN-4 — Radius 0 é lei do tema** (R2 do DS): qualquer view migrada tem cantos agudos.
- **RN-5 — Token, não literal.** Cor nova nasce como token `Trl*` em `Tokens.axaml`; XAML/VM referenciam o token, nunca o hex/nome cru.

## Corner cases

- **CC-1 — `ModInfoView` está inalcançável hoje.** `OpenModsInfoCommand` só existe em `TotalModsCard` (órfão, será deletado) e em `ProfileViewModel:412-413`; **nenhuma view viva** faz binding dele. O `ProfileView` reserva um botão de nav "LISTA DE MODS" **desabilitado** ("Em construção", `ProfileView.axaml:77-82`). Logo, migrar a tela é pré-requisito para religá-la, mas religar é decisão de produto → **GATE-P1**.
- **CC-2 — `ProgressBar.error`** (`Legacy.axaml:87-99`): grep por `Classes="error"` = 0 consumidores → parece morto, mas está **fora** do escopo `.card/.acc/.alt`. Não remover neste item sem verificação extra (pode ser adicionado via code-behind). Registrar como candidato do próximo passe.
- **CC-3 — `ImageSourceConverter` é síncrono.** `IValueConverter.Convert` não é `async`; "decodificar off-thread" literal não cabe. A correção efetiva do churn é **memoizar por path** (o converter é chamado por várias telas vivas — ClassSelection/Login/Register/Settings/Profile/MainWindow). O conjunto de paths é limitado (imagens de facção, ícones de classe, poucos fundos), então o cache é limitado por construção.
- **CC-4 — Notificação em design-time/headless.** Se `Application.Current` ou o recurso não resolver (preview do designer, teste headless), o mapeamento de token precisa de fallback não-nulo (`TrlFgMutedBrush` embutido) para não lançar.

## Fora de escopo

- **B3 — migração da `SettingsView` inteira** ao DS (item próprio, não é este).
- **Bloqueadores B1 (RCE auto-update) e B2 (`deleteFiles` traversal)** — segurança, itens próprios.
- **Refactor do `OptionalModsHelper`** (traversal/atômico/base-URL) — pertence a 019/021.
- **Religar de fato o nav "LISTA DE MODS"** — só entra se **GATE-P1** aprovar; default é migrar e manter a tela existente sem alterar navegação.
- **Remover `ProgressBar.error`** (ver CC-2).

## Gates

- **GATE-P1 (produto — humano).** Decidir o destino de `ModInfoView`/`ModInfoCard`:
  - **(a) default recomendado** — migrar aos tokens e **manter** a tela como está (inalcançável por ora), preservando a intenção do botão "LISTA DE MODS"; religar o nav fica para item futuro;
  - **(b)** migrar **e** religar o nav "LISTA DE MODS" → `OpenModsInfoCommand` agora (vira feature viva, exige QA da tela);
  - **(c)** **deletar** a feature inteira (`ModInfoView` + `ModInfoCard` + `ModInfoViewModel` + `OpenModsInfoCommand` + o botão desabilitado) — fecha os shims com menos código, mas descarta a tela de mods.
  Todas as três fecham os shims `.card/.acc/.alt`; a escolha é de produto.
- **GATE-BUILD.** `dotnet build SPT.Launcher.csproj -c Release` **e** `dotnet test SPT.Launcher.Tests.csproj -c Release` verdes. **Nunca** rodar via exe durante o build.
- **GATE-INGAME (obrigatório).** Escrita/renderização exigem validação **no launcher rodando**, não só build. Publicar e conferir visualmente: (1) Login/Register/Profile/Settings/ClassSelection abrem sem binding error; (2) as 4 notificações (Info/Warning/Success/Error) aparecem com as cores do tema; (3) a tela de mods-info (se mantida) renderiza migrada e legível; (4) `ModUpdateView` com cantos agudos; (5) trocar o fundo em Settings várias vezes sem hitch nem crescimento anômalo de memória; (6) botões `.danger` (Wipe/Excluir) com texto legível.
- **GATE-COOP (Fika PVE).** Mudança é UI/dead-code, sem toque no motor de sync nem em escrita de arquivos SPT → impacto de coop esperado nulo. Ainda assim, confirmar que notificações disparadas durante um sync em sessão coop seguem exibindo cor/target corretos (solo=host mascara; validar com um cliente extra se possível).

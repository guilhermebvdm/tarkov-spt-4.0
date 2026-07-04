# 004 — Tela de classes: dados reais · As-built

**Launcher:** Launcher4.0beta · **Status:** Entregue (build verde; E2E com server real pendente — gate humano)
**Data:** 2026-07-04
**Specs:** [01-spec](./004-classes-dados-reais-01-spec.md) · [02-spec-tech](./004-classes-dados-reais-02-spec-tech.md) · Kickoff: [00-kickoff](./004-classes-dados-reais-00-kickoff.md) · Rota server: as-built 058 do CustomClasses

> **Desvio de processo:** review de spec-tech (03) fundida na code-review pós-código (04), por velocidade do run autônomo — registrado na 02.

## Entregue

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `project/SPT.Launcher.Base/Models/TRL/ClassInfo.cs` | **novo** | DTOs `ClassInfo` + `LocalizedPair` (contrato SP0; `[JsonProperty]` camelCase; ausente=null; skills int / multipliers double sem render — uso futuro) |
| `project/SPT.Launcher.Base/Controllers/RequestHandler.cs` | editado | `RequestClassList()` = `GetJson("/customclasses/classes")` — infra existente descomprime zlib (exigência 058 §9 / P-058.4) |
| `project/SPT.Launcher.Base/MiniCommon/ImageRequest.cs` | editado | `CacheServerImage(route, fileName)` público: GET raw no backend CONECTADO (`RequestHandler.GetBackendUrl()`), cache em `Image_Cache/`, retorna path local ou null |
| `project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs` | **reescrito** | Mock removido (`LoadMockClasses` + índice `[3]`); load async pós-ativação (`WhenActivated`→`Task.Run`, mutações via `Dispatcher.UIThread.Post`, guard anti-reativação); dedupe defensivo por `editionKey`; fallbacks PT→EN→key/descriptions; ícones cacheados em thread de fundo; `nameColor`→`ImmutableSolidColorBrush` (thread-safe); `SelectedClass` = primeira da lista; registro envia `SelectedClass.EditionKey`; **fix D1** (`// ref: 005-D1`); fallback vanilla `editions[]`+`profileDescriptions{}` com Warning |
| `project/SPT.Launcher/Views/ClassSelectionView.axaml` | **reescrito** | Restyle TRL: `bg-hero.jpg`+`TrlPhotoOverlayBrush`; sidebar 300px `TrlPanelOverPhotoBrush` com `ListBox.trl-nav` (ícone 24px + nome; nameColor via 2 TextBlocks alternados p/ não sobrescrever foreground por estado); "Carregando classes..."; erro `trl-danger`; botões `.primary`/`.ghost`; `TrlVersionFooter` (defaults; 013L liga o dado); detalhe = `TrlPanel` (ShowHeader=False) + `TrlScreenBar` (nome uppercase) + descrição PT com wrap. **Removidos:** Vantagens/Desvantagens/Habilidades, painel "[Imagem do Personagem]", footer hardcoded, estilos locais com hex, `bg2.png`. Zero hex novo — só `{DynamicResource Trl*}` |

Fluxo de navegação/commands intacto (Voltar→Register; sucesso→auto-login→Profile; falha→Login). `ServerManager.cs` só leitura. Views de outros tracks não tocadas. Sem git.

## Fix D1 (item 005, escopo deste item)

Após `RegisterAsync == OK` e ANTES do auto-login: `await AccountManager.ChangePasswordAsync(_password)` (se senha não-vazia) — semeia o cofre `/redline/password/change` com a senha digitada no registro (o core cria a conta sem senha). Pré-condição OK: `Register` OK implica `SelectedAccount` populado (o `Register` interno já faz `Login`). Falha na troca → Warning no log + notificação "poderá definir no próximo login" + fluxo segue (comportamento pré-fix).

## Assunções registradas

1. **A1** — `Register()` OK ⇒ `SelectedAccount` setado (via `Login` interno em `AccountManager.Register:147`); é a pré-condição do `ChangePassword`. Verdadeiro no código atual.
2. **A2** — Static files (ícones) saem crus do server (as-built 058: `iconUrl` abre no browser) → `Request.Send` sem decompress; JSON da rota sai zlib → `GetJson` padrão.
3. **A3** — Legibilidade de `nameColor` sobre o tema grafite é responsabilidade do autor da classe; cor inválida → Warning + foreground padrão do `trl-nav`.
4. **A4** — `ImmutableSolidColorBrush` para brush criado fora da UI thread (binding lê na UI thread).
5. **A5** — Ícone cacheado como `Image_Cache/class_<basename do iconUrl>.png`; colisão de basename entre classes diferentes sobrescreveria o cache — aceito (ícones do 058 têm nomes únicos por classe).
6. **A6** — Sem `SelectedServer` E rota falha → lista vazia + mensagem na tela; botão protegido por guard `SelectedClass == null`.
7. **A7** — `RequestHandler.cs` é compartilhado com o track 013L (que adicionou `RequestTrlServerVersion` em paralelo); os métodos não conflitam.

## Build (gate)

```
dotnet build launcher/Launcher4.0beta/project/SPT.Launcher/SPT.Launcher.csproj -c Release
  146 Aviso(s)  ← todos pré-existentes (ModUpdate/Profile/Tailscale); ZERO nos arquivos deste item
  0 Erro(s)
```

Exe não executado (proibição do run autônomo).

## Pendências (gates humanos)

- **P-004.1** — E2E com server real: 7 classes listadas (nomes/cores/ícones/descrições PT), seleção default = primeira, registro cria perfil com a edition correta (verificar no server), auto-login → Profile. Cobre o DoD do kickoff.
- **P-004.2** — Validar fix D1 em jogo: registrar conta nova → logout → login manual com a senha do registro deve passar SEM o dialog de criar senha.
- **P-004.3** — Validar fallback: server sem o mod (ou rota desligada) → lista vanilla sem crash.
- **P-004.4** — Validação visual do restyle (grafite 8fa0190): contraste dos `nameColor` das 7 classes sobre `TrlPanelOverPhotoBrush`.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Claude | Criação (entrega do item 004L). |

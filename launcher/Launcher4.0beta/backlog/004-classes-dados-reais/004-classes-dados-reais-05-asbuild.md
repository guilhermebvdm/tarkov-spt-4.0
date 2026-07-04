# 004 — Tela de classes: dados reais · As-built

**Launcher:** Launcher4.0beta · **Status:** Entregue + review 01 aplicada (build verde; E2E com server real pendente — gate humano)
**Data:** 2026-07-04 (rev. 2 — pós /apply-code-review)
**Specs:** [01-spec](./004-classes-dados-reais-01-spec.md) · [02-spec-tech](./004-classes-dados-reais-02-spec-tech.md) · Review: [04-code-review-01](./004-classes-dados-reais-04-code-review-01.md) · Kickoff: [00-kickoff](./004-classes-dados-reais-00-kickoff.md) · Rota server: as-built 058 do CustomClasses

> **Desvio de processo:** review de spec-tech (03) fundida na code-review pós-código (04), por velocidade do run autônomo — registrado na 02.

## Entregue

| Arquivo | Ação | Conteúdo |
|---|---|---|
| `project/SPT.Launcher.Base/Models/TRL/ClassInfo.cs` | **novo** | DTOs `ClassInfo` + `LocalizedPair` (contrato SP0; `[JsonProperty]` camelCase; ausente=null; skills int / multipliers double sem render — uso futuro) |
| `project/SPT.Launcher.Base/Controllers/RequestHandler.cs` | editado | `RequestClassList()` = `GetJson("/customclasses/classes")` — infra existente descomprime zlib (exigência 058 §9 / P-058.4) |
| `project/SPT.Launcher.Base/MiniCommon/ImageRequest.cs` | editado | `CacheServerImage(route, fileName)` público: GET raw no backend CONECTADO (`RequestHandler.GetBackendUrl()`), cache em `Image_Cache/`, retorna path local ou null. **Pós-review:** sanitização interna do `fileName` (CR-01-06), chave de sessão prefixada pelo backend (CR-01-05), `CachedRoutes`→`ConcurrentDictionary` + lock por rota em `CacheServerImage` E `CacheImage`, callers vanilla preservados (CR-01-04) |
| `project/SPT.Launcher/ViewModels/ClassSelectionViewModel.cs` | **reescrito** | Mock removido (`LoadMockClasses` + índice `[3]`); load async pós-ativação (`WhenActivated`→`Task.Run`, mutações via `Dispatcher.UIThread.Post`, guard anti-reativação); dedupe defensivo por `editionKey`; fallbacks PT→EN→key/descriptions; `nameColor`→`ImmutableSolidColorBrush` (thread-safe); `SelectedClass` = primeira da lista; registro envia `SelectedClass.EditionKey`; **fix D1** (`// ref: 005-D1`); fallback vanilla `editions[]`+`profileDescriptions{}` com Warning. **Pós-review:** `LoadClassesAsync` com try/catch de última instância + `finally` garantindo o Post (CR-01-01); guard antes de limpar `RegisterErrorMsg` (CR-01-02); ícones em paralelo via `Task.WhenAll` em `BuildFromServerAsync` — pior caso ~1 timeout, não 7 em série (CR-01-03); `Task.Run` aninhado removido (CR-01-08.1) |
| `project/SPT.Launcher/Views/ClassSelectionView.axaml` | **reescrito** | Restyle TRL: `bg-hero.jpg`+`TrlPhotoOverlayBrush`; sidebar 300px `TrlPanelOverPhotoBrush` com `ListBox.trl-nav` (ícone 24px + nome; nameColor via 2 TextBlocks alternados p/ não sobrescrever foreground por estado); "Carregando classes..."; erro `trl-danger`; botões `.primary`/`.ghost`; `TrlVersionFooter` (defaults; 013L liga o dado); detalhe = `TrlPanel` (ShowHeader=False) + `TrlScreenBar` (nome uppercase) + descrição PT com wrap. **Removidos:** Vantagens/Desvantagens/Habilidades, painel "[Imagem do Personagem]", footer hardcoded, estilos locais com hex, `bg2.png`. Zero hex novo — só `{DynamicResource Trl*}` |

Fluxo de navegação/commands intacto (Voltar→Register; sucesso→auto-login→Profile; falha→Login). `ServerManager.cs` só leitura. Views de outros tracks não tocadas. Sem git.

## Fix D1 (item 005, escopo deste item)

Após `RegisterAsync == OK` e ANTES do auto-login: `await AccountManager.ChangePasswordAsync(_password)` (se senha não-vazia) — semeia o cofre `/redline/password/change` com a senha digitada no registro (o core cria a conta sem senha). Pré-condição OK: `Register` OK implica `SelectedAccount` populado (o `Register` interno já faz `Login`). Falha na troca → Warning no log + notificação "poderá definir no próximo login" + fluxo segue (comportamento pré-fix).

## Assunções registradas

1. **A1** — `Register()` OK ⇒ `SelectedAccount` setado (via `Login` interno em `AccountManager.Register:147`); é a pré-condição do `ChangePassword`. Verdadeiro no código atual.
2. **A2** — Static files (ícones) saem crus do server (as-built 058: `iconUrl` abre no browser) → `Request.Send` sem decompress; JSON da rota sai zlib → `GetJson` padrão.
3. **A3** — Legibilidade de `nameColor` sobre o tema grafite é responsabilidade do autor da classe; cor inválida → Warning + foreground padrão do `trl-nav`.
4. **A4** — `ImmutableSolidColorBrush` para brush criado fora da UI thread (binding lê na UI thread).
5. **A5** — Ícone cacheado como `Image_Cache/class_<basename do iconUrl>.png`; colisão de basename entre classes diferentes sobrescreveria o cache — aceito (ícones do 058 têm nomes únicos por classe). Stale entre servers na MESMA sessão fechado pelo CR-01-05 (chave prefixada pelo backend).
6. **A6** — Sem `SelectedServer` E rota falha → lista vazia + mensagem na tela; botão protegido por guard `SelectedClass == null` (mensagem preservada no clique — CR-01-02).
7. **A7** — `RequestHandler.cs` é compartilhado com o track 013L (que adicionou `RequestTrlServerVersion` em paralelo); os métodos não conflitam.
8. **A8 (apply)** — CR-01-03 resolvido com `Task.WhenAll` (opção de menor risco indicada pela review): perfis não estão bound à UI durante os downloads paralelos e o `await` do `WhenAll` estabelece visibilidade de memória antes do publish — sem necessidade de `ClassProfile` reativo.
9. **A9 (apply)** — Lock POR ROTA no `ImageRequest` (não global): preserva o paralelismo do CR-01-03 entre ícones diferentes e serializa apenas escritores do mesmo arquivo.

## Build (gate — re-rodado pós-apply)

```
dotnet build launcher/Launcher4.0beta/project/SPT.Launcher/SPT.Launcher.csproj -c Release
  0 Erro(s)
```

Warnings CS86xx (nullable) agora aparecem nos arquivos do item: `<Nullable>enable</Nullable>` entrou no `SPT.Launcher.csproj` por outro track ENTRE a review e o apply — mesmo ruído dos arquivos irmãos (`RegisterViewModel`, `ProfileViewModel` etc.); anotação de nullability do codebase é fora do escopo deste item (nota registrada também na review 04). Exe não executado (proibição do run autônomo).

## Pendências (gates humanos)

- **P-004.1** — E2E com server real: 7 classes listadas (nomes/cores/ícones/descrições PT), seleção default = primeira, registro cria perfil com a edition correta (verificar no server), auto-login → Profile. Cobre o DoD do kickoff.
- **P-004.2** — Validar fix D1 em jogo: registrar conta nova → logout → login manual com a senha do registro deve passar SEM o dialog de criar senha.
- **P-004.3** — Validar fallback: server sem o mod (ou rota desligada) → lista vanilla sem crash.
- **P-004.4** — Validação visual do restyle (grafite 8fa0190): contraste dos `nameColor` das 7 classes sobre `TrlPanelOverPhotoBrush`.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Claude | Criação (entrega do item 004L). |
| 2026-07-04 | Claude | Rev. 2 — review 01 aplicada: CR-01-01 (finally garante o Post), CR-01-02 (guard antes da limpeza), CR-01-03 (`Task.WhenAll` nos ícones), CR-01-04 (infra concorrente + lock por rota), CR-01-05 (chave por backend), CR-01-06 (sanitização interna); 🟢 07/08 aceitos (08.1 resolvido de graça); +A8/A9; nota sobre warnings nullable de outro track. |

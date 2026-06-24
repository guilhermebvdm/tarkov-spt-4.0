# Memory — CustomClasses

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados). Cada entrada resume o que foi feito. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero.

## Estado atual (snapshot ao fim da última sessão)

**Mod híbrido completo e maduro.** Itens **001–037 entregues** (🟢): 11 classes + identidade visual + **editor web Blazor completo** (018–029) + **épico UX (030–037) EXECUTADO** (autônomo via Workflow, 2026-06-11→12). Tudo **commitado** (15 commits do épico sobre baseline `3634548`); working tree limpo exceto frentes paralelas (`docs/migration/*`, `.claude/skills/*`, `.agents/resources.md`, `docs/technical/spt-antipatterns.md`, `mods/SPT-Menu-Overhaul/memory/`). **Branch ahead 22, SEM push** — aguarda revisão do usuário. Relatório: `.handoffs/handoff-2026-06-12-overnight-ux-030-037.md`.

**Redesign 11→6 — épico 050 (perks/drawbacks) IMPLEMENTADO (Sessão 10, 2026-06-23).** Roster: **6 + Peladão** (Médico/Fuzileiro/Caçador/**Furtivo**/Saqueador/Tanque); arquitetura **"tudo-é-perk-flat"** (signatures = perks **flat client-side no F12**, não skills custom — pivot do antigo "tudo-é-skill-real"). **050.0–050.4 compilam** (~21 efeitos: Bulwark, Pack Mule, Heavy Frame, Overladen, Rooted, Execution, Shaky Hands, Rattled, **Adrenaline** state-machine, Cool Under Fire, Ghost Step, Loud Operator, Silent Looter, **Bunker** armas pesadas, Sharpshooter, Iron Lungs…) — **NADA validado in-game** (🔴 P-10.1; vários gates de runtime). Item 047 (matriz) entregue. Design vivo: [class-design.md](../docs/class-design.md) (overview+levers ⚫ arquivados); matriz `scripts/class-matrix.mjs`. **Restante no backlog:** 051 (zona stances), 052 (validação), 053/055/056 (UI), 054 (propagar rename Furtivo `--force-config`), 057 (identidade coop). **Deferidos:** Combat Medic (transpiler), Quick Hands (server-side), Iron Lungs sway. ⚠️ **Sessão paralela do editor** commitando — coordenar antes de salvar `modded/Server/` (este chat não commitou).

**Em andamento (item 038 — redesign workspace 3 painéis estilo EFT):** F0 ✅ (`82bc4a5`, grade 2D X/Y/R no schema+builder) e F1 ✅ (`2cc4274`, workspace read-only: silhueta + grade 2D). Faltam **F2** (edição in-place + skins dialog + DnD HTML5 `.mjs` + migrar `@code` do ClassEdit + refino da silhueta) e **F3** (polimento/docs/smoke in-game). Plano: `~/.claude/plans/monte-um-plano-para-goofy-otter.md`. Silhueta ainda rascunho. Sem push.

- **Identidade:** `CustomClasses`, GUID `customclasses.mdj` (server) + `customclasses.mdj.client` (client BepInEx). SPT 4.0.13. Mod irmão `mods/CustomizationPersistenceFix` (corrige reset de customização do SPT core via Harmony).
- **Classes:** 10 reais + Peladão/**NAKED** (016, skin placeholder havaiana). Gerador `scripts/build-class-jsons.js` congelado como bootstrap (`--force`); **fonte de verdade = `.jsonc` do install** (volta via `/sync-classes`; compile-mod tem guard anti-clobber `--force-config`).
- **i18n:** nome da classe (`displayName {en,pt}`) segue o **idioma do EFT** (`LocaleManagerClass.String_0`; "po"=pt); seletor F12 `Language` removido. Descrição da edition = locale do **servidor** (limitação).
- **Identidade visual (item 015):** gradiente sutil (0.15) nos nomes E ícones (`ClassIconGradient`); ícones proporcionais à fonte (`ClassIconRatio` 1.35, `IconSizeFor`); deploy `DeployNameScale` 3.0. Integra com Menu-Overhaul (AccentColor + reposicionamento de botões). Configs F12 em `PROPRIEDADES.md`.
- **Multiplicadores de skill:** buff/debuff client-side (escala XP); rota `/customclasses/skill-multipliers` (server). Notificação de level-up `EASILY`/`FINALLY` (014).
- **Editor web (018–029 🟢):** Blazor Server servido pelo próprio server SPT (`https://<ip do fika>:6969/customclasses`) — lista, detalhe, edição (7 abas), criar/duplicar/deletar com **hot-apply** (perfil novo sem restart); custo RZ vivo (`CostService`/`SkillWeights`); validação = mesmo pipeline do boot (`ClassRegistrar` dry-run). Docs: [docs/class-editor.md](../docs/class-editor.md) + [docs/class-schema.md](../docs/class-schema.md).
- **Épico UX (030–037 🟢):** TODOS entregues. 037 cache+índices+throttle; 030 sidebar persistente; 031 SkillCanonicalList+SkillMaster (ordem canônica, 3 modos: read-only/edit/compare); 032 matriz heatmap (`/customclasses/skills`); 033 dashboard single-screen (2-col, sem expansion panels, `customclasses.css`); 034 GearPanel/StashPanel/ItemTooltip; 036 comparação A×B (?compare=); 035 densidade global + Ctrl+S + prefs localStorage (`customclasses.js`). Validado: build 0/0, boot `Loaded 11 class(es)`, editor smoke no Chrome MCP. **Pendente:** medição quantitativa 037 (logs `[perf]` são LogDebug; precisa server em nível Debug + baseline) + QA visual.
- **Coordenação multi-chat:** identidade visual/client correu em sessões paralelas (2026-06-09, 2026-06-11 21:45); o épico UX foi executado autônomo nesta sessão (Workflow por wave) — frentes paralelas (skills SKILL.md, mods-inventory, SPT-Menu-Overhaul) ficaram unstaged, intactas.

## Pendências / próximos passos conhecidos

- 🔴 [P-10.1] (aberta 2026-06-23) **Validação in-game 050.0–050.4** (~21 efeitos) + 047 — vários **gates de runtime** que o compile não pega (`method_67` som, `method_12` fôlego, injeção `_aimingSpeed`, getter `TotalErgonomics`, `IPlayerOwner.iPlayer`, weapClass vs DB). Regra `feedback_spt_validation`. (Supersede P-8.1.)
- 🟡 [P-10.2] (aberta 2026-06-23) **Deferrals do 050:** Combat Medic (transpiler em `DoMedEffect` + lock de cirurgia runtime), Quick Hands (buff `SearchDouble` server-side), Iron Lungs sway (`BreathEffector`). Detalhe em 050…-05-asbuild.
- 🟡 [P-10.3] (aberta 2026-06-23) **Redesign restante (backlog):** 051 (zona stances — coordenar stances mod), 057 (identidade coop — toca `modded/Server`), 053/055/056 (UI), 054 (propagar rename `--force-config`), 052 (validação final). (Supersede P-8.2.)
- 🟢 [P-8.3] (aberta 2026-06-21) **Badge "Not registered" no editor** — server registra a edition por `displayName.pt` mas o editor chaveia por `name` en. Cosmético; corrigir se incomodar.
- 🟡 [P-8.4] (aberta 2026-06-21) **Gear + identidade visual de furtivo/tanque são placeholder** (loadout/`iconFile` clonados do furtivo/tático) — curar (ícones próprios, gear definitivo).
- 🔴 [P-7.9] (aberta 2026-06-12) **PUSH dos 15 commits do épico** + QA visual no viewer (build-gate ≠ correção: matriz em células estreitas 032, dashboard em viewport estreito 033, comparação A×B 036).
- 🟡 [P-7.10] (aberta 2026-06-12) **Medição quantitativa do 037** (before/after) — fechar o DoD: subir server com log Debug (`[perf]` é LogDebug) + baseline pré-`d180195`.
- 🟡 [P-7.11] (aberta 2026-06-12) **Achados de review adiados** (no relatório 2026-06-12, por wave) — decidir follow-ups: 036 multiplicadores de B lado a lado (toca componente 031), 037 dispose de `_recomputeCts`, 034 msg "filtro sem resultados" na aba Stash.
- 🟡 [P-7.3] (aberta 2026-06-11) **Validação in-game pós-CR-EP-01**: stash agora spawna `preset`/`mods`/`ammo`/`contents` montados (antes só tpl+count) — criar perfil novo e conferir o nascimento (regra `feedback_spt_validation`).
- 🟡 [P-7.4] (aberta 2026-06-11) **Validações in-game da sessão paralela de 2026-06-11** (013 botão SKILLS×MO, 015 polish, 017 CustomizationPersistenceFix) — ver entrada Sessão 6 (2026-06-11 21:45).
- 🟡 [P-7.5] (aberta 2026-06-11) **Housekeeping deferido do editor**: CR-EP-10 (ícones client×server sem validação cruzada), CR2-EP-05 (óptica mínima não precificada), página `/customclasses/picker-test` (rota dev sem link).
- 🟢 [P-7.6] (aberta 2026-06-11) Conteúdo definitivo do **Peladão** (skin/descrição/cor — placeholder do 016).
- 🟢 [P-7.7] (aberta 2026-06-11) **Outfits definitivos das 10 classes** (aguardando escolhas skin↔classe; `scripts/suits-catalog.json` pronto — item 004/D1).
- 🟢 [P-7.8] (aberta 2026-06-07) **Importar loadout de profile real** (decisão de 2026-06-07, nunca executada) — superseded pelo editor de equipado/stash (026/028); vira candidata a feature futura do editor ("import from profile").

## Sessões

### 2026-06-07 — Planejamento + scaffold

- Adicionados ao repo (sessões anteriores deste chat) os mods de referência `SkillDistribution` (+ lib `ZGFueDkxCommonLibrary`) e `Skills-Extended` via `/add-mod-repo-for-modding`.
- Exploração profunda de RZCustomProfiles (limitações), SkillDistribution (padrão de multiplicadores), Skills-Extended (soft-detect) e do server source SPT (`references/spt-source/`: `CreateProfileService`, `ProfileHelper`, `LauncherController`, modelos `ProfileSides`/`TemplateSide`/`Customization`).
- Plano escrito, revisado 2× com `/g-review-content` e aprovado pelo usuário. Decisões travadas registradas no snapshot acima.
- Scaffold criado: `mod.json`, `README.md`, `backlog/mod-backlog.md` (roadmap 001-008), este `sessions.md`. Sem código de mod ainda.
- **Item 000 (infra) concluído:** `.agents/scripts/compile-mod.sh` reescrito para suportar C# multi-projeto/híbrido (server-csharp antes dava erro). Detecta N csproj, classifica client/server/lib, builda os entry projects e instala só as DLLs próprias (filtro por AssemblyName) em `BepInEx/plugins/<mod>/` (client) e `SPT/user/mods/<mod>/` (server). Caminhos client-csharp single e server-typescript preservados. Verificado: `bash -n` + classificação contra SkillDistribution/Skills-Extended.
- **Item 001 — fluxo de backlog completo até `/code-mod`:** spec funcional (01) + review inline + spec técnica (02, ancorada no `spt-source`) + review técnica (03, 6 pontos, 0 bloqueadores, todos resolvidos) + implementação (`/code-mod`) + asbuild (05). Código em `modded/Server/`: `CustomClasses.Server.csproj`, `CustomClassesMetadata.cs`, `CustomClassesMod.cs` (`[Injectable] IOnLoad` em `PostDBModLoader+1` injeta a edition "Test Class" clonando "Standard" e setando skills Endurance 5 / Strength 3). Evidências-chave: `GetProfileTemplates()` (DatabaseService.cs:141), Progress=nível*100 (ProfileHelper.cs:460), descrição via texto literal (ServerLocalisationService.cs:163 fallback). **Não compilado/validado ainda.**

### 2026-06-09 — Ícones de classe (estilo ChatSpecialIcon) + paleta Tarkov-dark

> ⚠️ **Sessão paralela.** Esta sessão mexeu **só na arte/identidade visual**. Os itens 010–016 (infra de identidade, patches de menu/skills/deploy, ClassIconCache, ClassVisualRegistry, classe Peladão) foram feitos por **outra(s) sessão(ões)** e **ainda não estão registrados aqui nem commitados** — todo o working tree de identidade visual está untracked/`M`.

- **Objetivo:** dar a cada classe um ícone próprio no estilo dos **selos de edição do EFT** (Unheard/EOD), consumido pelo `ChatSpecialIcon` (nome do jogador) já existente (item 015).
- **Assets (11 ícones = 10 classes + Peladão):** símbolos do **game-icons.net** (CC BY 3.0), **silhueta branca + alpha** (máscara), **tingida com a `nameColor` da classe em runtime** (igual ao tingimento de edição do EFT). Mapeamento e créditos em `modded/Client/icons/ATTRIBUTION.md`. Caçador = `skoll/bullseye` (trocado de `crosshair` → traços finos sumiam em ~40px); Op. Tático = `star-medal`; demais: anvil, binoculars, ak47, gears, health-normal, hooded-figure, swap-bag, campfire, underwear.
- **Pipeline (authoring-time, fora do runtime):** SVGs vendored em `scripts/icon-sources/<classe>.svg` → `scripts/build-icons.mjs` (dep **sharp**, isolada em `scripts/package.json`) rasteriza p/ **256² branco+alpha** em `modded/Client/icons/`. `scripts/preview-icons.mjs` gera folha de contato **tingida** em `scripts/preview/` (gitignored) p/ revisão. Trocar arte = trocar SVG/`CLASS_VISUAL` + `npm run build:icons` + compile-mod + **restart do cliente** (PNG cacheado; **não** recompila DLL).
- **Tingimento (C#, 2 linhas):** `ChatSpecialIconPatch` seta `____icon.color = color` (+ reset p/ `Color.white` no branch não-local → não vaza cor em célula reciclada de lista/chat); `ClassIdentityView.BuildOrRefresh` seta `img.color = color` no selo de menu/Skills. *(Esses 2 .cs também receberam, em paralelo, `BuildColoredName`/CAPSLOCK do item 015 — não é desta sessão.)*
- **Paleta Tarkov-dark (nova):** cores antigas eram vivas/saturadas demais. Refeitas p/ tons dessaturados/militares (bronze/ferrugem/oliva/cáqui/âmbar + frios apagados). Fonte = `CLASS_VISUAL` em `scripts/build-class-jsons.js`; aplicada cirurgicamente nos 10 `.jsonc` (só a linha `nameColor`). Hex: armeiro `#a8824e`, batedor `#5f7f93`, cacador `#c2973f`, fuzileiro `#b0573a`, gerenteDeOperacoes `#4f8a80`, medicoDeCombate `#6f9455`, operadorFurtivo `#7d7392`, operadorTatico `#7a818c`, saqueador `#c4ad45`, sobrevivencialista `#97934e`, peladao `#c28a60`.
- **Peladão (016):** entrada visual (`iconFile`+`nameColor`) **adiantada** em `CLASS_VISUAL`; inerte até o profile `peladao` existir em `class-recipes.js`. PNG já gerado.
- **Deploy:** `compile-mod CustomClasses` feito → DLL (com tingimento) + 11 PNGs + config instalados em `D:/SPT`. Compila 0 warn/err. **Validação in-game pendente** (usuário): perfil precisa ser classe do mod + `ShowClassOnPlayerName` (F12) on; olhar deploy/character/lista online.
- **Pendências:** validar in-game; afinar Op. Tático (`star-medal` é o tom/símbolo mais "neutro"); decisão de **commit** do bloco de identidade visual (010–016 + arte) — está tudo no working tree.

### 2026-06-09 — Plano do épico "editor web de classes" (itens 018–029)

> ⚠️ **Sessão paralela** (mesmo dia da sessão de ícones e da que criou o bug 017). Esta sessão só **planejou e materializou backlog** — zero código.

- **O que é:** editor web completo de classes **dentro do próprio mod** — visualizar/editar/criar classes (campos simples, skills, mults, hideout, outfit, equipado composto, stash) com **custo automático**. Plano aprovado pelo usuário (`~/.claude/plans/`, sessão 2026-06-09) após 2 rodadas de revisão de gaps (+ `/g-review-content`, 9 correções aplicadas).
- **Arquitetura travada:** Blazor Server **padrão Skills-Extended** — `IModWebMetadata` + `SPTarkov.Server.Web` 4.0.2 (MudBlazor transitivo); páginas `.razor` compiladas na DLL, rotas `@page "/customclasses/..."` na raiz (`https://localhost:6969/customclasses`); estáticos em `/{AssemblyName}/` = `/CustomClasses-Server/`. Catálogo de itens do **DB vivo** (`DatabaseService`) — decisão: **NÃO** integra com `tools/tarkov-itemdb`.
- **Fonte de verdade:** editor lê/escreve `.jsonc` no **install** (D:/SPT); volta pro repo via novo `/sync-classes`; `compile-mod.sh` e gerador ganham **anti-clobber** (gerador congelado como bootstrap). Decisão do usuário.
- **Custo:** fórmula RZ pura (SKILL_MULTS/BASELINE 15/clamp 0.25–5.00/budget 28–32 + loadoutTotalRub); XP-mults FORA do custo. Skills sem peso (4 do SE + futuras): peso derivado da **mecânica de upagem** (evento de XP/frequência/velocidade, analisando source do SE) + fallback por categoria — pedido explícito do usuário.
- **Fatos-chave descobertos:** hot-apply é viável (`CreateProfileService` lê o dict de templates a CADA criação de perfil; registries singleton mutáveis — mas **sem Remove/enumeração**, precisa adicionar); rename de classe órfã perfis existentes (`ProfileInfo.Edition` é string) → rename **bloqueado**, caminho = duplicar; round-trip do editor **perde comentários** dos `.jsonc` (aceito, `.bak`); lista de classes deve vir dos **arquivos** (disabled não está em registry); `compile-mod.sh` não copia `wwwroot/` e clobbera `config/`.
- **Backlog materializado:** itens **018–029** no `mod-backlog.md` + 12 pastas com `*-00-kickoff.md` (brief → insumo do `/create-spec`). **Renumerado +1 na hora de gravar**: sessão paralela criou o 017 (bug skin não persiste) — colisão detectada via re-read do backlog. Waves: W0 [018 doc schema] → W1 [019 guard-rails PRIMEIRO → 020 infra-web · 021 registrar/editor-service · 022 catalog-custo] → W2 [023 pickers · 024 viewer] → W3 [025 edit-simples, **fecha MVP**] → W4 [026 equipado · 027 criar/duplicar/deletar] → W5 [028 stash · 029 docs]. Sub-agents paralelos por wave (worktree quando tocam `Web/`).
- **Pendências:** executar as waves pelo workflow (`/create-spec 018` em diante); nenhuma implementação iniciada.

### 2026-06-10 — Épico do editor web IMPLEMENTADO (018–029) + 2 rodadas de review + teste de UI no Chrome

> Mesma sessão (continuação) da que planejou o épico em 2026-06-09. Execução completa por waves com sub-agents paralelos.

- **Itens 018–029 TODOS entregues (🟢 no backlog)** pelo fluxo spec→tech→code→asbuild por item, em 6 waves (W1 com 3-4 agentes paralelos em territórios de arquivo disjuntos; build integrado pelo orquestrador entre waves). Builds sempre 0 warn/0 err; **validação real**: server bootado em background (`DISABLE_VIRTUAL_TERMINAL=1 ./SPT.Server.exe`; bind no IP do **fika-server** `26.207.194.149:6969`, NÃO no http.json) + curl + **Chrome DevTools MCP**.
- **O que existe agora:** editor Blazor Server dentro do mod (`Web/` + `wwwroot/`, MudBlazor 8.13 transitivo de `SPTarkov.Server.Web` 4.0.2, Sdk.Web, `IModWebMetadata`): lista (`/customclasses/classes`, lê ARQUIVOS via `ClassEditorService`), detalhe read-only, edição com 7 abas (Geral/Skills/Mults/Hideout/Outfit/Equipado/Stash), criar/duplicar/deletar com hot-apply/hot-remove (sem restart p/ perfil novo), custo RZ ao vivo (`CostService`/`SkillWeights` — paridade validada com `scripts/check-skill-costs.mjs`), pickers (item/preset/ammo por calibre/customization), dry-run de capacidade do stash (GridPacker 10×30 do Zero to hero). Guard rails: anti-clobber no compile-mod (`--force-config`), `/sync-classes` (install→repo, propaga deleção SÓ no 1º nível de `classes/`), gerador congelado (`--force`).
- **2 rodadas de code-review consolidado** (`backlog/029-docs-e-fechamento/epico-editor-04-code-review-0{1,2}.md`): R1 = 0 bloq/2 maiores/9 menores → todos aplicados ou aceitos (CR-EP-01: stash agora honra preset/mods/ammo/contents como o equipado — `InventoryBuilder.PackSpecsIntoGrids` estendido; CR-EP-02: deleção via editor não ressuscita no compile-mod). R2 = 0 bloq/2 maiores/9 menores → aplicados (CR2-EP-01 contents×count no custo; CR2-EP-09 `ToDefinition()` fora do `Task.Run`; CR2-EP-02 propagação restrita ao 1º nível). Dívidas aceitas documentadas: CR-EP-03 (race de insert, uso local), CR-EP-10 (ícones client×server), CR2-EP-05 (óptica mínima não precificada).
- **Teste de UI completo no Chrome MCP** (fluxo E2E real): criar "Teste UI" (validação de colisão OK) → editar todas as abas → SV-98 preset premium + ammo LPS + loadedMag/chambered → Salewa no stash (capacidade 2/300 ✓) → **save + hot-apply confirmado no log do server** (`Registered ... skills=1 items=1 mults=1`) → duplicar → deletar com aviso de perfis (lista TestePerfil1 ao tentar deletar Armeiro; CANCEL) → cleanup. **3 achados corrigidos:** UI-01 save com campo inválido mostrava "Saved" (agora MudForm gate: "Save blocked — fix the invalid fields first", validado); UI-02 picker de outfit mostrava chaves cruas — nomes agora via locale **`"{id} Name"`** (filtro "P" → 74/268 validado); UI-03 erro de console `MudPointerEventsNone` = **cosmético do padrão upstream** (SE e host idem; tentativa de loader inline revertida — Blazor não renderiza `<script>` com corpo).
- **Lições:** (1) MudTable `OnRowClick`/MudTextField debounce NÃO respondem a eventos JS sintéticos — testar com `fill`/`press_key` do MCP (teclado real); (2) sub-agents paralelos no mesmo working tree funcionam com territórios de arquivo explícitos + exclusividade de `dotnet build` p/ UM agente; (3) `_props.Name` de customization é chave interna — nome humano só no locale `"{id} Name"`.
- **Pendências:** validação in-game (perfil novo nascendo com loadout editado — memória `feedback_spt_validation`); decisão de commit do épico inteiro (working tree gigante: 018–029 + fixes, nada commitado; inclui também o bloco 010–017 de sessões anteriores); housekeeping deferido (CR-EP-10 ícones, page `/customclasses/picker-test` mantida como rota de dev).

### 2026-06-10 — Comparação de UX (editor × viewer de perfis RZ) + épico UX materializado (030–035)

- **Análise comparativa** com `tools/tarkov-itemdb/viewer/profiles.html`/`profiles-skills.html`: o viewer antigo ganha em (1) tudo numa tela (header badges + skills à esquerda + loadout visual à direita, denso 12-14px), (2) skills SEMPRE na ordem canônica Ph→M→C→P (`SKILL_MASTER` em `profiles.js:10-45`, zeros esmaecidos) → comparação imediata entre classes, (3) sidebar persistente de perfis (1 clique, sem perder contexto), (4) matriz classes×skills com heatmap (`profiles-skills.html`), (5) tooltips no hover (item: nome/tamanho/preço). O editor atual perde nesses pontos (lista→linha→detail com expansion panels→edit com 7 abas; skills só as definidas na ordem do JSON; zero visão comparativa) mas ganha em edição/hot-apply/custos vivos/diagnostics.
- **Épico UX materializado no backlog: itens 030–035** (kickoffs em `backlog/0NN-*/`): 030 sidebar de classes (1 clique, preserva vista; `ListClassSummaries` leve — dry-run do ListClassFiles é pesado p/ sidebar), 031 skills canônicas (componente compartilhado, modo read-only+edit inline — mata o "Add skill" por dropdown), 032 matriz heatmap (`/customclasses/skills` + custo no rodapé + célula clicável), 033 detalhe single-screen (dashboard 2 colunas, sem expansion panels, CSS denso local), 034 loadout visual (gear slots estilo Tarkov + stash em grid de ícones por categoria + tooltip hover; ícones tarkov.dev), 035 densidade global + cliques (Dense, aba preservada ao trocar classe, Ctrl+S, regressão Chrome MCP). Waves: UX-W1 [030‖031] → UX-W2 [032‖033] → UX-W3 [034] → UX-W4 [035 solo].
- **Pendência:** executar as waves do épico UX pelo workflow (aguardando OK do usuário pra rodar; itens ⚪).

### 2026-06-11 21:45 (GMT-3) — Sessão 6: Polish de identidade (015) + i18n do nome + fixes 013/017 + mod CustomizationPersistenceFix

> Nota de migração (2026-06-13): esta entrada estava sem número de sessão e usava IDs derivados de data (`P-0611.x`), violando o esquema `P-<N>.<M>` (`memory-curation` §7). Numerada retroativamente como **Sessão 6** e os IDs convertidos para `P-6.x` (fatos inalterados — só a notação). Cf. revisão de valor D1-02.

> ⚠️ **Sessão paralela (client UI / polish).** O épico do editor (018–035) roda em paralelo no server — ver entradas 2026-06-10 e a memória global `project_customclasses_session_split`. O trabalho desta sessão cruzou 06-09/10/11 (conversa longa, compactada); timestamp = momento da gravação.

**Tema central:** acabamento da identidade visual + i18n do nome da classe, fechar os bugs 013/017, e extrair o fix do bug de skin do SPT para um mod dedicado.

**Decisões-chave:**
- **i18n do nome segue o IDIOMA DO EFT** (não mais o seletor F12): `displayName {en,pt}` no JSON → exposto na rota (`classNameEn/Pt`); client resolve via `LocaleManagerClass.String_0` (novo `UI/GameLocale.cs`). Seletor `Language` (F12) **removido** (redundante). Descoberta: **"po" = Português** no SPT (`locales/global/po.json`). Descrição da edition no launcher segue o **locale do servidor** (server-side — `LauncherController:63`; limitação documentada). Nomes EN em `build-class-jsons.js` `DISPLAY_NAME_EN`.
- **Ícones proporcionais à fonte** (não px absoluto): `ClassIconSize`→`ClassIconRatio` (1.35) + `ClassIdentityView.IconSizeFor` = `fontSize × ratio` por tela — resolve a inconsistência de tamanho entre menu/OVERALL/deploy/confirmation.
- **017 = bug do SPT CORE** (não do mod): `ProfileFixerService.CheckForAndFixPmcProfileIssues` reseta Body/Hands/Feet **válidos** p/ default no `/client/game/start` (lógica invertida — falta o `!`; só o Head está correto). Confirmado pelo usuário (afeta qualquer skin, pré-existe ao mod). **Fix = mod server dedicado `mods/CustomizationPersistenceFix`** (Harmony Prefix/Postfix preserva válidos). Ref: `017-customizacao-nao-persiste-00-bug.md`, memória global `reference_spt_customization_reset_bug`.
- **013 = clone órfão pelo Menu-Overhaul**: o MO reposiciona botões por nome via `anchoredPosition` (`ButtonHelpers`, y=-index*60); o clone com `SetSiblingIndex` ficava invisível. Fix: coroutine pós-MO posiciona o SKILLS abaixo do Character + empurra Trade/Hideout/Exit (posições absolutas). Ref: `SkillsNavButtonPatch.cs`, `013-...-06-fix-01`.
- **Gradiente sutil (escolha do usuário):** clareamento dos nomes 0.4→**0.15** (destoava do glow/EXP **sólidos** do MO — `LayoutHelpers.UpdateTopGlowColor` usa AccentColor.rgb + alpha; `PlayerProfileFeaturesPatch:617/690`) + **gradiente também nos ÍCONES** (novo `UI/ClassIconGradient.cs` `BaseMeshEffect` — silhueta branca × degradê vertical = look Unheard/EOD). Ref: `ClassIdentityView.cs`.
- **NAKED** = nome EN do Peladão (era Streaker). **014 aprovado** in-game.

**Atividade cronológica:**
1. i18n do nome (server `ClassDefinition`/`ClassVisualRegistry`/`Response`/`Router` + client `SkillMultipliers`/`GameLocale` + scripts) — nome resolve EN/PT pelo EFT; seletor `Language` removido; os 3 usos migraram p/ `GameLocale.IsPortuguese`.
2. Calibragem proporcional dos ícones + deploy `DeployNameScale` 1.2→2.2→**3.0** (escala o ChatSpecialIcon = ícone+nome juntos). **Lição:** BepInEx persiste o valor no `.cfg` — mudar só o default no código NÃO atualiza instalação existente; editar o `.cfg` junto.
3. 014 testado/aprovado (Endurance gigante temporário em `operadorTatico`=100 → revertido p/ 1.5).
4. 017 investigado (template tem a skin, perfil salvo tem default → reset pós-criação) → causa-raiz = bug do SPT core → criado `mods/CustomizationPersistenceFix` (Harmony 2.15.0; ref local `0Harmony.dll` Private=false). Skin havaiana re-gravada no perfil Peladon (workaround manual do save).
5. 013 fix (posicionamento relativo ao MO via coroutine).
6. Gradiente sutil + gradiente nos ícones (todas as telas via `ApplyClassIcon` central; `RevertIconGradient` no ramo não-local p/ não vazar).
7. NAKED (gerador + JSON + install).

**Pendências abertas nesta sessão:**
- [P-6.1] 🔴 Validação in-game: 013 (botão SKILLS no menu), 017 (skin persiste após fechar/reabrir), 015 polish (NAKED, ícones proporcionais, deploy 3.0, gradiente nome+ícones, nome×glow alinhados).
- [P-6.2] 🟢 016 — definir a skin definitiva do Peladão (placeholder havaiano).
- [P-6.3] 🟢 Melhorias sugeridas ao usuário: skins reais por classe + ícones coesos; validação coop/compat (FIKA + AllTheClothes/WTT mexem em customization); playtest do balanço dos multiplicadores; reportar o bug do `ProfileFixerService` upstream ao SPT.

**Cross-refs:**
- **Trabalho paralelo no MESMO mod** (server/editor): itens 018–035 — ver entradas 2026-06-10. Esta sessão = client UI/polish; só o i18n do server (displayName no `ClassVisualRegistry`) tocou área comum, integrado ao refactor do item 021 (`ClassRegistrar`).
- Mod separado **`mods/CustomizationPersistenceFix`** (sem `sessions.md` próprio ainda).
- Memória global: `reference_spt_customization_reset_bug`, `project_customclasses_session_split`.
- **Review do épico UX aplicado (12 achados):** contagem de skills sem números mágicos (fonte = `SkillWeights.cs`; "31" era falso, enumeração soma 32+4 SE); DoD do 033 realinhado (single-screen completo fecha no 034); **guard de unsaved changes no 030** (1 clique do sidebar não pode descartar edição — dialog Save/Discard/Cancel); fallback edit→detail p/ classe inválida; zeros explícitos no JSON preservados no round-trip do 031; disabled na matriz (toggle); **+item 036 — comparação A×B no dashboard** (deltas por skill ▲▼ + custo, B fixa enquanto A navega; UX-W3 ‖ 034); sidebar com filtro+status dots (030); lista ordenável + preferências em localStorage (035); stash do edit agrupado/filtrável (034). Métrica corrigida: detail atual tem 4 painéis fechados por default (Stash/Mults/Hideout/Outfit — verificado no código). Épico agora 030–036.
- **Revisão de DESEMPENHO do épico UX (lentidão reportada pelo usuário) — diagnóstico confirmado no código:** (1) `ClassEditorService.ListClassFiles()` roda `ValidateAndBuild` (deep clone do template base + InventoryBuilder + outfit) p/ CADA arquivo e é chamado por TODAS as páginas (`Classes.razor:126`, `ClassDetail.razor:407`, `ClassEdit.razor:580`) — navegação = 11 dry-runs; (2) prerender duplo do Blazor dobra → **22 dry-runs por navegação** (a lentidão dominante); (3) `CatalogService` sem índices (Search varre GetItems+locale por busca; GetClothing varre customization por render da aba Outfit ×4 pickers); (4) `ClassEdit` recalcula custo 2× + CheckStashCapacity (GridPacker) a cada keystroke. **Item 037 criado** (cache de `ClassFileEntry` por mtime invalidado no Save/Delete/Create; índices lazy no Catalog; recompute com throttle + 1 chamada; avaliar prerender:false; medição antes/depois obrigatória) — vira **UX-W0, PRIMEIRO**; 030 (`ListClassSummaries` = view do cache) e 032 (matriz não pode disparar dry-run) atualizados pra consumir. Épico agora 030–037.

### 2026-06-11 22:49 (GMT-3) — Sessão 7: Handoff do épico UX + curadoria da memória

**Tema central:** fechar a sessão do épico UX com um handoff executável em outra sessão e curar o topo desta memória (defasado desde a era 001–005).

**Decisões-chave:**
- Handoff como artefato de retomada: **`.handoffs/handoff-2026-06-10-epico-ux-editor-030-037.md`** — próxima ação no topo (`/create-spec 037`), links pro plano/kickoffs (sem duplicar), modelo de execução por waves com sub-agents (territórios + build exclusivo), receita do ambiente (boot do server com `DISABLE_VIRTUAL_TERMINAL=1`, URL do bind fika, bypass de cert, Chrome MCP × MudBlazor). `.handoffs/` adicionado ao `.gitignore` (scratch de sessão, não artefato).
- Curadoria do topo (skill `memory-curation` §6): snapshot "Estado atual" e "Pendências" reescritos como **delta** (≤10 bullets, só abertas, com IDs P-7.x); os blocos antigos ("Pendências" acumuladas da era 001–005, "Mudança de fluxo dos ITENS (003)" e "Riscos abertos") foram **movidos verbatim** para a seção "Arquivo" no fim deste arquivo — preservação sem revisionismo (§8).
- [P-7.8] registra que a decisão de 2026-06-07 ("itens virão de profiles reais montados in-game") foi **superseded** pelo editor de equipado/stash (026/028) sem nunca ter sido executada.

**Atividade cronológica:**
1. Handoff escrito + `.gitignore` atualizado.
2. Curadoria: topo reescrito, blocos antigos arquivados (esta entrada).

**Cross-refs:**
- Trabalho paralelo no mesmo dia: ver Sessão 6 (2026-06-11 21:45) (polish 015 + fixes 013/017).
- O plano do épico UX em si foi gravado nas entradas de 2026-06-10 (não duplicado aqui).

### 2026-06-12 — Épico UX 030–037 executado autônomo (Workflow por wave)

**Tema central:** executar o épico UX de ponta a ponta sem supervisão (usuário dormindo), pelo fluxo do repo, orquestrado por **Workflows** (1 por wave) com gates determinísticos.

**Setup:**
- Commit do working tree pré-existente em grupos (identidade+editor 010–029; mods novos CustomizationPersistenceFix e SPT-Menu-Overhaul; plano UX 030–037; `.handoffs/` gitignorado; `*.Backup.tmp` gitignorado; `LocalPlayer.cs` decompilado movido p/ `references/eft-decompiled/`).
- Verificação de settings: `bypassPermissions` já ativo nos dois `settings.json` → nenhuma mudança de permissão necessária p/ rodar autônomo.
- Política autônoma aprovada: auto-aplicar achados de review SEGUROS / adiar design; build hard-gate por wave; pular item em falha; **1 commit por item, pathspec explícito, SEM push**.

**Execução (waves):**
- **W0 [037]** — Workflow estourou **limite de sessão** após o code-mod (spec/tech-spec/review rodaram); loop principal **salvou** (build 0/0 + commit `d180195`). Reset de quota → retomada de manhã.
- **W1 [030+031]** paralelo → 🟢 `73228e9`/`1db0dad` (build teve 2 erros no SkillCanonicalList auto-corrigidos em 2 tentativas).
- **W2 [032+033]** paralelo → 🟢 `2e3ea9c`/`d52211f`.
- **W3 [034→036]** **sequencial** (compartilham `ClassDetail.razor`/`customclasses.css` — evitar clobber) → 🟢 `72866cc`/`b4dc2cf`.
- **W4 [035]** solo → 🟢 `fdc9439`.
- 037 code-review (lacuna da W0) fechado depois: `8e38bde`+`515ab1c` (0 fixes, código correto).

**Validação:** build integrado 0/0; `compile-mod` instalou (config sem divergência); boot `Loaded 11 class(es)` sem exceção; smoke Chrome MCP confirmou sidebar/matriz/skills canônicas/abas/prefs. Medição quantitativa 037 ficou pendente (logs `[perf]` são LogDebug).

**Lições:**
- **Custo real por wave ~370–630k tokens** → o épico inteiro estoura o limite de sessão em uma janela; a W0 morreu no meio. Mitigação que funcionou: **salvamento pelo loop principal** (build+commit do que estava pronto) + retomada pós-reset; pipeline comprimido (menos agentes/item) nas waves seguintes.
- **Itens que compartilham arquivo** (034/036 em ClassDetail) **não podem rodar em paralelo** — quebra o commit por-item com pathspec. Rodar sequencial.
- here-string PowerShell `@'...'@` na Bash tool insere `@` literal no commit subject → usar `git commit -F <arquivo>`.

**Cross-refs:** relatório completo (achados adiados por wave, premissas, hashes) em `.handoffs/handoff-2026-06-12-overnight-ux-030-037.md`. Plano de execução em `~/.claude/plans/`.

### 2026-06-12→13 — QA do editor + início do redesign workspace 3 painéis (item 038)

**QA visual (Chrome MCP) do épico 030–037 + correções:**
- **Bug de raiz:** `css/js` do editor estavam em `Server/Web/wwwroot/` (NÃO servido); o SPT serve `Server/wwwroot/`. Davam 404 → dashboard sem CSS (1 coluna), Ctrl+S/prefs mortos. Movidos pra `Server/wwwroot/`. **E**: shipar `.js` faz o SPT rejeitar o mod (`ModValidator.cs:316` varre `*.js`/`*.ts` → "pre-4.0.0 JS mod") — renomeado pra `customclasses.mjs` (módulo, `type=module`). Commit `e1360ae`.
- **Sidebar 3 estados** (open 250 / mini 64 / closed 0) ciclando no botão, **conteúdo sincronizado** via classe no `MudLayout` (`cc-sb-*`) controlando largura do drawer E margem do `mud-main-content` na mesma regra (mecânica nativa do MudBlazor dessincronizava; `display:none` no closed). Ícones centralizados no mini (tooltip wrapper inline-block encolhia). Commits `3ecd023`, `3079782`.
- **Skills matrix:** faltava `MudContainer`+`MudPaper` (sem topo/padding) — padronizado. Commit `5849e0d`.

**Redesign workspace (item 038) — plano aprovado, multi-sessão:** 3 painéis estilo EFT (esquerda skills/mults/hideout/outfit · centro silhueta CSS + 14 slots · direita grade 2D do stash X/Y/R + DnD), view/edit unificado (Editar destrava in-place), + seletor de skins com preview simbólico (sem imagem 2D nativa). Plano em `~/.claude/plans/`, revisado 3× com `/g-review-content`. **Decisões:** grade 2D honrada in-game (coords **opt-in**, não reescreve `.jsonc`); **tamanho-com-mods** (não base); DnD = **HTML5 + `.mjs`** (MudBlazor não faz 2D); página única + `_editMode` toggle.
- **F0 ✅ (`82bc4a5`):** X/Y/Rotated em `ItemSpec`/`ItemSpecModel`; `GridPacker.TryPlaceAt`; `InventoryBuilder.PlaceTree` honra coords (tamanho real) + fallback first-fit (coord-bearing primeiro); `CostService.GetStashGridSize`. Gate: boot dry-run das 11 (regressão auto-pack idêntica) + classe hand-crafted x/y registra limpo. Coords opt-in → classes coordless byte-idênticas.
- **F1 ✅ (`2cc4274`):** `ClassWorkspace.razor` + `CharacterDoll.razor` (silhueta+slots) + `StashGrid.razor` (grade 2D 10×30, 23 itens posicionados/dimensionados/ícones). ClassDetail single-class monta o workspace; compare (036) mantém dashboard antigo. Fix-chave: `--cc-cell-unit` içado pro `.cc-ws` (estava só em `.cc-item-cell` → `repeat(W,var())` inválido → grade colapsava em 1 col). Header dedup (página mantém nome/ações; workspace = barra slim de custo). **Silhueta = qualidade rascunho** (posições aproximadas, refino em F2/F3).
- **Pendente:** F2 (edição in-place + skins dialog + DnD `.mjs` + migrar `@code` do ClassEdit 1272L + refino silhueta) · F3 (polimento, retrocompat, docs, smoke in-game pelo usuário). **Sem push.**
- **Lição:** UI visual (silhueta/grade) feita por agente headless sai estruturalmente OK mas precisa de iteração visual no Chrome MCP (loop principal) — não dá pra fan-out cego.

### 2026-06-20 20:50 (GMT-3) — Sessão 9: Catálogo quantitativo de skills (class-skill-catalog)

> ⚠️ **Sessão paralela, gravada fora de ordem.** Trabalho de 2026-06-20 (anterior à Sessão 8 de 06-21), mas o `/update-memory` rodou depois da Sessão 8 já existir → recebeu o próximo ID de gravação (**9**) e foi posicionada por timestamp **antes** da Sessão 8 (`memory-curation` §10: ID = ordem de gravação, posição = timestamp). A Sessão 8 já **consumiu** o doc criado aqui (cita `class-skill-catalog §6` para masteries inertes).

**Tema central:** transformar o catálogo qualitativo de levers ([class-levers.md](../docs/class-levers.md)) em **fórmulas reais por skill**, para escolher fatores por classe com base quantitativa.

**Decisões-chave:**
- **Doc novo [docs/class-skill-catalog.md](../docs/class-skill-catalog.md)** (🟢 Vivo) — detalhamento quantitativo, cruzando 3 fontes: `SkillsConfig.json` do Skills-Extended, EFT decompilado (`references/eft-decompiled/Assembly-CSharp/`) e `globals.json` (`SkillsSettings`, linha ~35250). Por quê: definir levers exige a fórmula real, não só o rótulo qualitativo.
- **Fonte de verdade do EFEITO das skills = Skills-Extended** (físicas/médicas FirstAid/FieldMedicine + 6 gems); nativas (mastering de arma, AimDrills, vests, Surgery) seguem o EFT base. Regra-mestra de leitura dos valores do config no doc §1 (config = pontos %, `PerLevel(p)`→`p%·nível`, `Max(m)`=L50, `Elite(e)`=plano no L51).

**Lições / hipóteses descartadas:**
- **Bug no SE (Círculo de Cultistas):** `Server/Patches/CultistProductionPatch.cs:69` aplica `CultistCircleReturnTimeReduction=1` **sem** `NormalizeToPercentage()` (≠ `ScavCooldownTimerPatch.cs:52` ao lado) → tempo do círculo **zerado já no nível 1**, não −1%/nível.
- **Copy/paste em `SkillClasses/Physical/EnduranceSkill.cs:26`:** usa `BuffBreathTimeIncMax`(100) onde devia ser `BuffEnduranceIncMax`(50). Validar in-game.
- **Hipótese "RecoilControl removida em 0.14.5" — REFUTADA:** continua viva, −0,3%/nível (−15% no L50). Ref: `globals.json:35549`.
- **Multiplicadores por nível das nativas** (AimDrills, MagDrills, masteries de arma, Search, Attention) são getters compilados de `SkillManager` **não extraíveis** do dump → marcados `[inf]` no doc. `modpage.md` do SE tem números **desatualizados** (config antiga).

**Atividade cronológica:**
1. Mapeamento das fontes (globals `SkillsSettings`, SE `SkillsConfig.json` + plugin C#, EFT decompilado) — 6 gems confirmadas `[]` (mortas) no SPT base, reativadas pelo SE.
2. Investigação cruzada por **2 sub-agents paralelos** (semântica do config no código do SE; nativas no decompilado) com `arquivo:linha`.
3. Criação do doc (regra-mestra, físicas/médicas/gems, combate/movimento/armadura nativas, mortas/meta, achados). Refino posterior do doc (Surgery §4.1, marcação `[mec]`) por outra sessão.

**Pendências abertas nesta sessão:**
- [P-9.1] (aberta 2026-06-20) Achados do Skills-Extended a tratar caso alguma gem vire lever: (a) bug do Círculo de Cultistas não-normalizado; (b) copy/paste do Endurance; (c) confirmar os `[inf]` (mults nativos) via tooltip in-game ou dnSpy, se precisão exata for exigida. Categoria: 🟢 investigação. (**Não promovida ao topo** — o snapshot reflete a Sessão 8, posterior.)

**Cross-refs:**
- Detalha [docs/class-levers.md](../docs/class-levers.md) (consumido depois pela Sessão 8 do redesign 11→6).
- Sessão só de docs/referência — **sem mudança de código, sem `/update-mod-graph`**.

### 2026-06-21 02:21 (GMT-3) — Sessão 8: Redesign 11→6 (Fase 3–5) + item 047 implementado + viewer limpo

**Tema central:** Continuação do redesign de classes (handoff 2026-06-20): consolidar o design, recalibrar a matriz, materializar o épico 047–052 e **implementar/validar o 047** (aplicar a matriz do novo roster de 6 classes).

**Decisões-chave:**
- **Roster 11→6 + Peladão** + arquitetura **"tudo-é-skill-real"** (skills + skillMultipliers + skills custom padrão SE). Design consolidado em `docs/class-levers.md` (+overview, +class-skill-catalog) e `scripts/class-matrix.mjs` (reproduzível, cross-check ✅).
- **netMult topo ~+6 / base ~+4** (Saq/Tan compensados por signatures, fora do net). **Pesos das gems por categoria** (`skill-weights.mjs`+`SkillWeights.cs`: Usec/Bear→C, demais gems→P).
- **SMG e AttachedLauncher removidos da matriz** — inertes no `globals.json` (`[]`). Fantasma trocou SMG→Pistol ×1.8; Tanque GL vira 🔧 patch (não skill 🎯).
- **Decisão #8 (tudo configurável):** levers 🔧/🧪 no F12; matriz no editor web; server com nota de restart. Tabela em class-levers §6.4.
- **Órfão descopado:** **deletar** as 6 aposentadas (não desabilitar) e **sem** router de remap — decisão do usuário (server não roda oficialmente; perfis perdidos OK).
- **Viewer só com skills funcionais:** 6 gems promovidas à seção **"Gems (SE)"** no `SkillMaster`; removido o dump de skills inertes no `SkillCanonicalList`. Convenção: skills custom novas (048+) entram no `SkillMaster`.

**Lições / hipóteses descartadas:**
- **Masteries SMG/HMG/LMG/Launcher/AttachedLauncher são inertes** nesta build (`globals.json` `[]`, sem XP/efeito) → buffar/setar não faz nada. Ref: globals.json:35559/35261; class-skill-catalog §6.
- **Deletar uma classe quebra perfis existentes dela:** `GetProfileTemplateForSide` retorna null (ProfileHelper.cs:808-811) → `TraderHelper.ResetTrader` NRE em `.Trader` (TraderHelper.cs:150). Router de remap especificado e depois **descopado** (sem perfis ao vivo).
- **Launcher mostrava roster antigo** não por cache — estava no **IP do server oficial (remoto)**, não o local. Red herring.
- **Enum é `Shadowconnections`** (c minúsculo); `.jsonc` tolera via `Enum.TryParse ignoreCase` (ClassRegistrar.cs:220/329), mas `SkillWeights.cs` exige a caixa exata. `WaterCloset` = Lavatory (HideoutAreas.cs:8).
- **Badge "Not registered" no editor** = server chaveia edition por `displayName.pt`, editor por `name` en. Cosmético.
- **Dados do Caçador (extract 046, profile modado)**: braçadeira inexistente no DB (`6761b213…`) + 10 armas stash `loadedMag` sem mag → erros não-fatais; limpos (categoria P-7.3).

**Atividade cronológica** (resumida; detalhe nos commits `cb22668`→`d00d072`):
1. Fase 3: `class-levers.md` reescrito + `class-matrix.mjs` + `class-overview.md`.
2. Fase 4: net-check (gems por categoria, inertes fora, custos aparados) + épico 047–052 (kickoffs) + 2× `/g-review-content` (correções) + decisão #8/§6.4.
3. 047: spec → review-spec (3 decisões) → spec-tech (achado do crash de órfão) → review-tech (6 pontos resolvidos) → `/code-mod` (matriz nos 6 `.jsonc`, SkillWeights +3 gems, router descopado) → `/compile-mod` (build+install, `--force-config` + remoção das 6 órfãs do install).
4. Validação: server log "Loaded 7", launcher (após corrigir IP), editor (MCP) funcional.
5. Viewer limpo (Gems SE + sem fantasmas) + provisão no plano (048+). Fix de dados do Caçador (braçadeira vanilla + strip de bare-mags).

**Pendências abertas nesta sessão:** [P-8.1] validação raid · [P-8.2] itens 048–052 · [P-8.3] badge "Not registered" · [P-8.4] gear/visual placeholder de fantasma/tanque (ver topo).

**Cross-refs:**
- Base: handoff `.handoffs/handoff-2026-06-20-customclasses-class-redesign.md` (gitignored).
- Resolve a ponta solta de sync `SkillWeights.cs`↔`skill-weights.mjs` (3 categorias de gem agora em ambos).
- Memória global `project_customclasses_session_split` (sessão paralela do editor — agora roster 6).
- `/update-mod-graph CustomClasses` pendente (hook; mudança de estrutura no SkillMaster).

### 2026-06-23 22:46 (GMT-3) — Sessão 10: Redesign 11→6 — épico 050 (perks/drawbacks) implementado de ponta a ponta

> Conversa longa (compactada), cruzou 2026-06-21→23. Continuação direta da Sessão 8 (item 047 = matriz). Sessão paralela do editor seguiu commitando — **nada commitado por este chat** (evitar corrida).

**Tema central:** Refinar o design do redesign 11→6 e implementar o épico **050** (todos os perks/drawbacks de signature) client-side, fatia a fatia (050.0→050.4), autônomo até o compile (`/g-autodev`).

**Decisões-chave:**
- **Pivot arquitetural "tudo-é-skill-real" → "tudo-é-perk-flat"** — signatures viraram **perks flat** (sem leveling custom; valores medianos desde o início). 4 skills eram redundantes com vanilla (Mãos Rápidas=Search, Passo Fantasma=CovertMovement, Mula=Strength, Médico de Combate=FieldMedicine). Ref: [class-design.md](../docs/class-design.md).
- **Consolidação de docs:** `class-overview.md` + `class-levers.md` → **`class-design.md`** único/vivo; os 2 arquivados (⚫ `docs/.archived/`). 3× `/g-review-content`. Max skill corrigido 10→**51 (elite)**.
- **Rename Fantasma → Furtivo (Stealth)** (item 054): `git mv fantasma→furtivo.jsonc`, `name`="Stealth". ⚠️ **não propagado ao install** (precisa `--force-config`).
- **Gating pela chave estável `name` (EN)** via `SkillMultipliers.IsLocalClass(nameEn)` — `Info.GameVersion`=`displayName[idioma]` varia; `name` EN é idioma-independente. Ref: SkillMultipliers.cs.
- **Masteries inertes** (LMG/HMG/Launcher/AttachedLauncher = globals `[]`) **fora da matriz** → viram o perk **Bunker** (recuo ×0.85 + ergo ×1.15 por patch; weapClass machinegun/grenadeLauncher). Ref: class-design.md. → regra geral promovida a [AP-10](../../../docs/technical/spt-antipatterns.md) (lista versionada fica em class-skill-catalog §6).
- **Cool Under Fire re-escopado** (decisão do usuário): supressão/near-miss **não existe no cliente EFT 0.16.9** → vira **−50% flinch ao levar dano** (mesmo `ForceEffector.AddForce`, oposto ao Rattled).
- **Recon delegado a 3 subagentes** (decompile do `D:/SPT` via ilspycmd: armas, combate/saúde, som) devolvendo só os pontos. Padrão eficiente p/ membros obfuscados fora do decompile curado.

**Lições / hipóteses descartadas:**
- **Confiança de recon = candidato, não pinado.** O 1º recon citou `WeaponRecoil.CalculateRecoil` (✅) que **não existe** no curado (alucinado); o ponto real é `ProceduralWeaponAnimation.Shoot(str)`. Reconfirmar no assembly antes de codar. → promovido a [AP-09](../../../docs/technical/spt-antipatterns.md).
- **`damageInfo.Player` é `IPlayerOwner` (wrapper), não `Player`** — comparar por referência com `MainPlayer` **nunca casa**; usar `damageInfo.Player.iPlayer.ProfileId`. Bug: gatilho "causar dano" da Adrenaline compilava mas nunca disparava. Ref: AdrenalineTriggerPatch.cs · Player.cs:30410.
- **Obfuscados (`method_67` som, `method_12` fôlego) + injeção de campo (`_aimingSpeed`) + getter aninhado (`TotalErgonomics`)** = riscos de **runtime** que o compile não pega → try/catch no `.Enable()` + gate in-game.
- **Quick Hands (Search Double) é server-side** (buff `SearchDouble`); o lever client `CanStartNewSearchOperation` pode ser re-validado pelo server → melhor via server mod.
- **`Weapon.IsUnderbarrelWeapon` não existe** (recon supôs) → underbarrel acoplado vira follow-up; `weapClass` cobre LMG/HMG/GL standalone.
- **Combat Medic não-trivial:** med/cirurgia rápido está em var local de `DoMedEffect` (precisa transpiler); "cirurgia sem lock" não é localizável no estático (animação full-body) → investigação runtime.

**Atividade cronológica** (resumida; detalhe nos as-builts 050…-05 + 054):
1. Design: pivot perk-flat + `class-design.md` + 1 drawback/classe + nomes EN + árvore F12 + 3× review.
2. Backlog: 050 fatiado (050.0–050.4), 054 rename, +053/055/056/057.
3. **050.0:** Bulwark (dano ×0.85) + Pack Mule (+30% carga, piso; reflete no stash) + notificação de raid + rename Furtivo.
4. **050.1:** Heavy Frame (−10% vel), Overladen (inércia ∝ peso), Rooted (−15% vel ADS), Execution (+vel c/ faca).
5. **050.2:** Shaky Hands (recuo ×1.25), Rattled (aim-punch ×1.5), Adrenaline (state-machine 25s/cd120s), Cool Under Fire (flinch ×0.5).
6. **050.3:** Execution melee ×5, Heavy Frame fome/sede ×1.3, anti-jam ×0.5. (Combat Medic deferido.)
7. **050.4:** Ghost Step (som ×0.4), Loud Operator (som ×1.3), Silent Looter (loot ×0.4), Bunker (recuo+ergo), Sharpshooter (ADS ×0.85), Iron Lungs (fôlego ~2×). (Quick Hands server + Iron Lungs sway deferidos.)
8. Overweight do Tanque relatado mas **não reproduziu** → deprioritizado.

**Pendências abertas nesta sessão:**
- 🔴 [P-10.1] (aberta 2026-06-23) **Validação in-game 050.0–050.4** (~21 efeitos) + 047 — vários **gates de runtime** que o compile não pega (`method_67`, `method_12`, injeção `_aimingSpeed`, `TotalErgonomics`, `IPlayerOwner.iPlayer`, weapClass vs DB). Regra `feedback_spt_validation`.
- 🟡 [P-10.2] (aberta 2026-06-23) **Deferrals do 050:** Combat Medic (transpiler `DoMedEffect` + lock cirurgia runtime), Quick Hands (buff server-side), Iron Lungs sway (`BreathEffector`). Detalhe em 050…-05-asbuild.
- 🟡 [P-10.3] (aberta 2026-06-23) **Redesign restante (backlog):** 051 (zona stances — coordenar stances mod), 057 (identidade coop — toca `modded/Server`), 053/055/056 (UI), 054 (propagar rename `--force-config`), 052 (validação final).

**Cross-refs:**
- Continua a Sessão 8 (2026-06-21); **supersede [P-8.2]** (048–052: 048/049 descopados, 050 implementado).
- As-builts: [050…-05-asbuild.md](../backlog/050-signature-patches/050-signature-patches-05-asbuild.md).
- Sessão paralela do editor commitando — memória global `project_customclasses_session_split`.
- `/update-mod-graph CustomClasses` rodado ao fim desta sessão (5 arquivos de patches novos).

## Arquivo — blocos de topo pré-curadoria (2026-06-07 → 2026-06-10)

> Movidos verbatim do topo em 2026-06-11 22:49 (Sessão 7) ao aplicar a regra de snapshot-delta (`memory-curation` §6/§8). Conteúdo histórico — o estado vigente está no topo do arquivo.

## Pendências / próximos passos conhecidos (ARQUIVADO)


- **(Infra) ✅ feito** — `compile-mod.sh` estendido para C# multi-projeto/híbrido (classifica client/server/lib, builda entry projects, instala só DLLs próprias nos 2 destinos). Verificado syntax + classificação; build dotnet end-to-end só quando existirem projetos.
- **Item 001 — ENTREGUE (🟢)** via fluxo completo (spec → review-spec → spec-tech → review-tech → code-mod → compile-mod → playtest → code-review → apply-code-review). 3 arquivos em `modded/Server/`. Validado em isolamento (sem RZ): edition "Test Class" no launcher + personagem nasce **stash vazio + Endurance 5 / Strength 3**. **Base = `"SPT Zero to hero"`** (stash vazio — a classe controla os itens). Code-review 01: CR-01-02 (warn + contagem aplicada) e CR-01-03 (comentário deep clone) aplicados; CR-01-01/04 deferidos ao 002, CR-01-05 ao 007.
- **Achado importante (→ item 007 coexistência):** o `RZCustomProfiles` **clobbera** o dicionário de templates de perfil (roda depois do nosso e reconstrói/substitui), fazendo a edition do CustomClasses **sumir do launcher**. O walking skeleton foi validado com o RZ **desabilitado**. ⚠️ Para desabilitar um server mod no SPT, **mover a pasta para FORA de `user/mods`** — renomear dentro (ex.: `.disabled`) **não basta**, o SPT lê o DLL de qualquer subpasta.
- **Item 002 — ENTREGUE (🟢).** Validado in-game (log: "Loaded 2 class(es), skipped 0"; base/skills por arquivo; JSONC + `enabled` default OK). Code-review 01 aplicada (CR-01-01 doc baseEdition=vanilla, CR-01-02 `.Distinct()`, CR-01-03 `Trim()`). Loader dinâmico: lê `config/classes/*.json[c]` (`FileUtil.GetFiles`), desserializa (`JsonUtil`) o DTO `ClassDefinition` (name/enabled/baseEdition/description/skills), valida e registra cada classe; try/catch por arquivo + resumo no log. Arquivos: `ClassDefinition.cs` (novo), `CustomClassesMod.cs` (refatorado de hardcoded→loader), `config/classes/{exampleClass,testClass}.jsonc`, patch no `compile-mod.sh` (copia `config/` no install server-csharp — validado). Review 01 (5 pontos) aplicada: `Enum.IsDefined` (PA-01-01), contagem por-lado (PA-01-02), etc. Build fix: `Path` ambíguo → `System.IO.Path`. Decisões de formato: pasta `config/classes/`, identidade = campo `name`, campo `enabled` (default true), base default "SPT Zero to hero", aceita `.json`/`.jsonc`.
- **Item 003 — EM ANDAMENTO (spec funcional pronta), escopo EXPANDIDO por decisão do usuário (2026-06-07):** além de itens (stash/equipado/composto + validador), inclui **hideout** (estação por classe) E **a migração das 10 classes reais do RZ** (mesmos itens+skills, agora equipados/compostos, + hideout) — trazida do 007. **007 vira só coexistência/aposentar RZ.** Outros knobs do RZ (traders/secure/level/flags) ficam no default da base. **Decisão central aberta (p/ o tech spec):** formato JSON de itens compostos — referência a preset (globals ItemPresets) vs árvore manual vs ambos; e como mapear os loadouts "tudo no stash" do RZ para equipado/composto.
- **Item 003 — EM ANDAMENTO (🟡), implementação incremental por fatias.** Spec funcional + técnica + reviews 01 e 02 (todos aceitos/dobrados). **Fatia 1 entregue/compilada:** DTO estendido (`loadout`/`hideout`/`ItemSpec`/`ModSpec`), `HideoutBuilder` (Level + Active/Constructing), `InventoryBuilder` equipado-simples (slot-occupancy + subtree-removal). Formato JSON definido (preset+manual, mag+câmara com `ammo` obrigatório). **Fatias pendentes:** 2 preset/composto/contents, 3 carregador+câmara, 4 stash packing (GridPacker), 5 script `build-class-jsons.js` → gerar as 10 classes reais. Só então 🟢.
- **Doc canônica criada:** [docs/technical/inventario-itens-spt4.md](../../../docs/technical/inventario-itens-spt4.md) (estrutura `_id`/`_tpl`/`parentId`/`slotId`, `location {x,y,r}`, presets, munição, hideout). Skills `spt-mod-best-practices` e `csharp-mod-best-practices` agora apontam pra ela ao mexer com itens/inventário/equipamento/hideout.
- **Fatias 2-4 do 003 entregues/compiladas:** (2) preset + árvore manual; (3) carregador + câmara (`FillMagazineWithCartridge`, `ammo` obrigatório); (4) `GridPacker` (first-fit+rotação) → packing de stash + contents, stack-aware (`StackMaxSize`). `InventoryBuilder` injeta `PresetHelper`+`ItemHelper`. **Code review 01 aplicada** (CR-01-01..05: try/catch por slot, câmara só se template tem, não recarrega mag cheio, log com contagens). Tudo compila 0 warn/err (DLL ~39 KB).
- **Fatia 5 do 003 ENTREGUE + DEPLOYADA:** `scripts/build-class-jsons.js` + `class-recipes.js` geram as **10 classes reais** no formato novo. Auto-categorização via `tools/tarkov-itemdb/data/items.json` (`category.path`) → equipa 1º de cada (arma=preset+mag+câmara, pistola=Holster, armadura/capacete/rig/mochila); resto → stash (GridPacker do mod posiciona + stack-split). Ammo pareado por ordem em `primary` (ammo[0]=arma, ammo[1]=pistola). testClass removido; exampleClass → `config/classes/_docs/` (loader não-recursivo ignora). 10 `.jsonc` compiladas + instaladas no servidor.
- **Fix de playtest (presets) — 2026-06-07:** 1º playtest logou todos os presets de arma/pistola como "não encontrado". Causa: `PresetHelper.PresetCache` só é hidratado por `PresetController.Initialize()`, que roda **depois** do nosso `PostDBModLoader+1` → cache vazio. Fix: `InventoryBuilder` resolve preset direto de `databaseService.GetGlobals().ItemPresets` (default = `Items[0].Template==tpl` + `Encyclopedia!=null`) e **clona com `ICloner`** antes de re-id. Ctor agora injeta `DatabaseService`+`ItemHelper`+`ICloner` (saiu `PresetHelper`). Compila 0 warn/err. **Lição:** caches de helpers do SPT (ex.: PresetHelper) podem não estar prontos no `PostDBModLoader+1` — preferir os dicts crus do `DatabaseService`/globals.
- **Fix de playtest #2 (composto montado em todo lugar) — 2026-06-07:** itens compostos saíam "só a base". `InventoryBuilder` agora auto-completa com o **preset default** todo item que tem um, equipado E no stash (`BuildItemTree`/`RebaseClonedPreset`/`ClonePresetTree`; `PackSpecsIntoGrids` materializa árvore + posiciona pela dimensão montada via `InventoryHelper.GetItemSize`). Sem preset → simples. Confirmado no DB: presets default trazem `Soft_armor_*`/placas (armaduras) e mira/cano/coronha/bipé/mag (armas). **Nuance pendente:** acessórios premium da recipe (PSO-1, bipé) ficam soltos no stash (montar = refinamento futuro).
- **Premium na arma principal (etapa 1) — 2026-06-07:** `FirstPrimaryWeapon` equipada usa o preset mais kitado da arma (flag `premium` no `ItemSpec` + `InventoryBuilder.ResolvePremiumPreset` = max itens entre presets do tpl; gerador marca a primária). M4A1/AKM/AK74N/SV98/SAIGA12 têm presets premium (scope+foregrip+tac); AKS74U/AKMS/SAIGA9/Mosin/Makarov só base (= melhor disponível). **Etapa 2 pendente (pedido do usuário):** garantir mira mínima nas armas do **stash** (snipers tipo Mosin) — provavelmente montar/garantir optic nos presets do inventário.
- **Etapa 2 (mira mínima no stash) — 2026-06-07:** armas do stash usam `ResolveStashPreset` (menor preset com óptica real) + `EnsureMinimumOptic` (monta óptica/mount→óptica num slot vazio compatível via `_props.Slots` filter; baseclasses de óptica em `BaseClasses`). Cobertura: AKM/M4A1/AK74N/SV98/SAIGA12 (preset), AKS74U (mount→óptica). **NÃO scopáveis (template sem ponto de montagem): Mosin-infantry, AKMS, SAIGA9.** O Mosin-infantry só com swap. **Feito:** Caçador → Mosin Sniper **`5ae08f0a5acfc408fb1398a1`** (762x54R, `mod_mount`+preset com óptica); novo anchor `MOSIN_SNIPER`. ⚠️ Armadilha: o 1º candidato `5bfea6e9…` tinha nome "Mosin" mas é 7.62x51 (.308) — calibre errado; trocado. **Auditoria de calibre (arma vs munição) nas 10 classes:** swap OK; 20g do TOZ-106 OK (munição em caixa `AMMO_20_70_BUCK`=20x70 apesar do nome interno "556"); gap pré-existente do RZ (não do swap): pistola backup MAKAROV sem munição 9x18 sobressalente. Restam sem óptica: AKMS, SAIGA9. Compila 0 warn/err.
- **Munição da pistola backup + auditoria de calibre — 2026-06-07:** `backupKit` agora dá `AMMO_9x18_PST` à pistola backup MAKAROV (faltava; afetava Fuzileiro/Op.Tático com pistola primária MP443 9x19). Auditoria automática arma×munição (loose + caixas) nas 10 classes: **✓ tudo coerente**.
- **Item 003 — implementação COMPLETA (fatias 1-5 + premium/etapa1 + stash-óptica/etapa2 + Mosin swap + ammo fixes), 🟡 aguardando re-teste in-game.** Validar in-game (RZ desabilitado): log `Loaded 10 class(es)`; 10 edições no launcher; criar classe → nasce equipada (arma montada c/ mag+câmara, pistola, armadura/capacete/rig/mochila) + stash sem overflow + skills + hideout. Ressalvas: descrições pt-BR literais (i18n=008); placement heurístico (1º de cada categoria) — conferir por classe. Após OK → 🟢.
- **Code review 02 do 003 (2026-06-07):** 0 bloq. CR-02-01 (premium evita preset térmico/NV) + CR-02-02 (`PickSimpleOptic`: red dot determinístico) **aplicados**; **+ `EnsureMinimumOptic` agora roda na arma EQUIPADA também** (antes só stash) — corrige "arma principal sem mira" (AKMS/SV98-com-mount-sem-scope). Playtest: Mosin no stash veio com mira ✅; SV98 equipado veio sem mira (build anterior) → corrigido no build 51.7, requer restart + perfil novo. CR-02-03 deferido p/ 007; 04/05 opcionais.
- **Item 004 (outfits) — EM ANDAMENTO (🟡):** spec funcional + review + **tech-spec prontos**. Achados-chave da API de customization: **Head/Voice = escolha do jogador na criação** (`CreateProfileService:58/61` sobrescrevem o template — NÃO controláveis); **Body/Feet/Hands vêm do template** (controláveis); **roupas (`TemplateSide.Suits`) viram OBTAINED** via `AddSuitsToProfile` no criar perfil (`:134`) — é o que "habilita a skin" (senão fica UNAVAILABLE). Modelo: peça de roupa = entrada `_type:"Item"` no customization DB; upper traz `Body`+`Hands`, lower traz `Feet`; `_props.Side` = facção. Plano: `OutfitBuilder` por lado seta Customization + Suits, validando facção (skip-com-aviso). **Catálogo gerado:** `scripts/suits-catalog.json` (147 peças: nome↔ID↔aparência, upper/lower, USEC/BEAR). **CAPACIDADE ENTREGUE** (review-01 0 bloq + code-mod): `ClassDefinition.Outfit{usec,bear}.{upper,lower}`, `OutfitBuilder` (valida slot upper=Body/lower=Feet + facção `_props.Side`, seta Customization + Suits→OBTAINED), wiring no `RegisterClass`. Compila 0 warn/err (49.7 KB). **Pendente:** popular as 10 classes (D1 — aguardando amigo escolher skin↔classe; mapear nomes→IDs via `suits-catalog.json` e preencher `outfit`). **3 outfits de exemplo (provisórios):** Caçador (USEC Predator/ghillie), Operador Furtivo (Cultist), Saqueador (Adik). **✅ VALIDADO IN-GAME (2026-06-07):** perfil novo Caçador (TestClass6, USEC) nasceu com USEC Predator+Deep Recon **EQUIPPED** (vestido na entrada/menu) + base OBTAINED. CA atendido (skin aplicada automaticamente, não só liberada). **Lição:** outfit só aplica na CRIAÇÃO do perfil (perfil antigo não muda — confundiu o 1º teste). **Skins de mod (AllTheClothes):** têm padrão "aparência direta" (`_props.Body/Feet`=null + `_props.BodyPart=="Body"/"Feet"` → a própria peça é a malha). **OutfitBuilder ENDURECIDO (2026-06-07)** p/ cobrir vanilla E direta (usa `_props.Body/Feet` ou, na falta, o próprio id quando BodyPart casa). Compila 0 warn/err (50.2 KB). Teste in-game do caminho direto pendente. **Wire de teste:** upper do Operador Furtivo = `66a25a3af12f29d8a2599527` (AllTheClothes `top_boss_tagilla_nohead`, aparência direta) — usuário avalia no próximo teste. **Code review 01 do 004 aplicada:** CR-01-01 (try/catch no `new MongoId`), CR-01-03 (comentário); CR-01-02 (ownership aparência-direta) p/ reavaliar pós-teste. Fallback à prova de erro: equipar in-game + ler `Customization`/`Suits` do perfil.
- **Ambiente de teste (D:/SPT, NÃO é o nosso repo) — 2026-06-07:** a pedido do usuário, no mod **AllTheClothes** liguei `AllScavClothesFree`+`AllPMCClothesFree` = `true` (libera TODAS as roupas sem requisito no Ragman/Fence, p/ testar skin↔classe). Backup em `D:/SPT/SPT/user/mods/AllTheClothes/config/config.jsonc.bak-260607` (restaurar p/ reverter). É trade/runtime → vale p/ perfis existentes E novos (≠ nosso outfit, que só aplica na criação). **Bug observado (NÃO é do mod):** thumbnails de roupa brancos na tela de traders (afeta até vanilla; modelo 3D renderiza ok) → cache/render de ícone client-side, fora do escopo server.
- **Item 005 (multiplicadores de skill) — EM ANDAMENTO (🟡):** spec funcional + review + **tech-spec prontos**. Híbrido (1º projeto client BepInEx). Decisões: UI = **linha + tooltip**; **popular as 10** com multiplicadores **temáticos NOVOS** (RZ não tem XP-mult; o SKILL_MULTS dele era peso de custo), ajustáveis no JSON. **Abordagem:** server `skillMultipliers` no JSON → `SkillMultiplierRegistry` → `StaticRouter /customclasses/skill-multipliers` (resolve sessionId→edition); client BepInEx Prefix em `AbstractSkillClass.OnTrigger` (AbstractSkillClass.cs:100) escala `val` (clamp ≥0) + Postfix `SkillPanel`/`SkillTooltip` p/ `+X%/−X%`. Hook de XP confirmado no decompilado. Referência só conceitual `mods/SkillDistribution/original/` (reimplementar, GPL). Review-tech 01: 🔴1 (UI ofuscada, só Fatia 2) +🟡3+🟢1. **FATIA 1a (SERVER) ENTREGUE/compilada (54.3 KB):** `skillMultipliers` no JSON + `SkillMultiplierRegistry` (Singleton) + `SkillMultipliersRouter` (`/customclasses/skill-multipliers`, resolve sessionId→edition via `SaveServer.GetProfile().ProfileInfo.Edition`) + 10 classes com multiplicadores temáticos (buffs por skill-assinatura, em `build-class-jsons.js` SKILL_MULTIPLIERS, ajustável). PA-01-02/05 resolvidos. **Padrões confirmados:** StaticRouter+`RouteAction<EmptyRequestData>` (SkillDistribution é template real SPT4.0); `[Injectable]` default=Scoped→registry=Singleton; client fetch = `RequestHandler.GetJson` (SPT.Common.Http)+Newtonsoft. **FATIA 1b (CLIENT) ENTREGUE — mod agora HÍBRIDO.** `modded/Client/`: csproj (refs via `References/` populadas pelo compile-mod; +Newtonsoft.Json no `resolve_references`), `Plugin.cs` (BepInPlugin `customclasses.mdj.client`, dep `com.SPT.core`, config `EnableSkillMultipliers`), `SkillMultipliers.cs` (cache `ESkillId→fator`, fetch LAZY na 1ª ação via `RequestHandler.GetJson`+Newtonsoft, map case-insensitive `Enum.TryParse<ESkillId>` — PA-01-03/04), `Patches/OnTriggerPatch.cs` (Prefix `AbstractSkillClass.OnTrigger`, `val*=fator` clamp≥0, try/catch). Compila híbrido 0 warn/err: Client→`BepInEx/plugins/CustomClasses` (8KB), Server→`user/mods/CustomClasses` (54KB). **Erro resolvido:** faltava ref `UnityEngine.dll` (MonoBehaviour) no csproj. **Fatia 1c (gym):** `WorkoutBehaviourPatch` (Prefix/Postfix snapshot/delta em `WorkoutBehaviour.method_18` — gym usa SetCurrent direto, não OnTrigger) escala o XP de treino. **Fatia 2 (UI):** `SkillPanelPatch` (acende seta verde/vermelha na linha via `_effectivenessUp/Down`) + `SkillTooltipPatch` (anexa "XP da classe: +X%" no maior TMP_Text do tooltip). `SkillPanel`/`SkillTooltip` são tipos NOMEADOS em `EFT.UI` (não ofuscados — PA-01-01 resolvido; dump local era parcial). Refs add no compile-mod: Sirenix.Serialization (SerializedMonoBehaviour), Unity.TextMeshPro, UnityEngine.UI, Newtonsoft.Json. **005 IMPLEMENTADO POR COMPLETO** (1a server + 1b client XP + 1c gym + 2 UI), compila híbrido 0 warn/err (client 12 KB). Aguarda playtest → 🟢. Lição: dump `eft-decompiled` é PARCIAL — validar membros via compilação contra `Assembly-CSharp.dll` (References) + usar o mod de referência (SkillDistribution) como fonte de membros.
- **(Próximo, depois do 005)** 006 (compat SE), 007 (coexistência/aposentar RZ — inclui mover anchors p/ o mod, CR-02-03), 008 (i18n), 009 (ocultar edições vanilla). RZ desabilitado em `mods-disabled`.
- Backlog completo (001-008) em [../backlog/mod-backlog.md](../backlog/mod-backlog.md).

## Mudança de fluxo dos ITENS (003) — 2026-06-07 (decisão do usuário)

- **Novo processo de itens/armas:** o usuário vai **montar perfis personalizados in-game** (loadout completo: armas configuradas do jeito que quer, equipamento, stash) e **enviar o profile JSON**. Eu vou **extrair o inventário (`Inventory.Items`)** desse profile e usar como **base** pra definir os itens equipados/compostos de cada classe — em vez do gerador heurístico atual (`build-class-jsons.js` mapeando recipes→slots). Bulletproof (game-validated). Por isso **não investir mais** em corrigir a óptica/preset automático do `InventoryBuilder` (CR-02-01/02 viram menos relevantes; o stash/equipado virá do profile real). O `InventoryBuilder` (montar árvore no template) continua válido como **mecanismo**; só a FONTE dos dados muda (profile real, não recipes). Aguardando o(s) profile(s) do usuário.

## Riscos abertos (deferidos aos spec-tech)

- Launcher v1 vs v2 (`LauncherController` vs `LauncherV2Controller`) e formato exato de `ProfileSides`/`TemplateSide`.
- Como client/server descobrem a classe do perfil p/ multiplicadores (provável `ProfileInfo.Edition`).
- Sourcing de presets de arma (globals `ItemPresets`) e de suits/customization (DB customization) — não estão no tarkov-itemdb.
- Mapeamento pt-BR → `pt` + inclusão em `ServerSupportedLocales`.
- Capacidade do stash (lição de overflow do RZ).


# Épico — Editor web de classes (itens 018–029) · Code Review consolidado 01

**Mod:** CustomClasses
**Escopo:** itens 018–029 (schema doc, guard rails, infra Blazor, registrar/editor service, catálogo/custo, pickers, viewer, edição, lifecycle, stash, docs)
**Data:** 2026-06-10

> Code review CONSOLIDADO focado em **bugs de correção** (não estilo). Método: leitura integral dos arquivos novos/modificados + confirmação em callers/callees reais (`InventoryBuilder`, `OutfitBuilder`, `HideoutBuilder`, `FileUtil`/`JsonUtil` do spt-source, `docs/class-schema.md`). IDs `CR-EP-NN`. Severidades: **CR-BLOQ** (quebra real) · **CR-MAIOR** (bug em caso plausível) · **CR-MENOR** (edge raro / robustez).

## Resumo

> 🔴 CR-BLOQ: 0 · 🟠 CR-MAIOR: 2 · 🟢 CR-MENOR: 9 · Total: 11

## Índice

| ID | Severidade | Categoria | Título | Status |
| --- | --- | --- | --- | --- |
| CR-EP-01 | 🟠 CR-MAIOR | B — Bug latente | Stash/contents: editor expõe e CostService precifica campos que o builder ignora (`preset`/`mods`/`ammo`/`contents` aninhado) — linha de stash em modo Preset não spawna NADA | ✅ Aplicado (builder estendido) |
| CR-EP-02 | 🟠 CR-MAIOR | B — Bug latente | Classe deletada pelo editor **ressuscita** no próximo `compile-mod` (repo-only ≠ "novo"; sync-classes não propaga deleções) | ✅ Aplicado |
| CR-EP-03 | 🟢 CR-MENOR | B — Bug latente | `Commit`: claim "single reference write" não vale para INSERT de chave nova (resize do `Dictionary` lido concorrentemente) | ⚪ Aceito como dívida |
| CR-EP-04 | 🟢 CR-MENOR | C — Gap vs. spec | `CostService`: `count > 1` em slot equipado é precificado, mas o builder ignora (regra 17 do schema) | ✅ Aplicado |
| CR-EP-05 | 🟢 CR-MENOR | B — Bug latente | `LocalizedTextConverter.Read`: valor não-string aninhado em `{en,pt}` dessincroniza o reader (falta `Skip()`) | ✅ Aplicado |
| CR-EP-06 | 🟢 CR-MENOR | B — Bug latente | Dois arquivos com o mesmo `name`: dry-run do editor (`allowReplace=true`) mascara a colisão que o boot acusa | ✅ Aplicado |
| CR-EP-07 | 🟢 CR-MENOR | B — Bug latente | `ItemPicker`: `FilterTpls` aplicado DEPOIS do cap do `Search` — itens compatíveis podem ficar invisíveis | ✅ Aplicado |
| CR-EP-08 | 🟢 CR-MENOR | C — Gap vs. spec | `CostService.AddAmmo`: `chambered` soma 1 sem checar câmara; `mod_magazine` de árvore manual só é procurado no 1º nível | ✅ Aplicado |
| CR-EP-09 | 🟢 CR-MENOR | B — Bug latente | Guard do `compile-mod.sh`: `*.json` na RAIZ de `config/` não é guardado (só `*.jsonc`) — clobber silencioso se um dia existir | ✅ Aplicado |
| CR-EP-10 | 🟢 CR-MENOR | D — Arquitetura | Ícones em DUAS localizações (server `wwwroot/icons` × client `BepInEx/.../icons`) — dropdown do editor pode oferecer ícone que in-game não existe | ⚪ Deferido (housekeeping) |
| CR-EP-11 | 🟢 CR-MENOR | E — Doc | `class-schema.md` §6 "Sem hot-reload" ficou falso com o hot-apply do editor (corrigir no item 029) | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/Doc** · **F — Melhoria**

---

## Pontos

### CR-EP-01 · B — Bug latente · 🟠 CR-MAIOR

**Stash/contents: editor expõe e CostService precifica campos que o builder ignora — linha de stash em modo Preset não spawna NADA**

**Local:**
- [`modded/Server/Web/Shared/ItemSpecEditor.razor`](../../modded/Server/Web/Shared/ItemSpecEditor.razor) (usado pelo tab Stash de `ClassEdit.razor` e recursivamente em Contents)
- [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddSpec` (linhas ~233–298)
- Confirmado contra [`modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `PackSpecsIntoGrids` (linha ~550: `if (IsNullOrWhiteSpace(spec.Tpl)) → pulado`; nunca lê `Preset`/`Mods`/`Ammo`/`Contents`) e [`docs/class-schema.md`](../../docs/class-schema.md) §4.2/§6 ("stash/contents honram **apenas `tpl` + `count`**").

**Problema (3 faces do mesmo bug de paridade):**
1. **Linha morta:** o tab Stash usa o `ItemSpecEditor` completo. Trocar para o modo "Preset" zera `Tpl` (`SwitchModeAsync`) → a linha salva só com `preset` → `PackSpecsIntoGrids` a **pula inteira** no registro. O capacity dry-run avisa ("line without 'tpl'"), mas o editor deixou criar e o **custo conta a arma inteira** (`AddSpec` resolve `spec.Preset` para stash também).
2. **Contents fantasma:** o editor permite `Contents` em item de stash (e contents de contents até 6 níveis); o builder **não recursa** contents em stash nem em contents — itens somem silenciosamente no registro, mas são precificados pelo `CostService` e não geram warning no capacity check.
3. **Ammo/mods fantasma:** `loadedMag`/`chambered`/`mods` configuráveis em linhas de stash/contents; builder ignora, custo soma (`AddAmmo`/`AddModTree`).

**Cenário concreto:** usuário adiciona "M4A1 (modo Preset, premium, loadedMag)" no Stash e uma mochila com 3 itens em Contents → "Loadout total" sobe ~150k ₽, save passa limpo; no jogo o perfil novo nasce **sem a M4 e sem os 3 itens** (a mochila vem vazia).

**Por que importa:** o épico vende "custo espelha o que o builder spawna" (doc do `CostService`) — aqui o custo e a UI mentem juntos sobre o resultado in-game.

**Sugestão:** no contexto stash/contents (a) `ItemSpecEditor` ganhar um modo restrito (`AllowPreset=false`, sem ammo/mods, contents só se o builder passar a suportar); (b) `CostService.AddSpec(equipped:false)` honrar a semântica documentada (só `tpl`+`count` + preset auto via `ResolveStashPreset`); (c) `CheckStashCapacity` emitir warning também para contents/ammo/mods presentes em linha de stash.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Aceitar com modificação: em vez de RESTRINGIR o editor/custo, o **builder foi estendido** (direção inversa, mais valor): `InventoryBuilder.PackSpecsIntoGrids` agora honra a semântica completa em stash/contents — `preset` explícito (premium-aware, mesma resolução do equipado), árvore manual (`mods`), `LoadAmmo` em árvore composta e `contents` recursivo após `PlaceTree` (helper novo `PlaceSpecTrees`; caminho tpl+count puro inalterado). `CostService.ComputeLoadoutCost` e `CheckStashCapacity` revisados em paridade com o builder final (probe de árvore manual p/ medição; contents/ammo não consomem células do stash). Docs atualizadas (`class-schema.md` §3/§4.2/§5/§6; as-builts 026/028).
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-02 · B — Bug latente · 🟠 CR-MAIOR

**Classe deletada pelo editor ressuscita no próximo `compile-mod`**

**Local:**
- [`.agents/scripts/compile-mod.sh`](../../../../.agents/scripts/compile-mod.sh) — `install_server_config` (caminho sem divergência: `cp -r "$src" "$dest_root/"`; caminho divergente: loop `repo_only` com comentário "copying them clobbers nothing")
- [`scripts/sync-classes.sh`](../../scripts/sync-classes.sh) — itera **apenas** `list_config_files "$INSTALL_CONFIG"` (deleções no install nunca aparecem)

**Problema:** `ClassEditorService.Delete` apaga o `.jsonc` só no INSTALL. O arquivo segue existindo no repo. No próximo build: (a) se nenhum outro arquivo diverge, `cp -r` recopia tudo (incluindo o deletado); (b) se há divergência, o guard ainda copia os "repo-only" assumindo que são arquivos NOVOS — mas um repo-only também pode ser uma **deleção feita no install**. E o `/sync-classes` não tem como propagar a deleção para o repo (só compara arquivos presentes no install). Resultado: a edition deletada volta ao launcher no próximo boot, sem nenhum aviso de clobber.

**Cenário concreto:** deletar "Saqueador" pelo editor (com confirmação e scan de perfis) → ajustar 1 linha de C# → `/compile-mod` → reiniciar servidor → "Saqueador" está de volta.

**Por que importa:** anula o fluxo de delete do item 027; o usuário acredita que o guard anti-clobber (item 019) protege as edições do editor — deleção também é uma edição.

**Sugestão:** tratar deleção como divergência: se `rel` existe no repo e NÃO existe no install **mas existe `<rel>.bak1` no install** (marca de que o editor mexeu lá), não recopiar e listar como "DELETED in install — run /sync-classes". No `sync-classes.sh`, listar arquivos repo-only com `.bak1` no install e oferecer a remoção no repo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Aceitar com modificação: sem depender do marcador `.bak1` (mais simples e mais seguro): **qualquer** arquivo de `config/classes/` que existe só no repo BLOQUEIA a cópia do config (`compile-mod.sh` — mensagem "repo-only — new class in repo OR deleted via editor in install; use --force-config to copy, or sync-classes to propagate the deletion"); arquivos repo-only na RAIZ de `config/` mantêm o copy-as-new (não há fluxo de delete do editor pra eles). `sync-classes.sh` detecta arquivos de `classes/` que existem só no repo e oferece **remover a cópia do repo** ("DELETED in install"), respeitando `--dry-run`/`--yes`.
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-03 · B — Bug latente · 🟢 CR-MENOR

**`Commit`: "single reference write" só vale para REPLACE; INSERT de chave nova pode redimensionar o dict sob leitores concorrentes**

**Local:** [`modded/Server/ClassRegistrar.cs`](../../modded/Server/ClassRegistrar.cs) — `Commit` (linha ~282) e doc da classe.

**Problema:** `databaseService.GetProfileTemplates()[plan.Name] = plan.Sides` em chave **existente** é efetivamente atômico (sobrescreve o slot do value). Mas no **primeiro** hot-apply de uma classe (Create do item 027, ou re-enable) a chave é nova → `Dictionary.Add` pode disparar resize/realocação de buckets enquanto `ProfileHelper.GetProfileTemplateForSide` ou os routers leem o mesmo dict em outra thread (o Save roda em `Task.Run`). O mesmo vale para `SkillMultiplierRegistry`/`ClassVisualRegistry` (Dictionaries puros). O kickoff do 021 ACEITA races de hot-apply, mas o comentário promete uma atomicidade que o insert não tem — risco de `InvalidOperationException`/leitura corrompida raríssimo em servidor local single-user.

**Cenário concreto:** criar classe no editor exatamente enquanto o launcher pede `/launcher/server/connect` (enumera as editions).

**Sugestão:** documentar a exceção no comentário, ou trocar os 2 registries do mod por `ConcurrentDictionary` (templates é do SPT — fica como race aceito documentado).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): concorrência residual ACEITA — servidor local single-user, janela de race ínfima (criar classe exatamente durante um connect do launcher), consequência recuperável (retry). Coberto pelo race aceito do kickoff 021.

### CR-EP-04 · C — Gap vs. spec · 🟢 CR-MENOR

**`CostService`: `count > 1` em slot equipado é precificado N×, builder spawna 1×**

**Local:** [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddSpec` (`var count = Math.Max(1, spec.Count)` vale também para `equipped:true`; `AddPresetItems(preset, count)`, `AddSimple(tpl, count)`, `AddAmmo(... rounds * count)`).

**Problema:** o builder ignora `count` em equipado (regra 17 do schema, log debug em `InventoryBuilder.Apply`); o custo multiplica. O editor não expõe `count` em equipado (`AllowCount=false`), então só dispara via JSON editado à mão — mas o viewer/custo (024/022) também processa arquivos manuais.

**Cenário concreto:** `"FirstPrimaryWeapon": { "preset": "...", "count": 3 }` num jsonc manual → loadout total conta 3 armas; o perfil nasce com 1.

**Sugestão:** em `AddSpec`, `var count = equipped ? 1 : Math.Max(1, spec.Count);` (+ warning quando `spec.Count > 1 && equipped`, espelhando o log do builder).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado: equipado com `count > 1` é precificado 1× + warning informativo no breakdown ("the builder spawns 1 (schema rule 17); costed as 1").
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-05 · B — Bug latente · 🟢 CR-MENOR

**`LocalizedTextConverter.Read`: valor aninhado não-string em `{en,pt}` dessincroniza o reader**

**Local:** [`modded/Server/LocalizedText.cs`](../../modded/Server/LocalizedText.cs) — `Read`, caso `StartObject` (linhas ~49–67).

**Problema:** após `reader.Read()` no valor da property, se o token for `StartObject`/`StartArray` (ex.: `"displayName": { "en": { "x": 1 } }`), o conteúdo aninhado NÃO é consumido (`val = null` e segue). O `while (... != EndObject)` então termina no `EndObject` do objeto ANINHADO, deixando o reader desalinhado → exceção de parse confusa (ou propriedades seguintes interpretadas erradas) em vez de um erro claro. Só dispara com JSON malformado à mão; o editor nunca gera isso.

**Sugestão:** quando o token do valor não for `String`/`Null`, chamar `reader.Skip()` antes de continuar o loop.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado: valor não-string em `{en,pt}` agora é consumido via `reader.Skip()` (em token escalar é no-op), mantendo o reader alinhado com o `EndObject` externo.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-06 · B — Bug latente · 🟢 CR-MENOR

**Dois arquivos com o mesmo `name`: o editor não acusa a colisão que o boot acusa**

**Local:** [`modded/Server/ClassEditorService.cs`](../../modded/Server/ClassEditorService.cs) — `ListClassFiles` (dry-run com `allowReplace: true`) + [`modded/Server/ClassRegistrar.cs`](../../modded/Server/ClassRegistrar.cs) — guard de colisão (`allowReplace && classVisualRegistry.Contains(name)`).

**Problema:** no boot, o 2º arquivo com o mesmo `name` é pulado com `EditionCollision` (regra 5). No editor, `allowReplace=true` + o nome já estar no `ClassVisualRegistry` (registrado pelo 1º arquivo) faz o dry-run do 2º arquivo passar limpo — ambos aparecem "Registered" na lista, e salvar qualquer um hot-aplica por cima da MESMA edition. O usuário não tem como saber qual arquivo "vence" no próximo boot (o 2º em ordem alfabética será pulado).

**Cenário concreto:** copiar `cacador.jsonc` → `cacador-old.jsonc` à mão (sem renomear o `name` interno) e abrir o editor.

**Sugestão:** em `ListClassFiles`, detectar `name` duplicado entre os arquivos da pasta e injetar um diagnostic Warning/Error próprio (ex.: `DuplicateClassName`) nos dois arquivos.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado: `ListClassFiles` agrupa por `name` (trim, case-insensitive) e injeta **Error** `DuplicateClassName` (code novo em `DiagnosticCodes`) em TODOS os arquivos envolvidos, listando-os na mensagem.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-07 · B — Bug latente · 🟢 CR-MENOR

**`ItemPicker`: `FilterTpls` aplicado depois do cap do `Search` — resultados compatíveis podem sumir**

**Local:** [`modded/Server/Web/Shared/ItemPicker.razor`](../../modded/Server/Web/Shared/ItemPicker.razor) — `RunSearchAsync` (`Catalog.Search(query, categoryId, Limit)` corta em `Limit`; `FilterTpls` filtra DEPOIS).

**Problema:** o `Search` para nos primeiros `Limit` (100) matches do banco; o filtro de compatibilidade de slot (item 026: `IsAllowedInEquipmentSlot`/`IsAllowedInSlot`) roda sobre esse subconjunto. Buscas genéricas ("scope", "magazine") em slots restritivos podem retornar 0 itens visíveis mesmo havendo dezenas compatíveis fora do cap — o usuário conclui que "não existe item compatível".

**Sugestão:** passar o predicado para dentro do `CatalogService.Search` (filtrar ANTES de cortar no limit), ou aumentar o fetch interno quando `FilterTpls != null` (ex.: buscar `Limit*10` e cortar depois do filtro).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado (1ª opção): `Search` ganhou parâmetro opcional `Func<string,bool>? filter`, aplicado ANTES do cap; `ItemPicker.RunSearchAsync` repassa `FilterTpls` (capturado em local p/ o `Task.Run`) e o pós-filtro foi removido.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-08 · C — Gap vs. spec · 🟢 CR-MENOR

**`CostService.AddAmmo`: dois desvios mínimos vs `InventoryBuilder.LoadAmmo`**

**Local:** [`modded/Server/CostService.cs`](../../modded/Server/CostService.cs) — `AddAmmo` (linhas ~340–405).

**Problema:** (a) `chambered` soma 1 cartucho sem verificar se a arma TEM slot de câmara (`_props.Chambers`) — o builder ignora com warning (regra 22); (b) para árvore manual, o magazine é procurado só no 1º nível de `spec.Mods` (`spec.Mods?.FirstOrDefault(m => m.SlotId == "mod_magazine")`), enquanto o builder varre a árvore inteira (`tree.FirstOrDefault(i => i.SlotId == "mod_magazine")`) — um mag aninhado (incomum, mas válido) seria carregado pelo builder e não precificado. Impacto: centavos (1 cartucho / 1 mag fill).

**Sugestão:** (a) só somar a câmara quando `Chambers` existir no template; (b) busca recursiva por `mod_magazine` em `spec.Mods`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado: (a) `chambered` só conta quando o template raiz (preset root ou `spec.Tpl`) declara `_props.Chambers` (warning espelhando CR-01-02); (b) `FindMagazine` recursivo (depth-first) sobre `spec.Mods`. Bônus de paridade CR-EP-01: linha de stash SIMPLES (sem preset/mods) com flags de ammo não é mais precificada (o builder não carrega ammo fora de árvore montada) — warning explícito.
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-09 · B — Bug latente · 🟢 CR-MENOR

**Guard do `compile-mod.sh`: `*.json` na raiz de `config/` não é coberto**

**Local:** [`.agents/scripts/compile-mod.sh`](../../../../.agents/scripts/compile-mod.sh) — `list_config_files` (raiz: `find "$dir" -maxdepth 1 -type f -name '*.jsonc'`; classes/: json+jsonc). Mesmo gap em [`scripts/sync-classes.sh`](../../scripts/sync-classes.sh).

**Problema:** um arquivo `config/*.json` (raiz, extensão `.json` em vez de `.jsonc`) editado no install não contaria como divergência → o `cp -r` do caminho "sem divergência" o sobrescreveria sem aviso, e o `sync-classes` não o traria de volta. Hoje a raiz só tem `hidden-editions.jsonc`, então é latente — mas é exatamente o tipo de buraco que o item 019 existe para fechar. Nota menor adicional: o `sed "s|^$dir/||"` usa `$dir` cru como regex (metachars como `.` casam qualquer char) — inócuo nos paths atuais.

**Sugestão:** incluir `-name '*.json'` no find da raiz nos dois scripts (1 linha cada).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado nos dois scripts: `list_config_files` da raiz agora cobre `*.json` + `*.jsonc` (classes/ já cobria ambos). O nit do `sed` com `$dir` cru segue inócuo nos paths atuais (não alterado).
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

### CR-EP-10 · D — Arquitetura · 🟢 CR-MENOR

**Ícones em duas localizações: dropdown do editor (server `wwwroot/icons`) × jogo (client `BepInEx/plugins/CustomClasses/icons`)**

**Local:** [`modded/Server/Web/Pages/ClassEdit.razor`](../../modded/Server/Web/Pages/ClassEdit.razor) — `LoadCatalogs` (enumera `wwwroot/icons` do INSTALL do server) vs. doc do campo em [`modded/Server/ClassDefinition.cs`](../../modded/Server/ClassDefinition.cs) (`iconFile` = PNG em `BepInEx/plugins/CustomClasses/icons/` do CLIENT) e o texto do `ClassLifecycleCreateDialog`.

**Problema:** são duas cópias físicas (repo: `Server/wwwroot/icons` e `Client/icons`; `compile-mod` instala cada uma no seu destino). Se divergirem (PNG adicionado só num lado), o editor mostra/oferece um ícone que o jogo não resolve (skill panel degrada para texto) — ou vice-versa, um ícone que funciona in-game não aparece no dropdown. Nenhuma validação cruza as duas listas.

**Sugestão:** ter UMA fonte no repo (ex.: `Client/icons/`) e o `compile-mod` copiar para os dois destinos; ou o editor avisar quando o `iconFile` selecionado não existir na pasta do client.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): DEFERIDO — housekeeping de ícones client×server fica para item futuro (as duas cópias hoje são geradas pelo mesmo pipeline do item 011 e estão em sincronia; risco só ao adicionar PNG manualmente num lado).

### CR-EP-11 · E — Doc · 🟢 CR-MENOR

**`class-schema.md` §6 "Sem hot-reload: mudanças nos JSONs exigem reinício do servidor" ficou falso**

**Local:** [`docs/class-schema.md`](../../docs/class-schema.md) §6 "Limites conhecidos".

**Problema:** com os itens 021/025+ o save do editor hot-aplica (Commit/Remove) sem restart — o limite documentado vale só para edições feitas À MÃO no arquivo (que continuam exigindo restart, pois não há file-watcher). A doc canônica (item 018, status ✅) contradiz o comportamento entregue pelo épico.

**Sugestão:** no item 029, reescrever o bullet: "edições à mão exigem reinício; saves pelo editor web hot-aplicam para perfis NOVOS (perfis existentes e client aberto não mudam)". Aproveitar e registrar as regras novas do `ClassRegistrar` (allowReplace, hot-remove em `enabled:false`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão — aplicado em `class-schema.md`: §1 (adicionar classe = arquivo+restart OU editor web) e §6 (bullet reescrito: boot carrega uma vez; save do editor hot-aplica via `ClassRegistrar.Commit`/hot-remove em `enabled:false`; arquivo solto/editado À MÃO não registra sozinho até restart ou save daquele arquivo pelo editor; hot-apply vale p/ perfis novos).
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Verificado e OK (sem achado)

Pontos investigados a fundo que **não** geraram achado — registrados para não re-investigar:

- **Paridade de boot (021):** `CustomClassesMod.OnLoad` → `ValidateAndBuild(allowReplace:false)` + `Commit` reproduz as regras 1–31 do `class-schema.md` (ordem das validações, trims, logs, contagem loaded/skipped, disabled = Info skipped). `ValidateAndBuild` é puro de fato: builders mutam apenas o clone (`ICloner.Clone` deep), nada de templates/registries/`def` (conferido em `InventoryBuilder`, `OutfitBuilder`, `HideoutBuilder`); `Commit` não retém referências do `def` (Visual/multipliers são cópias).
- **Round-trip do JSON:** `JsonUtil` usa `WhenWritingNull` (nulls omitidos) + `ReadCommentHandling.Skip`; `LocalizedTextConverter.Write` preserva as duas formas (string legada ↔ objeto); `ClassEditModel.FromDefinition/ToDefinition` cobre todos os campos do schema; load→save-sem-editar não perde campo (defaults explícitos `count:1`/`premium:false` e perda de comentários JSONC são caveats documentados, `.bak1` preserva).
- **Hot-apply:** `Remove` limpa templates + os DOIS registries e recusa editions não-próprias; `Save(enabled:false, hotApply)` remove; replace de chave existente é atômico na prática; `Duplicate` usa `with` raso mas sobre um parse fresco sem refs retidas — sem mutação cruzada.
- **Filesystem:** rotação `.bak3→.bak2→.bak1` na ordem correta; `Directory.GetFiles("*.json"/"*.jsonc")` (extensões de 4–5 chars) NÃO captura `.bak1`/`_audit.log`; `Slugify` + `UniqueClassFileName` (checa .json E .jsonc) sem colisão; `ProfilesUsingEdition` resolve `user/profiles` corretamente e usa `JsonDocument` streaming.
- **Custo:** sem double-count de cartuchos (preset com cartuchos → fill pulado, espelha CR-01-03 do builder); moeda a valor de face; `SkillWeights.ResolveWeight` nunca retorna 0 (explicit → derived → mediana → 1.00); `CheckStashCapacity` espelha o split por `StackMaxSize` e o first-fit/rotação do `GridPacker`.
- **Blazor/threading:** continuações pós-`await Task.Run` voltam ao sync context do circuit (mutação de estado segura); MudBlazor debounce sem timer manual/`DotNetObjectReference` para descartar; serviços singleton novos (`CatalogService`/`CostService`/`ClassEditorService`) são stateless fora do race aceito dos registries; `_searchVersion` no `ItemPicker` descarta buscas obsoletas corretamente.
- **Scripts:** quoting de paths com espaço OK nos dois .sh; `cmp`/`diff | head || true` seguros sob `set -euo pipefail`; normalização CRLF simétrica entre guard, sync e freeze do gerador; freeze (`build-class-jsons.js`) compara conteúdo normalizado de forma estável e só escreve quando idêntico ou `--force`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-10 | Claude (code review) | Criação — review consolidado do épico 018–029 (0 BLOQ, 2 MAIOR, 9 MENOR). |
| 2026-06-10 | Claude (apply-review) | Fixes aplicados: 01 (builder estendido — stash/contents honram preset/mods/ammo/contents; Cost/Capacity em paridade), 02 (guard bloqueia repo-only em classes/; sync-classes propaga deleção), 04, 05, 06, 07, 08, 09, 11. Aceitos sem fix: 03 (race residual — single-user local), 10 (deferido — housekeeping de ícones). Build Release 0 err/0 warn; freeze do gerador 11 unchanged. |

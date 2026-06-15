# CustomClasses — Editor web de classes (guia de uso)

> **Data:** 2026-06-10<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-schema.md](./class-schema.md), [mod-backlog.md](../backlog/mod-backlog.md)<br>

---

Guia do **editor web de classes** entregue no épico 018–028 (Blazor Server + MudBlazor, embutido no próprio mod via `IModWebMetadata`/`SPTarkov.Server.Web`). O editor roda **dentro do servidor SPT** — sem processo extra — e lê/escreve os `.jsonc` de classe do **install** (`user/mods/CustomClasses/config/classes/`). O formato dos arquivos é o de [class-schema.md](./class-schema.md).

## 1. Acesso

1. Suba o servidor SPT normalmente (`SPT.Server.exe`).
2. Abra no browser: `https://<ip>:6969/customclasses`.
   - O `<ip>` é o `ip` de `SPT_Data/configs/http.json` (atual `127.0.0.1`), **mas mods podem sobrescrever o bind**: o **fika-server** dita o IP efetivo via `server.ip` em `user/mods/fika-server/assets/configs/fika.jsonc`. **Os dois precisam bater** — no install atual ambos estão em `127.0.0.1`, então o editor responde em `https://127.0.0.1:6969/customclasses`. (Para expor na LAN/Radmin, troque `server.ip` do fika **e** o `ip` do http.json para o mesmo IP, ex.: `26.207.194.149`.)
3. O certificado é **self-signed** → o browser bloqueia na primeira visita. Aceite a exceção ("Avançado → Continuar"); no Chrome sem botão, digite `thisisunsafe` na página de aviso.

### Rotas

| Rota | Página |
|---|---|
| `/customclasses` | Home (card "Class editor" + smoke test de editions) |
| `/customclasses/classes` | **Lista de classes** — ícone, nome colorido, status (Registered/Disabled/Invalid/Not registered + diagnostics em tooltip), nº de skills, custo de skills vs. budget, loadout ₽, arquivo. Toolbar "New class" + ações **Edit/Duplicate/Delete** por linha. Colunas **Class / Skill cost / Loadout** são ordenáveis (clique no header; ordenação persistida — ver §7). |
| `/customclasses/classes/{arquivo}` | **Workspace SEMPRE em edição** (não há mais modo "view" — redesign F1/F2). 3 painéis de largura igual p/ skills e equipped + stash justo à direita: **esquerda** skills + XP multipliers + hideout + outfit; **centro** silhueta do personagem (paper doll, layout fiel à tela Gear do EFT, slots por tipo); **direita** grade 2D do stash (itens no tamanho W×H). Editar é IN-PLACE: skills/multipliers/hideout editáveis, outfit abre o **seletor visual de skins** (dialog), clicar num slot da silhueta abre o **editor daquele item** (dialog), o stash vira **drag-and-drop 2D** (arrastar move, ⟳ rotaciona). Header slim com custo ao vivo + **Save/Discard** (só habilitam quando há mudança — chip "unsaved") + guard de não-salvo + **`Ctrl+S`**. Modo **"Compare with…"** (A×B read-only, item 036; deep-link `?compare=<arquivo>`) ainda usa o dashboard 2-col antigo. Duplicate/Delete no header da página. |
| `/customclasses/classes/{arquivo}/edit` | Alias da rota acima (ambas abrem o mesmo workspace já em edição). Preservada p/ deep-links antigos. |
| `/customclasses/skills` | **Matriz de skills** (item 032) — skills (linhas, ordem canônica) × classes (colunas), heatmap por tier; toggles "Mostrar desabilitadas" / "Multiplicadores XP" (persistidos). Clicar numa célula abre o **workspace de edição** da classe (item 035; sem abas desde o redesign F2). |
| `/customclasses/picker-test` | Harness de dev dos pickers (item 023) — sem link no menu, só URL direta. |

### Sidebar de classes (drawer)

Desde o item 030 o drawer esquerdo é uma **sidebar persistente** de classes: lista toda classe (ícone tintado + nome colorido + custo de skills + dot de status), com filtro por nome e **troca 1-clique** preservando a vista atual (detail→detail, edit→edit) e a **aba ativa** do edit (item 035 — comparar a mesma aba entre classes sem recliques). Cada item tem uma ação **Edit** direta (hover). Home / Classes / Skills matrix ficam no topo como utilitários. Guard de mudanças não salvas (item 030) intercepta a troca quando o form está sujo.

`{arquivo}` = nome do `.jsonc` sem extensão (ex.: `cacador`).

### O que dá pra editar

Desde o redesign F2 a edição é **in-place no workspace** e **sempre ativa** (não há mais modo "view" nem abas): abrir uma classe já entra em edição; **Save/Discard** só habilitam quando há mudança real (chip "unsaved") e o guard de não-salvo protege a saída.

- **Skills** (painel esquerdo): níveis 0–51 com peso/custo por linha e total vs. budget ao vivo.
- **Multipliers** (painel esquerdo): fatores de XP por skill (≥ 0; verde = buff, vermelho = debuff; badge p/ skills do Skills-Extended).
- **Hideout** (painel esquerdo): nível inicial por estação.
- **Outfit** (painel esquerdo): clicar num dos 4 cards (USEC/BEAR × upper/lower) abre o **seletor visual de skins** (galeria com thumbnail/glyph + nome, todas as skins vanilla + mods); "Use template default" limpa.
- **Equipped** (centro, silhueta): clicar num dos 14 slots abre o **editor daquele item** (modos item/preset, premium, ammo loadedMag/chambered, árvore de mods recursiva, contents de contêiner) num dialog; "Clear slot" remove.
- **Stash** (direita, grade 2D): **arrastar** um item reposiciona (célula calculada pelo cursor; colisão bloqueada), botão **⟳** rotaciona. Posições (`x`/`y`/`rotated`) são **opt-in** — só itens efetivamente movidos/rotacionados ganham coordenadas; o resto continua auto-empacotado. Honradas in-game pelo builder.
- **General** (campos simples — displayName/description en/pt, cor do nome, `enabled`, `baseEdition`, `iconFile`): hoje editados via os fluxos de lifecycle/lista; `name` é **read-only** (chave da edition — ver limite 4).
- **Lifecycle:** criar (template mínimo → abre na edição), duplicar (cópia verbatim com nome novo — **é o caminho oficial de rename**) e deletar/desabilitar (com varredura de perfis existentes que usam a edition + confirmação).

### Save = hot-apply

**Save** valida (erro bloqueia sem escrever nada), grava o `.jsonc` no install com backup rotativo (`.bak1`–`.bak3` ao lado do arquivo), registra no audit (`config/classes/_audit.log`) e **re-registra a edition a quente** (build-then-swap). Resultado: a classe nova/alterada aparece no **launcher imediatamente, sem reiniciar o servidor** — mas leia os limites abaixo.

## 2. Fluxo install ↔ repo

O editor escreve nos `.jsonc` do **INSTALL** (`D:/SPT/SPT/user/mods/CustomClasses/config/classes/`). O repo não muda sozinho — o ciclo é:

```
editor (install) ──/sync-classes──▶ repo ──commit──▶ git
repo ──/compile-mod──▶ install            (config só com guard; --force-config força)
```

- **`/sync-classes`** (`scripts/sync-classes.sh`): traz `config/classes/*.json[c]` + `config/*.jsonc` do install pro repo, com diff preview por arquivo, `--dry-run` e `--yes`. **Rode (e commite) logo depois de editar no editor** — edição pendente só no install é frágil.
- **Guard anti-clobber do `/compile-mod`**: antes de copiar `config/` repo→install, o `compile-mod.sh` compara `config/classes/` dos dois lados (conteúdo normalizado); divergência → **aborta a cópia de config** listando os arquivos e o lado mais novo. `--force-config` sobrescreve repo→install deliberadamente (use só quando o repo é a verdade). DLL e `wwwroot/` instalam sempre.
- **Gerador congelado**: `scripts/build-class-jsons.js` virou **bootstrap-only** — não sobrescreve `.jsonc` que divergiu do que ele geraria (`skipped (frozen)`); `--force` regenera. Depois do editor, a fonte de verdade dos `.jsonc` é o arquivo em si, não o gerador.

## 3. Os 4 limites do editor — leia antes de usar

> Estas são as regras que mais surpreendem. Nenhuma é bug.

1. **Hot-apply vale para PERFIS NOVOS.** Salvar re-registra a edition → o launcher já lista/aplica a versão nova na **criação de perfil**, sem reiniciar o servidor. Porém o **client com o jogo aberto não vê** identidade visual nem multiplicadores novos — o plugin BepInEx cacheia esses dados **1× por sessão** (refetch só ao trocar de perfil/relogar).
2. **Perfis existentes nunca mudam.** O template de classe é aplicado **só na criação do perfil**. Editar skills/itens/outfit de uma classe não altera nenhum perfil já criado — só os próximos.
3. **Salvar reserializa o `.jsonc` → comentários manuais são PERDIDOS** (e o formato é normalizado). Os backups rotativos `.bak1`–`.bak3` ao lado do arquivo preservam as 3 últimas versões — `.bak1` é a mais recente. Se o arquivo tem comentários que importam, edite-o à mão (e reinicie o server) em vez de usar o editor.
4. **Rename não existe.** `name` é a chave da edition (launcher, registries, perfis) e é read-only no editor. Renomear = **Duplicate** com o nome novo + desabilitar/deletar a antiga. Perfis existentes criados com a edition antiga continuam jogáveis, mas **perdem identidade visual e multiplicadores** (a edition deles some dos registries).

## 4. Custo (budget de balanceamento)

- **Custo de skills** (port fiel da fórmula RZ): `custo = Σ (nível × peso)` por skill, com peso = `BASELINE 15 ÷ nível esperado` da skill (31 pesos do RZ + 4 **derivados** para as skills do Skills-Extended — FirstAid, FieldMedicine, BearRawpower, UsecNegotiations — com fallback por categoria p/ skills não mapeadas). **Budget alvo: 28–32.** Fora do budget = warning informativo, não bloqueia o save. Paridade verificável offline: `node scripts/check-skill-costs.mjs` (pesos espelhados — mudou `SkillWeights.cs`, espelhe o script).
- **Loadout ₽**: soma equipped + stash com presets expandidos (mods/contents/ammo); preço = **flea efetivo** (dinâmico) com fallback **handbook**; dinheiro entra pelo valor facial. Item sem preço ganha flag "no price" no breakdown.
- **Multiplicadores de XP ficam FORA do custo** — decisão de design (custo mede o nascimento do perfil, não a progressão).

## 5. Ícones de classe

- Classe criada no editor **nasce sem PNG** → o nome aparece como texto colorido (degradação prevista; nada quebra).
- Adicionar ícone:
  1. Coloque o SVG (padrão game-icons.net, CC BY 3.0 — ver `ATTRIBUTION.md`) em `scripts/icon-sources/`.
  2. Em `scripts/`: `npm install && npm run build:icons` (`build-icons.mjs`) — gera o PNG 256×256 branco em **dois destinos**: `modded/Client/icons/` (tintado em runtime pelo client) e `modded/Server/wwwroot/icons/` (preview do editor).
  3. `/compile-mod CustomClasses` para instalar (client icons + `wwwroot/`).
  4. Selecione o `iconFile` na aba General do editor.
  5. **Restart do client** (jogo) para o ícone aparecer in-game — o cache de ícones é por sessão.

## 6. Smoke test ponta a ponta

Roteiro de verificação do ciclo completo (usado no fechamento do épico):

1. **Suba o server SPT** e abra `https://<ip>:6969/customclasses/classes` — a lista deve mostrar as 12 classes registradas com custos dentro do budget.
2. **Crie uma classe**: "New class" → nome (ex.: `Teste Smoke`) → o diálogo avisa que nasce sem ícone → abre direto na edição.
3. **Entre em edição** (botão Edit) e ajuste no painel esquerdo: adicione 2–3 skills com níveis, um multiplicador, uma estação de hideout.
4. **Confira o custo** na toolbar: total de skills vs. budget 28–32 (warning se fora — informativo).
5. **Adicione equipado**: clique o slot **Primary** da silhueta → modo preset → escolha uma arma (premium opcional) + ammo com loadedMag → Done; veja o "Loadout" recalcular. No painel do stash, **arraste** um item e **⟳** rotacione-o.
6. **Save**: snackbar "saved and hot-applied"; confira no install o `.jsonc` novo (com `x`/`y`/`rotated` só nos itens movidos), o `.bak1` e a linha no `_audit.log`.
7. **Launcher sem restart do server**: abra o launcher → a edition nova aparece na criação de perfil.
8. **Crie um perfil** com a classe e entre no jogo: skills nos níveis configurados, arma equipada montada (com mira mínima), stash conforme.
9. **`/sync-classes`** (ou `scripts/sync-classes.sh`): diff preview mostra o arquivo novo → confirme → repo == install.
10. **Commit** dos `.jsonc` sincronizados. (Limpeza: delete a classe de teste pelo editor + novo `/sync-classes`, ou descarte no git.)

## 7. Atalhos, densidade e preferências (item 035)

- **Densidade:** lista, abas de edição, pickers e diálogos usam densidade compacta — mais linhas/campos por tela.
- **`Ctrl+S` (e `Cmd+S`)** na edição salva (mesma validação do botão Save; bloqueio por Error continua valendo) e impede o "salvar página" nativo do browser. Só atua em modo edição.
- **Edit em 1 clique:** ação Edit por linha na lista e por item na sidebar; clicar numa célula da matriz abre o workspace de edição.
- **Sem abas (redesign F2):** o edit é in-place no workspace de 3 painéis; o antigo deep-link `?tab=N` e a "aba preservada" entre classes deixaram de existir (skills/equipado/stash convivem na mesma tela). O `?tab` ainda é aceito mas ignorado.
- **Preferências persistidas** (no `localStorage` do browser, escopo local single-user): estado da sidebar (aberta/ícones/fechada), ordenação da lista (coluna + direção), toggles da matriz e filtro da sidebar. Na primeira visita (sem chave salva) tudo abre nos defaults de hoje. As preferências são aplicadas **após** a página conectar o circuito interativo — pode haver um leve "flash" do default para o valor salvo no reload (esperado).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-10 | Guilherme | Criação (item 029) — acesso, fluxo install↔repo, os 4 limites, custo, ícones, smoke test. |
| 2026-06-12 | Guilherme | Atualização waves 030–036 (sidebar persistente, matriz de skills, dashboard 033, gear/stash visual 034, comparação A×B 036) + §7 (atalhos Ctrl+S, Edit 1-clique, aba preservada, densidade e preferências em localStorage do item 035). |
| 2026-06-12 | Claude | Item 035 implementado (densidade global, colunas ordenáveis + Edit na lista, Edit na sidebar, aba preservada via `?tab=`, Ctrl+S, matriz→edit na aba Skills, preferências em `localStorage` via `window.ccPrefs`/`UiPrefs`). |
| 2026-06-13 | Claude | Layout EQUIPPED fiel à tela Gear do EFT (slots por tipo, itens maiores), colunas skills/equipped iguais + stash justo; **removido o modo "view" — workspace SEMPRE em edição** com dirty-tracking (Save/Discard só habilitam com mudança; guard só pergunta quando sujo); fix do drag-and-drop (img nativa capturava o drag). |
| 2026-06-13 | Claude | Redesign F1/F2 (itens 038+): workspace unificado de 3 painéis (skills/mults/hideout/outfit · silhueta com 14 slots · grade 2D do stash), edição IN-PLACE (sem abas), seletor visual de skins, clicar-slot-edita-equipado, e drag-and-drop do stash (mover + ⟳ rotacionar, coords `x`/`y`/`rotated` opt-in honradas in-game). Sidebar 3-estados (aberta/ícones/fechada). `?tab` deep-link aposentado (ignorado). |

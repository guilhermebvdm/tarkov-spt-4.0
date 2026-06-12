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
   - O `<ip>` é o `ip` de `SPT_Data/configs/http.json` (default `127.0.0.1`), **mas mods podem sobrescrever o bind**. No install atual o **fika-server** força o IP do Radmin (`server.ip` em `user/mods/fika-server/assets/configs/fika.jsonc`, ex.: `26.207.194.149`) — o editor responde **só nesse IP** (`https://26.207.194.149:6969/customclasses`); `127.0.0.1` não responde.
3. O certificado é **self-signed** → o browser bloqueia na primeira visita. Aceite a exceção ("Avançado → Continuar"); no Chrome sem botão, digite `thisisunsafe` na página de aviso.

### Rotas

| Rota | Página |
|---|---|
| `/customclasses` | Home (card "Class editor" + smoke test de editions) |
| `/customclasses/classes` | **Lista de classes** — ícone, nome colorido, status (Registered/Disabled/Invalid/Not registered + diagnostics em tooltip), nº de skills, custo de skills vs. budget, loadout ₽, arquivo. Toolbar "New class" + ações Duplicate/Delete por linha. |
| `/customclasses/classes/{arquivo}` | **Detalhe read-only** — diagnostics, painéis General/Skills/XP multipliers/Hideout/Outfit/Equipped/Stash + breakdown completo de custo. Botões Edit/Duplicate/Delete. |
| `/customclasses/classes/{arquivo}/edit` | **Edição** — abas **General / Skills / Multipliers / Hideout / Outfit / Equipped / Stash**, toolbar sticky com Save/Discard + custo ao vivo. |
| `/customclasses/picker-test` | Harness de dev dos pickers (item 023) — sem link no menu, só URL direta. |

`{arquivo}` = nome do `.jsonc` sem extensão (ex.: `cacador`).

### O que dá pra editar

- **General:** displayName/description (en/pt), cor do nome, `enabled`, `baseEdition` (só editions vanilla), `iconFile` (enumerado do install, com preview). `name` é **read-only** — é a chave da edition (ver limite 4).
- **Skills:** níveis 0–51 com peso/custo por linha e total vs. budget ao vivo.
- **Multipliers:** fatores de XP por skill (≥ 0; verde = buff, vermelho = debuff; badge p/ skills do Skills-Extended).
- **Hideout:** nível inicial por estação.
- **Outfit:** upper/lower por facção via picker de customization.
- **Equipped:** 1 `ItemSpec` por slot do personagem — modos item/preset, premium, ammo (loadedMag/chambered), árvore de mods recursiva filtrada pelos slots do template, contents de contêiner.
- **Stash:** lista plana de `ItemSpec` (item 028).
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
3. **Edite campos simples**: displayName en/pt, descrição, cor do nome; na aba Skills adicione 2–3 skills com níveis.
4. **Confira o custo** na toolbar: total de skills vs. budget 28–32 (warning se fora — informativo).
5. **Adicione equipado**: aba Equipped → "Add slot" → FirstPrimaryWeapon com um preset de arma (premium opcional) + ammo com loadedMag; veja o "Loadout total" recalcular.
6. **Save**: snackbar "saved and hot-applied" + banner com os limites; confira no install o `.jsonc` novo, o `.bak1` e a linha no `_audit.log`.
7. **Launcher sem restart do server**: abra o launcher → a edition nova aparece na criação de perfil.
8. **Crie um perfil** com a classe e entre no jogo: skills nos níveis configurados, arma equipada montada (com mira mínima), stash conforme.
9. **`/sync-classes`** (ou `scripts/sync-classes.sh`): diff preview mostra o arquivo novo → confirme → repo == install.
10. **Commit** dos `.jsonc` sincronizados. (Limpeza: delete a classe de teste pelo editor + novo `/sync-classes`, ou descarte no git.)

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-10 | Guilherme | Criação (item 029) — acesso, fluxo install↔repo, os 4 limites, custo, ícones, smoke test. |

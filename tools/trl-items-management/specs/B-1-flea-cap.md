# B-1 · Remover / expor o teto do flea

> **Status:** 🟢 Spec (SDD) · **Data:** 2026-07-04 · **Depende de:** — (independe do B-2) · **Ref backlog:** [BACKLOG.md](../BACKLOG.md) B-1

## 1. Funcional

**Objetivo:** permitir preços de flea acima do teto que hoje o SPT impõe a **Weapon Mod** (handbook×6) e **Electronics** (handbook×11), sem plugin — é config.

**Contexto (confirmado):** o teto vem de `SPT_Data/configs/ragfair.json` → `dynamic.unreasonableModPrices` (2 baseclasses: `5448fe124...`=Weapon Mod mult 6, `57864a66...`=Electronics mult 11). O `load-spt` deriva `item.spt.fleaCeiling = round(handbook × newPriceHandbookMultiplier)` (`scripts/load-spt.js:295-298,364`); o `serve.js` **rejeita** `price > ceiling` no PATCH (`serve.js:566-577,614-627`) e o `index.html` põe `max=ceiling` (`index.html:1839`). Ou seja: teto do jogo e trava do viewer vêm da MESMA config.

**Critérios de aceite:**
1. Existe um controle no viewer (topbar, junto do "Flea Lvl") para **desligar o teto** de mods/electronics (global) — escreve `enabled:false` nas 2 entradas de `unreasonableModPrices` (ou remove o cap).
2. Após aplicar + regerar o catálogo, o viewer **aceita e grava** preço de flea acima do antigo teto para item dessas categorias (sem o erro "above the ceiling").
3. Estado atual do teto é **visível** (ligado/desligado) no viewer.
4. Reversível: religar restaura `enabled:true` + mults originais (6/11).
5. **Não** altera nenhuma outra chave de `ragfair.json`.

**Corner cases:**
- `unreasonableModPrices` ausente/vazio no ragfair.json → tratar como "sem teto" (nada a fazer).
- Multiplicadores customizados (≠6/11) já presentes → preservar o valor original para o "religar" (guardar o snapshot, não hard-code 6/11).
- Coop/Fika: config de servidor → aplica a todos; sinalizar que é global (não per-player). [ver [[feedback_coop_multiplayer_sync]]]

## 2. Técnico

**Arquivos:**
- `viewer/serve.js`: novo endpoint `POST /api/flea-cap` `{enabled:boolean}` → lê `ragfair.json`, para cada baseclass em `unreasonableModPrices` seta `enabled` (guardando o original num sidecar `ragfair.trl-cap-backup.json` na 1ª vez p/ religar exato) → escrita atômica + refresh `checks.dat`. `GET /api/flea-cap` → estado atual (`{enabled, categories:[...]}`).
- `viewer/index.html` + `components.css`: toggle na topbar (zona de config), lê `GET /api/flea-cap`, chama o POST, mostra estado.
- `scripts/load-spt.js`: **sem mudança** — `fleaCeilingFor` já lê o `enabled`? **VALIDAR:** hoje `UNREASONABLE` (`:295`) filtra por `enabled`? Se não, ajustar `fleaCeilingFor` para retornar `null` quando `enabled===false` (senão o `fleaCeiling` continua setado mesmo com a config desligada). **Provável ajuste de 1 linha.**

**Fluxo de aplicação:** mudar `ragfair.json` → o `fleaCeiling` no `items.json` só atualiza no **rebuild** (`load-spt`). Opções: (a) o endpoint dispara um rebuild leve (só load-spt+normalize) — pesado; (b) o `serve.js` passa a **derivar `fleaCeiling` ao vivo** do `ragfair.json` no GET de item (mais responsivo, desacopla do build). **Decisão assumida:** (a) para o MVP — o toggle avisa "regere o catálogo (ou reinicie o viewer) para o novo teto valer na UI"; o teto do JOGO já muda no próximo boot do SPT independent do viewer. Registrar (b) como refino.

**Validação in-game (PENDENTE — cliente EFT):** desligar o teto + setar GPU a 3M + reiniciar SPT → confirmar in-game que a oferta passa de 2.178M (checar que não há 2º limite no cliente). **Não executável enquanto o user dorme.**

## 3. Acceptance/verificação automatizável
- `node --check` no serve.js/load-spt.
- Endpoint `POST /api/flea-cap {enabled:false}` → ler `ragfair.json` e confirmar `enabled:false` nas 2 baseclasses + sidecar de backup criado; `{enabled:true}` restaura 6/11.
- Após rebuild com cap off → `items.json` do GPU tem `fleaCeiling:null`; PATCH de flea acima do antigo teto retorna 200.

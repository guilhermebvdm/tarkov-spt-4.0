# B-4 · Bulk: copiar preço tarkov.dev / tarkov-market → override de FLEA

> **Status:** 🟢 Spec (SDD) — **implementação adiada** (depende do B-2 M1) · **Data:** 2026-07-04 · **Ref backlog:** [BACKLOG.md](../BACKLOG.md) B-4

## 1. Funcional

**Objetivo:** editar o preço de **flea** de **vários itens de uma vez**, copiando o preço de referência do **tarkov.dev** ou do **tarkov-market** como override.

**Critérios de aceite:**
1. Modo de **seleção múltipla** na lista (checkbox por linha + "selecionar todos os filtrados"); contador de selecionados.
2. Ação em massa: **"Copiar preço [tarkov.dev | tarkov-market] → flea"** aplicada aos selecionados.
3. Cada item recebe o override de flea equivalente (mesma compensação `override = X − bonus` do fluxo single, `serve.js` linha ~535).
4. **Relatório do batch:** aplicados N, pulados M (sem preço da fonte), ajustados K (acima do teto → gravado no teto). Sem travar a UI (progress).
5. Reversível: "limpar overrides dos selecionados" (batch DELETE) e o DELETE-all de flea já existente.
6. Restart-para-aplicar continua valendo (aviso).

**Corner cases:**
- Item **sem preço** na fonte escolhida → pular + contar (não gravar 0).
- Item **acima do teto** (mods/electronics, se B-1 não desligou) → gravar `min(preço, ceiling)` e marcar como ajustado.
- Item **flea-banned** → pular + avisar (não faz sentido setar preço de item banido).
- Seleção grande (centenas) → batch server-side, uma escrita atômica por lote (não 1 request por item).

## 2. Técnico

**Depende de B-2 M1:** a UI de seleção nasce na UI servida pelo mod novo (senão seria refeita). Por isso: **spec agora, implementar após B-2 M1**.

**Arquivos (quando implementar):**
- `serve.js` (ou o handler C# equivalente pós-B-2): `POST /api/price/bulk` `{tpls:[...], source:"dev"|"market"}` → resolve o preço-fonte de cada tpl (do `items.json` já carregado), calcula `override = X − bonus` com clamp de teto, grava todos numa transação (uma leitura+escrita de `ragfair.json`), retorna `{applied, skipped:[{tpl,reason}], adjusted:[{tpl,from,to}]}`. E `DELETE /api/price/bulk {tpls:[...]}`.
- `index.html`: modo seleção (checkboxes), barra de ação em massa, modal de confirmação com preview da contagem, render do relatório.

**Reuso:** a lógica de compensação/clamp já existe no `handlePatchPrice` — extrair para uma função pura reutilizável pelo single e pelo bulk (evita divergência).

## 3. Acceptance/verificação automatizável
- `POST /api/price/bulk` com 3 tpls (1 com preço dev, 1 sem, 1 acima do teto) → resposta `applied:1, skipped:1, adjusted:1` e `ragfair.json` reflete os overrides corretos.
- Idempotência: rodar 2x não duplica/agrava.

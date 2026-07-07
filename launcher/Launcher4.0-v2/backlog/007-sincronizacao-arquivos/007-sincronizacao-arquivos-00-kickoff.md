# 007 — Sincronização de arquivos · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f itens 4.1, 4.1.1 (×4), 4.1.2, 4.1.3

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. Item grande — o motor de sync daqui é reusado pelo 008.

## Objetivo

Sincronização de arquivos server→cliente com **regra específica por pasta**, cancelamento seguro e manifesto de mudanças.

## Regras por pasta (do card)

| Pasta | Regra |
|---|---|
| `config` | Substituir os arquivos que forem **iguais** (não customizados); **manter** os que o usuário alterou |
| `config-server` | Espelho completo, **excluindo** do PC do usuário os arquivos que não existirem mais no server |
| `patchers` | Espelho completo, **movendo** removidos do server para `patchers-disabled` do usuário |
| `plugins` | Espelho completo, **movendo** removidos do server para `plugins-disabled` do usuário |

## Requisitos adicionais

- **4.1.2:** poder cancelar a "Verificação de arquivo" — com confirmação e alerta das consequências.
- **4.1.3:** gravar em `/user/launcher` um arquivo com a lista de arquivos modificados na última atualização; clicar em "X arquivos foram atualizados" abre a pasta.

## Atenção (memória do repo)

- O sync atual do launcher **reverte builds locais de mod client** quando o Dev Mod está off (lição registrada na memória do repo) — a spec precisa definir a interação sync × Dev Mod (ex.: exceção por hash local? aviso?).
- A regra de `config` ("substituir iguais, manter divergentes") implica comparação com o **estado esperado anterior** (baseline/hash), não só server×cliente — detalhar o mecanismo na spec técnica.

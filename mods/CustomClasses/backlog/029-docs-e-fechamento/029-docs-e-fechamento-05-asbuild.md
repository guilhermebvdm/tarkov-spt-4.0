# 029 — Docs e fechamento do editor · As-Built

**Mod:** CustomClasses · **Build:** 2026-06-10 · **Kickoff:** [00-kickoff](029-docs-e-fechamento-00-kickoff.md)

> Item **operacional de documentação** (zero código) — executado direto do kickoff, **sem spec** (dispensada por decisão de orquestração; os as-builts dos itens 018–028 são a fonte da verdade do que foi documentado).

## Arquivos

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `docs/class-editor.md` | Guia do editor web: acesso (URL/bind/cert self-signed) + tabela de rotas, o que cada aba edita, save = hot-apply (`.bak1-3` + `_audit.log`), fluxo install↔repo (`/sync-classes`, guard `--force-config` do compile-mod, gerador congelado), **os 4 limites** (seção destacada), custo (fórmula RZ Σ nível×peso, BASELINE 15, budget 28–32; loadout ₽ flea→handbook; XP-mults fora), pipeline de ícones (`build-icons.mjs`, 2 destinos), smoke test ponta a ponta em 10 passos. Padrão de cabeçalho/histórico do repo. |
| MODIFICADO | `README.md` | Seção nova "Editor web de classes (in-game server)" apontando pro doc; link pro `class-schema.md` no mecanismo central; tabela de estrutura atualizada (`docs/`, `Web/`+`wwwroot/`, scripts de autoria com freeze, sem `Common/`/`.sln` que não existem); seção Build atualizada (compile-mod pronto + Sdk.Web/4.0.2 + guard); roadmap aponta pro backlog (era "001-008/plano"). |
| MODIFICADO | `backlog/mod-backlog.md` | Linha 029 ⚪→🟢. |

## Decisões / achados na consolidação dos as-builts

- **Bind IP:** os as-builts 020/025 dizem que o install "binda no IP Radmin configurado em `http.json`" — na verdade `SPT_Data/configs/http.json` está em `127.0.0.1`; quem força `26.207.194.149` é o **fika-server** (`server.ip`/`backendIp` em `user/mods/fika-server/assets/configs/fika.jsonc`). O doc descreve o mecanismo correto (default `http.json`, override por mod).
- **Contagem de classes:** 020 logou 12 classes (Peladão incluso); 025/026 logaram 11 — variação de estado do install entre sessões, não divergência de código. O doc usa "12" como estado atual.
- **`picker-test`:** página harness do 023 mantida (decisão deste item: documentada como rota de dev, sem link no menu; remoção fica como housekeeping futuro se incomodar).
- **Smoke test:** registrado como roteiro numerado no doc (§6) — execução manual em jogo segue a memória `feedback_spt_validation` (validação real é in-game).

## Pendências (fora do escopo de docs)

- Erro de console pré-existente `MudPointerEventsNone ... already declared` (script MudBlazor 2×, infra 020) — candidato a housekeeping.
- Item 028 (aba Stash) rodou **em paralelo** a este: o doc já descreve a aba Stash conforme a spec do 028; conferir após o merge se algo mudou.
- `memory/sessions.md` — atualização fica com o `/update-memory` do orquestrador no fechamento da sessão.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-10 | As-built. Docs criadas (class-editor.md + README) e backlog atualizado. |

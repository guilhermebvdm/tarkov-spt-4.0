# SPT Realism Mod — Análise do canal de desenvolvimento (Discord)

Diretório de trabalho para a análise do canal **"Realism Mod Development"** (mod hardcore de overhaul para SPT, autor **Fontaine**) no Discord da comunidade SPT (SPT Pub).

**Fonte:** <https://discord.com/channels/875684761291599922/1123680324254171186> · recorte **07/03 → 04/06/2026** (~90 dias) · **1.132 mensagens** · **102 participantes** · capturado em 2026-06-05.

## Conteúdo

| Arquivo | Descrição |
|---|---|
| [01-transcricao.md](./01-transcricao.md) | **Transcrição fiel** das 1.132 mensagens (idioma original, horários em GMT-3 via snowflake, anexos linkados). |
| [02-analise.md](./02-analise.md) | **Análise em PT** — modularização do Realism 4.0, rework de stances, sistema médico/balística/hazards/bots, bugs e respostas do dev, compatibilidade, roadmap, curiosidades e cheat-sheet. |
| [assets/](./assets/) | 18 imagens/gifs + 6 vídeos baixados do canal, mais os JSONs brutos da captura (`_capture.json`, `_manifest.json`) e o gerador (`_gen-transcript.js`). |

## Como foi capturado

Navegação autenticada no Discord (extração do DOM via Chrome DevTools). Por ser um **canal aberto de ~2 anos**, fez-se **seek-up até o cutoff de 90 dias** + **down-sweep contíguo** até o fundo, com deduplicação por ID de mensagem e herança de autor para mensagens agrupadas. Timestamps derivados do **snowflake** (não do `time` do DOM). Imagens analisadas visualmente (`Read`); os 6 vídeos são demos de stance rework e trailers da comunidade.

## ⚠️ Peso dos assets

Os 6 vídeos `.mp4` somam **~295 MB** (att-04, 13, 18, 19, 23, 24); as imagens/gifs são ~4 MB. **Considere não versionar os `.mp4`** (`*.mp4` em `.gitignore`) ou removê-los após análise — eles não são analisáveis frame-a-frame e podem ser rebaixados do Discord novamente se necessário (URLs assinadas expiram).

## Resumo de 1 linha

Canal de dev do **SPT Realism Mod**: o port para SPT 4.0 será **modular** (mods standalone, um de cada vez, começando por **stances + CommonLib**), com **rework completo das stances** (animation curves, integração com a animação procedural da BSG) como entrega principal do período — em meio a muito **"when update?"** e moderação ativa.

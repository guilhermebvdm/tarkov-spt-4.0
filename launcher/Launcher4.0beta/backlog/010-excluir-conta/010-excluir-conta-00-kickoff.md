# 010 — Botão "Excluir conta" · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 4.4

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Botão "Excluir conta" na tela logada (ProfileView) — remoção definitiva do perfil no server, com confirmação forte.

## Estado atual

- Hoje só existe **wipe** ([Models/Launcher/WipeProfileModel.cs](../../project/SPT.Launcher.Base/Models/Launcher/WipeProfileModel.cs)) — wipe reseta o progresso, **não** exclui a conta.
- Verificar na spec técnica se o server SPT expõe rota de remoção de perfil (`/launcher/profile/remove` ou similar em `references/spt-source/`) e o que acontece com os arquivos de profile no disco do server.

## Perguntas p/ a spec

- Confirmação: digitar o nome da conta? duplo diálogo?
- Comportamento coop (Fika): perfil em uso por sessão ativa.

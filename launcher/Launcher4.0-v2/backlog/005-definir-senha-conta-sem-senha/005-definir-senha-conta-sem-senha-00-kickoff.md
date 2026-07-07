# 005 — Definir senha em conta sem senha · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f item 1.1

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Validar (e corrigir, se necessário) o comportamento de **definir senha** quando o usuário entra numa conta que ainda não tem senha.

## Estado atual

- Já existe [Views/Dialogs/CreatePasswordDialogView.axaml](../../project/SPT.Launcher/Views/Dialogs/CreatePasswordDialogView.axaml) + [CreatePasswordDialogViewModel.cs](../../project/SPT.Launcher/ViewModels/Dialogs/CreatePasswordDialogViewModel.cs) — mapear quando o diálogo dispara no fluxo novo de login (001) e se a senha persiste corretamente no server.

## Perguntas p/ a spec

- Em que momento detectar "conta sem senha" (login com senha vazia aceito pelo server?)?
- O diálogo é obrigatório ou adiável? O que acontece ao cancelar?
- Comportamento em conta criada pré-redesign (dados legados).

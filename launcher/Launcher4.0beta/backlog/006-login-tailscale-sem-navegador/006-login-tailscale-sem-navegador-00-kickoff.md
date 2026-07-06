# 006 — Login Tailscale sem navegador · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Origem:** Trello MTav8H5f itens 0 ("Ao abrir o Launcher") e 0.1

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo

Ao abrir o launcher, autenticar no Tailscale **sem abrir o navegador** para o usuário.

## Estado atual

- [Helpers/TailscaleHelper.cs](../../project/SPT.Launcher/Helpers/TailscaleHelper.cs) e [Helpers/WireGuardHelper.cs](../../project/SPT.Launcher/Helpers/WireGuardHelper.cs) já existem — mapear o fluxo atual (quando/por que o navegador abre).

## Direções a investigar na spec

- `tailscale up --auth-key=<key>` (auth key pré-provisionada, sem browser) — implicação: distribuir/rotacionar a key.
- Reaproveitar sessão existente (`tailscale status` antes de `up`; só pedir login quando expirado).
- UX de erro quando o Tailscale não está instalado/logado (mensagem no launcher, não navegador).

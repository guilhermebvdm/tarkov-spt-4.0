---
title: Fika PoolManager NullReferenceException Fix - Technical Spec
date: 2026-07-22
status: 🟢 Vivo
authors: [Antigravity, Guilherme]
---

# 001 — Fika PoolManager NullReferenceException Fix — Spec Técnica

## 1. Escopo Técnico
Implementar um patch Harmony no mod `TRL-Fixes` para interceptar a criação de objetos no pool de recarga de armas de outros jogadores remotos.

## 2. Ponto de Injeção
* **Classe alvo**: `PoolManagerClass`
* **Método alvo**: `CreateItem(Item item, ECameraType cameraType, IPlayer player, bool isAnimated)`
* **Tipo de Patch**: `Prefix`

## 3. Lógica do Patch
* Verificar se o parâmetro `player` não é nulo e se `player.IsYourPlayer` é `false`.
* Caso seja jogador remoto, redirecionar a chamada usando `Singleton<PoolManagerClass>.Instance.CreateItem(item, isAnimated)` e retornar `false` para impedir a execução do original com erro de NRE.

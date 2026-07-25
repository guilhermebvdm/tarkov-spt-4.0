---
title: Fika PoolManager NullReferenceException Fix
date: 2026-07-22
status: 🟢 Vivo
authors: [Antigravity, Guilherme]
---

# 001 — Fika PoolManager NullReferenceException Fix

## Descrição do Problema
Durante partidas cooperativas (Fika), quando jogadores remotos realizam ações de recarga de armas com cartuchos individuais (ex: escopetas, rifles de ferrolho), o evento de animação do Unity `OnAddAmmoInChamber` tenta instanciar o projétil físico nas mãos do jogador remoto usando `PoolManagerClass.CreateItem(...)`. 
No entanto, por se tratar de um jogador remoto, a referência de câmera ou propriedades locais são nulas no cliente atual, resultando em uma `NullReferenceException` que derruba a conexão do jogador local.

## Critérios de Aceite
* O mod `TRL-Fixes` deve interceptar o método de carregamento de itens de recarga `PoolManagerClass.CreateItem` de 4 parâmetros.
* Se a requisição de criação de item vier de um jogador remoto, o mod deve redirecionar a chamada para a versão simplificada de 2 parâmetros de `CreateItem` que não depende de referências de câmera.
* A correção não deve interferir no comportamento correto e detalhado para o jogador local.

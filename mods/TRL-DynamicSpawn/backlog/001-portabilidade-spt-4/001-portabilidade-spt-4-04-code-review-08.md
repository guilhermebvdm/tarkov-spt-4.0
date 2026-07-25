---
title: "Code Review 08: Refatoração para API de Waves Nativas (SPT 4.0)"
date: 2026-07-18
status: 🟢 Vivo
authors: [Agent]
---

# 📝 Code Review: Migração para `BotWaveDataClass`

## 1. O Problema (`NullReferenceException` em `BotsPresets.CreateProfile`)
A implementação anterior do `TRL-DynamicSpawn` injetava perfis "crus" na memória do BepInEx usando a instrução `BotCreationDataClass.Create(profileData...)`. 
Em versões antigas do SPT, o cliente aceitava perfis soltos sem pré-aviso. No SPT 4.0, no entanto, o servidor Node.js gerencia um *pool* restrito de PMCs e Scavs e exige que as ondas sejam "anunciadas" para formatar o cache local corretamente (`BotsPresets`).
Como nós chamávamos o spawn por conta própria em uma rotina temporizada avulsa e passávamos um `BotSpawnParams` nulo, o servidor Node retornava um perfil corrompido ou vazio para o BepInEx. O BepInEx empurrava esse `null` para a Engine e a exceção NRE explodia em `data._profileData.TryGetRole(...)`.

## 2. A Solução (Substituição por `BotWaveDataClass`)
Ao invés de criarmos perfis "Crus", transferimos a lógica de fabricação de perfis para a API nativa de ondas do Tarkov.

- Removemos as abstrações de `BotProfileDataClass` e o `DummyToken` (GInterface22) obsoleto.
- A função `GenerateBotsAsync` foi convertida em síncrona (removendo `yield returns`) e agora formata um struct `BotWaveDataClass`.
- A queue de "Smooth Injection" agora despacha usando o método oficial de sessão: `_botsController.ActivateBotsByWave(wave)`.

### Por que isso é robusto?
O `ActivateBotsByWave` coloca a responsabilidade nas mãos da Engine e do plugin de SPT-AKI. Quando essa função roda, o BepInEx:
1. Pega os parâmetros da onda (Facção, Dificuldade, Tamanho).
2. Pede os dados corretos ao Servidor Node usando a "cartinha" validada de Sessão.
3. Alimenta o Cache do BepInEx com sucesso.
4. Despacha no campo `SpawnAreaName` especificado.

## 3. Compatibilidade (Custom Spawns e Culling LoS)
Uma preocupação validada foi a perda de recursos customizados (como `EnableCustomPmcSpawns` e `enableLoSCulling`).
Como investigado, a classe `Patches.cs` atua no Menu de Load (antes da Raid) inserindo nossos *Custom Spawn Points* diretamentes nas BotZones do mapa original e filtrando pela distância de Culling antes mesmo da Raid ligar.
Como nós repassamos `wave.SpawnAreaName = selectedZone.NameZone`, a Wave Oficial usa as mesmas BotZones adulteradas pelo mod, resultando em:
- **0 Perdas:** Os bots continuarão nascendo em nossas coordenadas e preservando as distâncias seguras de Line-of-Sight, mas agora processados em pacotes à prova de quebras.

## 4. Resultado da Compilação
- Múltiplas funções em formato `IEnumerator` foram rebaixadas para métodos locais assíncronos e o script compila com sucesso.
- O build retornou com **0 Errors**.

A arquitetura Client agora espelha as diretrizes absolutas de injeção de mods avançados como o *SWAG/Donuts*, trazendo a estabilidade necessária sem sacrificar as lógicas independentes do nosso Mod.

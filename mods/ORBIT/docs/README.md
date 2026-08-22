# Documentação Técnica e Funcional — ORBIT

Este diretório contém a documentação técnica e detalhada de todos os subsistemas e funcionalidades implementados no mod **ORBIT** (*Objective-driven Raid Bot Intelligence Tactics* v1.2.1).

---

## Sumário de Documentos

| Documento | Descrição | Status |
|---|---|---|
| [01. Visão Geral e Arquitetura](01-visao-geral-e-arquitetura.md) | Arquitetura central, ciclo de vida de raid, modelo de entidades (`Agent`, `Squad`), camada BigBrain e integração com SAIN. | 🟢 Vivo |
| [02. Sistema de Objetivos e Metas](02-sistema-de-objetivos-e-metas.md) | Geração procedural de metas no spawn (`LootValue`, `Kills`, `Quest`), filtros por andares e progressão do esquadrão. | 🟢 Vivo |
| [03. Personalidades SAIN e Arquétipos](03-personalidades-sain-e-arquetipos.md) | Mapeamento dos 5 arquétipos (Timmy, Cauteloso, Médio, Agressivo, Muito Agressivo) e seus 13 parâmetros comportamentais. | 🟢 Vivo |
| [04. Movimentação, Advecção e Esquadrões](04-movimentacao-adveccao-e-esquadroes.md) | Campo de força vetorial por mapa, atratores/repulsores, dinâmica líder-seguidor, convergência com jogadores e *Squad Rally*. | 🟢 Vivo |
| [05. Sistema de Coleta (Looting) e Troca de Equipamentos](05-sistema-de-looting-e-troca-de-equipamentos.md) | Coleta orgânica de caixas, loose loot e corpos; avaliação e troca automática de armas, armaduras, coletes táticos, mochilas e fones. | 🟢 Vivo |
| [06. Sistema de Extração Tático e Emergência](06-sistema-de-extracao-tatico-e-emergencia.md) | Condições para rota de fuga (metas concluídas, valor de loot em ₽, tempo de raid, ferimentos críticos solo). | 🟢 Vivo |
| [07. Sistemas Auxiliares, Portas e Performance](07-sistemas-auxiliares-portas-e-performance.md) | Arrombamento/destrancamento de portas, sistema de olhar (*LookSystem*), otimização de tickrate de decisões e suporte a facções. | 🟢 Vivo |
| [Relatório de Auditoria Técnica de Código (Review 01)](relatorio-auditoria-codigo-01.md) | Relatório estático detalhado de qualidade de código, checagens cruzadas com EFT/SPT/FIKA, GC pressure e propostas de otimização de `Update()`. | 🟢 Vivo |

---

## Relação de Arquivos de Código-Fonte do Mod

- **Ponto de Entrada e Configurações:**
  - [Plugin.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Plugin.cs)
  - [LootConfig.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/LootConfig.cs)
- **Núcleo e Camada Cerebral:**
  - [OrbitManager.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Core/OrbitManager.cs)
  - [OrbitBrainLayer.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Brain/OrbitBrainLayer.cs)
- **Sistemas:**
  - [MovementSystem.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Systems/MovementSystem.cs)
  - [WaypointSystem.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Systems/WaypointSystem.cs)
  - [DoorSystem.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Systems/DoorSystem.cs)
  - [LookSystem.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Systems/LookSystem.cs)
- **Looting & Troca de Equipamentos:**
  - [OrbitLootHandler.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/OrbitLootHandler.cs)
  - [WeaponSwapper.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/WeaponSwap/WeaponSwapper.cs)
  - [ArmorSwapper.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/WeaponSwap/ArmorSwapper.cs)
  - [RigSwapper.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/WeaponSwap/RigSwapper.cs)
  - [BackpackSwapper.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/WeaponSwap/BackpackSwapper.cs)
  - [HeadsetSwapper.cs](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/ORBIT/original/Orbit/Looting/WeaponSwap/HeadsetSwapper.cs)

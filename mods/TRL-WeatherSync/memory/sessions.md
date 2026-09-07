# Memória de Sessões — TRL-WeatherSync

## Estado atual

> **Delta 2026-09-02 (Sessão 1):** Fundação da estrutura canônica do novo mod [`TRL-WeatherSync`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-WeatherSync/). Concluída a fase de concepção, investigação dos mecanismos de clima/estações do EFT/FIKA e consolidação profunda do [`ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-WeatherSync/ROADMAP.md) contendo: (1) Mapeamento exaustivo das 6 sub-fases nativas das estações do EFT (`SpringEarly` com `SpringSnowFactor`, `Spring`, `Summer`, `Storm`, `Autumn`, `AutumnLate`, `Winter` e `WinterStorm`); (2) Controle cronológico via `server/config.json` calibrado para 1 semana real (7 dias UTC) por fase com exatidão matemática; (3) Módulo de Otimização Extrema de Desempenho para Neve e Chuva (corte de partículas de 65k para 4k/8k, bypass do CommandBuffer HDR `SNOW_GLITTERS` e culling de neve em miras óticas `OpticCameraManager.Camera`); (4) Arquitetura de sincronização contínua em raid (`TrlWeatherSyncPacket`); (5) Integração de sensores de IA do SAIN com tempestades.

- **Status de Desenvolvimento:** 🔵 Planejamento e Roadmap arquitetural concluídos.
- **Versão:** v1.0.0 (Base inicial).

---

## Pendências

- [P-1.1] (aberta 2026-09-02) **Definição da stack de compilação (Client BepInEx C# vs Server)** — Estruturar a solução `.csproj` para BepInEx 5 (.NET Standard 2.1) em `modded/` e vincular referências locais do SPT. 🟢 Arquitetura.
- [P-1.2] (aberta 2026-09-02) **Implementação do Pacote de Rede `TrlWeatherSyncPacket`** — Criação dos serializers LiteNetLib compactados para transmissão de chuva, neblina, nuvens, vento e temperatura. 🟢 Networking.
- [P-1.3] (aberta 2026-09-02) **Implementação do Gestor de Estações Naturais e Sub-Fases** — Ciclo cronológico astronômico automático com modos Calendário Real (1 semana por fase via UTC), Rotação por Raids e Estação Fixa. 🟢 Feature.
- [P-1.4] (aberta 2026-09-02) **Implementação dos Patches de Otimização de Neve e Chuva** — Harmony patches em `SnowFlakes.cs` (redução de malhas de partículas) e `SnowWetRenderer.cs` (culling em miras óticas e bypass de CommandBuffer HDR). 🟢 Performance.

---

## 2026-09-02 14:10 (GMT-3) — Sessão 1: Concepção, Investigação de Sub-Fases de Estações, Otimização de Neve/Chuva e Roadmap

**Tema central:** Diagnóstico aprofundado dos gargalos de sincronização de clima do EFT/FIKA, investigação das sub-fases sazonais nativas do Tarkov, arquitetura de controle de 1 semana real por estação via servidor e soluções de otimização de FPS na neve/chuva.

**Decisões-chave:**
- [Sub-Fases Sazonais Nativas]: Mapeamento de todas as variantes internas do EFT no `ESeason`, `ESeasonStatus` e `Class444`:
  - Primavera: `SpringEarly` (degelo de neve residual) e `Spring` (plena e florida).
  - Verão: `Summer` (ensolarado) e `Storm` (tempestades tropicais).
  - Outono: `Autumn` (folhas douradas) e `AutumnLate` (árvores secas sem folhas, solo gélido).
  - Inverno: `Winter` (neve estável) e `WinterStorm` (nevasca violenta com vento horizontal).
- [Controle de Tempo via Servidor]: Definição de controle determinístico por timestamp UTC no `server/config.json`, com adoção oficial da **Opção A** (1 semana real de 7 dias por estação macro, totalizando 4 semanas / 28 dias reais para o ciclo anual completo, com 3,5 dias em sub-fases de transição na Primavera e Outono). Imune ao `TimeFactor` (~7x) da partida.
- [Otimização de Neve e Chuva]:
  - Constatado que a chuva/neve segue a câmera do jogador a 10m (`RainFollow.cs`).
  - Identificado o gargalo das 65.532 partículas do `SnowFlakes.cs` e a dupla renderização pesada nas miras telescópicas (`OpticCameraManager.Camera`).
  - Estabelecido o módulo de otimização com presets de partículas (4k/8k), culling de neve em miras e desativação do passe HDR 10-bit de glitters.

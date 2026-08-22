# Memória de Desenvolvimento — TRL-ActionPOV

## Snapshot do Projeto (2026-08-18)
- **Status:** Reconstrução limpa do zero (Kinetic Spring Engine).
- **Proposta:** Recriar a mecânica de física de mira e visão do jogo *Bodycam*, utilizando mola cinética (*Spring-Damper*), pivô esférico no ombro direito e roll orgânico da visão sem sobreposição de controladores obsoletos.

## Fontes e Repositórios de Consulta/Referência
Para fins de engenharia reversa, busca de nomes de métodos, variáveis de ofuscação, assinaturas de patches ou lógica de suporte, consultar:
1. `mods/TarkovIRL-SPT4.0/` — Repositório original herdado.
2. `mods/TarkovIRL-SPT4.0-beta/` — Repositório da primeira iteração de testes.
3. `references/eft-decompiled/` — Código descompilado da Assembly-CSharp do EFT.
4. `references/fika-plugin/` — Referências do FIKA (modo cooperativo).

## Decisões Arquiteturais Chave
- Apenas 3 hooks Harmony centrais (`Player.Rotate`, `PWA.SetHeadRotation`, `PWA.CalculateCameraPosition`).
- Separação clara entre `Core` (física pura e bindings) e `Patches` (injeção Harmony).
- Resolução e cache fraco $O(1)$ de referências de `Player` via `ConditionalWeakTable`.
- Blindagem contra acúmulo de `localPosition`/`localRotation` no `WeaponRoot` através de restauração de transform base.

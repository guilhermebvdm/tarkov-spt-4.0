# Visceral Combat 3.7.0 (SPT 4.0 / FIKA 2.2.6)

Mod de física avançada, desmembramento e ragdolls ativos para **Single Player Tarkov (SPT 4.0)** e **FIKA (2.2.6)**.

## Estrutura do Repositório

- `original/` — Código C# descompilado via `ilspycmd` das DLLs originais (`VisceralCombat.dll`, `VolumetricBloodFX.dll`, `bundleloader.dll`). Mantido como referência intacta.
- `modded/` — Cópia de trabalho contendo as modificações, correções de bugs e refatorações ativas.
- `PROPRIEDADES.md` — Mapeamento completo das configurações do menu F12 (BepInEx ConfigurationManager).
- `backlog/` — Especificações funcionais, técnicas e registros de revisão de bugs.
- `docs/` — Documentação complementar do mod.
- `memory/sessions.md` — Histórico de sessões, lições aprendidas e pendências do mod.

## DLLs Originais

1. **VisceralCombat.dll**: Mod principal com as mecânicas de dismemberment, ragdolls, arterial spraying e pacotes de rede FIKA.
2. **VolumetricBloodFX.dll**: Gerenciamento de partículas e decalques de sangue volumétrico.
3. **bundleloader.dll**: Carregador de bundles de efeitos visuais e sonoros (`ssh/bundles`).

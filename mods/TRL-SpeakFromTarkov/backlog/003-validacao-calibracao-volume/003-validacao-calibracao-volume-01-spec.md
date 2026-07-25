# 003 — validacao-calibracao-volume

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Ajuste e validação dos limiares de sensibilidade de volume do microfone em tempo real para alternar dinamicamente o alcance 3D da voz no mundo do jogo entre Sussurro (10m), Voz Normal (30m) e Grito (60m), com telemetria no painel de Profiler (`F9`).

## Comportamento atual

O código possui a fórmula basilar de `distanceMultiplier = Mathf.Clamp(voiceLevel * 10f, 0.33f, 2.0f)`, porém necessita de validação com microfones reais e telemetria precisa no Profiler.

## Comportamento desejado

- Modulação contínua do alcance de áudio 3D com base nos RMS/picos captados do microfone.
- Exibição de indicador visual no painel de Profiler (`F9`) mostrando a categoria atual (Sussurro / Normal / Grito) e o raio em metros.
- Proteção contra ruídos de fundo e respiração falsa para não classificar barulho de fundo como grito.

## Critérios de aceite

- [ ] Sussurros autênticos limitam a atenuação 3D a no máximo ~10 metros.
- [ ] Fala normal mantém propagação padrão a ~30 metros.
- [ ] Gritos expandem o raio de audição para até ~60 metros.
- [ ] Exibição da categoria e ganho atual no profiler de áudio F9.

## Corner cases

- [ ] Microfone com ganho excessivamente baixo ou alto configurado no Windows.
- [ ] Ruído contínuo de ventilador/ar condicionado tentando disparar o modo grito.

## Referências

- ROADMAP.md §2

# 004 — imersao-equipamento-mascaras-capacete

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Implementação dos efeitos acústicos de imersão baseados no equipamento vestuário do personagem: abafamento de voz para máscaras de gás e balaclavas (*Equipment Voice Muffle*), retorno/eco interno instantâneo da própria voz (*Self-Reverb* de 1ms-5ms), validação do abafamento passivo de capacetes fechados e modulação de voz sob dor grave.

## Comportamento atual

O áudio do microfone é processado de forma homogênea sem considerar os itens equipados na cabeça/rosto do jogador.

## Comportamento desejado

- **Abafamento por Máscara/Balaclava:** Aplicar filtro Low-Pass Filter (LPF) na transmissão ao vestir máscaras de gás (GP-5, Respirador, etc.) ou balaclavas pesadas.
- **Eco Interno da Própria Voz (Gas Mask / Helmet Self-Reverb):** Linha de retorno local sem delay visível (1ms a 5ms) para o próprio jogador escutar a ressonância da fala dentro de capacetes fechados (Altyn/Maska) ou máscaras.
- **Validação de Capacetes Fechados:** Investigar no `Assembly-CSharp` e `VoipMixer` se o `SpatialLowPassFilter` nativo do Tarkov já abafa o som de receptores com capacete pesado.
- **Distorção sob Dor Grave:** Modulação suave de tom/tremor quando HP crítico (<25%) ou sangramento pesado estiver ativo.

## Critérios de aceite

- [ ] Transmissão abafada realista ao falar usando máscara de gás ou respirador.
- [ ] O próprio jogador escuta a ressonância interna de sua fala ao usar capacete com visor/máscara em tempo real.
- [ ] Verificação empírica do filtro nativo `SpatialLowPassFilter` para capacetes fechados.
- [ ] Teste de realismo do efeito de dor grave.

## Corner cases

- [ ] Equipar/desequipar máscara de gás no meio da transmissão da fala.
- [ ] Transição rápida entre visores abertos e fechados.

## Referências

- ROADMAP.md §3

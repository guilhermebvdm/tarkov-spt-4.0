# SpeakFromTarkov (v1.0.0)

Arquivo original de referência: [original/VOIPPlugin.cs](original/VOIPPlugin.cs)

## Seção: VOIP

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Microfone | Microfone | string | `MicrophoneNames[0]` | Lista de microfones ativos | Selecione o microfone. |
| Desativar VOIP do Fika | Desativar VOIP do Fika | bool | `true` | - | - |
| Limiar VAD | Limiar VAD | float | `0.01` | - | - |
| Delay do Eco | Delay do Eco | float | `0.3` | - | - |
| Volume do Eco | Volume do Eco | float | `1.0` | - | - |
| Ganho do Microfone | Ganho do Microfone | float | `1.0` | - | - |
| SampleRate | Taxa de Amostragem | int | `48000` | - | - |
| PushToTalk | PushToTalk | KeyboardShortcut | `Ctrl+V` | - | PTT (ex: Ctrl+V) |
| Toggle Mode | Alternar Modo | KeyboardShortcut | `P` | - | Alternar modo |
| Mute | Mute | KeyboardShortcut | `M` | - | Mutar |
| VAD Decay Time | Tempo de Queda VAD | float | `0.7` | - | - |
| Max Audio Level | Nível Máximo de Áudio | float | `0.015` | - | - |

## Seção: Network & 3D Audio

> Nota: esta tabela não reflete todas as opções já existentes nessa seção (débito de documentação anterior a este item) — adicionada aqui só a entrada nova do item `015-ambiente-acustico-reverb-distancia`.

| Nome | Tradução (pt-BR) | Tipo | Valor Padrão | Faixa/Opções | Tooltip (pt-BR) |
|------|------------------|------|--------------|--------------|-----------------|
| Enable Native Reverb | Reverb Nativo | bool | `true` | - | Roteia a voz dos outros jogadores pelo mixer de áudio nativo do jogo, aplicando o reverb ambiental de cada sala/corredor automaticamente. Desative se notar algum problema de volume ao ligar. |

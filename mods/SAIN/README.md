# SAIN (Solarint's AI Modifications) — SPT 4.0

Repositório e documentação técnica do **SAIN (Solarint's AI Modifications)** para Escape From Tarkov / SPT 4.0.

O SAIN é a estrutura definitiva de inteligência artificial para bots no Tarkov singleplayer e cooperativo, fornecendo percepção visual e auditiva realista, máquinas de estado de combate hierárquicas, cobertura tática dinâmica por raycast e personalidades distintas.

---

## 📚 Documentação Técnica Completa

A documentação modular do mod está dividida em 8 tópicos aprofundados na pasta [`docs/`](./docs/):

1. [**01. Visão Geral e Arquitetura**](./docs/01-visao-geral-e-arquitetura.md) — Topologia Client-Server, ciclo de vida de raid, modelo ECS e BigBrain.
2. [**02. Máquinas de Estado e Tomada de Decisão**](./docs/02-maquinas-de-estado-e-tomada-de-decisao.md) — Camadas de combate, motor de decisão e enums.
3. [**03. Sistema Sensorial: Visão, Audição e Memória**](./docs/03-sistema-sensorial-visao-audicao-e-memoria.md) — Percepção visual, audição espacial e memória preditiva.
4. [**04. Sistema de Combate: Mira, Tiro e Recoil**](./docs/04-sistema-de-combate-mira-tiro-e-recoil.md) — Balística, suavização de visada, recoil e granadas.
5. [**05. Sistema de Cobertura: CoverFinder e Posicionamento**](./docs/05-sistema-de-cobertura-coverfinder-e-posicionamento.md) — Scanner de cobertura, steering e inclinação de tronco.
6. [**06. Personalidades e Sistema de Presets**](./docs/06-personalidades-e-sistema-de-presets.md) — Arquétipos de IA, presets JSON e Editor F6 in-game.
7. [**07. Táticas de Esquadrão, Comunicação e Interoperabilidade**](./docs/07-taticas-de-esquadrao-comunicacao-e-interop.md) — Liderança de grupo, linhas de voz e suporte a Fika/Questing/Looting.
8. [**08. Sistemas Auxiliares: Portas, Médico, Extração e Patches**](./docs/08-sistemas-auxiliares-portas-medico-extracao-e-patches.md) — Portas, medicina de campo, extração e patches Harmony.

👉 [**Acesse o Índice Central da Documentação**](./docs/README.md)

---

## ⚙️ Catálogo de Propriedades e Opções

Para a relação completa de todas as configurações F6, opções de preset e parâmetros editáveis, consulte:
👉 [**PROPRIEDADES.md**](./PROPRIEDADES.md)

---

## ⌨️ Atalhos Úteis em Jogo

- **F6:** Abre o Editor Gráfico In-Game do SAIN (`SAINEditor`), permitindo ajustar dificuldades, multiplicadores e ativar gizmos de debug visual em tempo real.

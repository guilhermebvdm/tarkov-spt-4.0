# 011 — Matriz de comportamento total (não-técnica) + plano de teste

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Consolida TODOS os cenários do sistema Trauma 2.0 (matriz original + toda decisão/premissa/default/interim adotado durante a implementação dos itens 002-008) num único documento em linguagem de COMPORTAMENTO (não técnica — sem nomes de classe, método ou arquivo do EFT), no espírito da planilha original do usuário que deu origem a [docs/trauma-matrix.md](../../docs/trauma-matrix.md). Este documento vira a fonte da verdade dos critérios de aceite para qualquer validação futura (manual in-game ou revisão de código).

## Comportamento atual

`docs/trauma-matrix.md` captura a matriz de design ORIGINAL (aprovada em 2026-07-18, antes de qualquer implementação) — 16 linhas de região×condição×efeito, 22 decisões de design e 20 defaults (D1-D20). Ela é a fonte de verdade de INTENÇÃO, mas não reflete:
- Decisões tomadas DURANTE a implementação que refinaram ou substituíram defaults originais (ex.: o ranking de severidade D1 original foi ajustado pela FSM de queda do item 004; o dedup de one-shots ganhou uma dimensão de região no item 006).
- Comportamentos de corner case resolvidos ao longo do caminho (ex.: qual toggle é master de qual sistema, o que acontece se dois estados coexistirem no mesmo frame).
- Interims e placeholders que existiram temporariamente entre itens (ex.: o item 003 tinha um "handoff Cair→N2" que o item 004 removeu).
- Config final exposta no F12 (nomes, defaults, faixas) — a matriz original só menciona "tudo configurável", sem os valores concretos que cada item escolheu.

Cada spec técnica dos itens 003-008 tem uma nota "premissas novas p/ item 011" ou similar, registrando o que precisa entrar aqui. Essas notas nunca foram consolidadas.

## Comportamento desejado

Um documento único (`docs/trauma-behavior-matrix.md`, ou nome equivalente a definir na spec técnica/implementação) que:

1. **Reapresenta a matriz de efeitos completa** (região × condição × efeito, com/sem analgésico) já **atualizada** com qualquer ajuste feito durante a implementação (não uma cópia da matriz original — a versão final de verdade).
2. **Consolida TODAS as decisões e defaults** (as 22 decisões + 20 defaults de `trauma-matrix.md`, MAIS toda decisão nova tomada durante 002-008) em uma lista única, sem duplicação, em linguagem de comportamento verificável (ex.: "ao expirar o analgésico com 2 pernas zeradas, o jogador agacha imediatamente" em vez de "PainkillerLost dispara reavaliação síncrona").
3. **Documenta a configuração final** exposta no F12 — nome de cada `ConfigEntry`, o que ela controla em termos de comportamento, default e faixa (sem nomes de campo C#).
4. **Registra os interims/premissas que só existiram durante o desenvolvimento** e como foram resolvidos (para rastreabilidade histórica — não são mais comportamento ATIVO, mas documentam POR QUE o sistema chegou nessa forma final).
5. **Alimenta um plano de teste estruturado**: uma lista de cenários de validação manual in-game, organizados por região/sistema, cobrindo os casos da matriz + os corners mais arriscados identificados durante a implementação (ex.: hit simultâneo tórax+cabeça, coexistência de dois one-shots adiados).

## Critérios de aceite

- [ ] O documento cobre as 15 linhas da matriz de efeitos original, cada uma com status (entregue como especificado / ajustado — com a razão do ajuste).
- [ ] Todas as 22 decisões + 20 defaults de `trauma-matrix.md` aparecem no documento novo, cada uma com uma nota se foi implementada como estava ou revisada (com link para a spec/review que revisou).
- [ ] Toda "premissa nova p/ item 011" encontrada nas specs técnicas e memória dos itens 002-008 está incorporada — nenhuma marcada como pendente sem essa marcação vir de uma varredura real dos artefatos (não é aceitável escrever o documento "de memória" sem reler as specs).
- [ ] A tabela de configuração final (F12) lista TODAS as `ConfigEntry` ativas relacionadas ao Trauma 2.0 (não as legadas/inertes), com nome exibido, o que controla (comportamento), default e faixa.
- [ ] O plano de teste cobre pelo menos 1 cenário de validação por linha da matriz + os corners mais arriscados identificados (mínimo: hit simultâneo em múltiplas regiões, coexistência de dois one-shots adiados na fila, comportamento sob toggle OFF mid-raid, comportamento em Fika coop com 2 peers).
- [ ] **Fika/multiplayer:** N/A — este item é documentação, não código; não introduz comportamento novo de rede.
- [ ] **Estado entre raids:** N/A — este item é documentação, não código.

## Corner cases

- [ ] Decisões que se CONTRADIZEM entre specs de itens diferentes (ex.: uma spec antiga descreve um comportamento que uma spec posterior mudou sem atualizar a primeira) — o documento final reflete o estado MAIS RECENTE (a spec do item que entregou por último "ganha"), com uma nota explícita da contradição e por quê.
- [ ] Decisões registradas na memória do mod (não em spec) — ex.: correções feitas diretamente durante `/apply-code-review` sem re-gerar a spec técnica. O documento precisa varrer também a memória (`memory/sessions.md`), não só as specs.
- [ ] Itens do backlog que ainda NÃO foram implementados no momento de escrever este documento (009, 010) — o documento cobre só 002-008 (o que já existe); 009/010/011 ficam fora do escopo de comportamento a documentar (011 é este próprio documento).

## Fora de escopo

- [ ] Qualquer mudança de código — este item é 100% documentação.
- [ ] O "code-review dedicado de validação de critérios" mencionado no resumo do item no `mod-backlog.md` — é uma atividade FUTURA que consome este documento como insumo, não parte da entrega deste item.
- [ ] Item 009 (hardening coop) e 010 (migração de configs + release) — cobertos pelo documento só na medida em que já têm comportamento definido nos itens 002-008; a AÇÃO desses itens é fora de escopo.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — matriz de design original (ponto de partida)
- Specs funcionais/técnicas e reviews de 002-008 (`mods/TRL-ImmersiveCombatMedicine/backlog/00{2..8}-*/`)
- `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` — decisões registradas fora de spec

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Item criado via `/create-spec` (retomada do overhaul Trauma 2.0, P-3.4/P-3.7 — item final do ciclo 003-011) |
| 2026-07-19 | Entregue como [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) — extração paralela por 7 agentes (um por item 002-008) + síntese + verificação de completude independente (achou 9 premissas de prioridade alta ausentes, majoritariamente do item 004; todas incorporadas). Item FECHADO 🟢. |

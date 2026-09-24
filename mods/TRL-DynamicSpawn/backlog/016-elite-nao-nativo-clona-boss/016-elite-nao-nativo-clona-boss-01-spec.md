# 016 — elite-nao-nativo-clona-boss

**Mod:** TRL-DynamicSpawn
**Status:** Backlog
**Criado:** 2026-09-23

## Visão geral

O caminho de spawn de "elites não-nativos" (chefes e esquadrões especiais em mapas onde o EFT não tem onda nativa pra eles) usa o mesmo mecanismo de "grupo" tanto pra esquadrões genéricos (Rogues, Raiders) quanto pra chefes únicos com nome próprio (Sanitar, Tagilla, Killa, Zryachiy, Gluhar, Kolontay, Reshala, Kaban, Shturman, Partisan). Pra chefes únicos isso está errado: em vez de acompanhar o chefe com os guardas dele, o mod clona o próprio chefe quando o sorteio de grupo forma 2-3 integrantes, gerando múltiplas cópias idênticas do mesmo boss na mesma raid.

## Comportamento atual

Um chefe configurado como "elite não-nativo" (ex: Sanitar habilitado em Labs pelo painel, mapa onde o Sanitar não tem onda nativa do EFT) passa por dois sorteios independentes: (1) chance de spawn do chefe no mapa e (2) chance de "virar grupo" (`groupChance`) que define um tamanho de 2 a `maxGroupSize`. Quando o segundo sorteio forma um grupo, o mod cria **todos** os integrantes desse grupo com o mesmo papel do chefe — resultando em 2 ou 3 cópias idênticas do mesmo boss (mesmo nome, mesmo modelo, mesmo comportamento de "chefe") em vez de 1 chefe único acompanhado dos guardas dele. Foi reproduzido em Labs com o Sanitar (relato do usuário: 2 "boss Sanitar" simultâneos após um transit Factory → Labs), mas o mesmo caminho de código afeta qualquer chefe único habilitado como elite não-nativo em um mapa fora do seu mapa de origem. Rogues e Raiders — que são esquadrões genéricos sem identidade única — já se comportam corretamente hoje formando grupos de integrantes idênticos entre si; esse é o comportamento correto e esperado só pra eles.

## Comportamento desejado

Rogues, Raiders e Bloodhounds continuam podendo formar grupo de múltiplos integrantes idênticos via `groupChance`/`maxGroupSize` (mecanismo inalterado; Bloodhounds passa a usar o mesmo caminho que Rogues/Raiders — decisão tomada durante a spec técnica — com um novo tamanho de esquadrão padrão). Para todo chefe único (incluindo Cultistas/`sectantPriest`, que ganha `sectantWarrior` como guarda dedicado), essa mesma configuração (`groupChance`, `maxGroupSize`, `disableFollowers`) passa a controlar exclusivamente se e quantos **guardas dedicados daquele chefe** acompanham o spawn — nunca cópias do próprio chefe. O chefe sempre spawna como uma única unidade. Chefes sem guarda dedicado conhecido (Killa, Partisan, Gifter) continuam spawnando sozinhos, independente da configuração de grupo.

## Critérios de aceite

- [ ] Ao spawnar um chefe único (ex: Sanitar) como elite não-nativo em um mapa sem onda nativa dele (ex: Labs), no máximo 1 unidade do próprio chefe aparece por acionamento do spawn — nunca 2 ou mais.
- [ ] Quando o chefe tem guarda(s) dedicado(s) e `disableFollowers = false`, o(s) integrante(s) extra(s) do grupo usam o papel de guarda correto daquele chefe, nunca o papel do próprio chefe — inclui Cultistas (`sectantPriest` + `sectantWarrior` como guarda).
- [ ] Quando `disableFollowers = true` para um chefe, ele spawna sozinho, sem nenhum guarda, independente de `groupChance`/`maxGroupSize`.
- [ ] Chefes sem guarda dedicado conhecido continuam sempre sozinhos, mesmo com `disableFollowers = false` e `groupChance > 0`.
- [ ] Rogues, Raiders e Bloodhounds continuam formando grupo de integrantes idênticos, no mesmo mecanismo de hoje (Bloodhounds passa a usar esse caminho pela primeira vez, com tamanho de esquadrão fixo em 4) — nenhuma regressão em Rogues/Raiders.
- [ ] **Fika/multiplayer:** N/A: a injeção do gerenciador de spawn dinâmico já detecta cliente convidado e desativa a própria lógica de geração de elites pra ele (só o host gera); a correção reorganiza quem nasce em cada grupo, não quem decide gerar — nenhuma superfície de rede nova, nenhum host convidado passa a gerar elite.
- [ ] **Estado entre raids:** N/A: o sorteio de chefe/guarda continua acionado no máximo 1× por raid pela mesma flag estática já existente hoje, resetada no fim de toda raid (extração, morte, MIA ou alt-F4) e de novo a cada transit; a correção não adiciona nenhuma coleção estática ou flag nova que precise de limpeza própria.

## Corner cases

- [ ] Chefe com mais de um tipo de guarda distinto (ex: Gluhar tem 4 papéis de guarda diferentes) — definir como distribuir os integrantes extras entre os tipos disponíveis quando mais de 1 guarda é sorteado.
- [ ] `maxGroupSize` configurado alto (ex: 3) para um chefe que só tem 1 tipo de guarda conhecido — o grupo deve poder repetir esse único tipo de guarda pra preencher o tamanho sorteado (repetir guarda é válido; repetir o chefe não).
- [ ] Zona configurada no painel sem pontos de spawn suficientes pra acomodar chefe + guardas — comportamento de fallback (mesma regra de restrição de zona já existente pra bosses, ver sessão 2026-09-03 v3.7.3).
- [ ] Chefe em mapa onde ele **tem** onda nativa do EFT (ex: Sanitar em Shoreline) — esse caminho já trata boss+guardas corretamente hoje (sessão 2026-09-03 v3.7.4) e não deve ser alterado por este item; confirmar que a correção fica isolada ao caminho "não-nativo".
- [ ] Líder do grupo (chefe) morre durante o escalonamento de spawn antes dos guardas terminarem de nascer — garantir que a sucessão de liderança existente não tente promover um guarda a "chefe" com o papel errado.
- [ ] Mais de um chefe elite não-nativo habilitado no mesmo mapa ao mesmo tempo (ex: Sanitar e Kolontay ambos configurados pra Labs) — cada chefe deve montar seu próprio grupo (ele + os guardas dele) de forma independente, sem misturar guarda de um chefe no grupo de outro.
- [ ] Compatibilidade com SAIN (quando instalado): guardas agora nascem com o papel de seguidor correto do chefe em vez do papel do próprio chefe — confirmar que o SAIN aplica a IA/dificuldade de guarda esperada pra esse papel, e não a de chefe.

## Fora de escopo

- [ ] A definir

## Referências

- [008-maxbot-dinamico-e-elites-nao-nativos/](../008-maxbot-dinamico-e-elites-nao-nativos/) — item que introduziu o caminho de elites não-nativos onde o bug vive hoje.
- Relato do usuário nesta sessão: transit Factory → Labs resultando em 2 "boss Sanitar" simultâneos.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Item criado via `/add-backlog-item` |
| 2026-09-23 | Revisão `/review-spec` — 0 gaps de conteúdo + 2 corner cases corrigidos (múltiplos chefes simultâneos no mesmo mapa; compatibilidade SAIN) + reforço de justificativa nos dois critérios N/A (Fika e estado entre raids) |
| 2026-09-23 | PA-01-03 (review técnica 01): Bloodhounds incluído no comportamento desejado e no critério de Rogues/Raiders/grupo; Cultistas/`sectantWarrior` mencionado no critério de guarda correto — alinhando com decisões tomadas na spec técnica |

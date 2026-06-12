# 033 — Detalhe single-screen (dashboard) — Spec

**Mod:** CustomClasses
**Status:** Especificado
**Criado:** 2026-06-12
**Origem:** [033-detalhe-single-screen-00-kickoff.md](./033-detalhe-single-screen-00-kickoff.md)

## Visão geral

A tela de detalhe de uma classe (`ClassDetail`) deixa de empilhar painéis expansíveis e passa a um **dashboard de tela única em duas colunas**, no espírito do viewer antigo de perfis: header compacto com badges (custos, status, ações) no topo, **coluna esquerda estreita** com a lista canônica de skills (reaproveitando o componente do item 031 em modo read-only) somada a multiplicadores e hideout, e **coluna direita** com equipado em cima e stash embaixo. Densidade tipográfica alta (12–14px, paddings curtos) via uma folha de estilo local nova. Nenhuma informação fica atrás de um clique ou de uma expansão: skills, multiplicadores, hideout, equipado, stash e custos estão todos visíveis de uma vez.

A motivação é de UX: hoje, ver "skills + equipado + custo" exige abrir três painéis e rolar uma página longa de padding generoso. O objetivo deste item é a **estrutura densa de duas colunas com zero expansões**. A compactação visual final do equipado/stash (grids de ícones) é entregue só no item 034 — aqui a coluna direita permanece **textual**, mas já estruturada num ponto de troca claro para o 034.

## Comportamento atual

- `ClassDetail.razor` renderiza um `MudExpansionPanels` com 8 painéis empilhados (General, Skills, XP multipliers, Hideout, Outfit, Equipped, Stash, Cost summary), alguns abertos por padrão e outros fechados.
- O painel "Skills" **já** usa o `SkillCanonicalList` em modo read-only (adoção feita pelo item 031); o total ponderado + chip de budget + warnings ficam logo abaixo.
- Header é uma linha (`MudStack Row`) com back, ícone, nome colorido, chip de status e os botões Edit/Duplicate/Delete; não há badges de custo/loadout no header — os custos só aparecem no painel "Cost summary" lá embaixo.
- Multiplicadores, hideout, outfit, equipado e stash estão cada um em seu painel; ver qualquer um exige expandir/rolar. Não há folha de estilo local do mod — só o tema MudBlazor default (paddings generosos).

## Comportamento desejado

- **Sem expansion panels:** o `MudExpansionPanels` é removido. Todo o conteúdo é organizado num layout fixo de duas colunas, tudo visível sem cliques.
- **Header compacto com badges:** nome na cor da classe + ícone + chip de status + descrição em 1 linha (truncada, resto no tooltip) + **badges** de custo ponderado de skills (com indicação de budget), total ₽ do loadout, baseEdition e enabled, mais os botões Edit/Duplicate/Delete. Diagnostics (parse/dry-run), se houver, aparecem como alertas finos logo abaixo do header.
- **Coluna esquerda (estreita, ~300px):** a lista canônica de skills (componente do 031, read-only, com custo inline e chips ±% de multiplicador), o total ponderado de skills + budget, e o hideout como badges compactos (ex.: "Heating L1"). Os multiplicadores de XP que existirem aparecem **na própria linha da skill** via o chip ±% do componente (já entregue pelo 031) — sem uma tabela separada de multiplicadores. Multiplicadores em skills fora do mapa canônico, ou que precisem de aviso (Skills-Extended ausente), aparecem como nota fina.
- **Coluna direita (flexível):** "Equipped" em cima (lista por slot, reaproveitando o renderer recursivo existente) e "Stash" embaixo (tabela compacta de linhas precificadas com subtotais). Esta coluna é **textual** neste item; o ponto de inserção é estruturado de forma que o 034 substitua o textual por painéis visuais (grids de ícones) sem rearranjar o layout.
- **Densidade:** todos os componentes Mud da página em modo denso + uma folha `customclasses.css` nova (importada uma vez pelo layout) com tipografia 12–14px, line-height ~1.3 e paddings 4–8px, espelhando os tokens de densidade do viewer antigo. Apenas o necessário para o dashboard — não um design system.
- **Troca de classe mantém o layout:** abrir outra classe pela sidebar (item 030) re-renderiza o mesmo dashboard, permitindo comparação visual imediata.
- **Outfit:** continua exibido (sem painel próprio), de forma compacta. Decisão de posicionamento na spec técnica (premissa registrada lá).

## Premissas registradas (decisões autônomas)

- **P1 — Coluna direita textual neste item:** o equipado e o stash permanecem textuais (lista por slot + tabela de stash). O kickoff é explícito que o "single-screen completo ≤1 scroll" só fecha no 034 com os grids de ícones. Este item entrega a **estrutura** (duas colunas, zero expansões, densidade) e um ponto de troca nomeado para o 034.
- **P2 — Sem tabela separada de multiplicadores:** os multiplicadores de XP por skill já aparecem como chip ±% na linha da skill (componente do 031). A tabela dedicada "XP multipliers" do detalhe atual é **removida** da página densa; nenhuma informação se perde (o chip cobre o caso comum). Multiplicadores em skills fora do mapa canônico continuam visíveis via a seção de transbordo do próprio componente. O aviso "Skills-Extended ausente" vira uma nota fina na coluna esquerda. Coerente com a P2 do item 031.
- **P3 — "Cost summary" full breakdown não cabe na tela densa:** o header carrega os dois totais como badges (skills ponderado + ₽ loadout) e os warnings de custo aparecem como alertas finos. O **breakdown completo linha-a-linha** (equipado + stash + ammo + contents) do painel "Cost summary" atual é redundante com o stash textual da coluna direita e não cabe sem reintroduzir scroll longo; é **retirado** da tela densa neste item. O dado bruto continua acessível via os serviços de custo (CostService) — não é perda de funcionalidade do servidor, só de uma tabela de auditoria nesta tela. Reavaliar se algum item futuro pedir auditoria de preço dedicada.
- **P4 — Largura do container:** a página passa a usar a largura cheia disponível (não mais `MaxWidth.Large` centralizado), porque o layout de duas colunas precisa de espaço horizontal — caso contrário a coluna direita fica espremida. Decisão cosmética, ajustável.
- **P5 — Outfit compacto na coluna esquerda:** o outfit (roupas USEC/BEAR) é informação de baixa frequência; vai como bloco compacto no fim da coluna esquerda (abaixo do hideout), não na direita, para não competir com equipado/stash. Cosmético.

## Critérios de aceite

- [ ] A página de detalhe **não contém nenhum `MudExpansionPanel`/`MudExpansionPanels`** — verificável por inspeção do `.razor` (zero ocorrências).
- [ ] Skills (lista canônica), hideout, equipado, stash e os dois totais de custo estão **todos visíveis ao abrir a classe**, sem nenhum clique de expansão — verificável abrindo uma classe rica e conferindo que cada bloco está renderizado de imediato.
- [ ] O layout é de **duas colunas**: skills/hideout/outfit à esquerda (coluna estreita), equipado/stash à direita (coluna flexível) — verificável visualmente e por inspeção das classes CSS de layout aplicadas.
- [ ] O header exibe os **badges de custo** (skill ponderado com indicação dentro/fora do budget + total ₽ do loadout) além de status e ações Edit/Duplicate/Delete — todos funcionando como hoje (Edit navega, Duplicate/Delete abrem os diálogos existentes).
- [ ] A lista de skills continua sendo renderizada pelo **`SkillCanonicalList` em modo read-only** (a adoção do item 031 é preservada, não duplicada nem reimplementada) — verificável por inspeção (a tag `<SkillCanonicalList ... Editable="false">` segue presente).
- [ ] Existe a folha `wwwroot/css/customclasses.css` nova com regras de densidade reutilizáveis, importada **uma única vez** pelo `BaseLayout.razor` (uma linha de `<link>`), sem alterar o restante do layout montado pelo item 030.
- [ ] Trocar de classe pela sidebar mantém o mesmo dashboard de duas colunas (sem regressão no fluxo de navegação do item 030).

## Corner cases

1. **Classe não encontrada / arquivo não parseável:** a página mantém o tratamento atual (alerta de erro quando o arquivo não casa; mensagem de "fix on disk" quando parseia mas `Definition` é null). Nestes casos **não** há duas colunas — só o header mínimo + o alerta; o layout denso só se aplica quando há `Definition`.
2. **Classe "pelada" (sem skills, sem loadout, sem hideout):** as duas colunas ainda aparecem; a lista canônica mostra tudo esmaecido (comportamento do 031), e os blocos de equipado/stash/hideout mostram seus estados vazios ("No equipped items." etc.) — nada quebra o grid.
3. **Loadout grande (muitos slots equipados + stash longo):** a coluna direita textual pode ficar mais alta que a esquerda; o grid não deve quebrar nem desalinhar o header. Aceita-se que a coluna direita gere scroll **interno à página** neste item (o ≤1 scroll total é meta do 034). As colunas alinham pelo topo (`align-items: flex-start`).
4. **Diagnostics presentes (warnings/errors de dry-run):** os alertas finos aparecem entre o header e as colunas, sem empurrar o conteúdo para fora de uma estrutura previsível, e sem reintroduzir expansão.
5. **Skill fora do mapa canônico / Skills-Extended ausente:** a skill fora do mapa aparece na seção de transbordo do `SkillCanonicalList` (já tratado pelo 031); o aviso de Skills-Extended ausente vira nota fina na coluna esquerda em vez de um `MudAlert` dentro de um painel.
6. **Nome de classe muito longo / descrição longa:** o nome não quebra o header em várias linhas a ponto de empurrar os badges; a descrição trunca em 1 linha com reticências e o texto completo fica no tooltip.

## Fora de escopo

- **Compactação visual do equipado/stash em grids de ícones (item 034):** este item deixa a coluna direita textual, com o ponto de troca estruturado. O CSS de densidade nasce aqui e é reaproveitado pelo 034.
- **Matriz multi-classe (032) e comparação A×B (036):** este item é a tela de **uma** classe.
- **Densidade global de todas as telas do editor (035):** aqui o `customclasses.css` cobre o necessário para o dashboard de detalhe; a aplicação ampla é do 035.
- **Qualquer mudança em serviços (CostService, ClassEditorService, CatalogService), no schema da classe, ou no componente `SkillCanonicalList` (item 031, consumido como está).**
- **NavMenu / sidebar / layout estrutural:** território do 030 (mergeado); o 033 só adiciona a linha de import do CSS no `BaseLayout`.

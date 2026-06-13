# 031 — Skills em ordem canônica (componente) — Spec

**Mod:** CustomClasses
**Status:** Especificado
**Criado:** 2026-06-12
**Origem:** [031-skills-ordem-canonica-00-kickoff.md](./031-skills-ordem-canonica-00-kickoff.md)

## Visão geral

Um único componente compartilhado renderiza **TODAS as skills da tabela canônica sempre na mesma ordem e na mesma posição visual**, agrupadas por categoria, com nível 0 esmaecido. A ordem é derivada da fonte de verdade (a tabela de pesos/categorias já existente no mod) — **sem números mágicos** de contagem em spec ou UI. O componente é adotado tanto na tela de detalhe (read-only) quanto na tela de edição (nível direto na linha, substituindo o fluxo "adicionar skill" por dropdown). Esse mesmo componente é projetado para servir, sem reescrita, o dashboard de leitura e a coluna de comparação A×B de itens posteriores.

A motivação é de UX: hoje cada tela mostra apenas as skills que a classe define, na ordem em que aparecem no arquivo, sem categorias — então comparar duas classes de cabeça é impossível. Com posição fixa, a leitura e a comparação ficam instantâneas.

## Comportamento atual

- **Detalhe:** painel "Skills" lista **somente** as skills definidas pela classe, na ordem do dicionário do arquivo, em forma de tabela (skill, nível, peso, origem do peso, custo). Sem categorias, sem barras, sem posição fixa.
- **Edição:** aba "Skills" lista somente as skills definidas, com campo numérico de nível e botão de remover por linha; novas skills entram por um dropdown "Add skill" + botão "Add". Skill com nível 0 só existe na lista se já estava no arquivo ou foi adicionada manualmente.
- Custo total ponderado, chip de budget e warnings já são exibidos em ambas as telas (origem: serviço de custo do item 022).

## Comportamento desejado

- **Lista canônica única:** ambas as telas renderizam o mesmo componente com **todas** as skills da tabela canônica, na ordem fixa Physical → Mental → Combat → Practical, seguida de uma seção final **Special Elite** com as skills revividas pelo mod de compatibilidade (Skills-Extended). Cada categoria tem um separador rotulado, na cor da categoria.
- **Nível 0 esmaecido:** skill com nível 0 aparece na sua posição fixa, esmaecida (opacidade reduzida) e com "—" no lugar do número; skill com nível > 0 aparece em destaque, com o número.
- **Barra por categoria:** cada linha tem uma barra de progresso proporcional ao nível (0–10 = 0–100%; níveis acima de 10 saturam em 100%), pintada na cor da categoria.
- **Custo inline:** cada linha com nível > 0 mostra o custo da skill (nível × peso) ao lado; o componente nunca recalcula peso/custo por conta própria — consome o breakdown já produzido pelo serviço de custo.
- **Chip de multiplicador de XP:** quando a classe define um multiplicador de XP para a skill, a linha exibe um chip ±% (ex.: "+50%", "−25%") derivado do fator; fator 1 não gera chip.
- **Modo edição inline:** na tela de edição, cada linha tem o campo de nível **direto na linha canônica** — editar o número é a única forma de "definir" a skill; o dropdown "Add skill" + botão "Add" é removido. Mudar o nível recomputa o custo total como já ocorre hoje.
- **Round-trip de zeros (decisão):** zeros **pré-existentes no arquivo** são preservados (a skill continua "definida em 0", visualmente distinguível de uma nunca-definida apenas pela origem no arquivo — ver premissa P3); o salvamento **não cria** entradas novas com nível 0 para skills que o autor não tocou. Skills nunca definidas e deixadas em 0 não entram no arquivo salvo.
- **Adoção:** detalhe usa o componente em modo read-only; edição usa em modo edit. A aba de multiplicadores de XP permanece como está (premissa P2).

## Premissas registradas (decisões autônomas)

- **P1 — Fonte da ordem/contagem:** a ordem canônica e a contagem derivam da tabela de categorias/pesos já existente no mod; nenhum número de contagem é escrito nesta spec nem embutido na UI. A spec técnica define como a ordem é construída a partir dessa tabela.
- **P2 — Aba de multiplicadores:** o kickoff deixa "decidir na spec" se a aba de multiplicadores adere ao mesmo layout. **Decisão: NÃO** neste item — multiplicador não tem nível nem categoria de custo e seu fluxo atual (adicionar/remover fator) é diferente o suficiente para não caber na lista canônica sem ruído. O chip ±% na lista de skills já dá a visão cruzada desejada. Reavaliar em item de densidade posterior.
- **P3 — "0 definido" × "0 não definido":** ambos renderizam idênticos (esmaecido, "—"). A distinção é **apenas de persistência** (um já está no arquivo, o outro não) e não precisa de marca visual dedicada neste item; o critério verificável é o conteúdo do arquivo salvo, não um pixel.
- **P4 — Seção Special Elite:** as 4 skills do mod de compatibilidade formam a seção final, mesmo quando a tabela canônica as classifica numa das 4 categorias principais para fins de peso. A pertinência à seção Special Elite é decidida pelo conjunto de skills do compat, não pela categoria de custo (a spec técnica detalha).

## Critérios de aceite

- [ ] Detalhe e edição mostram exatamente o mesmo conjunto e a mesma ordem de skills, e esse conjunto/contagem é igual ao número de skills derivado da tabela canônica (sem hardcode) — verificável comparando a contagem renderizada com a contagem derivada da tabela.
- [ ] Toda skill aparece sempre na mesma posição independentemente da classe aberta: abrir duas classes diferentes e conferir que a N-ésima linha é a mesma skill nas duas.
- [ ] Skill com nível 0 (ou ausente) renderiza esmaecida com "—"; skill com nível > 0 renderiza com o número e a barra proporcional na cor da categoria.
- [ ] Editar o nível de uma skill diretamente na linha altera o custo total exibido (mesmo recompute de hoje), sem precisar de nenhum dropdown "Add skill".
- [ ] Salvar uma classe sem ter tocado em skills NÃO adiciona entradas novas com nível 0 no arquivo; e um 0 que já existia no arquivo continua presente após salvar (round-trip preservado).
- [ ] Quando a classe define multiplicador de XP para uma skill, a linha exibe o chip ±% correspondente; fator 1 não exibe chip.

## Corner cases

1. **Classe sem nenhuma skill (ex.: classe "pelado"):** todas as linhas aparecem esmaecidas com "—", custo total 0, sem warnings de budget — a lista canônica completa ainda é exibida (a posição fixa é o ponto).
2. **Nível acima do teto visual (ex.: 20, ou níveis legados > 51):** a barra satura em 100% sem estourar o layout; o número exibido é o nível real, não o saturado.
3. **Skill no arquivo que não pertence à tabela canônica (nome desconhecido / enum não mapeado):** a lista canônica não tem posição para ela; ela não é "perdida" silenciosamente — deve aparecer numa área de transbordo (ex.: ao fim, fora das categorias) com indicação de que está fora do mapa, coerente com o warning que o serviço de custo já emite. A spec técnica define o local.
4. **Zero pré-existente no arquivo + autor não edita:** após salvar, o 0 continua no arquivo (não pode ser derrubado em silêncio).
5. **Multiplicador de XP para skill cujo nível é 0:** a linha aparece esmaecida (nível 0) mas ainda exibe o chip ±% — o multiplicador é independente do nível inicial.
6. **Skill da seção Special Elite com o mod de compatibilidade ausente:** a linha ainda aparece na seção (a definição da classe pode tê-la), mas a tela já sinaliza em outro ponto que o compat está ausente; este item não precisa de marca extra por linha além do comportamento já existente.

## Fora de escopo

- Matriz de skills multi-classe (item 032) e dashboard single-screen (item 033) — este item apenas **entrega o componente** já parametrizado para esses usos, sem montar essas telas.
- Coluna de comparação A×B renderizada (item 036) — os parâmetros de comparação nascem aqui, mas o cálculo/preenchimento dos deltas e a UI de seleção da classe B são do 036.
- Qualquer mudança no modelo de custo, nos pesos, ou na aba de multiplicadores de XP.
- Persistência de novos campos no schema da classe.
</content>
</invoke>

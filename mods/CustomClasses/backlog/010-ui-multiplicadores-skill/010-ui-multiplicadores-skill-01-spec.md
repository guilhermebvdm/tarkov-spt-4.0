# 010 — UI dos multiplicadores de skill

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Refinar a apresentação visual dos multiplicadores de skill por classe na tela de **Skills** do jogo (refino do item 005, já validado). Hoje a indicação é sutil (uma seta de "efetividade" reaproveitada no fim da barra de progresso + um texto anexado ao tooltip nativo). O objetivo é tornar o buff/debuff **óbvio e auto-explicativo**: borda colorida no ícone da skill, um marcador `±X%` colorido logo após o nome da skill, e um tooltip dedicado que nomeia a classe responsável.

## Comportamento atual

Implementado no item 005 (fatia 2, client BepInEx):

- **`SkillPanelPatch`** (postfix em `SkillPanel.method_1`): para skills com multiplicador, acende o `GameObject` `____effectivenessUp` (seta para cima) ou `____effectivenessDown` (seta para baixo) — os mesmos da lógica vanilla de efetividade/fadiga. Visualmente é a **seta azul no fim da barra de progresso**. Não há cor própria (usa o sprite vanilla) nem percentual textual na linha.
- **`SkillTooltipPatch`** (postfix em `SkillTooltip.Show(SkillClass)`): anexa ao maior `TMP_Text` do tooltip nativo uma linha `XP da classe: +X%` (verde) / `-X%` (vermelho).
- O **ícone** da skill não recebe destaque algum.
- O client conhece apenas o mapa `skill→fator` (rota `/customclasses/skill-multipliers`); **não conhece o nome da classe/edition** do perfil atual.

## Comportamento desejado

Na tela de Skills, para cada skill que tenha multiplicador da classe (`fator ≠ 1`):

1. **Borda colorida no ícone** da skill: **verde** quando buff (`fator > 1`), **vermelha** quando debuff (`fator < 1`). Brilho/intensidade é detalhe de implementação (pode ser borda sólida simples). Skills sem multiplicador não recebem borda.
2. **Marcador `±X%` à direita do nome** da skill, logo após o texto do nome: seta **verde para cima + `+X%`** (buff) ou seta **vermelha para baixo + `-X%`** (debuff). Este marcador **substitui** o uso atual da seta azul de efetividade no fim da barra (a lógica vanilla de efetividade/fadiga volta a ser deixada intacta).
3. **Tooltip dedicado** ao passar o mouse sobre o marcador `±X%` (e/ou sobre a borda do ícone), com a mensagem:
   - Buff: `Você possui **+X% de buff** nessa skill devido à Classe **<Nome da classe>**`
   - Debuff: `Você possui **-X% de debuff** nessa skill devido à Classe **<Nome da classe>**`
   - O **nome da classe** em **negrito**; o trecho `+X% de buff` / `-X% de debuff` colorido no **mesmo verde/vermelho** das bordas e da seta.
   - "buff" para verde, "debuff" para vermelho.
   <!-- review: o tooltip "dedicado" (hover na seta) depende de existir uma área de hover/tooltip reaproveitável no EFT (SimpleTooltip/HoverTooltipArea). Se inviável, fallback ACEITÁVEL: manter a frase completa (com nome da classe) anexada ao tooltip nativo da skill, substituindo o "XP da classe: +X%" atual do 005. Decidir na tech-spec. -->

4. O **nome da classe/edition** do perfil atual passa a ser conhecido pelo client (a rota do server precisa expô-lo junto com os multiplicadores).

As cores verde (buff) e vermelha (debuff) devem ser **consistentes** entre borda, seta, percentual e tooltip.

## Critérios de aceite

- [ ] Numa skill com **buff** da classe, o ícone exibe borda **verde** e aparece uma seta verde + `+X%` à direita do nome.
- [ ] Numa skill com **debuff** da classe, o ícone exibe borda **vermelha** e aparece uma seta vermelha + `-X%` à direita do nome.
- [ ] Skill **sem multiplicador** (`fator = 1` ou ausente) não recebe borda nem marcador — aparência vanilla intacta.
- [ ] O **tooltip do marcador** mostra a frase com o nome da classe em negrito e o percentual colorido (verde=buff/vermelho=debuff), usando o texto "buff"/"debuff" conforme o sinal.
- [ ] O **nome da classe** exibido no tooltip corresponde à edition do perfil logado (ex.: "Operador Tático").
- [ ] A seta azul vanilla de efetividade **não** é mais acionada pelo mod (volta ao comportamento original do jogo).
- [ ] Percentual calculado de forma consistente: `+50%` para fator `1.5`, `-30%` para fator `0.7` (arredondado a inteiro).

## Corner cases

- [ ] **Perfil sem classe do mod** (edition vanilla): sem multiplicadores → nenhuma borda/seta/tooltip; nada quebra.
- [ ] **Cache não carregado** (perfil novo, sem XP ganho ainda) ao abrir a tela de Skills: o nome da classe e os fatores precisam estar disponíveis (mesma garantia `EnsureLoaded()` já usada no 005) — borda/seta/tooltip aparecem mesmo sem ganho prévio de XP.
- [ ] **Nome da classe indisponível** (rota não retornou o nome, ou edition desconhecida): o tooltip deve degradar com um fallback (ex.: omitir o nome ou usar "sua Classe") sem lançar exceção.
- [ ] **Re-render / reuso de células** da lista de skills (UI recicla `SkillPanel` ao rolar): a borda/seta de uma skill não pode "vazar" para outra ao rolar; aplicar/limpar conforme o multiplicador da skill atual da célula.
- [ ] **Abrir/fechar a tela várias vezes**: o marcador `±X%` e a borda não podem ser duplicados (instanciar uma vez por célula / reusar; guardar contra duplicação como já feito no tooltip do 005).
- [ ] **Skill com fator extremo** (ex.: `2.0` = `+100%`, ou `0` = `-100%`): texto e cor continuam corretos.
- [ ] **Nome de skill longo**: o marcador `±X%` à direita do nome não pode empurrar/quebrar o layout da linha nem sobrepor a barra de progresso. Definir posicionamento/anchor robusto (a tech-spec escolhe o transform-pai e o anchoring).
- [ ] **Skill em nível Elite / máximo**: o destaque vanilla de elite (moldura/cor própria do jogo) coexiste com a borda do mod sem conflito visual — a borda do mod não substitui nem é substituída pela moldura de elite.
- [ ] **Modo de exibição da lista** (a tela de Skills tem alternância lista/grade — botões no topo): a borda/seta devem funcionar (ou degradar de forma limpa) em ambos os modos, ou o escopo deve declarar qual modo é suportado.
- [ ] **Localização**: textos "buff"/"debuff" e a frase do tooltip ficam em pt-BR neste item (i18n completo é o item 008); evitar hardcode que atrapalhe o 008 (centralizar as strings).

## Fora de escopo

- [ ] Internacionalização das novas strings (pt-BR/en) — fica no item 008.
- [ ] Alterar a lógica de **escala de XP** (server registry/router de fatores e patches de `OnTrigger`/gym) — já entregue no 005; aqui só muda a apresentação + exposição do nome da classe.
- [ ] Exibir multiplicadores fora da tela de Skills (ex.: HUD em raid, hideout).

## Referências

- Item 005 (base): [005-skill-multipliers-01-spec.md](../005-skill-multipliers/005-skill-multipliers-01-spec.md)
- Patches de UI atuais: `modded/Client/Patches/SkillPanelPatch.cs`, `modded/Client/Patches/SkillTooltipPatch.cs`
- Rota/registry do server: `modded/Server/SkillMultipliersRouter.cs`, `modded/Server/SkillMultiplierRegistry.cs`

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` |
| 2026-06-07 | Revisão `/review-spec` — +3 corner cases (nome longo, nível Elite, modo lista/grade); 1 trecho marcado `<!-- review -->` (fallback do tooltip dedicado, decisão técnica) |

# 003 — Itens iniciais (stash/equipado/composto) + hideout + 10 classes reais

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Item central do mod, com escopo expandido por decisão do usuário (2026-06-07). Entrega:

1. **Capacidade de itens iniciais** no schema da classe, em três formas: (a) **soltos no stash**, (b) **equipados** no personagem (slots de equipamento), (c) **compostos** — itens formados por outros itens, montados como árvore (arma+mods+carregador, rig/mochila com conteúdo, armadura com placas).
2. **Capacidade de hideout**: cada classe pode iniciar com **estações de hideout** em níveis definidos (o RZ dava 1 estação temática por classe).
3. **Conteúdo: as 10 classes reais** já montadas no RZCustomProfiles (Médico de Combate, Caçador, Fuzileiro, Batedor, Operador Furtivo, Armeiro, Operador Tático, Sobrevivencialista, Saqueador, Gerente de Operações), recriadas no formato novo **com os mesmos itens e skills** — agora com itens **equipados/compostos** (o que o RZ não fazia) — e com a estação de hideout de cada uma.

A migração das 10 classes (antes prevista no item 007) é **trazida para cá**; o 007 passa a ser só **coexistência / aposentar o RZCustomProfiles**.

<!-- review: DECISÃO CENTRAL de formato/UX (resolver no /create-technical-spec, validar com o usuário):
como o JSON expressa itens compostos sem o usuário escrever a árvore inteira de mods à mão?
Opções: (a) referência a PRESET de arma do jogo (globals ItemPresets) por id/nome; (b) árvore manual (item + filhos por slot); (c) ambos. Idem rig/mochila com conteúdo e armadura com placas.
Também: (i) como mapear os loadouts "tudo no stash" do RZ para equipado/composto (o que veste vs o que vai no stash vs o que é montado), por classe; (ii) armas nascem com carregador CARREGADO (munição) e/ou bala na câmara, ou vazias? -->

## Comportamento atual

Hoje (item 002) uma classe define só skills/base/descrição; o personagem nasce com stash vazio (base "SPT Zero to hero"), sem equipamento e sem hideout customizado. O mod ainda só tem as classes de exemplo/teste — as 10 classes reais não existem no formato novo (vivem no RZCustomProfiles, que é black-box e limitado a "tudo no stash").

## Comportamento desejado

Uma classe pode declarar, no JSON: itens (stash/equipado/composto) e níveis de estações de hideout. Ao criar o perfil, o personagem nasce **vestindo** o equipamento nos slots certos, com **itens compostos montados** (arma já com mira/carregador; rig já com munição), itens de stash com contagens corretas (stack-aware), e com as **estações de hideout** nos níveis definidos. As **10 classes reais** ficam disponíveis no launcher, cada uma com seus itens (mesmos do RZ, agora equipados/compostos), skills (mesmos budgets) e estação de hideout. Entradas inválidas são puladas com log; o perfil nunca fica quebrado.

## Critérios de aceite

- [ ] Uma classe pode definir itens **equipados**; o personagem nasce com eles nos slots corretos (visível in-game).
- [ ] Uma classe pode definir itens **compostos** (arma+mods+carregador; rig/mochila com conteúdo); aparecem **montados**, não como peças soltas.
- [ ] Uma classe pode definir itens de **stash** com contagem correta (stacks divididos pelo tamanho de pilha — regra stack-aware do RZ).
- [ ] Uma classe pode definir **estações de hideout** iniciais; o hideout do personagem reflete os níveis definidos in-game.
- [ ] As **10 classes reais** existem no formato novo e selecionáveis no launcher; para cada uma, o **conjunto de TPLs + contagens** bate com o loadout do RZ (independentemente de estarem equipados/compostos/no stash), as **skills** batem com os `SkillOverrides` do RZ, e a **estação de hideout** da classe está presente. (O conjunto de itens é portado fielmente; a **disposição** equipado/composto/stash é design novo por classe — o RZ só tinha stash.)
- [ ] Entrada inválida (tpl/slot/estação inválidos, item que não cabe) é **pulada com log claro**; o resto carrega e o perfil fica válido. Vale para USEC e BEAR.

## Corner cases

- [ ] **Seção de itens/hideout ausente:** classe carrega como no 002 (só skills, stash vazio, hideout base).
- [ ] **Tpl desconhecido/inválido:** item pulado + log; demais carregam.
- [ ] **Slot/parent inválido** (item em slot que não o aceita; slot obrigatório faltando): validador de integridade pula/loga; inventário não quebra.
- [ ] **Estação de hideout inválida** (nome inexistente; nível impossível; estação com pré-requisito não atendido): pular/limitar + log (o RZ só usava estações sem pré-requisito).
- [ ] **Overflow de stash** (mais itens do que cabe): logar aviso e colocar o que cabe / pular excedente, sem corromper (lição do RZ).
- [ ] **Contagem > tamanho de pilha:** dividir em entradas (stack-aware).
- [ ] **Preset/composto inexistente** (se referência a preset for adotada): pular + log.
- [ ] **IDs internos da árvore:** o mod gera IDs únicos por instância (itens montados/repetidos não colidem).
- [ ] **Carregador/câmara:** definir se armas equipadas/compostas nascem com **carregador carregado** (munição dentro) e/ou bala na câmara, ou vazias (ver review).
- [ ] **Item não cabe no contêiner (dims):** rig/mochila com mais itens do que a grade comporta → validador pula/loga o excedente (precisa de dims do itemdb).
- [ ] **Slot já ocupado pelo template base:** se a base "Zero to hero" já traz algo num slot que a classe quer equipar, definir substituir vs. conflito (log).
- [ ] **Dois lados (USEC/BEAR):** loadout + hideout aplicam nos dois.

## Fora de escopo

- Outfits/skins/aparência — item 004.
- Multiplicadores de skill — item 005 (o RZ **não** tinha; feature nova).
- Compatibilidade com Skills-Extended — item 006.
- Coexistência / aposentar o RZCustomProfiles (clobber) — item 007 (a migração das 10 classes saiu de lá e veio pra cá).
- i18n / seletor F12 — item 008.
- Outros knobs do RZ **não pedidos** (traders, secure container, starting level/prestige, flags como AllItemsExamined): ficam no **default da base "SPT Zero to hero"** (o RZ deixava esses sem alteração relevante). Fora de escopo salvo decisão futura.

## Referências

- Item 002 (schema + loader): [002-class-schema-loader-01-spec.md](../002-class-schema-loader/002-class-schema-loader-01-spec.md)
- **Dados das 10 classes (reusar):** loadouts/recipes, `anchor-items.json`, modelo de balanceamento de skills, escolhas de hideout (estações sem pré-requisito), regra stack-aware — em [mods/RZCustomProfiles/](../../../RZCustomProfiles/) (`scripts/`, `backlog/`, `memory/sessions.md`).
- Dados de item: [tools/tarkov-itemdb](../../../../tools/tarkov-itemdb/) (tpl/nome/dims/stackMax); presets de arma e estrutura de hideout vêm dos globals/DB do SPT — a confirmar no tech spec.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` |
| 2026-06-07 | Escopo expandido (decisão do usuário): + hideout, + migração das 10 classes reais (itens+skills+hideout) trazida do 007; 007 vira só coexistência/aposentar RZ |
| 2026-06-07 | Revisão `/review-spec` — critério das 10 classes tornado verificável (paridade TPL+contagem+skills+hideout); +3 corner cases (carregador carregado, dims/contêiner, slot já ocupado); marcadas decisões de formato (preset vs árvore) + carregador |

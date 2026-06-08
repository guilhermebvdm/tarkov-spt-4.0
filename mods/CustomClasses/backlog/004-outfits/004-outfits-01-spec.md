# 004 — Outfits por classe

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Hoje cada classe nasce com a aparência padrão herdada do perfil base ("SPT Zero to hero"). Este item permite que cada arquivo de classe defina um **outfit** próprio — a aparência do personagem (cabeça, corpo, mãos, pés) e as roupas/vestuário — de modo que o personagem nasça já com esse visual, para USEC e BEAR. Estende o schema de classe do 002/003 com uma seção de outfit opcional.

## Comportamento atual

- O loader (002) injeta cada classe como uma *edition* clonando o perfil base; a aparência do personagem é a herdada do base.
- O JSON de classe suporta `skills`, `hideout` e `loadout` (003), mas **não** há nada para definir a aparência/roupas — todas as classes parecem iguais visualmente.

## Comportamento desejado

- O JSON de classe ganha uma seção **opcional** de outfit com: identificadores de **aparência** (cabeça, corpo, mãos, pés) e de **roupas** (peças de vestuário).
- Ao criar um perfil de uma classe com outfit definido, o personagem nasce com a aparência e as roupas configuradas, em **USEC e BEAR**.
- Outfit ausente → aparência padrão do base (sem alteração). IDs inválidos → ignorados com aviso e o restante aplica (mesmo padrão "skip-com-aviso" dos itens 002/003).

## Critérios de aceite

- [ ] O JSON de classe aceita uma seção **opcional** de outfit com campos para aparência (cabeça, corpo, mãos, pés) e para roupas.
- [ ] Criar um perfil de uma classe com outfit definido → o personagem nasce com a aparência e roupas configuradas (verificável na tela de personagem/inventário in-game), tanto em **USEC** quanto em **BEAR**.
- [ ] Classe **sem** seção de outfit → personagem nasce com a aparência padrão do perfil base (nenhuma alteração de visual).
- [ ] ID de aparência/roupa **inválido ou desconhecido** → ignorado com aviso no log; os demais campos do outfit ainda aplicam; a classe continua registrada normalmente.
- [ ] Peça de roupa válida só para uma facção (USEC/BEAR) → aplicada no lado compatível; no lado incompatível é ignorada com aviso, **sem quebrar** o personagem desse lado.

## Corner cases

- [ ] Seção de outfit presente porém **vazia** → tratada como "sem outfit" (aparência padrão), sem erro.
- [ ] ID com **tipo trocado** (ex.: id de roupa colocado no campo de cabeça) → ignorado com aviso, não corrompe a aparência do personagem.
- [ ] Outfit define **apenas parte** dos campos (ex.: só a cabeça) → aplica o que foi definido; o resto permanece o padrão do base.
- [ ] Aplicação **independente por lado**: se um campo falhar no USEC, isso não impede a aplicação no BEAR (e vice-versa).
- [ ] Roupa restrita por **gênero/facção** aplicada ao lado errado → não deve aparecer "meio aplicada" nem deixar o personagem em estado inválido.
- [ ] Campo de **aparência** (ex.: cabeça) também restrito por facção/gênero → mesmo tratamento das roupas (ignorar no lado incompatível com aviso).
- [ ] Outfit aplica-se **somente na criação** do perfil (a *edition* é um template). Perfis **já criados** de uma classe **não** mudam de visual se o outfit da classe for editado depois — consistente com o comportamento de skills/itens (002/003).

## Fora de escopo

- [ ] Seleção/preview de outfit no F12 ou no launcher (a seleção de classe é por *edition*; o visual é fixo por classe).
- [ ] i18n de nomes de outfit (item 008).
- [ ] Skins de arma, vozes, dogtag e outras customizações fora de aparência + roupas.

## Decisões pendentes (resolver com o usuário)

<!-- review: [D1] Este item entrega a CAPACIDADE de outfit por classe. Popular as 10 classes reais com outfits temáticos (escolher aparência/roupas de cada uma) é uma decisão à parte — fazer já neste item (como o 003 trouxe os itens das 10) ou deixar como passo seguinte? -->
<!-- review: [D2] Fonte dos IDs de outfit: aparência/roupas NÃO estão no tarkov-itemdb. Como o autor de classe descobre os IDs válidos? Opções: (a) documentar uma lista de referência no mod; (b) um script que lista os outfits disponíveis do DB de customization; (c) deixar o autor extrair manualmente. Definir antes/junto ao tech-spec. -->
<!-- review: [D3] Relação aparência × "suit": em EFT, parte do visual (corpo/pés) vem de itens de vestuário (suits) que precisam estar "possuídos" no perfil, enquanto cabeça/mãos são customization direta. A forma exata (campos do JSON + se precisa registrar o suit como possuído) é detalhe do /create-technical-spec, mas o JSON funcional deve refletir essa separação se necessário. -->

## Referências

- [002 — Schema de classe + loader](../002-class-schema-loader/) (schema/loader, skip-com-aviso por arquivo)
- [003 — Itens + hideout](../003-starting-items/) (mesmo padrão de aplicação independente por lado USEC/BEAR)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado + spec funcional via `/create-spec` |
| 2026-06-07 | `/review-spec` — +2 corner cases (aplica só na criação; aparência também restrita por facção) + 3 decisões marcadas (D1 popular 10 classes, D2 fonte dos IDs, D3 aparência×suit) |

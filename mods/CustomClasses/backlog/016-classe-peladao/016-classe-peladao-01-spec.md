# 016 — Classe "Peladão"

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Adicionar uma **11ª classe** chamada **"Peladão"** — uma classe-piada/desafio: começa igual ao `SPT Zero to Hero` (stash vazio, sem itens), **sem nenhum buff/debuff** de skill, mas com **identidade visual própria** (ícone, descrição engraçada, cor) e uma **skin com o mínimo de roupa possível**.

## Comportamento atual

Não existe a classe "Peladão". As 10 classes atuais têm loadout, skills e/ou multiplicadores temáticos.

## Comportamento desejado

Uma classe "Peladão" registrada como edition, com:
- **Base:** `SPT Zero to Hero` (mesma base das outras classes — stash vazio; a classe controla seus próprios itens).
- **Itens:** nenhum (ou o mínimo) — sem loadout temático. (A definir: começa pelado/sem nada, ou com algum item simbólico.)
- **Skills:** sem `skills` iniciais e **sem `skillMultipliers`** (nenhum buff/debuff).
- **Identidade visual:** `iconFile` próprio (ícone a definir) + `nameColor` (cor a definir).
- **Descrição:** texto **engraçado** (pt + en — i18n do item 008), a definir.
- **Outfit/skin:** roupa com **o mínimo possível** (ex.: cueca/sem colete/sem capacete) — IDs de customization a definir (item 004 cobre o mecanismo de outfit).

> **Conteúdo a definir junto com o usuário:** o ícone (PNG), a descrição engraçada, a cor e a skin "menos roupa" (quais IDs de `Customization`/`Suits`). Este item entra em desenvolvimento quando esses dados forem escolhidos.

## Critérios de aceite

- [ ] A classe **"Peladão"** aparece como edition no launcher (criável).
- [ ] Nasce com a **base `SPT Zero to Hero`** (stash vazio), **sem buff/debuff** de skill.
- [ ] Tem **ícone próprio**, **cor própria** e **descrição engraçada** (pt/en) — visíveis onde a identidade da classe aparece (menu/Skills/etc.).
- [ ] Nasce com a **skin de menos roupa** definida.
- [ ] Não quebra nada nas outras classes (é só mais um JSON de classe).

## Corner cases

- [ ] **Skin "menos roupa":** confirmar quais peças de `Customization`/`Suits` resultam no visual desejado (algumas peças são obrigatórias — ex.: o jogo pode exigir um lower/upper mínimo). Validar in-game.
- [ ] **Sem itens:** garantir que nascer "pelado" (sem arma/colete) não gera estado inválido de perfil.
- [ ] **Sem multiplicadores:** o selo da classe (itens 011/012) ainda aparece (ícone + nome + cor), mesmo sem buff/debuff — já suportado (router expõe identidade independente de multiplicadores).

## Fora de escopo

- Mecanismos novos (reusa schema de classe, outfit e identidade já existentes).
- Balanceamento (a classe é proposital "sem nada").

## Referências

- Schema de classe + gerador: `scripts/build-class-jsons.js`, `scripts/class-recipes.js`.
- Outfit/skin: item 004. Identidade visual (ícone/cor): item 011.
- Base `SPT Zero to Hero`: já usada como `baseEdition` das outras classes.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (pedido do usuário). Conteúdo (skin/ícone/descrição/cor) a definir. |

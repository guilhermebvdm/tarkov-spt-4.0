# 048 — Infra de skill custom (padrão SE) — base 🧪

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-21

## Visão geral

Criar a **fundação reutilizável para "skills custom"** (🧪): habilidades novas, treináveis, que não fazem parte do conjunto de skills ativo do jogo, mas que passam a se comportar como skills de verdade — aparecem na tela de Skills, ganham XP por uma atividade definida, sobem de nível, **persistem no perfil** e o nível delas **dirige um efeito**. Esta entrega prova o mecanismo ponta a ponta com **uma skill de exemplo**; as signatures reais (Adrenalina, Fôlego de Aço, Pack Mule, Mãos Rápidas) vêm no item 049. A diferenciação por classe usa o **mesmo mecanismo de skills/multiplicadores** já existente (item 005): início + multiplicador de XP por classe, com `×0` desabilitando.

## Comportamento atual

- O mod sabe dar **nível inicial** e **multiplicador de XP** a skills que **já existem e funcionam** (camada 🎯, itens 005/047) — inclusive as 6 "gems" reativadas pelo Skills-Extended.
- **Não existe** caminho para tornar funcional uma skill **nova** (um slot de skill morto/sem efeito): hoje ela não aparece de forma útil, não acumula XP próprio, e não há como ligar o nível dela a um efeito customizado.
- Habilidades-assinatura novas (que precisam de um efeito inédito que escala com nível) **não têm onde existir**.

## Comportamento desejado

- O mod consegue **declarar uma skill custom** que passa a se comportar como skill real: **aparece na tela de Skills** (com nome e nível), **acumula XP** a partir de uma atividade definida, **sobe de 0 ao máximo**, e **persiste** no perfil entre raids e sessões.
- O **nível da skill custom dirige um efeito observável** (quanto maior o nível, mais forte o efeito), com os parâmetros do efeito **configuráveis** (F12 — decisão #8).
- A skill custom é **gatilhável por classe** pelo mesmo mecanismo das outras: uma classe que a recebe (nível inicial e/ou multiplicador > 0) a treina; uma classe sem ela (ou com multiplicador `×0` e início 0) **a mantém congelada em 0** — sem XP, sem efeito.
- Entrega **uma skill de exemplo** funcional ponta a ponta (prova do mecanismo) <!-- review: decidir se a skill de prova é uma das signatures REAIS do 049 (ex.: Pack Mule — a mais simples, peso×nível) para não virar trabalho descartável, ou uma skill trivial throwaway. --> + a skill aparece no **viewer do editor** (convenção do 047: skills custom entram no catálogo canônico do editor, não no dump removido).
- **Sem prepatcher / sem efeito injetado por buff efêmero** — segue a arquitetura "tudo-é-skill-real" (decisões #1/#2).

## Critérios de aceite

- [ ] Uma **skill custom de exemplo aparece na tela de Skills** in-game, com nome e nível, como qualquer outra skill.
- [ ] A skill **acumula XP** a partir da atividade definida e **sobe de nível** (0 → acima) ao longo do jogo.
- [ ] O **efeito escala com o nível** da skill (diferença observável entre nível baixo e alto), e seus parâmetros aparecem no **F12** (ajustáveis; runtime ou com nota de restart — decisão #8).
- [ ] **Gating por classe:** uma classe configurada com a skill a treina; uma classe sem ela (ou `×0` + início 0) a mantém **em 0, sem XP e sem efeito** — verificável criando perfis de classes diferentes.
- [ ] A skill custom **aparece no viewer do editor** (catálogo canônico, padrão da seção "Gems (SE)" do 047).
- [ ] **Fika/multiplayer:** o efeito da skill custom de cada player é dirigido pelo **nível do próprio perfil** e aplicado **só ao player local** (filtro MainPlayer — AP-02); não vaza do/para o nível de outro player. XP é per-jogador (mesmo escopo do item 005). *(Verificar no smoke do 052 com 2+ players.)*
- [ ] **Estado entre raids:** o nível da skill custom **persiste no perfil** entre raid1 → exit → raid2, e sobrevive a alt-F4/morte/MIA e restart do server (é dado de perfil, não de raid).

## Corner cases

- [ ] **Classe sem a skill custom (ou `×0`):** a skill fica em 0, não ganha XP e não aplica efeito — sem erro, sem aparecer "fantasma".
- [ ] **Skills-Extended ausente:** o mecanismo depende do pipeline do SE (o mod faz soft-detect, item 006). Sem SE, a skill custom deve **degradar com no-op + aviso claro**, nunca crashar o boot nem o raid.
- [ ] **Perfil criado antes da skill custom existir:** ao carregar, a skill simplesmente começa em 0 / ausente — não corrompe nem quebra o perfil.
- [ ] **Conflito de slot:** se outro mod (ou o próprio SE) já reativa o mesmo slot de skill morto, não pode haver **registro duplo** nem efeito aplicado duas vezes — detectar e ceder/avisar.
- [ ] **XP com a skill desabilitada (`×0`):** nenhum XP é acumulado (congela em 0) — coerente com a decisão #3 (o multiplicador 0 zera o ganho).
- [ ] **Nível no máximo / elite:** o efeito respeita o teto (não estoura além do nível máximo); comportamento no elite (se houver) é definido, não indefinido.
- [ ] **Fim de raid (AP-01):** o efeito da skill custom **para/limpa ao sair do raid** — não vaza para o hideout nem para o próximo raid; hooks de start/stop idempotentes.
- [ ] **Nome na tela de Skills:** a skill custom aparece com **nome legível (localizado)**, não em branco nem como a chave crua do slot revivido.
- [ ] **Level-up no meio do raid:** se a skill sobe de nível durante o raid, o efeito **reflete o novo nível na próxima leitura** (não exige reiniciar o raid).

## Fora de escopo

- As **skills-assinatura reais** das classes (Adrenalina, Fôlego de Aço, Pack Mule, Mãos Rápidas) — item **049** (consome esta infra).
- Habilidades por **patch (🔧)** que não são skills (Médico de Combate, Execução, Couraça, etc.) — itens **050/051**.
- Uso de **prepatcher** ou de um `EBuffId` novo — explicitamente **descartado** (decisão #2; pipeline frágil/ofuscado).
- Escolha de **quais** slots de skill mortos serão usados por cada signature — definido caso a caso no 049.

## Referências

- [048 kickoff](./048-skill-custom-infra-00-kickoff.md)
- [class-levers.md](../../docs/class-levers.md) §1 (decisões #1/#2/#3/#8) · §6.4 (configurabilidade)
- Skills-Extended em `mods/Skills-Extended/modded` — pipeline de referência do efeito-por-nível
- Item 005 (multiplicadores de skill client-side) — mecanismo de gating reusado

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Item criado via `/create-spec` |
| 2026-06-21 | Revisão `/review-spec` — Fika reescrito de N/A frágil → requisito de filtro local (AP-02); +3 corner cases (limpeza de efeito no fim de raid AP-01, nome localizado, level-up no meio do raid); 1 `<!-- review -->` (skill de prova = signature real vs throwaway). |

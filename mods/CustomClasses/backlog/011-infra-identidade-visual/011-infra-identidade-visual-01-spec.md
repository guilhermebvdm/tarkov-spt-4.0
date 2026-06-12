# 011 — Infra de identidade visual da classe

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Base técnica para as features visuais (012 e 013): cada classe passa a ter um **ícone (PNG)** e uma **cor (hex)** próprios, definidos no JSON da classe, expostos ao client e prontos para serem desenhados como um "selo" (ícone + nome da classe colorido) — análogo ao selo de tipo de conta (ex.: Unheard), porém dinâmico por classe. Este item **não** desenha nada na Ut ainda; entrega o **pipeline de dados + assets + componente reutilizável** que o 012/013 consomem.

## Comportamento atual

- A rota `/customclasses/skill-multipliers` devolve `{ className, multipliers }` ao client, mas o `className` só vem **quando a classe tem multiplicadores de skill** (item 010). Classes sem multiplicador não expõem identidade ao client.
- Não há campo de **ícone** nem **cor** por classe no schema (`ClassDefinition`).
- O client (`SkillMultipliers`) conhece `ClassName` mas não tem ícone/cor nem como carregá-los.
- O `/compile-mod` instala no client **apenas** o `.dll`/`.pdb` — **não** copia assets (PNGs) para `BepInEx/plugins/CustomClasses/`.

## Comportamento desejado

- O JSON da classe aceita **`iconFile`** (nome do arquivo PNG, opcional) e **`nameColor`** (cor hex `#RRGGBB`, opcional).
- O server expõe ao client, para a classe do perfil atual, **nome + ícone + cor**, **sempre que a edition for uma classe do mod** (mesmo sem multiplicadores de skill).
- O client consegue **resolver o ícone** (carregar o PNG do disco do plugin) e tem um **componente reutilizável** para montar o "selo" (ícone + nome colorido) — consumido pelo 012/013.
- A pipeline de build **entrega os PNGs** ao client (pasta de ícones no plugin).
- O mod traz **ícones placeholder** para validar o visual de ponta a ponta; o usuário troca por arte real depois, **sem recompilar** (basta trocar o PNG e/ou editar o JSON).

## Critérios de aceite

- [ ] O JSON de uma classe aceita `iconFile` e `nameColor` (ambos opcionais) sem quebrar classes que não os definem.
- [ ] Com o mod rodando, o client obtém **nome + ícone + cor** da classe do perfil logado — **inclusive** para uma classe **sem** `skillMultipliers`.
- [ ] Uma edition **vanilla** (não-classe) não devolve identidade (nada a exibir).
- [ ] Após `/compile-mod`, os PNGs do mod aparecem na pasta de ícones do plugin client.
- [ ] Existe um componente client reutilizável que, dado (nome, ícone, cor), produz o "selo" visual — verificável pelo 012 ao consumi-lo (neste item, basta compilar sem erro e o cache de ícone resolver um PNG existente).
- [ ] `iconFile` apontando para um PNG **inexistente** → degrada para **só o nome** (sem erro/crash).

## Corner cases

- [ ] **Sem `iconFile`:** classe mostra só nome colorido (ícone opcional).
- [ ] **Sem `nameColor`:** usa uma cor default (não quebra).
- [ ] **`nameColor` malformado** (hex inválido): cai na cor default + log, sem crash.
- [ ] **PNG ausente no disco** (nome certo no JSON, arquivo faltando): cache devolve "nada" → só nome.
- [ ] **Classe do mod sem multiplicadores:** ainda assim expõe identidade (nome/ícone/cor) — corrige a limitação atual da rota.
- [ ] **Troca de PNG sem recompilar:** substituir o arquivo na pasta de ícones do plugin e reabrir o jogo reflete o novo ícone (sem rebuild do DLL).
- [ ] **`iconFile` com caracteres de path** (`../`, `/`, `\`): tratar como **nome de arquivo simples** (sanitizar) — não permitir ler fora da pasta de ícones do plugin.
- [ ] **PNG de proporção não-quadrada:** exibir **preservando o aspect ratio** (não distorcer); o componente do selo define um tamanho-alvo e ajusta sem esticar.

## Fora de escopo

- Desenhar o selo no menu ou na tela de Skills — itens 012/013.
- Arte final dos ícones (placeholders aqui; usuário troca depois).
- Suporte a SVG (apenas PNG; SVG só se pré-rasterizado).

## Referências

- Pipeline server→client existente: `modded/Server/SkillMultipliersRouter.cs`, `modded/Server/SkillMultipliersResponse.cs`, `modded/Client/SkillMultipliers.cs`.
- Schema da classe: `modded/Server/ClassDefinition.cs`; doc: `modded/Server/config/classes/_docs/exampleClass.jsonc`.
- Build: `.agents/scripts/compile-mod.sh`.
- Briefing macro do conjunto (011–013): plano aprovado em 2026-06-08.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (briefing aprovado) |
| 2026-06-08 | Spec funcional criada via `/create-spec` |
| 2026-06-08 | Revisão `/review-spec` — +2 corner cases (sanitização de path do `iconFile`; preservar aspect ratio do PNG) |

# 005 — Multiplicadores de skill por classe (client)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Cada classe pode definir **multiplicadores de ganho de XP por skill** (buff/debuff). Um personagem da classe ganha XP de skill mais rápido (fator > 1) ou mais devagar (fator < 1) nas skills configuradas — em raid **e** fora de raid (hideout/menu). O fator também é **exibido na tela de Skills**. É **apenas escala de ganho** — **não** há distribuição dinâmica de pontos (decisão travada desde o planejamento). Item **híbrido**: server guarda/serve os fatores por classe; client (BepInEx) escala o ganho em runtime e mostra na UI.

## Comportamento atual

- As classes definem skills **iniciais** (002/003), mas o **ganho de XP** das skills é o vanilla, igual para todas as classes.
- Não há como uma classe progredir uma skill mais rápido/devagar, nem indicação visual disso.

## Comportamento desejado

- O JSON de classe ganha uma seção **opcional** de multiplicadores: nome da skill → fator (ex.: `1.5` = +50% de ganho, `0.5` = −50%).
- O personagem da classe ganha XP de skill **escalado pelo fator**, em raid e fora de raid.
- Skill sem fator (ou `1`) → ganho **vanilla**.
- **Runtime, não baked:** diferente de itens/outfit (que entram só na **criação** do perfil), os multiplicadores são lidos em runtime pela **classe/edition** do perfil → valem também para personagens **já criados** daquela classe (após reiniciar o server + nova sessão).
- Na **tela de Skills**, cada skill com fator mostra o multiplicador (`+50%` / `−50%`) **na linha da skill E no tooltip** (decisão D3), no mesmo estilo/layout da UI já existente; fator `1`/ausente não polui (omitido).
- **As 10 classes vêm com multiplicadores temáticos** (buff nas skills que combinam com o conceito da classe) — **design novo** (o RZ não tinha multiplicador de XP; o `SKILL_MULTS` dele era peso de *custo*, não de ganho). Tudo **ajustável fácil pelo JSON** da classe.

## Critérios de aceite

- [ ] O JSON de classe aceita uma seção **opcional** de multiplicadores (skill → fator decimal).
- [ ] Skill com fator **> 1** ganha XP proporcionalmente mais rápido; **< 1** mais devagar; **1/ausente** = vanilla (verificável comparando o XP ganho por uma ação repetida).
- [ ] Aplica **em raid e fora de raid** (ex.: skills que evoluem no hideout/menu — Metabolism, HideoutManagement, etc.).
- [ ] O fator é lido **por classe** (a partir da edition/classe do personagem) — classes diferentes aplicam fatores diferentes ao mesmo perfil.
- [ ] A **tela de Skills** exibe o multiplicador por skill (prefixo `+`/`−`), no padrão visual da tela; skills sem fator não aparecem alteradas.
- [ ] Fator inválido / nome de skill desconhecido → **ignorado com aviso**; as demais entradas aplicam; sem crash.
- [ ] Fator `< 1` reduz o ganho mas **nunca** o torna negativo nem remove XP já ganho (clamp ≥ 0).
- [ ] Sem **impacto perceptível de FPS** — o patch de XP não aloca em hot path nem faz I/O por ganho (config lida 1× por sessão).

## Corner cases

- [ ] Nome de skill **desconhecido** no JSON → ignorado com aviso (não quebra a classe).
- [ ] Fator **0 ou negativo** → comportamento definido (0 = trava o ganho? negativo = inválido → ignora). <!-- review: definir 0/negativo -->
- [ ] Classe **sem** seção de multiplicadores → ganho 100% vanilla, **nenhuma** alteração na UI.
- [ ] Personagem de **edition vanilla** (não-classe do mod) → nenhum multiplicador, comportamento vanilla.
- [ ] Skills **novas** (Skills-Extended) → fora deste item (006); multiplicadores são string-keyed e **não** devem quebrar se a skill não existir no momento.
- [ ] **Coop/FIKA** → o multiplicador é por jogador (cada cliente aplica o seu); não afetar o ganho de outros jogadores.
- [ ] Abrir/fechar a tela de Skills repetidamente → o label do multiplicador **não** duplica/empilha.
- [ ] Outro mod que também escala XP de skill (ex.: SkillDistribution) → **não** conflitar de forma destrutiva (idealmente compor; no mínimo, não crashar).
- [ ] **Múltiplas raids na mesma sessão** / fim de raid / morte → a escala aplica de forma consistente, **sem vazar estado** entre raids nem aplicar o multiplicador duas vezes (lifecycle de raid no client).
- [ ] XP de skill de **fonte não-gameplay** (recompensa de quest / pontos instantâneos) → definir se o multiplicador também escala ou só o ganho "natural". <!-- review: [D5] escopo do ganho escalado -->
- [ ] Interação com a **fadiga/retornos decrescentes** de skill do EFT → o multiplicador escala o ganho **efetivo** (após a regra vanilla), não burla a fadiga.
- [ ] Skill já no **nível máximo/elite** → multiplicador sem efeito (não há mais ganho a escalar).

## Fora de escopo

- [ ] **Distribuição dinâmica** de pontos/níveis de skill — explicitamente **NÃO** (só multiplicadores de ganho). O SkillDistribution é referência **conceitual**; não copiar código.
- [ ] Compat com skills novas do **Skills-Extended** (item 006).
- [ ] **i18n** dos labels do multiplicador e seletor F12 (item 008).
- [ ] Multiplicadores variáveis em runtime (são **fixos** por classe).

## Decisões pendentes (resolver no `/review-spec` / tech-spec)

<!-- review: [D1] Como o client descobre a classe do personagem + os fatores: rota do server lida no game start, chaveada pela edition do perfil? (detalhe técnico → tech-spec). -->
<!-- review: [D2] Comportamento de fator 0 e negativo. -->
✅ **D3 resolvido:** exibir **na linha da skill E no tooltip**, formato `+50%`/`−50%` (fator 1 = omitido).
✅ **D4 resolvido:** **popular as 10 neste item**, com multiplicadores **temáticos novos** (o RZ não tem essa variável) — definir por classe conforme o conceito, ajustável pelo JSON.

## Referências

- [002 — Schema de classe + loader](../002-class-schema-loader/) (schema por classe, skip-com-aviso)
- [mods/SkillDistribution](../../../SkillDistribution/) (referência **conceitual** de "skill multipliers" + patch de UI de skill — reimplementar, sem copiar)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado + spec funcional via `/create-spec` |
| 2026-06-07 | `/review-spec` — +5 corner cases (clamp ≥0, multi-raid no-leak, XP não-gameplay, fadiga, skill no máximo) + criérios (runtime/perfis existentes, FPS) + decisões D1-D5 marcadas |

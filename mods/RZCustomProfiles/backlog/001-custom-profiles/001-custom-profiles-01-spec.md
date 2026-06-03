# 001 — Perfis customizados temáticos

**Mod:** RZCustomProfiles
**Status:** Backlog
**Criado:** 2026-05-17

## Visão geral

Criar **10 perfis customizados** selecionáveis no launcher do SPT, cada um representando uma classe temática (médico, caçador, fuzileiro, etc.) com:

1. **Skills iniciais** calibradas por um modelo de custo ponderado (28–32 pontos / classe, ver [planejamento §Modelo de balanceamento](./001-custom-profiles-00-planejamento.md))
2. **1 estação extra de hideout em nível 1** alinhada ao tema da classe (ver [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md))
3. **Loadout inicial (~2M ₽)** com itens depositados no stash (ver [planejamento §Inventário inicial](./001-custom-profiles-00-planejamento.md))

Itens entram no stash em formato **plano** (`{ Tpl, Count }`), sem distinção entre equipped/stash, sem itens aninhados (mag carregada com munição), sem posicionamento em slot — limitação aceita do schema atual do `AdditionalStartingItems`. Traders, nível inicial do personagem e quests permanecem inalterados.

> **Contexto técnico:** RZCustomProfiles é um **mod server-side** (lê os JSONs de `profiles/` ao iniciar o servidor SPT e expõe os templates ao launcher). A entrega desta spec é **apenas conteúdo declarativo (JSONs)**, não código C# — não há ciclo de raid, Harmony patches, Unity lifecycle ou interação in-game a considerar. As skills só são aplicadas no momento da criação do personagem.

## Comportamento atual

- O mod RZCustomProfiles está instalado em [modded/](../../modded/) com apenas `exampleProfile.json` (template de referência) em `profiles/`.
- Nenhum perfil customizado real está registrado — o launcher só oferece os perfis base do SPT (Standard, Left Behind, EOD, etc.).
- Skills no SPT persistem entre raids: para "jogar como classe temática" hoje, o usuário teria que editar o save JSON manualmente após criar o personagem, ou usar dev tools — ambas opções fora do fluxo natural do launcher.

## Comportamento desejado

- 10 arquivos `.json` em `modded/profiles/`, um por classe, cada um com:
  - `Enabled: true`
  - `BaseProfile: 0` (Standard)
  - `Name` em PT-BR conforme tabela de classes
  - `Description` em PT-BR descrevendo o estilo de jogo
  - `SkillOverrides` com os níveis exatos definidos no [planejamento §Modelo de balanceamento](./001-custom-profiles-00-planejamento.md)
  - `AdditionalStartingItems.Enabled: true` com `Items` enumerando **todos** os itens (baseline + tema + primary + 3 backups) como entradas planas `{ Tpl, Count }`. Total alvo ~2M ₽ por classe.
  - `HideoutStartingLevels` com `Stash: 1` (padrão) + **1 estação extra da classe em nível 1** conforme [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md). Gerente de Operações recebe 2 estações como bônus de identidade.
  - `TradersLoyalty` com `Standing: 0.0, SalesSum: 0` para todos os traders (idêntico ao exampleProfile, equivalente a "sem alteração")
  - `ClearEquipment: false`, `ClearStash: false`, `MaxLevel: false`, `MaxSkills: false`, `AllItemsExamined: false`, `StartingLevel: null`, `StartingPrestigeLevel: null`, `SecureContainer: 0` (todos com valores neutros = sem alteração)
- Ao abrir o launcher do SPT após instalar o mod, os 10 novos perfis aparecem como opção de criação de personagem.
- Selecionar um perfil cria um personagem novo com: (a) skills pré-elevadas da classe, (b) estação(ões) temática(s) do hideout em nível 1, (c) loadout completo no stash. Traders, nível e quests permanecem como no Standard base.

**Decisão de schema:** todos os 10 JSONs replicam o schema completo de `exampleProfile.json` (zeros explícitos em `TradersLoyalty`, `HideoutStartingLevels` exceto `Stash: 1`, todas as skills em `SkillOverrides` listadas com valor 0 quando não atribuídas). Como `BaseProfile: 0` (Standard) já começa com tudo em zero no vanilla EFT, os zeros explícitos são a **identidade do Standard** — não causam downgrade. Caso futuro algum perfil mude `BaseProfile` para Unheard/EOD (que dão hideout/traders adiantados), esta decisão precisa ser revisitada.

## Critérios de aceite

- [ ] **Os 10 arquivos existem** em `modded/profiles/` com nomes em **camelCase sem acentos** (seguindo o estilo do `exampleProfile.json`): `medicoDeCombate.json`, `cacador.json`, `fuzileiro.json`, `batedor.json`, `operadorNoturno.json`, `armeiro.json`, `operadorTatico.json`, `sobrevivencialista.json`, `saqueador.json`, `gerenteDeOperacoes.json`. Acentos ficam apenas dentro do JSON (campo `Name`), nunca no nome do arquivo (evita problemas cross-platform).
- [ ] **Todos os 10 perfis aparecem no launcher** como opções de criação de personagem com `Name` e `Description` em PT-BR. **Verificação:** abrir SPT.Launcher.exe, ir para criação de personagem, conferir que a lista contém os 10 `Name`s definidos no planejamento (em qualquer ordem) com a `Description` correspondente legível e não truncada.
- [ ] **Cada perfil cria um personagem com as skills pré-elevadas exatamente** nos valores definidos no planejamento. **Verificação:** após criar personagem da classe, abrir Character → Skills (sem precisar entrar em raid) e conferir cada skill listada nos overrides contra a tabela do planejamento. Skills não listadas devem estar em 0.
- [ ] **Trader loyalty inalterado:** todos os traders aparecem em LL1 com Standing 0.0 e SalesSum 0 em qualquer classe. **Verificação:** abrir tela Traders após criar personagem; comparar com Standard.
- [ ] **Hideout temático aplicado:** cada classe começa com **a estação temática definida no planejamento em nível 1** (além de `Stash: 1`). Gerente de Operações começa com 2 estações. **Verificação:** abrir Hideout após criar personagem; conferir contra a tabela §Hideout inicial do planejamento.
- [ ] **Loadout temático no stash:** cada classe começa com o conjunto completo de itens (baseline + tema + primary + 3 backups, total ~2M ₽) **depositado no stash inicial**, em formato plano (sem itens equipados, sem mags carregadas, sem mods montados). **Verificação:** abrir stash após criar personagem; conferir quantidades e tipos contra a tabela §Inventário inicial do planejamento.
- [ ] **O custo ponderado de cada perfil** está no intervalo `[28, 32]` conforme tabela de referência rápida do planejamento — validado por planilha ou script de checagem (não in-game).
- [ ] **Nenhum perfil ultrapassa 6 skills** com nível > 0 nos overrides e **nenhuma skill ultrapassa nível 10** — limite de design do planejamento (o mod aceita até 51, mas estamos auto-restringindo).
- [ ] **Os 10 arquivos são UTF-8 sem BOM** e os caracteres acentuados em `Name`/`Description` (Médico, Caçador, Operações) renderizam corretamente no launcher. **Verificação:** abrir cada `.json` num editor que mostre encoding e conferir; abrir launcher e confirmar que nenhum nome aparece com `Ã©` ou `?` no lugar dos acentos.

## Corner cases

- [ ] **Skill já presente no base profile com nível ≠ 0:** se o base profile Standard começa com alguma skill em nível > 0 (ex: alguns base profiles dão pequenos bônus iniciais), o `SkillOverrides` substitui ou soma? Validar comportamento do RZCustomProfiles e documentar a semântica que estamos assumindo.
- [ ] **Nome de skill inválido no JSON:** se um typo em `SkillOverrides` (ex: `Firstaid` em vez de `FirstAid`) for introduzido, o mod silenciosamente ignora ou loga erro? Confirmar que cada arquivo passa por validação ou que erro é visível.
- [ ] **Usuário desabilita um perfil (`Enabled: false`):** garantir que perfis desabilitados não aparecem no launcher e que personagens já criados continuam funcionando normalmente.
- [ ] **Atualização do mod RZCustomProfiles upstream:** se o autor mudar o schema do JSON (ex: renomear `SkillOverrides` para `Skills`), nossos 10 arquivos quebram silenciosamente. Documentar versão do mod testada (1.1.0 / SPT 4.0.13) e processo de re-validação.
- [ ] **Skill `Memory` no Saqueador:** Memory acelera ganho de XP de outras skills. Iniciar com Memory 8 pode permitir farm de skills aceleradamente — confirmar que o ganho é apenas multiplicador (não retroativo) e que o efeito é o esperado.
- [ ] **Personagem com prestige:** se o usuário usar prestige em cima de um custom profile, as skills ressetam? O mod tem campo `StartingPrestigeLevel` — interação com `SkillOverrides` em prestige precisa ser validada (provavelmente sem efeito porque prestige reseta skills, mas confirmar).
- [ ] **Hot-reload vs. restart do servidor:** se o usuário adicionar ou editar um JSON de perfil com o servidor SPT já rodando, as mudanças aparecem imediatamente no launcher ou exigem restart? Documentar o fluxo esperado para o usuário não criar personagem com versão estale do JSON.
- [ ] **Múltiplos personagens com o mesmo template:** se o usuário criar dois personagens "Médico de Combate" sequencialmente (deletando o primeiro), o segundo recebe exatamente as mesmas skills iniciais? Garantir que a leitura do JSON é determinística e que não há estado persistente entre criações.
- [ ] **Colisão de `Name` com perfil base:** se um `Name` do JSON colide com um perfil base do SPT (ex: alguém renomeia para "Standard"), o launcher mostra dois itens iguais ou um sobrescreve o outro? Os 10 nomes propostos são únicos, mas vale validar o comportamento defensivo.
- [ ] **JSON malformado:** se um dos 10 arquivos tiver JSON inválido (vírgula final, chave duplicada), o mod pula o arquivo silenciosamente e expõe os outros 9, ou aborta o carregamento de todos? Documentar e, se silencioso, garantir log no console.
- [ ] **Personagem deletado e recriado do mesmo template:** se o usuário deleta um personagem "Médico de Combate" e recria, o novo recebe as mesmas skills iniciais limpas (sem resíduo do anterior)? Validar isolamento entre criações.
- [ ] **Description longa truncada no launcher:** descrições mais longas (3-4 frases) podem ser cortadas pelo widget do launcher. Estabelecer limite prático (ex: ≤ 200 caracteres) e testar visualmente as 10 descrições antes de finalizar.
- [ ] **Comentários no JSON (`//`):** o `exampleProfile.json` usa comentários estilo JSONC. Confirmar que o parser do mod aceita JSONC ou se exige JSON estrito — nossos 10 arquivos podem ter ou não comentários documentando cada bloco; decisão depende dessa validação.
- [ ] **Stash não comporta o loadout inteiro:** o stash inicial de Standard tem 10×28 slots (280 slots no nível 1). 1 primary + 3 backups com armas, mochilas, coletes e meds podem exceder essa capacidade — itens "transbordando" podem ser descartados pelo SPT ou bloquear a criação. Validar empiricamente; se acontecer, reduzir loadout (ex: 1 backup em vez de 3) ou subir `HideoutStartingLevels.Stash`.
- [ ] **TPL inválido em `AdditionalStartingItems.Items`:** se algum `Tpl` no JSON estiver errado (typo, item removido do EFT 0.16.x), o mod silenciosamente pula o item ou aborta o carregamento do perfil? Documentar e, se silencioso, garantir log no console + processo de re-validação a cada update do EFT.
- [ ] **Estação de hideout com dependências:** algumas estações exigem pré-requisitos (ex: `ShootingRange` exige `RestSpace` nível 1+, `IntelligenceCenter` exige `Heating` nível 1+). Pré-setar `ShootingRange: 1` sem o requisito pode quebrar a UI do hideout ou bloquear upgrades. Validar quais estações temáticas requerem pré-requisitos e ajustar (pré-setar também os requisitos OU trocar a estação).

## Fora de escopo

- **Itens equipados, aninhados ou posicionados em slot** — limitação aceita do schema atual do `AdditionalStartingItems`. Loadouts entram **todos no stash em formato plano** (sem armadura vestida, sem mag carregada com munição, sem mira montada em rifle, sem mochila nas costas). O jogador monta o loadout manualmente antes da primeira raid. Suporte a equipped/nested/slot fica para um backlog futuro condicionado a investigação do schema real do mod.
- **Alteração de trader loyalty inicial** — todos os perfis começam com Standard padrão. Personalização adiada (decisão revisitável — opções discutidas e arquivadas).
- **Localização (i18n)** — Names/Descriptions só em PT-BR nesta versão.
- **Validação automatizada de custo ponderado** — script de checagem é desejável mas opcional para esta entrega.
- **Validação automatizada do total ₽ por loadout** — checar que cada classe está em ~2M ₽ é desejável mas opcional; conferência manual contra a tabela do planejamento basta.

## Referências

- [001-custom-profiles-00-planejamento.md](./001-custom-profiles-00-planejamento.md) — modelo de balanceamento ponderado, tabela de multiplicadores e composição completa das 10 classes.
- [../../modded/profiles/exampleProfile.json](../../modded/profiles/exampleProfile.json) — template de referência do mod.
- [../../README.md](../../README.md) — documentação do RZCustomProfiles upstream (v1.1.0 / SPT 4.0.13).
- [../../assets/](../../assets/) — screenshots do personagem lvl 43 usado como referência de balanceamento.

## Histórico

| Data | Evento |
|---|---|
| 2026-05-17 | Item criado via `/create-spec` (planejamento pré-existente em `00-planejamento.md`) |
| 2026-05-17 | Revisão `/review-spec` — adicionado contexto server-side, comportamento atual corrigido, campos JSON neutros explicitados (+1 `<!-- review: -->`), recipes de verificação adicionadas a 2 critérios, 4 corner cases novos (hot-reload, múltiplos personagens, colisão de Name, JSON malformado) |
| 2026-05-17 | Revisão `/review-spec` (2ª passada) — escopo do `<!-- review: -->` de campos neutros ampliado (downgrade silencioso de Hideout/Loyalty), critérios 2/6 reescritos para verificabilidade, +2 critérios (encoding UTF-8, esclarecimento do cap 10 como design), +3 corner cases (deletar/recriar personagem, truncamento de Description, suporte a JSONC/comentários), +1 `<!-- review: -->` (convenção de nome de arquivo) |
| 2026-05-17 | Decisões aplicadas: (1) nome de arquivo em camelCase sem acentos (alinhado com `exampleProfile.json`); (2) schema completo com zeros explícitos (= identidade do Standard, sem risco de downgrade). Zerados os 2 `<!-- review: -->` pendentes. |
| 2026-05-17 | Escopo expandido: incluídos **HideoutStartingLevels temáticos** (estação por classe conforme planejamento) e **loadouts iniciais** (Opção 1 simplificada — todos os itens no stash em formato plano `{Tpl, Count}`, sem equipped/nested/slot). Traders **continuam fora de escopo** (decisão arquivada). +3 critérios, +3 corner cases (stash overflow, TPL inválido, dependências de hideout). |

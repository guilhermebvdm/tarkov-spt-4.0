# 047 — Roster 11→6 (aplicar matriz)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-21

## Visão geral

Substituir o roster antigo (10 classes balanceadas + Peladão) pelas **6 classes do redesign** — Médico, Fuzileiro, Caçador, Fantasma, Saqueador, Tanque — mantendo o Peladão (isento). Cada classe recebe a **matriz recalibrada** (skills iniciais + multiplicadores de XP), o **loadout inicial** (gear) e o **hideout** (1 estação pré-construída + 1 com −50% de tempo). Esta entrega cobre **apenas a camada de skills (🎯)** — as habilidades-assinatura por patch (🔧) e as skills custom (🧪) ficam para os itens 048–051. É o passo que destrava a validação visual cedo, porque a camada de skills funciona sozinha.

## Comportamento atual

- A criação de perfil oferece **10 classes balanceadas** (Médico de Combate, Sobrevivencialista, Armeiro, Gerente de Operações, Fuzileiro, Operador Tático, Batedor, Operador Furtivo, Caçador, Saqueador) **+ Peladão**.
- Cada classe carrega a matriz **pré-redesign** (rodadas de balance 040–045): skills iniciais e multiplicadores de XP daquela fase.
- O editor web lista essas 11 classes; o custo de cada uma cai na faixa 28–32 do modelo antigo.

## Comportamento desejado

- A criação de perfil passa a oferecer **6 classes** — Médico, Fuzileiro, Caçador, Fantasma, Saqueador, Tanque — **+ Peladão**. As 6 classes aposentadas (Armeiro, Batedor, Gerente de Operações, Operador Furtivo, Operador Tático, Sobrevivencialista) **não aparecem mais**.
- Cada uma das 6 nasce com as **skills iniciais e níveis exatos** e os **multiplicadores de XP** da matriz recalibrada (fonte: [`scripts/class-matrix.mjs`](../../scripts/class-matrix.mjs), espelhada em [class-levers.md](../../docs/class-levers.md) §4).
- Cada classe nasce com um **loadout** (equipamento + stash inicial) e **1 estação de hideout pré-construída**, e tem **1 estação com −50%** de tempo de construção ([class-levers.md](../../docs/class-levers.md) §5).
- Identidade observável **topo ~+6 / base ~+4**: Médico/Fuzileiro/Caçador/Fantasma sentem progressão forte; Saqueador/Tanque um pouco menor (compensados depois pelas signatures, fora desta entrega).

## Critérios de aceite

- [ ] Criar um perfil novo oferece **exatamente 7** opções de classe do mod (6 redesenhadas + Peladão); nenhuma das 6 aposentadas aparece.
- [ ] Cada uma das 6 classes, ao criar perfil, concede as **skills iniciais e níveis** idênticos aos de `class-matrix.mjs` (conferível na tela de Skills).
- [ ] Os **multiplicadores de XP** de cada classe batem com a matriz em sinal e valor (conferível na UI/tooltip de skill — buff `>1`, debuff `<1`).
- [ ] Cada classe nasce com **algum loadout** (gear) e com as **estações de hideout** por classe (1 pré-construída + 1 a **−50%**), conforme [class-levers.md](../../docs/class-levers.md) §5 — atribuição pinada: Caçador = Shooting Range (inicial) + Intelligence Center (−50%); Tanque = Rest Station (inicial) + Kitchen (−50%); demais = estação única (inicial + −50%).
- [ ] O editor web lista as 6 + Peladão **sem diagnostics de erro**; o **custo** de cada uma das 6 fica em **[28, 32]** (Peladão isento).
- [ ] **Fika/multiplayer:** `N/A` justificado — o mecanismo de multiplicador de XP (item 005, já em produção) aplica o fator **só aos eventos de XP do jogador local**, lendo a classe do **próprio perfil**; a classe de um player remoto não altera as skills/XP do local. O 047 só troca os **valores** desse mecanismo, sem mudar o escopo per-jogador. (Verificação multiplayer incidental movida para o smoke do item **052** — confirmar com 2+ players que a classe/mults de um não afetam as skills/XP do outro.)
- [ ] **Estado entre raids:** skills e multiplicadores persistem no perfil entre raids (são do perfil, não da raid). A matriz nova vale **apenas para perfis criados após a troca**; perfis pré-existentes não são reescritos.

## Corner cases

- [ ] **Perfil já criado com uma classe que será removida** (ex.: um perfil "Armeiro"): a classe do perfil é guardada como **texto**. **Decisão (2026-06-21): deletar as 6** (roster limpo — sem clutter no editor/criação). **Requisito de segurança:** um perfil existente de classe deletada deve **continuar carregando** (fallback para uma edição neutra), **nunca crashar** — a spec técnica verifica o que o SPT faz com edição inexistente e adiciona remap/fallback se necessário.
- [ ] **Perfil já criado com uma classe mantida** (ex.: "Caçador" com a matriz antiga): a matriz nova **não** reescreve skills já concedidas — só vale para perfis novos. Confirmar que aplicar o redesign **não corrompe** nem altera retroativamente esse perfil.
- [ ] **Saqueador com multiplicadores acima do teto** (Lockpicking/Strength ×3 > ×2.0): é **intencional** (ressalva de viabilidade peso-baixo) — o editor/checker pode avisar, mas **não** pode tratar como erro bloqueante.
- [ ] **Cobertura de categorias incompleta** (ex.: Médico sem skills iniciais em todas as 4 categorias): gera **aviso não-bloqueante**, não impede registrar a classe.
- [ ] **Saqueador + Círculo de Cultistas** (skill ShadowConnections): o efeito do Círculo é instantâneo desde o nível 1 (bug conhecido no servidor). Decidir **aceitar** o comportamento ou **corrigir** antes de ativar a classe (registrar a decisão).
- [ ] **Edição concorrente com o editor web**: o install é a fonte de verdade e há uma sessão paralela editando classes. Aplicar a matriz pelo repo **não pode clobberar** edições não sincronizadas — exige sincronização/guard antes de gravar.
- [ ] **Peladão permanece isento** (`noBaseline`, sem skills/multiplicadores): continua aparecendo na criação de perfil e o editor/checker **não** o trata como erro nem o inclui na verificação de custo [28, 32].
- [ ] **Stash com itens compostos** (preset/mods/munição/conteúdo): ao autorar o gear das classes (sobretudo as novas), os itens compostos do stash devem **nascer montados** no perfil novo (não só tpl+count) — ver pendência de validação **P-7.3** da memória do mod (`feedback_spt_validation`: validar in-game, não só write+hash).

## Fora de escopo

- Habilidades-assinatura por **patch (🔧)** e **skills custom (🧪)** — itens 048–051.
- **Gear definitivo/curado** das classes (esta entrega pode usar gear inicial via importação de profile/placeholder; refino visual é posterior).
- **Correção** do bug do Círculo de Cultistas (decisão de aceitar-vs-corrigir é parte do escopo; o fix em si, se escolhido, é trabalho à parte).
- Exposição de parâmetros no F12 — não há lever 🔧/🧪 nesta entrega (só camada de skills, server-side).

## Referências

- [047 kickoff](./047-roster-6-classes-00-kickoff.md)
- [class-levers.md](../../docs/class-levers.md) §4 (matriz) / §5 (loadout+hideout) · [class-overview.md](../../docs/class-overview.md) (resumo por classe)
- [scripts/class-matrix.mjs](../../scripts/class-matrix.mjs) — matriz fonte (cross-check ✅) · [scripts/extract-from-profile.mjs](../../scripts/extract-from-profile.mjs) — gear (item 046)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Item criado via `/create-spec` |
| 2026-06-21 | Revisão `/review-spec` — 3 gaps + 2 corner cases corrigidos; 3 trechos marcados `<!-- review -->` (decisão deletar-vs-desabilitar, atribuição de estações por classe, validação Fika do item 005) |
| 2026-06-21 | 3 markers resolvidos: (1) **deletar** as 6 + requisito de fallback p/ perfil órfão; (2) estações pinadas (Caçador SR+IC, Tanque Rest+Kitchen); (3) validação Fika → smoke do 052 |

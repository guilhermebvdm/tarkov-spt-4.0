# Handoff — CustomClasses (perks 072 + bugfixes de movimento/stamina)

> **Data:** 2026-07-15<br>
> **Autor da sessão:** Guilherme (+ agente)<br>
> **Mod:** `mods/CustomClasses/` · versão atual **v0.2.4**<br>
> **Branch:** `main` · **26 commits locais à frente do remote — NADA foi pushado** (push exige aprovação humana)<br>

---

## ⚠️ Próxima ação #1 — VALIDAR IN-GAME (nada desta sessão foi testado no jogo)

Toda a entrega desta sessão está **implementada, buildada (0 warnings), deployada em `D:\SPT`, revisada adversarialmente — mas NÃO validada in-game** (um agente não joga). Ligar o **`Perk Diagnostics` no F12** (seção `0 · General`) e testar, de preferência em **cliente Fika** (não só solo — o host mascara bugs client-side). Roteiros prontos:

1. **Bug de STAMINA do Tank (v0.2.4)** — entrar de Tank (Strength baixa) carregando **~48–55 kg** e ANDAR. A stamina **não deve** drenar. A linha de diag **"Walk overweight limit (kg)"** deve mostrar **~58.5** (com Pack Mule +30%), não ~45. Se drenar com peso abaixo do limite mostrado → o fix não pegou (ver "Riscos").
2. **Bug de VELOCIDADE do Tank (v0.2.2)** — andar (WASD) como Tank; a velocidade deve ficar **constante** (antes decaía a cada movimento até quase parar).
3. **Perks 072** (v0.2.0/0.2.1) — roteiro no relatório `.handoffs/report-2026-07-13-customclasses-perks-pendentes.md`. O teste crítico: **cirurgia como Médico + apertar W** (Mobile Surgery — o único risco é um lock de Animator não inspecionável em código).
4. **Coop de som (B14/B20, sessão anterior)** — roteiro em `mods/CustomClasses/docs/coop-sound-test-plan.md`.

⚠️ **O launcher TRL reverte a DLL local no sync** (memória `feedback_server_launcher_sync_builds`). Se um teste "não pegar", conferir se a build certa (v0.2.4) está em `D:\SPT\BepInEx\plugins\CustomClasses\` — forense: `python -c "print(b'diag/peer' in open(r'D:/SPT/BepInEx/plugins/CustomClasses/CustomClasses-Client.dll','rb').read())"`. Para o parque coop, a build precisa ser distribuída idêntica a todos (ver decisão de config idêntica, abaixo).

## Próxima ação #2 — decidir o push

26 commits em `main` sem push. Quando o usuário aprovar, `git push`. **Commits são cirúrgicos** — só `mods/CustomClasses/**` e `references/graphs/mods/CustomClasses/**`. A working tree tem trabalho **de outra sessão** em `mods/TRL-ItemsManagement/**` (NÃO commitar, NÃO tocar).

---

## O que foi entregue nesta sessão

Detalhes nos commits (não repito aqui). Resumo:

| Entrega | Commits | Versão |
|---|---|---|
| **Perks 072** — Calm Sights (Caçador), Rapid Care + Swift Surgeon + **Mobile Surgery** (Médico) | `d49a5d27`, `d039a7bb` | 0.2.0→0.2.1 |
| **Fix velocidade do Tank** decaindo (Heavy Frame) | `05fafd11` | 0.2.2 |
| **Fix stamina do Tank** drenando ao andar (Pack Mule/timing) | `f35b2768` | 0.2.4 |
| Doc dos levers inertes de Rooted/Execution + backlog 074 | `1aaf1fea` | 0.2.3 |
| Backlog 073 (rename perk→buff) | `9e87c8ef` | — |

Os dois bugfixes de movimento **passaram por code-review adversarial limpo** (0 bloqueadores). Os perks 072 tiveram 6 achados, todos aplicados.

## Backlog aberto (prioridade sugerida)

Todos em `mods/CustomClasses/backlog/mod-backlog.md`. Ordem que o usuário definiu (067→071) + os novos:

- **071** — remover ruído de "weapon mastery" da aba CLASS (texto vanilla que sobra; `SkillsClassTabPatch` só faz `SetActive`, não esconde os nativos). **Independente e provavelmente o mais rápido de sentir.**
- **067** — editar cor das classes pelo F12 (⚠️ criar seção `8 · Naked` é BREAKING: 4 props de Vanilla Skill Fixes resetam). Desenho já mapeado no item (color picker nativo do ConfigurationManager + resolver no `Identity.NameColor`).
- **068** — texto de mérito do Peladão na aba CLASS + tooltip (⚠️ a `description` do `.jsonc` **não chega ao client hoje** — precisa expor numa rota primeiro).
- **069** — review completo de bilinguismo EN/PT.
- **070** — `/review-mod-properties` + tooltips EN/PT em todas as props (fazer por ÚLTIMO — depois do F12 reorganizado).
- **073** — rename perk/drawback → buff/debuff (só texto VISÍVEL nas 2 línguas; **NÃO** renomear os ~348 identificadores de código). Casa com 069/070.
- **074** — Rooted (−15% ADS) e Execution (+10% melee) estão **inertes** pelos levers errados (achado do code-review 050.1). Rooted precisa do `AimMovementSpeed`/`StateSpeedLimit` de mira; Execution precisa levantar o cap do `SpeedLimiter` (`GClass2175` obfuscado — frágil). Ambos já estavam quebrados antes (dentro do decaimento), não é regressão.

Também aberto de sessões anteriores: **B12** ✅ fechado; **B13/B19** (board `balance-review-2026-07-05.md`); épico 038 (workspace 3 painéis, F2/F3 pendentes); banco de ideias de perks novos em `mods/CustomClasses/docs/perk-ideas.md` (nada decidido).

---

## Contexto técnico que a próxima sessão PRECISA saber

1. **⚠️ Decompile do repo tem 102 namespaces VAZIOS** (`EFT.HealthSystem`, `EFT.Animations`, `EFT.InventoryLogic`, `EFT.CameraControl`). Memória `reference_eft_decompile_incomplete`. **MAS** — o repo + grafos cobrem a **grande maioria** dos tipos (Player, GameWorld, MovementContext, BasePhysicalClass, SkillManager, etc. estão COMPLETOS). Só cair no `ilspycmd -t <FQN>` na DLL real quando o tipo estiver num dos namespaces vazios — verificar antes com `grep -rl "class <Tipo>" references/eft-decompiled/`. **Não descompilar por hábito** (erro corrigido nesta sessão).
2. **Anti-pattern de timing** (memória `reference_spt_init_before_mainplayer`): patch gateado em `MainPlayer` NÃO aplica durante `Player.Init`/`Physical.Init` (MainPlayer ainda null). Valores que o EFT **cacheia** no Init nascem vanilla → reforçar no Postfix de `GameWorld.OnGameStarted` (foi a causa do bug de stamina).
3. **Anti-pattern de campo com feedback** (foi a causa do bug de velocidade): não multiplicar valor em campo que o EFT relê+regrava por frame (`CharacterMovementSpeed`, `SprintSpeed_1`) — compõe geometricamente. Usar getters STATELESS (`MaxSpeed`, `SprintingSpeed`). Documentado no header de `ClassMovementPatches.cs`.
4. **Coop = config idêntica** (decisão do usuário, board `balance-review-2026-07-05.md`): perks de som resolvem a classe do EMISSOR; o VALOR vem do F12 de quem faz a conta. Sem sync — a config é distribuída idêntica a todos. Se um dia virar config por jogador, os valores teriam que vir do server.
5. **Gate de versão** no `/compile-mod`: toda build precisa evoluir o semver (Plugin.cs BepInPlugin + 2 csproj + CustomClassesMetadata.cs — 4 fontes em sincronia).
6. **Deploy**: `bash .agents/scripts/compile-mod.sh CustomClasses` builda os dois (client+server) e instala em `D:\SPT`.

## Riscos conhecidos deixados no código

- **Stamina fix**: se a rota `/customclasses/skill-multipliers` estiver offline **e** o cache virgem no raid-start, o recálculo roda sem o piso → Tank mantém o dreno aquela raid (🟢, não-regressivo, auto-cura no próximo fetch — o review aceitou).
- **Mobile Surgery**: só o C# foi verificado; um lock full-body no Animator (Mecanim) não é inspecionável — validar no jogo.
- **Rooted/Execution**: inertes (item 074).
- **Silent Looter** (Saqueador): suspeita de placebo — só vale contra o SAIN, não cobre abrir container/porta (item C8 do `coop-sound-test-plan.md`).

## Suggested skills para a próxima sessão

- `/code-review` — após qualquer implementação (é gate formal antes de release, memória `feedback_code_review_before_release`).
- `/compile-mod CustomClasses` — build + deploy.
- `/update-mod-graph CustomClasses` — regenerar grafo após mudança de código (commitar junto).
- `/review-mod-properties CustomClasses` — quando chegar no item 070.
- `/update-memory` — ao fim da sessão.
- `spt-mod-best-practices`, `csharp-mod-best-practices`, `graph-code-navigation` — durante o trabalho.

## Referências (não duplicar conteúdo)

- Backlog: `mods/CustomClasses/backlog/mod-backlog.md` · Board: `.../backlog/balance-review-2026-07-05.md`
- Memória do mod: `mods/CustomClasses/memory/sessions.md` (topo = snapshot)
- Relatório da sessão autônoma dos perks: `.handoffs/report-2026-07-13-customclasses-perks-pendentes.md`
- Roteiros de teste: `mods/CustomClasses/docs/coop-sound-test-plan.md`
- Ideias de perks: `mods/CustomClasses/docs/perk-ideas.md`
- Memórias globais novas desta sessão: `reference_eft_decompile_incomplete`, `reference_spt_init_before_mainplayer`, `reference_fika_peer_effects_client_side`

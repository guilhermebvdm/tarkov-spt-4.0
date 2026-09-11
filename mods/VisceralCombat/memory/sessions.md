# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.9.13 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Item `002` (drop de arma/capacete/óculos) e item `003` (Boss/escolta vivos nunca desmembram a perna) implementados e compilados, aguardando validação in-game/coop. Item `004` (reformulação completa da chance de desmembramento) implementado: `KillPatch.cs`/`LimbKillPatch.cs` agora decidem chance por **parte do corpo** (braço/perna/cabeça-arranca/cabeça-estourar) via 3 mecanismos com precedência — (C) exceção por munição individual, (A) soma dinâmica de momento pra munição multi-projétil (calibre 12/20, parte do 23x75), (B) tabela por calibre — em vez do valor único por calibre de antes. Cabeça ganhou um segundo efeito, "estourar" (`BurstHead`, novo — cabeça real permanece, só sobrepõe `Head_1`/`Head_2`; `Head_3` continua sendo "arranca"/sem cabeça). `VD_Calibers.json` reescrito com o schema novo (29 calibres da planilha do usuário + exceções Barrikada/Zvezda do 23x75 + thresholds placeholder do mecanismo A). Planilha de referência em `mods/VisceralCombat/docs/municoes-tabela-completa.xlsx`.
- **Pendências:** 🔴 1 · 🟡 4 · 🟠 1 (ver abaixo).

## Pendências / próximos passos conhecidos
- [P-7.1] (aberta 2026-09-09) 🔴 Item 002 não foi validado in-game nem em coop. Teste **decisivo**: client morre com arma em mãos enquanto host observa — decide se o gate `FikaBackendUtils.IsServer` no host aplicado sobre o `InventoryController` de um `ObservedPlayer` remoto replica corretamente via canal nativo de inventário do EFT/Fika. Ver `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md` §7/§8. Se falhar, o item volta para `/create-technical-spec`.
- [P-7.2] (aberta 2026-09-09) 🟡 Validação solo básica do item 002 (bot com arma/faca em mãos; desmembramento de cabeça na morte e pós-morte; hit de cabeça sem desmembrar não duplica) ainda não feita.
- [P-7.3] (aberta 2026-09-09) 🟡 Débito técnico pré-existente descoberto durante a auditoria Fika do item 002: os pacotes próprios do mod (`DismembermentPacket`, `LivingDismembermentPacket`, `RagdollSyncPacket`) não seguem `docs/technical/fika-packet-desync-prevention-plan.md` — serializam com `Put`/`Get*` crus, sem envelope de comprimento, sem `TryGet*`, sem flag `Valid`. O mod nunca esteve no inventário auditado do guia (auditoria de 2026-07-26 cobriu 6 mods, VisceralCombat não incluído — guia desatualizado). Candidato a item de backlog de auditoria/fix separado (categoria AP-11).
- [P-8.1] (aberta 2026-09-10) 🟡 Item 003 (Boss/escolta vivo não desmembra a perna) não foi validado in-game — precisa de raid com boss pra confirmar (Killa/Reshala/etc. + um seguidor), além do teste de regressão (Scav comum continua desmembrando normal).
- [P-8.2] (aberta 2026-09-10) 🟠 `CR-01-01` do item 004 — `KillPatch.Postfix` e `LimbKillPatch.ProcessLimbKill` podem processar o mesmo pellet independentemente em corpos já mortos; se confirmado, o acumulador de momento do mecanismo A conta em dobro (chance mais alta que a curva calibrada prevê, não é crash). Precisa de validação em jogo ou de uma guarda de idempotência no acumulador. Ver `mods/VisceralCombat/backlog/004-reformular-chance-desmembramento-calibre/004-reformular-chance-desmembramento-calibre-04-code-review-01.md`.
- [P-8.3] (aberta 2026-09-10) 🟡 Item 004: thresholds do mecanismo A (`momentum_min_ns`/`momentum_max_ns`/`head_burst_multiplier` em `VD_Calibers.json`) são placeholders (3.0/15.0/1.5), não calibrados. Ajustar com teste em jogo (calibre 12 buckshot no mesmo membro).

---

## 2026-09-10 12:51 (GMT-3) — Sessão 2026-09-10 (Sessão 8b): Reformulação Completa da Chance de Desmembramento (item 004) — 3 Mecanismos + Efeito "Cabeça Estourada"

**Tema central:** Ciclo SDD completo do item 004 — reformular a chance de desmembramento pra ser por parte do corpo (braço/perna/cabeça-arranca/cabeça-estourar), calibrada por calibre com base na planilha do usuário, mais um novo efeito visual de cabeça "estourada" distinto do "arranca" existente.

**Decisões-chave:**
- **3 mecanismos com precedência C→A→B→0** (`KillPatch.ResolveDismemberChance`/`ResolveHeadOutcome`): (C) exceção por munição individual (`ammo.Name`/`ammo.AmmoTemplate.Name`, chave = nome interno tipo `patron_23x75_barricade`); (A) soma dinâmica de momento (N·s) pra munição `ProjectileCount > 1`, agrupada por `(player, FireIndex, parte do corpo)` — reusa o padrão de agrupamento já existente do desmembramento de perna em vivos, mas **acumulando** em vez de deduplicar; (B) tabela por calibre (a antiga, agora com 4 valores em vez de 1); fallback final = 0 (nunca mais um default alto tipo o `0.5f` de antes).
- **Cabeça ganhou 2 rolagens independentes:** "arranca" (`Head_3`, remoção completa, pipeline `DismemberLimb` existente) e "estourar" (`Head_1`/`Head_2`, novo método `BurstHead` — cabeça real **não** é escondida/encolhida, só sobrepõe o prop e aplica sangue/drop de equipamento). Mapeamento confirmado pelo usuário via AssetStudio (não pela leitura de código).
- **`VD_Calibers.json` reescrito**, gerado programaticamente a partir da planilha `municoes-tabela-completa.xlsx` (29 calibres com valores manuais do usuário) + lista de exceção pequena (Barrikada/Zvezda do 23x75 — os únicos membros de projétil único desse calibre, já que Shrapnel-10/25/Volna-R e o restante caem automaticamente no mecanismo A) + bloco de curva do mecanismo A (thresholds ainda placeholder).
- **Thresholds do mecanismo A movidos pro JSON** em vez de constante hardcoded (`PA-01-03` da review técnica) — dá pra recalibrar sem recompilar.

**Lições / hipóteses descartadas:**
- **Bug pré-existente real encontrado durante a implementação:** `AmmoTemplate.Caliber` mantém o prefixo `"Caliber"` (`"Caliber556x45NATO"`), mas `AmmoItemClass.Caliber` (`AmmoItemClass.cs:32`) já vem **sem** o prefixo. O código antigo de `LimbKillPatch.cs` tentava as duas formas de busca mas nunca batia com as chaves da tabela (que tinham o prefixo) — a chance de desmembramento pós-morte por bala provavelmente **sempre caiu no default `0.5f`** desde que essa lógica foi escrita, nunca usou os valores calibrados por calibre. Corrigido com `KillPatch.NormalizeCaliber` aplicado nos dois pontos de consumo.
- **`AmmoItemClass.Name` não é o nome interno** — é `Item.Name => Template.NameLocalizationKey` (chave de localização). O nome interno real (`"patron_23x75_barricade"`) precisa de `ammo.AmmoTemplate.Name` (indo pelo template). Hipótese inicial (usar `ammo.Name` direto) teria quebrado silenciosamente o mecanismo C vindo do `LimbKillPatch` (nunca acharia as exceções).
- **`SkeletonRootJoint` não é `Transform`** — é `Diz.Skinning.Skeleton` (só serve pro `Skin.Init()`). Pra ancorar efeitos de sangue em `BurstHead`, usar `player.PlayerBones.Head.Original` (o bone real da cabeça) é mais preciso, e resolveu de quebra o `TODO confirmar` que a spec técnica tinha deixado em aberto.
- **Achado de code-review não resolvido (`CR-01-01`, 🟠):** `KillPatch.Postfix` (via `ApplyDamageInfo`) e `LimbKillPatch.ProcessLimbKill` (via `BallisticsCalculator.Shoot`) parecem ser dois caminhos independentes que podem processar o **mesmo pellet físico** num corpo já morto — no código antigo isso era inofensivo pra `DismemberLimb` (guarda de idempotência por escala já existente), mas o acumulador de momento novo não tem essa guarda, então pode contar o mesmo pellet duas vezes se os dois caminhos realmente disparam pro mesmo evento. Não resolvido nesta sessão — registrado como pendência (`[P-8.2]`), precisa de validação em jogo antes de decidir se vale a pena adicionar a guarda.

**Atividade cronológica:**
1. Usuário terminou de calibrar a tabela B (4 colunas por parte do corpo, 31 calibres) na planilha e confirmou visualmente no AssetStudio que `Head_3` = arranca, `Head_1`/`Head_2` = estourar.
2. Criado item 004: spec funcional → review (2 corner cases amarrados a condições) → spec técnica (achado + corrigido: contagem dupla de momento na cabeça; achado + confirmado: `damageInfo.FireIndex` disponível) → review técnica (0 bloqueadores, thresholds movidos pro JSON).
3. Gerado `VD_Calibers.json` novo programaticamente a partir da planilha + exceções + curva.
4. Implementado em `KillPatch.cs`/`LimbKillPatch.cs`/`GameStartedPatch.cs`/`VisceralEntry.cs` — 4 bugs reais encontrados e corrigidos durante a implementação (ver Lições acima), incluindo um bug pré-existente de normalização de calibre.
5. Compilado com sucesso (versão 3.9.13). Code review: 1 achado 🟠 (`CR-01-01`), não aplicado nesta rodada — registrado como pendência pra decisão informada (validar em jogo vs. adicionar guarda de idempotência).

**Pendências abertas nesta sessão:**
- Ver bloco "Pendências / próximos passos conhecidos" no topo — [P-8.2] 🟠, [P-8.3] 🟡.

**Cross-refs:**
- Artefatos completos: `mods/VisceralCombat/backlog/004-reformular-chance-desmembramento-calibre/`.
- Continuação direta da Sessão 8 (mesmo dia, mesmo fio de trabalho — calibre/desmembramento).

---

## 2026-09-10 00:30 (GMT-3) — Sessão 2026-09-10 (Sessão 8): Boss/Escolta Imunes ao Desmembramento em Vivos (item 003) + Investigação de Assets de Cabeça + Planilha de Munições

**Tema central:** Ciclo SDD completo do item 003 (Boss/escolta vivos nunca desmembram a perna) + investigação exploratória sobre o sistema de chance de desmembramento por calibre (preparando uma revisão futura da fórmula) e sobre os assets 3D de cabeça decepada.

**Decisões-chave:**
- **Item 003 implementado:** guarda adicionada em [`LimbKillPatch.cs:66-79`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L66-L79) — dentro do ramo `!isDead` já existente, checa `player.Profile?.Info?.Settings?.Role` e retorna cedo se `WildSpawnType.IsBossOrFollower()` for verdadeiro (`BotSettingsRepoClass.cs:555-559`). Decisão de usar a extension method nativa do jogo em vez de lista própria de bosses no mod — evita dívida de manutenção a cada boss novo. Pós-morte não é afetado (Boss morto desmembra normal). Code review: 0 achados.
- **Achado — chance de desmembramento é tabela estática, não fórmula física:** `KillPatch.calibers` (`% dismember_calibers` em `VD_Calibers.json`) é uma tabela hand-tuned por calibre, não usa momento (N·s). A fórmula `p = m·v` que existe no código (`IsHeavyCaliberNoAgony`) serve só pra decidir se pula a animação de agonia num hit fatal — sistema totalmente separado da chance de desmembrar.
- **Achado — `IsHeavyCaliberNoAgony` mal calcularia buckshot se reaproveitada como está:** ela já trata calibre 12 balote (`ProjectileCount<=1`) como pesado, mas chumbo de espalhamento (`ProjectileCount>1`) cairia no momento de **um** chumbinho isolado (~1-1.4 N·s, abaixo do threshold de 5.0 N·s) — subestimaria o poder real do disparo. Qualquer fórmula nova precisa somar os chumbinhos do mesmo `FireIndex` (padrão já usado em `LimbKillPatch` pro desmembramento de perna em vivos) em vez de avaliar isolado.
- **Achado — `bleed_calibers`/`BleedPatch` é 100% cosmético:** só escolhe partícula de sangue (VFX), não toca `HealthController` nem dano de sangramento real. O único sangramento real que o mod aplica é o `20f HP/s` fixo do `LivingDismembermentController` (perna decepada em vivo), que não depende de calibre.
- **Assets de cabeça:** `gorecaps.bundle` tem 3 prefabs `Head_1/2/3` usados pelo código, mas o bundle contém MAIS assets que o código nunca referencia por nome: `gore_neck_cap01/03` (pescoço sem cabeça) e `head_exploded01/02` (variante "estourada", nunca ligada no C#). Achado por `grep` bruto nas strings do `.bundle` — indício (não confirmado visualmente) de que `Head_3` é a variante sem cabeça, já que só ela tem textura temática de "Neck" em vez de "Brain/Eye/Teeth" como `Head_1`/`Head_2`.

**Lições / hipóteses descartadas:**
- Nenhuma lição nova de causa raiz — sessão de investigação + 1 fix pequeno e limpo (sem retrabalho).

**Atividade cronológica:**
1. Levantada a tabela `VD_Calibers.json` completa (22 calibres cobertos) e confrontada com a fórmula de momento já existente no código — identificado o desconexo entre os dois sistemas.
2. Investigados os assets `gorecaps.bundle` via `grep` nas strings binárias — orientado o usuário a usar AssetStudio pra confirmar visualmente.
3. Gerada `mods/VisceralCombat/docs/municoes-tabela-completa.xlsx` (openpyxl) a partir do banco real do servidor SPT (`references/spt-source/.../templates/items.json`, 210 munições/31 calibres) cruzado com `VD_Calibers.json` — achado: `.50 BMG` (127x99) ausente das duas tabelas do mod.
4. Confirmada ausência de qualquer proteção pra Boss no desmembramento em vivos (`grep` "Boss"/"WildSpawnType" no mod → 0 ocorrências) e localizada a API canônica `WildSpawnType.IsBossOrFollower()`.
5. Ciclo completo do item 003: spec funcional → review (1 corner case) → spec técnica (verificação de tipos `Profile.Info.Settings.Role` linha a linha) → review técnica (0 bloqueadores) → código → build → code review (0 achados).

**Pendências abertas nesta sessão:**
- [P-8.1] 🟡 — ver bloco "Pendências / próximos passos conhecidos" no topo.

**Cross-refs:**
- Artefatos do item 003: `mods/VisceralCombat/backlog/003-bloquear-desmembramento-boss-vivo/`.
- Planilha de referência: `mods/VisceralCombat/docs/municoes-tabela-completa.xlsx` (não é artefato de backlog — material de apoio pra decisão futura de calibre/momento).

---

## 2026-09-09 23:02 (GMT-3) — Sessão 2026-09-09 (Sessão 7): Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça (item 002) + Auditoria Fika

**Tema central:** Ciclo SDD completo (`/add-backlog-item` → `/code-review`) do item 002 — dropar a arma em mãos (exceto faca) na morte, dropar capacete+óculos a 100% no desmembramento real de cabeça (distinto do `ShootOffHelmetPatch` já existente), e auditar sincronismo Fika de ambos.

**Decisões-chave:**
- **Ponto de patch da arma — `Player.DropItemDead`, não `CreateCorpsePatch`/`CreateBSGRagdollPatch`:** investigação no Assembly revelou que o vanilla **já** intercepta o item em mãos na morte (`Player.OnDead` → `method_98` → `DropItemDead`, [Player.cs:30539/30681/30686](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs)), mas só faz um fling cosmético (`AttachWeapon`, item continua no cadáver) — não um drop real. `WeaponDropOnDeathPatch` (novo) faz Prefix nesse método vanilla e chama `ThrowItem` (mesmo padrão de `ShootOffHelmetPatch.cs:41-44`) quando não é faca. Ver `002-...-02-spec-tech.md` §0.
- **Ponto de patch do capacete/óculos — dentro de `KillPatch.DismemberLimb`, fora do `foreach` de transforms:** os 3 gatilhos reais de desmembramento de cabeça (`KillPatch.Postfix` caso 0, `LimbKillPatch.ProcessLimbKill` estratégias A/B) convergem nesse método único. Correção pós-review (`PA-01-01`, 🔴): o ponto de inserção precisa ficar **fora** do `foreach (Transform val in array)` (`KillPatch.cs:230-448`), senão dispararia 1x por transform casado em vez de 1x por evento.
- **Gate de autoridade de rede — `FikaBackendUtils.IsServer || IsSinglePlayer`, sem gate de paridade:** decisão do usuário (via pergunta direta) de **não** condicionar os novos drops ao `AllPlayersHaveVisceralCombat` (usado só pelo desmembramento de perna em bots vivos, item 001) — precedente do mod é incondicional (nem o desmembramento de cabeça pós-morte nem o `ShootOffHelmetPatch` checam esse gate).
- **Detecção de faca — `item is KnifeItemClass`, não `GetItemComponent<KnifeComponent>()`:** ver Lições abaixo.

**Lições / hipóteses descartadas:**
- *"A arma não cai hoje porque ninguém implementou isso"* — falso. O vanilla já dropa fisicamente a arma (`Player.cs:26802-26855 DropItemDead` → `Corpse.Ragdoll.AttachWeapon`), só que como efeito cosmético (arma balança presa ao cadáver via rigidbody), não como remoção real do inventário. Confundir "solta visualmente" com "vira item independente no mundo" teria levado a duplicar lógica desnecessariamente se não investigado a fundo primeiro.
- *`Item.GetItemComponent<KnifeComponent>()` seria seguro por ser exatamente o teste que o vanilla usa (`Player.cs:26848`)* — citação correta no `.cs` do dump, mas o `dotnet build` real falhou (`CS0311`/`CS0012`): `IItemComponent` vive num assembly (`ItemComponent.Types`) não referenciado por `VisceralCombat.csproj`. Caso real de AP-09 — recon/leitura do dump correto ≠ compila no projeto. Corrigido para `item is KnifeItemClass` (`KnifeItemClass.cs:7`, mesmo assembly `Assembly-CSharp`), sem mudar o resultado.
- *Achado de auditoria (não uma hipótese testada, mas relevante registrar):* os pacotes de rede já existentes do mod (`DismembermentPacket`, `LivingDismembermentPacket`, `RagdollSyncPacket`) **não seguem** `docs/technical/fika-packet-desync-prevention-plan.md` (sem envelope, sem `TryGet*`, sem `Valid`) — dívida pré-existente, não introduzida por este item (item 002 não criou pacote novo — replica pela operação nativa de inventário). Ver [P-7.3].

**Atividade cronológica:**
1. Investigado Assembly-CSharp (`Player.cs`, `TraderControllerClass.cs`, `EquipmentSlot.cs`, `Corpse.cs`) e código do mod (`KillPatch.cs`, `LimbKillPatch.cs`, `ShootOffHelmetPatch.cs`, pacotes Fika) — spec técnica com 9 refs verificadas ao dump.
2. `/review-technical-spec` rodada 01: 1 🔴 (ponto de inserção errado) + 2 🟡 (efeito de pular `DropItemDead` sobre `Corpse.SetItemInHandsLootedCallback`; autoridade de rede do item 1 sem precedente) + 1 🟢. Todos os 4 resolvidos por investigação adicional (leitura de `Corpse.cs`, `Player_OnDead_Patch.cs`/`ObservedPlayer.cs` do FIKA) antes do `/code-mod` — nenhum ficou pendente.
3. `/code-mod`: criado `WeaponDropOnDeathPatch.cs`, modificado `KillPatch.cs` (`DropHeadEquipment`) e `VisceralEntry.cs` (2 `ConfigEntry`, registro do patch, versão 3.9.10→3.9.11). `dotnet build` revelou o erro de assembly do `KnifeComponent` (ver Lições) — corrigido antes de prosseguir.
4. `/code-review` rodada 01: 1 🟡 (bug latente — `WeaponDropOnDeathPatch` podia descartar a arma silenciosamente se o cast pra `TraderControllerClass` falhasse) + 1 🟢. Ambos aplicados via `/apply-code-review` — rebuild confirmado.
5. `PROPRIEDADES.md` e `mod-backlog.md` atualizados; `05-asbuild.md` gerado e mantido com as 2 rodadas de correção.

**Pendências abertas nesta sessão:**
- Ver bloco "Pendências / próximos passos conhecidos" no topo — [P-7.1] 🔴, [P-7.2] 🟡, [P-7.3] 🟡.

**Cross-refs:**
- Artefatos completos: `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/` (spec funcional, spec técnica, review técnica 01, code review 01, as-built).

---

## 2026-08-24 21:55 (GMT-3) — Sessão 2026-08-24: Refatoração de Performance, Wake on Hit, Ancoragem de Sangue, Correção de Impulso em Vivos, Poças de Ambiente e Prone Death

**Tema central:** Refatoração profunda de estabilidade e performance do Visceral Combat (versões 3.9.0 a 3.9.10), abrangendo Wake on Hit dinâmico, preservação de animações de agonia, mitigação de gargalos de CPU no EFT, ancoragem precisa de esguichos arteriais, bloqueio de impulsos físicos em jogadores/bots vivos, geração de poças reais no chão e eliminação de teleporte em pé de bots deitados.

**Decisões-chave:**
- **Wake on Hit & Sono Cinemático Inteligente (`RagdollHelperClass.cs`):**
  - Cadáveres entram em sono cinemático (`isKinematic = true`, `UnsupportRigidbody`, discrete collision) após repouso completo (3 checagens consecutivas < 0.08 m/s) e término das animações de agonia do PuppetMaster.
  - Ao receberem tiros ou impacto de granadas, `WakeCorpse(hitCollider, duration)` acorda temporariamente os rigidbodies por 2.5s, permitindo reações físicas completas e retornando ao repouso cinemático logo em seguida (consumindo 0% de CPU na maior parte da raid).
  - Adicionada guarda `if (rb.isKinematic)` antes de chamar `EFTPhysicsClass.GClass745.SupportRigidbody`, evitando duplicações desnecessárias na `List_0` interna do EFT.
- **Otimização de Granadas (`GrenadeDeadBodiesPatch.cs` e `GrenadeItemsPatch.cs`):**
  - Substituído `SphereCastAll` por `Physics.OverlapSphere`.
  - Implementada deduplicação via `HashSet<Transform> awakenedRoots` (1 chamada de `WakeCorpse` por cadáver) e `HashSet<Rigidbody> processedRigidbodies` (1 impulso por osso físico), eliminando o travamento de CPU ao explodir granadas perto de múltiplos corpos.
- **Ancoragem Dinâmica dos Esguichos Arteriais (`KillPatch.cs` e `BleedPatch.cs`):**
  - Implementado `GetPhysicalBone` para ancorar `SpawnArterialSprays` diretamente ao osso físico em movimento do ragdoll.
  - `HitEffect` e `BleedEffect` ancorados diretamente ao transform/rigidbody atingido (`worldPositionStays = true`, `simulationSpace = World`), eliminando o bug do esguicho jorrando fixo no ar no ponto A da morte enquanto o corpo caía no ponto B.
- **Eliminação do Deslize/Tropeço do Jogador ao Tomar Tiro (`BodiesImpulsePatch.cs` & `RagdollHelperClass.cs`):**
  - Adicionada verificação `targetPlayer.HealthController.IsAlive`.
  - Se a entidade atingida estiver **VIVA**, o impulso de ragdoll e a ativação de rigidbodies (`WakeCorpse`) são estritamente ignorados, mantendo os ossos em `isKinematic = true` sob controle do `CharacterController` do Tarkov e eliminando empurrões/tropeços involuntários para trás.
- **Intangibilidade de Partículas de Sangue em Personagens (`ConfigureBloodParticleCollision`):**
  - Força `collision.enabled = true` em modo 3D World para detecção no ambiente, mas exclui explicitamente as camadas `Player`, `HitCollider`, `Deadbody` e `TransparentFX` de `collision.collidesWith`, com `colliderForce = 0f`.
- **Geração Real de Poças de Sangue no Ambiente (`ParticleFloorPainter.cs`):**
  - Substituído o método de micro-pingos (`EmitBleeding`) pelo método de poças reais (`Singleton<Effects>.Instance.EmitBloodOnEnvironment`).
  - Reduzido o cooldown para `0.15s` e garantida a resolução resiliente de `ParticleSystem` em nós pais e filhos.
- **Morte Suave de Bots Deitados (`RagdollHelperClass.cs`):**
  - Detecção de postura `isProne` (`p.IsInPronePose || p.PoseLevel <= 0.1f`).
  - Bloqueio de animações gravadas em pé (`Death_Neck`, `Death_Stomach`, `Death_Thigh`), substituindo-as por `Flail_Loop` no chão (65%) ou colapso direto em ragdoll natural (35%), eliminando o snap/teleporte em pé de bots deitados ao morrerem.

**Lições / hipóteses descartadas:**
- *Cena de Física Fantasma (Shadow Scene):* Avaliada a viabilidade da técnica de `darkarchon` (Multi-Scene Physics na Unity). Concluiu-se que o sistema Wake on Hit atual já entrega >95% do ganho real de desempenho (0% CPU com corpos no chão) sem os riscos de corpos atravessarem o mapa ou dessincronizarem no FIKA coop.
- *Falso Positivo de Sangue no Tropeço:* O tropeço involuntário do PMC ocorria devido a `BodiesImpulsePatch` chamar `WakeCorpse` em jogadores vivos, ativando física dinâmica nos ossos do PMC que colidiam por dentro com a cápsula do `CharacterController`.

**Atividade cronológica:**
1. Implementado Wake on Hit e preservação física de rigidbodies/joints em `RagdollHelperClass.cs`.
2. Implementada detecção de repouso dinâmico `IsCorpseAtRest` e proteção contra corpos pendurados.
3. Corrigida a ancoragem de esguichos arteriais aos ossos físicos em movimento em `KillPatch.cs` e `BleedPatch.cs`.
4. Otimizados os patches de granadas (`GrenadeDeadBodiesPatch` e `GrenadeItemsPatch`) com `OverlapSphere` e deduplicação.
5. Corrigido vazamento de CPU em `SupportRigidbody` com verificação `rb.isKinematic`.
6. Implementado `ConfigureBloodParticleCollision` para isolar colisões de partículas de sangue das camadas de personagens.
7. Corrigido `SleepCorpseWhenAtRest` para inspecionar PuppetMaster ativo e evitar congelamento prematuro de agonias.
8. Bloqueado impulso físico e `WakeCorpse` em jogadores vivos em `BodiesImpulsePatch.cs`.
9. Atualizado `ParticleFloorPainter.cs` para emitir poças de ambiente reais via `EmitBloodOnEnvironment` com cooldown de 0.15s.
10. Implementada mitigação para bots deitados (`isProne`) em `PlayDeathAnimation`, eliminando teleporte em pé.
11. Compilada a versão final `3.9.10` com 0 erros.

---

## 2026-08-12 10:00 (GMT-3) — Sessão 2026-08-12: Balanceamento 0.25x, Bloqueio de Agonia, Exsanguição 20f, Nuvem Vanilla e Faíscas em Coletes

**Tema central:** Balanceamento fino do impulso de ragdolls (fator 0.25x), remoção de agonia em mortes por calibres pesados, ajuste de dano de sangramento para 20f HP/s, reversão completa de overrides de materiais de sangue e implementação dos controles de nuvem vanilla e faíscas de colete.

**Decisões-chave:**
- **Massa de Chumbinho de Calibre 12 Corrigida:** Removida divisão duplicada `/ projectileCount` em `BodiesImpulsePatch.cs`.
- **Fator Redutor de Impulso 0.25x:** Aplicado multiplicador `0.25f` no momento linear $p = m \cdot v$ em `BodiesImpulsePatch.cs`, reduzindo a projeção do cadáver para valores altamente realistas.
- **Bloqueio de Animação de Agonia em Kills Fatais por Calibres Pesados:** Em `KillPatch.cs`, mortes fatais com calibres pesados (.338 Lapua, 12g/20g Slugs, 23x75mm, 40x46mm, .50 BMG, 30x29mm) ou $p_{\text{raw}} \ge 5.0\text{ N}\cdot\text{s}$ chamam `InterruptAgony` diretamente, permitindo movimentação física imediata do corpo.
- **Toggle "Arterial Spraying":** Adicionado cheque `!ArterySpray.Value` em `BleedPatch.cs` para pausar jorros de sangue ao desativar a opção no F12.
- **Dano de Exsanguição (20f HP/s):** Alterado dano em `LivingDismembermentController.cs` para `20f`. Validação em `ActiveHealthController.cs` provou que a Unity aplica o `OverDamageFactor` ($\sim 0,7$) em membros destruídos, resultando em perda líquida real de $\sim 14$ HP/s de vida total.
- **Reversão de Materiais de Sangue:** Revertidas todas as alterações de materiais/shaders via C# para manter 100% dos shaders e transparências originais do mod intactos sem quads pretos.
- **Ajuste F12 da Nuvem de Sangue (Vanilla):** Adicionadas configurações BepInEx em `VisceralEntry.cs` (`EnableImpactBloodCloud`, `ImpactBloodCloudParticleCount`, `ImpactBloodCloudScale`) que atuam sobre o `Systems.Effects.Effects.Instance` para `MaterialType.Body`.
- **Faíscas Metálicas ao Atingir Placa de Colete/Capacete:** Em `BleedPatch.cs`, tiros que atingem placas de blindagem (`HitArmorItemID != null`) ou capacetes/metais desativam a nuvem de sangue e disparam o efeito nativo de faíscas metálicas (`MaterialType.MetalThick`).
- **Desmembramento de Bots Vivos a 30% por Disparo (Agrupamento de Chumbinhos):** Em `LimbKillPatch.cs`, a chance de desmembramento de perna em bots vivos foi fixada em **30% por disparo** (`0.30f`). Para escopetas/chumbinhos, todas as esferas do mesmo disparo compartilham o mesmo `shot.FireIndex` e são agrupadas em `_evaluatedLivingVolleys`, garantindo exatamente 1 teste de 30% por tiro (e não 30% por esfera).
- **Bloqueio Absoluto de Postura (Trava de Bruços Perto de Obstáculos):** Criado `ProneLockPatch.cs` (`ProneLockPatch`, `ProneMoverDoPronePatch`, `ProneMoverSetPosePatch`) interceptando chamadas internas da IA do Tarkov ao encostar em paredes/superfícies (`BotLay.IsLay = false`, `BotMover.DoProne(false)` e `BotMover.SetPose(>0)`). Bots em agonia de perna amputada agora ficam 100% travados de bruços sem o efeito visual de levantar e cair repetidamente.

**Lições / hipóteses descartadas:**
- *Overdamage Factor no Tarkov:* Danos aplicados a membros já destruídos (HP = 0) sofrem uma redução de $\sim 30\%$ via `OverDamageFactor` na redistribuição de dano para o restante do corpo do bot.
- *MaterialPropertyBlock em VolumetricBloodFX:* Modificar materiais/shaders via C# não sobrescreve os `MaterialPropertyBlock` aplicados no `Update()` das partículas pelo `VolumetricBloodFX`. Restaurar os materiais originais foi a solução mais estável.

**Atividade cronológica:**
1. Ajustada massa de calibre 12 e aplicado fator `0.25f` de impulso em `BodiesImpulsePatch.cs`.
2. Adicionada verificação de `!ArterySpray.Value` em `BleedPatch.cs`.
3. Implementado filtro `IsHeavyCaliberNoAgony` em `KillPatch.cs`.
4. Analisado erro de stack trace `BotWeaponManager.UpdateHandsController` e entregue laudo.
5. Ajustado dano de exsanguição para `20f` em `LivingDismembermentController.cs`.
6. Revertidos os testes de alteração de cor/shader de sangue para preservar o material original.
7. Investigado `references/eft-decompiled` para a nuvem de sangue vanilla (`Systems.Effects.Effects.cs`) e faíscas metálicas.
8. Criadas propriedades BepInEx e rotinas de injeção para nuvem de impacto e faíscas em placas de colete.
9. Recompilado o mod e realizado commit git (`ebaf7a1f`).

## 2026-08-11 22:37 (GMT-3) — Sessão 2026-08-11: Física de Impacto Realista, Fix de LookRotation e Finalização da Spec 001

**Tema central:** Correção definitiva do aviso C++ LookRotation via escala `limbSize`, implementação da física universal $p = m \cdot v$ em ragdolls e finalização dos artefatos de spec/review do item 001.

**Decisões-chave:**
- **Fix de `LookRotation` sem supressão de log:** Alterada a constante `RagdollHelperClass.limbSize` de `0.001f` para `Vector3(0.1f, 0.1f, 0.1f)`. A escala 0.1f resolve a imprecisão flutuante em float32 da Unity C++ Engine no cálculo de vetores de ossos sem revelar o osso amputado a olho nu.
- **Partículas de Sangue Isentas de Escala:** `BleedPatch.cs` e `KillPatch.cs` ajustados para anexar partículas de sangue à raiz do jogador (`player.Transform.Original`) com `worldPositionStays = false` e `localScale = Vector3.one`.
- **Física Universal de Impacto de Projétil ($p = m \cdot v$):** Removido impulso duplicado `shot.Speed * 0.15f` em `LimbKillPatch.cs`. Em `BodiesImpulsePatch.cs`, substituída a tabela estática pelo momento linear direto $p = (m/1000) \times v$ em N.s. Compatibilidade 100% automática com munições nativas e de mods.
- **Sangramento de Vida (10 HP/s) & Poças de Sangue 0.2s:** `LivingDismembermentController` emite poças nativas no chão a cada 0.2s e aplica 10 HP/s de `HeavyBleedingDamage`, garantindo 30–40s de agonia/rastejo enquanto inviabiliza cura completa por Medkits.
- **Finalização do Backlog 001:** Geradas a Spec Técnica (`02-spec-tech`), Review Técnica (`03-spec-tech-review-01`), As-Built (`05-asbuild`) e Code Review (`04-code-review-01`, com `CR-01-01` rejeitado pelo usuário). Status atualizado para `🟢 Entregue` no `mod-backlog.md`.

**Lições / hipóteses descartadas:**
- *Hipótese descartada (Partículas de Sangue em Escala Zero):* O aviso `Look rotation viewing vector is zero` continuava ocorrendo após ajustar as partículas de sangue porque o `Animator` C++ da Unity estava ativo num bot vivo cujo osso da coxa fora encolhido a `0.001f`. Aumentar o osso para `0.1f` satisfez o limite de precisão do vetor C++ sem suprimir logs.
- *Duplicidade de Impulso em Ragdolls:* A velocidade pura `shot.Speed * 0.15f` em pistolas 9mm injetava +57 N.s no osso da cabeça de 2.5 kg, resultando em aceleração irrealista de 82 km/h. Unificar a força no momento linear real $p = m \cdot v$ corrigiu o comportamento em todas as munições.

**Atividade cronológica:**
1. Ativado SPY de 2ª geração para mapear origens do aviso `LookRotation`.
2. Identificada origem no solver de ossos nativo C++ da Unity ao utilizar `limbSize = 0.001f`.
3. Ajustado `limbSize` para `(0.1f, 0.1f, 0.1f)` em `RagdollHelperClass.cs` e verificado desaparecimento total do aviso.
4. Adicionada emissão de poças visuais de sangue a cada 0.2s e balanceada a perda de HP em 10 HP/s no `LivingDismembermentController.cs`.
5. Removido SPY logger do `VisceralEntry.cs`.
6. Refatorado `BodiesImpulsePatch.cs` e `LimbKillPatch.cs` para momento físico universal $p = m \cdot v$.
7. Gerados artefatos formais do workflow de backlog do item 001 e atualizada a memória.

---

## Sessão 2026-08-10 — Implementação da Feature 001 (LivingDismembermentController v3.8.2)

### Implementação do `LivingDismembermentController.cs`
- **Prone Lock:** Mantém `BotLay.IsLay = true` e `NextPosibleGetUp = Time.time + 99999f` no `Update()` para travar permanentemente o bot no chão de bruços.
- **Exsanguição (Heavy Bleed):** Aplica `15 HP` de `HeavyBleedingDamage` a cada `2.5s` na perna amputada. Se curado, reaplica automaticamente no tick seguinte.
- **Esguicho Arterial:** Instancia o efeito de sangramento pesado no coto da perna com shader escuro coagulado (`ApplyDarkCoagulatedBloodFx`).
- **Rastro de Sangue:** Utiliza a API nativa do Tarkov (`Singleton<Effects>.Instance.EmitBleeding`) para gerar poças no chão enquanto o bot rasteja.
- **Frases de Agonia:** Chama `Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Dying, true)` periodicamente (8–14s).
- **Gate FIKA:** Condicionado a `VisceralEntry.AllPlayersHaveVisceralCombat` (retorna `null` se nem todos os humanos tiverem o mod em raid coop).

### Integração no `LimbKillPatch.cs`
- Balística atualizada para permitir bots vivos (`!isDead && player.IsAI && AllPlayersHaveVisceralCombat`).
- Ao amputar perna (`LeftLeg` / `RightLeg`), anexa `LivingDismembermentController` no bot.

---

## Sessão 2026-08-10 — Handshake FIKA para LivingDismemberment

- **`VisceralHandshakePacket.cs`**: packet bidirecional host↔cliente.
- **`VisceralEntry.AllPlayersHaveVisceralCombat`**: flag estática gating da feature.
- **`VisceralEntry.StartVisceralHandshake()`**: solo SPT = `true` imediato; FIKA coop = host envia ping e avalia ACKs em 5s.

---

## Sessão 2026-08-10 — Estilização de Sangue Escuro & Zero Glow

- **Fix Final (`ApplyDarkCoagulatedBloodFx`):** Escopo corrigido para `ps.gameObject`. Tratamento bifurcado por shader (`VD 3D Blood Shader V14` vs `Alpha Blended Premultiply`).

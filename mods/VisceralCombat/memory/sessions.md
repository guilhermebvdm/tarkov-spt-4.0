# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.11.9 (SPT 4.0 / FIKA 2.2.6)
- **Code-review ad-hoc da feature "Item Physics" (`ItemForce`, `BodiesImpulsePatch`/`GrenadeItemsPatch`/`PhysicalItemsPatch`):** usuário pediu revisão depois de discutir performance da feature. Achados e aplicados: (1) `_dictionary` (23 calibres, força de impulso) em `BodiesImpulsePatch.cs` era código morto herdado do `original/` — a versão modded já substituiu por fórmula física real (massa×velocidade da munição), removido; (2) `GrenadeItemsPatch.cs` alocava um array novo por explosão (`Physics.OverlapSphere`) — trocado por `OverlapSphereNonAlloc` com buffer estático de 128 (tradeoff: trunca silenciosamente se >128 colliders simultâneos, cenário raro); (3) **bug real**: `PhysicalItemsPatch.cs` reatribui item largado pra camada "Deadbody" quase imediatamente após o drop (assim que a corrotina de assentamento do jogo começa), mas `GrenadeItemsPatch.cs` só buscava na camada "Default" — granada nunca empurrava um item que já tinha mudado de camada. Corrigido incluindo as duas camadas na busca da granada.
- **Item `002` fechado — 🟢 Entregue.** Todos os fixes da sessão (01, 02, 03) validados em raid pelo usuário: sincronização+posição do drop pós-morte de capacete/óculos, fone/máscara incluídos no drop, arma dropando corretamente mesmo se o bot/player morre no meio de tiro/recarga. Fix 04 (ícone de viseira) fechado como N/A — não era bug do mod, revertido. Item `005` (toggle mestre) segue 🟢 Entregue (build), mas com `[P-14.1]` aberta — usuário ainda não testou o toggle em si.
- **Investigação do "bug de ícone de capacete" ENCERRADA — não era o VisceralCombat.** Cadeia completa (pra não repetir): (1) usuário reportou ícone 2D só mostrando a viseira → investigação achou `TogglableComponent`/`FaceShieldComponent`, contorno aplicado forçando viseira levantada. (2) usuário revelou que capacete SEM viseira TAMBÉM bugava — descartou a teoria da viseira. (3) confirmado que loot normal do mapa funcionava certo (só capacete de drop bugava) — parecia confirmar que era o mecanismo do mod. (4) usuário propôs instrumentar o caminho manual de drop (via UI) com patches de log — nessa etapa, um bug AUTOINFLIGIDO (versão `"3.11.5-debug"` no `[BepInPlugin]`, sufixo não-numérico que o BepInEx rejeita silenciosamente, "pulando" o plugin inteiro sem crash visível) invalidou o primeiro teste. (5) **antes de repetir o teste, o usuário comprou um capacete com viseira no MERCADO (fora de raid, nenhuma relação com nenhum código deste mod) e reproduziu o MESMO bug** — prova definitiva de que a causa era outro mod no pacote do usuário, não o VisceralCombat. Resolvido pelo usuário revertendo pro pacote de mods conhecido-bom (mantendo a build atual do VisceralCombat) — confirmado funcionando no menu E em raid. **Limpeza:** removidos os 2 patches de diagnóstico e revertido o contorno da viseira (nenhum dos dois era necessário) — código volta ao estado limpo. Ver `002-drop-arma-capacete-oculos-cabeca-06-fix-04.md` pro histórico completo dos 5 rounds.
- **Lições importantes desta investigação:**
  1. `[BepInPlugin]` version string precisa ser SEMPRE puramente numérico (`major.minor.build[.revision]`) — sufixos tipo `-debug`/`-rc` fazem o BepInEx pular o plugin INTEIRO silenciosamente (só um aviso discreto no log: `Skipping type [X] because its version is invalid.`). Sempre conferir `LogOutput.log` por "Skipping type" depois de qualquer mudança de string de versão, especialmente em builds de diagnóstico.
  2. **Correlação não é causa, mesmo quando parece muito forte.** O usuário alternando o toggle da viseira e "consertando" o ícone (2 vezes, reproduzível) parecia prova definitiva — mas era coincidência (algum efeito colateral do toggle, tipo forçar um refresh de UI genérico, mascarando um bug de OUTRO mod). O teste que realmente decidiu a causa foi um teste de CONTROLE fora do escopo do mod inteiro (comprar no mercado, fora de raid) — vale sempre buscar esse tipo de teste de controle antes de aceitar uma correlação como causa, especialmente quando a "correção" não faz sentido de causa raiz (por que veiseira abaixada quebraria SÓ pra itens que passaram pelo nosso drop, e não pra loot normal do mapa? Essa pergunta devia ter sido feita mais cedo).
- **Fix 04 (mesma sessão, 3 rounds):** usuário reportou ícone 2D bugado (só a viseira aparece, círculo de carregamento travado) em capacete com viseira lootado. Confirmação experimental do usuário (toggle da viseira no inventário corrige/quebra o ícone) + o fato do modelo 3D no chão renderizar em tamanho normal apontam pra uma limitação de renderização DINÂMICA de ícone do próprio SPT (não sprite fixo, só pra itens com peça visível) — coincide com erros `SkinnedMeshRenderer` no console. **Não é bug do mod, e a causa raiz exata do lado do motor gráfico não foi confirmada** (fora do alcance de correção do mod). Contorno aplicado: `DeathInventoryDropPatch.RaiseHelmetVisorIfPresent(Item helmet)` força viseira levantada (`Set(true, silent: true)` — sem disparar efeito colateral de animação/som) antes do drop, via `GetItemComponentsInChildren<TogglableComponent>` (mesma API do próprio jogo). **Round 3 importante:** usuário testou 3.11.3 e capacete ainda caiu com viseira abaixada — achado um TERCEIRO caminho de drop nunca coberto, `ShootOffHelmetPatch` (chance de arrancar capacete de bot AINDA VIVO, tiro não-fatal — diferente de `DeathInventoryDropPatch`/`LimbKillPatch`). Fix aplicado lá também; varredura (`grep EquipmentSlot.Headwear` em todo `modded/`) confirma que são só esses 3 pontos no mod, todos cobertos agora. Precisou de nova referência `ItemComponent.Types.dll`. Build 3.11.4. Ver `002-drop-arma-capacete-oculos-cabeca-06-fix-04.md`. **Ainda não validado em jogo** — build mais recente não copiada pra `SERVER TEST` ainda.
- **Extensão pós-`[P-10.3]`:** a pedido do usuário, drop de equipamento de cabeça (item 002) estendido pra cobrir também `EquipmentSlot.FaceCover` (máscara) e `EquipmentSlot.Earpiece` (fone), reaproveitando 100% o mecanismo já validado (mesmo toggle `DropHeadEquipmentOnDismemberment`, nome da chave mantido por compatibilidade com valor já salvo). Aplicado em `DeathInventoryDropPatch.cs` e `LimbKillPatch.cs`. Build 3.11.0. Ver `002-drop-arma-capacete-oculos-cabeca-06-fix-02.md`. **Ainda não validado em jogo** — só fone/máscara em si são novos, o mecanismo de drop (rede + posição) já é o mesmo testado e confirmado em `06-fix-01.md`.
- **Fix 03 (mesma sessão):** usuário reportou arma de bot não dropando quando morto no meio de tiro/recarga. Investigação 100% analítica (sem log) confirmou: `FirearmController.CanExecute` (`Player.cs:13271`) recusa a operação de arremesso em silêncio se a arma (slot "animado") estiver com uma operação de tiro/recarga ativa em `CurrentOperation`, e `DropHandsWeapon` passava `null` como callback (falha nunca logada). Corrigido chamando `player.FastForwardCurrentOperations()` (mecanismo OFICIAL do jogo, o mesmo que `Player.OnDead` já usa — só que tarde demais pra nossa janela de sync) antes do `ThrowItem`. Capacete/óculos/máscara/fone NÃO são afetados por esse bug (não são slots animados). Build 3.11.1. Ver `002-drop-arma-capacete-oculos-cabeca-06-fix-03.md`. **Não validado em jogo ainda** — usuário optou por não copiar pra `SERVER TEST` ainda.
- **Estado:** Item `005` (toggle mestre no F12 + núcleo de compatibilidade sempre ativo) implementado e compilado via `/code-mod` na Sessão 13 — 19 patches "Camada 1" gateados por `VisceralCombatEnabled`/`IsCategoryActive`, queda de arma/capacete/óculos (Camada 2) e núcleo anti-fantasma (Camada 3) mantidos fora do toggle mestre por exigência de sincronização em coop. Marcado 🟢 Entregue no `mod-backlog.md`; **usuário confirmou explicitamente em 2026-09-20 que ainda NÃO testou o toggle em si** — só testou (e validou) o fix de `[P-10.3]` abaixo, que é um item DIFERENTE (item 002). Validação do toggle mestre continua pendente. Na Sessão 14, investigação de `[P-10.3]` (drop de capacete/óculos em desmembramento **post-mortem**, desativado desde `CR-03-01`) achou a causa raiz real via decompile (`ilspycmd`) de `InteractionsHandlerClass.Throw`/`smethod_17`/`ThrowOperationClass`: a operação não exige que o item pertença ao controller que a executa, e a POSIÇÃO do drop vem de um parâmetro `IPlayer` separado do controller de autoridade de rede. Fix aplicado em 2 rounds (round 1: sincronização correta mas posição errada, na do atirador; round 2: posição corrigida usando o cadáver como `IPlayer` de referência) — build 3.10.2, **usuário confirmou funcionando em raid ("Funcionou!")**, `[P-10.3]` fechado. **Descoberta operacional:** o usuário usa DUAS instalações separadas do jogo — `E:\Tarkov Red Line` (onde o `/compile-mod` deste repo instala por padrão, via `.spt-path`) e `E:\Tarkov Red Line - SERVER TEST` (onde ele efetivamente testa) — builds/assets precisam ser copiados pras DUAS quando o usuário estiver testando na segunda. Também achado nesta sessão (não relacionado ao P-10.3): `VD_Calibers.json` desatualizado (esquema antigo pré-item-004) especificamente na pasta SERVER TEST, causando `JsonSerializationException` no load — corrigido copiando o arquivo correto do repo. Item `002` segue com sua rodada 03/04 de `/code-review` concluída (`[P-10.1]` resolvido) mais o fix `[P-10.3]` validado. Itens `003`/`004` seguem implementados, sem validação in-game própria.
- **Pendências:** 🔴 0 · 🟡 6 · 🟠 1 (ver abaixo).

## Pendências / próximos passos conhecidos
- [P-7.3] (aberta 2026-09-09) 🟡 Débito técnico pré-existente descoberto durante a auditoria Fika do item 002: os pacotes próprios do mod (`DismembermentPacket`, `LivingDismembermentPacket`, `RagdollSyncPacket`) não seguem `docs/technical/fika-packet-desync-prevention-plan.md` — serializam com `Put`/`Get*` crus, sem envelope de comprimento, sem `TryGet*`, sem flag `Valid`. O mod nunca esteve no inventário auditado do guia (auditoria de 2026-07-26 cobriu 6 mods, VisceralCombat não incluído — guia desatualizado). Candidato a item de backlog de auditoria/fix separado (categoria AP-11).
- [P-8.1] (aberta 2026-09-10) 🟡 Item 003 (Boss/escolta vivo não desmembra a perna) não foi validado in-game — precisa de raid com boss pra confirmar (Killa/Reshala/etc. + um seguidor), além do teste de regressão (Scav comum continua desmembrando normal).
- [P-8.2] (aberta 2026-09-10) 🟠 `CR-01-01` do item 004 — `KillPatch.Postfix` e `LimbKillPatch.ProcessLimbKill` podem processar o mesmo pellet independentemente em corpos já mortos; se confirmado, o acumulador de momento do mecanismo A conta em dobro (chance mais alta que a curva calibrada prevê, não é crash). Precisa de validação em jogo ou de uma guarda de idempotência no acumulador. Ver `mods/VisceralCombat/backlog/004-reformular-chance-desmembramento-calibre/004-reformular-chance-desmembramento-calibre-04-code-review-01.md`.
- [P-8.3] (aberta 2026-09-10) 🟡 Item 004: thresholds do mecanismo A (`momentum_min_ns`/`momentum_max_ns`/`head_burst_multiplier` em `VD_Calibers.json`) são placeholders (3.0/15.0/1.5), não calibrados. Ajustar com teste em jogo (calibre 12 buckshot no mesmo membro).
- [P-10.2] (aberta 2026-09-11) 🟡 **Revisar à luz de causa raiz mais concreta.** Hardening defensivo (`try/catch`) de `CreateBSGRagdollPatch`/`RagdollClassPatch`, hipótese pra "bot morre em pé, sem ragdoll" — nunca confirmado por log real. Descoberto em 2026-09-20 que uma sessão paralela (2026-09-14) já resolveu esse mesmo sintoma por uma causa raiz DIFERENTE e concreta: campo de reflexão errado (`PlayerBody_0` → `PlayerBody`) em `CreateBSGRagdollPatch.cs` + `RagdollHelperClass.FindPlayerByNetId` não achava o player certo em coop (checava só `p.Id`, não `fp.NetId`) + falta de desligamento forçado de músculos do `PuppetMaster` fora do `RagdollMaxDistance`. Ver `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-05-asbuild.md` (entrada 2026-09-14). O hardening defensivo continua válido como proteção adicional (nunca piora nada), mas a causa raiz "de verdade" provavelmente já é essa, não a hipótese original de exceção não tratada. Fechar quando confirmado em jogo que o sintoma não volta mais.
- [P-14.1] (aberta 2026-09-20) 🟡 Item 005 (toggle mestre `VisceralCombatEnabled` no F12): build 3.10.x compilada e entregue, mas o usuário confirmou explicitamente que ainda **não testou o toggle em si** — só testou (e validou) o fix separado de `[P-10.3]` (drop de capacete pós-morte, item 002). Falta validar: toggle desligado suprime as 6 categorias mas mantém arma/capacete/óculos caindo; toggle ligado = comportamento idêntico ao atual; alternância em tempo real durante a raid; teste em coop com hosts/peers em estados diferentes do toggle.

---

## 2026-09-20 06:57 (GMT-3) — Sessão 14: `[P-10.3]` — Mecanismo Seguro pra Drop de Capacete/Óculos Post-Mortem Achado e Aplicado

**Tema central:** Continuação direta da Sessão 13 (mesmo dia). Usuário perguntou o status do drop de capacete/óculos após o `/code-mod` do item 005; ao ouvir que o caminho post-mortem (`LimbKillPatch.DropCorpseHeadEquipment`) estava desativado desde `CR-03-01`/`[P-10.3]`, pediu pra investigar o mecanismo seguro que a pendência apontava como necessário.

**Decisões-chave:**
- **Causa raiz real de `[P-10.3]` achada por decompile direto, não por tentativa-e-erro.** `InteractionsHandlerClass` (Assembly-CSharp) tem erro de decompile conhecido no dump (`DECOMPILE-ERROR`, ver `references/eft-decompiled/Assembly-CSharp/InteractionsHandlerClass.cs`) — usado `ilspycmd` (fallback documentado pro hook `remind-use-graph.sh`, com `# allow-ilspy`) direto no `Assembly-CSharp.dll` do jogo pra ler o IL de `Throw`/`smethod_17`. Achado: o parâmetro `itemController` do `Throw` **não precisa ser o dono do item** — só é usado pra contabilizar limite de descarte (comparando `item.Owner` com o dono do **endereço de destino**, nunca com `itemController`). O bug de `CR-03-01` não era "falta de mecanismo pra remover item de cadáver já criado" — era usar um controller **desconectado da rede** (`helmet.Owner as TraderControllerClass` = o `GClass3385` do próprio cadáver, sem override de `vmethod_1`) em vez de um controller Fika-aware.
- **Confirmado do lado Fika:** `HostInventoryController.RunHostOperation`/`ClientInventoryController.RunClientOperation` (`references/fika-plugin/Fika.Core/Main/HostClasses/HostInventoryController.cs`, `.../ClientClasses/ClientInventoryController.cs`) sincronizam a operação (`InventoryPacket.FromValue(FikaPlayer.NetId, operation)`) pelo **NetId de quem executa**, não pelo dono atual do item — qualquer controller Fika-aware pode agir sobre item de outro dono, e cada peer replica a operação localmente pelo ID do item.
- **Armadilha evitada: `ObservedInventoryController` não sobrescreve `vmethod_1`.** A ideia inicial (usar o `InventoryController` do atirador, `shot.Player`) tinha um furo: se o atirador é um peer remoto, no lado do host ele é representado por `ObservedInventoryController` (`references/fika-plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs`), que **não** sobrescreve `vmethod_1` — cairia no fallback local sem broadcast, reproduzindo o mesmo bug de novo (só que mascarado, porque funcionaria pra atirador=host/local mas falharia silenciosamente pra atirador=peer remoto). Resolvido usando sempre o `InventoryController` do **host** (`Singleton<GameWorld>.Instance.MainPlayer`, sempre `HostInventoryController` — método já gateado por `IsServer || IsSinglePlayer`), evitando a ambiguidade de identidade do atirador por completo.
- **Usuário aprovou aplicar direto e testar**, em vez de formalizar via SDD primeiro — decisão explícita dele após eu apresentar a descoberta e o tradeoff (área já causou 2 bugs reais antes por fixes não-formalizados). Registrado aqui para não repetir a pergunta numa sessão futura se o usuário confirmar que funcionou.
- **Round 2 — bug de posição achado pelo próprio teste do usuário.** Build 3.10.1 (round 1) sincronizava certo (sem duplicata/item travado, confirmado), mas o item aparecia na posição do HOST, não do cadáver (usuário relatou cenário concreto: atirador e corpo a 100m de distância, capacete caindo 1m na frente do atirador). Causa: `hostController.ThrowItem(...)` usa `Player.PlayerOwnerInventoryController.ThrowItem`, que sempre monta a operação com `Player_0` = o player DONO do controller (o host) como origem do arremesso — não configurável pelo wrapper público. Usuário perguntou diretamente se dava pra usar a referência do cadáver como ponto de partida mesmo com o atirador "dando o comando" — a resposta, confirmada via IL, é sim: `ThrowOperationClass` recebe `itemController` (autoridade de rede) e `player`/`IPlayer` (posição/trajetória) como parâmetros SEPARADOS. Fix: construir `ThrowOperationClass` manualmente (`InteractionsHandlerClass.Throw` + `.vmethod_1`, ambos confirmados `public` via IL) passando o cadáver como `IPlayer`, mantendo `hostController` só pra rede.
- **Descoberta operacional: usuário testa numa instalação DIFERENTE da que o `/compile-mod` deste repo instala por padrão.** `.spt-path` aponta pra `E:\Tarkov Red Line`, mas o usuário mencionou estar testando em `E:\Tarkov Red Line - SERVER TEST` — uma pasta separada. Builds precisaram ser copiadas manualmente pras duas. Também achado nesse contexto (não relacionado a `[P-10.3]`): `VD_Calibers.json` desatualizado (schema antigo, pré-item-004) especificamente nessa pasta, causando `JsonSerializationException` — corrigido copiando o arquivo certo do repo. Registrado como memória de projeto (`project_visceralcombat_dual_test_install.md`) pra sessões futuras não repetirem a confusão.
- **Usuário confirmou em raid: "Funcionou!"** — capacete/óculos caem na posição do cadáver, sincronização correta pra todos os peers. `[P-10.3]` fechado. **Importante: usuário deixou explícito que isso valida só o fix do capacete (item 002) — o toggle mestre em si (item 005) ainda não foi testado**, registrado como pendência nova `[P-14.1]`.

**Lições / hipóteses descartadas:**
- **`ilspycmd` direto no assembly do jogo é uma ferramenta legítima quando o dump local tem `DECOMPILE-ERROR`** — não é bypass da regra "usar o dump primeiro" (a regra já lista essa exceção). Rodar `-t "Tipo"` falha se UM método do tipo não decompila (aborta o tipo inteiro); `-il` sem `-t` restritivo dump a assembly inteira (~3.8MB de IL) — melhor buscar a assinatura específica (`grep "NomeDoTipo::NomeDoMetodo"`) no dump de IL depois.
- **Hipótese descartada:** usar o `InventoryController` do atirador (`shot.Player.iPlayer.InventoryController`) diretamente — parecia a escolha "mais correta" semanticamente (quem atirou fez a ação), mas tem o furo do `ObservedInventoryController` acima. Descartada em favor do `InventoryController` do host, que é sempre `HostInventoryController` dado o gate `IsServer || IsSinglePlayer` já existente no método.

**Atividade cronológica:**
1. Usuário perguntou o status do drop de capacete — respondido com o estado real (drop "na morte" funciona, drop "post-mortem" suspenso desde `CR-03-01`).
2. Usuário pediu pra investigar `[P-10.3]`. Decompile de `InteractionsHandlerClass.Throw`/`smethod_17` via `ilspycmd` (IL puro, já que a descompilação em C# falha nesse tipo) + leitura de `HostInventoryController.cs`/`ClientInventoryController.cs`/`ObservedInventoryController.cs`/`BaseInventoryController.cs` do Fika.
3. Apresentado o achado ao usuário em linguagem simples (2 rodadas de pergunta/esclarecimento, incluindo reformular com uma analogia concreta: "o atirador dropa o capacete sem abrir o inventário, igual já acontece hoje com a arma").
4. Usuário aprovou aplicar direto. `LimbKillPatch.cs`: reativadas as 2 chamadas `DropCorpseHeadEquipment(player)` (ramos `HeadOff`/`HeadBurst`); método reescrito pra usar `Singleton<GameWorld>.Instance.MainPlayer.InventoryController` em vez de `helmet.Owner as TraderControllerClass`.
5. Build compilado (`compile-mod.sh VisceralCombat --allow-same-version` pros assemblies não tocados) — 0 erros, mesmos 20 warnings pré-existentes. Versão bumpada `3.10.0` → `3.10.1` (`VisceralEntry.cs` + `.csproj`), rebuild confirmado (hash do `.dll` mudou entre as duas builds arquivadas). Instalado automaticamente em `E:/Tarkov Red Line/BepInEx/plugins/VisceralCombat` pelo script padrão.
6. Usuário reportou erro de log (`JsonSerializationException` em `VisceralEntry.ParseDismembermentJson`) — investigado, achado `VD_Calibers.json` desatualizado (schema antigo) especificamente em `E:\Tarkov Red Line - SERVER TEST` (pasta de teste diferente da apontada por `.spt-path`, mencionada pelo usuário só nesse momento). Corrigido copiando o arquivo certo do repo.
7. Usuário testou a build 3.10.1 em coop: confirmou sincronização correta (sem duplicata/item travado), mas reportou bug de posição — item caía na posição do atirador/host, não do cadáver (exemplo concreto: 100m de distância entre os dois).
8. Usuário perguntou diretamente se dava pra usar o cadáver como referência de posição. Investigação via IL (`ilspycmd`, `ThrowOperationClass.cs`/`TraderControllerClass.cs`) confirmou que sim — `ThrowOperationClass` separa `itemController` (rede) de `player`/`IPlayer` (posição). Novo método `ThrowFromCorpsePosition` criado em `LimbKillPatch.cs`, construindo a operação manualmente.
9. Build falhou (`CS0012`, `Player` implementa `IDissonancePlayer` não referenciado) — corrigido adicionando `<Reference Include="DissonanceVoip">` ao `.csproj` (mesmo padrão já usado por SAIN/FIKA no repo). Rebuild OK, versão `3.10.1` → `3.10.2`. Instalado em `E:\Tarkov Red Line` E `E:\Tarkov Red Line - SERVER TEST` (com confirmação do usuário antes de escrever na segunda pasta).
10. Usuário confirmou em raid: "Funcionou!" — capacete cai na posição do cadáver. `002-drop-arma-capacete-oculos-cabeca-06-fix-01.md` atualizado com os 2 rounds e checklist de validação marcado. `[P-10.3]` fechado.

**Pendências abertas nesta sessão:**
- `[P-10.3]` ✅ Resolvido em 2026-09-20 — mesma sessão, fix em 2 rounds, confirmado pelo usuário em raid.
- `[P-14.1]` (aberta 2026-09-20) 🟡 Item 005 (toggle mestre) — usuário confirmou explicitamente que ainda não testou o toggle em si, só o fix separado de `[P-10.3]`.

**Cross-refs:**
- Resolve `[P-10.3]`, aberta na Sessão 13 (`002-drop-arma-capacete-oculos-cabeca-04-code-review-03.md`, achado `CR-03-01`) — confirmado em raid pelo usuário, fix documentado em `002-drop-arma-capacete-oculos-cabeca-06-fix-01.md`.
- Abre `[P-14.1]` — validação do item 005 (toggle mestre) continua como trabalho futuro, não coberta por este fix.

---

## 2026-09-20 05:21 (GMT-3) — Sessão 13: Toggle Mestre (Item 005) + `/code-review` Rodada 03 do Item 002 — Regressão Real Achada

**Tema central:** Duas frentes na mesma sessão. (1) Usuário relatou que um convidado sem o VisceralCombat viu uma arma "duplicada" — investigação mostrou que `Player.OnDead`/`DropItemDead` roda em qualquer peer que recebe o pacote de morte (`NetworkHealthControllerAbstractClass.method_22` → `DiedEvent`, confirmado por leitura do Assembly), então um peer sem o mod não tem como suprimir a animação vanilla redundante — isso motivou o item `005` (toggle mestre + núcleo de compatibilidade sempre ativo). (2) Usuário autorizou rodar o `/code-review` formal pendente do item 002 (`[P-10.1]`), que revelou uma sessão paralela nunca registrada na memória (2026-09-14) e uma regressão real introduzida por ela.

**Decisões-chave:**
- **Item 005 aprovado com estrutura em 3 camadas:** 6 categorias de config atrás de um toggle mestre novo; "Drop Weapon On Death" **e** "Drop Headwear/Eyewear On Head Dismemberment" com toggle próprio, fora do mestre (decisão final do usuário — inicialmente só a arma, depois capacete/óculos entrou também por "se cai pra um jogador, tem que cair pra todos", mesmo sem ter o mesmo bug técnico de fantasma); núcleo de compatibilidade anti-fantasma sempre ativo, independente de tudo. Usuário confirmou que o launcher TRL Red Line vai sempre distribuir o mod (diferença fica só na config) e que a tela de config do launcher fica bloqueada com o jogo aberto (resolve, sem necessidade de hot-reload, um ponto que tinha ficado marcado `<!-- review: -->`). Spec funcional: `mods/VisceralCombat/backlog/005-toggle-mestre-compat-coop/005-toggle-mestre-compat-coop-01-spec.md`.
- **`/review-spec` do item 005 achou uma contradição interna real** (a spec dizia, em duas seções diferentes, coisas opostas sobre se a arma desligava com o mestre) — corrigida preferindo a decisão mais específica/recente, conforme a regra do `review-spec`. Também achou (via memória) que os patches que substituem o ragdoll inteiro do EFT não têm NENHUM gate de config hoje — vão precisar de um novo quando a spec técnica for escrita.
- **Regressão real achada na rodada 03 de `/code-review` do item 002 (`CR-03-01`, 🔴):** uma sessão paralela (2026-09-14, nunca registrada nesta memória até agora) rodou sua própria `/code-review` (`CR-02-01` a `CR-02-06`) e, entre correções boas, introduziu `LimbKillPatch.DropCorpseHeadEquipment` — capacete/óculos de um cadáver desmembrado **post-mortem** removidos via `item.Owner` (o `TraderControllerClass` do cadáver já criado, `GClass3385`) sem nenhum gate de autoridade. Isso reproduz exatamente o mecanismo que a investigação `CR-NET-LOCK-01` desta sessão (11/09) já tinha provado ser quebrado (item travado/inlootiável), agora multiplicado por rodar em todo peer simultaneamente (sem gate `IsServer`/`IsSinglePlayer`). Desativado (chamadas comentadas) até uma investigação dedicada achar um mecanismo seguro pra remover item de um cadáver **já existente** — diferente do caso "antes da morte" que `CR-NET-LOCK-01` resolveu, aqui não há o mesmo truque de timing disponível.
- **Descoberta lateral importante:** a MESMA sessão paralela de 14/09 já tinha resolvido "bot morre em pé" (hipótese da `[P-10.2]` desta memória) por uma causa raiz **diferente e concreta** — campo de reflexão errado (`PlayerBody_0` → `PlayerBody`) + `NetId` não batendo em coop + falta de desligamento forçado de músculo do `PuppetMaster` fora de alcance. O hardening defensivo (`try/catch`) registrado em `[P-10.2]` continua válido como proteção adicional, mas a causa raiz "de verdade" provavelmente já é essa outra.

**Lições / hipóteses descartadas:**
- **Trabalho paralelo em sessões diferentes pode divergir sem ninguém perceber, se a memória não for consultada/atualizada com disciplina.** A sessão de 14/09 nunca foi registrada em `sessions.md` — só foi descoberta porque a rodada 03 de `/code-review` leu o código atual do zero (não confiou em cache/suposição do que "deveria" estar lá) e achou comentários `CR-02-XX` que não batiam com nada conhecido. Reforça por que `/code-review` deve sempre ler o estado REAL do arquivo, não presumir a partir do histórico de conversa.
- **Um padrão já provado quebrado (`item.Owner` sobre corpo já criado) pode ser reintroduzido em outro lugar do mesmo mod sem ninguém ligar os pontos**, se a decisão de design (não só o código) não estiver documentada de forma que outra sessão/agente a encontre antes de escrever algo parecido. A causa raiz de `CR-NET-LOCK-01` está bem documentada no asbuild do item 002 e nesta memória — mas isso não impediu a regressão, porque a sessão de 14/09 não consultou nenhum dos dois antes de codar. Vale reforçar esse hábito nas próprias skills/checklists deste repo.

**Atividade cronológica:**
1. Usuário relatou arma "duplicada" vista por um convidado sem o mod; investigação no Assembly (`NetworkHealthControllerAbstractClass.method_22`) confirmou que `OnDead`/`DropItemDead` roda em qualquer peer, com ou sem o mod.
2. Usuário sugeriu um mod "lite" separado; alternativa proposta e aceita: toggle mestre único no mesmo mod. `/add-backlog-item` → item 005 criado, `/create-spec` rodado.
3. Usuário pediu lista de todas as features do mod pra decidir o que entra no toggle mestre; levantamento feito direto do código (34 `ConfigEntry` atuais — `PROPRIEDADES.md` estava desatualizado, ainda na v3.7.0).
4. 3 rodadas de decisão com o usuário: quais categorias entram no mestre, exceção da arma, depois exceção do capacete/óculos também. `/review-spec` rodado, achou e corrigiu a contradição interna + adicionou corner cases (incluindo o achado sobre os patches de ragdoll sem gate).
5. Usuário autorizou rodar `/code-review` formal do item 002 (resolve `[P-10.1]`). Leitura do código atual revelou a rodada 02 (14/09) nunca registrada + a regressão `CR-03-01`.
6. `/apply-code-review` rodado — `CR-03-01` (desativação de `DropCorpseHeadEquipment`) e `CR-03-02` (reordenação de checagem) aplicados. Rebuild confirmado, versão `3.9.22`.

**Pendências abertas nesta sessão:**
- [P-10.3] 🟡 — ver bloco "Pendências / próximos passos conhecidos" no topo.
- `[P-10.1]` ✅ Resolvido em 2026-09-20 — `/code-review` rodada 03 + `/apply-code-review` concluídos.
- `[P-10.2]` — não resolvida, mas revisada com nova informação (ver bloco de pendências no topo).

**Cross-refs:**
- Item 005: `mods/VisceralCombat/backlog/005-toggle-mestre-compat-coop/005-toggle-mestre-compat-coop-01-spec.md` — spec funcional completa, aguardando `/create-technical-spec`.
- Item 002: `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-04-code-review-03.md` (achados) e `-05-asbuild.md` (histórico completo, inclusive o trabalho de 14/09 documentado retroativamente).

---

## 2026-09-14 ~12:00 (GMT-3) — Sessão 12: `/code-review` Rodada 02 do Item 002 + Fix de "Bot Morre em Pé" (Sessão Paralela, Registrada Retroativamente em 2026-09-20)

**Tema central:** Sessão paralela (não a mesma linha de conversa das Sessões 9-11/13) — rodou sua própria `/code-review` do item 002 e, separadamente, resolveu o problema de bots mortos ficando em pé no coop. Registrada aqui retroativamente em 2026-09-20 porque nunca tinha sido gravada — só foi descoberta quando a Sessão 13 leu o código atual pra fazer a rodada 03 de `/code-review` e achou referências `CR-02-XX` desconhecidas. Timestamp `~12:00` é estimativa (sem registro original do horário).

**Decisões-chave:**
- **6 achados de code-review aplicados (`CR-02-01` a `CR-02-06`)** — detalhes completos em `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-04-code-review-02.md` e no asbuild do item (entrada 2026-09-14). Resumo: gate de autoridade expandido pra incluir `player.IsYourPlayer` (clientes humanos coop); `ShootOffHelmetPatch` protegido contra rodar em bot já morto; filtro de `EDamageType` balístico/explosivo pro drop de cabeça; **`DropCorpseHeadEquipment` criado** (viria a ser a regressão achada na Sessão 13 — `CR-03-01`); limpeza de `PendingHeadOutcome` no início de raid; guard de item já desanexado fortalecido.
- **"Bot morre em pé" resolvido por causa raiz concreta**, não pela hipótese de hardening defensivo desta mesma memória (`[P-10.2]`, aberta na Sessão 11): campo de reflexão errado (`PlayerBody_0` → `PlayerBody`) em `CreateBSGRagdollPatch.cs`, `NetId` não batendo em `RagdollHelperClass.FindPlayerByNetId` em coop, e falta de desligamento forçado de músculo do `PuppetMaster` fora do alcance de agonia. Ver asbuild do item 002, entrada 2026-09-14.

**Lições / hipóteses descartadas:**
- Nenhuma lição nova capturada no momento — sessão registrada retroativamente só a partir do que os artefatos (code-review-02.md, asbuild) documentam, sem acesso à conversa original.

**Atividade cronológica:** não reconstruível em detalhe (sem acesso à conversa original) — ver Histórico dos artefatos citados acima pra sequência de eventos.

**Pendências abertas nesta sessão:** nenhuma nova — o que ficou pendente (a regressão `CR-02-04`) só foi identificado depois, na Sessão 13.

**Cross-refs:**
- Ver Sessão 13 (2026-09-20) pra como essa sessão foi descoberta e a regressão corrigida.

---

## 2026-09-11 (GMT-3) — Sessão 2026-09-11 (Sessão 11): Hardening Defensivo do Ragdoll (`CreateBSGRagdollPatch`/`RagdollClassPatch`) — Hipótese pra "Bot Morre em Pé"

**Tema central:** Um amigo do usuário relatou (de segunda mão, sem log — "esqueceu de mandar") que **algumas vezes** um bot morto fica em pé, sem cair em ragdoll. Usuário perguntou se pode ser o VisceralCombat e cogitou pedir ajuda ao Gemini pra investigar, já com receio de gastar mais crédito do Claude nesta sessão já longa.

**Decisões-chave:**
- **Achado por leitura de código (sem log):** `CreateBSGRagdollPatch` (Prefix em `Corpse.method_16`) e `RagdollClassPatch` (Prefix em `RagdollClass.Start()`) **substituem o sistema de ragdoll do EFT inteiro** — os dois retornam `false` incondicionalmente (pulam o vanilla sempre) e reimplementam a criação de joints/rigidbodies na mão, via reflexão em campos privados de `Corpse`. **Nenhum dos dois tinha `try/catch`.** Uma linha em particular (`val.massScale = val.connectedBody.mass / val.GetComponent<Rigidbody>().mass;`, sem checar se o componente existe) e qualquer outra falha de reflexão/spawner aborta o método no meio — como o vanilla nunca roda (o `return false` já foi decidido), o corpo fica **sem nenhum ragdoll**, preso na última pose. Isso bate tecnicamente com o sintoma relatado ("morre em pé, algumas vezes" — comportamento intermitente, condizente com uma condição de borda rara nesses spawners).
- **Não é confirmado por log — é uma hipótese bem fundamentada, tratada como tal.** Nenhuma dessas duas classes foi tocada nas sessões anteriores desta mesma investigação (002/`CR-NET-LOCK-01`); é código pré-existente do mod. Registrado explicitamente como não-confirmado (`[P-10.2]`), não fechado como bug resolvido.
- **Fix aplicado é puramente defensivo, nunca deveria piorar nada:** em `RagdollClassPatch`, cada joint/rigidbody agora é criado dentro do próprio `try/catch` do laço — um spawner problemático é pulado (logado) em vez de abortar o ragdoll inteiro (degrada pra "ragdoll parcial" em vez de "nenhum"). Em `CreateBSGRagdollPatch`, se a falha acontece **antes** de atribuir `__instance.Ragdoll`, cai pro vanilla (`return true`, seguro); se falha **depois** (`OnRigidbodyStarted`/`method_19`), mantém o comportamento antigo (`return false`) porque repassar pro vanilla ali criaria um `RagdollClass` duplicado por cima do que já foi montado — não dava pra ser "mais seguro" que isso sem arriscar piorar.
- **Sobre pedir ao Gemini:** avaliado e descartado pra esta tarefa específica — os `/gemini-handoff-*` disponíveis no repo são pra fases de **spec**/**review** do fluxo SDD, não implementação direta; formalizar esse fix pequeno (~diagnóstico já pronto, ~15 linhas em 2 arquivos já lidos) como item de backlog só pra rodar por Gemini teria mais overhead que aplicar direto. Usuário concordou (`AskUserQuestion` → "Eu aplico agora").

**Lições / hipóteses descartadas:**
- **Nem toda investigação precisa de log pra progredir.** Quando o comportamento relatado ("intermitente", "sem crash visível pro usuário") bate com um padrão de código já conhecido (aqui: reimplementação total de um sistema do engine, sem tratamento de erro, retornando `false` incondicional), dá pra propor um fix defensivo sólido só com leitura de código — mas é importante **não fechar a pendência como resolvida** sem confirmação; registrar como hipótese tratada, não como bug corrigido.
- **Fallback "voltar pro vanilla" só é seguro ANTES de qualquer mutação de estado real ter acontecido.** Em ambos os patches, o ponto de decisão "posso devolver pro método original?" dependeu de rastrear exatamente até onde a reimplementação tinha ido — depois de criar rigidbodies/joints reais, nunca é seguro deixar o vanilla rodar por cima (duplicaria). Vale esse mesmo raciocínio pra qualquer Prefix futuro deste mod que retorne `false` (skip completo) sobre um método com efeitos colaterais físicos/Unity.

**Atividade cronológica:**
1. Usuário repassou relato de terceiro (sem log) sobre bot morrendo em pé, perguntou se é o VisceralCombat e cogitou pedir ao Gemini.
2. Investigados os 3 patches de ragdoll registrados no `Awake()` (`CreateCorpsePatch`, `CreateBSGRagdollPatch`, `RagdollClassPatch`) — achado que os 2 últimos substituem o ragdoll do EFT inteiro sem `try/catch`.
3. Apresentada a hipótese ao usuário com proposta de hardening; usuário pediu pra reconsiderar Gemini por causa do crédito; explicado que os handoffs disponíveis são só pra spec/review, e que o fix é pequeno o bastante pra sair mais barato direto. Usuário escolheu aplicar direto.
4. Aplicado `try/catch` por iteração (joints/rigidbodies) em `RagdollClassPatch` e fallback condicional (`return true`/`false` conforme o que já foi mutado) em `CreateBSGRagdollPatch`. Build 3.9.19 → 3.9.20.

**Pendências abertas nesta sessão:**
- [P-10.2] 🟡 — ver bloco "Pendências / próximos passos conhecidos" no topo. **Não confirmado por log real** — aguarda o problema se repetir (ou não) pra fechar.

**Cross-refs:**
- Continuação da investigação do item 002 (mesma sessão de trabalho), mas o achado em si (`CreateBSGRagdollPatch`/`RagdollClassPatch`) é código pré-existente, não relacionado ao escopo do item 002.

---

## 2026-09-11 (GMT-3) — Sessão 2026-09-11 (Sessão 10): `CR-NET-LOCK-01` — Reformulação Completa do Drop + Cadeia de 3 Bugs de "Objeto Visual Órfão"

**Tema central:** Continuação direta do teste do usuário nos itens 002/004 (sessões 9/9b, mesmo dia). O fix de `CR-DUP-01` (usar `item.Owner` em vez de `player.InventoryController`) trocou "duplicado" por algo **pior** em coop — item inlootiável, rollback ao tentar pegar. O usuário, explicitamente, pediu pra **parar de corrigir reativamente** e investigar o mecanismo correto antes de mexer em código de novo. Essa investigação achou a causa raiz real — e ela não tinha nada a ver com qual controller ou qual operação (`ThrowItem`/`Move`) usar.

**Decisões-chave:**
- **Causa raiz real (`CR-NET-LOCK-01`):** o Fika sincroniza o inventário do cadáver em coop via **snapshot**, não via replay de operação. `FikaPlayer.SetupCorpseSyncPacket` (`Fika.Core/Main/Players/FikaPlayer.cs:1497-1557`) serializa `Inventory.Equipment` inteiro e manda pra rede — e isso roda de dentro de `ActiveHealthController.Kill` (`Player.cs:3923-3931`, especificamente `method_35`, `Player.cs:4554`), **antes** de `DiedEvent?.Invoke()` disparar `Player.OnDead`. Ou seja: quando `OnDead` roda (ou qualquer Postfix de `ApplyDamageInfo`, mais tarde ainda), o snapshot já foi montado e mandado — remover o item ali sempre chega tarde demais pros outros peers, não importa o mecanismo usado.
- **Por que não dá pra interceptar antes disso também:** com o player ainda vivo (`IsAlive == true`), tanto `HostInventoryController` quanto `ClientInventoryController` do Fika adiam a operação de inventário pro **próximo frame** (`HandleOperation`: `if (_player.HealthController.IsAlive) await Task.Yield();`) — de novo, tarde demais. A única janela síncrona e correta é **entre** `base.IsAlive = false` e a chamada de `method_35()`, dentro de `Kill()` — só acessível via Prefix em `ActiveHealthController.method_35` (único call site de `Kill`).
- **Unificação:** criado `DeathInventoryDropPatch` (Prefix em `method_35`), cobrindo arma **e** capacete/óculos no mesmo ponto — exatamente o que o usuário pediu ("a questão é descobrir este método... a mesma coisa pra arma também"). `WeaponDropOnDeathPatch` (`OnDead`) e `KillPatch.DropHeadEquipment` (`item.Owner` + `WaitSeconds`) removidos, superados. A decisão de desmembramento de cabeça (que antes só existia no Postfix de `ApplyDamageInfo`) precisou ser adiantada pra dentro desse novo patch (usando `Player.LastDamageInfo`/`LastBodyPart`, campos `protected` já preenchidos nesse ponto — lidos via reflexão) e cacheada (`KillPatch.PendingHeadOutcome`) pro Postfix consumir uma única vez, sem rolar a chance de novo.
- **3 bugs de "objeto visual órfão" em cadeia, cada um achado só depois de testar em jogo o fix anterior:**
  1. **`CR-HEAD-PROP-GHOST-01`:** integrando com `TRL-DynamicSpawn` (feature de converter corpo em "mochila" pra otimizar performance, `CorpseCleanupManager.ConvertToBackpack`/`DestroyCorpse`), props de gore (`val5`/`val7` em `DismemberLimb`) ficavam flutuando no ar depois do corpo sumir — porque eram instanciados **fora** da hierarquia do `Player` (`GetComponentsInChildren`/`Destroy(gameObject)` do outro mod nunca os alcançava). Corrigido com `SetParent(player.gameObject.transform, true)` — zero dependência entre mods, qualquer limpeza de cadáver baseada no padrão Unity padrão passa a funcionar.
  2. **`CR-WEAPON-GHOST-01`:** a arma dropava e era lootável do chão, mas uma segunda versão visual (não interagível) ficava congelada na pose da mão. Causa: `Player.DropItemDead(_handsController.Item, _handsController.ControllerGameObject)` é quem normalmente faz a mão "largar" o modelo — e o guard que pula esse método (pra não duplicar o `LootItem`) nunca avisava esse modelo que devia sumir. Corrigido (nessa hora) com `Object.Destroy(prefab)`.
  3. **`CR-HANDS-DISPOSE-01`:** o fix acima quebrou outra coisa, reportado **por outra sessão** investigando um softlock de extração no Fika — `Object.Destroy(prefab)` destruía o MESMO GameObject que `FirearmController` usa internamente (`_controllerObject`), deixando `firearmsAnimator_0` com uma referência C# não-nula mas um Animator nativo já destruído. No fim da raid, `Player.Dispose() → method_118() → HandsController.Destroy() → FirearmsAnimator.SetBoltCatch() → Animator.SetBool()` crashava com `NullReferenceException`, capturada e **relançada** pelo `CoopGame.Stop()` do Fika — abortando o disposal pra **todos** os jogadores da sala, não só o morto. Corrigido chamando o mesmo caminho oficial que o jogo já usaria (`player.method_118()` + `Object.Destroy(handsController)`, o componente em si) — deixa `HandsController` já `null` quando a raid termina, então o `Destroy()` duplicado nunca roda.

**Lições / hipóteses descartadas:**
- **A lição mais importante desta sessão:** quando um fix reativo piora o sintoma em vez de melhorar (duplicação → item travado/inlootiável), é sinal de que o modelo mental do problema está errado, não de que falta mais um ajuste no mesmo mecanismo. O usuário pediu explicitamente pra parar e investigar o mecanismo real antes de continuar mexendo — e a resposta certa não estava em "qual controller usar" (a pergunta que vinha sendo feita), estava em **timing** relativo a um sistema de rede (Fika) que nenhuma das tentativas anteriores tinha investigado a fundo.
- **Ler o código-fonte do dependente (Fika) resolveu o que ler só o Assembly do jogo não resolvia:** a causa raiz (`SetupCorpseSyncPacket` dentro de `Kill`/`method_35`, antes de `DiedEvent`) só apareceu lendo `Fika.Core.Main.Players.FikaPlayer.cs`/`ClientHealthController.cs`/`BotHealthController.cs`/`HostInventoryController.cs` diretamente — o Assembly decompilado do jogo sozinho não mostra COMO o Fika consome os eventos de morte pra montar pacotes de rede.
- **Padrão que se repetiu 3 vezes nesta sessão:** destruir/remover um objeto que outro sistema (do próprio jogo ou de outro mod) ainda referencia sempre exige achar o caminho de descarte OFICIAL desse sistema (`AssetPoolObject.ReturnToPool`, `player.method_118()`) em vez de um `Object.Destroy` cru — mesmo quando o `Destroy` cru "funciona" visualmente na hora, o problema aparece depois, em outro código que não esperava a referência órfã. Vale desconfiar de qualquer novo `Object.Destroy`/remoção de GameObject deste mod que não passe primeiro pelo método de disposal do dono original do objeto.

**Atividade cronológica:**
1. Usuário reportou: fix de `CR-DUP-01` piorou a situação (item inlootiável, rollback) — pediu pra parar de corrigir reativamente e investigar o mecanismo certo antes de codar.
2. Investigação profunda: `TraderControllerClass.List_0`/`CheckItemAction`/`vmethod_1`, `MoveOperationClass`/`RemoveOperationClass`, e por fim o código-fonte do Fika (`HostInventoryController.HandleOperation`, `FikaPlayer.SetupCorpseSyncPacket`, `ClientHealthController`/`BotHealthController.SendNetworkSyncPacket`) — achada a causa raiz real de timing com o snapshot de sync.
3. Usuário confirmou a direção. Implementado `DeathInventoryDropPatch` (Prefix em `ActiveHealthController.method_35`), unificando arma + capacete/óculos; removidos `WeaponDropOnDeathPatch`/`KillPatch.DropHeadEquipment`. Rebuild 3.9.16. **Usuário testou em coop: duplicação resolvida.**
4. Usuário perguntou se o encolhimento da cabeça também encolhe capacete/óculos (confirmado: sim, mesmo bone `PlayerBones.Head.Original`, `PlayerBody.GetSlotBone`) e se o drop "limpa" isso (confirmado: sim, pela ordem — drop acontece antes do encolhimento agora).
5. Usuário reportou outro bug (mod errado mencionado primeiro, corrigido pra `TRL-DynamicSpawn`): props de desmembramento flutuando após o corpo virar "mochila". Corrigido reparentando os props sob `player.gameObject.transform` (`CR-HEAD-PROP-GHOST-01`). Rebuild 3.9.17.
6. Usuário reportou: arma com "fantasma" visual congelado na pose da mão. Corrigido com `Object.Destroy(prefab)` (`CR-WEAPON-GHOST-01`). Rebuild 3.9.18.
7. Usuário confirmou o fix funcionando, mas reportou (achado em outra sessão) um softlock de extração real em coop com stack trace completo. Investigado `FirearmController.Destroy()` no Assembly — achada a colisão exata com o `Object.Destroy(prefab)` do passo 6. Corrigido usando `player.method_118()` + `Object.Destroy(handsController)` (`CR-HANDS-DISPOSE-01`). Rebuild 3.9.19. **Usuário confirmou em coop: extração funcionando, sem duplicação.**
8. Usuário reportou alguns `NullReferenceException` residuais no log de fim de raid (`Player.get_PointOfView` → `_playerBody` null). Investigado e classificado como **não relacionado** a este mod — `_playerBody`/`RaycastBreacher`/`FikaPlayer` não são tocados por nenhum código do VisceralCombat.

**Pendências abertas nesta sessão:**
- [P-10.1] 🔴 — ver bloco "Pendências / próximos passos conhecidos" no topo.

**Cross-refs:**
- Asbuild atualizado: `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-05-asbuild.md` (seção "Mudanças posteriores", 2026-09-11 — 4 entradas: `CR-NET-LOCK-01`, `CR-HEAD-PROP-GHOST-01`, `CR-WEAPON-GHOST-01`, `CR-HANDS-DISPOSE-01`).
- Continuação direta das Sessões 9/9b (mesmo dia — mesmo teste em jogo do usuário).

---

## 2026-09-11 04:37 (GMT-3) — Sessão 2026-09-11 (Sessão 9b): Fix `CR-HEAD-DUP-01` — "2 Cabeças" no Efeito Estourar

**Tema central:** Continuação do teste em jogo do usuário — segundo bug real encontrado: o efeito "cabeça estourada" (item 004) mostrava a cabeça original intacta **e** o prop `Head_1`/`Head_2` ao mesmo tempo, no mesmo corpo.

**Decisões-chave:**
- **Causa raiz:** `Head_1`/`Head_2` são modelos de cabeça **completos** (confirmado pelas texturas do bundle investigadas numa sessão anterior — `Brain`/`Eyeball`/`Teeth`/`Mouth`), não decorações pequenas de coto como os caps de braço/perna (`Arm_LeftCap`, `gore_leg_torn01`). `BurstHead` (método próprio criado pra "estourar") deliberadamente não escondia a cabeça original — premissa de design da spec funcional 004 ("cabeça real permanece") — então o novo prop renderizava ao lado da cabeça real em vez de no lugar dela. Personagens EFT usam mesh combinado único (sem renderer separado por parte do corpo pra desligar individualmente), então o único jeito comprovado de esconder geometria que este mod já usa é o encolhimento de osso que `DismemberLimb` já faz.
- **Decisão do usuário (mudança de premissa):** abandonar a ideia de "cabeça real visível" pro estourar — "estourar" passa a usar o **mesmo** `DismemberLimb` que "arranca" usa (esconde a cabeça original do jeito já comprovado em jogo), só trocando qual prop aparece no lugar (`Head_3` = coto sem cabeça; `Head_1`/`Head_2` sorteado = cabeça caída/estourada). Método `BurstHead` **removido inteiramente** — simplificação, não só correção (elimina uma segunda implementação paralela que só existia por causa da premissa que não funcionou na prática).
- Revertido também o despacho especial de pacote de rede (`OnDismembermentPacketClient` não precisa mais de caso especial pra "head_burst" — os dois efeitos replicam pelo mesmo caminho de `DismemberLimb` agora).

**Lições / hipóteses descartadas:**
- **Premissa de design refutada por teste em jogo:** "estourar mantém a cabeça original visível, só sobrepõe um efeito" não é tecnicamente viável no motor de skinning deste personagem (mesh único combinado, sem renderer por parte pra desligar) — qualquer variante visual alternativa de uma parte do corpo neste mod precisa passar pelo mesmo mecanismo de "encolher o osso" que já existe, não dá pra "só decorar por cima" quando o asset é um substituto completo (não um adesivo de coto). Vale lembrar disso antes de propor um terceiro efeito de corpo no futuro.
- Reforça a lição já registrada na Sessão 9 (mesmo dia): decisões de design sobre efeito visual só se confirmam testando em jogo — a leitura estática do código (mesmo com investigação cuidadosa de assets/texturas) não pegou este problema antes do teste real.

**Atividade cronológica:**
1. Usuário reportou "2 cabeças no mesmo bot" (uma estourada, uma inteira).
2. Investigado por que `BurstHead` não esconde a cabeça original, e confirmado que os assets `Head_1/2` são cabeças completas, não decorações.
3. Usuário confirmou: usar a mesma lógica de encolhimento do "arranca" pro "estourar" também.
4. Removido `BurstHead`; `KillPatch.cs` (case 0 do `Postfix`) e `LimbKillPatch.cs` (ramo pós-morte) simplificados pra chamar `DismemberLimb` nos dois casos, só trocando o `capAssetName`. Revertido o despacho especial em `VisceralEntry.cs`.
5. Recompilado (versão 3.9.14 → 3.9.15). **Ainda não revalidado em jogo.**

**Pendências abertas nesta sessão:**
- Consolidada em [P-9.1] junto com o fix da sessão anterior (mesmo dia) — ver bloco "Pendências / próximos passos conhecidos" no topo.

**Cross-refs:**
- Asbuild atualizado: `mods/VisceralCombat/backlog/004-reformular-chance-desmembramento-calibre/004-reformular-chance-desmembramento-calibre-05-asbuild.md` (seção "Mudanças posteriores", 2026-09-11).
- Continuação direta da Sessão 9 (mesmo dia — segundo bug do mesmo teste em jogo).

---

## 2026-09-11 03:49 (GMT-3) — Sessão 2026-09-11 (Sessão 9): Fix `CR-DUP-01` — Item Duplicado (Arma/Capacete/Óculos no Chão E no Cadáver)

**Tema central:** Usuário testou os itens 002/003/004 pela primeira vez em jogo e reportou um bug real: a arma e o capacete/óculos dropados na morte/desmembramento apareciam **duplicados** — um exemplar solto no chão e outro ainda visível no inventário do cadáver. Sessão inteira dedicada a achar a causa raiz e corrigir.

**Decisões-chave:**
- **Causa raiz confirmada no Assembly:** `Player.ApplyDamageInfo` dispara `ActiveHealthController.ApplyDamage` (`Player.cs:30480`), que já processa a morte inteira **sincronamente** — incluindo `Player.CreateCorpse()` (`Player.cs:30641/30699-30707`) — **antes** de qualquer Postfix nosso rodar (`KillPatch.Postfix` é Postfix de `ApplyDamageInfo`; `WeaponDropOnDeathPatch` roda em `DropItemDead`, 1 frame depois via `method_98`, Player.cs:30681-30692). A criação do cadáver **troca o dono** da árvore de itens: `Corpse.method_17` (`Corpse.cs:243`) faz `new GClass3385(equipment, ...)` — e `GClass3385 : GClass3384 : TraderControllerClass` é um **novo** `TraderControllerClass` dedicado ao cadáver. `player.InventoryController` (o que o código usava) vira um dono **obsoleto** assim que o cadáver é criado — `ThrowItem` nele ainda consegue spawnar o item solto (efeito colateral visível da operação) mas não remove do dono real, deixando o item duplicado.
- **Correção:** resolver o dono **atual** direto do item (`item.Owner is TraderControllerClass`, `Item.cs:483` — `IItemOwner`), não presumir que é `player.InventoryController`. Aplicado em `WeaponDropOnDeathPatch.cs` e `KillPatch.DropHeadEquipment`. Padrão genérico e correto pra qualquer situação futura de "dropar item de alguém que acabou de morrer" — não é um workaround específico deste bug.

**Lições / hipóteses descartadas:**
- **Lição arquitetural importante (vale pra qualquer patch futuro que mexa em inventário no momento da morte):** nunca presumir que `player.InventoryController` continua sendo o dono correto de um item depois que `Player.CreateCorpse()` já rodou — o EFT troca a posse pra um controller dedicado do cadáver (`GClass3385`) de forma síncrona e sem aviso. A forma robusta de mutar um item nesse contexto é sempre resolver `item.Owner` no momento da chamada, nunca cachear/assumir qual objeto é o dono.
- **Confirma por que isso não foi pego na spec técnica nem no code-review anteriores:** a leitura estática do Assembly não deixava óbvio que `ApplyDamageInfo`→`ApplyDamage` dispara a cadeia de morte de forma síncrona (achado só depois de seguir a cadeia de chamada linha a linha, motivado pelo bug relatado) — reforça que testar em jogo é o critério real de "funciona" (AP-06), não só compilar e a lógica "parecer certa" na leitura.

**Atividade cronológica:**
1. Usuário reportou duplicação de arma e capacete/óculos após primeiro teste em jogo dos itens 002/003/004.
2. Investigada a cadeia `ApplyDamageInfo` → `ApplyDamage` → `OnDead`/`DiedEvent` → `CreateCorpse` → `Corpse.method_17` → `GClass3385` (dono dedicado do cadáver) no Assembly.
3. Corrigido `WeaponDropOnDeathPatch.cs` e `KillPatch.DropHeadEquipment` pra resolver `item.Owner` em vez de `player.InventoryController`.
4. Recompilado (versão 3.9.13 → 3.9.14). **Ainda não revalidado em jogo** — fica como próximo passo imediato.

**Pendências abertas nesta sessão:**
- [P-9.1] 🔴 — ver bloco "Pendências / próximos passos conhecidos" no topo.

**Cross-refs:**
- Asbuild atualizado: `mods/VisceralCombat/backlog/002-drop-arma-capacete-oculos-cabeca/002-drop-arma-capacete-oculos-cabeca-05-asbuild.md` (seção "Mudanças posteriores", 2026-09-11).

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

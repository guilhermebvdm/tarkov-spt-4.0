# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Code Review 02

**Mod:** VisceralCombat  
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)  
**Spec técnica:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)  
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)  
**Data:** 2026-09-14  

> Segunda rodada de Code Review formal, cobrindo a arquitetura unificada de drop síncrono em `ActiveHealthController.method_35` (`DeathInventoryDropPatch`), a interação com o Fika coop, o ciclo de vida dos ragdolls/cadáveres e o isolamento de efeitos pós-morte.

**Memória consultada:** sessões de 2026-09-09 e 2026-09-11 (`sessions.md`).  
**Docs técnicos conferidos:** `spt-antipatterns.md` (AP-02, AP-04, AP-09), `Fika.Core` e `Assembly-CSharp` descompilado.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 6 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | A — Crítico | 🔴 Bloqueador | Gate de autoridade em `DeathInventoryDropPatch` ignora `player.IsYourPlayer`, bloqueando drop síncrono para clientes coop | ✅ Aplicado em 2026-09-14 |
| CR-02-02 | B — Bug latente | 🟠 Forte | `ShootOffHelmetPatch` roda no Postfix pós-morte sem checar `IsAlive`, reintroduzindo duplicação de capacete | ✅ Aplicado em 2026-09-14 |
| CR-02-03 | B — Bug latente | 🟠 Forte | `ResolveAndDropHeadEquipment` não valida `EDamageType`, reavaliando tiros residuais em mortes por sangramento/queda | ✅ Aplicado em 2026-09-14 |
| CR-02-04 | C — Gap vs spec | 🟡 Médio | Desmembramento de cabeça em cadáveres (`LimbKillPatch`) não desanexa capacete/óculos | ✅ Aplicado em 2026-09-14 |
| CR-02-05 | D — Arquitetura | 🟡 Médio | Dicionário `PendingHeadOutcome` não é resetado ao reiniciar raids em `GameStartedPatch` | ✅ Aplicado em 2026-09-14 |
| CR-02-06 | B — Bug latente | 🟢 Menor | Fragilidade do guard `item.Owner != null` em `WeaponDropOnDeathSkipVanillaFlingPatch` | ✅ Aplicado em 2026-09-14 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflexão.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refatoração de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix altamente recomendado; previne regressões de rede ou desyncs severos.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — melhoria defensiva ou cosmética.

---

## Pontos

### CR-02-01 · A — Crítico · 🔴 Bloqueador · ✅ Aplicado em 2026-09-14

**Gate de autoridade em `DeathInventoryDropPatch` ignora `player.IsYourPlayer`, bloqueando drop síncrono para clientes coop**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs:76`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L76)

**Problema:**
```csharp
// Só o host (ou singleplayer) executa a remoção real...
if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;
```
Em uma partida cooperativa com o Fika, para um jogador que não seja o host (um cliente conectado):
1. `FikaBackendUtils.IsServer` é `false`.
2. `FikaBackendUtils.IsSinglePlayer` é `false`.
3. Quando o jogador cliente morre localmente, seu próprio `ClientHealthController.Kill` dispara `method_35`, gerando o `SetupCorpseSyncPacket` local para ser enviado ao servidor e outros peers.
Com a trava anterior, o cliente local abortava o drop da arma e do capacete. O snapshot do cadáver era serializado ainda com os itens anexados, e os itens nunca eram dropados na máquina do cliente antes do sync, impedindo o drop síncrono para clientes humanos.

**Por que importa:** Jogadores humanos que jogam como clientes no Fika não tinham suas armas e capacetes dropados de forma sincronizada ao morrer, gerando desync de inventário entre o host e o client.

**Sugestão:** Expandir o gate para permitir que o cliente local processe seu próprio player:
```csharp
if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer || player.IsYourPlayer)) return;
```

**Resolução:** Aplicado em `DeathInventoryDropPatch.cs`. A autoridade agora contempla o host (`IsServer`), solo SPT (`IsSinglePlayer`) e o próprio jogador local conectado (`player.IsYourPlayer`), permitindo que a serialização do pacote de sync do cadáver seja enviada à rede já sem a arma e capacete.

---

### CR-02-02 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-14

**`ShootOffHelmetPatch` roda no Postfix pós-morte sem checar `IsAlive`, reintroduzindo duplicação de capacete**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs:19-45`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L19-L45)

**Problema:**
`ShootOffHelmetPatch` intercepta como Postfix o método `Player.ApplyDamageInfo`.
Se um bot recebia um tiro letal na cabeça que **não** desmembrava (o capacete deveria ficar no cadáver como loot):
1. `ApplyDamageInfo` executava sincronamente `ActiveHealthController.Kill`, `method_35` e gerava o pacote de sync do cadáver (`SetupCorpseSyncPacket`) com o capacete no slot `Headwear`.
2. Em seguida, o Postfix de `ShootOffHelmetPatch` executava. Como ele não checava se `player.HealthController.IsAlive` era falso, rolava a chance `HelmetShootOffChance` e executava `controller.ThrowItem` no bot já morto.
3. Isso removia o capacete depois que o pacote do cadáver já havia sido disparado na rede, recriando exatamente o bug de duplicação e desync de inventário em tiros fatais na cabeça não desmembrados. Além disso, o patch não possuía gate de autoridade do Fika coop.

**Por que importa:** Desestabilizava o inventário do cadáver e gerava duplicação de itens (loot no chão + no cadáver).

**Sugestão:** Garantir que o patch só atue em bots vivos e com autoridade do host/SP:
```csharp
if (__instance.HealthController == null || !__instance.HealthController.IsAlive) return;
if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;
```

**Resolução:** Aplicado em `ShootOffHelmetPatch.cs`. Adicionados guards explícitos de vida (`__instance.HealthController.IsAlive`) e autoridade coop (`FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer`), garantindo que o patch seja estritamente restrito a combates com bots vivos.

---

### CR-02-03 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-14

**`ResolveAndDropHeadEquipment` não valida `EDamageType`, reavaliando tiros residuais em mortes por sangramento/queda**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs:107-142`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L107-L142)

**Problema:**
`ResolveAndDropHeadEquipment` lia diretamente `Player.LastBodyPart` e `Player.LastDamageInfo`:
```csharp
EBodyPart lastBodyPart = (EBodyPart)_lastBodyPartField.GetValue(player);
if (lastBodyPart != EBodyPart.Head) return;
```
Em C#, o valor default do enum `EBodyPart` é `0` (`Head`). Se um jogador morria por dano ambiental, sangramento (`LightBleeding`/`HeavyBleeding`), desidratação (`Dehydration`), fome ou queda (`Fall`), e `LastBodyPart` era 0 ou continha um tiro antigo residual na cabeça, o método tentava resolver dano balístico passado e podia arrancar/dropar o capacete mesmo em uma morte por sangramento na perna ou queda de altura.

**Por que importa:** Jogadores e bots podiam perder o capacete no chão ao morrer de desidratação, sangramento ou queda.

**Sugestão:** Passar o `damageType` de `method_35` para `ResolveAndDropHeadEquipment` e abortar caso o dano não seja balístico ou explosivo.

**Resolução:** Adicionado método `IsBallisticOrExplosiveDamage(EDamageType damageType)` filtrando via máscara de bits (`Bullet`, `Sniper`, `GrenadeFragment`, `Explosion`, `Landmine`, `Artillery`, `Btr`, `ThermobaricExplosion`, `Blunt`). Se a causa mortis não for traumática/balística, o drop de cabeça é abortado e `KillPatch.PendingHeadOutcome[player.Id]` é fixado em `None`.

---

### CR-02-04 · C — Gap vs spec · 🟡 Médio · ✅ Aplicado em 2026-09-14

**Desmembramento de cabeça em cadáveres (`LimbKillPatch`) não desanexa capacete/óculos**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:253-270`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L253-L270)

**Problema:**
No branch de tiros em cadáveres (`Dead corpses branch`), quando um projétil atingia a cabeça de um corpo e resultava em `HeadOff` ou `HeadBurst`, `DismemberLimb` era chamado e escalava a cabeça original para zero.
Porém, se o cadáver ainda possuía capacete ou óculos (porque não desmembrou na morte inicial), os itens de cabeça continuavam associados ao slot do cadáver, gerando incongruência visual (itens flutuando ou presos em um pescoço decapitado).

**Por que importa:** Quebrava o critério de imersão onde cabeças desmembradas devem soltar seus respectivos capacetes e óculos.

**Sugestão:** Implementar `DropCorpseHeadEquipment(Player player)` resolvendo o `TraderControllerClass` do `item.Owner` do cadáver (`GClass3385`) e chamando `ThrowItem`.

**Resolução:** Criado `DropCorpseHeadEquipment(Player player)` em `LimbKillPatch.cs`, chamado após `DismemberLimb` nos ramos `HeadOff` e `HeadBurst`. O método resolve `helmet?.Owner as TraderControllerClass` e executa `ThrowItem`, liberando os equipamentos no chão do raid.

---

### CR-02-05 · D — Arquitetura · 🟡 Médio · ✅ Aplicado em 2026-09-14

**Dicionário `PendingHeadOutcome` não é resetado ao reiniciar raids em `GameStartedPatch`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:158`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L158) e [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs:33`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/GameStartedPatch.cs#L33)

**Problema:**
`KillPatch.PendingHeadOutcome` armazena decisões calculadas em `DeathInventoryDropPatch` para serem consumidas no Postfix de `ApplyDamageInfo`. Se um jogador morresse de uma forma atípica onde o Postfix não era executado ou em edge-cases de desconexão, a entrada em `PendingHeadOutcome` permanecia em memória. `GameStartedPatch` limpava `MultiProjectileMomentum`, mas não possuía chamada para limpar `PendingHeadOutcome`.

**Por que importa:** Risco de acúmulo gradual de referências de IDs entre raids sucessivas.

**Sugestão:** Criar `KillPatch.ClearPendingHeadOutcomes()` e chamá-lo em `GameStartedPatch.Postfix`.

**Resolução:** Adicionado `ClearPendingHeadOutcomes()` em `KillPatch.cs` e chamada correspondente em `GameStartedPatch.Postfix()`, alinhado com o ciclo de limpeza de início de raid.

---

### CR-02-06 · B — Bug latente · 🟢 Menor · ✅ Aplicado em 2026-09-14

**Fragilidade do guard `item.Owner != null` em `WeaponDropOnDeathSkipVanillaFlingPatch`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs:61`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/WeaponDropOnDeathPatch.cs#L61)

**Problema:**
A condição anterior era:
```csharp
if (item.Owner != null && item.Owner != __instance.InventoryController)
```
Se o `ThrowItem` anterior já tivesse nulificado `item.Owner` ou desanexado o item (`item.CurrentAddress == null`), a cláusula `item.Owner != null` avaliava para `false`, caindo em `return true` e fazendo o vanilla tentar processar um item já desanexado.

**Por que importa:** Robustez preventiva em cenários onde a desacoplagem do inventário limpa a referência de dono antes da chamada de `DropItemDead`.

**Sugestão:** Alterar para:
```csharp
if (item.Owner != __instance.InventoryController || item.CurrentAddress == null)
```

**Resolução:** Aplicado em `WeaponDropOnDeathPatch.cs`. A checagem agora considera desanexado qualquer item cujo dono não coincida com o inventory controller atual ou cujo `CurrentAddress` seja nulo.

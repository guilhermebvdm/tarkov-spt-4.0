# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.1 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). FIKA handshake de presença de mod implementado e buildando. Feature de desmembramento de bot vivo (LivingDismemberment) gated pela flag `AllPlayersHaveVisceralCombat`. Próxima etapa: implementar `LivingDismembermentController.cs`.
- **Origem dos Bugs de Gigantismo/Inflamento:** Todos os episódios de "gigantismo" e "corpo inflando" foram causados por código experimental no `modded/` durante a refatoração de agonia/músculos. Corrigidos por nós — **não vieram do mod original.**
- **Pendências:** 🟢 Nenhuma pendência blocker ou alta aberta. Handshake FIKA implementado e commitado; LivingDismembermentController ainda não existe (próxima sessão).

---

## Sessão 2026-08-10 (continuação) — Handshake FIKA para LivingDismemberment

### Investigação da Feature 001 (Desmembramento de Bot Vivo)
- **Prone forçado:** `BotMover.DoProne(true)` + `SetPose(0f)` — API nativa do EFT confirmada em `BotMover.cs:L383-390`.
- **Bloqueio de GetUp:** sobrescrever `BotLay.NextPosibleGetUp = Time.time + 999f` em `Update()` no `LivingDismembermentController`.
- **Sangramento (exsanguição):** `player.ActiveHealthController.ApplyDamage(leg, dmg, GClass3051.HeavyBleedingDamage)` — `GClass3051.cs:L40`.
- **FIKA mod-check:** mecanismo nativo do FIKA (`ClientService._requiredMods`, rota `/fika/client/check/mods`) é para bloquear join; **não serve** para verificação runtime in-raid. Optamos por handshake C# customizado.

### Implementação do Handshake FIKA de Presença do Mod
- **`VisceralHandshakePacket.cs`** (novo): packet bidirecional. `IsRequest=true` = host→clientes (ping). `IsRequest=false` = cliente→host (ACK) com `ResponderNetId`.
- **`VisceralEntry.AllPlayersHaveVisceralCombat`** (flag estática): `false` por padrão, `true` apenas quando todos confirmam.
- **`VisceralEntry.StartVisceralHandshake()`**: chamado pelo host/solo no `OnGameStarted`. Solo SPT → flag imediata `true`. FIKA → broadcast ping + coroutine 5s → avalia ACKs vs `CoopHandler.AmountOfHumans - 1`.
- **`GameStartedPatch.cs`**: chama `StartVisceralHandshake()` se `!FikaBackendUtils.IsClient`.
- **Clientes sem o mod:** não registram o packet → não respondem → ACK faltando → feature OFF para todos. **Sem crash.**
- **Build:** ✅ 0 erros após adicionar `using Fika.Core.Networking.LiteNetLib`, corrigir `FikaServer.NetManager` inexistente e adicionar `using Fika.Core.Main.Utils` em `GameStartedPatch`.

---

## Sessão 2026-08-10 — Estilização de Sangue Escuro, SPY de Shader & Backlog 001

### Estilização de Sangue Escuro Coagulado & Remoção de Brilho Branco
- **Descoberta do SPY:** Dois shaders distintos nos efeitos de esguicho:
  - `Particles/VD 3D Blood Shader V14` → sub-partículas; responde a `_Color`, `_TintColor`.
  - `Legacy Shaders/Particles/Alpha Blended Premultiply` → filamento raiz; brilho = alpha alto + premultiplicação.
  - Escopo errado: `transform.root` subia até o personagem inteiro.
- **Fix Final (`ApplyDarkCoagulatedBloodFx`):** Escopo corrigido para `ps.gameObject`. Tratamento bifurcado por shader. SPY removido.

### Backlog 001 — Desmembramento de Perna em Bots Vivos
- **Spec criada:** `mods/VisceralCombat/backlog/001-alive-leg-dismemberment/001-alive-leg-dismemberment-01-spec.md`
- **Status:** 🔵 Investigado + Handshake implementado. Aguarda `LivingDismembermentController`.

---

## Sessão 2026-08-07 — Execução do Refactor, Build Clean 3.7.1 e Aplicação do Code Review 01
- Refatoração concluída de `PlayerInitPatch`, `ShellCasingPatch`, `PhysicalItemsPatch`, `KillPatch`.
- Code Review 01 aplicado (CR-01-01 a CR-01-05): callbacks protegidos, cache estático, destruição de `AnimatorOverrideController` anterior, pool de gore conectado.

---

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- Code-review minucioso: 15+ gargalos de FPS, vazamentos de RAM, `async void`, propriedades F12 placebo.
- Entregável: roadmap de refatoração em `docs/refactor-roadmap.md`.

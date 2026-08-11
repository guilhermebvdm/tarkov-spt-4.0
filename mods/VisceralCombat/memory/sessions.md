# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.2 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Feature de desmembramento de perna em bots vivos (`LivingDismembermentController`) totalmente implementada e compilada. Handshake FIKA garante gating de segurança.
- **Origem dos Bugs de Gigantismo/Inflamento:** Todos os episódios de "gigantismo" e "corpo inflando" foram causados por código experimental no `modded/` durante a refatoração de agonia/músculos. Corrigidos por nós — **não vieram do mod original.**
- **Pendências:** 🟢 Nenhuma pendência aberta. Fase 5 do roadmap concluída.

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

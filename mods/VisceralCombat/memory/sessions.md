# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.2 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Feature de desmembramento de perna em bots vivos (`LivingDismembermentController`) totalmente entregue e testada. Física de impacto em ragdolls unificada sob momento linear universal ($p = m \cdot v$).
- **Origem dos Bugs de Gigantismo/Inflamento:** Todos os episódios de "gigantismo" e "corpo inflando" foram causados por código experimental no `modded/` durante a refatoração de agonia/músculos. Corrigidos por nós — **não vieram do mod original.**
- **Pendências:** 🟢 Nenhuma pendência aberta. Item 001 (`001-alive-leg-dismemberment`) concluído e entregue.

---

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

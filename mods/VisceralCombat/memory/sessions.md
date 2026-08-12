# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.2 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Físicas de impacto reduzidas por fator universal de 0.25x. Tiros fatais de calibres pesados (>= 5 N.s) bloqueiam animação de agonia. Dano de exsanguição ajustado para 20f HP/s. Materiais originais de sangue preservados intactos. Nuvem de sangue de impacto (vanilla) ajustável via F12. Tiros em placas de colete/capacete substituem sangue por faíscas metálicas nativas.
- **Pendências:** 🟢 Nenhuma pendência aberta.

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

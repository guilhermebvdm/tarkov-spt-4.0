# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.9.10 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Arquitetura de Wake on Hit implementada para cadáveres em repouso cinemático (0% CPU). Eliminação de travamentos de CPU em granadas (deduplicação de corpos e rigidbodies). Proteção de `SupportRigidbody` contra duplicações na lista do EFT. Ancoragem de esguichos e jatos arteriais ao osso físico em movimento. Eliminação de tropeços/deslizes involuntários do PMC ao tomar tiro (proteção de entidades vivas em `BodiesImpulsePatch`). Intangibilidade de partículas de sangue em personagens (`ConfigureBloodParticleCollision`). Emissão de poças reais de ambiente no chão (`EmitBloodOnEnvironment` a 0.15s). Eliminação do teleporte em pé de bots deitados (substituição por `Flail_Loop` no chão e ragdoll natural).
- **Pendências:** 🟢 Nenhuma pendência aberta.

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

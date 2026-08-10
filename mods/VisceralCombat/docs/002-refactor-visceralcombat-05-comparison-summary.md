# Resumo Cronológico do Desenvolvimento: VisceralCombat (Sessão Atual)

Este documento resume de forma concisa todos os problemas investigados, diagnósticos e correções aplicadas ao longo de toda esta sessão de trabalho.

---

## 🎯 1. Resolução do Gigantismo no Início da Agonia
- **Problema:** Ao amputar perna/braço e iniciar a animação de agonia, o bot inflava por 1 segundo.
- **Causa:** O bot não era adicionado a `dismemberedPlayers` no frame 0, fazendo a camada de agonia (`Layer 18`) levar 1 segundo para atingir o peso `1.0f` e permitindo ao `PuppetMaster` esticar o osso amputado.
- **Solução (`b2613869`):** Movida a adição `dismemberedPlayers.Add(p)` para o topo do `DeathSetup`, forçando peso `1.0f` instantâneo no frame 0.

---

## 🎯 2. Correção no Match de Músculos & Keywords
- **Problema:** Braço/perna oposta desligava incorretamente ou inflava.
- **Causa:** A keyword `"rarm"` dava match falso com `humanl`**`upperarm`** (braço esquerdo).
- **Solução (`a3adf3ef`):** Substituição por keywords canônicas exatas (`humanrupperarm`, `humanlupperarm`, etc.) e verificação em dupla hierarquia (`m.target`, `m.joint`, `m.rigidbody` e `m.name`).

---

## 🎯 3. Fim do Salto em T-Pose ao Atirar na Agonia
- **Problema:** Ao atirar em um bot executando animação de agonia no chão, ele se levantava em pé (T-pose) antes de cair morto.
- **Causa:** Desativação simultânea do `PuppetMaster` e do `Animator` no mesmo frame do tiro.
- **Solução (`767f727f`):** Criação da função `InterruptAgony`, que zera `pinWeight`, `muscleWeight` e `muscleSpring`, mantendo `mappingWeight = 1.0f` por 3s para o corpo desabar naturalmente via gravidade física a partir da pose atual.

---

## 🎯 4. Fim do Inflamento ao Cancelar Agonia por Tiro
- **Problema:** Ao atirar no bot em agonia, ele caía no chão mas inflava (gigantismo).
- **Causa:** 
  1. O `PuppetMaster.State.Dead` resetava `mappingWeightMlp = 1f` nos músculos amputados.
  2. Zerar o peso da Layer 18 fazia o `Animator` da Unity rodar a Layer 0 (pose em pé do bot vivo com escala 1.0f).
- **Solução (`c9868850` / `328a0c89`):**
  1. Marcado `m.state.isDisconnected = true` em músculos amputados para forçar o solver a ignorar os ossos.
  2. Substituído por `p.BodyAnimatorCommon.enabled = false;`, desligando o Animator completamente e forçando escala `0.001f` via `DismemberedLimbScaler`.

---

## 🎯 5. Proteção de Tiros na Cabeça de Cadáveres
- **Problema:** Atirar na cabeça de um cadáver no chão fazia o corpo do bot desaparecer do mapa.
- **Causa:** O tiro na cabeça acionava a desativação do `PuppetMaster` e escalava `Base HumanHead` de mortos. No EFT, a malha de pele do tronco de alguns bots está ancorada na cadeia da cabeça, encolhendo o corpo inteiro.
- **Solução (`ad289fff`):** Bloqueada a desativação do `PuppetMaster` em mortos (`mappingWeight <= 0.05f`), mantendo a física de impacto sem encolher o osso raiz da malha gráfica.

---

## 🎯 6. Desmembramento Pós-Morte em Corpos Mortos
- **Problema:** Atirar em braços ou pernas de um corpo no chão não desmembrava nada.
- **Causa:** O EFT desativa o `Player.ApplyDamageInfo` em mortos, e o `LimbKillPatch` usava `GetComponentInChildren` sem `includeInactive: true`, ignorando cadáveres com componentes desativados.
- **Solução (`cec73e98`):** Adicionado `includeInactive: true` na busca de componentes e conectados os disparos balísticos (`BallisticsCalculator.Shoot`) de membros diretamente à função `KillPatch.DismemberLimb`.

---

## 🎯 7. Otimizações de RAM, CPU & BepInEx F12 Binds
- **PhysX Fix:** Destruídas juntas PhysX (`Object.Destroy(j)`) em membros amputados para impedir travamentos de FPS por âncoras multiplicadas por 1000x.
- **RAM Leaks:** Cache de buscas por componentes e limpeza de referências em `deadPlayers`/`dismemberedPlayers`.
- **Menu F12 Binds:** Mapeados 100% dos sliders e toggles de multiplicadores de força com `ConfigEntry`.

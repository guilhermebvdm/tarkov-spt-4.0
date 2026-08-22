# Resumo Cronológico do Desenvolvimento: VisceralCombat (Sessão Atual)

Este documento resume de forma concisa todos os problemas investigados, diagnósticos e correções aplicadas ao longo de toda esta sessão de trabalho.

---

> [!IMPORTANT]
> **Esclarecimento sobre Gigantismo & Inflamento:**
> Todos os episódios de "gigantismo" e "corpo inflando" observados durante a sessão foram causados exclusivamente por inconsistências introduzidas temporariamente no próprio código **`modded`** (tentativas de desabilitar/destruir componentes, zerar pesos de animação sem desativar o Animator, ou resets de escala nos músculos pelo `PuppetMaster.State.Dead`). **Nenhum desses problemas foi herdado do mod original.** Todos foram identificados e **100% corrigidos** no repositório.

---

## 🎯 1. Resolução do Gigantismo no Início da Agonia
- **Problema:** Ao amputar perna/braço e iniciar a animação de agonia, o bot inflava por 1 segundo.
- **Causa (Modded):** O bot não era adicionado a `dismemberedPlayers` no frame 0, fazendo a camada de agonia (`Layer 18`) levar 1 segundo para atingir o peso `1.0f` e permitindo ao `PuppetMaster` esticar o osso amputado.
- **Solução (`b2613869`):** Movida a adição `dismemberedPlayers.Add(p)` para o topo do `DeathSetup`, forçando peso `1.0f` instantâneo no frame 0.

---

## 🎯 2. Correção no Match de Músculos & Keywords
- **Problema:** Braço/perna oposta desligava incorretamente ou inflava.
- **Causa (Modded):** A keyword `"rarm"` dava match falso com `humanl`**`upperarm`** (braço esquerdo).
- **Solução (`a3adf3ef`):** Substituição por keywords canônicas exatas (`humanrupperarm`, `humanlupperarm`, etc.) e verificação em dupla hierarquia (`m.target`, `m.joint`, `m.rigidbody` e `m.name`).

---

## 🎯 3. Fim do Salto em T-Pose ao Atirar na Agonia
- **Problema:** Ao atirar em um bot executando animação de agonia no chão, ele se levantava em pé (T-pose) antes de cair morto.
- **Causa (Modded):** Desativação simultânea do `PuppetMaster` e do `Animator` no mesmo frame do tiro.
- **Solução (`767f727f`):** Criação da função `InterruptAgony`, que zera `pinWeight`, `muscleWeight` e `muscleSpring`, mantendo `mappingWeight = 1.0f` por 3s para o corpo desabar naturalmente via gravidade física a partir da pose atual.

---

## 🎯 4. Fim do Inflamento ao Cancelar Agonia por Tiro
- **Problema:** Ao atirar no bot em agonia, ele caía no chão mas inflava (gigantismo).
- **Causa (Modded):** 
  1. O `PuppetMaster.State.Dead` resetava `mappingWeightMlp = 1f` nos músculos amputados.
  2. Zerar o peso da Layer 18 fazia o `Animator` da Unity rodar a Layer 0 (pose em pé do bot vivo com escala 1.0f).
- **Solução (`c9868850` / `328a0c89`):**
  1. Marcado `m.state.isDisconnected = true` em músculos amputados para forçar o solver a ignorar os ossos.
  2. Substituído por `p.BodyAnimatorCommon.enabled = false;`, desligando o Animator completamente e forçando escala `0.001f` via `DismemberedLimbScaler`.

---

## 🎯 5. Proteção de Tiros na Cabeça de Cadáveres
- **Problema:** Atirar na cabeça de um cadáver no chão fazia o corpo do bot desaparecer do mapa.
- **Causa:** O tiro na cabeça acionava a desativação do `PuppetMaster` e colapsava o `Base HumanHead`.
- **Solução (`ad289fff` / `cbcf2634`):** Confirmado via SPY que `Base HumanHead` é filho de `Base HumanNeck` e seguro para desmembrar. O problema de sumir era a desativação forçada do `PuppetMaster` em cadáveres. Com o fix do `PuppetMaster` mantido ativo no ragdoll, o desmembramento de cabeça em corpos mortos passou a funcionar 100% sem sumir nem inflar.

---

## 🎯 6. Desmembramento Pós-Morte em Corpos Mortos (Braços, Pernas e Cabeça)
- **Problema:** Atirar em braços, pernas ou cabeça de um corpo no chão não desmembrava nada.
- **Causa:**
  1. O `BodyPartCollider` não fica ativo nos colliders de física de um cadáver (`Base HumanRThigh1`, `Base HumanHead`, etc.).
  2. O parâmetro `out chance` na busca de calibres zerava a variável (`0.0f`) quando a munição não batia exatamente no dicionário de calibres, tornando a chance 0%.
- **Solução (`a485df99` / `cbcf2634` / `bb92b4fb`):**
  1. **Estratégia Dupla em `LimbKillPatch.cs`:** Usa `BodyPartColliderType` se disponível (bots vivos) ou fallback por string de Rigidbody física (`Base Human[L/R/Head]`) para corpos mortos.
  2. **Fix de Calibres:** Corrigida a lógica de `TryGetValue` com variável temporária `foundChance`, mantendo os `50%` padrão caso o calibre não seja encontrado.
  3. **Suporte a Cabeça:** Incluído o caso `humanhead` / `humanskull` mapeando para `bone = "head"` e cap `Head_N`.

---

## 🎯 7. Otimizações de RAM, CPU & BepInEx F12 Binds
- **PhysX Fix:** Destruídas juntas PhysX (`Object.Destroy(j)`) em membros amputados para impedir travamentos de FPS por âncoras multiplicadas por 1000x.
- **RAM Leaks:** Cache de buscas por componentes e limpeza de referências em `deadPlayers`/`dismemberedPlayers`.
- **Menu F12 Binds:** Mapeados 100% dos sliders e toggles de multiplicadores de força com `ConfigEntry`.

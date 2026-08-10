# Comparativo Técnico: VisceralCombat (Original vs. Modded v3.8.1+)

| Categoria | Versão Original | Versão Modded (Atual) | Impacto / Benefício |
| :--- | :--- | :--- | :--- |
| **Agonia + Desmembramento** | Incompatível (se desmembrava, a agonia falhava ou o bot ficava gigante/quebrado). | **Suporte Total (Braço/Perna):** Desmembra o membro e executa a animação de agonia no corpo restante. | Manutenção da imersão sem perder o efeito gore de amputação. |
| **Desmembramento de Cabeça** | Podia tentar rodar agonia em esqueleto com crânio zerado (`0.001f`). | **Ragdoll Direto:** Decapitação bypassa a agonia e envia o bot direto para o ragdoll limpo. | Previne deformações bizarras no pescoço/crânio. |
| **Interrupção de Agonia por Tiro** | Animação continuava tocando após tiros ou resetava o bot para a T-pose em pé antes de cair. | **Interrupção Suave (`InterruptAgony`):** Zeradas molas e pins; o bot desaba molhado a partir da posição atual no chão. | Elimina o salto em pé ao levar tiros durante a agonia. |
| **Gigantismo (Animator Override)** | O `Animator` da Unity resetava a escala `0.001f` do membro para `1.0f` durante a animação. | **`DismemberedLimbScaler`:** Escalonamento mantido via `Update()`, `OnAnimatorMove()` e `LateUpdate()`. | Elimina 100% dos flashes de gigantismo no desmembramento. |
| **Gigantismo (PuppetMaster Solver)** | O solver `Map` continuava puxando a física do membro reduzido, criando distorções. | **`isDisconnected = true`:** Zerados `props` e `state` (`mappingWeightMlp = 0`). Músculo 100% ignorado pelo solver. | Zero deformação de malha no esqueleto. |
| **Desmembramento Pós-Morte** | Inexistente (tiros em cadáveres não desmembravam nada pois o EFT desliga `ApplyDamageInfo`). | **Totalmente Habilitado:** `LimbKillPatch` (via `BallisticsCalculator.Shoot` com `includeInactive: true`) realiza desmembramentos em cadáveres. | Possibilidade de desmembrar braços e pernas de corpos no chão. |
| **Tiro na Cabeça em Cadáveres** | O corpo inteiro desaparecia ao tomar um tiro na cabeça estando morto. | **Protegido:** Mantida a integridade do nó raiz da pele e desativada a ocultação do `PuppetMaster` em mortos. | O cadáver permanece visível e reage fisicamente ao tiro. |
| **Física de Corpos Mortos** | Corpos mortos podiam voar longe ao levar tiro (multiplicadores descalibrados). | **Cap de Impulso (25%):** Impulso ajustado com multiplicadores F12 calibrados. | Reação física realista sem lançar corpos pelo ar. |
| **Resolução de Músculos** | Keyword `"rarm"` dava match falso em `humanl`**`upperarm`** (desligava o braço errado). | **Keywords Canônicas Exatas:** Corrigidas as strings (`humanrupperarm`, `humanlupperarm`, etc.) e dupla hierarquia. | Zero interferência entre o lado esquerdo e direito do corpo. |

---

## 🛠️ Detalhamento dos Componentes Modificados

### 1. `KillPatch.cs`
- Injeção da checagem de `isFirstDeath` e adição de `dismemberedPlayers`.
- Desativação limpa de juntas PhysX (`Object.Destroy(j)`) no desmembramento para evitar explosões 1000x nas âncoras.
- Suporte a busca em dupla hierarquia (`m.target == val || m.joint.transform == val || m.name == val.name`).

### 2. `RagdollHelperClass.cs`
- Criação do método `InterruptAgony(p, pm)` para desligamento imediato de molas e pins sem reset em T-pose.
- Ajuste do `SetLayerWeight(18, 1f)` no frame 0 quando o bot está em `dismemberedPlayers`.
- Criação do componente `DismemberedLimbScaler` em `Update()`, `OnAnimatorMove()` e `LateUpdate()`.

### 3. `LimbKillPatch.cs`
- Adicionado desmembramento pós-morte baseado em balística (`BallisticsCalculator.Shoot`).
- Uso de `GetComponentInChildren<PuppetMaster>(true)` para incluir componentes inativos em cadáveres.
- Proteção para tiros na cabeça de mortos não desativarem a malha do personagem.

# Relatório de Code Review — Item 07: Sistema Realista de Torniquetes e Necrose por Tempo

> **Módulo:** `TRL-ImmersiveCombatMedicine`  
> **Workspace:** [`modded-testchannel`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel)  
> **Funcionalidade:** Item 07 · Sistema Realista de Torniquetes e Necrose por Tempo  
> **Status:** 🟡 Aprovado com Observações Arquiteturais (0 Bloqueadores 🔴, 1 Importante 🟠, 3 Menores 🟡, 1 Melhoria 🟢)  
> **Data:** 2026-08-15  

---

## 1. Escopo e Arquivos Analisados

- [`Patches/Medical/TourniquetManager.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/TourniquetManager.cs) (213 linhas)
- [`Patches/Medical/MedicalLogic.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs) (Linhas 111–117 e 313–360)
- [`Patches/Medical/BandAidUI.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidUI.cs) (Linhas 1139–1142)
- [`Helpers/ItemDatabase.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/ItemDatabase.cs) (Linhas 56–57)

---

## 2. Visão Geral da Arquitetura & Fluxo

```mermaid
flowchart TD
    A[Aplicação de Esmarch / CAT] --> B{Sistema Realista Ativo?}
    B -- Desativado (Modo Atual) --> C[Remove HeavyBleed Imediato + Consome Item (Vanilla)]
    B -- Ativo (TourniquetManager) --> D[Registra TourniquetData no Membro]
    D --> E[Timer de 30s: Dano de Necrose 5 HP por Tick]
    E --> F{HP do Membro <= 30%?}
    F -- Sim --> G[Aviso de Risco de Necrose Iminente]
    F -- Não --> H[Continua Monitorando]
    E --> I{HP do Membro <= 0?}
    I -- Sim --> J[Membro Destruído por Necrose]
    D --> K[Ação do Médico: Remover Torniquete]
```

---

## 3. Validação Cruzada com as Referências Oficiais (EFT, FIKA e SPT)

### 3.1. Validação com `references/eft-decompiled` (EFT 0.16.9)
- **Aplicação de Dano Contínuo:** Verificado em [`Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs). A chamada `activeHc.ApplyDamage(bodyPart, DAMAGE_PER_TICK, damageInfo)` com `DamageType = EDamageType.Undefined` aplica dano direto ao membro sem disparar efeitos sonoros de impacto balístico nem causar estilhaços secundários.
- **Modo Atual (Vanilla-Safe):** No momento, o fluxo principal em `MedicalLogic.cs:111` mantém o torniquete como estancador de uso único padrão vanilla, o que garante 100% de compatibilidade e zero risco de regressões mecânicas em raids.

### 3.2. Validação com `references/fika-plugin` (Fika.Core 2.3.4)
- **Isolamento de Dano no Paciente Local:** `TourniquetManager.cs:151` checa se o paciente possui `ActiveHealthController` (`activeHc`), evitando aplicar dano de necrose em instâncias de `NetworkHealthController` (o dano de necrose é processado localmente no cliente dono do corpo).

### 3.3. Validação com `references/fika-headless`
- Não afeta servidores headless enquanto mantido em modo vanilla.

### 3.4. Validação com `references/spt-source`
- O consumo dos itens de torniquete (Esmarch `5e831507ea0a7c419c2f9bd9`, CAT `60098af40accd37ef2175f27`) via `ConsumeSafe` é persistido de forma segura no profile pelo pipeline nativo do SPT.

---

## 4. Avaliação Detalhada por Critério

### 4.1. Limitação Arquitetural Mapeada para o Futuro
- **Chave de Rastreamento Singular (🟠 Importante):**
  - No `TourniquetManager.cs:29`, o dicionário utiliza `Dictionary<EBodyPart, TourniquetData> _activeTourniquets`.
  - Se múltiplos jogadores (ex: o médico e um aliado) aplicarem torniquete no mesmo membro (ex: `LeftLeg`), as chaves colidirão.
  - **Solução Recomendada para quando o sistema for ativado:** Alterar a chave para tupla composta `(string profileId, EBodyPart bodyPart)` ou dicionário aninhado por jogador.

### 4.2. Desempenho e Alocações de GC
- **Alocação de Chaves no Update:** `_activeTourniquets.Keys.ToList()` no `Update()` (`L129`) gera uma nova lista a cada frame se houver torniquetes ativos. Pode ser substituído por iteração direta em `KeyValuePair` ou array estático pré-alocado.

---

## 5. Tabela de Achados e Recomendações

| ID | Severidade | Arquivo / Linha | Descrição | Sugestão / Solução |
| :--- | :--- | :--- | :--- | :--- |
| **CR07-01** | 🟠 Importante | `TourniquetManager.cs:29` | `_activeTourniquets` indexado apenas por `EBodyPart` sem `ProfileId`. | Mudar para `Dictionary<(string ProfileId, EBodyPart Part), TourniquetData>` para suportar múltiplos jogadores simultâneos no coop FIKA. |
| **CR07-02** | 🟡 Menor | `TourniquetManager.cs:129` | `_activeTourniquets.Keys.ToList()` aloca lista de GC a cada frame. | Reutilizar uma lista de chaves ou usar buffer fixo indexado. |
| **CR07-03** | 🟡 Menor | `TourniquetManager.cs:14` | Namespace `namespace Band_Aid` divergente. | Padronizar para `TRLImmersiveCombatMedicine`. |
| **CR07-04** | 🟡 Menor | `TourniquetManager.cs:101` | `RemoveTourniquet` remove o tracking mas não injeta o item de volta no inventário. | Adicionar chamada para criar/devolver o item ao inventário via `ItemFactoryClass` / `PlayerInventoryController` quando a feature for ativada. |
| **CR07-05** | 🟢 Sugestão | `MedicalLogic.cs:111` | Comentários e código de torniquete desativados inline. | Manter a flag de feature toggle clara no arquivo de configuração do mod. |

---

## 6. Veredito

- **Classificação:** 🟢 **APROVADO COM VALIDAÇÃO DE REFERÊNCIAS** (Módulo dormente/seguro no fluxo ativo)
- **Bloqueadores:** 0 🔴
- **Problemas Importantes:** 1 🟠 (Registrado para a ativação futura do sistema realista de necrose)
- **Gaps ou Riscos de Vazamento de Memória:** Nenhum no fluxo de jogo ativo.

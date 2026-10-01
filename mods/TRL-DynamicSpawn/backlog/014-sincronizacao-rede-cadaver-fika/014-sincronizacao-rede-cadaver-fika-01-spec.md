# 014 — sincronizacao-rede-cadaver-fika

**Mod:** TRL-DynamicSpawn  
**Status:** ⚪ Backlog  
**Criado:** 2026-09-15T09:05:00-03:00  

> **Perfil desta spec:** Sincronização de Rede e Consistência Multi-Cliente (FIKA Coop). Replicar eventos de transformação (`ConvertToBackpack`) e remoção (`DestroyCorpse`) de cadáveres do Host para todos os clientes convidados conectados à raid.

---

## 1. Visão Geral

No mod **TRL-DynamicSpawn**, a limpeza e otimização de cadáveres de bots (`CorpseCleanupManager`) é executada exclusivamente pelo Host da raid (`if (!FikaHelper.IsHostOrSolo()) return;`).

### O Problema Atual no FIKA:
- Quando o Host decide que um bot morto expirou seu tempo de vida:
  - **No Host:** O bot tem suas malhas desligadas e vira uma mochila (ou seu GameObject é destruído).
  - **Nos Clientes Convidados:** Nenhum pacote de rede é transmitido pelo FIKA ou pelo mod. Cada cliente continua executando a renderização completa do ragdoll do bot (`ObservedPlayer` / `ObservedCorpse`), vestindo todas as roupas e armas.
  - Se o Host destrói o bot (`DestroyCorpse`), o inventário no servidor é eliminado. Quando um cliente convidado tenta interagir com o corpo que ainda vê na sua tela, a requisição ao servidor quebra ou trava a interface de inventário.

---

## 2. Comportamento Desejado

1. **Broadcast de Eventos de Cadáver:**
   - Sempre que o Host executar `ConvertToBackpack` ou `DestroyCorpse` para um bot identificado por seu `ProfileId` / `NetId`, um pacote de rede compacto deve ser transmitido a todos os clientes conectados.
2. **Processamento Espelhado nos Clientes:**
   - Ao receber o pacote de conversão, o cliente localiza o `ObservedPlayer` / `ObservedCorpse` correspondente e aplica o congelamento de renderers e criação local da mochila visual.
   - Ao receber o pacote de remoção total, o cliente destrói com segurança seu `ObservedPlayer` e referências locais na cena.
3. **Zero Impacto em Performance:**
   - Sincronização orientada a eventos (sem polling contínuo de rede).
   - Uso de buffers reutilizáveis (`NetDataWriter`) para evitar alocações de heap (zero GC pressure).
   - Execução estritamente agendada na **Main Thread** da Unity no recebimento de pacotes nos clientes.

---

## 3. Critérios de Aceite

### Não-Regressão
- [ ] **NR-1:** Modo Solo (SPT sem FIKA ativo) continua funcionando sem qualquer dependência ou erro de classes de rede do FIKA.
- [ ] **NR-2:** Desconexões ou reconexões de clientes não afetam o loop principal do Host.

### Metas Mensuráveis
- [ ] **AC-M1:** 100% de paridade visual entre Host e Clientes: se o Host vê a mochila, todos os convidados vêem a mochila no mesmo local.
- [ ] **AC-M2:** 0 chamadas de APIs da Unity em background threads de rede durante o processamento de pacotes nos clientes.
- [ ] **AC-M3:** 0 vazamentos de GameObjects de cadáveres órfãos nas cenas dos clientes após remoção pelo Host.

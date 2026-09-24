# 007 — ACK de Operação de Inventário Bloqueado por Spawn de Bots · Code Review 01

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Spec Funcional:** [007-fila-inventario-bloqueada-spawn-bot-01-spec.md](007-fila-inventario-bloqueada-spawn-bot-01-spec.md)  
**Spec Técnica:** [007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md](007-fila-inventario-bloqueada-spawn-bot-02-spec-tech.md)  
**Review Técnica:** [007-fila-inventario-bloqueada-spawn-bot-03-spec-tech-review-01.md](007-fila-inventario-bloqueada-spawn-bot-03-spec-tech-review-01.md)  
**Data da Review:** 2026-09-15  
**Parecer:** 🟢 APROVADO COM RESSALVAS RESOLVIDAS  

---

## 1. Escopo das Alterações Analisadas

Foram auditados os seguintes arquivos modificados em `mods/FIKA/modded-V2/`:
1. `Fika.Core/Networking/NetworkUtils.cs`: adição da classe utilitária `NetworkChannels` e método de roteamento `GetChannelForSubPacket`.
2. `Fika.Core/Networking/FikaClient.cs`: adição de sobrecarga de `SendNetReusable` recebendo `byte channelNumber` e roteamento automático em `SendGenericPacket`.
3. `Fika.Core/Networking/FikaServer.cs`: adição de sobrecargas de `SendNetReusable` e `SendNetReusableToPeer` recebendo `byte channelNumber` e roteamento em `SendGenericPacket` e `SendGenericPacketToPeer`.
4. `Fika.Core/Plugin/FikaPlugin.cs` e `Fika.Core/Fika.Core.csproj`: incremento de versão SemVer para `2.4.4`.

---

## 2. Itens Auditados e Validações

### CR-07-01: Remoção de enum inexistente na compilação
- **Severidade:** Alta (Quebra de compilação)
- **Constatação:** Inicialmente, a tabela de mapeamento em `NetworkChannels.GetChannelForSubPacket` continha uma referência para `EGenericSubPacketType.ItemPacket`, que não existe no enum do BepInEx do FIKA.
- **Resolução:** A referência foi removida, mantendo o chaveamento estrito em `InventoryOperation` e `OperationCallback`.

### CR-07-02: Retrocompatibilidade de Assinaturas
- **Severidade:** Média
- **Constatação:** Garantir que chamadas de `SendNetReusable` legadas sem o parâmetro `channelNumber` continuem compilando e funcionando normalmente.
- **Resolução:** Sobrecargas com `byte channelNumber = 0` foram mantidas em `FikaClient.cs` e `FikaServer.cs`, redirecionando de forma transparente para o Canal 0.

### CR-07-03: Risco de Violação de Limites no LiteNetLib
- **Severidade:** Alta
- **Constatação:** Verificar se `channelNumber = 1` ultrapassa o limite alocado na criação dos sockets.
- **Resolução:** Tanto `FikaClient.cs` (linha 146) quanto `FikaServer.cs` (linha 162) configuram explicitamente `ChannelsCount = 2`. O índice `1` é plenamente válido e suportado nativamente pelo pool de canais do LiteNetLib.

---

## 3. Resultado do Build e Conclusão

- **Compilação:** `dotnet build -c Release` executada com sucesso (0 erros, 1 warning inofensivo de unificação de versão de System.Runtime.CompilerServices.Unsafe herdado do SPT).
- **Aprovação:** A alteração elimina o *Head-of-Line Blocking* do pipeline de inventário sob concorrência de spawn de bots sem alterar a semântica nem a integridade das operações.

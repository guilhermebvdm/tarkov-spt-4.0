---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 01, comparativo original/ vs modded/)"
date: 2026-09-10
status: 🟢 Vivo
authors: Claude
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 01)

> **Escopo:** auditoria comparativa `original/` (upstream FIKA intocado) vs `modded/` (nosso fork), particionada por subsistema, motivada por dois sintomas reais em raid: **(1)** colisão `"Default Inventory is currently being modified"` ao equipar arma logo após carregar munição continuamente com `SPT-ContinuousLoadAmmo`; **(2)** `ParseException: Undefined packet in NetDataReader: 9885` repetido nos logs de um jogador. Objetivo: confirmar se a "auditoria anterior" (sessões 1-4, incluindo os itens de backlog 001-005) foi completa ou deixou lacunas.
>
> **Método:** 4 sub-auditorias paralelas, cada uma lendo `original/` e `modded/` lado a lado, cruzando com `references/eft-decompiled/` (Assembly-CSharp EFT 0.16.9) e `references/fika-plugin/` (FIKA genérico upstream) como 3º ponto de comparação — Partição A (inventário/transações de rede), Partição B (registro de pacotes), Partição C (handlers de mãos observadas), Partição D (demais subsistemas, varredura ampla atrás de promessas não cumpridas da auditoria anterior).
>
> **Nenhum achado desta auditoria exige quebra de compatibilidade pública.** Todas as correções propostas são internas (métodos privados, tratamento de exceção, lógica de guarda) — nenhuma assinatura pública precisa mudar.
>
> **Correção 2026-09-10 (durante `/review-spec` do item `006`):** `AUD-01-03` original propunha construir detecção de colisão de hash que **já existe** (`node scripts/check-packet-hashes.js`, documentado em `docs/technical/spt-antipatterns.md` AP-11 e `docs/technical/fika-packet-desync-prevention-plan.md`) — severidade rebaixada de 🟡 pra 🔵 e reescrito. As sub-auditorias originais não tinham lido esses dois documentos; `AUD-01-01`, `AUD-01-02` e `AUD-01-04` permanecem validados sem alteração de mérito (achado `AUD-01-02` inclusive bate linha por linha com a "causa 4" já catalogada em `AP-11`).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 1 | Causa raiz confirmada do bug relatado #1 (trava de arma após ContinuousLoadAmmo) — e de uma família mais ampla do mesmo defeito |
| 🟠 **Alto** | 1 | Amplificador sistêmico: pacote desconhecido derruba o lote inteiro de eventos de rede do frame (explica a repetição do sintoma #2) |
| 🟡 **Médio** | 3 | Duplicação de mods de ladder, exceção silenciada em pool de operação (colisão de hash rebaixada — detecção já existe, ver `AUD-01-03`) |
| 🔵 **Baixo** | 4 | Verificação de colisão de hash já coberta por script existente, vazamento limitado sem risco de OOM, contrato de API interno enganoso, imprecisão de documentação |
| 💡 **Otimização** | 1 | Ausência de negociação de capacidade entre peers (item de roadmap, não correção cirúrgica) |

**Sobre o bug #2 (`Undefined packet: 9885`):** identificado com alta confiança — **não é um bug do fork FIKA**. É `CameraRotationMod.Networking.StanceSyncPacketV2` do mod `stancesAndCameraPositionSPT4.0.11` (v2.11.0+), recebido por um peer que ainda roda uma versão anterior desse mod (version-skew entre jogadores/Headless). Ver AUD-01-02 para o porquê disso se manifesta como erro repetido em vez de ser ignorado silenciosamente.

**Sobre o bug #1 (trava ao equipar arma):** causa raiz confirmada em código — ver AUD-01-01.

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🔴 Crítico | [`ObservedInventoryController.cs:218-223`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L218-L223) | Erro de lógica / Gap | Tolerância a colisão de mãos só funciona quando o item sendo validado agora é um carregador — nunca quando é a própria arma, faca ou granada |
| `AUD-01-02` | 🟠 Alto | [`NetPacketProcessor.cs:83-91`](../modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L83-L91) | Resiliência de rede | Pacote de tipo desconhecido lança exceção não capturada que descarta o lote inteiro de eventos de rede do frame |
| `AUD-01-03` | 🔵 Baixo | `scripts/check-packet-hashes.js` | Robustez | ⚠️ Corrigido — detecção de colisão de hash já existe (0 colisões hoje); só falta formalizar como gate |
| `AUD-01-04` | 🟡 Médio | [`ClientInventoryOperationHandler.cs:58-65,106-117`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L58-L65) | Exceção silenciosa | 2 blocos `catch (Exception) { }` vazios, possivelmente mascarando reentrância no pool de handlers |
| `AUD-01-05` | 🟡 Médio | `mods/Climbable Ladders/modded/ladders.fika/` | Duplicação de mod | Módulo `ladders.fika` (substituído por `TRL-FikaSync-ClimbableLadders`) continua compilável e pode voltar a registrar handlers concorrentes |
| `AUD-01-06` | 🔵 Baixo | [`InOutHandsProcessTimestampPatch.cs:94-126`](../modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs#L94-L126) | GC / limpeza | Entradas do dicionário de correlação só são removidas em `Succeed`, nunca por TTL — vazamento limitado, sem risco de OOM |
| `AUD-01-07` | 🔵 Baixo | [`FikaUIGlobals.cs:90-94`](../modded/Fika-Plugin/Fika.Core/UI/FikaUIGlobals.cs#L90-L94) | Contrato de API | `ShowFikaMessage` retorna objeto "fantasma" quando despachado fora da main thread |
| `AUD-01-08` | 🔵 Baixo | `mods/FIKA/backlog/004-colisao-cura-swap-magazine/` (docs) | Documentação | "Reanimação" citada como beneficiária do fix 004, mas o caminho de revive não passa pelo mesmo pipeline — extrapolação não confirmada |
| `AUD-01-09` | 💡 Otimização | `FikaServer.cs:611-618` / `IFikaNetworkManager.cs` | Arquitetura | Sem negociação de capacidades entre peers para pacotes de mods de terceiros (item de roadmap) |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Tolerância a colisão de mãos (`IsSelfReferentialHandsTransition`) só cobre carregador como ação-alvo — causa raiz confirmada do bug relatado

- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`ObservedInventoryController.cs:218-223`](../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs#L218-L223)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:32223-32355`](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) (`TryRemoveFromHands`, `TrySetInHands`, `method_33/34/35`), `GEventArgs17.cs`, `GClass1561.cs`
- **Causa Raiz:** o item `004` generalizou apenas o **`movedItem`** (o item que abriu a janela `Begin` anterior) de "outro carregador" para "qualquer item" — mas não tocou a guarda de entrada do método:
  ```csharp
  private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
  {
      if (item is not MagazineItemClass || inOutHandsProcess?.Item is not Weapon weapon)
      {
          return false;
      }
      ...
  }
  ```
  `item` aqui é o item sendo validado **agora**, na operação que está tentando prosseguir. Quando esse item não é um carregador — é a própria arma, uma faca ou uma granada — o método retorna `false` incondicionalmente, e qualquer colisão residual com uma transição de mãos recente na mesma arma é rejeitada com `GClass1561` ("is currently being modified"), mesmo sendo uma sequência legítima de ações do mesmo jogador.
- **Impacto Técnico Real — duas manifestações confirmadas do mesmo defeito:**
  1. **Bug relatado (ContinuousLoadAmmo → equipar arma):** `ContinuousLoadAmmo/Controllers/LoadAmmoController.cs` chama `_player.SetEmptyHands(...)` ao iniciar carregamento fora do inventário e `_player.TrySetLastEquippedWeapon()` ao terminar. No Headless/Host, essas duas transições de mãos na mesma arma podem se sobrepor (latência de rede); quando `TrySetLastEquippedWeapon` chega com um `Begin` residual de `SetEmptyHands` ainda pendente, `item == weapon` (não um carregador) → `IsSelfReferentialHandsTransition` retorna `false` → `GClass1561(item, ParentItem.GetRootItem())` → mensagem literal `"Cannot apply {arma} because Default Inventory is currently being modified"`. Bate exatamente com o sintoma relatado.
  2. **Gap adicional confirmado independentemente pela Partição C:** curar um aliado (`TRL-ImmersiveCombatMedicine`) abre a mesma janela `Begin` com o item de cura; se a AÇÃO SEGUINTE do jogador dentro da janela de graça (~0.35s) for sacar a faca, arremessar uma granada, ou trocar de arma (em vez de trocar o carregador da arma atual — único caso que o item 004 testou), `item` também não é `MagazineItemClass` e a mesma rejeição ocorre.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* só tolera quando `item is MagazineItemClass`.
  - *Abordagem Otimizada:* generalizar a guarda de entrada da mesma forma que o item 004 generalizou `movedItem`, preservando o bloqueio real de duplicata (mesma arma colidindo com ela mesma):
  ```csharp
  private bool IsSelfReferentialHandsTransition(Item item, GEventArgs17 inOutHandsProcess)
  {
      if (inOutHandsProcess?.Item is not Weapon weapon)
      {
          return false;
      }

      if (!InOutHandsProcessTimestampPatch.TryGetPendingBegin(this, weapon, out var movedItem, out var elapsed))
      {
          return false;
      }

      if (movedItem == null || elapsed > GraceWindowSeconds)
      {
          return false;
      }

      // Continua bloqueando o caso real de duplicata: a própria arma
      // tentando reentrar nas mãos enquanto a transição pendente também
      // foi aberta pela própria arma (saque real concorrente).
      if (item == weapon && movedItem == weapon)
      {
          return false;
      }

      return true;
  }
  ```
- **Como validar:** cenário 1 (bloqueador) — carregar munição continuamente fora do inventário e equipar a arma logo em seguida, em raid Headless real, repetidas vezes; deve parar de gerar `GClass1561`. Cenário 2 — curar um aliado e, dentro de ~0.35s, sacar faca / arremessar granada / trocar de arma (não só carregador); deve parar de travar a mão. Cenário 3 (regressão, crítico) — dois jogadores tentando pegar a MESMA arma ao mesmo tempo (saque real concorrente) precisa continuar sendo rejeitado com `GClass1561` — este é o teste que a incerteza de `P-4.1` já pedia e continua pendente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · `NetPacketProcessor` não trata pacote de tipo desconhecido com segurança — derruba o lote inteiro de eventos de rede do frame

- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`NetPacketProcessor.cs:83-91`](../modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L83-L91) (`GetCallbackFromData` lança `ParseException`) → [`LiteNetManager.cs:1436-1441`](../modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/LiteNetManager.cs#L1436-L1441) (`PollEvents()` sem try/catch) → [`FikaClient.cs:495-501`](../modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L495-L501) / [`FikaServer.cs:934-947`](../modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L934-L947) (`OnNetworkReceive` sem try/catch)
- **Antipattern já catalogado:** este é exatamente `AP-11` (`docs/technical/spt-antipatterns.md`), "causa raiz 4" no guia canônico [`fika-packet-desync-prevention-plan.md` §2](../../../docs/technical/fika-packet-desync-prevention-plan.md#2-causas-raiz-de-desincronização--parseexception) — descrito lá como "o mecanismo comum" pelo qual as causas 1-6 (registro tardio, perda de registro em troca de sessão, `UnregisterPacket` indevido, assimetria Serialize/Deserialize, `SendData` fora da main thread) todas terminam. O guia documenta a mitigação **do lado de cada mod que registra pacote** (airbag por callback, envelope, `TryGet*`) — mas isso não cobre o caso de hash **nunca registrado** (mod ausente no peer, ou ainda não registrado nesse frame), porque não existe callback nenhum pra colocar um try/catch. Esse é o gap que este achado fecha: um airbag **central**, na fronteira `ReadAllPackets`, complementar (não substituto) à conformidade por-mod que o guia já exige.
- **Causa Raiz:** comportamento **herdado do upstream** (idêntico em `original/`, `modded/` e `references/fika-plugin/` — diff = 0 linhas nos 3). Quando um peer recebe um pacote cujo hash não está registrado localmente (mod ausente, ou versão desalinhada do mesmo mod nos dois lados — ver nota sobre `9885` abaixo), `ParseException` propaga sem barreira até `Update()` do `MonoBehaviour`.
- **Impacto Técnico Real:** Unity absorve a exceção na fronteira do `Update()` (o jogo não trava), mas **todos os demais eventos de rede já enfileirados naquele lote são descartados** — inclusive movimentação/estado de outros jogadores — e o resto do corpo de `Update()` daquele frame é pulado (`ObservedPlayers[i].ManualStateUpdate`, `HandleInventoryOperations()`). Isso produz stutter visível e perda de dados a cada ocorrência, explicando por que o sintoma é reportado como repetido em vez de único.
- **Causa raiz confirmada do `9885`:** reimplementação do algoritmo de hash (CRC-16-CCITT sobre `typeof(T).ToString()`) rodada contra ~150 tipos de pacote de todo o repo encontrou exatamente um match: `CameraRotationMod.Networking.StanceSyncPacketV2` (mod `stancesAndCameraPositionSPT4.0.11`, introduzido na v2.11.0 desse mod para não colidir com o formato de pacote antigo). **Não é um bug do fork FIKA** — é version-skew: algum peer (possivelmente o Headless) ainda roda uma versão desse mod anterior à 2.11.0. Ação recomendada fora do escopo desta auditoria: confirmar com o grupo que todos os peers, incluindo o Headless dedicado, estão na mesma versão do `stancesAndCameraPositionSPT4.0.11`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* pacote desconhecido lança exceção não capturada.
  - *Abordagem Otimizada:* envolver `_packetProcessor.ReadAllPackets(reader, peer)` em `FikaClient.OnNetworkReceive`/`FikaServer.OnNetworkReceive` com try/catch que loga e continua; nenhum caller no repo depende do `throw` hoje (busca não encontrou nenhum `catch (ParseException)`).
  ```csharp
  private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, ...)
  {
      try
      {
          _packetProcessor.ReadAllPackets(reader, peer);
      }
      catch (ParseException ex)
      {
          FikaGlobals.LogWarning($"Unknown/incompatible packet from {peer}: {ex.Message}");
      }
  }
  ```
- **Como validar:** com um mod de terceiro deliberadamente desabilitado num dos dois peers, confirmar que o outro peer continua recebendo movimentação/estado normalmente (sem stutter) mesmo enquanto o pacote desconhecido é descartado e logado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · Hash de 16 bits sem detecção de colisão entre pacotes de mods diferentes — ⚠️ CORRIGIDO em 2026-09-10, achado original estava incompleto

> **Nota de correção (durante `/review-spec` do item `006`):** a formulação original deste achado propunha *construir* detecção de colisão dentro do `NetPacketProcessor`. Isso ignorava que o repositório **já tem** essa detecção pronta — `node scripts/check-packet-hashes.js` — documentada em `docs/technical/spt-antipatterns.md` (AP-11) e em `docs/technical/fika-packet-desync-prevention-plan.md` §1/§7. Rodado nesta correção: **84 tipos `INetSerializable`, 0 colisões de CRC-16**; 4 pares de FQN duplicado entre `Band-Aid` e `TRL-ImmersiveCombatMedicine` (`Band_Aid.BandAidHeal*`) já são um caso conhecido e aceito (§6.1 do guia — só o ICM está instalado hoje). Severidade rebaixada de 🟡 para 🔵 e proposta de correção reescrita — não é preciso construir nada, só formalizar o uso do script já existente.

- **Severidade:** 🔵 Baixo (era 🟡 Médio)
- **Localização no Mod:** `scripts/check-packet-hashes.js` (já existe, fora de `mods/FIKA/`) — nada a alterar em [`NetPacketProcessor.cs:206-397`](../modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L206-L397).
- **Causa Raiz:** o identificador de pacote é um `ushort` (CRC-16, 65536 valores) do nome completo do tipo; sem coordenação de namespace entre os ~10+ mods deste repo que registram pacotes próprios via `IFikaNetworkManager`, uma colisão faria o segundo `RegisterPacket` sobrescrever o handler do primeiro em silêncio (`Dictionary<ushort,...>`). O script já cobre esse risco por varredura estática — o gap real é que ele não está amarrado a nenhum gate automático (`/code-review`, CI, pre-commit).
- **Impacto Técnico Real:** hoje, zero — 0 colisões confirmadas por execução real do script (não simulação). O risco é só de regressão futura (novo mod/pacote introduzindo colisão sem ninguém rodar o script).
- **Proposta de Correção:** nenhuma mudança de código no FIKA. Formalizar `node scripts/check-packet-hashes.js` como parte do checklist de `/code-review` sempre que um mod tocar `INetSerializable` (já é o caso nominal em `spt-mod-best-practices` §9, checklist 11) — se ainda não estiver, considerar adicionar ao hook de pre-commit (`.agents/hooks/`) quando arquivos que implementam `INetSerializable` mudarem.
- **Decisão:**
  - `[x]` Aceitar com modificação: sem ação de código; confirmar que o script já roda em `/code-review` (não faz parte deste item de backlog — é achado informativo, resolvido por correção do próprio relatório)

---

### AUD-01-04 · Exceção silenciada em `ClientInventoryOperationHandler` — possível reentrância mascarada no pool

- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`ClientInventoryOperationHandler.cs:58-65`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L58-L65) e [`:106-117`](../modded/Fika-Plugin/Fika.Core/Main/ClientClasses/ClientInventoryOperationHandler.cs#L106-L117)
- **Causa Raiz:** dois blocos `catch (Exception) { }` totalmente vazios (sem log, sem re-throw) foram adicionados junto com o TRL-Fixes #2 e um patch defensivo em `HandleResult` — destoam do resto do arquivo, que usa `FikaGlobals.LogError` em todo outro branch de erro. O segundo envolve `Operation.Dispose()` dentro de try/catch aninhado, somado a `?.` defensivos (`InventoryController?.ID`, `Operation?.Id`) sobre campos que o próprio `Dispose()` da classe zera — padrão consistente com blindagem contra reentrância/double-dispose na reutilização de instância via `ClientInventoryOperationHandlerPool`, sem a causa raiz ter sido diagnosticada.
- **Impacto Técnico Real:** se `Operation.Dispose()` falhar silenciosamente, um handler pode voltar ao pool sem liberar recursos corretamente, ou mascarar um `NullReferenceException` real de reentrância — invisível em produção por falta de log.
- **Proposta de Correção:** trocar os dois `catch (Exception) { }` por `catch (Exception ex) { FikaGlobals.LogError($"ClientInventoryOperationHandler: {ex}"); }`, padrão já usado no resto do arquivo. Se os logs revelarem reentrância real, abrir item de backlog dedicado para investigar o pool.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-05 · Duplicação funcional entre `Climbable Ladders/ladders.fika` e `TRL-FikaSync-ClimbableLadders`

- **Severidade:** 🟡 Médio
- **Localização no Mod:** `mods/Climbable Ladders/modded/ladders.fika/` vs `mods/TRL-FikaSync-ClimbableLadders/modded/`
- **Causa Raiz:** `TRL-FikaSync-ClimbableLadders` foi construído como substituto do módulo `ladders.fika` nativo (já documentado em `mods/Climbable Ladders/docs/relatorio-auditoria-codigo-01.md:281-349`). Hoje `mods/Climbable Ladders/builds/` só distribui `tarkin.ladders.bep.dll`/`tarkin.ladders.shared.dll` — `tarkin.ladders.fika.dll` não está entre os binários ativos, mas o projeto `.csproj`/`.sln` continua íntegro e compilável.
- **Impacto Técnico Real:** namespaces diferentes (`tarkin.ladders.fika.*` vs `TRL.FikaSync.ClimbableLadders.*`) evitam colisão de hash hoje — não é a causa do `9885`. Mas se `ladders.fika` for recompilado/reativado (ex: `/compile-mod` acidental do projeto errado), os dois plugins passam a registrar handlers concorrentes para o mesmo conceito (ladder state), cada um com seu próprio `ObservedPlayerLadderController` remoto.
- **Proposta de Correção:** documentar em `mods/Climbable Ladders/mod.json`/`README.md` que `ladders.fika` não deve ser compilado/distribuído enquanto `TRL-FikaSync-ClimbableLadders` for o substituto ativo, ou remover o projeto do `.sln` para impedir build acidental.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-06 · `InOutHandsProcessTimestampPatch` — entradas só removidas em `Succeed`, nunca por TTL

- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`InOutHandsProcessTimestampPatch.cs:94-126`](../modded/Fika-Plugin/Fika.Core/Main/Patches/InventoryPatches/InOutHandsProcessTimestampPatch.cs#L94-L126)
- **Causa Raiz:** confirmado por leitura do EFT nativo que `RaiseInOutProcessEvents` nunca é chamado com `CommandStatus.Failed` para `GEventArgs17` — hoje isso não é bug funcional, mas se o delegate de conclusão nunca disparar (ex: teardown de cena no meio de uma operação, desconexão do jogador), a entrada daquela arma fica no dicionário pelo resto do raid.
- **Impacto Técnico Real:** vazamento limitado (bounded pelo número de armas distintas manuseadas no raid), sem risco de OOM — `ConditionalWeakTable` já garante que não cruza raids.
- **Proposta de Correção (opcional, baixa prioridade):** em `TryGetPendingBegin`, remover proativamente entradas com `elapsed > GraceWindowSeconds` (lazy eviction), custo zero adicional.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-07 · `FikaUIGlobals.ShowFikaMessage` retorna objeto "fantasma" quando despachado fora da main thread

- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`FikaUIGlobals.cs:90-94`](../modded/Fika-Plugin/Fika.Core/UI/FikaUIGlobals.cs#L90-L94)
- **Causa Raiz:** ao corrigir o TRL-Fixes #5 (despacho thread-safe via `AsyncWorker.RunInMainTread`), a função pública `ShowFikaMessage` passou a despachar corretamente para a main thread quando chamada de outra thread, mas retorna imediatamente `new GClass3835()` — um objeto de contexto vazio, desconectado do diálogo real exibido depois.
- **Impacto Técnico Real:** hoje sem impacto prático (o único caller do repo, `FikaConfig.cs:168`, ignora o retorno), mas é um contrato de API pública potencialmente enganoso para qualquer mod externo que dependa de `Fika.Core.dll` e use o retorno fora da main thread.
- **Proposta de Correção:** documentar via XML doc que o retorno é inválido quando despachado fora da main thread, ou logar aviso nesse caso. Não requer mudança de assinatura.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-08 · "Reanimação" citada como beneficiária do fix 004 sem confirmação por leitura de código

- **Severidade:** 🔵 Baixo
- **Localização:** `mods/FIKA/backlog/004-colisao-cura-swap-magazine/*` (spec funcional, spec técnica, memória de sessão) vs [`ReviveInteractable.cs:232-246`](../modded/Fika-Plugin/Fika.Core/Main/Components/ReviveInteractable.cs#L232-L246)
- **Causa Raiz:** a documentação do item 004 lista "reanimação" ao lado de cura/granada/faca como ação que se beneficia da generalização de `IsSelfReferentialHandsTransition`. Leitura de `ReviveInteractable.StartRevive` mostra que a reanimação do FIKA usa `CurrentManagedState.Plant(...)` — uma state machine de interação genérica, sem nenhuma referência a `Player.MedsController`, `HandleInProcess` ou `RaiseInOutProcessEvents`. Não há evidência de que reviver um aliado dispare a mesma janela `Begin`/`Succeed` na arma do revivedor.
- **Impacto Técnico Real:** provavelmente nulo (se reanimação nunca dispara `Begin`, o cenário não ocorre) — mas é uma afirmação não confirmada por código que pode gerar falsa confiança numa validação in-game futura que assuma esse caminho como coberto.
- **Proposta de Correção:** nenhuma mudança de código. Ajustar a linguagem da spec/memória para "reanimação — hipótese não confirmada por leitura de código" até validação in-game dedicada.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-09 · Sem negociação de capacidades entre peers para broadcast de pacotes de terceiros

- **Severidade:** 💡 Otimização (item de roadmap, não correção cirúrgica)
- **Localização:** [`FikaServer.cs:611-618`](../modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L611-L618) (`SendData<T>` → `SendToAll` incondicional), `IFikaNetworkManager.cs`
- **Causa Raiz:** `IFikaNetworkManager` (API pública usada pelos ~10 mods de terceiro que registram pacotes) não expõe nenhum "o peer X suporta este tipo de pacote?" — `RegisterPacket<T>` só afeta recepção local, nunca envio.
- **Impacto Técnico Real:** é o mecanismo geral por trás de `AUD-01-02`/`9885`: qualquer mod de terceiro que faz broadcast de pacote custom gera `ParseException` em qualquer peer sem aquele mod (ou com versão desalinhada) — por design, não bug pontual. Cada mod (`stancesAndCameraPositionSPT4.0.11`, `TRL-ImmersiveCombatMedicine`) hoje reinventa sua própria estratégia de compat (versionamento V1/V2 de pacote).
- **Proposta de Correção:** fora de escopo para correção cirúrgica — registrar como item de roadmap: handshake de capacidades por peer na conexão, permitindo a um mod de terceiro decidir não enviar/logar para peers sem suporte ao tipo.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Confirmações (sem achado — a auditoria anterior foi genuína nestes pontos)

Verificação direta contra 7 afirmações específicas dos relatórios da Sessão 1 (`docs/modded/relatorio-correcao-01..08.md`) — todas confirmadas **reais e corretas**, sem "promessa vazia":

| Afirmação | Local | Veredito |
|---|---|---|
| Teardown `FikaServer.OnDestroy()`/`FikaClient.OnDestroy()` | `FikaServer.cs:585-600`, `FikaClient.cs:350-355` | ✅ real |
| Descarte recursivo `PacketPool.Dispose()` | `Pooling/PacketPool.cs:74-82` + cadeia até `BasePacketPoolManager.cs:52-60` | ✅ real, cadeia completa até `OnDestroy()` |
| Desinscrição de delegates de armadura em `FikaPlayer.OnDestroy()` | `Players/FikaPlayer.cs:1718-1727` | ✅ real (original não tinha nenhum unsubscribe) |
| `VoipEftSource.Release()` em `ObservedPlayer.OnDestroy()` | `Players/ObservedPlayer.cs:1794-1804` | ✅ real |
| Limpeza de refs estáticas em `FikaHostWorld`/`FikaClientWorld.OnDestroy()` | `HostClasses/FikaHostWorld.cs:47-52`, `ClientClasses/FikaClientWorld.cs:103-113` | ✅ real |
| Proteção Singleton na FreeCam | `FreeCamera/FreeCameraController.cs:27-33` | ✅ real |
| Timeout `CancellationTokenSource` em WebSocket headless | `FikaServer/WebSockets/HeadlessClientWebSocket.cs:82-89` | ✅ real |

Outras confirmações relevantes desta rodada:
- `NetPacketProcessor.cs`, `IFikaNetworkManager.cs` e a lista/ordem de `RegisterPacket<T>` em `FikaClient`/`FikaServer` são **idênticos** ao upstream (`original/` e `references/fika-plugin/`) — nenhuma regressão do fork no registro de pacotes nativos.
- Nenhum outro bloco de `ObservedInventoryController.CheckItemAction` (além do `inOutHandsProcess` já coberto por `AUD-01-01`) é vulnerável ao mesmo tipo de falso-positivo — confirmado por leitura linha a linha comparando com `original/`.
- Item `005` (`ObservedMedsSpeedHook`) confirmado aditivo, síncrono/main-thread, sem necessidade de reset entre raids.
- `GraceWindowSeconds = 0.35f` é reaproveitado de forma simétrica pelos itens 003 e 004 — não há assimetria estrutural entre os dois usos (pendência de calibração `P-3.1` continua válida como estava, sem piora).
- Watchdog do item `002` (client-side, callbacks de rede) e a correlação Begin/Succeed dos itens 003/004 (server-side, Headless) são subsistemas complementares, não sobrepostos — nenhum dos dois está incompleto por não cobrir o escopo do outro.

---

## 5. Plano de Ação e Recomendações

1. **Priorizar `AUD-01-01`** — é a causa raiz confirmada do bug mais visível em raid (trava ao equipar arma) e generaliza para outros gatilhos (faca/granada/troca de arma após cura). Requer novo ciclo de backlog (spec → spec técnica → code-mod → validação in-game, incluindo o teste de regressão do saque real concorrente).
2. `AUD-01-02` (tratamento seguro de pacote desconhecido) é uma correção pequena e de baixo risco que reduz o blast radius de qualquer descompasso futuro de versão entre mods de terceiros — vale agrupar no mesmo item de backlog que `AUD-01-03`.
3. `AUD-01-04` e `AUD-01-06` são candidatas a um item de "limpeza" de baixo risco (trocar catch vazio por log; lazy eviction).
4. `AUD-01-05` e `AUD-01-08` são ações de documentação/config, não exigem `/code-mod`.
5. `AUD-01-09` fica registrado como item de roadmap, não entra no próximo ciclo de correção.
6. Ação imediata fora do código: **confirmar com o grupo que todos os peers (incluindo o Headless) estão na mesma versão do mod `stancesAndCameraPositionSPT4.0.11` (≥ 2.11.0)** — resolve o sintoma `9885` sem precisar de nenhuma mudança no FIKA.

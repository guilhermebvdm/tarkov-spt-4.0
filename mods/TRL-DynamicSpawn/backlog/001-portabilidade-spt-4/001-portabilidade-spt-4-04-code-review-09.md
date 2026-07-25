---
title: "Code Review 09: Correção do Headless Spawn e HUD Client-Side"
date: 2026-07-20
status: 🟢 Vivo
authors: [Agent]
---

# 📝 Code Review: Correção do Headless Spawn e HUD Client-Side

## 1. O Problema (Falta de Spawn no Headless e Falta de HUD no Cliente)
O usuário relatou que, ao utilizar o Fika Headless para hospedar a partida, nenhum PMC estava spawnando. Além disso, o cliente convidado não via a interface de HUD de debug (F12).
A causa detectada foi:
- O patch `DynamicSpawnManagerPatch` exigia a existência de `BotsController` para adicionar o manager ao `GameWorld`. Como clientes de rede (Fika Clients) não possuem controle físico dos bots localmente, o `BotsController` é nulo, impedindo a injeção do componente de interface de usuário.
- O método `IsFikaClient()` identificava o Headless Dedicated Host como um cliente na arquitetura interna de rede do Fika (`FikaBackendUtils.IsClient` retornado como `true`), causando o desligamento do loop de horda em ambas as máquinas e não spawnando nenhum bot.

## 2. A Solução Implementada
- **`DynamicSpawnManagerPatch.cs`**: O bloqueio por `botsController == null` foi removido. Agora o manager é adicionado ao `GameWorld` de qualquer forma, e, caso os objetos de IA sejam nulos, a inicialização ocorre permitindo apenas o processamento de UI (HUD do F12).
- **`DynamicSpawnManager.cs`**: Foi adicionada uma regra verificando `UnityEngine.Application.isBatchMode` no método `IsFikaClient()`. Quando o servidor Fika opera em batch mode (modo terminal sem renderização), o método ignora as flags internas do Fika e atua forçosamente como host.

## 3. Análise Crítica (Achados)

Nenhum bloqueador grave 🔴 foi detectado nesta revisão. A alteração utilizou um método muito resiliente ao engine (`isBatchMode`) e retirou acoplamentos incorretos que impediam a inicialização do UI de forma client-side.

### CR-09-01 · F — Melhoria Opcional · 🟢 Menor

**Filtro no Update de Status no HUD para Fika Client e Host**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:857`](../../Client/Components/DynamicSpawnManager.cs#L857)

**Problema:** O texto exibido no painel F12 define `fikaStatus` via Reflection a cada loop na OnGUI (rodando todo frame). Isso consome muito mais ciclos do que o necessário, pois o status de sessão não muda durante a raid.

**Por que importa:** O `OnGUI()` é chamado várias vezes por frame, e buscar um *Type* via Reflection nele não é adequado em termos de performance, podendo causar flutuações de framerate, embora pequeno. 

**Sugestão:** Fazer o cache da string `fikaStatus` durante a inicialização (no `Init` ou `Start`) e apenas imprimir o valor no `OnGUI`. 

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

## 4. Resultado da Compilação
- O cliente compilou com **0 Erros**.
- A UI não gerou NullReferenceExceptions por receber `botsController = null` devido à proteção de instanciamento de listas no `Update`/`OnGUI`.
- Próximo passo recomendado é apenas aceitar ou ignorar a melhoria CR-09-01.

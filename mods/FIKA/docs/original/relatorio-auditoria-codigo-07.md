---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 07: Fika-Server-CSharp & Server Architecture)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 07: Fika-Server-CSharp & Server Architecture)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 7 (Servidor C#, Controllers HTTP, Roteadores de Requisição, WebSockets, Gerenciamento de Sessão de Raids e APIs de Matchmaking)** do código original do mod **FIKA**, inspecionando ~4.100 linhas de código C# distribuídas nos módulos `FikaServer/Controllers/`, `Routers/`, `Services/`, `WebSockets/` e `FikaShared/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 1 | Callback assíncrono `async void` em `System.Threading.Timer` dentro de `MatchService.AddTimeoutInterval` com risco de crash fatal do processo do servidor SPT por exceção não capturada. |
| 🟠 **Alto** | 1 | Serialização JSON redundante N vezes e quebra de broadcast em cadeia por `WebSocketException` desprotegida em `NotificationWebSocket.BroadcastAsync`. |
| 🟡 **Médio** | 2 | Acesso não defensivo a salas inexistentes em `RaidController.HandleRaidJoin` (NRE) e lançamento manual anti-padrão de `NullReferenceException` em `FikaDialogueController`. |
| 💡 **Otimização** | 1 | Substituição de busca linear LINQ (`.Where().FirstOrDefault()`) por iteração direta em `NotificationWebSocket.OnClose`. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-07-01` | 🔴 Crítico | [`MatchService.cs:L36-44`](../../original/Fika-Server-CSharp/FikaServer/Services/MatchService.cs#L36-L44) | Concorrência / Crash | Lambda `async void` em `System.Threading.Timer` sem bloco `try-catch`, podendo derrubar o servidor em falha de timeout. |
| `AUD-07-02` | 🟠 Alto | [`NotificationWebSocket.cs:L91-99`](../../original/Fika-Server-CSharp/FikaServer/WebSockets/NotificationWebSocket.cs#L91-L99) | Performance / Falha | Serialização JSON repetida por destinatário em `BroadcastAsync` e ausência de captura de erro por cliente desconectado. |
| `AUD-07-03` | 🟡 Médio | [`RaidController.cs:L70, L75`](../../original/Fika-Server-CSharp/FikaServer/Controllers/RaidController.cs#L70) | AP-02 (Defensiva) | Supressão nula `match!` gerando HTTP 500 por NRE ao consultar partida inexistente no `HandleRaidJoin`. |
| `AUD-07-04` | 🟡 Médio | [`FikaDialogueController.cs:L98`](../../original/Fika-Server-CSharp/FikaServer/Controllers/FikaDialogueController.cs#L98) | Boas Práticas | Lançamento explícito de `NullReferenceException` em validação de argumentos ao invés de `ArgumentException`. |
| `AUD-07-05` | 💡 Otimização | [`NotificationWebSocket.cs:L64`](../../original/Fika-Server-CSharp/FikaServer/WebSockets/NotificationWebSocket.cs#L64) | Desempenho | Consulta LINQ `.Where(x => x.Value == ws).FirstOrDefault()` no fechamento de conexões WebSocket. |

---

## 3. Detalhamento dos Achados

### AUD-07-01 · Risco de Crash do Servidor SPT por `async void` em `Timer`
- **Severidade:** 🔴 Crítico
- **Localização:** [`MatchService.cs:L36-44`](../../original/Fika-Server-CSharp/FikaServer/Services/MatchService.cs#L36-L44)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O temporizador de timeout de partidas em `MatchService.AddTimeoutInterval` é criado com uma função anônima `async _ => { ... await EndMatch(...) }`. Em .NET, passar uma lambda assíncrona para um construtor de `Timer` resulta em uma assinatura `async void`.
- **Impacto Técnico Real:** Se o método `EndMatch` falhar ou lançar uma exceção (ex.: erro de I/O em salvamento ou banco de dados), a exceção não é capturada por nenhuma Task chamadora e explode diretamente na ThreadPool da aplicação, forçando a finalização abrupta (*crash*) do processo do servidor SPT.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:*
    ```csharp
    Timer timer = new(async _ =>
    {
        var match = GetMatch(matchId);
        if (match != null && match.Timeout++ >= fikaConfig.Config.Server.SessionTimeout)
        {
            await EndMatch(matchId, EFikaMatchEndSessionMessage.PingTimeout);
        }
    }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    ```
  - *Abordagem Otimizada:*
    ```csharp
    Timer timer = new(async _ =>
    {
        try
        {
            var match = GetMatch(matchId);
            if (match != null && match.Timeout++ >= fikaConfig.Config.Server.SessionTimeout)
            {
                await EndMatch(matchId, EFikaMatchEndSessionMessage.PingTimeout);
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error in match timeout timer for {matchId}: {ex}");
        }
    }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-07-02 · Churn de Serialização e Interrupção em Cadeia no `BroadcastAsync`
- **Severidade:** 🟠 Alto
- **Localização:** [`NotificationWebSocket.cs:L91-99`](../../original/Fika-Server-CSharp/FikaServer/WebSockets/NotificationWebSocket.cs#L91-L99)
- **Causa Raiz:** `BroadcastAsync` itera sobre todos os clientes conectados e invoca `SendAsync`, serializando a mensagem para JSON e convertendo para UTF-8 repetidamente dentro do loop. Além disso, se o envio para um cliente falhar com `WebSocketException`, a exceção propaga e cancela o envio para todos os demais clientes restantes.
- **Impacto Técnico Real:** Desperdício de ciclos de serialização JSON e perda de notificações de matchmaking para jogadores válidos caso um peer tenha caído repentinamente.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Serializar o JSON e obter o buffer de bytes uma única vez antes do loop, e encapsular o `SendAsync` em bloco `try-catch` individual por cliente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-07-03 · Acesso Não Defensivo a Salas Inexistentes no `HandleRaidJoin`
- **Severidade:** 🟡 Médio
- **Localização:** [`RaidController.cs:L70, L75`](../../original/Fika-Server-CSharp/FikaServer/Controllers/RaidController.cs#L70)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-02`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `HandleRaidJoin` assume que `matchService.GetMatch(request.ServerId)` nunca retorna nulo (`match!`).
- **Impacto Técnico Real:** Se uma solicitação for enviada com um ID de sala encerrada ou inválido, o servidor dispara `NullReferenceException` gerando status HTTP 500 no cliente.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Validar `if (match == null)` e retornar um payload de erro amigável ao cliente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-07-04 · Lançamento Anti-padrão de `NullReferenceException`
- **Severidade:** 🟡 Médio
- **Localização:** [`FikaDialogueController.cs:L98`](../../original/Fika-Server-CSharp/FikaServer/Controllers/FikaDialogueController.cs#L98)
- **Causa Raiz:** O código executa `throw new NullReferenceException(...)` explicitamente para validar parâmetros de entrada.
- **Impacto Técnico Real:** Violação dos padrões de engenharia .NET Core (NRE deve ser reservada para falhas de ponteiro de runtime, usando-se `ArgumentException` ou `ArgumentNullException` para validação de entrada).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Substituir por `throw new ArgumentException(...)`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-07-05 · Otimização de Busca Linear em `NotificationWebSocket.OnClose`
- **Severidade:** 💡 Otimização
- **Localização:** [`NotificationWebSocket.cs:L64`](../../original/Fika-Server-CSharp/FikaServer/WebSockets/NotificationWebSocket.cs#L64)
- **Causa Raiz:** A desconexão utiliza `clientWebSockets.Where(x => x.Value == ws).FirstOrDefault()`, percorrendo a lista com alocação de enumerador LINQ.
- **Impacto Técnico Real:** Oportunidade de iterar diretamente sem LINQ para fechar conexões de forma rápida e com zero alocações.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Iterar com loop `foreach` simples procurando a chave correspondente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de integridade com clientes de terceiros e mods de servidor:

| Símbolo Público / Rota HTTP | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `/fika/raid/create`, `/fika/raid/join`, `/fika/raid/leave` | *Fika-Plugin*, *Headless Client* | Preservar rotas REST e payloads JSON inalterados. |
| `/fika/notification/` | *Client WebSocket Handlers* | Preservar handshake de autorização e protocolo de pacotes. |
| `MatchService.Matches` | *Server Mods*, *Web UI* | Preservar estrutura concorrente e propriedades de consulta. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-07.md
```

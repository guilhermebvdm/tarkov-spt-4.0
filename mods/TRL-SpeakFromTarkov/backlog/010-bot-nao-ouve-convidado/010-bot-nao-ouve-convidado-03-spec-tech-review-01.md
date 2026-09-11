# 010 — Bot Não Ouve Convidado · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [010-bot-nao-ouve-convidado-02-spec-tech.md](010-bot-nao-ouve-convidado-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Evidência do check AP-01 não corresponde ao pacote em questão | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 Importante | Timing de `VoipController.Instance`/`botVoiceBridge` não nulo no recebimento do pacote | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟢 Menor | Falta nota explícita sobre arquivo compartilhado com o item 014 | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · ✅ Resolvido em 2026-09-09 · C — Erro de Lógica · 🟡 Importante

**Evidência do check AP-01 (§9) não corresponde ao pacote em questão**

**Problema:** A linha 1 da tabela de conformidade (§9) justifica o check de lifecycle de raid (AP-01) dizendo: *"a fila de pacotes já existente (`sendQueue`) e o `EnsurePacketsRegistered` cobrem o ciclo de vida"*. Mas `SendVoicePowerToHost` (§5) **não usa `sendQueue`** — ela chama `IFikaNetworkManager.SendData` diretamente e de forma síncrona, dentro de `TriggerBotVoiceEvent`. `sendQueue` é a fila de frames de áudio (`SftNetwork.Broadcast`), um mecanismo completamente diferente e não relacionado a este pacote novo.

**Por que importa:** A evidência citada não sustenta a afirmação — quem ler a spec pode presumir erroneamente que o pacote novo passa por uma fila com proteção de overflow/thread-safety, quando na verdade ele é enviado direto e só é seguro porque `TriggerBotVoiceEvent` já roda inteiramente na main thread (via `VoipController.Update()` → `ProcessVoiceFrame`).

**Sugestão:** Reescrever a evidência do check 1 (AP-01) para: *"`SendVoicePowerToHost` não introduz recurso persistente — é uma chamada de envio pontual dentro de `TriggerBotVoiceEvent`, já executada na main thread a cada frame de `VoipController.Update()` (existente, sem novo hook de start/stop necessário)."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Evidência do check 1 (§9 da spec técnica) reescrita conforme sugerido.

---

### PA-01-02 · ✅ Resolvido em 2026-09-09 · B — Edge Case · 🟡 Importante

**Timing de `VoipController.Instance`/`botVoiceBridge` não nulo no recebimento do pacote**

**Problema:** `OnReceiveBotVoicePower` (§5) acessa `Core.VoipController.Instance?.botVoiceBridge?.NotifyBotsOfVoice(...)` com null-conditional — se `VoipController.Instance` ou `botVoiceBridge` ainda não estiverem prontos (ex.: pacote chega nos primeiros frames da raid, antes do `VoipController` do host terminar sua própria inicialização), a chamada inteira é silenciosamente descartada (não lança, mas também não notifica o bot). A spec não diz se esse descarte silencioso é o comportamento **pretendido** ou um efeito colateral não examinado.

**Por que importa:** Se um convidado falar bem no início da raid (comum — "opa, cheguei"), o aviso ao host pode ser perdido sem log e sem retry, e ninguém vai perceber a causa porque não há nenhuma mensagem indicando que isso aconteceu.

**Sugestão:** Documentar explicitamente na seção 7 (Riscos) que esse descarte é aceitável (janela de poucos frames, impacto mínimo — bot só "perde" uma frase isolada, não a interação como um todo) OU, se preferir mais robustez, adicionar um log (`LogErrorThrottled`-style, mas em nível Info/Debug pra não parecer erro) quando `VoipController.Instance` ou `botVoiceBridge` forem nulos no recebimento, só pra visibilidade durante testes.

**Decisão:**
- `[x]` Aceitar sugestão (caminho 1 — documentar como aceitável, sem log adicional)

**Resolução:** Nota adicionada logo após o stub de `OnReceiveBotVoicePower` (§5 da spec técnica) documentando o descarte silencioso como comportamento aceitável.

---

### PA-01-03 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟢 Menor

**Falta nota explícita sobre arquivo compartilhado com o item 014**

**Problema:** A seção 7 (Riscos) menciona a sinergia conceitual com o item `014-spatial-culling-host-side/` (ambos processam dados de convidados no host), mas não menciona que **os dois itens modificam `Network/SftNetwork.cs`** — este item toca `EnsurePacketsRegistered` (adiciona `SftBotVoicePowerPacket`) e adiciona `OnReceiveBotVoicePower`; o item 014 toca `DrainSendQueue`, `OnReceiveVoipDataV2` e adiciona `RelayVoiceToNearbyPeers`. São métodos diferentes no mesmo arquivo — risco de conflito é baixo, mas o item `013` já documentou esse mesmo tipo de aviso pro `RemoteSpeaker.cs` compartilhado com o item `011`, e este item não seguiu a mesma consistência.

**Por que importa:** Sem o aviso, quem implementar os itens 010 e 014 em paralelo (ex.: duas sessões/branches diferentes) pode gerar um merge menos trivial do que o esperado, ou simplesmente não perceber que está no mesmo arquivo até o diff ficar confuso.

**Sugestão:** Adicionar à seção 7: *"Este item e o `014-spatial-culling-host-side/` tocam `Network/SftNetwork.cs` em métodos diferentes (`EnsurePacketsRegistered`/`OnReceiveBotVoicePower` aqui vs. `DrainSendQueue`/`OnReceiveVoipDataV2`/`RelayVoiceToNearbyPeers` no 014) — risco de conflito de merge baixo, mas implementar em sequência (não em paralelo) evita qualquer surpresa."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Nota adicionada ao final do primeiro item de risco na seção 7 da spec técnica.

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).

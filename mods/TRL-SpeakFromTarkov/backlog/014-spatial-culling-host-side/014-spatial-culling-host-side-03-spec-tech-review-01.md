# 014 — Spatial Culling Host-Side · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [014-spatial-culling-host-side-02-spec-tech.md](014-spatial-culling-host-side-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | `RelayVoiceToNearbyPeers` itera `AllAlivePlayersList` — jogadores mortos/espectadores param de receber voz de vivos | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 Menor | TODO confirmar da seção 7 (hierarquia `FikaPlayer`) já pode ser resolvido — `ObservedPlayer : FikaPlayer` confirmado | ✅ Resolvido |

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

### PA-01-01 · ✅ Resolvido em 2026-09-09 · C — Erro de Lógica · 🔴 Bloqueador

**`RelayVoiceToNearbyPeers` itera `AllAlivePlayersList` — jogadores mortos/espectadores param de receber voz de vivos**

**Problema:** O stub de `RelayVoiceToNearbyPeers` (seção 5) itera `Singleton<GameWorld>.Instance.AllAlivePlayersList` pra decidir a quem retransmitir. Confirmei em `references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs:556` que esse campo é `List<Player>` e, por `GameWorld.cs:2262-2278` (`RegisterPlayer`), só recebe jogadores **vivos** — mortos nunca entram nessa lista (e são removidos dela ao morrer, padrão já confirmado em `2098-2106`/`2187`). Já `docs/03-rede-e-protocolo.md` (lido nesta sessão em investigação anterior) documenta que jogadores mortos/espectadores (Canal de Channel==2 no app, mas ainda recebendo Canal 0) têm "escuta dupla": **recebem o Canal 0 em 3D dos amigos vivos ao redor da câmera assistida**, além do Canal 2 em 2D. O campo certo pra pegar TODOS os jogadores registrados (vivos e mortos) é `RegisteredPlayers` (`List<IPlayer>`, `GameWorld.cs:546`, populado incondicionalmente em `RegisterPlayer:2262`).

**Por que importa:** Se implementado com `AllAlivePlayersList`, jogadores mortos/espectadores **parariam de receber a voz de jogadores vivos por rede** assim que este item entrar em produção — regressão direta de uma feature já existente e documentada (escuta dupla do espectador), não coberta por nenhum teste do checklist atual (que só testa "convidado dentro/fora de alcance", sempre implicitamente vivo).

**Sugestão:** Trocar, em `RelayVoiceToNearbyPeers` (seção 5), `var allPlayers = Singleton<GameWorld>.Instance.AllAlivePlayersList;` por `var allPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;` (tipo `List<IPlayer>`, ajustar a assinatura do loop de `Player candidate` pra `IPlayer candidate` e o cast `candidate is FikaPlayer` continua funcionando via `IPlayer`/`FikaPlayer`). Adicionar ao checklist de testes (seção 8): *"Jogador morto/espectador continua recebendo a voz de jogadores vivos dentro do alcance, exatamente como hoje — testar com pelo menos 1 jogador morto na raid."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `RelayVoiceToNearbyPeers` (§5) agora itera `GameWorld.RegisteredPlayers`; diagrama de fluxo (§6) e checklist (§8) atualizados com o teste de jogador morto/espectador.

---

### PA-01-02 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟢 Menor

**TODO confirmar da seção 7 já pode ser resolvido — `ObservedPlayer : FikaPlayer` confirmado**

**Problema:** A seção 7 (Riscos) tem um `TODO confirmar` dizendo não ter sido lida a hierarquia completa de `FikaPlayer`/`ObservedPlayer` nesta sessão, e sugere `ObservedPlayers` da interface como plano B se o cast `candidate is FikaPlayer` não funcionar pra jogadores observados. Confirmei agora: `references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs:41` — `public sealed class ObservedPlayer : FikaPlayer`. O cast funciona pra qualquer jogador humano remoto observado pelo host, não só `MyPlayer` local.

**Por que importa:** O TODO como está pode levar quem implementar a gastar tempo testando/validando algo que já está confirmado por evidência de Assembly — ou a adotar o plano B (`ObservedPlayers`) sem necessidade.

**Sugestão:** Remover o `TODO confirmar` da seção 7 e substituir por: *"Confirmado: `ObservedPlayer : FikaPlayer` (`Fika.Core/Main/Players/ObservedPlayer.cs:41`) — o cast `candidate is FikaPlayer` em `RelayVoiceToNearbyPeers` funciona pra qualquer jogador humano remoto observado pelo host."* Também remover o item correspondente do checklist (seção 8, primeiro item) já que não há mais nada a confirmar aqui.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `TODO confirmar` da seção 7 substituído pela confirmação; item correspondente removido do checklist (§8).

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).

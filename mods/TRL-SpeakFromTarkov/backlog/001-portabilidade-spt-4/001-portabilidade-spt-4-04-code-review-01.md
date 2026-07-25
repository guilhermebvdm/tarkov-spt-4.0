# 001 — portabilidade-spt-4 · Code Review 01

**Mod:** TRL-ImmersiveVoip
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** (Ignorado neste fluxo manual)
**Data:** 2026-07-16

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 1 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | A — Crítico | 🔴 Bloqueador | Criação de GameObject a cada frame de áudio no NetworkManager | Pendente |
| CR-01-02 | C — Gap vs. spec | 🟠 Forte | Desativação do Fika VOIP não implementada | Pendente |
| CR-01-03 | E — Legibilidade/manutenção | 🟢 Menor | OnDestroy vazio no VOIPPlugin | Pendente |

---

## Pontos

### CR-01-01 · A — Crítico · 🔴 Bloqueador

**Criação de GameObject a cada frame de áudio no NetworkManager**

**Local:** [`mods/TRL-ImmersiveVoip/modded/NetworkManager.cs:58`](../../modded/NetworkManager.cs#L58)

**Problema:** A cada pacote Opus recebido (o que acontece dezenas de vezes por segundo), o método `OnReceiveVoipData` está instanciando um novo `GameObject`, adicionando um `AudioSource`, tocando o som, e destruindo em seguida.

**Por que importa:** Em Unity, a constante alocação e desalocação (`Instantiate` e `Destroy`) de `GameObjects` num loop de alta frequência causa picos brutais no Garbage Collector, resultando em stutters graves na raid.

**Sugestão:** Implementar um Dicionário que mapeie o `ProfileId` para um `AudioSource` persistente (ex: anexado diretamente ao pescoço do `EFT.Player` remoto correspondente, ou em um objeto pool reutilizável). Alimentar este source com o `clip` gerado, ou melhor ainda, utilizar streaming no clip em vez de tocar múltiplos clips de 20ms separadamente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · C — Gap vs. spec · 🟠 Forte

**Desativação do Fika VOIP não implementada**

**Local:** [`mods/TRL-ImmersiveVoip/modded/VOIPPlugin.cs:88`](../../modded/VOIPPlugin.cs#L88)

**Problema:** O método antigo que chamava `_harmony.PatchAll(typeof(Fika.Core.Networking.VOIP.FikaVOIPClient));` foi removido e substituído apenas por um `Log.LogInfo` de aviso que a desativação não é mais suportada via Harmony global. Contudo, o critério da spec técnica exigia desativar a rede nativa do Dissonance se a flag estivesse ativada.

**Por que importa:** Sem desativar o Dissonance, os jogadores podem sofrer eco ou o PTT do Fika ser disparado simultaneamente com o nosso, conflitando o áudio.

**Sugestão:** Criar um patch específico para `Fika.Core.Networking.VOIP.FikaCommsNetwork.Update` ou desativar a instância do DissonanceComms na raid através de um script anexado ao `GameWorld`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · E — Legibilidade/manutenção · 🟢 Menor

**OnDestroy vazio no VOIPPlugin**

**Local:** [`mods/TRL-ImmersiveVoip/modded/VOIPPlugin.cs:102`](../../modded/VOIPPlugin.cs#L102)

**Problema:** Foi deixado um método `void OnDestroy() {}` vazio após a remoção do unpatch global do Harmony.

**Por que importa:** Código morto/poluição visual.

**Sugestão:** Remover o método `OnDestroy()`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-16 | Code review 01 criada via fluxo automatizado `/code-review` |

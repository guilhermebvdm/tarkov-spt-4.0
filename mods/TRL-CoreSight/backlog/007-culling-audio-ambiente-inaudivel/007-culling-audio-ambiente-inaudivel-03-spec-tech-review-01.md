# 007 — culling-audio-ambiente-inaudivel · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [007-culling-audio-ambiente-inaudivel-02-spec-tech.md](007-culling-audio-ambiente-inaudivel-02-spec-tech.md)  
**Data:** 2026-09-15T22:50:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 Importante | Preservação de emissores desligados ou ativados condicionalmente (Power Switches) | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | C — Erro de Lógica | 🟡 Importante | Prevenção de exceções em `AudioSource` inativo ou destruído | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | A — Gap | 🟢 Menor | Reescaneamento periódico para emissores ativados tardiamente | ✅ Resolvido em 2026-09-15 |

---

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

### PA-01-01 · B — Edge Case · 🟡 Importante

**Preservação de emissores desligados ou ativados condicionalmente (Power Switches)**

**Problema:** Em mapas como Interchange e Reserve, certas lâmpadas, caixas de força e alarmes só entram em execução quando o jogador aciona o disjuntor principal de energia (power station). Se o mod tentar invocar `Pause()` / `UnPause()` indiscriminadamente, pode ligar um som que ainda deveria estar desligado.

**Por que importa:** Um som de gerador ou lâmpada não deve começar a tocar se o jogo ainda não disparou o `Play()` nele.

**Sugestão:** Apenas pausar fontes que estejam ativamente tocando (`item.Source.isPlaying`) quando saírem do alcance. Ao retornar ao alcance, só chamar `UnPause()` se o estado anterior foi explicitamente pausado pelo nosso gerenciador (`item.IsPaused == true`).

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionado flag booleano de controle estrito `IsPausedByMod`, garantindo que apenas sons pausados pelo mod sejam retomados, sem alterar sons que nunca foram iniciados.

---

### PA-01-02 · C — Erro de Lógica · 🟡 Importante

**Prevenção de exceções em `AudioSource` inativo ou destruído**

**Problema:** Chamar `audioSource.Pause()` ou `audioSource.UnPause()` em um componente desativado na hierarquia (`!src.isActiveAndEnabled`) ou com `src.clip == null` pode gerar avisos/erros no console da Unity.

**Por que importa:** Limpeza de logs e estabilidade em raid.

**Sugestão:** Adicionar verificação de `src.isActiveAndEnabled` e tratamento defensivo no loop de avaliação em lotes.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Condições de guarda adicionadas no `AmbientAudioCullingManager`.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Reescaneamento periódico para emissores ativados tardiamente**

**Problema:** Emissões instanciadas dinamicamente após o início da raid (por exemplo, após ligar a chave de força geral) podem não estar na lista se a varredura ocorrer apenas uma vez no spawn.

**Por que importa:** Garante que 100% dos sons ambientes contínuos sejam gerenciados durante toda a raid.

**Sugestão:** Executar um escaneamento leve a cada 60 segundos ou quando houver troca de configuração no F12.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Implementado timer de re-escaneamento a cada 60 segundos.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |

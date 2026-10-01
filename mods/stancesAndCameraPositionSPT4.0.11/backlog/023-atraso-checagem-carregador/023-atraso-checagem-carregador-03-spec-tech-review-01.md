# 023 — Atraso na checagem de carregador (Magazine Check Delay) · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica revisada:** [023-atraso-checagem-carregador-02-spec-tech.md](023-atraso-checagem-carregador-02-spec-tech.md)
**Data:** 2026-09-22

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod` — neste caso, o item já está implementado; zerar bloqueadores libera o `/code-review`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 | Nenhum checklist de validação in-game preenchido (AP-06) | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 | Fluxo de dados não menciona o gate `_ShowChamberAmmoOnCheck` antes do ponto compartilhado | ✅ Resolvido |

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

### PA-01-01 · A — Gap · 🟡 Importante

**Nenhum checklist de validação in-game/Fika/entre-raids foi preenchido pra este item (AP-06)**

**Problema:** a feature já roda em produção (build 2.25.x), mas nunca passou por `/code-mod` (que gera
o `05-asbuild.md` com o checklist de validação) — não existe registro de que alguém confirmou, em raid
real, que: (a) o atraso realmente funciona pro carregador E pra câmara, (b) o cancelamento de check
duplo funciona (segunda checagem rápida cancela a primeira), (c) o `OnRaidEnd()` realmente impede o
painel de vazar entre raids. É exatamente o sintoma que `docs/technical/spt-antipatterns.md` AP-06
descreve: "fix pronto que nunca foi observado funcionando — compila é tratado como funciona".

**Por que importa:** sem essa validação, não dá pra fechar este item como 🟢 Entregue com confiança —
o código PARECE correto na leitura (§9 da spec técnica), mas leitura de código não substitui teste em
raid real, principalmente pra Coroutines com múltiplos pontos de saída antecipada (`yield break`).

**Sugestão:** ao gerar o `05-asbuild.md` retroativo (já listado no checklist §8 da spec técnica), incluir
a seção "Validação pendente" com os itens: (1) checar carregador, confirmar atraso de ~2s; (2) checar
câmara, confirmar que TAMBÉM atrasa (validando o achado desta spec); (3) checar duas vezes rápido,
confirmar que só um painel aparece com o dado da segunda checagem; (4) trocar de arma durante o atraso,
confirmar que nenhum painel aparece depois; (5) sair de raid com atraso pendente, confirmar que não
vaza pra próxima raid. Pedir ao usuário pra rodar esses 5 testes antes de marcar o item 🟢.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** aplicado na spec técnica (§8, checklist) — item "Gerar `023-...-05-asbuild.md` retroativo" expandido com os 5 testes de validação listados acima.

### PA-01-02 · A — Gap · 🟢 Menor

**Fluxo de dados (§6) não menciona o gate `_ShowChamberAmmoOnCheck` antes do ponto compartilhado**

**Problema:** o fluxo `[A2]` na §6 da spec técnica mostra `ChamberCheckAmmoPatch` chamando
`screen.ShowAmmoDetails(...)` direto, mas omite que esse Postfix **primeiro** checa
`if (!Plugin._ShowChamberAmmoOnCheck.Value) return;` (`ChamberCheckAmmoPatch.cs` linha inicial do
Postfix) — ou seja, com essa config desligada, o fluxo `[A2]` nem chega a existir; `[D]` só é alcançado
por `[A1]` (carregador nativo). Isso responde diretamente o corner case da spec funcional ("checar a
câmara com `Show Chamber Ammo On Check` desligado não deve produzir painel fantasma") mas a spec
técnica não deixa essa conexão explícita.

**Por que importa:** um leitor futuro da spec técnica (ou quem for validar o corner case da spec
funcional) precisaria reabrir `ChamberCheckAmmoPatch.cs` pra descobrir isso — a spec técnica deveria
já entregar essa resposta, já que é o artefato que documenta o fluxo completo.

**Sugestão:** adicionar uma linha em `[A2]` da §6: `(gated por Plugin._ShowChamberAmmoOnCheck — se
desligado, [A2] nunca dispara e o corner case correspondente da spec funcional já está trivialmente
resolvido: sem chamada, sem painel)`.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** aplicado na spec técnica (§6, fluxo `[A2]`) — linha do gate `_ShowChamberAmmoOnCheck` adicionada.

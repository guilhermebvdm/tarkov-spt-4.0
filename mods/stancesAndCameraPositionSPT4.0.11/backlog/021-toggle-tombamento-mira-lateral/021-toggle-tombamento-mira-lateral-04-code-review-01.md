# 021 — Toggle para tombamento de mira lateral · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [021-toggle-tombamento-mira-lateral-01-spec.md](021-toggle-tombamento-mira-lateral-01-spec.md)
**Spec técnica:** [021-toggle-tombamento-mira-lateral-02-spec-tech.md](021-toggle-tombamento-mira-lateral-02-spec-tech.md)
**Asbuild:** [021-toggle-tombamento-mira-lateral-05-asbuild.md](021-toggle-tombamento-mira-lateral-05-asbuild.md)
**Data:** 2026-09-08

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: nenhuma.
**Docs técnicos:** `spt-antipatterns.md` conferido — nenhuma violação de AP-01…AP-09 encontrada no código implementado (mesmas conclusões da spec técnica §9, evidências reconferidas contra o código real, não só contra o stub).
**Verificação:** as 3 mudanças de código (`Plugin.cs`, `ApplyComplexRotationPatch.cs`, `ApplySimpleRotationPatch.cs`) foram lidas por inteiro e comparadas trecho a trecho com os stubs da spec técnica §5 — implementação bate com o planejado, sem desvio.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟡 Médio | Critérios de aceite visuais ainda não validados in-game | Pendente |
| CR-01-02 | F — Melhoria opcional | 🟢 Menor | Confirmar em qual contexto `ApplySimpleRotation` realmente dispara para o jogador local | Pendente |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · C — Gap vs. spec · 🟡 Médio

**Critérios de aceite visuais da spec funcional ainda não validados in-game**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ApplyComplexRotationPatch.cs:392-397`](../../modded/Patches/ApplyComplexRotationPatch.cs#L392-L397), [`ApplySimpleRotationPatch.cs:210-215`](../../modded/Patches/ApplySimpleRotationPatch.cs#L210-L215)

**Problema:** O código implementa exatamente o que a spec técnica planejou (composição `weapRotation * effectiveScopeRotation * CurrentRotation`), mas os critérios de aceite #1–#3 da spec funcional ("a arma tomba com a opção desativada", "tombamento aparece combinado com a postura customizada", "com a opção ativada, comportamento idêntico ao atual") são todos verificáveis só visualmente, em raid, com uma arma real equipada com mira de trilho lateral. A checklist da spec técnica (§8, itens 7–8) segue com as caixas de "compilar" e "validar in-game" desmarcadas, e o `05-asbuild.md` registra isso explicitamente no Histórico.

**Por que importa:** O item já está marcado 🟢 no `mod-backlog.md` (convenção do `/code-mod`: 🟢 ao concluir o build de código). Sem a validação visual, existe risco real de a ordem de composição escolhida (`scopeRotation` antes de `CurrentRotation` — ver PA-01-01 da review técnica) produzir uma pose estranha ao combinar Stance customizada + mira lateral, e isso só vai aparecer quando alguém testar em raid.

**Sugestão:** Antes de considerar o item verdadeiramente fechado (não só "código pronto"), rodar `/compile-mod stancesAndCameraPositionSPT4.0.11` e validar em raid os 5 cenários do item 8 do checklist da spec técnica — em especial o (b) (Stance 1/2/3 + mira lateral), que é o cenário que a PA-01-01 identificou como potencialmente sensível à ordem de composição. Se a pose parecer errada, aplicar o plano B já documentado na spec técnica §1 (inverter para `weapRotation * CurrentRotation * scopeRotation`).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · F — Melhoria opcional · 🟢 Menor

**Confirmar em qual contexto `ApplySimpleRotation` realmente dispara para o jogador local**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ApplySimpleRotationPatch.cs:114-128`](../../modded/Patches/ApplySimpleRotationPatch.cs#L114-L128); comentário pré-existente em `ApplySimpleRotationPatch.cs:189-190` ("CR-08 — ... Se este caminho voltar a rodar, o slider vale")

**Problema:** O caller nativo de `ApplySimpleRotation` é `GClass910.ApplyTransformations` ([`GClass910.cs:36-45`](../../../../references/eft-decompiled/Assembly-CSharp/GClass910.cs#L36-L45)), e o comentário no topo desse arquivo traz um alias comunitário não verificado (`// [SPT 4.1 alias — RÓTULO...] ThirdPersonStrategy`). Um comentário pré-existente do próprio mod (`ApplySimpleRotationPatch.cs:189-190`, não introduzido por este item) já registra incerteza sobre se esse caminho está de fato ativo hoje. Isso é uma constatação, não um erro de código — a mudança deste item foi replicada corretamente nos dois arquivos como a spec técnica pediu.

**Por que importa:** Se `ApplySimpleRotation` só roda para contextos onde `player.IsYourPlayer` é `false` (ex.: renderização de terceiros ou uma câmera alternativa), o guard já existente na linha 146-150 faz o Postfix retornar cedo sempre, e a mudança deste item nesse arquivo seria correta porém inerte na prática — sem impacto funcional, mas também sem risco. Isso não é uma regressão (o guard já existia antes deste item), só uma lacuna de certeza sobre qual dos dois arquivos o testador deve observar ao validar o critério de aceite.

**Sugestão:** Ao validar in-game (ver CR-01-01), anotar no `06-fix-NN.md` ou na memória do mod qual dos dois Postfix (`ApplyComplexRotationPatch` ou `ApplySimpleRotationPatch`) de fato disparou para o jogador local nas configurações gráficas testadas — resolve a ambiguidade de uma vez por todas para futuras specs que toquem este mesmo par de arquivos. Não é necessário nenhum código novo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-08 | Code review 01 criada via `/code-review` |

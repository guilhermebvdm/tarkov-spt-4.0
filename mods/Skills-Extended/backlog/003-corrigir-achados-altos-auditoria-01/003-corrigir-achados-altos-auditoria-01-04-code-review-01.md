# 003 — Corrigir achados altos da auditoria 01 · Code Review 01

**Mod:** Skills-Extended
**Spec funcional:** [003-corrigir-achados-altos-auditoria-01-01-spec.md](003-corrigir-achados-altos-auditoria-01-01-spec.md)
**Spec técnica:** [003-corrigir-achados-altos-auditoria-01-02-spec-tech.md](003-corrigir-achados-altos-auditoria-01-02-spec-tech.md)
**Asbuild:** [003-corrigir-achados-altos-auditoria-01-05-asbuild.md](003-corrigir-achados-altos-auditoria-01-05-asbuild.md)
**Data:** 2026-09-07

> Análise crítica do código implementado por `/code-mod`. Memória consultada: snapshot de 2026-09-07 (Sessão 1) · pendências que afetam: nenhuma. Docs técnicos lidos: `spt-antipatterns.md` (gatilho obrigatório) — nenhum outro doc disparado (sem `ConfigEntry` nova, sem pacote FIKA novo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ⏭️ Rejeitados: 1 · Total: 1

**Nota geral:** revi os 10 arquivos tocados linha a linha contra o Assembly real e contra a spec funcional/técnica. Confirmei que o build compilou de fato (Plugin + Common, 0 erros de código, `dotnet build` redirecionado — ver asbuild). O único achado desta rodada não é um bug de implementação — o código bate exatamente com o que a spec técnica descreveu — é uma divergência entre o que a **spec funcional** promete (critério de aceite) e o que a **spec técnica** decidiu aceitar como débito (Recoil). Vale destacar isso formalmente aqui porque um leitor futuro que só olhar a spec funcional (sem ler a spec técnica §1.2 inteira) vai achar que o critério foi cumprido por completo.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟠 Forte | Critério de aceite "ergonomia/recuo" da spec funcional não é totalmente satisfeito — Recoil continua vazando entre entidades em coop | ⏭️ Rejeitado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa um critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código existente, abuso de reflection, leak de estado entre raids.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · C — Gap vs. spec · 🟠 Forte · ⏭️ Rejeitado em 2026-09-07

**O critério de aceite "ergonomia/recuo" da spec funcional não é totalmente satisfeito — Recoil ainda muta o template compartilhado**

**Local:** [`mods/Skills-Extended/modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs:137-138,210-211`](../../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs#L137-L138) — comparar com a spec funcional [003-...-01-spec.md:28](../003-corrigir-achados-altos-auditoria-01-01-spec.md#L28).

**Problema:** O critério de aceite da spec funcional (linha 28) diz, literalmente:

> "As estatísticas de ergonomia/recuo de uma arma ajustadas pela skill de armas do jogador local não alteram o comportamento da mesma arma (mesmo tipo/modelo) quando usada por outra entidade na mesma sessão."

Este único critério **bundla** dois valores (ergonomia **e** recuo) sob a mesma exigência. O código implementado resolve por completo a metade de Ergonomia (via `ErgonomicsTotalPatch`, por-instância, confirmado), mas **Recoil continua exatamente como antes**:

```csharp
// UpdateWeaponsPatch.cs:137-138 (UpdateUsecWeapons) — idêntico em UpdateEasternWeapons:210-211
weapon.Template.RecoilForceUp = UsecOriginalWeaponValues[item.TemplateId].weaponUp * (1 - skillMgrExt.UsecArSystemsRecoilBuff);
weapon.Template.RecoilForceBack = UsecOriginalWeaponValues[item.TemplateId].weaponBack * (1 - skillMgrExt.UsecArSystemsRecoilBuff);
```

Essa mutação continua escrevendo no `Template` **compartilhado por `TemplateId`** — exatamente o mecanismo que o critério de aceite pede para eliminar. Isso não é um bug de implementação: bate exatamente com o que a spec técnica (`003-...-02-spec-tech.md` §1.2) documentou e o usuário aceitou explicitamente como débito técnico durante o `/review-technical-spec` ("aceito o recuo como está"), após confirmação de que não existe nenhuma property por-instância equivalente a `ErgonomicsTotal` para Recoil no Assembly (`Weapon.RecoilForceBack` é só um repasse direto de `Template.RecoilForceBack`, e `RecoilForceUp` não tem property de instância alguma).

**Por que importa:** A spec funcional é o documento que descreve o comportamento **prometido ao usuário/jogador** — é o artefato que alguém consultaria pra saber "isso já foi resolvido?". Hoje, o checkbox da linha 28 não tem nenhuma anotação indicando que só a metade Ergonomics foi resolvida — um leitor futuro (inclusive numa auditoria de continuidade) marcaria esse critério como "implementado" sem perceber que Recoil continua com o comportamento antigo: em uma sessão cooperativa, se um bot ou outro jogador usar uma arma do mesmo modelo que a do jogador local com Weapon Systems treinado, o recuo dessa arma (renderizado no cliente local, conforme o `Player.cs:26489` já documentado na spec técnica) continua refletindo a skill do jogador local, não a do dono real.

**Sugestão:** Duas ações complementares, não mutuamente exclusivas:
1. Editar a spec funcional (`003-...-01-spec.md` linha 28) pra desmembrar o critério em dois, deixando explícito o que foi de fato entregue:
   ```markdown
   - [x] Ergonomia de uma arma ajustada pela skill de armas do jogador local não altera o comportamento da mesma arma (mesmo tipo/modelo) quando usada por outra entidade na mesma sessão.
   - [ ] Recuo de uma arma ajustado pela skill de armas do jogador local não altera o comportamento da mesma arma quando usada por outra entidade — **N/A nesta rodada: aceito como débito técnico, ver `003-...-02-spec-tech.md` §1.2 (nenhuma property por-instância equivalente a `ErgonomicsTotal` existe pra Recoil no Assembly).**
   ```
2. Registrar o achado Recoil (a metade não resolvida) como pendência 🟡 explícita na memória do mod (`mods/Skills-Extended/memory/sessions.md`), pra não se perder entre sessões — é um candidato natural a reabrir numa investigação futura (ex.: via profiling/logging em runtime pra achar o consumidor real de `RecoilForceUp`, já que a busca estática nesta sessão não encontrou).

Não é bloqueador porque a decisão de aceitar o débito já foi tomada conscientemente pelo usuário na etapa de spec técnica — o ponto aqui é só garantir que a **spec funcional não fique com um critério de aceite silenciosamente incompleto**.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[x]` Rejeitar (deferir / aceitar como dívida): usuário confirmou explicitamente ("ignora o recoil, pode seguir") — débito técnico aceito sem editar a spec funcional. Recoil permanece como está, documentado na spec técnica §1.2.

**Resolução:** Rejeitado — usuário optou por não desmembrar o critério de aceite nem editar a spec funcional; o débito técnico de Recoil (já documentado em `003-...-02-spec-tech.md` §1.2) fica registrado só ali e neste ponto do code-review, sem alteração adicional.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-07 | Code review 01 criada via `/code-review` |
| 2026-09-07 | Aplicação automática via `/apply-code-review` — rejeitado: CR-01-01 (débito técnico aceito por decisão explícita do usuário) |

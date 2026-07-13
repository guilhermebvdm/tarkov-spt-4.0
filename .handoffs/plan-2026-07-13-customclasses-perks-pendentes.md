# Plano autônomo — perks pendentes do CustomClasses (execução noturna)

> **Data:** 2026-07-13<br>
> **Status:** 🔵 Em andamento<br>
> **Responsáveis:** Guilherme (autorizou execução autônoma; estará dormindo)<br>
> **Referências:** [mod-backlog.md](../mods/CustomClasses/backlog/mod-backlog.md), [050-signature-patches-05-asbuild.md](../mods/CustomClasses/backlog/050-signature-patches/050-signature-patches-05-asbuild.md)<br>

---

## Objetivo

Implementar os perks que restam do item **050** (os que o catálogo ainda marca como `pending: true` e aparecem como **"soon"** no painel), seguindo o **workflow SDD do repo**, de forma **autônoma até o fim**, sem pedir aprovação.

## Escopo — o que implementar

| # | Perk | Classe | Efeito | Alvo técnico (verificar antes de codar) |
|---|---|---|---|---|
| 1 | **Calm Sights** / Mira Serena | 🎯 Caçador | sway **×0.7** | `ProceduralWeaponAnimation.UpdateSwayFactors()` recalcula → **Postfix** multiplicando `MotionEffector.SwayFactors` (MotionEffector.cs:28). ⚠️ É recalculado: **não** escrever direto (seria sobrescrito) |
| 2 | **Rapid Care** / Cuidado Rápido | 🩺 Médico | tempo de cura/estabilização **×0.7** | duração é **var local** em `ActiveHealthController.DoMedEffect` |
| 3 | **Swift Surgeon** / Cirurgião Ágil | 🩺 Médico | tempo de cirurgia **×0.5** | mesma família do #2 |

### ⚠️ Fora do escopo de implementação — **Mobile Surgery**

O as-build (050, linha 105) registra: *"cirurgia sem lock de movimento **não foi localizável no estático** → precisa investigação em runtime"*. **Não implementar às cegas.** Em vez disso: fazer uma **investigação estática aprofundada** (decompile + grafo) e entregar um **relatório** com os alvos candidatos, o que falta confirmar e como confirmar in-game. Se — e somente se — surgir evidência **conclusiva** no decompile, aí sim implementar.

## Decisão técnica que o agente precisa tomar (#2 e #3)

O as-build aponta **dois caminhos**:

- **(A) Transpiler** em `ActiveHealthController.DoMedEffect` (a duração é variável local).
- **(B) Par de patches**: `HealthEffectsComponent.UseTimeFor` + `FirearmsAnimator.SetUseTimeMultiplier` — para **casar efeito e animação** (se só um for patchado, o item cura em 0.7× mas a animação continua no tempo cheio, ou vice-versa → dessincronia visível).

**Preferir (B).** Transpiler é o último recurso do `csharp-mod-best-practices` (§3: *"quebram em toda atualização do EFT"*). Só ir de (A) se (B) provar-se inviável **com evidência** — e, nesse caso, o transpiler tem que ser defensivo (validar o padrão de IL antes de emitir; falhar para no-op, nunca para exceção).

⚠️ **O risco real aqui é dessincronia efeito↔animação.** A spec técnica precisa dizer explicitamente qual patch controla o quê e provar que os dois usam o mesmo multiplicador.

## Workflow obrigatório (SDD do repo)

Para os perks do escopo, seguir a cadeia — **cada etapa é um artefato**:

1. `/add-backlog-item` (ou entrada manual) → item novo no `mod-backlog.md`
2. `/create-spec` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec` → revisão crítica
4. `/create-technical-spec` → **refs `arquivo.cs:linha` do Assembly, verificadas uma a uma**
5. `/review-technical-spec` → **bloqueadores 🔴 têm que ser resolvidos antes do código**
6. `/code-mod` → implementação em `mods/CustomClasses/modded/`
7. `/code-review` → **adversarial**; e `/apply-code-review` nos achados
8. `/update-mod-graph` → regenerar o grafo

## Guardrails (INEGOCIÁVEIS)

- **NUNCA `git push`.** Nem `gh pr`/`gh issue`. Push exige aprovação humana. Commits locais são livres.
- **Commit cirúrgico:** só `mods/CustomClasses/**` e `references/graphs/mods/CustomClasses/**`. A árvore tem trabalho **de outra sessão** (`mods/TRL-ItemsManagement/**`) — **não tocar, não commitar**.
- **Build tem que fechar com 0 warnings e 0 erros** (`bash .agents/scripts/compile-mod.sh CustomClasses`). Build quebrada = reverter, não seguir em frente.
- **Não dá para testar in-game** (o agente não joga). A validação possível é: build limpa + code-review adversarial + refs do Assembly conferidas. **Não afirmar que algo "está funcionando"** — dizer "implementado, pendente de validação in-game".
- **Não inventar refs do Assembly.** Toda linha citada (`arquivo.cs:NNN`) tem que ser aberta e conferida. Ref inventada é falha grave.
- Se um perk se mostrar **inviável** com a evidência disponível: **parar aquele perk**, documentar o porquê e seguir para o próximo. Entregar 2 perks sólidos > 3 perks quebrados.

## Ao terminar (tudo isso é parte do trabalho)

- **`PerksCatalog.cs`**: tirar o `pending: true` dos perks implementados (senão o painel segue mostrando **"soon"** — foi exatamente a reclamação do usuário uma vez).
- **F12** (`PerksConfig.cs`): props novas na **seção da classe** (`2 · Combat Medic`, `4 · Hunter`), com `AcceptableValueRange` e descrição **bilíngue PT / EN** (padrão do arquivo).
- **`PROPRIEDADES.md` + `PROPERTIES.md`**: documentar as props novas nos **dois** (o repo mantém os dois espelhados).
- **`mod-backlog.md`** e o **board** (`balance-review-2026-07-05.md`, item **B12**): atualizar status.
- **Memória do mod** (`mods/CustomClasses/memory/sessions.md`): registrar sessão, decisões e pendências.

## Relatório final (é o que o usuário vai ler ao acordar)

Escrever em `.handoffs/report-2026-07-13-customclasses-perks-pendentes.md`:

1. **O que foi entregue** — por perk, com o commit.
2. **O que NÃO foi** e **por quê** (esp. Mobile Surgery).
3. **Decisões tomadas sozinho** (esp. transpiler vs par de patches) e a razão.
4. **O que precisa de validação in-game** — um roteiro curto de teste, no molde de [coop-sound-test-plan.md](../mods/CustomClasses/docs/coop-sound-test-plan.md).
5. **Riscos conhecidos** que ficaram no código.

# 007 — Impulso de Tiro Menos Dependente do Peso do Item · Review Técnica 01

**Mod:** VisceralCombat
**Spec técnica revisada:** [007-piso-impulso-tiro-itens-02-spec-tech.md](007-piso-impulso-tiro-itens-02-spec-tech.md)
**Data:** 2026-09-21

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** snapshot de 2026-09-21 (Sessão 14), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam esta revisão:** nenhuma. **Docs técnicos conferidos:** `spt-antipatterns.md` (sempre) — nenhuma contradição encontrada entre a spec e a taxonomia (§9 da spec técnica preenchida com 2 ✅ evidenciados e 9 N/A justificados, consistentes com o fato de este item não introduzir nenhum ponto de patch novo).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Divisão por massa efetiva sem piso mínimo — risco de item com peso zero/quase-zero gerar velocidade absurda | ✅ Resolvido em 2026-09-21 |
| PA-01-02 | A — Gap | 🟢 Menor | Torque/rotação continuam escalando com a massa real (não com a massa "grampeada") — não documentado | ✅ Resolvido em 2026-09-21 |

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

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**Divisão por massa efetiva sem piso mínimo — risco de item com peso zero/quase-zero gerar velocidade absurda**

**Problema:** O stub de §5 calcula `effectiveMass = Mathf.Min(lootRb.mass, MassCapKg)` e em seguida `Vector3 deltaV = shot.Direction * (physicalImpulse / effectiveMass);` — uma divisão feita manualmente em C#, sem a proteção interna que a Unity aplica normalmente ao converter `ForceMode.Impulse` (que internamente já lida com massa mínima do `Rigidbody`). Como `MassCapKg` (`0.5f`) é sempre positivo, `effectiveMass` só pode chegar a zero (ou próximo disso) se `lootRb.mass` em si for zero ou muito próximo de zero — e `lootRb.mass` vem diretamente do peso cadastrado do item (`LootItem.cs:334`, `item_0.TotalWeight`). Itens de documento/quest no Tarkov (chaves, papéis, fotos, alguns itens de barter) plausivelmente têm peso `0` ou muito baixo cadastrado no template. Se isso ocorrer, `physicalImpulse / effectiveMass` produz `Infinity` (divisão por zero em `float` não lança exceção em C#, retorna `Infinity`), e `AddForceAtPosition` recebendo um vetor com componentes `Infinity` corrompe a simulação física daquele `Rigidbody` especificamente (item pode desaparecer do mapa, "explodir" pra uma posição inválida, ou travar a física daquele objeto pro resto da raid).

**Por que importa:** Esse é exatamente o tipo de item (leve, baixo valor, munição/chave/documento) que um jogador atira "só pra testar" ou atinge incidentalmente durante um tiroteio perto de uma mesa de loot — cenário plausível, não hipotético. O comportamento antigo (`ForceMode.Impulse`, divisão feita pela própria Unity) já tinha uma variante desse risco, mas a Unity historicamente clampa a massa mínima de um `Rigidbody` a um valor positivo interno pequeno ao processar `ForceMode.Impulse`; a divisão manual em C# proposta aqui não tem essa mesma rede de segurança.

**Sugestão:** Adicionar um piso mínimo explícito na massa efetiva, não só o teto: `float effectiveMass = Mathf.Clamp(lootRb.mass, MinMassKg, MassCapKg);` com `MinMassKg` pequeno (ex.: `0.05f`) — protege contra o caso de peso zero/quase-zero sem alterar o comportamento pra nenhum item com peso realista (tudo acima de 0.05kg já é maior que esse piso, então o `Clamp` se comporta igual ao `Min` original pra qualquer item plausível). Atualizar o stub de §5 e a explicação de §1/§7 de acordo.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — `MinMassKg = 0.05f` adicionado, `effectiveMass` agora usa `Mathf.Clamp` em vez de `Mathf.Min` em `007-piso-impulso-tiro-itens-02-spec-tech.md` §1, §5, §7 e checklist §8.

---

### PA-01-02 · A — Gap · 🟢 Menor

**Torque/rotação continuam escalando com a massa real (não com a massa "grampeada") — não documentado**

**Problema:** `Rigidbody.AddForceAtPosition` aplica dois efeitos quando o ponto de aplicação não coincide com o centro de massa: a mudança de velocidade linear (o que §1/§5 da spec técnica documentam e corrigem) e um torque proporcional ao deslocamento do ponto de impacto em relação ao centro de massa, convertido em variação de velocidade angular através do tensor de inércia do `Rigidbody` — que depende da massa e distribuição REAL do objeto, não da `effectiveMass` "grampeada" calculada só pra este item. Isso não muda com a mudança de `ForceMode` proposta (`VelocityChange` também respeita o tensor de inércia real pra a componente rotacional; só a componente linear vira independente de massa pelo cálculo manual). A spec técnica (§1, §6, §7) não menciona essa distinção em nenhum momento.

**Por que importa:** Na prática, depois do fix, um item pesado (arma, capacete) vai deslizar/deslocar de forma comparável a um item leve (objetivo do item cumprido), mas vai **girar/tombar menos** que um item leve atingido do mesmo jeito, porque a inércia rotacional real dele continua grande. Isso pode ser lido, na validação em jogo, como "ainda não está reagindo direito" quando na verdade é um resíduo esperado e correto (linear corrigido, rotacional não) — sem essa nota na spec, quem for validar (ou uma futura rodada de `/code-review`) pode interpretar a diferença de rotação como um bug não corrigido.

**Sugestão:** Adicionar uma frase em §7 (Riscos e dependências): "A correção desta spec equaliza só a componente LINEAR da reação (deslocamento) entre itens leves e pesados — a componente rotacional (torque/spin) continua proporcional à massa e ao tensor de inércia reais do item, não ao teto `effectiveMass`. É esperado que um item pesado desloque de forma comparável a um item leve, mas gire/tombe visivelmente menos — isso não é uma regressão, é uma limitação conhecida e aceita do escopo deste item (só o critério de deslocamento perceptível está coberto)." Opcionalmente, adicionar ao checklist de §8 uma validação explícita: "confirmar que o item pesado desloca perceptivelmente mesmo girando menos que um item leve — comportamento esperado, não é bug."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada — nota adicionada em §1 e §7 de `007-piso-impulso-tiro-itens-02-spec-tech.md`, comentário inline no stub §5, e item de validação explícito adicionado ao checklist §8.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Review 01 criada — 0 bloqueadores, 1 importante, 1 menor |
| 2026-09-21 | Aplicação dos 2 achados via decisão do usuário ("Ok pode aplicar") — IDs: PA-01-01, PA-01-02. Spec técnica atualizada. |

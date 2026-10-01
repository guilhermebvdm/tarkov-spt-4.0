# 023 — Sequência de mãos da cura sem confirmação de operação · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md](023-sequencia-maos-cura-sem-confirmacao-02-spec-tech.md)
**Data:** 2026-09-12

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 12) · pendências que afetam este item: nenhuma (P-12.1/P-11.1/P-9.1/P-9.2 são de outros itens).
**Docs técnicos:** só o obrigatório (`spt-antipatterns.md`) — nenhum outro gatilho do roteamento (`docs/technical/README.md`) se aplica a este item (não toca item/inventário/hideout, não declara `INetSerializable`, não cita `GClassNNNN`).

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | Stub acessa `.Error` num tipo cujo membro nunca foi confirmado | ✅ Resolvido em 2026-09-12 |
| PA-01-02 | B — Edge Case | 🟡 Importante | Coroutine nova de `EmergencyDrop` pode perder o item se o `MonoBehaviour` for destruído em voo | ✅ Resolvido em 2026-09-12 |

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

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-12

**Stub acessa `.Error` num tipo cujo membro nunca foi confirmado — pode não compilar**

**Problema:** A própria spec técnica já registra em §7 que `Callback<T>`/`Result<T>`/`Callback` **não estão no `types-index.json`** — são de uma assembly externa (`Comfort.dll`), não decompilada por este repo, e marca isso como `TODO confirmar`. Só que os dois stubs de §5 **não refletem essa incerteza**: escrevem `result.Error` com confiança total —

```csharp
doctor.SetInHands(itemUsed, (result) =>
{
    if (result != null && !string.IsNullOrEmpty(result.Error))
```
```csharp
doctor.TrySetLastEquippedWeapon(true, (result) =>
{
    if (result != null && !string.IsNullOrEmpty(result.Error))
```

Pra `SetInHands`, o parâmetro é `Callback<IHandsController>` — genérico, então o `result` do lambda é `Result<IHandsController>` (o tipo genérico é conhecido pela própria assinatura em `Player.cs:31845`, isso está OK). Mas pra `TrySetLastEquippedWeapon` (`Player.cs:31800`), o parâmetro é `Callback` **não-genérico** — e o corpo do método (`Player.cs:31804-31807`) mostra `callback?.Invoke(result)` sendo chamado com um `Result<IHandsController>`, o que só type-checka se `Callback` (não-genérico) for algo como `delegate void Callback(IResult result)`, e `Result<T>` implementar essa interface `IResult`. **Nenhuma dessas duas coisas foi confirmada** — nem que `IResult` existe com essa forma, nem que ela expõe um membro `.Error`. Se o tipo real não tiver `.Error` na interface base (só no `Result<T>` genérico, por exemplo), o segundo stub não compila como está escrito.

**Por que importa:** a própria regra do `/create-technical-spec` exige que "Stubs devem compilar se copiados num projeto vazio" — e dado o histórico de fragilidade dessa animação que o usuário relatou (spec funcional, restrição de processo), entrar no `/code-mod` com um stub que pode falhar a primeira tentativa de compilação é exatamente o tipo de atrito que a restrição de "mudança incremental e cuidadosa" queria evitar. Não é um erro caro de corrigir, mas é melhor resolver na spec do que descobrir no meio do `/code-mod`.

**Sugestão:** simplificar os dois callbacks pra **não acessar nenhum membro de `result`** — só usar a própria invocação do callback como sinal de "terminou", sem tentar extrair detalhe de erro por enquanto (isso é seguro de compilar não importa qual seja a forma exata de `IResult`/`Result<T>`, porque não toca em nenhum membro específico):

```csharp
// HealRoutine — versão sem acessar membros de `result`:
doctor.SetInHands(itemUsed, (result) => { setInHandsDone = true; });
```

```csharp
// EmergencyDropWeaponReturnAndThrow — idem:
doctor.TrySetLastEquippedWeapon(true, (result) => { weaponReturnDone = true; });
```

Detalhe de sucesso/erro (`result.Error`, se existir) pode ser adicionado **depois**, no `/code-mod`, quando o compilador real disser exatamente quais membros `Result<IHandsController>`/`IResult` expõem — nesse ponto vira um ajuste de 1 linha guiado pelo próprio erro de compilação, não uma suposição. Atualizar §5 (os dois stubs) e §7 (a nota já existente) pra refletir essa simplificação.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicada em `023-...-02-spec-tech.md` §5 — os dois stubs (`HealRoutine` e `EmergencyDrop`) agora só usam a invocação do callback como sinal (`setInHandsConfirmed = true` / log), sem acessar `result.Error` nem nenhum outro membro. §7/§8/§9 atualizados de acordo.

---

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Coroutine nova de `EmergencyDrop` pode perder o item de cura se o `MonoBehaviour` for destruído em voo**

**Problema:** §5 propõe extrair o PASSO 3 (`TrySetLastEquippedWeapon`) + PASSO 4 (`ThrowItem`) de `EmergencyDrop` pra uma nova coroutine (`EmergencyDropWeaponReturnAndThrow`), que fica até `WeaponReturnConfirmTimeoutSeconds` (1.5s) esperando confirmação antes de chamar `ThrowItem`. Isso é uma mudança real de comportamento que a spec não avalia: **hoje** `EmergencyDrop` é síncrono — roda inteiro (limpar mãos + dropar item) no mesmo frame, sem chance de ser interrompido no meio. **Depois** da mudança, existe uma janela de até 1.5s onde a operação está "em voo" numa coroutine. Se o `GameObject` dono de `BandAidController` for destruído nessa janela (fim de raid, extração, desconexão), a Unity mata a coroutine no meio — e o `ThrowItem(savedItem)` **nunca executa**. O item de cura que deveria ter sido salvo/dropado simplesmente some, sem log, sem erro — pior do que o comportamento atual (que sempre completa ou pelo menos loga uma exceção).

**Por que importa:** é uma regressão concreta introduzida pela própria correção — trocar "sempre dropa, mesmo com erro" por "pode simplesmente não dropar, silenciosamente, numa janela de até 1.5s" é pior pro jogador (perde um item de cura) do que o bug que o item tenta resolver. A spec funcional cobre "médico extrai durante a cura" pro fluxo do `HealRoutine`, mas não cobre explicitamente esse caso específico causado pelo desenho técnico (tornar `EmergencyDrop` assíncrono).

**Por que importa (2):** a checklist §9 da própria spec marca o check 5 ("Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos") como ✅ com a justificativa "`_activeHealCoroutine`/nova coroutine já são descartadas no `OnDestroy`/fim de raid do mesmo jeito que hoje" — mas "descartada do mesmo jeito que hoje" é exatamente o problema: hoje não há nada pra descartar (é síncrono), então "do mesmo jeito" não é uma equivalência válida, é uma nova exposição.

**Sugestão:** duas opções, escolher uma antes do `/code-mod`:
1. **Reduzir a superfície:** manter a chamada de `ThrowItem(savedItem)` **síncrona e imediata** (como já é hoje), e usar a coroutine só pra fins de log/diagnóstico da confirmação de `TrySetLastEquippedWeapon` (sem gatear o drop nela). Isso perde um pouco do valor do fix (o drop ainda não espera confirmação), mas elimina o risco de perda de item — e ainda é uma melhoria real (logar falha em vez de ignorar).
2. **Manter o gate, mas proteger a destruição:** só se o usuário preferir manter o comportamento "espera confirmação antes de dropar" — adicionar um fallback que garanta o drop mesmo se a coroutine for interrompida (ex: um hook de encerramento do `BandAidController`/raid que verifica se há um `savedItem` pendente de drop e força o `ThrowItem` síncrono nesse ponto). Mais robusto, mas é mais código pra revisar — pesa contra a restrição de "mudança mínima" da spec funcional.

Recomendo a opção 1 — é estritamente mais simples e já resolve o problema central do item (parar de assumir sucesso sem checar), sem introduzir uma nova classe de bug (perda de item).

**Decisão:**
- `[x]` Aceitar sugestão (opção 1)

**Resolução:** aplicada em `023-...-02-spec-tech.md` §5 — `EmergencyDrop` não vira coroutine. `TrySetLastEquippedWeapon` ganhou um callback só de log; `ThrowItem` continua rodando imediatamente em seguida, síncrono, exatamente como hoje. §6/§7/§8/§9 atualizados de acordo.

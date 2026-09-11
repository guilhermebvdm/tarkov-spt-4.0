# 090 — Velocidade da animação de cura não replica pros outros jogadores (Fika) · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [090-velocidade-cura-nao-replica-02-spec-tech.md](090-velocidade-cura-nao-replica-02-spec-tech.md)
**Data:** 2026-09-08

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

Memória consultada: snapshot 2026-07-15 (Sessão 16, `mods/CustomClasses/memory/sessions.md`) · pendências que afetam este item: nenhuma diretamente. Doc técnico lido (gatilho sempre): `spt-antipatterns.md` — o achado PA-01-01 abaixo é uma instância nova de risco de timing/ordem de carregamento, adjacente ao espírito de AP-01/AP-09 mas não coberto literalmente por nenhum `AP-NN` existente (candidato a promoção futura via `memory-curation`, não bloqueia esta review).

Assembly/mod-evidence reconferida nesta review: `Player.cs:19549-19551` ✅, `ClassMedicPatches.cs:86-91` e `:114-131` ✅, `ClassIdentities.cs:118-124` e `:143-151` ✅ (todas batem exatamente com o citado na spec). `ObservedMedsController.cs:182-183` no fork (`mods/FIKA/modded/`) ✅ idêntico à referência. Item irmão `FIKA/005` conferido: sua review técnica 01 não levantou nenhum bloqueador que impeça este item de prosseguir em paralelo (2 🟡 lá, nenhum 🔴).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A | 🔴 Bloqueador | Sem `[BepInDependency]` para o Fika, `AccessTools.TypeByName` pode nunca resolver mesmo com Fika instalado | ✅ Resolvido 2026-09-09 |
| PA-01-02 | A | 🟡 Importante | `Register()` não tem log de sucesso — impossível confirmar em campo que o hook foi assinado | ✅ Resolvido 2026-09-09 |
| PA-01-03 | B | 🟢 Menor | Checklist não cobre o cenário "FIKA/005 instalado mas em versão sem o hook" (fork desatualizado) | ✅ Resolvido 2026-09-09 |

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

### PA-01-01 · A — Gap · 🔴 Bloqueador · ✅ Resolvido em 2026-09-09

**Sem `[BepInDependency]` declarada para o Fika, a resolução por reflection pode falhar SEMPRE, mesmo com Fika instalado**

**Problema:** O stub de `ClassMedicReplicationHook` (§5) resolve `HookType`/`ExtraSpeedMultiplierField` como campos `static readonly`, avaliados na primeira vez que a classe é tocada — ou seja, no exato instante em que `Register()` é chamado a partir do `Awake()` do Plugin (§5, trecho de `Plugin.cs`). `AccessTools.TypeByName("Fika.Core...")` só encontra o tipo se o assembly `Fika.Core.dll` já estiver **carregado no AppDomain** naquele momento.

Conferido em `mods/CustomClasses/modded/Client/Plugin.cs:13-15`: o mod declara `[BepInDependency("com.SPT.core", "4.0.0")]` e `[BepInDependency("me.sol.sain", SoftDependency)]` — mas **nenhuma** entrada para o Fika (`com.fika.core`, confirmado em `mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs:41`). Sem essa declaração, o BepInEx **não garante** que o assembly do Fika já esteja carregado antes do `Awake()` do CustomClasses rodar — a ordem de carregamento entre dois plugins sem relação de dependência declarada não é definida pelo grafo de dependências do Chainloader (cai em ordem de descoberta de arquivo, não determinística pelo conteúdo).

**Por que importa:** Se o `Awake()` do CustomClasses rodar antes do assembly do Fika estar carregado, `HookType` fica `null` **permanentemente** (é `static readonly`, avaliado uma única vez) — `Register()` vira no-op silencioso pelo resto da sessão, mesmo com Fika instalado e funcionando normalmente em todo o resto do jogo. Isso é exatamente o modo de falha que a AP-06 deste repo alerta ("fix entregue sem validação... nunca foi observado funcionando") — o item 090 pode ser buildado, instalado, e **nunca fazer efeito**, sem nenhum erro no log que aponte a causa. Pior: dependendo de como os arquivos estão organizados no disco do usuário, isso pode ser 100% determinístico (sempre falha, ou sempre funciona) — não é um "às vezes", tornando o comportamento observado em teste do desenvolvedor não-confiável como prova de que funciona em outras máquinas.

**Sugestão:** Adicionar `[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]` no topo de `Plugin.cs` (mesmo padrão já usado pra `me.sol.sain` na linha 15) — isso garante que, SE o Fika estiver presente, seu assembly carrega e seu `Awake()` roda antes do `Awake()` do CustomClasses, tornando a resolução de `HookType` determinística. Atualizar a §4 (Arquivos do mod) e o Checklist (§8) da spec técnica pra incluir essa mudança em `Plugin.cs` explicitamente (hoje só menciona "registrar a chamada num try/catch", não a dependência).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** `[BepInDependency("com.fika.core", SoftDependency)]` adicionado ao stub de atributos de `Plugin.cs` (§5) e à §4/§7/Checklist (§8) da spec técnica.

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-09

**`Register()` não loga sucesso — impossível confirmar em campo que o hook foi assinado**

**Problema:** O stub de `Register()` (§5) só loga em caso de FALHA silenciosa implícita (`return` sem log quando `ExtraSpeedMultiplierField == null`) e o `catch` do bloco em `Plugin.cs` só loga se `Register()` **lançar** — mas `Register()` nunca lança no caminho "Fika ausente" (só retorna cedo). Não existe nenhum `LogInfo` confirmando "hook assinado com sucesso" quando tudo dá certo, nem um aviso explícito quando o Fika está presente mas o TIPO do hook não foi encontrado (cenário do PA-01-03, e também um sintoma indireto do PA-01-01).

**Por que importa:** Combinado com o PA-01-01, isso significa que, se a assinatura falhar por QUALQUER motivo (ordem de carregamento, versão do fork sem o hook, etc.), não há absolutamente nenhum sinal no log do BepInEx pra alguém investigar — o comportamento observável é indistinguível de "o item 090 nunca foi implementado".

**Sugestão:** Em `Register()`, adicionar `Plugin.Log?.LogInfo("[CustomClasses] (090) hook de velocidade (Fika) assinado com sucesso.");` no caminho de sucesso (depois do `SetValue`), e `Plugin.Log?.LogWarning("[CustomClasses] (090) ObservedMedsSpeedHook não encontrado — Fika ausente ou fork sem o hook do item 005 (sem efeito, fail-open).");` no caminho onde `ExtraSpeedMultiplierField == null` (hoje é um `return` mudo). Isso não muda o comportamento fail-open — só o torna DIAGNOSTICÁVEL.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Ambos os logs adicionados em `Register()` (§5) exatamente como sugerido.

### PA-01-03 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-09

**Checklist não cobre "Fika instalado mas em versão sem o hook" explicitamente**

**Problema:** O Checklist (§8) tem "Confirmar que o item FIKA/005 está implementado e buildado" como pré-requisito, mas não tem um passo de validação explícito pro cenário em que o usuário tem um Fika instalado que É este fork, mas de uma versão ANTERIOR ao item 005 (ex.: reinstalou uma build antiga do `Fika.Core.dll` por engano) — um cenário plausível dado que este repo já tem histórico de builds trocadas manualmente (`feedback_server_launcher_sync_builds`, citado na memória do CustomClasses).

**Por que importa:** Esse cenário já É coberto pelo desenho fail-open (`ExtraSpeedMultiplierField == null` → no-op) — não é um bug, mas vale um teste explícito de regressão pra não confundir com o PA-01-01 durante troubleshooting futuro.

**Sugestão:** Adicionar ao Checklist: "Validar com um `Fika.Core.dll` ANTERIOR ao item 005 instalado (sem o hook): o mod não quebra no boot, e o log mostra o aviso de 'hook não encontrado' sugerido no PA-01-02 (não um erro genérico sem contexto)."

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Item adicionado ao Checklist (§8) da spec técnica.

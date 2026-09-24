# 001 — Fix 01 · Checagem de oferta quebrada não cobria lista não-vazia com primeiro item nulo

**Mod:** TRL-ItemsManagement
**Item raiz:** [001-crash-flea-oferta-sem-itens-01-spec.md](001-crash-flea-oferta-sem-itens-01-spec.md)
**Asbuild:** [001-crash-flea-oferta-sem-itens-05-asbuild.md](001-crash-flea-oferta-sem-itens-05-asbuild.md)
**Criado:** 2026-09-23
**Disparado por:** Validação em produção — servidor real (`D:\SPT 4.0`, ~50 mods, flea com 75.433 ofertas ativas) continuou crashando com o mod já instalado e o Harmony confirmadamente aplicado.

## Contexto

Depois do `/code-mod` + `/code-review` (item já 🟢), o usuário instalou o `.dll` num servidor real e o crash de `/client/ragfair/find` **persistiu**, com o stack trace idêntico ao original. Diagnóstico em produção, passo a passo:

1. Confirmado que o `.dll` deployado era exatamente o compilado (tamanho e hash de build batendo).
2. Confirmado via log que o `Harmony.PatchAll` rodou sem erro (`"Harmony patch applied — buy price backstop"`).
3. Adicionado um log de diagnóstico temporário incondicional no `Postfix` — confirmou que o patch **executa** normalmente, duas vezes por busca (`count=75433` nas duas chamadas), exatamente como o fluxo de dados da spec técnica §6 previa.
4. Apesar disso, nenhuma oferta foi filtrada (`hasBroken` nunca virou `true`) e o crash aconteceu do mesmo jeito, imediatamente depois do Postfix rodar.

## Causa raiz

A checagem original ([001-crash-flea-oferta-sem-itens-02-spec-tech.md §5](001-crash-flea-oferta-sem-itens-02-spec-tech.md)) era:

```csharp
public static bool IsBroken(RagfairOffer offer) => offer.Items is not { Count: > 0 };
```

Isso só detecta `Items` nulo ou com `Count == 0`. Mas a linha que quebra na vanilla ([RagfairCategoriesService.cs:66](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/RagfairCategoriesService.cs#L66)) é:

```csharp
.GroupBy(x => x.Items.FirstOrDefault().Template)
```

`Items.FirstOrDefault()` retorna `null` em **dois** cenários, não só um: lista vazia (coberto pela checagem antiga) **ou lista não-vazia cujo primeiro elemento já é `null`** (não coberto). A produção provou esse segundo caso: com 75.433 ofertas passando pela checagem sem nenhuma sinalizada como quebrada, a oferta causadora do crash tinha `Items.Count > 0` mas `Items[0] == null` — algo no mecanismo (ainda não identificado — fora de escopo deste item, ver spec funcional) zera a posição do primeiro item sem encolher a lista, em vez de remover o item de fato.

Esta é uma refutação parcial de uma premissa da spec técnica original (§2, tabela de evidência): a suposição implícita era "a lista fica vazia". Registrando aqui em vez de editar a spec técnica (artefato append-only, `repo-workflow-best-practices` §5) — a spec técnica foi atualizada separadamente para refletir a checagem corrigida, mas este documento é o registro histórico do diagnóstico que levou à correção.

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Server/Ragfair/RagfairEmptyOfferGuardPatch.cs` | `IsBroken` trocado de `offer.Items is not { Count: > 0 }` para `offer.Items?.FirstOrDefault() is null` — espelha exatamente a expressão vanilla que quebra, cobrindo nulo, vazio, e primeiro-elemento-nulo numa lista não-vazia. Adicionado `using System.Linq;`. Log de diagnóstico temporário (usado pra confirmar que o Postfix executava) removido após a causa ser encontrada. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila sem erros (`dotnet build TRLItemsManagement.csproj -c Release -p:SkipDeploy=true` — 0 erros, 0 warnings)
- [ ] **Em produção:** comportamento corrigido observado no servidor real (`D:\SPT 4.0`) — pendente de novo deploy + teste pelo usuário
- [ ] **Fika/multiplayer:** N/A pela spec funcional original (server-side, vale igualmente pra qualquer jogador) — reconfirmar que a busca não falha mais com múltiplos jogadores ativos
- [ ] **Estado entre raids:** N/A — ofertas da flea vivem em memória do processo do servidor, não em escopo de raid (mesma justificativa da spec funcional)
- [ ] **Restart do servidor:** N/A — não há teardown de raid neste item; comportamento do próprio restart (levantado como hipótese pelo usuário) seria uma investigação de causa raiz separada, fora de escopo
- [ ] Memória do mod atualizada (`/update-memory`) com a lição deste fix (checagem de "primeiro elemento nulo" além de "lista vazia" ao filtrar coleções por corrupção externa desconhecida)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-23 | Fix criado — causa raiz identificada e corrigida; aguardando validação em produção pelo usuário |

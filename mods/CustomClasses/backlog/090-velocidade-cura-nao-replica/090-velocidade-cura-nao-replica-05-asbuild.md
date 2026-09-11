# 090 — Velocidade da animação de cura não replica pros outros jogadores (Fika) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [090-velocidade-cura-nao-replica-01-spec.md](090-velocidade-cura-nao-replica-01-spec.md)
**Spec técnica:** [090-velocidade-cura-nao-replica-02-spec-tech.md](090-velocidade-cura-nao-replica-02-spec-tech.md)
**Última review técnica:** [090-velocidade-cura-nao-replica-03-spec-tech-review-01.md](090-velocidade-cura-nao-replica-03-spec-tech-review-01.md)
**Build inicial:** 2026-09-09

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha.

> ⚠️ **Build/instalação não executada por decisão do usuário.** Este `/code-mod` alterou só `mods/CustomClasses/modded/` (o repositório). `/compile-mod` (que geraria o `.dll` e o instalaria em `BepInEx/plugins/`) **não foi rodado** a pedido explícito do usuário ("não crie uma versão na pasta do jogo, só quero na pasta do mod"). O mesmo vale para o item irmão `FIKA/005` — código implementado, `.dll` não compilado/instalado.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Client/Patches/ClassMedicReplicationHook.cs` | Assina `ObservedMedsSpeedHook.ExtraSpeedMultiplier` (FIKA/005) por reflection de tipo; `ResolveFactor` reusa `ClassIdentities.ClassIdOf` (057) + `MedicTiming.IsSurgery`/`FactorFor` (072); fórmula de fallback por reflection (Opção A, não implementada) documentada em comentário. |
| MODIFICADO | `mods/CustomClasses/modded/Client/Plugin.cs` | Adicionado `[BepInDependency("com.fika.core", SoftDependency)]` (PA-01-01); chamada a `ClassMedicReplicationHook.Register()` num `try/catch` perto do bloco do item 072. |

## PA-01-MM resolvidos durante o build

> Já tinham sido aplicados na spec técnica antes deste build (ver Histórico de `02-spec-tech.md`, 2026-09-09) — replicados aqui 1:1 no código.

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🔴 | `[BepInDependency("com.fika.core", SoftDependency)]` adicionado em `Plugin.cs`, garantindo ordem de carregamento determinística com o Fika. |
| PA-01-02 | A — Gap · 🟡 | `Register()` loga sucesso (`LogInfo`) e ausência do hook (`LogWarning`) — diagnosticável em campo. |
| PA-01-03 | B — Edge Case · 🟢 | Cenário "Fika/fork sem o hook" coberto pelo mesmo caminho de log do PA-01-02 (fail-open, sem erro). |

## Mudanças posteriores

### 2026-09-09 — `/apply-code-review` (rodada 01)

| ID | Achado | Ação |
| --- | --- | --- |
| CR-01-01 | `Register()` não avisa ao sobrescrever hook já assinado | ✅ Aplicado — guard + `LogWarning` em `ClassMedicReplicationHook.cs` antes do `SetValue` |

Arquivos tocados nesta rodada: `mods/CustomClasses/modded/Client/Patches/ClassMedicReplicationHook.cs` (modificado).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` (código-fonte apenas; sem `/compile-mod`, por instrução do usuário). |
| 2026-09-09 | Aplicação de 1 achado de code-review 01 via `/apply-code-review` — ID: CR-01-01. |

# 009 — Correção Definitiva de Reconnect no FIKA (Ressincronização de Corpo In-Place) · Code Review 01

**Mod:** FIKA  
**Spec funcional:** [009-reconnect-ressincronizacao-corpo-inplace-01-spec.md](009-reconnect-ressincronizacao-corpo-inplace-01-spec.md)  
**Spec técnica:** [009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md](009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md)  
**Asbuild:** [009-reconnect-ressincronizacao-corpo-inplace-05-asbuild.md](009-reconnect-ressincronizacao-corpo-inplace-05-asbuild.md)  
**Data:** 2026-09-13  

> Análise crítica do código implementado. Cada achado recebe um ID `CR-01-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos/Aplicados: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟢 Menor | Sincronismo de versão SemVer e isolamento de build | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 Médio | Proteção defensiva de nulos em ForceTeleport | ✅ Aplicado |
| CR-01-03 | C — Gap vs. spec | 🟢 Menor | Garantia de canal ReliableOrdered no broadcast | ✅ Aplicado |

---

## Pontos

### CR-01-01 · D — Arquitetura · 🟢 Menor

**Sincronismo de versão SemVer e isolamento de build**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/FikaPlugin.cs:49`](../../modded/Fika-Plugin/Fika.Core/FikaPlugin.cs#L49) e [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Fika.Core.csproj:6`](../../modded/Fika-Plugin/Fika.Core/Fika.Core.csproj#L6)

**Problema:** Regra obrigatória do repositório (`GEMINI.md`) exige incremento SemVer antes de compilação e restrição de compilação sem poluição de diretórios externos (`D:/SPT` ou `.spt-path`).

**Por que importa:** Mantém rastreabilidade de binários e integridade do repositório.

**Sugestão:** Incrementar patch SemVer para `2.3.20` em `FikaVersion` e no `<Version>`, compilando diretamente no diretório do mod.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Aplicado em 2026-09-13. Versão `2.3.20` definida e compilada localmente com sucesso.

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Proteção defensiva de nulos em ForceTeleport**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs:1073-1098`](../../modded/Fika-Plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1073-L1098)

**Problema:** Em servidores Headless ou instâncias inicializadas prematuramente, `MovementContext` ou `_cullingHandler` podem ser nulos no momento do teletransporte.

**Por que importa:** Uma `NullReferenceException` durante o teletransporte abortaria o callback de rede e impediria a re-ancoragem física.

**Sugestão:** Envolver acessos a `MovementContext` e `_cullingHandler` com checagens `if (MovementContext != null)` e `if (_cullingHandler != null)`.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Aplicado em 2026-09-13. Código implementado com verificações defensivas para ambos os membros.

---

### CR-01-03 · C — Gap vs. spec · 🟢 Menor

**Garantia de canal ReliableOrdered no broadcast**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs:196`](../../modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L196) e [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:448`](../../modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L448)

**Problema:** O pacote de re-ancoragem não pode sofrer perda ou chegar desordenado em relação aos pacotes regulares de movimentação do jogador.

**Por que importa:** Se entregue fora de ordem, o jogador poderia ser re-ancorado na posição antiga após já ter começado a se mover.

**Sugestão:** Utilizar estritamente `DeliveryMethod.ReliableOrdered` no envio inicial do cliente e na retransmissão de broadcast do servidor.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Aplicado em 2026-09-13. Ambos os pontos utilizam `DeliveryMethod.ReliableOrdered`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-13 | Code review 01 realizada; todos os 3 pontos validados e aplicados no código. |

# 001 — TarkovIRL General Code Review · Code Review 03

**Mod:** TarkovIRL-SPT4.0-beta
**Data:** 2026-07-17

> Análise crítica do código do mod TarkovIRL. Cada achado recebe um ID `CR-03-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 1 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-03-01 | D — Arquitetura | 🔴 Bloqueador | Dependência Rígida de `shwngFpsCameraStances4.dll` | `[ ]` Pendente |
| CR-03-02 | D — Arquitetura | 🟠 Forte | Vazamento de Memória Estática de Player em `WeaponSelectionController` | `[ ]` Pendente |

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

### CR-03-01 · D — Arquitetura · 🔴 Bloqueador

**Dependência Rígida de `shwngFpsCameraStances4.dll`**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/StanceController.cs:15`](../../modded/StanceController.cs#L15)

**Problema:**
A classe `StanceController` acessa diretamente o tipo `CameraRotationMod.StanceManager` da DLL externa `shwngFpsCameraStances4.dll` no corpo de sua propriedade estática `CurrentStance`:
```csharp
switch (StanceManager.CurrentStance.ToString())
```
Como essa propriedade é avaliada a cada frame em `NewSwayController.cs` e `EfficiencyController.cs`, a ausência física da DLL no diretório `BepInEx/plugins/` causará um crash imediato de `TypeLoadException` ou `FileNotFoundException` ao entrar em raid, quebrando o mod para todos os jogadores que não utilizam o mod de stances.

**Por que importa:**
A integração com o mod de stances deve ser opcional. O mod não deve forçar sua instalação nem crashar na ausência dele.

**Sugestão:**
Implementar uma checagem de presença de DLL/tipo por reflexão (`Type.GetType(...)`) e isolar o acesso direto em um método auxiliar com `[MethodImpl(MethodImplOptions.NoInlining)]`. Se o mod de stances não estiver instalado, a propriedade deve retornar silenciosamente `EStance.None`.

---

### CR-03-02 · D — Arquitetura · 🟠 Forte

**Vazamento de Memória Estática de Player em `WeaponSelectionController`**

**Local:** [`mods/TarkovIRL-SPT4.0-beta/WeaponSelectionController.cs:36`](../../modded/WeaponSelectionController.cs#L36), [`mods/TarkovIRL-SPT4.0-beta/WeaponSelectionController.cs:118`](../../modded/WeaponSelectionController.cs#L118)

**Problema:**
A classe `WeaponSelectionController` armazena a instância do jogador na variável estática `_player`:
```csharp
private static Player _player = (Player) null;
```
Esta variável nunca é definida como `null` ao sair da raid.

**Por que importa:**
Como o objeto `Player` no Tarkov é um GameObject complexo que carrega centenas de componentes da Unity, texturas, meshes e referências de inventário, manter uma referência estática viva após o encerramento da raid causa um vazamento de memória (Memory Leak) massivo a cada raid concluída, reduzindo o desempenho do jogo gradualmente até causar crash por falta de memória (OOM).

**Sugestão:**
Adicionar um método `Reset()` em `WeaponSelectionController` que define `_player = null` e limpa as variáveis de interpolação histórica, registrando a sua chamada no patch `Patch_GameWorldOnDestroy`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 03 criada via `/code-review` |

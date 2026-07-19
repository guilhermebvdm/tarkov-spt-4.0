# 001 — portabilidade-spt-4 · Code Review 04 (Server)

**Mod:** TRL-DynamicSpawn (Server)
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** Não aplicável
**Data:** 2026-07-17T23:06:00-03:00

> Análise crítica do código do servidor implementado. Cada achado recebe um ID `CR-04-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-04-01 | D — Arquitetura | 🟠 Forte | Caminho de arquivo (Hardcoded BaseDirectory) em SpawnPointsManager | Pendente |

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

### CR-04-01 · D — Arquitetura · 🟠 Forte

**Caminho de arquivo (Hardcoded BaseDirectory) em SpawnPointsManager**

**Local:** [`mods/TRL-DynamicSpawn/Server/Helpers/SpawnPointsManager.cs:18`](../../Server/Helpers/SpawnPointsManager.cs#L18)

**Problema:**
O código que define o caminho para carregar as posições personalizadas dos spawns usa um caminho estático combinando o `BaseDirectory` do executável do servidor:
```csharp
string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "mods", "TRL-DynamicSpawn-Server", "config", "Spawns");
```

**Por que importa:**
No SPT 4.0, o diretório onde os mods residem pode ser dinâmico ou estar localizado fora da estrutura clássica do `AppDomain.CurrentDomain.BaseDirectory` caso o executável seja executado a partir de caminhos relativos ou atalhos. Além disso, viola as regras de portabilidade de caminhos e infraestrutura de mods do SPT, que fornecem propriedades utilitárias para resolver caminhos de mods de forma segura e limpa.

**Sugestão:**
Usar a API oficial do SPT 4.0 para resolver caminhos do próprio mod em execução ou ler de forma relativa a partir do local físico da DLL do mod.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 04 (Server) criada via `/code-review` |

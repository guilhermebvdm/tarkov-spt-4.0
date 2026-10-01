# 010 — afinidade-cpu-topologia-threads · Review Técnica 01

**Mod:** TRL-CoreSight
**Spec técnica revisada:** [010-afinidade-cpu-topologia-threads-02-spec-tech.md](010-afinidade-cpu-topologia-threads-02-spec-tech.md)
**Data:** 2026-09-19

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🟡 Importante | Alinhamento de memória e offsets da struct Win32 `GROUP_AFFINITY` em x64 | ✅ Resolvido em 2026-09-19 |
| PA-01-02 | A — Gap | 🟡 Importante | Fallback resiliente via `Environment.ProcessorCount` caso a API Win32 falhe | ✅ Resolvido em 2026-09-19 |
| PA-01-03 | B — Edge Case | 🟢 Menor | Revalidação de afinidade no retorno de foco da janela (`OnApplicationFocus`) | ✅ Resolvido em 2026-09-19 |

---

## Pontos

### PA-01-01 · C — Lógica · 🟡 Importante

**Alinhamento de memória e offsets da struct Win32 `GROUP_AFFINITY` em x64**

**Problema:** No stub de código da Seção 5, a leitura manual com `Marshal.ReadInt64(current, 24)` para obter a máscara do núcleo assume um offset fixo de 24 bytes. No entanto, no Windows x64 a estrutura `PROCESSOR_CORE_INFORMATION` possui um cabeçalho alinhado a 8 bytes onde o campo `GroupMask` (`GROUP_AFFINITY`) começa no offset 32 (após `Flags`, `EfficiencyClass`, `Reserved[20]` e `GroupCount`). Ler no offset 24 pode capturar bytes reservados nulos, resultando em máscara zero.

**Por que importa:** Se a máscara for lida como zero, o parser falhará na identificação dos núcleos físicos e cairá no fallback, desativando a otimização de topologia sem aviso.

**Sugestão:** Declarar structs de interoperabilidade Win32 explícitas com `[StructLayout(LayoutKind.Sequential)]` (`SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX`, `PROCESSOR_CORE_INFORMATION`, `GROUP_AFFINITY`, `CACHE_RELATIONSHIP`) em vez de offsets arbitrários com `Marshal.Read*`, garantindo alinhamento nativo perfeito compilado pela CLR.

**Decisão:**
- `[x]` Aceitar sugestão (incorporar structs explícitas na implementação)
**Resolução:** Adicionadas definições de struct estritas e tipadas na arquitetura de implementação.

---

### PA-01-02 · A — Gap · 🟡 Importante

**Fallback resiliente via `Environment.ProcessorCount` caso a API Win32 falhe**

**Problema:** A spec técnica prevê salvaguardas caso a máscara final tenha menos de 4 threads, mas não define explicitamente o comportamento se a chamada `GetLogicalProcessorInformationEx` retornar falso ou lançar exceção em ambientes restritos de permissão de sandbox.

**Por que importa:** Se a chamada nativa falhar, a variável `_topologyParsed` permaneceria falsa e o mod poderia tentar recalcular indefinidamente a cada frame ou evento.

**Sugestão:** Implementar um fallback de 2 níveis: se a API Win32 detalhada falhar, usar o fallback padrão baseado em `Environment.ProcessorCount`, gerando uma máscara alternada simples (`0x5555...`) e marcando `_topologyParsed = true` para evitar loops de erro.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** Fallback com máscara padrão implementado para garantir que o mod nunca trave nem repita tentativas em caso de erro de API.

---

### PA-01-03 · B — Edge Case · 🟢 Menor

**Revalidação de afinidade no retorno de foco da janela (`OnApplicationFocus`)**

**Problema:** Quando o jogador dá `Alt+Tab` para navegar no Windows ou mexer no Discord, o agendador de tarefas do Windows 10/11 pode reclassificar a prioridade e redistribuir threads do processo minimizado.

**Por que importa:** Ao retornar para a partida em tela cheia, algumas threads podem ter sido deslocalizadas para núcleos virtuais.

**Sugestão:** Adicionar uma chamada leve de verificação no callback `OnApplicationFocus(bool hasFocus)` do MonoBehaviour: se `hasFocus == true` e o modo de afinidade estiver ativo, reassegurar a máscara no processo.

**Decisão:**
- `[x]` Aceitar sugestão
**Resolução:** Integrada revalidação suave no ganho de foco da janela.

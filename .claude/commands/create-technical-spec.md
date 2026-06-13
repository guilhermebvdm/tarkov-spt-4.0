# /create-technical-spec

Cria a **spec técnica** (pré-código) de um item do backlog, usando o Assembly descompilado como fonte primária.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices`, `repo-workflow-best-practices` e `graph-code-navigation` antes de redigir. Toda decisão técnica (lifecycle, leaks, hot paths, patches, threading) deve ser ancorada nelas. Consultar `memory-curation` § "Consumo de memória por commands" (§14) para o passo de contexto de memória.

## Uso

```
/create-technical-spec <ref>
```

`<ref>` segue as regras do `/create-spec` (path da pasta, path de arquivo, ou `<mod> <NNN>`).

## Hierarquia de fontes (obrigatória, nesta ordem)

Fonte de verdade desta ordem: [.agents/resources.md](../../.agents/resources.md) → §"Hierarquia de evidência (spec/review técnicas)". Resumo (toda assinatura/fórmula/constante/ponto de patch citado vem com `arquivo.cs:linha`):

1. **🥇 Assembly descompilado (cliente EFT)** — [references/eft-decompiled/Assembly-CSharp/](../../references/eft-decompiled/Assembly-CSharp/).
2. **🥇 Código-fonte do servidor SPT** — `references/spt-source/` (gitignored — obter via [references/README.md](../../references/README.md)). Verdade para lógica de **servidor**.
3. **🥇 Códigos do FIKA (coop)** — `references/fika-server/`, `references/fika-plugin/` (contém `Fika.Core`), `references/fika-headless/`. Verdade para lógica cooperativa.
4. **🥈 Código do mod** — `mods/<mod>/original/` (upstream intocado, padrões já aplicados) e `mods/<mod>/modded/` (nosso fork; identificar patches existentes).
5. **🥉 Wiki SPT** — [wiki/spt/](../../wiki/spt/) para SPT (instalação, modding, profile, server APIs).
6. **🪛 Web** — só como último recurso. Marcar `[fonte externa]` no texto.

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Pré-condição.** A spec funcional `<NNN>-<slug>-01-spec.md` precisa existir e ter conteúdo real (não placeholders). Se não, avisar que `/create-spec` precisa rodar antes.

3. **Verificar duplicata.** Se `<NNN>-<slug>-02-spec-tech.md` já existe, perguntar se o usuário quer **sobrescrever** ou **abortar**.

4. **Ler contexto:**
   - **Memória do mod** — topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas que citam o item `<NNN>`. Aplicar `memory-curation` § "Consumo de memória por commands" (§14): reportar pendências que afetam esta tarefa; pendência 🔴 do item/mod → alertar antes de prosseguir. Se o arquivo não existir, registrar "sem memória prévia".
   - A spec funcional inteira (critérios de aceite e corner cases pautam a busca).
   - `mods/<mod>/modded/Plugin.cs` e `mods/<mod>/modded/Patches/` — entender padrões já usados pelo mod.
   - `mods/<mod>/PROPRIEDADES.md` se existir.

5. **Pesquisar no Assembly.** Para cada comportamento da spec funcional:
   - **Grafo primeiro** (skill `graph-code-navigation`): `query_graph`/`get_neighbors` para localizar classes, callers/callees e **TODOS os overrides de alvos virtuais (AP-03)** antes do Grep manual; `shortest_path` para a cadeia input→efeito do fluxo de dados (§6). O grafo aponta, a leitura prova.
   - Identificar a classe e método mais provável (Grep por palavras-chave quando a busca for textual: strings, configs, logs).
   - Ler o trecho relevante e capturar `arquivo:linha` exatos.
   - Anotar fórmula, constantes, dependências.
   - Investigar callers e callees até ter um fluxo de dados completo.

6. **Renderizar `.agents/templates/technical-spec.md.tmpl`** preenchendo:
   - `{{NUM}}`, `{{TITLE}}`, `{{SLUG}}`, `{{MOD}}`, `{{CREATED_AT}}`.

7. **Preencher cada seção com conteúdo real:**

   1. **Estratégia** — tipo de patch (Prefix/Postfix/Transpiler/Replace), classe.método alvo, justificativa em 1 parágrafo.
   2. **Pontos de patch** — tabela com link clicável `[arquivo.cs:L###](../../../../references/eft-decompiled/Assembly-CSharp/arquivo.cs#L###)`.
   3. **Novas propriedades F12** — só se aplicável; usar mesmo formato de `PROPRIEDADES.md`.
   4. **Arquivos do mod** — tabela com `MODIFICAR` / `CRIAR` + resumo.
   5. **Stubs de código** — blocos C# **compiláveis** (assinatura completa, namespaces, atributos Harmony, corpo mínimo plausível). Cada referência ao código do EFT comentada com `// ref: Assembly-CSharp/<arquivo>:<linha>`.
   6. **Fluxo de dados** — diagrama A→B→C com linhas de ref do Assembly e do mod.
   7. **Riscos e dependências** — patches existentes em `modded/`, mods externos relacionados, ordem de inicialização.
   8. **Checklist de implementação** — tarefas atômicas em ordem (cada uma rodável e verificável).
   9. **Conformidade com skills (auto-checklist)** — preencher a tabela ANTES de salvar. Cada check: ✅ com evidência (seção da spec ou `arquivo:linha`) ou N/A + razão. **Check sem evidência não vale ✅.** Qualquer ❌ → resolver na própria spec antes de prosseguir. Taxonomia de referência: `docs/technical/spt-antipatterns.md`.

8. **Salvar** como `<path-pasta>/<NNN>-<slug>-02-spec-tech.md`.

9. **Confirmar:**
   ```
   ✓ Spec técnica criada: <path>
   Refs ao Assembly: N (verificadas)
   Stubs C# compiláveis: N
   Memória consultada: snapshot de YYYY-MM-DD (Sessão N) · pendências que afetam: [P-N.M ...] / nenhuma
   Conformidade: 8/8 checks ✅ ou N/A justificado
   Próximo: rode /review-technical-spec <ref> para análise crítica
   ```

## Regras

- **Toda referência ao EFT precisa vir do Assembly local** com linha. Sem isso, a spec não é técnica — é palpite.
- **Stubs devem compilar** se copiados num projeto vazio com referência a Harmony e SPT.Reflection. Não escrever pseudocódigo.
- Não inventar nomes de classe ou método. Se não achar, registrar **TODO confirmar:** explicitamente.
- Tooltip de `ConfigEntry` deve ser **traduzido para pt-BR**, mantendo coluna `Tooltip (pt-BR)` consistente com `PROPRIEDADES.md`.
- Versão alvo: SPT 4.0+ / EFT 0.16.x — nunca sugerir padrões do SPT 3.x.

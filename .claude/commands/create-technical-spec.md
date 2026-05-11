# /create-technical-spec

Cria a **spec técnica** (pré-código) de um item do backlog, usando o Assembly descompilado como fonte primária.

> **Skills obrigatórias:** carregar `spt-mod-best-practices`, `csharp-mod-best-practices` e `repo-workflow-best-practices` antes de redigir. Toda decisão técnica (lifecycle, leaks, hot paths, patches, threading) deve ser ancorada nelas.

## Uso

```
/create-technical-spec <ref>
```

`<ref>` segue as regras do `/create-spec` (path da pasta, path de arquivo, ou `<mod> <NNN>`).

## Hierarquia de fontes (obrigatória, nesta ordem)

1. **🥇 Assembly descompilado** — [references/eft-decompiled/Assembly-CSharp/](../../references/eft-decompiled/Assembly-CSharp/). Toda assinatura, fórmula, constante, ponto de patch citado **deve** vir daqui com `arquivo.cs:linha`.
2. **🥈 Código do mod** — `mods/<mod>/original/` (upstream intocado, mostra padrões já aplicados) e `mods/<mod>/modded/` (nosso fork; identificar patches existentes para evitar conflito).
3. **🥉 Wiki SPT** — [wiki/spt/](../../wiki/spt/) para questões de SPT (instalação, modding, profile, server APIs).
4. **🪛 Web** — só como último recurso. Marcar `[fonte externa]` no texto.

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Pré-condição.** A spec funcional `<NNN>-<slug>-01-spec.md` precisa existir e ter conteúdo real (não placeholders). Se não, avisar que `/create-spec` precisa rodar antes.

3. **Verificar duplicata.** Se `<NNN>-<slug>-02-spec-tech.md` já existe, perguntar se o usuário quer **sobrescrever** ou **abortar**.

4. **Ler contexto:**
   - A spec funcional inteira (critérios de aceite e corner cases pautam a busca).
   - `mods/<mod>/modded/Plugin.cs` e `mods/<mod>/modded/Patches/` — entender padrões já usados pelo mod.
   - `mods/<mod>/PROPRIEDADES.md` se existir.

5. **Pesquisar no Assembly.** Para cada comportamento da spec funcional:
   - Identificar a classe e método mais provável (Grep por palavras-chave).
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

8. **Salvar** como `<path-pasta>/<NNN>-<slug>-02-spec-tech.md`.

9. **Confirmar:**
   ```
   ✓ Spec técnica criada: <path>
   Refs ao Assembly: N (verificadas)
   Stubs C# compiláveis: N
   Próximo: rode /review-technical-spec <ref> para análise crítica
   ```

## Regras

- **Toda referência ao EFT precisa vir do Assembly local** com linha. Sem isso, a spec não é técnica — é palpite.
- **Stubs devem compilar** se copiados num projeto vazio com referência a Harmony e SPT.Reflection. Não escrever pseudocódigo.
- Não inventar nomes de classe ou método. Se não achar, registrar **TODO confirmar:** explicitamente.
- Tooltip de `ConfigEntry` deve ser **traduzido para pt-BR**, mantendo coluna `Tooltip (pt-BR)` consistente com `PROPRIEDADES.md`.
- Versão alvo: SPT 4.0+ / EFT 0.16.x — nunca sugerir padrões do SPT 3.x.

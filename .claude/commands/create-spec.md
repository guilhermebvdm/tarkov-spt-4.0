# /create-spec

Cria a **spec funcional** de um item do backlog (sem código — foca em intenção, critérios e corner cases).

## Uso

```
/create-spec <ref>
```

`<ref>` aceita uma das três formas:

- **Path da pasta:** `mods/<mod>/backlog/NNN-<slug>/`
- **Path de arquivo dentro da pasta:** `mods/<mod>/backlog/NNN-<slug>/qualquer-arquivo.md`
- **Forma curta:** `<mod> <NNN>` ou `<mod> <slug>`

## Resolução do `<ref>`

1. Se for path: extrair `<mod>` (2º segmento) e `<NNN>-<slug>` (5º segmento). Subir pra pasta se for arquivo.
2. Se for forma curta: procurar pasta em `mods/<mod>/backlog/` que case `^<NNN>-` ou contenha `<slug>`.
3. Se não encontrar: listar pastas existentes no backlog do mod e parar.

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Verificar pré-condição.** Se `<path-pasta>/<NNN>-<slug>-01-spec.md` já existe: avisar e perguntar se o usuário quer **sobrescrever** ou **abortar**. Não modificar sem permissão.

3. **Buscar contexto.** Ler:
   - `mods/<mod>/backlog/mod-backlog.md` — pegar resumo e título do item.
   - `mods/<mod>/README.md` (se existir) — entender o mod.
   - `mods/<mod>/PROPRIEDADES.md` (se existir) — propriedades atuais do F12.

4. **Renderizar `.agents/templates/spec.md.tmpl`** preenchendo:
   - `{{NUM}}` = `NNN`
   - `{{TITLE}}` = título do item (do `mod-backlog.md`)
   - `{{MOD}}` = nome do mod
   - `{{CREATED_AT}}` = data ISO do dia (`YYYY-MM-DD`)
   - `{{DESCRIPTION_SUMMARY}}` = parágrafo de 2–3 frases ampliando o resumo

5. **Preencher seções** com conteúdo real, não placeholders genéricos:
   - **Comportamento atual:** baseado no que foi observado em `original/` ou no que o usuário descreveu.
   - **Comportamento desejado:** o que muda após implementar.
   - **Critérios de aceite:** 3–6 itens **verificáveis** (cada um deve ser testável manualmente in-game ou via assert no código).
   - **Corner cases:** pelo menos 3, pensando em estados-limite (nulo/vazio, race condition, interação com outros sistemas do EFT, troca rápida de estado, sair de raid).
   - **Fora de escopo:** deixar `[ ] A definir` se não for óbvio. **Nunca inferir.**

6. **Salvar** como `mods/<mod>/backlog/<NNN>-<slug>/<NNN>-<slug>-01-spec.md`.

7. **Confirmar:**
   ```
   ✓ Spec funcional criada: mods/<mod>/backlog/<NNN>-<slug>/<NNN>-<slug>-01-spec.md
   Próximo: revise o conteúdo e rode /review-spec <ref>
   ```

## Regras

- Spec funcional **não contém código nem nomes de classe do EFT**. Isso é problema do `/create-technical-spec`.
- Critérios de aceite no infinitivo, mensuráveis (ex: "Drain de stamina em postura 1 cessa quando o jogador troca para Padrão" — não "stamina deve funcionar bem").
- Se faltar info essencial pra escrever o comportamento desejado, perguntar antes em vez de chutar.

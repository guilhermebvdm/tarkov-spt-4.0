# /code-mod

Implementa um item do backlog em `mods/<mod>/modded/`, seguindo a spec técnica e o checklist.

> **Skills obrigatórias:** carregar `spt-mod-best-practices` e `csharp-mod-best-practices` antes de codar. Validar cada arquivo escrito contra os checklists ao fim de cada skill antes de marcar `[x]`.

## Uso

```
/code-mod <ref>
```

`<ref>` segue as regras do `/create-spec`.

## Pré-condições

1. Existir `<NNN>-<slug>-spec.md` e `<NNN>-<slug>-technical-spec.md` com conteúdo real.
2. Ter pelo menos uma review (`<NNN>-<slug>-technical-review-NN.md`).
3. **Última review sem bloqueadores 🔴 pendentes.** Se houver, **bloquear** e pedir ao usuário pra resolvê-los antes (ou marcá-los `[x] Resolvido` se já tratados na spec).

## O que fazer

1. **Resolver `<ref>`** → `<mod>`, `<NNN>`, `<slug>`, `<path-pasta>`.

2. **Validar pré-condições.** Se faltar arquivo ou houver `🔴 [ ]` na última review, parar com mensagem clara.

3. **Ler:**
   - Spec técnica completa.
   - Última review (e quaisquer pontos `🟡`/`🟢` ainda pendentes — registrar para resolver durante).
   - Os arquivos do Assembly citados na spec, **conferindo `arquivo:linha`** antes de escrever código que dependa deles.
   - Estado atual de `mods/<mod>/modded/` para entender onde encaixar.

4. **Implementar seguindo o checklist da spec técnica**, em ordem:
   - **Apenas em `mods/<mod>/modded/`.** Nunca tocar em `original/`.
   - Para cada arquivo `CRIAR`/`MODIFICAR`, partir do stub da spec técnica e completar a lógica.
   - Comentar referências ao Assembly inline com `// ref: Assembly-CSharp/<arquivo>:<linha>` quando fizer sentido para manutenção.
   - Reusar utilities/patterns já presentes em `modded/` antes de inventar novos.

5. **Após cada arquivo** mudado, marcar a tarefa correspondente do checklist na spec técnica como `[x]`.

6. **Atualizar [PROPRIEDADES.md](../../mods/<mod>/PROPRIEDADES.md)** se novas `ConfigEntry` foram adicionadas (mesma tabela, mesma ordem).

7. **Atualizar status no `mod-backlog.md`** do mod:
   - Em progresso parcial → 🟡
   - Entregue → 🟢
   - (legenda completa: ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado)

8. **Reportar:**
   ```
   ✓ Build concluído — <NNN> <Título>
   Arquivos alterados:
     - mods/<mod>/modded/Plugin.cs (modificado)
     - mods/<mod>/modded/Patches/<X>.cs (criado)
   Pontos pendentes da review (não-bloqueadores):
     - PA-01-04 (🟡): [resumo]
   Próximo:
     - Build do .dll: ver mods/<mod>/scripts/ ou README do mod
     - /review-tech opcional para revisão pós-implementação
   ```

## Regras

- **Sandbox = `modded/`.** Nunca alterar `original/`. Se precisar de algo do upstream, copiar para `modded/`.
- **Não inventar APIs.** Se a spec técnica omite algo necessário, parar e pedir `/create-technical-spec` ou `/review-technical-spec` adicional.
- Não criar arquivos fora do escopo declarado na spec técnica. Se aparecer necessidade nova, **registrar como ponto pendente** numa nova `/review-technical-spec` em vez de improvisar.
- Versão alvo: SPT 4.0+ / EFT 0.16.x — código deve compilar contra os assemblies do jogo nessa versão.
- Compilação efetiva (gerar .dll) **não** está no escopo deste comando; é responsabilidade dos scripts/build do mod.

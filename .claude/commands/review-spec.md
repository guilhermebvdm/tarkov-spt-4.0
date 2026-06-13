# /review-spec

Revisão crítica e **edição inline** da spec funcional. Não cria arquivo novo — corrige o existente.

> **Skills obrigatórias:** carregar `spt-mod-best-practices` e `csharp-mod-best-practices` para identificar lacunas técnicas (lifecycle de raid, leaks, patches, threading) que a spec funcional precise mencionar como restrições/critérios.

## Uso

```
/review-spec <ref>
```

`<ref>` segue as mesmas regras do `/create-spec` (path da pasta, path de arquivo, ou `<mod> <NNN>`).

## O que fazer

1. **Resolver `<ref>`** → `<path-pasta>` e localizar `<NNN>-<slug>-01-spec.md`. Se não existir, parar.

2. **Ler todo o conteúdo** da spec. Ler também o topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências, `memory-curation` §14) — lições registradas (ex.: bugs Fika, estado entre raids) que a spec ignora são gaps a corrigir.

3. **Analisar criticamente** procurando:
   - **Gaps** — informação ausente que ambigua o que precisa ser feito.
   - **Erros lógicos** — pressupostos que não se sustentam ou contradizem o comportamento atual.
   - **Contradições internas** — comportamento desejado bate com critérios de aceite? Corner case bate com fora de escopo?
   - **Critérios vagos / não-verificáveis** — "deve funcionar bem", "ser intuitivo" → pedir versão mensurável.
   - **Corner cases óbvios faltando** — usar checklist mental: estado nulo/vazio, race condition, troca rápida de modo, fim de raid, interação com mod do mesmo escopo.
   - **Critérios padrão ausentes ou N/A-ados indevidamente** — os critérios **Fika/multiplayer** e **estado entre raids** precisam existir (preenchidos com comportamento verificável ou `N/A: <razão>`). Ausência = gap. **`N/A` não é aceito de graça:** se o "Comportamento desejado" descreve algo que reage a ação de player (tiro, recarga, postura, movimento) ou que mantém estado estático/raid-scoped, um `N/A` no critério Fika ou estado-entre-raids é ele mesmo um gap 🟡 — exigir justificativa concreta de por que a feature é imune (AP-02/AP-01). Marcar com `<!-- review: N/A frágil em <critério> — justificar -->`.

4. **Aplicar correções inline** no arquivo:
   - Reescrever critérios vagos.
   - Adicionar corner cases que faltam.
   - Resolver contradições preferindo o lado mais específico.
   - Marcar com `<!-- review: ... -->` qualquer trecho que precise de decisão humana antes de prosseguir.

5. **Adicionar entrada no Histórico:**
   ```markdown
   | YYYY-MM-DD | Revisão `/review-spec` — N gaps + M corner cases corrigidos |
   ```

6. **Reportar ao usuário** um diff resumido:
   ```
   ✓ Spec revisada: <path>
   Mudanças:
     - Critério "X" reescrito para verificabilidade
     - Adicionados N corner cases (lista)
     - Y trechos marcados com <!-- review: --> (precisam de decisão sua)
   ```

## Regras

- **Editar inline** preservando o que estava correto. Não recriar do zero.
- Se a spec estiver vazia ou só com placeholders: avisar que `/create-spec` precisa rodar antes.
- Não inferir conteúdo técnico (assinatura de método, classe do EFT) — isso é trabalho do `/create-technical-spec`.
- Quando precisar de decisão humana, **marcar com `<!-- review: ... -->`** e listar no resumo. Não inventar respostas.

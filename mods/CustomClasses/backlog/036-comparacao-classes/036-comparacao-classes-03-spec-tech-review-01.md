# 036 — Auto-review da spec técnica (review-01)

**Mod:** CustomClasses
**Status:** Concluído — 🔴 resolvidos no 02
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./036-comparacao-classes-02-spec-tech.md) · [01-spec](./036-comparacao-classes-01-spec.md)

Auto-review adversarial da spec técnica do 036. Achados graves (🔴) resolvidos diretamente no 02 antes de fechar; 🟡 são riscos aceitos/registrados; 🟢 confirmações. Convenção de severidade e referência de FORMATO: `repo-workflow-best-practices` / `csharp-mod-best-practices`.

## 🔴 Resolvidos (alterados no 02)

### 🔴-R1 — Inversão de sinal do delta de skill (semântica de cor A vs componente B−A)

**Achado.** A spec funcional (01) descreve "▲ verde quando **A>B**". Mas o `DeltaCell` JÁ existente no `SkillCanonicalList` (031, `:395-410`) calcula **delta = B−A** e pinta **B>A** de verde. Como PA-036-03 proíbe reescrever o componente, passar `Compare` cru produziria a cor **invertida** em relação à narrativa da 01 — bug de leitura silencioso (usuário lê "verde" como "A vantagem" quando é o contrário).

**Resolução (02 §"Nota de semântica de cor" + §4).** v1 **adota a convenção do componente** (delta da coluna de skill = B−A; verde = B tem mais) e **rotula explicitamente** a coluna/legenda como "Δ B−A" para remover ambiguidade — não inverte o componente. **Em contrapartida**, os **deltas de resumo do header** (`DeltaChip`) são calculados do ponto de vista de **A** (A − B), com ▲/▼ e cor coerentes com "A maior". Dois pontos da tela usam polaridades opostas (coluna de skill: B−A; badges: A−B) — por isso **ambos ganham rótulo explícito** ("vs B" nos badges, "Δ B−A" na coluna). Documentado como decisão consciente, não acidente. Inverter o componente seria reescrever o contrato 031→036 (fora da v1).

### 🔴-R2 — "Multiplicadores lado a lado" não tem suporte no componente

**Achado.** A 01 e o kickoff pedem chips ±% das duas classes **lado a lado na mesma linha**. O `SkillCanonicalList` (031) renderiza o ±% **só de A** (`MultiplierChip :374-393`); não há parâmetro/coluna para o multiplicador de B. Implementar "lado a lado" de verdade **exige editar o componente** — colide com PA-036-03 (não reescrever) e com a fronteira do 031.

**Resolução (02 §"Multiplicadores lado a lado").** v1 = opção (A): **não** mostrar multiplicador de B na linha; a comparação de skills é entregue pela coluna de **delta de nível** (foco central do kickoff) + deltas de resumo. "Lado a lado" fica **parcialmente** satisfeito e registrado como **limitação v1 + follow-up** (opção B: parâmetro aditivo `CompareMultipliers` + célula condicional, sem reescrita, mas mexendo no componente do 031 — fora desta wave). Premissa autônoma, sem aprovação disponível. Evita reabrir o contrato 031 numa wave paralela ao 034.

### 🔴-R3 — Fonte de cor do delta duplicada (CSS vs MudChip)

**Achado.** Risco de o CSS do 036 redefinir cores de delta (verde/vermelho) e divergir do `MudChip Color="Success/Error"` que o `DeltaCell` (031) já usa — duas fontes de verdade para a mesma cor, igual ao anti-padrão que o 033 evitou com cores de categoria.

**Resolução (02 §CSS).** O apêndice CSS do 036 **não** define cor de ▲/▼: a cor continua vindo do `MudChip Color` (componente 031 e `DeltaChip` do header). O CSS só adiciona layout (badge duplo `.cc-cmp-b`, 2 colunas `.cc-cmp-2col`). Bloco estritamente **aditivo** ao fim do arquivo.

## 🟡 Riscos aceitos / registrados

### 🟡-Y1 — `MudMenu` ainda não usado no projeto

`grep` confirma que `MudMenu`/`MudMenuItem` **não** aparecem em nenhum `.razor` do mod hoje (só `MudButton`/`MudIconButton`/`MudSelect`). É componente padrão MudBlazor (`@using MudBlazor` global em `_imports.razor`), então compila — mas é o **primeiro** uso. Mitigação: o code-mod valida o render no jogo (lição de memória: escrita SPT exige validação no jogo, não só compile). Alternativa de baixo risco se `MudMenu` der atrito de estilo: `MudSelect<string>` (já usado em `SkillCanonicalList :68`) com os candidatos — mas perde ícone/cor por item. Aceito: MudMenu é a escolha; fallback documentado.

### 🟡-Y2 — Polaridade `higherIsA: true` para skill cost / ₽ é arbitrária

`DeltaChip` pinta "A maior = verde" para as três métricas, inclusive skill cost ponderado e loadout ₽, onde "maior" não é claramente bom (custo pode estourar budget). Decisão v1 (02 §"higherIsA"): manter neutro/consistente — é leitura comparativa, não veredito. Risco: usuário pode ler verde como "melhor". Mitigado pelo `SkillTotalChip` (budget) que continua ao lado. Reavaliar no 035.

### 🟡-Y3 — B perde-se ao navegar A pelo sidebar (PA-036-01)

Limitação de produto já registrada na 01 (PA-036-01) e nos corner cases (#3). Não é defeito técnico — é a fronteira com o 030 (NavMenu). O DoD essencial ("2 cliques a partir do detail") é cumprido. Follow-up: 035 ou 030 propaga `?compare=` nos links de classe.

### 🟡-Y4 — Nomes de roupa de B sem catálogo dedicado

No bloco de outfit 2 colunas, `ClothingLabel` resolve nomes só para A (`_clothingNames` populado p/ A em `Reload :310-325`). Os ids de roupa de B caem no fallback "id cru". Aceito: comparação textual simples de outfit é fora de escopo aprofundar (01 §Fora de escopo); popular catálogo de B é custo sem valor proporcional. Registrado.

## 🟢 Confirmações

- **🟢 net9.0** confirmado (`CustomClasses.Server.csproj:4`) ⇒ `[SupplyParameterFromQuery]` e `NavigationManager.GetUriWithQueryParameter` disponíveis (ambos `Microsoft.AspNetCore.Components`, sem `WebUtilities`).
- **🟢 `Compare` + delta + transbordo de B** já implementados no `SkillCanonicalList` (031 `:104-106,144-156,179-188,366-410,414`); a spec só **passa** o parâmetro — zero reescrita, coerente com PA-036-03.
- **🟢 `ListClassFiles()` cacheado por file-stamp** (`:80-90`) ⇒ a 2ª chamada em `ResolveCompare` é hot; otimização de reúso da lista é trivial e não-bloqueante.
- **🟢 `Compute*` de B fora do render** (em `ResolveCompare`, 1× por resolução) ⇒ sem custo por-render; mesma ordem de grandeza de uma classe A.
- **🟢 Coluna direita do 034 intacta** — território disjunto confirmado; 036 só toca header + coluna esquerda + hideout/outfit (PA-036-04).
- **🟢 CSS aditivo** — apêndice ao fim, sem reescrever 033/034.
- **🟢 Read-only/efêmero** — nenhum caminho chama `Save`/`Delete`; B vive só na URL.

## Veredito

Sem 🔴 abertos. Os três achados graves foram resolvidos no 02 por **decisão de design registrada** (adotar convenção do componente + rotular; multiplicadores de B como follow-up; cor de delta só via MudChip). 🟡 são limitações de produto/risco conscientes, todas com mitigação ou follow-up nomeado (035/030 ou item de multiplicadores). Pronto para `/code-mod`.

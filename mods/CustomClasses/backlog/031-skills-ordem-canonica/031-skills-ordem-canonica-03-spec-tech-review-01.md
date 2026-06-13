# 031 — Skills em ordem canônica — Auto-review da spec técnica (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./031-skills-ordem-canonica-02-spec-tech.md) · [01-spec](./031-skills-ordem-canonica-01-spec.md)

Revisão crítica do 02. 🔴 = bloqueador (resolvido editando o 02). 🟡 = risco aceito/anotado. 🟢 = confirmação.

## 🔴 Bloqueadores encontrados e resolvidos

### 🔴 B1 — Multipliers.ToDictionary lança em chave duplicada
`_model.Multipliers.ToDictionary(m => m.Skill, m => m.Factor)` em `ClassEdit.razor` lança `ArgumentException` se houver dois rows com o mesmo `Skill`. A UI hoje bloqueia duplicatas no add (`AvailableSkills`), mas um arquivo editado à mão pode ter dois fatores para a mesma skill, e `FromDefinition` materializa todos. Um crash de circuito ao abrir a aba Skills é regressão grave.
**Resolução:** trocado por agregação tolerante a duplicata (primeira ocorrência vence), espelhando `ClassEditModel.ToDict` (`ClassEditModel.cs:297-311`). Atualizado no 02.

### 🔴 B2 — Comparação de chaves A×B / Levels é case-sensitive e some no dict
`Compare.Skills[name]` e `def.Skills[name]` usam as chaves cruas do arquivo. As chaves do arquivo podem divergir de `SkillTypes.ToString()` em casing (o schema aceita `ignoreCase` — `CostService.cs:120`). Indexar por `entry.Name` exato perderia o nível de uma skill grafada `endurance`. Também: indexação direta `dict[name]` lança `KeyNotFoundException` se ausente.
**Resolução:** o 02 passa a especificar lookup **case-insensitive e seguro** (montar um `Dictionary<string,int>(StringComparer.OrdinalIgnoreCase)` a partir de `Levels`/`Compare.Skills` uma vez por render, com `TryGetValue`). Documentado.

### 🔴 B3 — Remoção do "Add skill" elimina o único caminho para skills fora da canônica
Hoje `AvailableSkills` oferece o enum `SkillTypes` inteiro (`ClassEdit.razor:762`), incluindo skills "mortas" (SMG, Sniping, …) que NÃO estão em `SkillMaster.Entries` (que deriva só de `Explicit` + Special Elite). Removendo o dropdown, o autor perde a capacidade de adicionar/editar essas skills pela UI de edição — e a seção de transbordo do 02 era descrita só para **read-only** ("loader ignores"). Regressão funcional silenciosa.
**Resolução:** o 02 passa a exigir que a seção de transbordo seja **editável quando `Editable=true`** (campo de nível inline também), e que continue existindo um affordance mínimo de "adicionar skill fora da canônica" (dropdown reduzido só com as skills do enum que não estão na master). Sem isso, edição perde cobertura vs. hoje. Documentado como sub-seção nova no 02.

### 🔴 B4 — Criar SkillLevelRow ao digitar exige StateHasChanged + ordem estável
A regra "digitar > 0 numa skill sem row → cria row" muta `EditRows` dentro do componente filho. Sem re-render coordenado, a linha recém-criada e o `MudNumericField` podem ficar dessincronizados do binding (o campo ainda aponta para o "nível virtual 0", não para o row). E a posição da linha é canônica (não depende da ordem de `EditRows`), então a criação não pode reordenar a UI.
**Resolução:** o 02 passa a especificar que o `MudNumericField` liga a uma **propriedade computada por linha** (get = nível resolvido; set = cria/atualiza/—) em vez de ligar direto a `SkillLevelRow.Level`, de modo que "sem row ainda" é um estado válido do getter (retorna 0) e o setter materializa o row sob demanda e chama `OnLevelChanged`. A posição vem sempre de `SkillMaster.Entries`, nunca de `EditRows`. Documentado.

## 🟡 Riscos aceitos / anotados

- 🟡 R1 — Cor de acento da categoria Combat: o 02 usa "accent do tema, fallback #c8a35a". Se o tema MudBlazor não expuser a cor trivialmente em CSS inline, usar o literal `#c8a35a` direto (cosmético, não bloqueia). Special Elite `#9a6ec8` é escolha estética livre.
- 🟡 R2 — `Cost` (breakdown) só tem entradas para skills definidas; skills em nível 0 não têm linha de custo — o componente exibe custo só quando `level>0` e a entrada existe, senão omite. Coerente.
- 🟡 R3 — Performance: montar 2 dicts case-insensitive + lookup por linha (~35 linhas) por render é trivial; sem cache necessário neste item (037 cuida de hot paths de loadout, não disto).
- 🟡 R4 — `SkillMaster.Entries` em static ctor: se `SkillWeights`/`SkillsExtendedCompat` mudarem em runtime (não mudam — são `static readonly`), a lista ficaria velha. Aceito (são constantes de compilação efetiva).

## 🟢 Confirmações

- 🟢 C1 — Round-trip de zeros (P7): confirmado por leitura de `ClassEditModel.cs:205-208` (FromDefinition materializa todo par, inclusive 0) e `:237/:297-311` (ToDefinition reemite todos os rows). Não precisa tocar o model.
- 🟢 C2 — Sem números mágicos: `SkillMaster.Entries` é derivado; contagem = `Entries.Count`. A spec 01 não cita nenhum número de contagem.
- 🟢 C3 — Território respeitado: só os 4 arquivos do item; `SkillWeights`/`CostService`/`ClassEditModel` apenas consumidos.
- 🟢 C4 — Contrato 031→033/036: os 6 parâmetros cobrem os 3 modos; 033 e 036 não precisam de params novos.

## Bloqueadores abertos

**0.** Os 4 🔴 (B1–B4) foram resolvidos editando o 02.
</content>

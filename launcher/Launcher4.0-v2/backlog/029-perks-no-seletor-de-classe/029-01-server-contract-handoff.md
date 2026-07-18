# 029-01 — Contrato de servidor: expor perks/drawbacks das classes ao launcher

> **Para:** sessão paralela do mod **CustomClasses** (server C#)<br>
> **De:** sessão do **launcher TRL** (item 029 — perks no seletor de classe)<br>
> **Tipo:** spec de contrato / handoff (o launcher CONSOME; o CustomClasses PRODUZ)<br>
> **Status:** proposta a implementar no server do CustomClasses<br>

---

## 0. TL;DR

O launcher TRL quer mostrar, na tela de **seleção de classe (pré-registro)**, os perks e drawbacks
de cada classe — a mesma informação que hoje aparece só **dentro do jogo** (aba CLASS / notificação de
raid), gerada pelo catálogo hardcoded no **client** (`PerksCatalog`).

Precisamos que o **server** do CustomClasses passe a expor esses efeitos por classe, em formato
**já formatado** (o launcher não tem acesso ao catálogo, nem ao EFT, nem ao F12 do jogador).

**Recomendação:** estender a rota existente `GET /customclasses/classes`, adicionando um array
**opcional** `effects` em cada `ClassListItem`. É backward-compatible (o launcher 2.x já ignora campos
desconhecidos), não versiona a rota e reusa toda a infra de listagem já pronta.

**Fonte dos dados:** o catálogo hoje é **client-only**. Para o server servir os efeitos, recomendamos
extrair o catálogo para um **JSON compartilhado** (fonte única lida por client e server). Se o esforço
não couber, portar/duplicar o catálogo para o projeto Server é o caminho mais rápido — com alerta de
**drift** entre client e server.

---

## 1. De onde vêm os dados hoje

O catálogo de perks vive **hardcoded em C# no client**:

- `mods/CustomClasses/modded/Client/PerksCatalog.cs`
  - `Library` = `Dictionary<string, PerkGroup>` — cada grupo é um perk/drawback **nomeado** (ex.:
    `"sharpshooter"`, `"shaky_hands"`), keyed por uma **chave EN estável** (não é o nome da classe).
  - `PerkGroup` = `{ NameEn/NamePt, Icon/IconAlt (ESkillId?), Lines[] }`.
  - `PerkLine` = um **efeito atômico** (uma variável): `{ TitleEn/TitlePt, LabelEn/LabelPt, Format,
    Multiplier, Polarity, FlagIsPerk, Pending, Icon (EBuffId), Live }`.
  - `ByClass` = `Dictionary<string, string[]>` — mapeia **nome EN da classe** → array de chaves de
    perk. **A ordem do array é a ordem de exibição.** Classes atuais: `Combat Medic`, `Rifleman`,
    `Hunter`, `Stealth`, `Scavenger`, `Tank`.
- `mods/CustomClasses/modded/Client/MultiplierFormat.cs`
  - `ValueToken(PerkLine)` → produz o token exibido (`"+30%"`, `"−10%"`, `"×0.85"`, `"✓"`, `"✗"`).

Os **valores são ajustáveis via F12** por cada cliente, através do `PerksConfig`. No catálogo isso
aparece como a lambda `PerkLine.Live` (ex.: `live: () => PerksConfig.SharpshooterAdsTime?.Value ?? 0.85f`).
Quando `Live != null`, o getter `PerkLine.Multiplier` **resolve o valor vivo do F12 daquele cliente**;
quando `Live == null`, usa o valor nominal hardcoded (`_multiplier`, passado na fábrica `P(...)`).

---

## 2. O que o server deve servir: os VALORES NOMINAIS (não o F12)

O server (e portanto o launcher) deve expor os **valores nominais do catálogo** — os defaults
hardcoded, o `_multiplier` fallback — **não** os valores vivos do F12 de cada cliente.

**Por quê:**

- O F12 é **por cliente/por processo do jogo**. O server não conhece o `PerksConfig` de um cliente
  específico, e o launcher roda **antes** de qualquer sessão de jogo (é um **seletor pré-registro**).
- O propósito da tela é mostrar o que a classe faz **nominalmente** — "o que você ganha ao escolher
  essa classe" —, não o estado sintonizado de um jogador.

**Detalhe importante de implementação:** por design, o valor nominal passado na fábrica
(`_multiplier`) **coincide** com o default embutido na lambda `Live`. Exemplos do catálogo:

| Efeito | `_multiplier` (nominal) | `Live` (default do F12) |
|---|---|---|
| Sharpshooter | `0.85` | `PerksConfig.SharpshooterAdsTime ?? 0.85` |
| Iron Lungs (duração) | `1.5` | `1f / (IronLungsBreathDrain ?? 0.667)` ≈ `1.5` |
| Pack Mule | `1.3` | `1f + (PackMule…CarryBonus ?? 0.3)` = `1.3` |

Ou seja: sirva o `_multiplier`. Se o catálogo for portado/duplicado para o server (opção **a** da §8),
**não invoque a lambda `Live`** — ela depende de `PerksConfig`/BepInEx, que não existe no processo do
server. Use o valor cru.

---

## 3. Mapeamento `editionKey` ↔ nome EN

O `ByClass` do catálogo é keyed pelo **nome EN da classe** (`"Hunter"`, `"Tank"`, …). Mas a rota do
server é keyed por `editionKey`. A ponte:

- `mods/CustomClasses/modded/Server/ClassDefinition.cs`
  - `def.Name` → identificador PT que vira o `editionKey`.
  - `def.DisplayName?.En` → o **nome EN** da classe.
- No `ClassListRouter` (`mods/CustomClasses/modded/Server/ClassListRouter.cs`), o item já é montado com
  `DisplayName.En = def.DisplayName?.En ?? name`. **Use esse EN** para o lookup em
  `PerksCatalog.ByClass[<nomeEn>]`.

**Regra:**

1. Para cada `ClassListItem`, obtenha o nome EN (`def.DisplayName?.En ?? def.Name`).
2. Procure em `ByClass`. Se **não houver entrada** (ex.: classe custom nova, ou **Peladão/Naked**),
   `effects` é **vazio ou omitido** — não é erro.
3. Se houver, expanda as chaves de perk na ordem do array (ver §5) e ache o `PerkGroup` correspondente
   em `Library`.

> Observação: o `editionKey` que a rota serve pode estar em PT ou EN dependendo do pipeline de língua
> (ver comentários em `ClassListRouter`). **Não** faça o join dos perks pelo `editionKey` — faça
> **sempre por `DisplayName.En`**, que é a chave real do `ByClass`.

---

## 4. `valueToken` — as 3 regras de formatação (espelhar `MultiplierFormat.ValueToken`)

O server deve produzir o token **pronto**. O launcher **não** reimplementa a derivação — só renderiza a
string. Espelhe exatamente `MultiplierFormat.ValueToken`:

| `Format` | Regra | Exemplos |
|---|---|---|
| **Percent** | `(mult > 1 ? "+" : "−")` + `round(abs(mult − 1) · 100)` + `"%"` | `1.3` → `"+30%"` · `0.85` → `"−15%"` · `0.7` → `"−30%"` |
| **Multiplier** | `"×"` + `mult.ToString("0.##")` | `0.85` → `"×0.85"` · `1.25` → `"×1.25"` · `3.5` → `"×3.5"` |
| **Flag** | `IsPerk ? "✓" : "✗"` (qualitativo, sem número) | perk → `"✓"` · drawback → `"✗"` |

**Atenção ao caractere de menos:** no caso Percent, o sinal negativo é **U+2212 (MINUS SIGN, `−`)**,
**não** o hífen-menus ASCII `-` (U+002D). Copie o caractere exato de `MultiplierFormat.cs` linha 30.
Os símbolos de Flag são `✓` (U+2713) e `✗` (U+2717).

`valueToken` pode, em contrato, ser **string vazia** `""` (o launcher trata como "sem token, mostra só
o label"). Hoje as três `Format` sempre produzem token não-vazio — mas o launcher não deve assumir isso.

### 4.1 `isPerk` por linha (para o server decidir a cor/coluna)

`isPerk` vem de `PerkLine.IsPerk` (`PerksCatalog.cs` linhas 45-47):

```csharp
IsPerk = Format == ValueFormat.Flag
    ? FlagIsPerk
    : (Polarity == Polarity.HigherBetter) == (Multiplier > 1f);
```

- **Flag** → usa `FlagIsPerk` literal.
- **Percent/Multiplier** → é perk quando a direção "boa" bate com a direção do multiplicador:
  `HigherBetter` com `mult > 1`, **ou** `LowerBetter` com `mult < 1`. Caso contrário, drawback.

No launcher: `isPerk: true` → **verde** (`#9ad27a`), coluna da esquerda. `isPerk: false` → **vermelho**
(`#d27a7a`), coluna da direita. `pending: true` → **âmbar** (`#cc9a3e`, "em breve"), sobrepõe a cor.

---

## 5. Ordem dos efeitos

O array `effects` deve preservar a **ordem de exibição**:

1. Ordem das chaves de perk em `ByClass[<classe>]` (ordem do array).
2. Dentro de cada grupo, ordem das `PerkGroup.Lines`.

Ou seja: **achate** (`flatten`) `ByClass → grupos → Lines` nessa ordem, emitindo **um `effect` por
`PerkLine`**. Perks e drawbacks **podem vir misturados** no array (um grupo é homogêneo, mas classes
têm grupos perk e drawback intercalados). O launcher **separa por `isPerk`** ao renderizar as duas
colunas — não dependa da ordem para saber a coluna, use `isPerk`.

---

## 6. Localização

`title` e `label` são **sempre** objetos `{ "en": ..., "pt": ... }` (nunca string solta), espelhando o
`LocalizedPair` já usado em `displayName`/`description` (`ClassListResponse.cs`). O launcher escolhe
**pt → en** (fallback para EN se PT ausente).

- `title` ← `PerkLine.TitleEn` / `PerkLine.TitlePt`
- `label` ← `PerkLine.LabelEn` / `PerkLine.LabelPt`

---

## 7. Intencionalmente OMITIDO do contrato (decisões, não esquecimento)

1. **Ícone por efeito.** Cada `PerkLine` tem um `Icon` (`EBuffId`) que no jogo vira um sprite da tela de
   Skills (`StaticIcons.BuffIdSprites`). O launcher **não tem acesso aos sprites do EFT** (é um processo
   externo, sem os assets do jogo). Portanto o ícone **não** entra no contrato. Se um dia quisermos
   ícones no launcher, será por outra via (ex.: exportar PNGs) — fora deste escopo.
2. **Valores vivos do F12.** Conforme §2, o server serve valores **nominais**. O estado sintonizado do
   `PerksConfig` de um cliente não é conhecido pelo server nem relevante para o seletor pré-registro.

---

## 8. Como o server passa a ter acesso ao catálogo (hoje client-only)

O `PerksCatalog` está no projeto **Client**. Para o **Server** servir os efeitos, há 3 caminhos:

### (a) Portar/duplicar o catálogo para o projeto Server (C#)
- **Prós:** server autônomo; caminho mais rápido de implementar; sem novo formato de arquivo.
- **Contras:** **duplicação** do catálogo; **drift** — mexer nos valores no client e esquecer o server
  (ou vice-versa) deixa launcher e jogo divergentes. Exige disciplina/checklist para manter em sincronia.
- Se seguir por aqui: sirva o `_multiplier` cru, **sem** as lambdas `Live` (dependem de `PerksConfig`).

### (b) Extrair o catálogo para um JSON compartilhado (fonte única) — **RECOMENDADO**
- Client e server leem o **mesmo** arquivo JSON (o catálogo vira dado, não código). O client mantém
  as lambdas `Live` (F12) por cima dos valores nominais lidos do JSON; o server lê os valores nominais
  direto.
- **Prós:** **sem drift** — uma única fonte de verdade; alinhado com o resto do contrato (dados, não
  código); facilita futuras ferramentas.
- **Contras:** exige **refactor do client** para ler o catálogo do JSON (hoje é `Dictionary` literal em
  C#), incluindo mapear `EBuffId`/`ESkillId` e as lambdas `Live` por chave.

### (c) O client ESCREVE um JSON no boot que o server lê
- **Prós:** o catálogo continua morando no client (sem refactor de leitura).
- **Contras:** **acoplamento de ordem de boot** (o server precisa do arquivo já escrito), arquivo
  intermediário no disco, e um estado inválido possível na primeira execução / se o client não rodar.

**Recomendação:** **(b)** como fonte única de verdade, **se o esforço couber** no escopo do 029.
Caso contrário, **(a)** como caminho mais rápido — **desde que** se adote um checklist explícito de
sincronização client↔server para conter o drift. Evitar **(c)** (acoplamento de boot frágil).

---

## 9. Contrato recomendado — estender `GET /customclasses/classes`

Adicionar um campo **opcional** `effects` (array) em cada `ClassListItem`
(`mods/CustomClasses/modded/Server/ClassListResponse.cs`). O `JsonUtil` do SPT serializa com
`WhenWritingNull`, então **ausente/null = classe sem perks** — o launcher já trata campo ausente como
"sem efeitos".

### 9.1 Shape de cada efeito

```json
{
  "isPerk": true,
  "pending": false,
  "title":  { "en": "Sharpshooter", "pt": "Atirador" },
  "label":  { "en": "aim (ADS) time, all weapons", "pt": "mira (ADS), todas as armas" },
  "valueToken": "−15%"
}
```

| Campo | Tipo | Origem | Notas |
|---|---|---|---|
| `isPerk` | `bool` | `PerkLine.IsPerk` | `true` = perk (verde, esquerda) · `false` = drawback (vermelho, direita) |
| `pending` | `bool` | `PerkLine.Pending` | `true` = "em breve" (âmbar) |
| `title` | `{en,pt}` | `PerkLine.TitleEn/TitlePt` | sempre objeto localizado |
| `label` | `{en,pt}` | `PerkLine.LabelEn/LabelPt` | sempre objeto localizado |
| `valueToken` | `string` | `MultiplierFormat.ValueToken` | PRÉ-FORMATADO; `"+30%"`, `"×0.85"`, `"✓"`, `"✗"` ou `""` |

### 9.2 Exemplo completo — `ClassListItem` estendido (classe **Combat Medic**)

Classe real, mostrando o **achatamento** de `ByClass["Combat Medic"]` =
`["combat_medic", "efficient_metabolism", "shaky_hands"]` → 5 efeitos (4 perks + 1 drawback), na ordem
de exibição. Os campos pré-existentes (`editionKey`, `displayName`, …) são omitidos aqui por brevidade,
mas continuam presentes no item real.

```json
{
  "editionKey": "Médico de Combate",
  "displayName": { "en": "Combat Medic", "pt": "Médico de Combate" },
  "description": { "en": "…", "pt": "…" },
  "iconUrl": "/CustomClasses-Server/icons/medico.png",
  "nameColor": "#7ac0d2",
  "skills": { "Surgery": 3 },
  "skillMultipliers": { "Surgery": 1.5 },
  "effects": [
    {
      "isPerk": true,  "pending": false,
      "title": { "en": "Rapid Care",   "pt": "Cuidado Rápido" },
      "label": { "en": "heal/stab use time", "pt": "tempo de cura/estabilização" },
      "valueToken": "−30%"
    },
    {
      "isPerk": true,  "pending": false,
      "title": { "en": "Swift Surgeon", "pt": "Cirurgião Ágil" },
      "label": { "en": "surgery time",  "pt": "tempo de cirurgia" },
      "valueToken": "−50%"
    },
    {
      "isPerk": true,  "pending": false,
      "title": { "en": "Mobile Surgery", "pt": "Cirurgia em Movimento" },
      "label": { "en": "walk during surgery", "pt": "andar durante a cirurgia" },
      "valueToken": "✓"
    },
    {
      "isPerk": true,  "pending": false,
      "title": { "en": "Efficient Metabolism", "pt": "Metabolismo Eficiente" },
      "label": { "en": "hunger/thirst drain",  "pt": "fome/sede" },
      "valueToken": "−15%"
    },
    {
      "isPerk": false, "pending": false,
      "title": { "en": "Shaky Hands", "pt": "Mãos Trêmulas" },
      "label": { "en": "recoil",      "pt": "recuo" },
      "valueToken": "×1.25"
    }
  ]
}
```

> Note como perk e drawback aparecem **no mesmo array** (o `shaky_hands` é o último); o launcher separa
> por `isPerk`. O `valueToken` do `Shaky Hands` é `"×1.25"` (Multiplier), e o dos Percent usa o menos
> **U+2212** (`−30%`, `−15%`).

---

## 10. ALTERNATIVA — rota nova `GET /customclasses/perks`

Em vez de estender, criar uma rota dedicada retornando só os efeitos por classe:

```json
[
  { "editionKey": "Médico de Combate", "effects": [ /* … igual à §9.1 … */ ] },
  { "editionKey": "Caçador",           "effects": [ … ] }
]
```

| | Estender `classes` (§9) | Rota nova `perks` (§10) |
|---|---|---|
| Backward-compat | ✅ campo opcional, launcher 2.x ignora | ✅ rota nova não afeta a antiga |
| Requests do launcher | **1** (já busca `classes`) | **2** (precisa casar por `editionKey`) |
| Reuso de infra | ✅ mesma montagem/dedupe do `ClassListRouter` | ⚠️ duplica filtro Enabled/Registered/dedupe |
| Acoplamento de dados | efeitos junto da classe (coeso) | join por `editionKey` no launcher |
| Payload | levemente maior sempre | separável (lazy) |

**Recomendação: estender** (§9). O launcher já consome `classes`; um `effects` opcional evita segundo
request e segundo join, e reusa toda a lógica de filtro/dedupe já existente e testada no
`ClassListRouter`. A rota separada só valeria se o payload de efeitos fosse grande e raramente usado —
não é o caso.

---

## 11. Backward-compat e versionamento

- Adicionar `effects` opcional **não quebra** clientes atuais: o launcher 2.x ignora campos que não
  conhece, e classes sem perks simplesmente **omitem** o campo (`WhenWritingNull`).
- **Não versionar a rota.** O contrato SP0 de `/customclasses/classes` permanece; `effects` é uma
  extensão aditiva e opcional.

---

## 12. Checklist de aceite (para a sessão do CustomClasses)

- [ ] A rota serve `effects` por `editionKey`, correlacionado à classe certa via `DisplayName.En` →
      `PerksCatalog.ByClass`.
- [ ] Cada `effect` traz `isPerk`, `pending`, `title{en,pt}`, `label{en,pt}`, `valueToken`.
- [ ] `valueToken` bate **exatamente** com `MultiplierFormat.ValueToken`: Percent com sinal e **U+2212**
      no negativo (`−15%`), Multiplier `×0.85`, Flag `✓`/`✗`.
- [ ] O server usa os **valores nominais** (`_multiplier`), **não** as lambdas `Live`/F12.
- [ ] Classe **sem entrada** em `ByClass` (ex.: **Peladão/Naked**) → `effects` **vazio ou omitido**
      (sem erro/exceção).
- [ ] **Ordem preservada**: `ByClass` (ordem do array) × `Lines` (ordem dentro do grupo); perks e
      drawbacks podem vir misturados.
- [ ] `title`/`label` sempre com **`pt` e `en`** presentes (objeto `{en,pt}`).
- [ ] Backward-compat: request antigo (sem interesse em `effects`) continua funcionando; rota **não**
      versionada.
- [ ] (Se opção **a** — catálogo duplicado no server) checklist de sincronização client↔server anotado
      para conter drift.

---

## 13. Arquivos-fonte de referência (repo)

| Arquivo | Papel |
|---|---|
| `mods/CustomClasses/modded/Client/PerksCatalog.cs` | Catálogo hardcoded: `Library`, `ByClass`, `PerkGroup`, `PerkLine`, regra `IsPerk` |
| `mods/CustomClasses/modded/Client/MultiplierFormat.cs` | `ValueToken` (formatação a espelhar) + cores hex |
| `mods/CustomClasses/modded/Server/ClassListRouter.cs` | Rota atual `GET /customclasses/classes` a estender |
| `mods/CustomClasses/modded/Server/ClassListResponse.cs` | DTO `ClassListItem` + `LocalizedPair` (onde adicionar `effects`) |
| `mods/CustomClasses/modded/Server/ClassDefinition.cs` | `Name` (=editionKey PT) e `DisplayName.En` — a ponte do mapeamento |

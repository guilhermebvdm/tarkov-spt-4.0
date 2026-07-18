---
title: SPT 4.0 vs SPT 4.1 — Deofuscação de GClass/GInterface/GStruct
date: 2026-07-06
status: 🟢 Vivo
authors: Guilherme + agente
---

# SPT 4.0 vs SPT 4.1 — Deofuscação de GClass/GInterface/GStruct

Origem: conversa no Discord com um modder do WTT (FireFly), que descreveu a principal mudança estrutural do SPT/EFT 4.1 como "desofuscação do código — mataram as assombrações de GClass" e enviou os arquivos de mapeamento que hoje vivem em [`docs/files-from-4.1/`](../files-from-4.1/).

**Escopo deste documento:** cobre apenas a mudança de nomenclatura de identificadores ofuscados (classes, interfaces, structs, delegates, eventos, exceptions e atributos), que é a única mudança estrutural do 4.1 confirmada até agora por essa fonte. Não é um changelog completo do 4.1 — outras mudanças (gameplay, itens, hideout) não estão documentadas aqui.

---

## 1. O problema hoje (SPT 4.0)

O decompilado do cliente EFT usado neste repo (`references/eft-decompiled/`) expõe identificadores sequenciais sem significado — `GClass774`, `GInterface34`, `method_11`, `action_3` — numerados pelo obfuscator da BSG. Duas consequências diretas, já catalogadas em [`spt-antipatterns.md`](spt-antipatterns.md):

- **AP-03** (Alvo virtual/ofuscado sem auditar overrides): "alvos ofuscados (`GClass####`, `method_##`) resolvidos por nome literal quebram entre builds do EFT."
- A numeração **não é estável nem dentro da mesma major version** — o mesmo `GClassNNNN` pode apontar para classes diferentes em builds diferentes.

O próprio código deste repo já tem defesas explícitas contra isso. Exemplo real, [`StanceManager.cs:1138-1144`](../../mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs#L1138):

```csharp
// === REFLECTION CACHEADA PARA BACKING FIELDS DE EVENTOS DE GClass774 ===
// Os backing fields são private e foram renomeados pelo decompilador.
// Resolvemos por lista de candidatos: nome público primeiro, nomes do decompilador como fallback.
private static readonly FieldInfo _onValueChangedBacking =
    ResolveBackingFieldByCandidates(typeof(GClass774), nameof(GClass774.OnValueChanged), "action_3");
```

E em [`PassiveMountDetectPatch.cs:13`](../../mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/PassiveMountDetectPatch.cs#L13), o comentário trata `GClass2667` literalmente como "caixa-preta" — não há nome para saber o que a classe faz além de inferir pelo uso.

## 2. O que muda no 4.1

```
Cliente ofuscado (hoje)        Tabelas de mapeamento           Cliente 4.1
GClass774, method_11    ──►    docs/files-from-4.1/*.json ──►  Stamina, WeaponMountingComponent
GInterface34                                                    ISourceGroup
```

Os 10 arquivos JSON(C) em `docs/files-from-4.1/` mapeiam identificador ofuscado → nome real, um arquivo por categoria:

| Arquivo | Entradas de topo | Cobre |
|---|---|---|
| `GClass-Mappings.json` | 3.599 | Classes ofuscadas de nível superior (a maior categoria, de longe) |
| `GInterface-Mappings.json` | 498 | Interfaces ofuscadas (`GInterfaceNN` → `ISourceGroup`, etc.) |
| `GStruct-Mappings.json` | 348 | Structs ofuscadas |
| `Named-Class-Mappings.json` | 222 | Classes que **já têm** nome real, mas escondem tipos aninhados ofuscados dentro (ex.: `BetterAudio` já é nomeada, mas seus tipos genéricos internos `Class510\`1`, `GClass886\`1` não eram) |
| `Class-Mappings.json` | 142 | Outra família de nomes sequenciais (`Class99`…), sem o prefixo `G` |
| `GAttribute` / `GDelegate` / `GEvent` / `GException` -Mappings.json | 92 (soma) | Categorias menores — atributos, delegates, eventos e exceptions ofuscados |
| `Interface-Mappings.json` | 13 | Interfaces já nomeadas com ajuste de namespace |

Contagem de entradas de topo por arquivo. Somando também os tipos aninhados (`NestedTypes`) dentro de cada entrada, o total de identificadores renomeados passa de **5.800**.

> 🔎 **Vista consolidada, grep-friendly:** [`consolidated-mappings.txt`](../files-from-4.1/consolidated-mappings.txt) achata as 10 categorias numa lista `nome-4.0 -> Namespace.Tipo-4.1` (1 linha por tipo, aninhados com `+`; 5.961 linhas). É a superfície de **consulta** do harness — `grep '^GClass680 -> '` resolve num passo, sem carregar 338 KB. **É um índice de conveniência, não um substituto dos JSONs:** um diff (2026-07-18) confirmou que ela **não é superset estrito** — ao menos 22 chaves dos JSONs (`Class1051`, `Class1124`, `Interface1`, …) não aparecem no flat, e os JSONs guardam os campos tipados (`kind`/`namespace`/`NestedTypes`) que o flat descarta. Tratar o flat como resolvedor rápido e os JSONs como a visão categorizada — não aposentar um pelo outro sem reconciliar. Uso e ressalvas para agentes: [.agents/resources.md](../../.agents/resources.md) → nota "Deofuscação de nomes" + skill `graph-code-navigation`.

## 3. Exemplos reais no nosso código

Cruzando `GClass\d+` usados nos mods deste repo com as tabelas, seis referências batem exatamente:

| Ofuscado hoje | Nome real (4.1) | Onde aparece no repo | Status |
|---|---|---|---|
| `GClass774` | `Stamina` | `StanceManager.cs:1138` — reflection cacheada pros eventos privados | ✅ mapeado |
| `GClass2667` | `WeaponMountingComponent` (`EFT.WeaponMounting`) | `PassiveMountDetectPatch.cs:13` — comentada como "caixa-preta" | ✅ mapeado |
| `GClass2666` | `MountingPointDetectionSystem` (`EFT.WeaponMounting`) | `DumpMountingApp/gclass2666.txt` — dump de apoio ao item 011 | ✅ mapeado |
| `GClass897` | `BulletSoundsUtils` | `UnderFire-2.0.1/Plugin.cs:106` | ✅ mapeado |
| `GClass2813` | `GestureCommandMessage` (`EFT.NextObservedPlayer`) | `UnderFire-2.0.1/Plugin.cs:177` | ✅ mapeado |
| `GClass2823` | `MountingCommandMessage` (`EFT.NextObservedPlayer`) | `UnderFire-2.0.1/Plugin.cs:177` | ✅ mapeado |
| `GClass898` | — | `UnderFire-2.0.1/Plugin.cs:137` | ⚠️ sem entrada nas tabelas |
| `GClass3008` | — | `UnderFire-2.0.1/Plugin.cs:172-185` | ⚠️ sem entrada nas tabelas |

## 4. Relação com o AP-03 já documentado

A deofuscação **não elimina** o AP-03 — patches ainda vão precisar validar o alvo contra o assembly real (dnSpy/ilspycmd), porque assinatura de método pode mudar mesmo com nome estável. Mas ela remove a causa mais comum do sintoma: hoje o número muda a cada build; com nome real, o nome tende a persistir entre builds (o alvo deixa de ser "GClass2667 nesta build específica" e passa a ser "WeaponMountingComponent", ponto).

## 5. Ressalvas sobre a fonte desses arquivos

- **Não parece um dump oficial da BSG.** `GStruct-Mappings.json` e `GInterface-Mappings.json` têm comentários manuscritos como `// Custom, doesn't exist in the 1.0 client anymore` e `// Yo dawg I heard you were interested in some nested classes so I put some nested classes in your nested classes` — indício de uma base mantida pela comunidade de engenharia reversa ao longo de várias versões do jogo (não gerada oficialmente só para o 4.1).
- **Cobertura não é 100%.** `GClass898` e `GClass3008`, usados no próprio `UnderFire-2.0.1/Plugin.cs`, não aparecem em nenhum dos 10 arquivos.
- **A numeração já era instável antes disso** — não é um problema introduzido pelo 4.1, é o motivo pelo qual desofuscar é uma boa notícia estrutural.

## 6. Ação recomendada quando o 4.1 sair de fato

1. Levantar todas as ocorrências de `GClass\d+`/`method_\d+`/`action_\d+` nos mods (`stancesAndCameraPositionSPT4.0.11`, `UnderFire-2.0.1`, `TRLTraderPrices`, `CustomClasses`) e remapear pelos nomes reais confirmados contra o assembly do 4.1.
2. Tratar `GClass898` e `GClass3008` (e qualquer outro identificador sem entrada nas tabelas) como prioridade de reconfirmação manual — mesmo procedimento já descrito na seção de "decompile parcial" do `spt-antipatterns.md`.
3. Manter os fallbacks defensivos existentes (padrão `ResolveBackingFieldByCandidates`) como rede de segurança — não removê-los só porque o nome ficou estável, já que a cobertura das tabelas não é total.
4. Antes de reescrever patches em massa, aguardar o 4.1 realmente sair — a FireFly foi clara que "falta muita coisa" e essas tabelas são referência, não confirmação final.

## Ver também

- [`spt4-vs-spt41-gclass-deobfuscation.html`](spt4-vs-spt41-gclass-deobfuscation.html) — versão visual deste documento (diagrama de fluxo + tabelas), mesma análise, formato para compartilhar
- [`docs/files-from-4.1/consolidated-mappings.txt`](../files-from-4.1/consolidated-mappings.txt) — vista consolidada flat `nome-4.0 -> FQN-4.1` (superfície de grep do harness)
- [`docs/files-from-4.1/`](../files-from-4.1/) — dados brutos (os 10 arquivos JSON, por categoria)
- [`spt-antipatterns.md`](spt-antipatterns.md) — AP-03 e a seção de decompile parcial
- Skill `graph-code-navigation` — como localizar overrides/callers antes de repatchear

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-06 | Guilherme + agente | Criação — análise dos arquivos de mapeamento de deofuscação enviados por modder do WTT (FireFly), cruzados com `GClass\d+` usados nos mods deste repo. |
| 2026-07-06 | Guilherme | Merge branch 'feat/053-perks-property-model' |
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
| 2026-07-18 | Guilherme + agente | Consolidação flat (`consolidated-mappings.txt`, 5.961 linhas `nome-4.0 -> FQN-4.1`) movida do `.txt` solto para `docs/files-from-4.1/`; registrada como superfície de grep do harness (§2 + resources.md + skill `graph-code-navigation`). Diff confirmou que **não** é superset dos JSONs. |
| 2026-07-18 | Guilherme | spec(TRL-ImmersiveCombatMedicine): 001 review round A applied — P9+P10, assembly-verified anchors |

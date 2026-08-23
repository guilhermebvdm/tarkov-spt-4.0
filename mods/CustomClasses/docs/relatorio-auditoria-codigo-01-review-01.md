---
title: "Revisão do Relatório de Auditoria de Código — CustomClasses (Review 01 do relatório 01)"
date: 2026-08-22
status: 🟢 Vivo
authors: Claude (revisão adversarial 2026-08-22)
---

# Revisão do Relatório de Auditoria de Código — CustomClasses (Review 01 do relatório 01)

> Revisão adversarial de [relatorio-auditoria-codigo-01.md](relatorio-auditoria-codigo-01.md), feita **antes** de qualquer Decisão humana ser marcada. O relatório 01 é imutável: as correções abaixo entraram lá como **anotações** (`⚠️ Revisão 01`) e um achado novo (`AUD-01-08`), nunca como reescrita.

## 1. Escopo e premissas

Reapliquei as premissas da própria auditoria e **acrescentei cinco** que ela não usou:

| # | Premissa | Origem |
|---|---|---|
| P1 | Modelo de custo: frequência × entidades × duração × acúmulo, ajustado por custo unitário | `spt-performance-analysis` §1 (a auditoria já usou) |
| P2 | Taxonomia FREQ · PATCH · ENT · LIFE · GROW · UNITY · ALLOC · LOG · IO · CFG | idem §2 (a auditoria usou **parcialmente** — ver RV-03) |
| P3 | Grafo aponta, **leitura prova**: todo `arquivo.cs:linha` tem de resolver | idem §3 / AGENTS.md |
| P4 | Sucesso = medição; sem número antes/depois nada fecha | idem §7 / AP-06 |
| P5 | Falso positivo é custo: código frio e trabalho já gated **não são achado** | idem §8 |
| **P6** | **Verificabilidade:** cada citação do relatório é reaberta e conferida — inclusive as do decompile | acrescentada |
| **P7** | **Cobertura declarada:** o relatório precisa dizer o que **não** foi lido; veredicto universal exige cobertura universal | acrescentada |
| **P8** | **Retenção e VRAM como lente própria**, não subproduto do exame de CPU | `spt-memory-leak-analysis` §3 (STAT/HOT) — acrescentada |
| **P9** | **Fika headless como perfil de execução distinto**, não como nota de rodapé | `spt-memory-leak-analysis` §1 — acrescentada |
| **P10** | **Falseabilidade e custo da correção:** cada achado diz o que o **refutaria** e quanto custa consertar | acrescentada |

## 2. Veredicto da revisão

**O núcleo técnico do relatório 01 se sustenta. A sua conclusão categórica, não.**

Reabri e confiri **todas** as 30 citações (mod + decompile + spt-source): nenhuma está errada. A estimativa de frequência dos 33 alvos Harmony, a integridade dos gates da regra 075, a verificação de que `TotalErgonomics`/`UpdateWeaponVariables`/`UpdateSwayFactors`/`CarryingWeightRelativeModifier` **não** são por-frame, e a conclusão de que não há alavanca de configuração — tudo isso resistiu.

O que não resistiu foi o alcance da conclusão. A auditoria **rodou as lentes de CPU-em-raid com rigor e as lentes de retenção/VRAM quase não rodou** — e é exatamente ali que estava o achado que faltou: um cache de textura que cresce sem teto, alimentado por uma ação do usuário no F12. Pior: ele mora num arquivo que a auditoria **abriu e leu** ([ClassIconCache.cs](../modded/Client/UI/ClassIconCache.cs)) — o que descarta "não vi o arquivo" e deixa só "não fiz a pergunta certa sobre ele".

Consequência direta: a frase *"nenhuma coleção do mod cresce — não há GROW neste mod"* (matriz de lifecycle do relatório 01) é **falsa**, e foi escrita com a mesma confiança do resto. Esse é o defeito mais grave desta revisão — não o achado perdido em si, mas a afirmação universal feita sem a varredura que a sustentaria.

### Resumo

| Severidade | Qtd | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 1 | Achado não detectado (`RV-01` → vira `AUD-01-08`) |
| 🟠 **Alto** | 2 | Afirmação categórica falsa no relatório; lente de retenção/VRAM sub-aplicada |
| 🟡 **Médio** | 4 | Dois "achados" não são de performance; evidência superdeclarada; instrumentação inviável como escrita; §4 internamente inconsistente |
| 🔵 **Baixo** | 3 | Cobertura não uniforme e não declarada; links mortos neste worktree; falta falseabilidade e custo da rodada |

---

## 3. Achados da revisão

### RV-01 · 🔴 Achado não detectado — cache de textura tingida cresce sem limite

**O que a auditoria perdeu.** [`ClassIconCache.GetTinted`](../modded/Client/UI/ClassIconCache.cs#L74-L137) guarda um `Sprite` por chave `nome|corTopo|corBase`. Cada entrada nova custa:

- `new Texture2D(256, 256, RGBA32)` → **256 KB de VRAM** (ícones confirmados 256×256 no disco);
- `tex.GetPixels32()` → `Color32[65536]` = **256 KB gerenciados por chamada**. Acima do limiar de 85 KB, então cada uma vai para o **Large Object Heap** — que não é compactado e só é recolhido em coleta de geração 2;
- 65.536 operações de pixel + `SetPixels32` + `Apply` (upload à GPU) + `Sprite.Create`.

**A entrada nunca é liberada.** `DestroySprite` só roda no `Dispose()`, que só é chamado no `Plugin.OnDestroy` (fechar o jogo). Não há substituição, teto nem invalidação.

**O gatilho que torna isso ilimitado.** A chave inclui a **cor**, e a cor é um `ConfigEntry<Color>` do F12 (item 067). A cadeia está toda montada:

```
F12: arrasta o picker de cor de uma classe
  → ConfigEntry<Color>.SettingChanged                (PerksConfig.cs:682)
  → PerksConfig.ClassColorsChanged                   (PerksConfig.cs:54)
  ├→ MenuClassIdentityPatch.RefreshColors            (Plugin.cs:88)
  │    → StartCoroutine(ApplyToMenu) → ApplyClassIcon
  │      → ClassIconCache.GetTinted(cor NOVA)        (ClassIdentityView.cs:134)  ← textura nova
  └→ SkillsClassTabPatch.OnColorsChanged             (SkillsClassTabPatch.cs:30)
       → rebuild da aba CLASS → PerksPanelView
         → ClassIconCache.GetTinted(cor NOVA)        (PerksPanelView.cs:242)     ← outra textura nova
```

Cada valor intermediário do arrasto vira uma cor distinta → uma chave distinta → **uma textura permanente**, por **dois** consumidores.

**Eixos de custo (P1):** per-event × 2 consumidores × **acúmulo sem teto** × vida = sessão inteira. Custo unitário **alto** (alocação em LOH + 65k pixels + upload à GPU).

**Dois regimes, como em AUD-01-01:**
- **Uso normal** (não mexe no picker): 1–2 cores por classe na sessão → um punhado de texturas. Inofensivo.
- **Arrastando o picker:** uma textura por evento de mudança. Se o ConfigurationManager emitir por quadro de arrasto — o comportamento típico de slider —, alguns segundos de arrasto produzem dezenas a centenas de entradas: **dezenas de MB de VRAM presos até fechar o jogo**, mais o mesmo volume de lixo no LOH, mais travamento visível enquanto se arrasta (é o pior caso somado ao AUD-01-01, que **compartilha o mesmo gatilho** — cada evento também reinicia a busca de 60 quadros no menu).

**Severidade: 🟡 Médio**, pela mesma régua aplicada ao AUD-01-01 (superfície de menu, acionada por ação específica do usuário). **Sobe para 🟠 Alto** se a medição confirmar emissão por quadro de arrasto — é retenção sem teto, e a régua do repo reserva 🟠 para vazamento.

**O que refutaria este achado (P10):** se o ConfigurationManager emitir `SettingChanged` só ao **soltar** o controle (um evento por escolha de cor, não por quadro), o crescimento fica na casa de unidades por sessão e o achado morre como preventiva. **É a primeira coisa a medir** — um contador de `TintedCache.Count` no fim de um arrasto responde em 10 segundos.

**Correção proposta:** (a) `DestroySprite` na entrada substituída quando a chave muda para o **mesmo ícone** (manter no máximo uma variante tingida viva por ícone — ninguém precisa do histórico de cores); ou (b) quantizar a cor na chave (arredondar cada canal para múltiplos de 8) — corta a cardinalidade em ~32× sem diferença visível; ou (c) descartar o pipeline de tingimento por textura e voltar ao `ClassIconGradient` (o `BaseMeshEffect` que já existe e **não aloca nada por cor** — reusa a lista `_verts`). A opção (c) é a de melhor custo/benefício, mas foi justamente o caminho abandonado no 06-fix-02 por falhar em `Image` criada em runtime; a decisão razoável é **(a) + (b)**.

**Como validar:** logar `TintedCache.Count` e a soma estimada de VRAM (`Count × 256 KB`) ao abrir/fechar o F12. Cenário pareado: arrastar o picker de uma classe por ~5 s, antes e depois. Critério: `Count` deixa de crescer com o arrasto (fica ≤ nº de ícones × 1). Não-regressão: o ícone da classe continua com o gradiente correto no menu, no chat, na tela de deploy e na aba CLASS, e trocar a cor no F12 continua refletindo ao vivo.

**→ Registrado no relatório 01 como `AUD-01-08`, com bloco de Decisão próprio.**

---

### RV-02 · 🟠 Afirmação categórica falsa no relatório

Matriz de lifecycle do relatório 01, linha "Múltiplas ondas": *"nenhuma coleção do mod cresce — **não há GROW neste mod**"*.

RV-01 prova o contrário. E o erro não é de digitação: é uma generalização feita a partir de uma varredura que não cobriu o caso. As coleções que a auditoria **de fato** conferiu (`ClassIdentities.ByNickname` substituída no `Commit`, `PerkDiag.LastLog` limpa por raid, `SeenNetIds` com `Clear`, `PerksConfig.ClassColors` fixa) estão todas corretas — mas `ClassIconCache.TintedCache` e `ClassIconCache.Cache` nunca entraram na lista.

**Correção aplicada:** a linha foi anotada no relatório 01 com a ressalva e o ponteiro para `AUD-01-08`.

---

### RV-03 · 🟠 Lente de retenção/VRAM sub-aplicada (P8)

A auditoria rodou FREQ, PATCH, ENT, LIFE, UNITY, LOG, IO e CFG com rigor. **GROW e ALLOC ela rodou só como um `grep`** — e só contra superfícies de raid (`grep` por `new List<`/`new Dictionary<`/LINQ dentro de `Patches/`). Nenhuma pergunta foi feita sobre:

- quem **destrói** as texturas/sprites que o mod cria, e quando;
- quais dicionários **estáticos** têm inserção sem remoção correspondente;
- o que acontece com estado de sessão (não de raid) ao longo de horas — o caso do headless.

Isso é uma lacuna de **método**, não de esforço: o `grep` do mecanismo ALLOC foi delegado à skill de leak (correto), mas a skill de leak nunca foi efetivamente aplicada, e o relatório não declarou essa delegação como "não executada".

**Correção aplicada:** o relatório 01 ganhou, no §1, a declaração explícita de que a varredura de retenção foi parcial, com ponteiro para esta revisão. **Recomendação:** rodar `/analyze-memory-leak CustomClasses` como frente própria — não é escopo do `--perf`, e um `--perf` não deve fingir que cobre isso.

---

### RV-04 · 🟡 Dois "achados" não são de performance pelo critério do próprio relatório

A skill é explícita: *"o objetivo é achar os maiores ofensores, não produzir 80 sugestões cosméticas"* e *"micro-otimização de código frio ilegível não entra"*. Dois itens violam isso:

- **AUD-01-03** (consolidar patches no mesmo alvo): o próprio texto admite que o ganho de CPU é "pequeno" e que o valor real é **estrutural** (legibilidade da ordem de composição do recuo). Ao mesmo tempo declara que é a mudança **de maior risco de regressão de balance** do conjunto. Um item de performance com ganho não medido, benefício declaradamente de manutenção e o maior risco da rodada **não deveria estar num relatório `--perf`** — é assunto de `/code-review` ou de dívida estrutural anotada.
- **AUD-01-06** (`GetType().Name` → `is HideoutPlayer`): o texto diz "desprezível em CPU" e justifica por "desvio de padrão". Isso é conformidade, não performance.

Nenhum dos dois está **errado** — os dois são observações válidas. O defeito é de **classificação**, e ele tem consequência prática: inflam a contagem de achados de um relatório cuja mensagem central é "não há o que otimizar", e empurram para uma rodada de trabalho que o próprio relatório diz não se justificar.

**Correção aplicada:** ambos anotados no relatório 01 como **fora do escopo de performance**, mantidos para rastreabilidade, e **retirados da recomendação de agrupamento do §4**.

---

### RV-05 · 🟡 "Suspeita: 0" superdeclara a evidência (P4)

O relatório declara "Forte 6 · Suspeita 0" e justifica: *"nenhum eixo de custo ficou em aberto na leitura"*. Isso mistura duas coisas que a skill separa:

- **Mecanismo** — provado por leitura. Aqui, sim, é Forte em todos.
- **Magnitude** — o produto real de frequência × entidades no setup do usuário. Em AUD-01-01 (quantos quadros até o painel aparecer) e em AUD-01-08 (quantos eventos por arrasto) a magnitude é **desconhecida**, e é ela que decide se o achado vale uma rodada.

Pela definição da skill, um achado com eixo não provado que **precisa de instrumentação antes de virar refactor** é **Suspeita**, não Forte. O relatório inclusive descreve a instrumentação para os dois — o que confirma a contradição.

Somando: o relatório é **o único desta leva sem um único número medido**, e ainda assim declara evidência máxima em todos os achados. O relatório irmão do DynamicSpawn está ancorado em captura de frametime, contagem de requisições e RSS; este está ancorado só em leitura.

**Correção aplicada:** AUD-01-01 e AUD-01-08 reclassificados para **Suspeita (mecanismo Forte, magnitude não medida)**; contagem do §1 corrigida para "Forte 4 · Suspeita 2 · Preventiva 1".

---

### RV-06 · 🟡 O plano de instrumentação não roda como está escrito

`INSTR-2` manda despejar os contadores *"1× no raid-end (`GameWorld.OnDestroy` ou o mesmo hook do `AdrenalineState.Reset`)"*. Dois problemas:

1. **`AdrenalineState.Reset` roda no raid-START**, não no end (é chamado do `RaidPerksNotificationPatch`, que postfixa `GameWorld.OnGameStarted`). Despejar ali reportaria os contadores da raid **anterior** — silenciosamente deslocados em uma raid.
2. **O mod não tem hook de raid-end nenhum.** Confirmei o inventário de patches: não há patch em `GameWorld.OnDestroy` nem em `BaseLocalGame.Stop`. Todo o reset de estado do mod acontece no **start** da raid seguinte — decisão de projeto legítima, mas significa que `INSTR-2` **exige adicionar um patch novo** que hoje não existe. Isso é mais invasivo do que "instrumentação temporária" sugere, e muda o risco da mini-rodada.

**Correção aplicada:** `INSTR-2` reescrito no relatório 01 para despejar por **tempo** (a cada 60 s, enquanto `GameWorld.Instantiated`) em vez de por raid-end — sem exigir hook novo. A observação "o mod não tem hook de raid-end" também foi registrada no Panorama, porque é um fato de lifecycle que vale para qualquer trabalho futuro.

---

### RV-07 · 🟡 O §4 se contradiz e empurra para o trabalho que ele mesmo desaconselha

O §4 abre com *"Não há 🔴 nem 🟠 para priorizar"* e emenda com um agrupamento de quatro achados para um item de backlog. Um leitor apressado lê a segunda parte. Se a leitura honesta é "nada aqui justifica uma rodada", a **opção default tem de ser "não abrir rodada"**, com o agrupamento apresentado como alternativa caso o usuário queira mesmo assim.

Falta também (P10) o **custo da própria rodada**: cada correção deste mod exige compilar, bumpar SemVer, parar o SPT.Server quando houver lado servidor, reinstalar, **reiniciar o EFT** (plugin BepInEx só recarrega no boot) e validar in-game com gate humano. Para um mod com 0 🔴 e 0 🟠, esse custo plausivelmente supera o ganho — e essa comparação é do usuário fazer, não minha de esconder.

**Correção aplicada:** §4 reescrito com a opção "não abrir rodada" como default declarado e o custo do ciclo explicitado.

---

### RV-08 · 🔵 Cobertura não uniforme e não declarada (P7)

A auditoria leu **integralmente** ~55% das 9.444 linhas do client (todos os patches de raid, os hubs de estado, o cache de ícones, os roteadores do servidor consumidos em raid). Os ~45% restantes — `PerksConfig.cs` (54 KB), `PerksPanelView.cs`, `SkillsClassTabPatch.cs`, `PerksCatalog.cs` e a maior parte de `Server/` — passaram só por **greps direcionados** da taxonomia.

Para um veredicto de "não há ofensor em raid" essa cobertura é **suficiente**, porque as superfícies de raid foram lidas por inteiro e os greps cobrem os mecanismos que faltavam. Para a frase *"o client não tem ofensor"* sem qualificador, não é. O relatório devia ter dito onde parou.

Reconferi o resíduo nesta revisão (li os arquivos pequenos que faltavam e escaneei os grandes por padrão quente): **nada novo apareceu além de RV-01**. Os arquivos de menu restantes usam reflexão cacheada por linha renderizada, sem laço por quadro. A cobertura está, agora, declarada.

---

### RV-09 · 🔵 Links do decompile apontam para arquivos ausentes neste checkout

A referência cruzada de AUD-01-02 usa `../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910`. O caminho relativo está **certo**, mas **este worktree não tem o dump em disco** (é gitignored; a auditoria leu do checkout principal `tarkov-spt-4.0/`). Quem clicar aqui cai num 404.

**Correção aplicada:** a citação passou a nomear a origem em texto (`MovementContext.cs:910`, decompile EFT 0.16.9) em vez de linkar, com nota de que o dump não existe neste worktree e como gerá-lo (`bash scripts/decompile-eft.sh`).

---

### RV-10 · 🔵 Falta "o que refutaria este achado" (P10)

Cada achado tem "Como validar" — que mede **o resultado da correção**. Nenhum tem o teste que decide se o achado **merece correção**. São perguntas diferentes, e a segunda vem antes.

Exemplo concreto: se o painel do Menu-Overhaul aparecer sempre em 1–2 quadros no setup do usuário, AUD-01-01 vale ~nada e a correção não paga o ciclo de build+restart+validação. Isso devia estar em destaque no achado, não implícito na descrição dos "dois regimes".

**Correção aplicada:** AUD-01-01 e AUD-01-08 ganharam a linha **"O que refutaria"** explícita.

---

## 4. O que se sustentou (verificado item a item)

Registrado para que a revisão não seja lida como demolição — a maior parte do relatório resistiu ao reexame:

| Afirmação do relatório 01 | Veredicto | Como conferi |
|---|---|---|
| As 30 citações `arquivo.cs:linha` (mod + decompile + spt-source) | ✅ **todas resolvem** | Reabri cada uma |
| `GameWorld.MainPlayer` é **campo** (`:572`), não propriedade com busca | ✅ | Decompile |
| `MaxSpeed`/`SprintingSpeed` são per-frame **e rodam para bots** (`BotMover:930/:985` → `ChangeSpeed`) | ✅ | Cadeia de chamadores provada |
| `TotalErgonomics`, `UpdateWeaponVariables`, `UpdateSwayFactors`, `CarryingWeightRelativeModifier` **não** são per-frame | ✅ | Todos os chamadores lidos; são orientados a evento |
| Gates de identidade de instância (regra 075) íntegros em 100% dos patches que rodam para bots/peers | ✅ | Conferido patch a patch |
| Só 2 métodos por-quadro no client, ambos neutralizados | ✅ | `grep` exaustivo de `Update`/`LateUpdate`/`FixedUpdate`/`OnGUI` + inventário de `MonoBehaviour` |
| Sem LINQ, alocação ou reflexão não cacheada em caminho de **raid** | ✅ | Greps da taxonomia + leitura |
| A lição do `GetLocaleDb` (item 022) está aplicada — `_localeCache` em `CatalogService.cs:219-231`, sem remanescente | ✅ | Grep de todos os acessos a locale/DB no servidor |
| `SaveServer.GetProfiles()` é **cópia rasa** → LINQ do roteador é desprezível | ✅ | `spt-source SaveServer.cs:147-150` |
| O mod **não tem alavanca de configuração** (nenhum timer/raio/verbosidade ajustável) | ✅ | 97 `ConfigEntry` classificadas |
| `AUD-01-01`, `AUD-01-02`, `AUD-01-04`, `AUD-01-05` como mecanismos | ✅ | Reconferidos |
| Observação fora de escopo sobre `PartyInfoPanelPrefetchPatch` (`Reset()+EnsureLoaded()` destrutivo) | ✅ | Confirmada; segue como nota |

## 5. Efeito líquido no relatório 01

| Item | Antes | Depois da revisão |
|---|---|---|
| Achados | 7 | **8** (`AUD-01-08` novo) |
| Distribuição | 0🔴 0🟠 1🟡 5🔵 1💡 | 0🔴 0🟠 **2🟡** 5🔵 1💡 — com AUD-01-03 e AUD-01-06 marcados **fora do escopo de performance** |
| Evidência | Forte 6 · Suspeita 0 | **Forte 4 · Suspeita 2** · Preventiva 1 |
| Veredicto | "não há ofensor em raid" | mantido **para raid**, com a ressalva de que a varredura de retenção/VRAM foi parcial e produziu 1 achado |
| Recomendação | agrupar 4 achados numa rodada | **default: não abrir rodada**; medir AUD-01-01 e AUD-01-08 primeiro (mini-rodada de instrumentação), decidir depois |
| Instrumentação | despejo no raid-end (**inviável** — não existe hook) | despejo periódico a cada 60 s + contador de `TintedCache.Count` |

## 6. Recomendação da revisão

1. **Não abrir rodada de otimização ainda.** Rodar só a **mini-rodada de instrumentação** prevista na Fase 1 (passo 2 do command): INSTR-1 (contagem de buscas no menu), INSTR-3 (`TintedCache.Count` por arrasto do picker) e, se quiser o baseline, INSTR-2. Uma build, `client-only`, bump de versão *patch*.
2. **Só então decidir.** Se a medição mostrar arrasto do picker gerando dezenas de texturas, `AUD-01-08` sobe para 🟠 e passa a justificar a rodada sozinho — e ela naturalmente carrega junto o `AUD-01-01`, que **compartilha o mesmo gatilho e o mesmo arquivo de correção**. Se não mostrar, os dois viram dívida anotada e o mod fica como está.
3. **Frente separada:** `/analyze-memory-leak CustomClasses`, para fechar de verdade a lente que este `--perf` só tangenciou (RV-03).
4. **Não misturar** `AUD-01-03` e `AUD-01-06` na rodada de performance — encaminhar por `/code-review` se quiser tratá-los.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-22 | Claude | Criação — revisão adversarial do relatório 01 com 5 premissas acrescentadas (verificabilidade, cobertura declarada, retenção/VRAM, headless, falseabilidade+custo). 10 achados de revisão: 1 🔴 (achado não detectado → `AUD-01-08`), 2 🟠, 4 🟡, 3 🔵. 13 afirmações do relatório reconferidas e sustentadas. |

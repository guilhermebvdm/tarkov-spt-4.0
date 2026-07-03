# 057 — Identidade de classe per-player em coop (Fika) · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [057-class-identity-coop-02-spec-tech.md](057-class-identity-coop-02-spec-tech.md)
**Data:** 2026-07-03

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
> Review executada por agente adversarial de contexto limpo (verificou cada `arquivo:linha` citado abrindo os
> arquivos); decisões tomadas em modo autônomo (`/g-autodev`) e registradas em cada ponto.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 11 · Total: 11

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🟡 | Using errado p/ `EmptyRequestData` no stub server | ✅ Resolvido |
| PA-01-02 | B — Edge | 🔴 | SCAV herda a classe do PMC (nickname do loading é sempre o do PMC) | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟡 | Brasão na linha prometido em §1/§4, ausente do stub | ✅ Resolvido |
| PA-01-04 | B — Edge | 🟡 | `_loaded` 1× por sessão: perfil novo invisível até restart | ✅ Resolvido |
| PA-01-05 | B — Edge | 🟡 | Fetch síncrono no main thread durante o loading | ✅ Resolvido |
| PA-01-06 | C — Lógica | 🟢 | 4 refs de linha erradas/off-by-one | ✅ Resolvido |
| PA-01-07 | A — Gap | 🟢 | §5.3 inconsistente (contrato do Refresh, `LocalIdentity`, `PanelState` NRE) | ✅ Resolvido |
| PA-01-08 | A — Gap | 🟢 | Fallback local precisa de `ClassNamePt` (privado hoje) | ✅ Resolvido |
| PA-01-09 | B — Edge | 🟢 | Perfil órfão: 01-spec pede log 1×, design silenciava | ✅ Resolvido |
| PA-01-10 | B — Edge | 🟢 | Determinismo do dedup entre restarts do server | ✅ Resolvido |
| PA-01-11 | B — Edge | 🟢 | Popover raycast-target pode roubar o hover de outra linha (flicker) | ✅ Resolvido |

## Pontos

### PA-01-01 · C — Lógica · 🟡 ✅ Resolvido em 2026-07-03

**Stub 5.1 não compila: using errado para `EmptyRequestData`**

**Problema:** O stub importava `SPTarkov.Server.Core.Models.Utils` alegando "usings espelhados da SkillMultipliersRouter.cs:1-14". Falso duas vezes: os usings reais são as linhas 1-5 e `EmptyRequestData` vive em `SPTarkov.Server.Core.Models.Eft.Common` (ref: SkillMultipliersRouter.cs:3; spt-source EmptyRequestData.cs:6). `Models.Utils` existe (contém `ISptLogger`) → o erro só estoura no `RouteAction<EmptyRequestData>` (CS0246).

**Por que importa:** implementador que confie no stub "já espelhado" quebra o build.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** stub corrigido na spec (§5.1) com os usings reais 1:1 + citação corrigida. Nit incluído: `namespace CustomClasses;` (convenção do projeto), não `CustomClasses.Server`.

### PA-01-02 · B — Edge · 🔴 ✅ Resolvido em 2026-07-03

**SCAV herda a classe do PMC — mecanismo por nickname viola corner da 01-spec**

**Problema:** A 01-spec exigia que raid scav não mostrasse a classe do PMC do mesmo dono. Mas o FIKA usa **sempre o nickname do PMC** na linha do loading (evidência: MatchmakerAcceptScreen_Show_Patch.cs:36 `FikaBackendUtils.PMCName = session.Profile.Nickname` com `IsScav` setado separado na :23; FikaClient.Callbacks.cs:464 `AddPlayer(NetId, FikaBackendUtils.PMCName)`) → o mapa por nickname casa e mostra a classe do PMC, garantido.

**Por que importa:** falha um checkbox explícito da spec funcional.

**Decisão:** `[x]` Caminho alternativo — **Resolução (autônoma, registrada):**
(a) **Local:** gate por `FikaBackendUtils.IsScav` lido via reflection (`AccessTools.TypeByName("FikaBackendUtils")` + campo/prop `IsScav` — zero tipos FIKA no IL, mesmo padrão do 055). Raid scav local → patch inteiro vira no-op (sem identidade em nenhuma linha, coerente: a tela é da raid scav).
(b) **Remotos:** indetectável com só `netId+nickname` (o side não trafega) → **limitação documentada** em §7 da spec técnica e **corner da 01-spec emendado** para o comportamento alcançável: "raid scav local → sem identidade em nenhuma linha; player remoto em raid scav pode exibir a classe do PMC do dono — limitação conhecida do mecanismo por nickname (cosmético)".

### PA-01-03 · A — Gap · 🟡 ✅ Resolvido em 2026-07-03

**Brasão na linha prometido no corpo da spec, ausente do stub**

**Problema:** §1/§4 prometiam "cor + brasão" na linha; stub 5.4 e checklist só tingiam o TMP. Inserir Image no prefab do FIKA (layout desconhecido) não tinha orientação nenhuma.

**Decisão:** `[x]` Caminho alternativo — **Resolução (autônoma):** **tint-only na linha** (satisfaz o "e/ou" do aceite da 01-spec); o brasão fica no popover (header + marca d'água, já existentes). §1/§4/checklist alinhados. Racional: mexer no layout de prefab de terceiro (FIKA) sem F12 de calibração é risco visual alto pra ganho marginal — candidato a polish futuro se o tint não bastar in-game.

### PA-01-04 · B — Edge · 🟡 ✅ Resolvido em 2026-07-03

**Perfil novo criado após o fetch fica invisível até restart do client**

**Problema:** fetch 1× por sessão de client; player novo no server coop vivo não aparece no mapa dos clients já abertos. §7 só cobria staleness de edits do editor.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** refetch **por instância da tela de loading**: o Postfix guarda o `__instance` (object, estático) e, quando muda (nova tela de loading/nova raid), chama `ClassIdentities.Reset()` antes de resolver → 1 fetch por raid (payload de dezenas de linhas; custo desprezível). Cobre também PA-01-05 parcialmente (frequência conhecida e baixa). Spec §5.2/§5.4/§7 atualizadas.

### PA-01-05 · B — Edge · 🟡 ✅ Resolvido em 2026-07-03

**Fetch síncrono no main thread durante o loading**

**Problema:** `RequestHandler.GetJson` bloqueante dentro do Postfix; corner "catálogo ainda não respondeu → UI não trava" da 01-spec não endereçado.

**Decisão:** `[x]` Caminho alternativo — **Resolução (autônoma):** **síncrono aceito e documentado** em §7: precedente idêntico no mesmo Postfix (`SkillMultipliers.EnsureLoaded`, 055, validado in-game), server LAN do caso de uso real, payload minúsculo, frequência 1×/raid (PA-01-04), e falha marca `_loaded` (sem retry em loop). Async traria race de "mapa chega depois das linhas" — pior que o custo. Se o gate in-game mostrar hitch mensurável, promover a fix 06 (fetch em `Task.Run` + apply no próximo `AddPlayer`).

### PA-01-06 · C — Lógica · 🟢 ✅ Resolvido em 2026-07-03

**4 refs de linha erradas/off-by-one**

**Problema/Resolução:** corrigidas na spec: `LoadingScreenUI.cs:24→25` (Headless); `ClassDetailLoadingPatch.cs:174→197` (`OnDestroy` — a única apontando código sem relação); `PerksPanelView.cs:22→23` (`_lastPanelClass`); `PerksCatalog.cs:177→178` (`LocalGroups`) e `SptProfile.cs:99→100` (`Edition`).

**Decisão:** `[x]` Aceitar sugestão.

### PA-01-07 · A — Gap · 🟢 ✅ Resolvido em 2026-07-03

**§5.3 internamente inconsistente**

**Problema:** contrato divergente (`Refresh(panel, Identity)` em §4/§5.4 vs 5 parâmetros soltos em §5.3); `LocalIdentity()` citado sem definição; guard `panel.GetComponent<PanelState>().LastClass` → NRE (componente nunca adicionado no `Build`).

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** contrato único `Refresh(GameObject, ClassIdentities.Identity)`; `ClassIdentities.Local()` definido (constrói Identity do `SkillMultipliers`, exige PA-01-08); `PanelState` obtido com `GetComponent<PanelState>() ?? AddComponent<PanelState>()`. §5.3 reescrita.

### PA-01-08 · A — Gap · 🟢 ✅ Resolvido em 2026-07-03

**Fallback local precisa de `ClassNamePt` (privado)**

**Problema:** `Identity.DisplayName` resolve pt→`NamePt`, mas `SkillMultipliers` só expõe `ClassNameEn`/`ClassName` — `_classNamePt` é privado (SkillMultipliers.cs:20-21) → fallback pt degradaria pra EN.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** accessor `ClassNamePt` de 1 linha adicionado ao plano (§4 + checklist).

### PA-01-09 · B — Edge · 🟢 ✅ Resolvido em 2026-07-03

**Perfil órfão sem o log informativo 1× pedido pela 01-spec**

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** log server-side 1× por edition órfã (edition não-vanilla ausente do registry é indistinguível de vanilla — o log cobre apenas o caso diagnóstico: `HashSet` estático de editions já logadas; nível Debug para não poluir). Nota: edition vanilla legítima ("Standard" etc.) não loga — filtro: só loga se a edition não estiver no registry E o perfil tiver sido criado com launcher do mod não é detectável; aceito log Debug genérico.

### PA-01-10 · B — Edge · 🟢 ✅ Resolvido em 2026-07-03

**Determinismo do dedup entre restarts**

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** `OrderBy(kv => kv.Key.ToString(), StringComparer.Ordinal)` antes do foreach (1 linha; ordem estável entre restarts).

### PA-01-11 · B — Edge · 🟢 ✅ Resolvido em 2026-07-03

**Popover raycast-target pode roubar o hover de outra linha (flicker)**

**Problema:** o painel do popover (Image de fundo + cards com `CardHover`) é raycast-target; se cobrir outra linha, rouba o ponteiro → `OnPointerExit` na linha → esconde → loop de flicker. Com N linhas hoveráveis (057) a chance de sobreposição cresce.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** no host do loading, após o `Build`, desabilitar `raycastTarget` de TODOS os Graphics do painel (`GetComponentsInChildren<Graphic>`) — o popover vira só-exibição (o realce `CardHover` deixa de atuar nesse host; aceitável). Adicionado ao §5.4/checklist.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Review 01 criada (agente adversarial de contexto limpo) — 11 pontos; decisões autônomas registradas; spec técnica e 01-spec atualizadas; 🔴 zerado |

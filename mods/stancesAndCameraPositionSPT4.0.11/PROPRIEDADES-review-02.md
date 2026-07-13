# stancesAndCameraPositionSPT4.0.11 — Review de Propriedades F12 · 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Fonte de verdade:** `modded/Plugin.cs` (chamadas `Config.Bind`) · **Doc espelho:** `PROPRIEDADES.md`
**Data:** 2026-07-12

> Revisão de **UX e organização** das propriedades do menu F12 (BepInEx ConfigurationManager). Cada achado recebe um ID `MP-02-MM` **permanente**. A aplicação acontece no **`Plugin.cs`** (as `Config.Bind`), não no `PROPRIEDADES.md` — este é regenerado depois.
>
> ⚠️ **Breaking change:** renomear uma **seção** ou **key** recria a `ConfigEntry` (BepInEx casa por `(seção, key)` literal) e **descarta o valor salvo** do usuário, voltando ao default. Todo achado que renomeia está marcado `⚠️ BREAKING` com estratégia de migração.
>
> **Motivação desta rodada:** validação in-game da **v2.0.0** (2026-07-12). O usuário confirmou que **ordem das seções e tooltips estão corretos**, mas relatou a impressão de que "continuamos com algumas ou várias configs fantasmas". Esta review investiga exatamente isso — e a impressão **estava certa**.

## Resumo

> 🔴 Bloqueadores: 3 · 🟠 Fortes: 2 · 🟡 Médios: 3 · 🟢 Menores: 2 · Total: 10
> Props analisadas: 120 · Seções: 21 · Props mortas: **7 confirmadas** (+3 inertes por regressão) · Breaking changes propostos: 0

**A review 01 caçou props *bindadas e nunca lidas* e removeu 23. Todas as 120 restantes são lidas** — por isso a auditoria por `grep` diz que está tudo limpo. Os fantasmas que sobraram são de um tipo que `grep` não pega: **a propriedade é lida, mas o caminho onde ela é lida nunca acontece.**

## Critérios avaliados

| # | Critério | Categoria |
|---|---|---|
| 1 | A ordem das seções no F12 faz sentido? Seções relacionadas ficam próximas? | **ORD** |
| 2 | As seções estão bem distribuídas? Os nomes são intuitivos e consistentes? | **SEC** |
| 3 | Cada propriedade está na seção temática certa? | **LOC** |
| 4 | O nome (key) representa bem o que a propriedade faz? | **NAM** |
| 5 | O tipo/faixa dá boa experiência de edição? | **TYP** |
| 6 | O tooltip explica de forma clara e intuitiva? | **TIP** |
| 7 | A propriedade **faz algo**? | **DEAD** |
| 8 | A marcação **"Advanced"** é apropriada? | **ADV** |

## Panorama

- **Props reais no código: 120** (90 binds diretos + 15 via `BindStance` × 4 stances + 15 via `BindMult` × 15 cenários de stamina). **Seções: 21.** Bate exatamente com o `PROPRIEDADES.md`.
- **Divergências código × `PROPRIEDADES.md`:** nenhuma key sobrando ou faltando; nenhum default/faixa/seção divergente. O doc está **sincronizado**. Duas ressalvas menores viram achados (MP-02-08).
- **Tooltips:** 95/95 `ConfigDescription` seguem o padrão bilíngue `"<EN>\n\n<PT>"`. **Nenhuma violação** — a impressão do usuário de que "os tooltips deram certo" está confirmada por varredura exaustiva.
- **Ordem das seções:** confere com o documentado. As seções `Stance 0 - Vanilla` e `Stance 3 - Custom` têm o primeiro bind deliberadamente antecipado no `Awake` para forçar a posição no F12 (o `Order` só ordena *dentro* da seção, nunca entre seções) — isso está correto e comentado no código.
- **Props mortas encontradas:** **7 confirmadas** — `Default Hands/Arms Positions` (4) e `Stance 1/2/3 Apply When Prone` (3). Mais **3 inertes por regressão** (`Field of View`), que não são "mortas" no sentido do bind, mas não produzem efeito.

---

## Índice

| ID | Cat | Impacto | Título | Breaking? | Status |
|---|---|---|---|---|---|
| MP-02-01 | DEAD | 🔴 | `Default Hands/Arms Positions` (4 props): o único branch que as lê é inalcançável | — | ✅ Aplicado v2.1.0 |
| MP-02-02 | DEAD | 🔴 | `Stance 1/2/3 Apply When Prone` (3 props): deitar já força a Stance 0 antes da leitura | — | ✅ Aplicado v2.1.0 |
| MP-02-03 | DEAD | 🔴 | `Field of View` (3 props): o `FOVClampPatch` virou órfão — regressão apagada por arrasto | — | ✅ Aplicado v2.1.0 |
| MP-02-04 | ADV | 🟠 | Fantasma **inverso**: 4 props do ciclo funcionam mas **somem do F12** por padrão | — | ✅ Aplicado v2.1.0 |
| MP-02-05 | DEAD | 🟠 | `Camera Position` (4 props): cache stale — pode parar de valer da 2ª raid em diante | — | Pendente |
| MP-02-06 | TYP | 🟡 | Dois valores em **segundos** exibidos como **porcentagem** (range 0–1) | — | Pendente |
| MP-02-07 | NAM | 🟡 | 29 keys misturam inglês e português no nome — e a família `Camera Position` não | ⚠️ | ✅ Aplicado v2.2.0 (traduzido) |
| MP-02-08 | SEC | 🟢 | `PROPRIEDADES.md`: 7 headers com sufixo `— Item NNN` que não existe no F12 | — | Pendente |
| MP-02-09 | DEAD | 🟢 | Código morto colateral (não gera prop fantasma, mas confunde) | — | Pendente |
| MP-02-10 | NAM | 🟡 | Comentário do código afirma que a Stance 0 é "irrelevante" — ela aplica um cap permanente | — | Pendente |

---

## Achados

### MP-02-01 · DEAD — Propriedade morta · 🔴 Bloqueador

**A seção `Default Hands/Arms Positions` inteira (4 props) não faz nada — o branch que a lê é inalcançável**

**Local:** seção `Default Hands/Arms Positions (Advanced)` · keys `Enable Default Hands/Arms Position`, `Default Forward/Backward`, `Default Up/Down`, `Default Sideways` · [`Plugin.cs:662-692`](../../modded/Plugin.cs#L662)

**Problema:** as 4 entries são lidas em `StanceManager.RebuildCachedStanceValues` (`StanceManager.cs:769-775`), que monta o campo `_cachedDefaultPosition`. Esse campo é consumido em **exatamente um lugar**: o branch default (`_ =>`) do switch em `GetTargetPosition` (`StanceManager.cs:830`), que só executa quando `stance == Stance.Default`.

Só que os **três** call-sites de `GetTargetPosition` são todos gated em "estar em uma stance":
- `ApplyComplexRotationPatch.cs:257` — `isInStance ? GetTargetPosition(...) : Vector3.zero`
- `ApplySimpleRotationPatch.cs:175` — idêntico
- `ObservedStanceAnimator.cs:37-39` — `inStance = _stance > 0`

E `isInStance ⇔ CurrentStance != Default` (`StanceManager.cs:62`). Logo o switch **sempre** cai em `Stance1/2/3` e o branch `_ =>` nunca roda. A contradição é estrutural: a propriedade descreve "a posição das mãos quando você **não** está em postura", mas o único código que a leria exige **estar** em postura.

**Por que importa (UX):** são 4 opções que o usuário pode passar meia hora calibrando sem que absolutamente nada mude na tela. É o pior tipo de propriedade — não dá erro, não avisa, só ignora.

**Sugestão:** **remover as 4 `Config.Bind`** + os 4 campos + o `_cachedDefaultPosition` e o branch `_ =>` (substituir por `Vector3.zero`). *Alternativa*, se a intenção original era boa (ajustar a pose padrão do vanilla): implementar de fato, aplicando o offset também fora de stance — mas isso é **feature nova**, vai para o backlog, não para esta review. Recomendo remover: o mod já tem `Camera Position` para ajuste fora de stance.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (remover as 4 props)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### MP-02-02 · DEAD — Propriedade morta · 🔴 Bloqueador

**`Stance 1/2/3 Apply When Prone` (3 props): ao deitar, o mod já forçou a Stance 0 — a config lida é sempre a da Stance 0**

**Local:** seções `Stance 1 - High Ready`, `Stance 2 - Low Ready`, `Stance 3 - Custom` · key `Stance N Apply When Prone` · [`Plugin.cs:1449`](../../modded/Plugin.cs#L1449) (via `BindStance`)

**Problema:** a prop é lida em dois pontos, e **ambos** usam o `cfg` da **stance ativa** (`_stanceConfigs[_activeStaminaStance]`, que espelha `CurrentStance`):
- `ApplyStaminaStance` — `StanceManager.cs:1248`: `IsSuspendedByProne = inProne && !cfg.ApplyWhenProne.Value`
- `EvaluateProneSuspensionTick` — `StanceManager.cs:1345`: `isSuspended = player.IsInPronePose && !cfg.ApplyWhenProne.Value`

Mas `StanceManager.Update()` (`StanceManager.cs:165-176`) **força `SetStance(Stance.Default)` sempre que `IsInPronePose`**, e roda **antes** dessas leituras na ordem do `Plugin.Update` (`:1414-1419`). Conclusão: `prone == true` ⇒ stance ativa == `Default` ⇒ o `cfg` consultado é **sempre o da Stance 0**. Para as Stances 1/2/3, as duas expressões viram `false && ...` — o valor da prop é irrelevante.

**Consequência importante:** **`Stance 0 Apply When Prone` é a única das quatro que funciona** — é ela que realmente decide se o cap de velocidade continua valendo ao deitar. As outras três são decoração.

**Por que importa (UX):** o usuário vê a mesma opção repetida em 4 seções e supõe que cada uma controla a sua stance. Três delas mentem.

**Sugestão:** remover o bind de `ApplyWhenProne` para `Stance1/2/3` dentro de `BindStance` (mantendo **apenas** para `Stance.Default`), e renomear a que sobra para algo que diga o que ela faz de fato — sugestão: mover para a seção `Movement & Inertia` com a key `Keep Speed Cap When Prone`. ⚠️ Esse rename é BREAKING (a prop volta ao default `false`, que é o comportamento atual — impacto nulo na prática, mas entra no changelog).
*Alternativa conservadora (sem breaking):* remover só as 3 mortas e deixar `Stance 0 Apply When Prone` onde está.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (remover 3 + renomear/mover a da Stance 0)
- `[ ]` Aceitar com modificação (só remover as 3 mortas, sem breaking)
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### MP-02-03 · DEAD — Regressão · 🔴 Bloqueador

**`Field of View`: o patch que faz a feature funcionar nunca é habilitado — foi apagado por arrasto num commit de mounting**

**Local:** seção `Field of View` · keys `Enable Expanded FOV Range`, `Minimum FOV`, `Maximum FOV` · [`Plugin.cs:1144-1168`](../../modded/Plugin.cs#L1144) · patch órfão: [`Patches/FOVClampPatch.cs`](../../modded/Patches/FOVClampPatch.cs)

**Problema:** o mod tem duas peças de FOV:
1. **`FOVSliderPatch`** — **ativo** (`Plugin.cs:254`). Só alarga o **slider da UI** de settings (`BindNumberSliderToSetting`).
2. **`FOVClampPatch`** — **órfão**. Seu docstring diz literalmente *"This allows FOV values outside the default 50-75 range to be applied"*, e ele faz Postfix no clamp interno (`GClass1085.Class1841.method_0`). **Nunca é habilitado.**

O mod habilita 35 patches (33 via `SafeEnable` + 2 diretos). O `FOVClampPatch` **não está entre eles** — e `git log -S` prova que o `.Enable()` dele **existia** no commit inicial (`c078925`) e sumiu no commit `9816946` ("implement weapon collision/mounting..."), que não tinha nada a ver com FOV. Foi deleção acidental.

Resultado: o slider da UI aceita valores maiores, mas o clamp interno do jogo continua ativo — o valor é limitado de volta. A feature está **inerte**.

**Por que importa (UX):** três opções que prometem exatamente o que não entregam. E, diferente das outras, esta **já funcionou** — é regressão, não design ruim.

**Sugestão:** reabilitar o patch: adicionar `SafeEnable("FOVClampPatch", () => new FOVClampPatch());` junto aos demais em `Plugin.cs:254`, **gated** pela prop `Enable Expanded FOV Range` (que hoje é `false` por default — manter). ⚠️ **Validar in-game**: alterar o FOV além de 75 e confirmar que o valor persiste. Se `GClass1085.Class1841.method_0` não existir mais em 0.16.x, o `SafeEnable` já loga a falha sem derrubar o mod — nesse caso a decisão passa a ser *remover as 3 props* em vez de reabilitar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (reabilitar o patch + validar)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (remover as 3 props de FOV): _________________

---

### MP-02-04 · ADV — Fantasma inverso · 🟠 Forte

**As 4 props que governam o ciclo da tecla `V` desaparecem do F12 justamente na configuração padrão**

**Local:** seção `Stance Cycle & Hotkeys` · keys `Include Stance 0 - Vanilla in Cycle`, `Enable Stance 1 - High Ready in Cycle`, `Enable Stance 2 - Low Ready in Cycle`, `Enable Stance 3 - Custom in Cycle` · [`Plugin.cs:405-441`](../../modded/Plugin.cs#L405) · lógica: [`Plugin.cs:1288-1297`](../../modded/Plugin.cs#L1288)

**Problema:** `RefreshScrollModeVisibility` faz `Browsable = wheelEnabled && mode == Cycle`. Ou seja, as 4 props só aparecem no F12 quando a roda do mouse está ligada **e** em modo `Cycle`. Mas elas **também governam o ciclo da tecla `V`** (`IsStanceEnabled` → `GetNextStance`, `StanceManager.cs:627,677-680`) — e o próprio tooltip admite isso: *"Always affects the V key cycle; affects mouse scroll only when Mouse Wheel Scroll Mode = Cycle"*.

**Isto é visível nos seus prints:** seu `Mouse Wheel Scroll Mode` está em **`Linear`**, então as 4 sumiram da seção `Stance Cycle & Hotkeys` — exatamente o que se vê na captura. Elas continuam ativas, decidindo quais stances o `V` percorre, e você não tem como editá-las pela interface.

**Por que importa (UX):** é o inverso do fantasma e é pior: a prop **faz** algo e **não aparece**. Para desativar uma stance do ciclo do `V`, hoje o usuário precisa editar o `.cfg` na mão ou trocar o scroll mode para `Cycle` só para revelar as opções.

**Sugestão:** trocar o gate de visibilidade para `Browsable = true` (as 4 sempre visíveis), já que afetam o `V` em qualquer modo. Se a intenção era só reduzir ruído visual, o correto seria condicionar **apenas** as props exclusivas da roda (`Mouse Wheel Modifier Key`), nunca as do ciclo. Sem breaking (não muda `(seção, key)`).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (sempre visíveis)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-02-05 · DEAD — Cache stale · 🟠 Forte

**`Camera Position`: o offset pode deixar de ser aplicado a partir da 2ª raid da sessão**

**Local:** seção `Camera Position` · keys `Enable Camera Position`, `Forward/Backward Offset`, `Up/Down Offset`, `Sideways Offset` · [`Plugin.cs:366-394`](../../modded/Plugin.cs#L366) · lógica: [`Plugin.cs:1519-1547`](../../modded/Plugin.cs#L1519)

**Problema:** `UpdateCameraOffset` começa com `if (!_cameraOffsetDirty) return` (`:1522`) e zera a flag ao fim (`:1546`). E `MarkCameraOffsetDirty()` (`:1407`) é chamado **somente** pelos 4 handlers de `SettingChanged` (`:1214-1217`) — **nunca** no início de raid nem no `ResetState`. O `_cameraOffsetDirty = true` inicial (`:1401`) é `static`, então vale **uma vez por processo**.

Consequência: na 2ª raid da sessão, o `HandsContainer` é novo (nasce com o default do jogo) e o mod não reescreve o offset, porque a flag já foi consumida na 1ª raid.

**Ressalva honesta (por isso 🟠 e não 🔴):** existe um segundo consumidor — `PlayerSpringPatch` (Postfix de `PlayerSpring.Start`, roda a cada raid) que escreve `PlayerSpring.CameraOffset`. **Se** `PlayerSpring.CameraOffset` e `HandsContainer.CameraOffset` forem o mesmo storage, esse patch salva o recurso e as props estão vivas. Não consegui provar: nenhum dos dois tipos está no dump de `references/eft-decompiled/`.

**Por que importa (UX):** se a suspeita se confirmar, o offset de câmera "some" depois da primeira raid — um bug intermitente, do tipo que o usuário sente mas não consegue reproduzir de propósito.

**Sugestão:** independentemente da dúvida, **chamar `MarkCameraOffsetDirty()` no início de raid** (no `GameWorldOnGameStartedPatch` / `ResetState`). É uma linha, é inofensivo se a prop já estiver viva, e elimina a classe inteira de bug. **Verificação in-game** que decide a questão: entrar em uma raid, sair, entrar em outra e conferir se o offset de câmera ainda vale.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (marcar dirty no raid start)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-02-06 · TYP — Faixa incoerente · 🟡 Médio

**Duas opções medidas em segundos são exibidas como porcentagem**

**Local:** `Stance Transition & Kick` · `ADS Kick Delay (In)` ([`Plugin.cs:500`](../../modded/Plugin.cs#L500)) e `Tac Sprint Settings (Advanced)` · `Tac Sprint Reset Delay` ([`Plugin.cs:1133`](../../modded/Plugin.cs#L1133))

**Problema:** ambas são `float` em **segundos** com `AcceptableValueRange<float>(0f, 1f)`. O ConfigurationManager renderiza todo float cuja faixa é exatamente 0–1 como **porcentagem, sem caixa de valor** — daí o `15%` e o `35%` que aparecem nos prints (defaults 0,15 s e 0,35 s). O tooltip diz "segundos"; a interface diz "%".

(`Passive Sway Multiplier`, também 0–1, aparece como `65%` — mas ali é um multiplicador, e a leitura percentual é legítima. Não é achado.)

**Por que importa (UX):** quem calibra pela interface acha que está mexendo numa proporção, e não em tempo. E, por não ter caixa de texto, não dá para digitar um valor exato.

**Sugestão:** alargar a faixa para `(0f, 2f)` nas duas — o CM volta a mostrar slider + caixa de valor em segundos, e de quebra permite atrasos acima de 1 s. Não é breaking (a key não muda; valores salvos continuam válidos).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-02-07 · NAM — Idioma inconsistente · 🟡 Médio · ⚠️ BREAKING

**29 keys misturam inglês e português no nome — e a família `Camera Position`, que é a mesma coisa, não mistura**

**Local:** todas as props de rotação/posição de mãos — `ADS Default Values` (6), `Default Hands/Arms` (3), `Stance 1/2/3` (18) + `Stance Kick Intensity (Contra o Peito)` e `Stance Overshoot Damping (Menos gera Mais Quicada)`. Ex.: [`Plugin.cs:720`](../../modded/Plugin.cs#L720) — `Stance 1 Pitch (Cano Sobe/Desce)`.

**Problema:** a convenção adotada na review 01 foi **nome em inglês, tooltip bilíngue**. Essas 29 keys carregam a tradução dentro do próprio nome. A inconsistência fica evidente na comparação: `Stance 1 Forward/Backward (Frente/Trás)` × `Forward/Backward Offset` (seção `Camera Position`) — mesma grandeza, duas convenções.

**Por que importa (UX):** é ambíguo dos dois lados. Vale reconhecer, porém, que os sufixos são **didáticos** — `(Cano Sobe/Desce)` explica o eixo melhor que "Pitch", e foi por causa desse tipo de rótulo que os eixos trocados (Roll/Yaw) foram detectados na review 01.

**Sugestão:** **manter como está** e tratar como decisão consciente, **documentando a exceção** no `PROPRIEDADES.md` ("keys de eixo levam a dica em português entre parênteses; demais keys em inglês"). Padronizar custaria um rename de 29 keys — **breaking**, resetando a calibração fina de todas as stances — em troca de pureza estética. **Não recomendo.** Se for padronizar, o caminho barato é o oposto: *acrescentar* o sufixo pt às 3 keys de `Camera Position`, que aí ficam iguais às outras 29 (breaking de apenas 3 keys, de valor tipicamente default).

**Decisão:**
- `[x]` **Aceitar com modificação — TRADUZIR os sufixos para inglês** (decisão do usuário, 2026-07-12).
  Nem "manter em PT" (sugestão original) nem "remover": os sufixos **existem para tornar o eixo óbvio**, já que
  `Pitch`/`Yaw`/`Roll` são jargão — o valor deles é didático, e some se forem removidos. Foram traduzidos:
  `(Cano Sobe/Desce)` → `(Muzzle Up/Down)` · `(Apontar Esq/Dir)` → `(Point Left/Right)` ·
  `(Tombar Arma)` → `(Cant Weapon)` · `(Coronha Sobe/Desce)` → `(Stock Up/Down)` ·
  `(Coronha Esq/Dir)` → `(Stock Left/Right)` · `(Contra o Peito)` → `(Toward the Chest)` ·
  `(Menos gera Mais Quicada)` → `(Lower = More Bounce)`. `(Frente/Trás)` foi removido (o nome em inglês,
  `Forward/Backward`, já dizia o mesmo). Config migrada em vez de resetada — ver `MP-02-11`.

**Aplicação:** `Plugin.cs` (keys) + `PROPRIEDADES.md` · v2.2.0, commit `d9069fb`.

---

### MP-02-08 · SEC — Doc × jogo · 🟢 Menor

**O `PROPRIEDADES.md` decora 7 nomes de seção com um sufixo que não existe no F12**

**Local:** [`PROPRIEDADES.md`](../PROPRIEDADES.md) linhas 156, 162, 172, 199, 217, 228, 236

**Problema:** headers como `Weapon Mount (Active) — Item 015` ou `Stamina Management — Item 012`. A seção real no jogo é `Weapon Mount (Active)`, sem sufixo. Quem usar a busca do F12 com o nome do doc não acha.

**Por que importa (UX):** o doc se apresenta como espelho fiel do F12; a rastreabilidade para o item de backlog é útil, mas não pode se disfarçar de nome de seção.

**Sugestão:** mover a referência para fora do header — ex.: `### Weapon Mount (Active)` e, na linha seguinte, `> Origem: item 015.` Aplicar na próxima regeneração do doc.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir): _________________

---

### MP-02-09 · DEAD — Código morto colateral · 🟢 Menor

**Quatro trechos mortos que não criam props fantasmas, mas fazem qualquer auditoria futura tropeçar**

**Local:** `modded/` — vários

**Problema:**
- **`CameraBobbingScript.cs`** — `MonoBehaviour` **nunca instanciado** (nenhum `AddComponent` no mod). Arquivo inteiro morto.
- **`PlayerSpringPatch._cameraOffsetField`** (`:13`, `:18`) — resolvido por reflection e **nunca usado**. (Some junto com a reflection por frame da **P-7.2**.)
- **`Plugin.FixedUpdate()`** (`:1394`) — vazio; o Unity paga o custo da chamada.
- **`ApplySimpleRotationPatch`** — está habilitado, mas o caminho de 3ª pessoa não aplica transformação de câmera; e ele **hardcoda `damping = 12f`** (`:180`), ignorando a prop `Stance Overshoot Damping`. Se um dia esse caminho voltar a rodar, a prop será silenciosamente ignorada ali.

**Por que importa:** foi exatamente esse tipo de resíduo que produziu as 23 props mortas da review 01. Limpar agora evita a próxima safra.

**Sugestão:** remover `CameraBobbingScript.cs`, `_cameraOffsetField` e o `FixedUpdate` vazio. Para o `ApplySimpleRotationPatch`, trocar o `12f` hardcoded por `_StanceOvershootDamping.Value` (1 linha) — barato e remove a bomba-relógio. Encaixa naturalmente na **P-7.2**.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Rejeitar (deferir para a P-7.2): _________________

---

### MP-02-10 · NAM — Comentário enganoso · 🟡 Médio

**O código afirma que a Stance 0 é "irrelevante" — e ela aplica um cap permanente de 90% na sua velocidade**

**Local:** [`Plugin.cs:47`](../../modded/Plugin.cs#L47) — comentário `// Stance 0: irrelevante` · efeito real: `StanceManager.cs:1250-1255` e `:1361-1372`

**Problema:** a seção `Stance 0 - Vanilla` existe e **funciona**. Com os defaults (`Stance 0 Modifies Movement Speed = true`, `Stance 0 Movement Speed Multiplier = 90`), o mod aplica um **limite de 90% da velocidade no estado "sem stance"** — isto é, **na maior parte do tempo de jogo** — via `mc.AddStateSpeedLimit`. Isso ainda **compõe** com `Walk Speed Multiplier` (default 0,85), que patcheia o getter `MovementContext.MaxSpeed`. O comentário no código diz o contrário.

**Por que importa (UX):** este é o achado com **maior impacto no jogo** de toda a review, e o mais fácil de passar despercebido — quem lê o código conclui que a Stance 0 não faz nada e não pensa duas vezes ao mexer nos defaults. E responde a pergunta em aberto do **MP-01-10** ("a seção da Stance 0 se justifica?"): **sim, e ela é a mais impactante das quatro.**

**Sugestão:** (a) corrigir o comentário `Plugin.cs:47` para descrever o efeito real; (b) reforçar no tooltip de `Stance 0 Movement Speed Multiplier` que ele vale **fora de qualquer postura** e **multiplica com** `Walk Speed Multiplier`; (c) **confirmar in-game** que o cap de 90% + walk 0,85 é o comportamento desejado, e não uma lentidão acidental herdada.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-02-11 · NAM — Eixo enganoso · 🔴 Bloqueador · ✅ Aplicado em 2026-07-12 (v2.2.0)

**`Yaw` e `Roll` faziam a coisa um do outro — em todas as stances e no ADS**

**Local:** `Stance 1/2/3` e `ADS Default Values` · keys `... Yaw` e `... Roll` · montagem: [`StanceManager.cs:720-760`](../../modded/StanceManager.cs#L720)

**Como apareceu:** **reportado pelo usuário jogando** (2026-07-12) — "o Yaw está tombando a arma e o Roll está
movendo para esq/dir". Nenhuma das duas reviews de propriedades pegou: ambas conferiram **rótulo × nome do campo**,
e esses batiam. O que não batia era o **campo × eixo físico**.

**Causa raiz:** a rotação é aplicada como `weapRotation * Quaternion.Euler(euler)`
([`ApplyComplexRotationPatch.cs:280`](../../modded/Patches/ApplyComplexRotationPatch.cs#L280)) — ou seja, no espaço
**local da arma**, não no do mundo. Nesse espaço (como os próprios comentários de *posição* já registravam):
`X = lateral · Y = LONGITUDINAL (o cano) · Z = vertical`. Portanto girar em torno de **Y tomba** (roll) e em torno
de **Z aponta** (yaw). O código montava `new Vector3(pitch, yaw, roll)` — a ordem canônica do **Unity** —, que joga
o yaw no eixo do cano e o roll no eixo vertical.

**Agravante — é uma regressão nossa.** O commit `261c069` (**MP-01-02**, review 01) presumiu a convenção do Unity e
"corrigiu" trocando os **rótulos**. Os rótulos estavam certos; o **mapeamento** é que estava errado. A troca de
rótulos inverteu os dois eixos para o usuário e mascarou a causa real por mais um ciclo.

**Correção:** consertado na origem — `Y` recebe o **roll** e `Z` o **yaw**, nos 4 pontos de montagem. Rótulo, nome
de campo e efeito físico passam a concordar. A config do usuário foi **migrada** (valores de `Yaw`↔`Roll` trocados)
para que as poses continuem idênticas in-game; backup em `cfg.bak-pre-v220`.

**Lição:** validar propriedade de rotação **contra o eixo físico**, nunca contra a convenção da engine — em espaço
local de osso/arma os eixos não são os do mundo. E: **quando o rótulo e o efeito divergem, suspeitar do mapeamento
antes de renomear o rótulo.** Renomear é o conserto que parece mais barato e é o que esconde o bug.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-12 | **v2.1.0** (`ca9f868`): aplicados MP-02-01 (4 props mortas), MP-02-02 (3 props mortas), MP-02-03 (FOVClampPatch reabilitado), MP-02-04 (toggles de ciclo sempre visíveis). F12: 120 → 113 props. |
| 2026-07-12 | **v2.2.0** (`d9069fb`): aplicado MP-02-07 — sufixos das keys **traduzidos** para inglês (não removidos). Junto, corrigido o **`MP-02-11`** (abaixo), reportado pelo usuário in-game. |
| 2026-07-12 | Review de propriedades 02 criada via `/review-mod-properties`, após a validação in-game da v2.0.0. 10 achados: 3 🔴 (7 props mortas + 3 inertes por regressão), 2 🟠, 3 🟡, 2 🟢. Confirmado que ordem das seções e tooltips (95/95 bilíngues) estão corretos, e que o `PROPRIEDADES.md` está sincronizado com o código. |

# Code Review — stancesAndCameraPositionSPT4.0.11 · v2.0.0 → v2.2.1

**Mod:** stancesAndCameraPositionSPT4.0.11
**Escopo:** o diff de código desta sessão — `e886857` (baseline, pré-2.0.0) → `4936e8f` (v2.2.1).
Arquivos: `modded/Plugin.cs` (149 linhas), `modded/StanceManager.cs` (61), `modded/CameraRotationMod.csproj`, `modded/CHANGELOG.md`.
**Data:** 2026-07-13

> Revisão pedida pelo usuário **antes** de mexer na ordem de inicialização do `Awake`. Motivo: nesta mesma sessão
> um `Config.Bind` com `=` no nome da key abortou o `Awake` e produziu `NullReferenceException` infinito em raid
> (corrigido na v2.2.1). A pergunta que esta review responde é: **o que mais desta natureza está no código?**
>
> Método: 3 lentes independentes (correção/regressão · ciclo de vida BepInEx · semântica dos eixos), com cada
> achado verificado no código antes de entrar aqui.

## Resumo

> 🔴 Bloqueadores: 2 · 🟠 Fortes: 2 · 🟡 Médios: 3 · 🟢 Menores: 2 · Total: 9

**A causa do incidente de hoje continua viva no código.** O `=` foi só o gatilho; a **arma** é a ordem do `Awake`
(`CR-01`). E há um segundo bloqueador que não é código: **o changelog promete uma migração automática de config
que não existe** (`CR-02`) — o que vai apagar silenciosamente a calibração dos outros jogadores do servidor.

### Verificado e correto (não viraram achados)

- **Sentinel `null` do `ApplyWhenProne`** — os **3** consumidores tratam null (`Plugin.cs:1168` com `!= null`;
  `StanceManager.cs:1258` e `:1356` com `?.Value ?? false`). Não há um 4º uso. Sem NRE.
- **Remoção da seção `Default Hands/Arms`** — zero referências penduradas; comportamento fora de stance inalterado
  (o branch removido já devolvia `Vector3.zero` na prática).
- **Troca dos eixos Yaw/Roll** — exatamente **4** pontos de montagem, todos migrados; nenhum outro lugar monta euler
  de stance. **Corroboração independente:** `PassiveMountDetectPatch.cs:21-23` usa `_down=(0,0,-0.19)` e
  `_left/_right=(±0.143,0,0)` — nesse espaço local Z=vertical e X=lateral, logo **Y=cano ⇒ Y=roll, Z=yaw**. Confirma
  o fix por um caminho totalmente diferente.
- **Keys/seções** — as 90 `Config.Bind` e todas as constantes de seção estão livres dos caracteres proibidos pelo
  BepInEx, e não há par `(seção, key)` duplicado.

---

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01 | A | 🔴 | Os patches são habilitados ~900 linhas ANTES dos binds — qualquer bind que lance repete o incidente | Pendente |
| CR-02 | A | 🔴 | O CHANGELOG promete uma migração de `.cfg` que **não existe no código** | Pendente |
| CR-03 | A | 🟠 | Dois patches usam `.Enable()` cru, fora do `SafeEnable` — e o comentário acima diz o contrário | Pendente |
| CR-04 | B | 🟠 | `FOVClampPatch` (que acabei de reabilitar) lê config sem null-guard | Pendente |
| CR-05 | E | 🟡 | `RefreshScrollModeVisibility` virou no-op, mas ainda dispara rebuild do ConfigurationManager | Pendente |
| CR-06 | B | 🟡 | `StaminaController.Multipliers` é um array de tamanho fixo `[16]` indexado por enum | Pendente |
| CR-07 | D | 🟡 | `Plugin.Update` não tem try/catch — uma exceção derruba o tick inteiro, todo frame | Pendente |
| CR-08 | E | 🟢 | `ApplySimpleRotationPatch` hardcoda `damping = 12f`, ignorando a propriedade | Pendente |
| CR-09 | E | 🟢 | A memória registra o nome de key envenenado (`Lower = More Bounce`) | Pendente |

---

## Achados

### CR-01 · A — Crítico · 🔴 Bloqueador

**A causa-raiz do incidente de hoje continua no código: os patches ligam antes dos binds**

**Local:** [`Plugin.cs:241-330`](./modded/Plugin.cs#L241) (habilitação dos patches) × [`Plugin.cs:335-1234`](./modded/Plugin.cs#L335) (os ~120 `Config.Bind`)

**Problema:** o `Awake` habilita **~35 patches Harmony** e só **depois** binda as `ConfigEntry`. Se **qualquer**
bind lançar, o `Awake` aborta — e os patches ficam **ativos** com todas as `ConfigEntry` posteriores em `null`.
Foi exatamente isso hoje: a key com `=` abortou na linha ~508 e os patches do mount passivo passaram a fazer
`Plugin._EnablePassiveMount.Value` (null) a cada frame.

O diff desta sessão **piorou** o quadro: acrescentou mais um patch a esse bloco pré-bind (o `FOVClampPatch`,
`Plugin.cs:253`). E a superfície é grande: há **53 leituras `Plugin._X.Value` sem `?.` em 18 arquivos** de patch.

Pior: com o `Awake` abortado, `StanceManager.Initialize` (`:1184`) nunca roda, mas o Unity **continua chamando**
`Plugin.Update` (`:1380`) → `StanceManager.cs:196` faz `_stanceToggleKeyConfig.Value` sem guard → NRE por frame.
E `PassiveMountUI.Update` (`PassiveMountUI.cs:53`) não tem nem null-guard nem try/catch.

**Por que importa:** transforma qualquer erro de digitação numa config em **spam infinito de exceção dentro da
raid** — o pior sintoma possível, porque parece que o mod inteiro apodreceu. O usuário teve de desabilitar o mod
no meio da sessão de jogo.

**Sugestão (3 partes, nesta ordem):**
1. **Reordenar:** extrair os binds para `BindAllConfig()` (335-1234, **sem mover nenhum `Config.Bind` de lugar
   entre si** — a ordem deles é a ordem das seções no F12) e o bloco de ativação para `EnableEverything()`
   (241-330). O `Awake` passa a ser: resolver reflection → **bindar** → **só então ativar os patches**.
   ⚠️ Manter `ResolveFirearmControllerSetTrigger()` (`:236`) **antes** da ativação — o `SnapFireTriggerPatch`
   depende dele no `GetTargetMethod` (`SnapFireTriggerPatch.cs:47`). É a **única** ordem obrigatória.
2. **`try/catch` em volta dos binds:** no catch, logar `[BOOT]` com a exceção, marcar `ConfigReady = false`,
   `enabled = false` e **retornar sem ativar patch nenhum** → o mod fica inerte e o jogo roda vanilla, em vez de
   inundar o log.
3. **Kill-switch `public static bool ConfigReady`:** checado no topo de `Plugin.Update` e de
   `PassiveMountUI.Update` — são os dois `MonoBehaviour` que o Unity chama mesmo com o `Awake` abortado.

**Segurança da reordenação (verificada):** nenhum `Config.Bind` depende de patch ativo; nenhum `GetTargetMethod`
lê `ConfigEntry`; `InitFikaSync` só assina um evento; e **nenhum patch perde evento** — o `Awake` inteiro é uma
chamada síncrona no chainloader do BepInEx, antes da cena do jogo carregar. O alvo mais cedo de todos
(`LocaleClassReloadPatch`) só dispara no login. Janela de risco: **zero frames**.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

### CR-02 · A — Crítico · 🔴 Bloqueador

**O CHANGELOG promete uma migração automática de `.cfg` que não existe no código**

**Local:** [`modded/CHANGELOG.md:23-26`](./modded/CHANGELOG.md#L23)

**Problema:** o changelog da v2.2.0 afirma:

> *"Sua calibração foi preservada. Se você já tinha ajustado as posturas, os valores foram **migrados
> automaticamente** (o que estava em `Yaw` foi para `Roll` e vice-versa) ... Um backup do arquivo antigo ficou
> como `com.shwng.fpscamerastances.cfg.bak-pre-v220`."*

**Não existe nenhum código de migração no mod.** Nenhuma leitura/reescrita de `.cfg`, nenhum `.bak`. A migração
que aconteceu foi um **script manual, rodado uma vez, na máquina deste desenvolvedor**.

**Por que importa (e este é um mod de servidor coop Fika):** qualquer outro jogador que atualizar o client vê as
**27 keys renomeadas** virarem órfãs. O BepInEx recria as novas com os **defaults** → **toda a calibração de
rotação/posição dele é resetada silenciosamente**, nenhum `.bak` aparece, e o changelog garante que está tudo
preservado. Ele não tem nem como saber o que perdeu, nem como reconstruir. É um documento que mente para o
usuário — o pior tipo de bug, porque nenhum teste pega.

**Sugestão — escolher uma:**
- **(a) Migração de verdade:** implementar no `Awake`, **antes** dos `Config.Bind`: ler o `.cfg` cru, salvar
  `.bak-pre-v220`, renomear as keys antigas → novas trocando os valores `Yaw`↔`Roll`. É a única forma de a
  promessa do changelog ser verdadeira para terceiros.
- **(b) Corrigir o changelog** (mais barato e honesto): dizer que a calibração **é resetada**, explicar o porquê,
  e publicar a tabela de conversão manual (o valor que estava em `Yaw` vai para `Roll` e vice-versa), deixando
  claro que o `.bak` só existe para quem rodou a migração local.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar (a) · `[ ]` Aceitar (b) · `[ ]` Rejeitar: _______

---

### CR-03 · A — Crítico · 🟠 Forte

**Dois patches usam `.Enable()` cru, fora do `SafeEnable` — e o comentário logo acima afirma o contrário**

**Local:** [`Plugin.cs:243-244`](./modded/Plugin.cs#L243)

```csharp
new CameraRotationMod.Patches.ApplyComplexRotationPatch().Enable();
new CameraRotationMod.Patches.HoldBreathPatch().Enable();
```

**Problema:** o comentário acima (`:238-240`) diz que **cada** patch é isolado em try/catch via `SafeEnable`. É
**falso para estes dois** — e o `ApplyComplexRotationPatch` é justamente o patch central do mod (aplica a
rotação da stance).

**Por que importa:** se a BSG renomear o alvo num update do EFT, o `Enable()` lança → **o `Awake` aborta** → é a
mesma cascata do incidente de hoje, por outra porta. E o comentário dá falsa segurança a quem revisar.

**Sugestão:** `SafeEnable("ApplyComplexRotationPatch", () => new ApplyComplexRotationPatch());` e idem para o
`HoldBreathPatch`. (Com o `CR-01` aplicado, o dano de uma falha aqui já cai muito — mas o mod ficaria sem sua
função principal, então o log explícito de `[enable] FAIL` importa.)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

### CR-04 · B — Bug latente · 🟠 Forte

**O `FOVClampPatch` (reabilitado nesta sessão) lê `ConfigEntry` sem null-guard**

**Local:** [`Patches/FOVClampPatch.cs:23,27`](./modded/Patches/FOVClampPatch.cs#L23) · ativado em [`Plugin.cs:253`](./modded/Plugin.cs#L253)

```csharp
if (!Plugin._FOVExpandEnabled.Value) return;                                   // :23
__result = Mathf.Clamp(x, Plugin._FOVMinRange.Value, Plugin._FOVMaxRange.Value); // :27
```

**Problema:** o patch é ativado na linha 253, mas `_FOVExpandEnabled` / `_FOVMinRange` / `_FOVMaxRange` só são
bindados na ~1115-1131. **Uma janela de ~860 linhas** em que o patch está ativo com config nula. Reabilitar esse
patch (fiz isso na v2.1.0) **aumentou** a superfície do problema do `CR-01`. O mesmo padrão existe no
`FOVSliderPatch.cs:23,30,31` (pré-existente).

*Sobre "e se o tipo sumir na 0.16.x": o `SafeEnable` cobre — `TypeLoadException`/erro do Harmony saem no `Enable()`
e viram `[enable] FAIL` no log, sem derrubar o mod. O que ele **não** cobre é o null-deref acima.*

**Sugestão:** `if (!(Plugin._FOVExpandEnabled?.Value ?? false)) return;` e
`Mathf.Clamp(x, Plugin._FOVMinRange?.Value ?? 50, Plugin._FOVMaxRange?.Value ?? 75)`. Com o `CR-01` aplicado isso
vira cinto-e-suspensório — mas é 1 linha e o patch é meu, desta sessão.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

### CR-05 · E — Legibilidade/manutenção · 🟡 Médio

**`RefreshScrollModeVisibility` virou no-op, mas ainda dispara um rebuild do ConfigurationManager**

**Local:** [`Plugin.cs:1257-1273`](./modded/Plugin.cs#L1257)

**Problema:** na v2.1.0 (MP-02-04) troquei o gate por `Browsable = true` incondicional. Como **nada** no mod seta
`Browsable = false`, e `null` já significa "visível", as 4 atribuições são **inertes**. O que sobrou vivo é o
`_cmBuildSettingListMethod.Invoke(...)` (`:1271`): toda mudança em `Enable Mouse Wheel Stance Cycle` ou
`Mouse Wheel Scroll Mode` continua forçando um **rebuild completo da lista do F12** — para nada, e justamente
enquanto o usuário mexe naquele dropdown (pode resetar scroll/foco da tela).

Ficou morta junto toda a máquina do F2: os 4 campos `_attr*`, `TryResolveConfigurationManager`, `_cmInstance`,
`_cmBuildSettingListMethod`, `_cmRefreshAvailable`, `OnScrollModeSettingChanged` e os 2 unsubscribes do
`OnDestroy`. O XML-doc (`:1253-1256`) ainda descreve o comportamento antigo — **está mentindo**.

**Sugestão:** remover `RefreshScrollModeVisibility`, `OnScrollModeSettingChanged`, as 2 assinaturas, os unsubs, os
4 campos `_attr*` (passando `new ConfigurationManagerAttributes { Order = N }` inline nos binds) e o aparato
`TryResolveConfigurationManager`/`_cm*` — a menos que haja plano de voltar a usar `Browsable` em breve.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (deferir): _______

---

### CR-06 · B — Bug latente · 🟡 Médio

**`StaminaController.Multipliers` é um array de tamanho fixo `[16]` indexado por enum — e é populado no `Awake`**

**Local:** [`StaminaController.cs:41`](./modded/StaminaController.cs#L41) · uso em [`Plugin.cs:1453-1467`](./modded/Plugin.cs#L1453)

**Problema:** `new ConfigEntry<float>[16]`, indexado por `(int)StaminaScenario`. Se alguém adicionar um 17º
cenário ao enum, o `BindStaminaManagement()` estoura `IndexOutOfRangeException` — **no meio do `Awake`**, ou seja,
exatamente a classe de incidente que acabamos de viver.

**Sugestão:** `new ConfigEntry<float>[Enum.GetValues(typeof(StaminaScenario)).Length]`. Uma linha, e a bomba
some.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

### CR-07 · D — Arquitetura · 🟡 Médio

**`Plugin.Update` não tem try/catch — uma exceção derruba o resto do tick, todo frame**

**Local:** [`Plugin.cs:1380-1391`](./modded/Plugin.cs#L1380)

**Problema:** o `Update` chama, em sequência, `StanceManager.Update()`, `StaminaController.Tick()` e
`UpdateCameraOffset()`. Uma exceção no primeiro impede os outros **em todo frame**, silenciosamente (o sintoma
aparece como "a stamina parou de funcionar", sem ninguém suspeitar da câmera).

**Sugestão:** try/catch por bloco, com log **rate-limited** (logar 1×, depois a cada N segundos) — sem
rate-limit, vira o mesmo spam que acabamos de sofrer.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar (deferir p/ P-7.2): _______

---

### CR-08 · E — Legibilidade/manutenção · 🟢 Menor

**`ApplySimpleRotationPatch` hardcoda `damping = 12f` e ignora a propriedade do F12**

**Local:** [`Patches/ApplySimpleRotationPatch.cs:180`](./modded/Patches/ApplySimpleRotationPatch.cs#L180)

**Problema:** enquanto o `ApplyComplexRotationPatch:262` lê `Plugin._StanceOvershootDamping?.Value ?? 12f`, o
"simples" fixa `12f`. Se esse caminho voltar a rodar, o slider `Stance Overshoot Damping` será silenciosamente
ignorado ali. Já era o `MP-02-09` da review de propriedades; o diff desta sessão **renomeou essa key**, então
vale fechar junto.

**Sugestão:** trocar por `Plugin._StanceOvershootDamping?.Value ?? 12f`.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

### CR-09 · E — Legibilidade/manutenção · 🟢 Menor

**A memória registra o nome de key envenenado — quem reaplicar a partir dela reintroduz o crash**

**Local:** [`memory/sessions.md:417`](./memory/sessions.md#L417)

**Problema:** a entrada da Sessão 9 documenta a tradução como
`` `(Menos gera Mais Quicada)` → `(Lower = More Bounce)` `` — o nome **com o `=`**, que é justamente o que derrubou
o `Awake`. A memória é lida por sessões futuras como fonte de verdade.

**Sugestão:** corrigir a linha para `(Lower Means More Bounce)` e **anotar ali o porquê** (o BepInEx proíbe `=` em
nome de key) — a memória é o lugar certo para essa lição não se perder.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Rejeitar: _______

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-13 | Code review criada a pedido do usuário, antes de reordenar o `Awake`. Escopo: diff `e886857`→`4936e8f` (v2.0.0→v2.2.1). 9 achados: 2 🔴, 2 🟠, 3 🟡, 2 🟢. Confirmados como corretos: o sentinel null do `ApplyWhenProne`, a remoção da seção `Default Hands/Arms` e o fix dos eixos Yaw/Roll (este com corroboração independente via `PassiveMountDetectPatch`). |

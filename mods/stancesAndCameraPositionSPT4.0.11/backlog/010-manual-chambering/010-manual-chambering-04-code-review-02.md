# 010 — Manual Chambering & Bolt Action · Code Review 02

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)  
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)  
**Asbuild:** [010-manual-chambering-05-asbuild.md](010-manual-chambering-05-asbuild.md)  
**Data:** 2026-09-04  

> Análise crítica do plano de implementação para inibição de animações espúrias de auto-chambering, parametrização F12 opcional e suporte a Manual Bolt Action. Cada achado recebe um ID `CR-02-MM` permanente.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 0 · Total: 4

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CR-02-01** | B — Bug latente | 🟠 Forte | Cliques rápidos no gatilho (`tap`) acionam `SetBoltActionReload(true)` já no `Start()` | Aceito no Plano |
| **CR-02-02** | B — Bug latente | 🟡 Médio | Vazamento de `AmmoInChamber = 1f` em caso de aborto/reset do Reload (`GClass2016`) | Aceito no Plano |
| **CR-02-03** | B — Bug latente | 🟡 Médio | Vazamento de `AmmoInChamber = 1f` em interrupção abrupta do Draw (`GClass2055`) | Aceito no Plano |
| **CR-02-04** | D — Arquitetura | 🟢 Menor | Sincronização de rede FIKA ao acionar o ferrolho manual via `Shift + T` | Aceito no Plano |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

---

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; deve ser incorporado ao plano de execução.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-02-01 · Cat B — Bug latente · 🟠 Forte

**Cliques rápidos no gatilho (`tap`) acionam `SetBoltActionReload(true)` já no `Start()`**

**Local:** [`Assembly-CSharp/EFT/Player.cs:3178`](../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L3178) e [`Assembly-CSharp/EFT/ClientFirearmController.cs:31`](../../references/eft-decompiled/Assembly-CSharp/EFT/ClientFirearmController.cs#L31)

**Problema:**
No EFT vanilla, o método `Start()` de `DefaultWeaponOperationClass` e `Class1268` executa:
```csharp
FirearmsAnimator_0.SetBoltActionReload(!FirearmController_0.IsTriggerPressed);
```
Se o jogador der um clique rápido de mouse (soltando o botão antes ou durante a execução de `Start()`), `!FirearmController_0.IsTriggerPressed` já será `true`. Se o patch interceptar apenas `SetTriggerPressed()`, o ferrolho ciclará automaticamente no primeiro frame de tiro em cliques rápidos.

**Por que importa:**
O jogador notará que ao segurar o mouse o ferrolho não cicla, mas ao dar um clique seco/rápido a arma ainda cicla sozinha, quebrando a proposta do Manual Bolt Action.

**Sugestão:**
Interceptar tanto `Start()` quanto `SetTriggerPressed()` em `DefaultWeaponOperationClass` e `Class1268`, garantindo que `SetBoltActionReload(false)` seja forçado incondicionalmente enquanto o comando `Shift + T` não for acionado pelo jogador.

---

### CR-02-02 · Cat B — Bug latente · 🟡 Médio

**Vazamento de `AmmoInChamber = 1f` em caso de aborto/reset do Reload (`GClass2016`)**

**Local:** [`Assembly-CSharp/EFT/Player.cs:8510`](../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L8510) (`GClass2016.Reset`)

**Problema:**
Ao iniciar o reload com câmara vazia, setamos temporariamente `AmmoInChamber = 1f` no Animator para forçar o clipe de Tactical Reload. Se o reload for interrompido abruptamente antes de `SwitchToIdlingState` (por exemplo, sprint imediato ou cancelamento por dano crítico), o parâmetro no Animator poderia permanecer em `1f`.

**Por que importa:**
O Animator da arma poderia registrar visualmente que a câmara tem munição para transições subsequentes mesmo estando vazia.

**Sugestão:**
Garantir a restauração de `FirearmsAnimator_0.SetAmmoInChamber(0f)` tanto no `SwitchToIdlingState` quanto no `Reset()` de `GClass2016`.

---

### CR-02-03 · Cat B — Bug latente · 🟡 Médio

**Vazamento de `AmmoInChamber = 1f` em interrupção abrupta do Draw (`GClass2055`)**

**Local:** [`Assembly-CSharp/EFT/Player.cs:10621`](../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L10621) (`GClass2055.Reset`)

**Problema:**
Similar ao CR-02-02, durante o Draw o parâmetro `AmmoInChamber` é elevado para `1f`. Se o jogador trocar de arma antes de `WeaponAppeared()`, o callback pode não rodar.

**Por que importa:**
Evita inconsistências visuais de animação caso haja troca de armas antes da conclusão da animação de saque.

**Sugestão:**
Restaurar `FirearmsAnimator_0.SetAmmoInChamber(0f)` no método `Reset()` de `GClass2055` caso `ManualChamberingState.BlockChambering` esteja ativo.

---

### CR-02-04 · Cat D — Arquitetura · 🟢 Menor

**Sincronização de rede FIKA ao acionar o ferrolho manual via `Shift + T`**

**Local:** [`Assembly-CSharp/EFT/ClientFirearmController.cs:64`](../../references/eft-decompiled/Assembly-CSharp/EFT/ClientFirearmController.cs#L64) (`Class1268.method_14`)

**Problema:**
No EFT cliente, ao ciclar o ferrolho, o método `method_14(true)` seta `ClientFirearmController_0.FirearmPacket.ReloadBoltAction = true`. Se o mod apenas setar a animação na Unity e não despachar o pacote de rede, outros jogadores no coop FIKA podem não ver a animação de bolt action do jogador local.

**Por que importa:**
Garante paridade visual em raids coop no FIKA.

**Sugestão:**
No `ManualChamberingInputPatch`, ao disparar o ciclo de ferrolho em `Class1268`, chamar também `clientBoltOp.method_14(true)`.

---

## Histórico

| Data | Evento |
| :--- | :--- |
| 2026-09-04 | Code review 02 criada analisando o plano de implementação |

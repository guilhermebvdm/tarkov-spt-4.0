---
title: "Relatório de Code Review — SPT-ContinuousLoadAmmo (Review 03 - v1.1.10)"
date: 2026-09-05
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Code Review — SPT-ContinuousLoadAmmo (Review 03)

**Mod:** `SPT-ContinuousLoadAmmo`  
**Versão Avaliada:** `1.1.10` (SPT 4.0.13 / EFT 0.16.9)  
**Data:** 2026-09-05  
**Autor:** Antigravity  

> Análise crítica do código implementado na versão **1.1.9** e refinado na versão **1.1.10** para correção do bug de concorrência na recarga de armas (tecla R), disputa de transição de mãos com o `LoadAmmoAnim`, isolamento estrito de fechamento de inventário (`InventoryScreenClosePatch`) e melhorias defensivas de input e runtime.

---

## 1. Resumo Executivo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 pendentes · 🟢 Menores: 0 pendentes · ✅ 4 Resolvidos na v1.1.10

| Severidade / Status | Quantidade | Descrição |
| :--- | :---: | :--- |
| 🔴 **Bloqueador** | 0 | Falhas graves, travamentos de tela ou quebra garantida. |
| 🟠 **Forte** | 0 | Riscos altos de regressão ou corrupção de estado. |
| 🟡 **Médio** | 0 | Todos os gaps de entrada e defensiva foram sanados na v1.1.10. |
| 🟢 **Menor** | 0 | Nomenclatura semântica documentada e simetria de guardas aplicada. |
| ✅ **Resolvidos na v1.1.10** | **4** | `CR-03-01`, `CR-03-02`, `CR-03-03` e `CR-03-04` aplicados com 100% de conformidade. |
| 🚀 **Status Final** | — | **APROVADO PARA PRODUÇÃO (0 Bloqueadores — 100% Resolvido)** |

---

## 2. Índice de Achados

| ID | Categoria | Impacto | Título | Status |
| :--- | :--- | :---: | :--- | :---: |
| `CR-03-01` | B — Bug latente / C — Gap | 🟡 Médio | Omissão de `SelectFastSlot9` e `SelectFastSlot0` no cancelamento de entrada | ✅ **Resolvido na v1.1.10** |
| `CR-03-02` | B — Bug latente / D — Arquitetura | 🟡 Médio | Falta de encapsulamento `try-catch` protetivo no `Prefix` do `InventoryScreenClosePatch` | ✅ **Resolvido na v1.1.10** |
| `CR-03-03` | E — Legibilidade / Prontidão 4.1 | 🟢 Menor | Referência direta ao tipo de interface ofuscada `GInterface198` sem documentação semântica | ✅ **Resolvido na v1.1.10** |
| `CR-03-04` | E — Manutenção / Simetria | 🟢 Menor | Guarda explícita de `hasLoadAmmoAnim` ausente em `StopLoading()` para simetria de estado | ✅ **Resolvido na v1.1.10** |

---

## 3. Avaliação Técnica Detalhada das Alterações

### 3.1. Interceptação de Comandos e Liberação da Tecla R
- **Arquivo:** [`mods/SPT-ContinuousLoadAmmo/modded/Components/LoadAmmoComponent.cs:L97-L115`](../../modded/Components/LoadAmmoComponent.cs#L97-L115)
- **Diagnóstico:** A inclusão dos comandos de recarga (`ECommand.ReloadWeapon`, `QuickReloadWeapon`), seleção de armas principais/secundárias/faca e toda a grade de slots rápidos (4 a 0) no `TranslateCommand` resolve diretamente o travamento da tecla R. Ao acionar `StopLoading()` e retornar `ETranslateResult.Ignore`, o comando flui livremente para o `FirearmControllerClass` do EFT restabelecendo o manuseio e a recarga da arma sem qualquer latência ou bloqueio de entrada.

### 3.2. Gating Condicional e Proteção Defensiva em `InventoryScreenClosePatch`
- **Arquivo:** [`mods/SPT-ContinuousLoadAmmo/modded/Patches/InventoryScreenClosePatch.cs:L24-L56`](../../modded/Patches/InventoryScreenClosePatch.cs#L24-L56)
- **Diagnóstico:** O patch agora inspeciona `LoadAmmoController.Instance?.IsActive` e está totalmente protegido por bloco `try-catch`. Se o jogador fechar o inventário sem que haja um carregamento contínuo de munição ativo, o patch retorna imediatamente sem anular `___inventoryController_0`. Isso preserva integralmente o ciclo de vida vanilla do `InventoryScreen.Close()`, permitindo que o EFT encerre processos normais quando o jogador não estiver utilizando as mecânicas do mod. Se houver ações de mãos pendentes (`HasAnyHandsActionNonLinq()`), o mod cancela o abastecimento e devolve o controle ao jogo.

### 3.3. Coexistência com `LoadAmmoAnim` e Simetria de Estado FSM
- **Arquivo:** [`mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs:L277-L281`](../../modded/Controllers/LoadAmmoController.cs#L277-L281), [`L434-L448`](../../modded/Controllers/LoadAmmoController.cs#L434-L448) e [`L469-L474`](../../modded/Controllers/LoadAmmoController.cs#L469-L474)
- **Diagnóstico:** Ao verificar a presença do `LoadAmmoBundleController`, o mod opera de maneira totalmente não-destrutiva: não força `SetEmptyHands()` nem disputa `TrySetLastEquippedWeapon()`. O callback defensivo com anotação semântica em `GInterface198` (`IHandsController`), associado à limpeza de velocidade (`ESpeedLimit.BarbedWire`) e sprint lock em `StopLoading()`, anula qualquer chance de travamento físico do personagem.

---

## 4. Achados da Rodada e Resoluções

### CR-03-01 · B — Bug latente / C — Gap de Entrada · 🟡 Médio

**Omissão de `SelectFastSlot9` e `SelectFastSlot0` no cancelamento de entrada**

**Local:** [`mods/SPT-ContinuousLoadAmmo/modded/Components/LoadAmmoComponent.cs:104-110`](../../modded/Components/LoadAmmoComponent.cs#L104-L110)

**Problema:**
A lista de comandos interceptados em `TranslateCommand` para cancelar o carregamento fora do inventário cobria os slots rápidos de 4 a 8, mas omitia os slots 9 e 0 (teclas '9' e '0' no teclado para itens de bolso/colete).

**Aplicação na v1.1.10:**
Adicionados `ECommand.SelectFastSlot9` e `ECommand.SelectFastSlot0` à condicional de cancelamento.

**Status:** ✅ **Resolvido na v1.1.10**

---

### CR-03-02 · B — Bug latente / D — Arquitetura · 🟡 Médio

**Falta de encapsulamento `try-catch` protetivo no `Prefix` do `InventoryScreenClosePatch`**

**Local:** [`mods/SPT-ContinuousLoadAmmo/modded/Patches/InventoryScreenClosePatch.cs:28-55`](../../modded/Patches/InventoryScreenClosePatch.cs#L28-L55)

**Problema:**
O método `Prefix` realizava acessos de propriedades e verificação de controllers sem proteção contra exceções. Caso ocorresse erro não tratado, o Harmony abortaria o `InventoryScreen.Close()` original, impedindo o fechamento da tela de inventário.

**Aplicação na v1.1.10:**
Encapsulado todo o corpo do `Prefix` em um bloco `try-catch (Exception ex)`, logando eventuais falhas sem bloquear a execução do método original vanilla.

**Status:** ✅ **Resolvido na v1.1.10**

---

### CR-03-03 · E — Legibilidade / Prontidão 4.1 · 🟢 Menor

**Referência direta ao tipo de interface ofuscada `GInterface198` sem documentação semântica**

**Local:** [`mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs:437`](../../modded/Controllers/LoadAmmoController.cs#L437)

**Problema:**
O callback em `SetEmptyHands` utilizava a interface ofuscada `GInterface198` sem comentário indicativo de seu papel arquitetural.

**Aplicação na v1.1.10:**
Adicionado comentário explicativo documentando `GInterface198 = IHandsController (resultado assíncrono da transição de mãos do EFT)`.

**Status:** ✅ **Resolvido na v1.1.10**

---

### CR-03-04 · E — Manutenção / Simetria · 🟢 Menor

**Guarda explícita de `hasLoadAmmoAnim` ausente em `StopLoading()` para simetria de estado**

**Local:** [`mods/SPT-ContinuousLoadAmmo/modded/Controllers/LoadAmmoController.cs:277-281`](../../modded/Controllers/LoadAmmoController.cs#L277-L281)

**Problema:**
Em `SetPlayerStateRoutine(false)` existia a checagem explícita `!hasLoadAmmoAnim`, enquanto em `StopLoading()` a guarda dependia exclusivamente da avaliação de `_player.HandsIsEmpty`.

**Aplicação na v1.1.10:**
Adicionada a checagem explícita `bool hasLoadAmmoAnim = _player.HandsController?.GetType().Name == "LoadAmmoBundleController";` e a guarda `!hasLoadAmmoAnim` antes de invocar `TrySetLastEquippedWeapon()`.

**Status:** ✅ **Resolvido na v1.1.10**

---

## 5. Auditoria de Versionamento e Build

- **Versão SemVer:** `1.1.10` (sincronizada em [ContinuousLoadAmmo.cs](../../modded/ContinuousLoadAmmo.cs), [ContinuousLoadAmmo.csproj](../../modded/ContinuousLoadAmmo.csproj) e [mod.json](../../mod.json)).
- **Compilação Release:**
  - **Erros:** 0
  - **Avisos:** 0
  - **Binário Gerado:** `mods/SPT-ContinuousLoadAmmo/builds/ContinuousLoadAmmo.dll` (54.272 bytes)
- **Isolamento de Build:** Configurado estritamente em [Directory.Build.targets](../../modded/Directory.Build.targets) direcionando binários exclusivamente para a pasta local `builds/`, sem poluir diretórios do jogo.

---

## 6. Histórico

| Data | Evento |
| :--- | :--- |
| 2026-09-05 | Code Review 03 inicial gerado via `/code-review` com 4 achados (2 médios, 2 menores). |
| 2026-09-05 | Aplicação integral de todas as melhorias defensivas, bump para v1.1.10 e compilação Release com 0 erros e 0 avisos. |

# 010 — Manual Chambering · Code Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)
**Asbuild / Correção prévia:** [010-manual-chambering-06-fix-01.md](010-manual-chambering-06-fix-01.md)
**Data:** 2026-09-03

> Análise crítica do código implementado por `/code-mod` (v2.17.3 em `modded-testchannel/`). Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 4 · Total: 7

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| **CR-01-01** | **A** | 🟢 (✅ Resolvido) | Travamento na Stance 0 após rechamber manual corrigido via `EndActionStance` | ✅ Resolvido |
| **CR-01-02** | **A** | 🟢 (✅ Resolvido) | Isolamento absoluto de bots e jogadores observados no FIKA coop | ✅ Resolvido |
| **CR-01-03** | **B** | 🟢 (✅ Resolvido) | Prevenção de vazamento de estado de câmara vazia em `StanceManager.EndActionStance` | ✅ Resolvido |
| **CR-01-04** | **C** | 🟢 (✅ Resolvido) | Resolução do gap de reload vazio via supressão de `PopNewAmmoResult` em `GClass2006.Run` | ✅ Resolvido |
| **CR-01-05** | **B** | 🟡 Médio | Escopo de recarga limitado a carregadores externos destacáveis (`GClass2006`) | ℹ️ Anotado (Escopo) |
| **CR-01-06** | **D** | 🟢 Menor | Correlação com lição da Sessão 16: eliminação de delays assíncronos e conflitos de inventário | ✅ Conforme |
| **CR-01-07** | **E** | 🟢 Menor | Comentários semânticos nos tipos ofuscados do EFT 0.16.9 (Readiness 4.1) | ✅ Conforme |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · Cat A — Crítico · 🟢 (✅ Resolvido)

**Restauração da postura anterior do jogador ao concluir o rechamber manual**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:72-78`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L72-L78)

**Problema:** O método `RechamberRound` chamava `StanceManager.StartActionStance()` para posicionar a arma no centro durante a puxada do ferrolho. Porém, ao fim da animação no `ManualChamberingComponent` (Phase 2), a chamada `StanceManager.EndActionStance()` não existia.

**Por que importa:** O jogador ficaria travado na postura padrão (Stance 0) após engatilhar manualmente com `Shift + T`, perdendo a postura tática (ex: Low Ready ou High Ready).

**Resolução aplicada:** Na conclusão da Phase 2 do componente, inserida a chamada explícita `StanceManager.EndActionStance()`.

---

### CR-01-02 · Cat A — Crítico · 🟢 (✅ Resolvido)

**Isolamento estrito de bots e jogadores observados no FIKA Coop**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:97-101`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L97-L101) e [`ManualChamberingPatches.cs:228-232`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L228-L232)

**Problema:** Sem validação prévia de identidade de jogador, patches em métodos compartilhados poderiam interceptar o reload de bots (causando travamento de IA com arma vazia) ou de companheiros de equipe em sessões multiplayer com o FIKA.

**Por que importa:** Travamento de IA em combate e risco de desync de armas observadas (`ObservedFirearmController`).

**Resolução aplicada:** Inseridas guardas rigorosas em todos os patches:
- No `StartEquipWeapPatch`: `if (player == null || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return true;`
- No `ReloadExternalMagChamberPatch`: `if (mainPlayer == null || itemController != mainPlayer.InventoryController) return true;`

---

### CR-01-03 · Cat B — Bug Latente · 🟢 (✅ Resolvido)

**Eliminação do vazamento de estado em `EndActionStance`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/StanceManager.cs:370-385`](../../modded-testchannel/StanceManager.cs#L370-L385)

**Problema:** `StanceManager.EndActionStance()` forçava `ManualChamberingState.CanLoadChamber = true` ao fim de qualquer ação (inspecionar arma, checar carregador, checar modo de tiro).

**Por que importa:** Se o jogador realizasse qualquer inspeção visual na arma antes de puxar o ferrolho, a câmara passava a aceitar carregamento automático na próxima troca de arma, violando a regra de câmara vazia.

**Resolução aplicada:** As linhas mutadoras de estado foram removidas de `EndActionStance()`. O flag `CanLoadChamber` agora é controlado exclusivamente pelos gatilhos legítimos de carregamento.

---

### CR-01-04 · Cat C — Gap vs. Spec · 🟢 (✅ Resolvido)

**Supressão nativa de alimentação de câmara no reload via `GClass2006.Run`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:272-302`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L272-L302)

**Problema:** A opção `Manual Chambering On Reload` não funcionava porque dependia de um método inexistente (`method_18` do EFT 0.14). No EFT 0.16.9, a transferência de munição no reload de câmara vazia ocorre dentro de `Player.FirearmController.GClass2006.Run(...)`.

**Por que importa:** A arma continuava recarregando a câmara automaticamente após inserir o carregador, tornando o toggle inútil.

**Resolução aplicada:** Criado o `ReloadExternalMagChamberPatch` interceptando `GClass2006.Run`. Quando a câmara está vazia e o toggle está ativo, passa `null` no argumento `popNewAmmoResult`. O pipeline nativo do Tarkov detecta `PopNewAmmoResult == null` e vai diretamente para `SwitchToIdlingState()`, concluindo a recarga sem alimentar a câmara.

---

### CR-01-05 · Cat B — Bug Latente · 🟡 Médio

**Escopo da recarga manual restrito a carregadores externos destacáveis**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:214`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L214)

**Problema:** O método `ReloadExternalMagChamberPatch` intercepta `GClass2006.Run`, que trata exclusivamente de carregadores externos destacáveis (M4, AK, MP5, etc.). Armas de alimentação interna (como SKS ou Mosin sem carregador destacável, e escopetas tubulares como MP-133) utilizam classes de operação diferentes (`GClass2011` / `InternalReloadOperation`).

**Por que importa:** Armas de carregamento interno bala-a-bala continuarão colocando a primeira bala na câmara se recarregadas vazias pelo menu ou via inserção única.

**Sugestão:** Manter o escopo focado em carregadores externos destacáveis (que representam a quase totalidade das armas de serviço no jogo e onde o conceito de "inserir pente e dar rack no ferrolho" se aplica). Registrar formalmente como delimitação de escopo da funcionalidade.

**Decisão:**
- `[x]` Aceitar sugestão (delimitado como escopo intencional de carregadores externos)

---

### CR-01-06 · Cat D — Arquitetura · 🟢 Menor

**Prevenção contra o bug histórico da Sessão 16 (GClass1561 e conflitos de inventário)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:18-35`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L18-L35) e [`memory/sessions.md:68-71`](../../memory/sessions.md#L68-L71)

**Problema:** Na Sessão 16, foi registrado que atrasos assíncronos (`Phase = 3` com 200ms chamando `TranslateCommand(ECommand.ChamberUnload)`) abriam transações concorrentes de inventário que travavam o FIKA multiplayer com o erro `GClass1561` (*"Default Inventory is currently being modified"*).

**Por que importa:** Garantir que a nova implementação não reintroduza o problema que causou a remoção temporária do patch no passado.

**Avaliação:** O novo código elimina 100% da `Phase = 3` e chamadas recursivas a comandos de entrada. Todas as operações de inventário ocorrem no frame síncrono padrão do EFT e do FIKA, sem risco de corrupção ou concorrência.

---

### CR-01-07 · Cat E — Legibilidade · 🟢 Menor

**Documentação dos conceitos de tipos ofuscados do EFT 0.16.9 (Readiness 4.1)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:17-23`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L17-L23)

**Problema:** O código utiliza aliases para classes ofuscadas do EFT (`GClass2055`, `GClass2016`, `GClass2006`).

**Avaliação:** Os aliases foram acompanhados de comentários semânticos claros explicitando os conceitos equivalentes no EFT 0.16.9 e no padrão 4.1:
- `GClass2055` = `SpawnOperation`
- `GClass2016` = `ReloadExternalMagOperation`
- `GClass2006` = `ReloadExternalMagOperationClass`

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-03 | Code review 01 criada via `/code-review` no canal `modded-testchannel` (v2.17.3) |

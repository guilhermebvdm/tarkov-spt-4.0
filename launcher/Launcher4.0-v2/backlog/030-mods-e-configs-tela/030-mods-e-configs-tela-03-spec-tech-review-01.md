# 030 — Tela "Mods e Configs" · Review da spec técnica 01

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [02-spec-tech](./030-mods-e-configs-tela-02-spec-tech.md) · [01-spec funcional](./030-mods-e-configs-tela-01-spec.md)<br>

---

## Índice

| ID | Cat | Impacto | Título |
|---|---|---|---|
| PA-01-01 | C | 🔴 | Mod opcional desligado **não** vai para quarentena — o motor faz o oposto hoje |
| PA-01-02 | A | 🔴 | Contrato do manifesto novo não especificado — trava 3 pontos da spec |
| PA-01-03 | A | 🔴 | Como a tela nova dispara o sync não está definido |
| PA-01-04 | C | 🟡 | Stub 5.4 usa `SyncActionKind.Preserve`, que não existe |
| PA-01-05 | B | 🟡 | `ForceApplyGroups` é transiente: falha no meio deixa item ligado e não aplicado |
| PA-01-06 | B | 🟡 | Dev Mode bloqueia a ação explícita do player no canal de performance |
| PA-01-07 | A | 🟡 | Resumo da tela logada precisa das contagens sem a tela ter sido aberta |
| PA-01-08 | A | 🟢 | Chaves i18n não enumeradas |

**Contadores:** 🔴 3 · 🟡 4 · 🟢 1 — **todos os 8 resolvidos** na spec técnica (2026-07-19)

**Memória consultada:** sem `memory/sessions.md` para o launcher (item de launcher, não de mod). Índice de memória do projeto consultado — nenhuma pendência 🔴 afeta este item.

---

### PA-01-01 · C — Erro de lógica · 🔴 Bloqueador

**Mod opcional desligado não vai para quarentena — o motor faz exatamente o oposto hoje**

**Problema:** CA-030.8 exige que desligar um mod mova **todos** os `paths` dele para `*-disabled/optional/`. Mas o motor atual **protege deliberadamente** esses arquivos de serem movidos. Em [SyncPlanner.cs:59-63](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L59-L63):

```csharp
// Full manifest path set (mandatory + optional, active or not) — protection CC3:
// files of disabled optional groups are never treated as extras.
var manifestPaths = new HashSet<string>(
    manifestFiles.Select(f => SyncPathUtil.Normalize(f.path)),
    StringComparer.Ordinal);
```

O filtro de `filesToCheck` (`:65-67`) apenas **remove** o arquivo da verificação; como o path continua em `manifestPaths`, o `ScanExtras` não o toca. Resultado atual: arquivo de grupo desligado **fica onde está**, intocado.

A spec técnica só prevê "repontar o filtro" (E-11, §2.1) — o que preserva esse comportamento e faz CA-030.8 **falhar silenciosamente**.

**Por que importa:** é o critério central do eixo de mods opcionais. Implementando como está, desligar um mod não tira nada do jogo: o player desmarca o TarkovIRL, o launcher diz que aplicou, e o mod continua carregando na próxima raid. O bug só aparece in-game (G-1/G-2), depois de todo o resto pronto.

Vale notar **por que** a proteção CC3 existe: no modelo antigo o mod opcional vinha de `Opcionais/`, **fora** do `mods_repo`, e era baixado sob demanda — o path nunca deveria ser tratado como extra. No modelo novo (D-3) o mod vive em `mods_repo/plugins/` como qualquer outro, então a premissa da proteção deixa de valer para ele.

**Sugestão:** o planner passa a emitir ação **explícita** para mod opcional desligado, em vez de depender do `ScanExtras`. Adicionar à spec um branch antes do filtro atual:

- para cada `path` de mod opcional **desligado** que exista no disco → `SyncActionKind.MoveToDisabled` com `MoveTargetRelative = BuildDisabledTarget(path, prefix, DisabledOrigin.Optional)`;
- manter o path em `manifestPaths` (a proteção CC3 continua correta para o `ScanExtras`, que não deve duplicar a ação);
- o filtro de `filesToCheck` continua removendo o arquivo do download.

Isso também torna a ação **visível no relatório** (`moved-to-disabled`) e contável no `IoActionCount` (E-12), o que o caminho via `ScanExtras` não daria.

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

### PA-01-02 · A — Gap de especificação · 🔴 Bloqueador

**O contrato do manifesto novo não está especificado — e três pontos dependem dele**

**Problema:** S-7 (§2.3) diz apenas *"trocar `optionalGroups` por `optionalMods` (novo shape) e manter `performanceOverlay` como lista de itens"*, sem definir o shape. Três pontos da própria spec ficam sem chão:

1. **E-11** — como o planner descobre que um path pertence a um mod opcional desligado? Hoje é via `ManifestFile.optional` / `.optionalGroup` ([ManifestFile.cs:14-15](../../project/SPT.Launcher.Base/Models/Launcher/ManifestFile.cs#L14-L15), consumidos em `SyncPlanner.cs:66`). O modelo novo define os mods por `paths` num JSON — o servidor continua taggeando cada arquivo, ou o launcher resolve por path?
2. **`GroupIdOf(file)`** no stub 5.4 — marcado como *TODO confirmar*, mas é o discriminador do híbrido (§1). Sem ele o branch não compila.
3. **PA-01-01** — a ação de quarentena precisa saber quais paths pertencem a qual mod.

**Por que importa:** é um gap de **contrato entre servidor e launcher**. Decidir isso durante a implementação significa descobrir tarde que o servidor emite uma coisa e o launcher espera outra — e o sintoma é mod opcional que não liga/desliga, difícil de rastrear porque nada quebra explicitamente.

**Sugestão:** especificar o manifesto na spec técnica antes do código, mantendo o tagging por arquivo (é o que o motor já sabe consumir) e somando os metadados. Proposta concreta:

```jsonc
{
  "files": [
    { "path": "BepInEx/plugins/TarkovIRL.dll", "hash": "...", "size": 123,
      "optional": true, "optionalId": "tarkov-irl" },
    { "path": "BepInEx/config-performance/sombras.cfg", "hash": "...", "size": 45,
      "performanceId": "shadows-low" }
  ],
  "optionalMods":      [ { "id": "tarkov-irl", "name": ..., "description": { "pt": ..., "en": ... } } ],
  "performanceItems":  [ { "id": "shadows-low", "name": ..., "description": { "pt": ..., "en": ... } } ]
}
```

Assim: `optionalGroup` vira `optionalId` (rename semântico, mesmo mecanismo — `SyncPlanner.cs:66` sobrevive com ajuste mínimo), `performanceId` resolve o `GroupIdOf(file)` por leitura direta, e as listas de metadados alimentam a UI sem o launcher precisar cruzar paths.

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

### PA-01-03 · A — Gap de especificação · 🔴 Bloqueador

**Como a tela nova dispara o sync não está definido**

**Problema:** a spec diz que a aplicação ao sair reusa "a mesma experiência de atualização" citando `ProfileViewModel.cs:778-803` (§8, passo 13). Mas toda a orquestração de sync vive **dentro do `ProfileViewModel`**: o ponto de entrada `CheckForUpdates` com o guard de concorrência `Interlocked.CompareExchange(ref _syncGate, ...)` ([ProfileViewModel.cs:535](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L535)) e o motor `CheckForUpdatesCore` (`:563-885`). A tela nova é um ViewModel **diferente**, e a spec não diz como ela alcança isso.

**Por que importa:** dois critérios ficam sem caminho de implementação — CA-030.21 (aplicar ao sair reusando barra/progresso/relatório) e **CC-15** (não sobrepor com o sync automático do login). Se cada tela instanciar seu próprio fluxo, o guard `_syncGate` do `ProfileViewModel` não protege nada: duas execuções podem escrever no mesmo arquivo simultaneamente — exatamente o que CC-15 existe para impedir.

Agrava que o `ModUpdateViewModel` **já duplica** essa lógica hoje (R-2 da própria spec); uma terceira cópia consolidaria o problema em vez de resolvê-lo.

**Sugestão:** extrair o fluxo para um serviço compartilhado antes de escrever a tela — adicionar à Fase 3 do checklist, como passo anterior ao 13:

- criar `SyncCoordinator` (singleton ou injetado) com o `_syncGate` **movido para dentro dele**, expondo `Task<SyncResult> RunAsync(SyncRequest request, IProgress<SyncProgress> progress, CancellationToken ct)`;
- `SyncRequest` carrega `ForceApplyGroups` (§1) e a origem da chamada (login automático · verificar arquivos · aplicar da tela);
- `ProfileViewModel`, `ModUpdateViewModel` e `ModsConfigsViewModel` passam a ser **clientes** dele, mantendo só o binding de progresso/status.

Alternativa mais barata, se a extração for grande demais para este item: a tela **não** aplica; ela salva as preferências e navega de volta para o Launcher, e o `ProfileViewModel` dispara o sync ao receber foco. Menos elegante, mas usa o guard existente e não cria a terceira cópia. **Escolher explicitamente uma das duas** — o risco é a implementação improvisar a terceira.

**Decisão:** ✅ **Resolvido** — escolhida a alternativa: **a tela só salva; o `ProfileViewModel` aplica** (spec técnica §5.7). O motor em produção não é tocado e o `_syncGate` segue como único ponto de serialização. A extração de um `SyncCoordinator` fica como item futuro.

---

### PA-01-04 · C — Erro de lógica · 🟡 Importante

**O stub 5.4 usa `SyncActionKind.Preserve`, que não existe**

**Problema:** o stub do branch do planner (§5.4) emite `Kind = SyncActionKind.Preserve` no caminho de customização. Esse valor **não existe** no enum — os valores reais são `Download`, `PreserveCustomized`, `PreserveDevMode`, `DeleteExtra`, `MoveToDisabled`, `SeedCopy`, `ForceCopy` ([SyncAction.cs:6-28](../../project/SPT.Launcher.Base/Sync/SyncAction.cs#L6-L28)). O correto é `PreserveCustomized`, como usado em [SyncPlanner.cs:270](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L270).

**Por que importa:** a regra do workflow é que stubs **compilem** se colados no projeto. Este não compila. Além do erro em si, `PreserveCustomized` é o valor que o `SyncEngine` reconhece para emitir o label `preserved` ([SyncEngine.cs:66](../../project/SPT.Launcher.Base/Sync/SyncEngine.cs#L66)) e que o `SyncPlan.PreserveCount` conta ([SyncPlan.cs:38](../../project/SPT.Launcher.Base/Sync/SyncPlan.cs#L38)) — com o nome errado, o relatório e os contadores ficariam furados mesmo que compilasse.

**Sugestão:** trocar `SyncActionKind.Preserve` → `SyncActionKind.PreserveCustomized` no stub §5.4, e revisar os demais stubs contra o enum real.

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

### PA-01-05 · B — Edge case · 🟡 Importante

**`ForceApplyGroups` é transiente: falha no meio deixa o item ligado e nunca aplicado**

**Problema:** o discriminador do híbrido (§1) é um conjunto em memória, populado quando o player alterna o item. Se o sync falhar, for cancelado (CC-8) ou o launcher fechar antes de concluir, o item fica persistido como **ligado**, mas o arquivo nunca foi aplicado. No próximo sync ele já não está em `ForceApplyGroups` → cai em `preserve-divergent`.

A recuperação depende inteiramente do baseline: se o local ainda bate com o baseline, aplica normalmente; se não bate (o player editou nesse meio-tempo, ou o baseline registrou outro estado), o motor conclui "customizado" e **preserva para sempre** — o item aparece ligado na tela e nunca surte efeito.

**Por que importa:** é o cenário de rede instável, que num servidor coop remoto via Tailscale não é raro. O sintoma — "liguei a config e não acontece nada, mesmo religando" — é exatamente o que D-16 quis evitar, e não tem workaround óbvio para o player (desligar e religar recoloca em `ForceApplyGroups`, mas ele precisa adivinhar isso).

**Sugestão:** persistir a intenção em vez de mantê-la só em memória. Há precedente na base: `PendingOptionalChanges` ([LauncherSettingsProvider.cs:201-202](../../project/SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs#L201-L202)), que hoje é `[JsonIgnore]`. Proposta:

- gravar `PendingApply` (persistido, **sem** `[JsonIgnore]`) com os ids alternados;
- `ForceApplyGroups` é alimentado por ele no início de cada sync;
- um id só sai de `PendingApply` quando a ação daquele item **conclui com sucesso** no `SyncResult`;
- falha/cancelamento preserva a intenção, e o próximo sync retenta sozinho.

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

### PA-01-06 · B — Edge case · 🟡 Importante

**Dev Mode bloqueia a ação explícita do player no canal de performance**

**Problema:** no stub §5.4 o guard de Dev Mode é avaliado **antes** de `justEnabled`, espelhando o `ForceToConfig` ([SyncPlanner.cs:199-211](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L199-L211)). Consequência: um dev com Dev Mode ligado que **liga um item de performance na tela** não vê efeito nenhum — a ação explícita dele é silenciosamente convertida em `PreserveDevMode`.

CC-14 na spec funcional trata de Dev Mode **para mods opcionais** ("não mover build local do dev"), e a razão ali é sólida: proteger build local não solicitada. Mas aqui é o contrário — o player **pediu** explicitamente a troca, clicando no toggle.

**Por que importa:** o Dev Mode é a configuração de quem desenvolve os mods do servidor, ou seja, provavelmente você. O sintoma seria "a tela não funciona na minha máquina" enquanto funciona para os jogadores — o tipo de divergência que consome muito tempo até alguém lembrar do Dev Mode.

**Sugestão:** inverter a ordem no stub §5.4 — avaliar `justEnabled` **antes** do guard de Dev Mode, com comentário explicando a assimetria: Dev Mode protege contra reversão **automática** (sync de rotina), não contra ação **explícita** do usuário. E registrar como corner case novo na spec funcional (CC-19), para o gate humano testar com Dev Mode ligado.

**Decisão:** ✅ **Resolvido** — sugestão aceita: `justEnabled` é avaliado **antes** do Dev Mode (spec técnica §5.4) + CC-19 na spec funcional. Ou seja, **ação explícita do player vence o Dev Mode**; o Dev Mode segue protegendo apenas contra reversão automática do sync de rotina.

---

### PA-01-07 · A — Gap de especificação · 🟡 Importante

**O resumo da tela logada precisa das contagens sem a tela ter sido aberta**

**Problema:** CA-030.13 exige que o resumo mostre "X de Y mods opcionais · Performance: A de B" e destaque novidade. A spec técnica lista o resumo em §4 (`ProfileView.axaml` — "somar resumo clicável"), mas não diz **de onde** o `ProfileViewModel` tira as definições e as contagens. As listas de metadados vêm do manifesto, lido dentro de `CheckForUpdatesCore` ([ProfileViewModel.cs:625](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L625)) — que só roda no fluxo de sync.

**Por que importa:** casos concretos sem resposta: o que o resumo mostra **antes** do primeiro sync da sessão? E se o sync falhar (servidor offline)? Sem definição, o provável é mostrar "0 de 0", que induz o player a achar que não há nada — o oposto de CA-030.13, que quer convidar o clique.

**Sugestão:** definir na spec que as definições são cacheadas junto com o manifesto e expostas por uma propriedade do `ProfileViewModel` alimentada ao fim do sync; e especificar o estado de fallback explicitamente: sem manifesto disponível, o resumo mostra as **preferências salvas** (que persistem localmente) ou fica oculto — nunca "0 de 0". Alinhar com CA-030.15b, que já trata do estado vazio real (servidor sem itens).

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

### PA-01-08 · A — Gap de especificação · 🟢 Menor

**Chaves i18n não enumeradas**

**Problema:** §8 passo 15 pede "i18n: chaves nos 3 lugares, com paridade verificada", mas nenhuma chave é listada. A tela nova, o modal de onboarding, o resumo, os estados vazios e os labels novos do relatório precisam de chaves nomeadas.

**Por que importa:** risco baixo de bug, mas alto de retrabalho — o loader é all-or-nothing (uma chave faltando derruba o locale inteiro e cai no fallback pt), então a conferência é melhor feita contra uma lista fechada do que por varredura no fim.

**Sugestão:** acrescentar à spec técnica uma tabela das chaves previstas (nome + pt + en), agrupadas por área (tela, modal, resumo, relatório). Serve de checklist na implementação e de base para o teste de paridade.

**Decisão:** ✅ **Resolvido** — sugestão aceita e aplicada na spec técnica.

---

## Verificação da §9 (Conformidade com skills) da spec técnica

Conferi as 9 evidências citadas. Resultado: **8 sustentadas, 1 enfraquecida**.

- Checks 1, 2, 4, 6, 7, 8 e o N/A do 9 — evidências conferem (as 27 refs `arquivo:linha` foram validadas linha a linha; `_baseline` existe em [SyncPlanner.cs:19,28](../../project/SPT.Launcher.Base/Sync/SyncPlanner.cs#L19); `ResolveUnderRoot` em `SyncPathUtil.cs:35`).
- **Check 3 (lifecycle/ordem)** — sustentado, mas incompleto: cobre o gatilho do onboarding (R-10) e não cobre a ordenação entre o sync da tela e o sync do login, que é PA-01-03/CC-15.
- **Check 5 (coop/Fika)** — ✅ mantido: RN-2 e G-5 são evidência concreta.

Nenhum check ✅ sem evidência (que seria bloqueador por si).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação — 8 pontos (3 🔴, 4 🟡, 1 🟢). Bloqueadores: quarentena de mod desligado contradiz o motor atual, contrato do manifesto indefinido, disparo do sync pela tela nova sem caminho |
| 2026-07-19 | Guilherme | Todos os 8 pontos resolvidos na spec técnica. PA-01-03 decidido pela alternativa (tela só salva). Spec funcional ganhou CC-19 e CC-20 |

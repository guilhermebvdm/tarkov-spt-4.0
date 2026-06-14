# Memória de sessões — OutfitPersistenceFix

> **Renomeado em 2026-06-13:** `CustomizationPersistenceFix` → **`OutfitPersistenceFix`** (grafia correta "Persistence", a pedido do usuário). Namespace, classes, GUID (`outfitpersistencefix.mdj`), csproj/assembly, README e pasta instalada migrados. A Sessão 1 abaixo é registro histórico — referências a `CustomizationPersistenceFix*` ali eram os nomes vigentes naquele momento.

## Estado atual (snapshot ao fim da última sessão)

- Mod server-side (SPT 4.0.13 / EFT 0.16.9) que corrige o reset de roupa do PMC (`Body/Hands/Feet`) a cada `game/start`. Nome atual: **OutfitPersistenceFix**.
- Patch Harmony aplicado em **`FixProfileBreakingInventoryItemIssues`** (método correto), não mais em `CheckForAndFixPmcProfileIssues` (era no-op).
- Lógica completa: peça válida → preserva; peça inválida/ausente → default da facção. `Head/DogTag/Voice` intocados.
- Melhorias de code-review aplicadas: `[HarmonyPriority(Priority.Last)]` no Postfix + logging Debug de peças preservadas.
- Compila 0 warn / 0 err; DLL instalada em `D:/SPT/SPT/user/mods/OutfitPersistenceFix` (pasta antiga `CustomizationPersistenceFix` removida). Carrega sem erro no log do servidor.
- Validação in-game preliminar: **aparentemente persiste** ("deu sim"); usuário fará mais testes.
- Correção do método + melhorias commitadas em `f4b3296` (sob o nome antigo); rename commitado em seguida.

## Pendências / próximos passos conhecidos

- [P-1.1] Validação in-game definitiva da persistência da skin (equipar → fechar → reabrir, com servidor pronto). Categoria: 🟡 débito (aparentemente OK, não 100% confirmado).
- [P-1.2] Confirmar que `Priority.Last` resolve conflito de ordem com outros mods de customização (AllTheClothes, WTT-*) no stack de ~40 mods. Categoria: 🟢 ideia (validar durante uso real).
- [P-1.3] Report upstream ao SPT (fix de 1 char: `!` em Body/Hands/Feet + typo `DefaulUsecFeet`). Aposentar o mod se o SPT corrigir. Categoria: 🟢 ideia.

---

## 2026-06-13 00:08 (GMT-3) — Sessão 1: corrigir patch target (método errado) + code-review + diagnóstico in-game

**Tema central:** o mod não funcionava; investigar a fundo, achar as classes/variáveis reais, reescrever o fix corretamente e validar. (Trabalho iniciado em 2026-06-12, atravessou a meia-noite.)

**Decisões-chave:**
- **Patch target trocado de `CheckForAndFixPmcProfileIssues` → `FixProfileBreakingInventoryItemIssues`** — o handoff apontou o método errado; `CheckForAndFixPmcProfileIssues` só mexe em quests/hideout/skills e nunca toca em customização, então o mod era um **no-op silencioso**. O código bugado real está em `FixProfileBreakingInventoryItemIssues` (linhas ~180-204), chamado em [GameController.cs:101](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/GameController.cs#L101). Ref: [ProfileFixerService.cs:180-204](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/ProfileFixerService.cs#L180-L204).
- **Gate de config confirmado:** o método só roda quando `core.json → fixes.fixProfileBreakingInventoryItemIssues == true`. Default SPT = `false`; install do usuário (`D:/SPT/SPT/SPT_Data/configs/core.json:28`) = `true` → por isso o bug dispara pra ele.
- **Lógica completa em vez de só restaurar:** Postfix reescreve cada peça — válida → preserva o valor do jogador; inválida/ausente → default da facção (o que o SPT pretendia). Atende o edge case 100%. `Head/DogTag/Voice` intocados. Ref: [ProfileFixerCustomizationPatch.cs](../modded/Server/ProfileFixerCustomizationPatch.cs).
- **Typo `DefaulUsecFeet` mantido verbatim:** a entry existe com esse typo na própria DB do EFT (`customization.json`, 1 ocorrência) — o `FirstOrDefault` do SPT acha, então não há NRE. Por isso o sintoma é reset, não crash.
- **Code-review aplicado:** CR-01 `[HarmonyPriority(Priority.Last)]` (palavra final sobre conflito com mods de customização); CR-02 logging Debug via `CustomizationPersistenceFixMod.Log` (a v1 era no-op silencioso — log confirma disparo); CR-03 sem ação.

**Lições / hipóteses descartadas:**
- **Não confiar no nome de método citado em handoff.** O handoff afirmava bug em `CheckForAndFixPmcProfileIssues`; o real era `FixProfileBreakingInventoryItemIssues`. Sempre confirmar QUAL método contém o código bugado lendo o source. Registrado na memória global [[reference_spt_customization_reset_bug]].
- **Modelo real `Customization` tem 6 campos** (`Head, Body, Feet, Hands, DogTag, Voice` — [BotBase.cs:303](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L303)), não "upper/lower body" como o usuário supunha. O bug afeta só Body/Hands/Feet.
- **Erros in-game NÃO eram do mod (corrida de boot).** `CustomAiPatch.GetBossTypesFromServer()` roda HTTP **síncrono e sem retry** no `.cctor` durante o `Awake` do BepInEx; lançar o cliente antes do servidor terminar de bootar (~40 mods, minutos) → conexão recusada → `TypeInitializationException` → "SUBSEQUENT PATCHES HAVE NOT LOADED". Servidor confirmado carregando o mod sem erro. Lição operacional: subir servidor e esperar ficar pronto **antes** de lançar o jogo.

**Atividade cronológica:**
1. Li o handoff e o código do mod — desconfiança do usuário procedia.
2. Li `ProfileFixerService.cs` real → descobri que `CheckForAndFixPmcProfileIssues` não toca customização; o bug está em `FixProfileBreakingInventoryItemIssues`.
3. Confirmei callers, gate de config (`true` no install), modelo `Customization` (6 campos) e o typo na DB.
4. Reescrevi `ProfileFixerCustomizationPatch.cs` para o método certo com lógica completa; corrigi docs do Mod + README + memória global.
5. Compilei (0/0), instalei. Rodei revisão crítica do diff → CR-01/02/03; usuário pediu "implemente todas" → apliquei Priority.Last + logging; recompilei (0/0).
6. Usuário reportou erros in-game; investiguei (processos, portas, http.json, fika.jsonc, launcher.log, server log, LogOutput.log) → corrida de boot, não o mod. Servidor escuta só no IP Radmin VPN `26.207.194.149` (config FIKA).
7. Usuário subiu servidor primeiro e validou: skin aparentemente persiste.

**Pendências abertas nesta sessão:**
- [P-1.1] Validação in-game definitiva. 🟡 débito.
- [P-1.2] Confirmar Priority.Last vs outros mods de customização no stack real. 🟢 ideia.
- [P-1.3] Report upstream ao SPT (1-char `!` + typo `DefaulUsecFeet`). 🟢 ideia.

**Notas relevantes (não-mod):**
- `_FikaDiscordPresence` não inicia todo boot do servidor: `Discord.WebhookUrl is missing or empty. Mod will NOT start`. Inofensivo; corrigir o `config.json` dele ou remover se não usa Discord presence.
- Servidor bindado só ao IP Radmin VPN `26.207.194.149` (FIKA `fika.jsonc:38-41`), não a `127.0.0.1`. Jogo solo depende do adaptador Radmin ativo.

**Cross-refs:**
- Memória global: [[reference_spt_customization_reset_bug]] (corrigida nesta sessão — método certo) · [[reference_spt_customization_model]].
- Origem: `.handoffs/handoff-2026-06-11-customization-persistence-fix.md` (apontava o método errado).

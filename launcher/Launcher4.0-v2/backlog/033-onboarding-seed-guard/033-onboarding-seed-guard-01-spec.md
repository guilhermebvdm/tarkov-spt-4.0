# 033 — Seed do disco + onboarding universal + guard de alterações (tela Mods e Configs)

**Mod:** Launcher4.0-v2
**Status:** Backlog
**Criado:** 2026-08-01

## Visão geral

Correção de **rollout** do item 030. Do jeito que o 030 saiu, introduzir os mods opcionais **remove os mods que o jogador já usa**: itens novos nascem desligados (D-6), e um mod opcional desligado é movido para `plugins-disabled/optional/` no primeiro sync. O onboarding que ligaria tudo (D-5) só dispara para quem **não tem** plugins — exatamente quem já usa os mods não é protegido. Este item introduz três mecanismos que, juntos, garantem que **ninguém perca mod ao atualizar**, que **todo jogador veja uma mensagem inicial uma vez**, e que **alterações não salvas não escapem** sem confirmação.

## Comportamento atual

- **Default desligado.** `LauncherSettingsProvider.IsOptionalEnabled(id)` retorna `false` para qualquer mod cujo id nunca foi salvo em `EnabledOptionals`. O motor (`SyncPlanner`) exclui mods opcionais desligados do que fica no jogo e `QuarantineDisabledOptionalMods` os move para `plugins-disabled/optional/` — em **todo** sync, não só ao salvar. Único freio: o **Dev Mode**, que preserva os arquivos locais e não move nada.
- **Onboarding restrito.** `ProfileViewModel.ShouldTriggerOnboarding(gamePath)` só retorna `true` quando o `ModsConfigsOnboardingDone` é falso **E** a pasta `BepInEx/plugins` não tem nenhum `.dll`. O gatilho vive **dentro** do `CheckForUpdatesCore`, que o Dev Mode pula no auto-check do login. Resultado: quem já tem mods instalados **nunca** vê o onboarding e cai direto no default-desligado.
- **Sem guard de saída.** A tela Mods e Configs (após o 030 ganhar o menu completo) permite navegar por Launcher/Settings/Buy us a coffee. `OpenSettings` chama `SaveChanges` antes de navegar (salva sempre); os demais itens do menu do sidebar podem sair sem confirmar nem avisar sobre alterações pendentes.

## Comportamento desejado

Três mecanismos independentes, aplicados na tela Mods e Configs e no fluxo de login:

**1. Seed do disco (proteção — roda no login, ANTES do sync).** Antes do `SyncPlanner` executar, o launcher define o estado inicial de cada **mod opcional** que o jogador ainda **não decidiu** (id ausente de `EnabledOptionals`):
- Se o jogador **tem** algum plugin instalado (existe pelo menos um `.dll` em `BepInEx/plugins`, em qualquer profundidade) → **semeia pelo disco**: o mod nasce **ligado** se estiver instalado (arquivo/pasta dos seus `paths` presente), **desligado** se não.
- Se o jogador **não tem nenhum** plugin instalado → nasce **ligado** todo mod das categorias **Optional, Heavy, Performance**; a categoria **dev/Desenvolvedor** nasce **desligada**.
- Itens que o jogador **já decidiu** (salvou ligado ou desligado antes) são **respeitados** — nunca re-semeados.
- **Configs opcionais** nunca são semeadas: nascem **desligadas** por default. Só ficam ligadas se o jogador ligar; a escolha persiste e é respeitada ao voltar ao menu.

Efeito: o sync passa a ver os mods que o jogador usa como **ligados** → não os move para a quarentena → ninguém perde mod, mesmo sem abrir a tela e sem Dev Mode.

**2. Onboarding universal (educação — 1ª vez, todos).** O gatilho passa a ser apenas `!ModsConfigsOnboardingDone` — dispara para **todo** jogador na primeira vez (novo ou que atualizou o launcher), em **qualquer** versão (2.8.x, 2.9.x e diante), **uma única vez** (persiste ao concluir clicando "Salvar"). Roda **independente** de ter plugins e **independente** do Dev Mode (movido para fora do `CheckForUpdatesCore`). Abre a tela mostrando o **estado já semeado** (mecanismo 1) mais uma **mensagem inicial** explicando o que são os opcionais e que ele pode ligar/desligar. Como o seed já protege os mods, se o onboarding **não** for concluído não há perda — ele apenas redispara no próximo login.

**3. Guard de alterações não salvas (segurança ao sair).** Ao clicar em outro item do menu lateral (Launcher / Settings / Buy us a coffee) estando na tela Mods e Configs:
- Se houver **alteração pendente** (estado atual dos toggles ≠ estado salvo) → diálogo **[Salvar e sair] · [Descartar e sair] · [Cancelar]**.
- Se **não** houver alteração → sai **direto**, sem diálogo (caso "entrei pelo Launcher, não mexi em nada, clico Launcher de novo").
- No **modo onboarding**, sair por item de menu **sempre** pede confirmação (garante que o jogador viu e decidiu, mesmo sem mexer).
- O botão **"Salvar e voltar"** segue como fluxo explícito de sempre.

## Critérios de aceite

- [ ] **CA-033.1 (seed, jogador com plugins):** com pelo menos um `.dll` em `BepInEx/plugins`, um mod opcional **não decidido** cujo arquivo/pasta está presente no disco nasce **ligado**; um cujo arquivo/pasta está ausente nasce **desligado**. Verificável: instalar `FooMod`, deixar `BarMod` fora, primeiro login → `Foo` ligado, `Bar` desligado, sem tocar na tela.
- [ ] **CA-033.2 (seed, jogador sem plugins):** com a pasta `BepInEx/plugins` sem nenhum `.dll`, todo mod das categorias **Optional/Heavy/Performance** nasce **ligado** e todo mod da categoria **dev** nasce **desligado**, no primeiro login.
- [ ] **CA-033.3 (seed respeita decisão):** um mod que o jogador salvou (ligado ou desligado) num acesso anterior **não** é re-semeado num login posterior — o estado salvo prevalece, independentemente do que está no disco.
- [ ] **CA-033.4 (seed protege do sync):** após o seed rodar no login, o sync subsequente **não** move para `plugins-disabled/optional/` nenhum mod que o seed deixou ligado. Verificável in-game: jogador com mods, sem Dev Mode, primeiro login pós-atualização → os mods continuam em `BepInEx/plugins/` (nada some).
- [ ] **CA-033.5 (configs sempre desligadas):** nenhuma config opcional nasce ligada por seed ou onboarding. Uma config só fica ligada se o jogador a ligar na tela e salvar; ao reabrir o menu, ela aparece ligada (persistida).
- [ ] **CA-033.6 (onboarding universal, uma vez):** o onboarding dispara na primeira vez para jogador **com** e **sem** plugins e **com** e **sem** Dev Mode; ao concluir (clicar "Salvar"), grava `ModsConfigsOnboardingDone` e **não** dispara novamente nos logins seguintes.
- [ ] **CA-033.7 (onboarding em qualquer versão):** um jogador cujo `ModsConfigsOnboardingDone` está ausente vê o onboarding uma vez ao atualizar de qualquer versão anterior — independentemente de ser 2.8.x, 2.9.x ou posterior.
- [ ] **CA-033.8 (guard com alteração):** com uma alteração pendente na tela (um toggle mudado em relação ao salvo), clicar em Launcher/Settings/Buy us a coffee abre o diálogo com as três opções; "Salvar e sair" persiste e navega, "Descartar e sair" reverte ao salvo e navega, "Cancelar" permanece na tela.
- [ ] **CA-033.9 (guard sem alteração):** sem nenhuma alteração pendente, clicar em outro item de menu navega **direto**, sem diálogo.
- [ ] **CA-033.10 (guard no onboarding):** no modo onboarding, clicar em qualquer item de menu sempre pede confirmação, mesmo sem alteração.
- [ ] **CA-033.11 (botão "Salvar e voltar" como footer fixo):** o botão "Salvar e voltar" fica **sempre visível** num rodapé fixo da tela Mods e Configs, independente da rolagem — não rola junto com a lista de itens (hoje ele fica no fim do conteúdo rolável e some quando há muitos itens). Verificável: com a lista longa o suficiente para rolar, o botão permanece à vista no rodapé.
- [ ] **Fika/multiplayer:** o seed garante que um mod coop-essencial já instalado (ex.: um plugin da família Fika marcado como opcional por engano) permaneça **ligado** e não seja movido — mantendo a paridade de mods entre host e clientes. O mecanismo é 100% client-side (estado local antes do sync), sem pacote de rede; a proteção existente (guard coop-safe do 030) segue valendo como segunda barreira.
- [ ] **Estado entre raids:** `N/A` — o comportamento acontece no launcher (fluxo de login/pré-jogo), não durante um raid; nenhum estado é criado ou alterado dentro do EFT.

## Corner cases

- [ ] **CC-1 (mod-pasta vs .dll na detecção):** **Decisão (2026-08-01):** "instalado" = existe **qualquer arquivo sob o prefixo** no disco. Pasta (`BepInEx/plugins/FooMod`) conta se contém ao menos um arquivo (em qualquer profundidade); `.dll` (`BepInEx/plugins/Foo.dll`) conta se o arquivo existe. Pasta vazia = não instalado.
- [ ] **CC-1b (mod com múltiplos `paths`):** **Decisão (2026-08-01):** "instalado" = **qualquer** um dos `paths` presente (não exige todos) — mesmo critério do CC-1.
- [ ] **CC-1c (arquivo em plugins-disabled não conta como plugins para o gate "sem plugins"):** o teste "jogador sem nenhum plugin" (CA-033.2) olha `BepInEx/plugins`; arquivos em `BepInEx/plugins-disabled` não devem contar como "tem plugins" — senão um cliente cujos mods foram todos quarentenados seria tratado como "tem plugins" e não receberia o default de categorias. (Interage com CC-13.)
- [ ] **CC-2 (pasta plugins inexistente):** se `BepInEx/plugins` nem existe (instalação nova/limpa), tratar como "sem plugins" (CA-033.2), não como erro.
- [ ] **CC-3 (mod parcialmente instalado):** mod-pasta com alguns arquivos presentes e outros faltando — decidir se conta como instalado (proposta: presença de qualquer arquivo sob o prefixo = instalado, para não deixar meio-mod virar quarentena).
- [ ] **CC-4 (Dev Mode):** o seed roda mesmo em Dev Mode (popula o estado); o onboarding dispara em Dev Mode; o sync não move nada em Dev Mode de qualquer forma. Confirmar que não há conflito nem dupla-ação.
- [ ] **CC-5 (onboarding não concluído):** jogador abre o launcher, o onboarding dispara, ele fecha sem clicar "Salvar" → `ModsConfigsOnboardingDone` não é setado, redispara no próximo login; os mods continuam protegidos pelo seed (nenhuma perda no intervalo).
- [ ] **CC-6 (idempotência do seed):** logins repetidos não alteram itens já semeados/decididos — o seed só age sobre ids ausentes de `EnabledOptionals`.
- [ ] **CC-7 (ordem de execução seed × sync):** o seed deve rodar **antes** de o `SyncPlanner` avaliar a quarentena; se rodar depois, o mod é movido antes de ser semeado. Vale para **qualquer** caminho que dispare o sync — o auto-check do login **e** o "Verificar arquivos" manual (que roda mesmo em Dev Mode). O seed não pode depender de a tela ter sido aberta.
- [ ] **CC-8 (detecção de alteração no guard):** a comparação "atual ≠ salvo" precisa refletir o estado efetivo — ligar e desligar de volta (voltando ao salvo) **não** conta como alteração pendente.
- [ ] **CC-9 (config ligada persiste):** o jogador liga uma config opcional, salva, sai e volta ao menu → a config aparece ligada. Desligar e salvar → volta a desligada.
- [ ] **CC-10 (seed × onboarding não conflitam):** quando o onboarding dispara, a tela mostra o estado que o seed definiu (não sobrescreve com "tudo ligado" nem "tudo desligado").
- [ ] **CC-11 ("Descartar e sair"):** descartar reverte os toggles para o último estado salvo, sem persistir nada nem disparar apply.
- [ ] **CC-12 (jogador que já concluiu onboarding no 030):** quem já tem `ModsConfigsOnboardingDone=true` de um acesso anterior ao 030 **não** vê o onboarding de novo — mas o seed ainda protege os mods dele (o seed independe do flag de onboarding).
- [ ] **CC-13 (RECOVERY de quem já foi afetado pelo bug):** um jogador que rodou o launcher **sem** Dev Mode entre o 030 e este item já teve os mods opcionais movidos para `BepInEx/plugins-disabled/optional/`. **Decisão (2026-08-01): o seed olha SÓ `BepInEx/plugins`** — não inspeciona a quarentena. Quem já foi afetado **religa manualmente** os mods na tela (o launcher os restaura da quarentena ao religar, via o fluxo do 030). Caso de borda coberto pela interação com CC-1c: se **todos** os mods do jogador eram opcionais e foram quarentenados, `plugins/` fica vazio → cai no gate "sem plugins" (CA-033.2) → as categorias Optional/Heavy/Performance são religadas automaticamente. **Ação de rollout:** comunicar aos jogadores que já atualizaram sem Dev Mode que devem reabrir a tela e religar o que usam.
- [ ] **CC-14 (fechar o launcher / logout com alteração pendente):** **Decisão (2026-08-01): descartar sem aplicar.** Fechar o launcher (X) ou clicar Logout com alteração não confirmada na tela **não** persiste nem aplica nada — o estado salvo anterior prevalece. Só o que o jogador confirma explicitamente (Salvar / Salvar e sair) é aplicado.
- [ ] **CC-15 (mod coop-essencial ausente marcado opcional):** um mod coop-essencial (ex.: família Fika) que **não** está no disco do cliente e foi marcado opcional no `plugins-optional.json` → o seed não o liga (ausente) → o sync não o baixa → o cliente pode não conseguir entrar no raid do host. É erro de **conteúdo** (esses mods nunca devem ser opcionais), e o guard coop-safe do 030 só protege o caso "já instalado". Sinalizar no rollout; fora do escopo de código deste item, mas registrado como risco.
- [ ] **CC-16 (novo opcional adicionado com o tempo):** um mod opcional que o servidor passa a distribuir **depois** do primeiro login do jogador é não-decidido → é semeado no login seguinte (o seed não é só-na-1ª-vez; age sobre qualquer id ausente de `EnabledOptionals`, sempre).
- [ ] **CC-17 (o seed persiste, o flag do onboarding não):** o seed grava `EnabledOptionals` no `config.json` (para o sync ler), mas **não** seta `ModsConfigsOnboardingDone` — este só é gravado quando o jogador conclui o onboarding clicando "Salvar". Consequência coerente com CC-5: mods protegidos mesmo sem concluir o onboarding.

## Fora de escopo

- [ ] Redesenho do modal de onboarding além da mensagem inicial (texto/layout ficam na spec técnica).
- [ ] Extração do sidebar duplicado para um controle compartilhado (pendência separada — ver memória `project_launcher_sidebar_settingsview_pendente`; o guard aqui é implementado nos comandos de navegação existentes).
- [ ] Seed de **configs** opcionais a partir do disco (decisão explícita: configs nascem sempre desligadas).

## Referências

- [Item 030 — spec funcional](../030-mods-e-configs-tela/030-mods-e-configs-tela-01-spec.md) (D-5 onboarding liga tudo, D-6 item novo desligado, CA-030.8 quarentena de mod desligado)
- Memória `reference_launcher_devmode_skips_optional_catalog` (Dev Mode pula o auto-check que popula o catálogo)

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/create-spec` — comportamentos e corners fechados com o usuário em sessão (seed do disco, onboarding universal, guard de alterações). |
| 2026-08-01 | Revisão `/review-spec` — 7 corner cases adicionados (CC-1b/1c mod-pasta/gate-sem-plugins, CC-13 recovery de quem já foi afetado, CC-14 fechar/logout, CC-15 mod coop ausente, CC-16 novo opcional com o tempo, CC-17 seed persiste ≠ flag onboarding) + 4 pontos marcados para decisão (critério de instalado, múltiplos paths, recovery, fechar/logout). |
| 2026-08-01 | 4 decisões fechadas com o usuário: "instalado" = qualquer arquivo/path sob o prefixo (CC-1/1b); recovery = seed olha SÓ `plugins/`, jogador religa manual (CC-13); fechar/logout com alteração = descarta sem aplicar (CC-14). Spec pronta para a técnica. |
| 2026-08-01 | CA-033.11 adicionado (pedido do usuário): botão "Salvar e voltar" como footer fixo sempre visível na tela. |

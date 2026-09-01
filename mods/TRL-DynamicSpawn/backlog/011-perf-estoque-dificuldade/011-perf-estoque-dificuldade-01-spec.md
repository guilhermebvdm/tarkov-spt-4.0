# 011 — perf-estoque-dificuldade

**Mod:** TRL-DynamicSpawn
**Status:** Backlog *(implementação só após a validação V2 do item 010 — os números da V2 calibram os valores daqui)*
**Criado:** 2026-08-23T20:29:00-03:00

> Fecha o ciclo do estoque de perfis iniciado em AUD-01-04/CR-01-01 (item 010). Três frentes que se completam: estoque pequeno numa dificuldade só, etiqueta de dificuldade reescrita no momento da escolha, e o sorteio de dificuldade do painel web voltando a valer com SAIN ativo. Desenho de fluxo acordado com o usuário em 2026-08-23 (sessão desta data).

## Visão geral

Hoje o estoque de perfis é mantido pelo SPT em níveis permanentes por (tipo, dificuldade), e a dificuldade sorteada pelo painel só funciona sem SAIN — com SAIN o mod pede tudo em `normal` por uma premissa que se provou **falsa** (item 004). Este item: (1) deriva o nível do estoque do cap do mapa, com teto; (2) mantém o estoque **só em `normal`** e reescreve a etiqueta de dificuldade do perfil escolhido para a dificuldade sorteada da onda; (3) remove o desvio `isSainActive → normal`, fazendo os pesos do painel valerem com e sem SAIN.

## Fatos verificados que sustentam o desenho (2026-08-23)

- **SAIN respeita a etiqueta de dificuldade.** Os arquivos de configuração do SAIN (instalação real, `D:\SPT\BepInEx\plugins\SAIN\Default Bot Config Values\*.json`) têm seções `easy`/`normal`/`hard`/`impossible` por tipo de bot; no BEAR, **48 parâmetros** diferem entre easy e impossible (precisão, tempo de pânico, desvio de mira ao ser atingido, etc.). O SAIN lê a etiqueta no nascimento e aplica a seção correspondente — não sorteia nem sobrescreve. **Consequência: a decisão do item 004 ("com SAIN, pedir tudo normal") anula os pesos do painel hoje.**
- **A etiqueta NÃO influencia o equipamento neste setup.** O equipamento dos bots é gerado pelo **ProgressiveBotSystem 2.2.1** (server mod C#, vendorizado em [mods/progressivebotsystem-csharp/](../../../progressivebotsystem-csharp/), tag `19bfaa6c`): a seleção é por **Tier 0–7** derivado do **nível do jogador** (`Data/Tiers/TierData.json`: `PlayerMinLevel`/`PlayerMaxLevel` + variância de nível do bot) e por **papel** (`Data/Equipment/TierN_equipment.json` chaveado por `pmcUSEC`/`pmcBEAR`/`scav`/bosses). A palavra "difficulty" aparece **3 vezes no código inteiro, todas em logging** (`Models/BotLogData.cs:11`, `Helpers/BotLogHelper.cs:57/:116`). Reescrever a etiqueta no cliente não desalinha corpo e mente de bot algum.
- **Reescrever a etiqueta é barato e o ponto certo já existe:** o `ChooseProfilePatch` (item 010) já segura o perfil na mão no momento da escolha; a troca é uma escrita de campo (`Info.Settings.BotDifficulty`), antes do nascimento — o jogo e o SAIN leem a etiqueta só no nascimento.

## Comportamento atual

- Estoque: nível fixo `Initial Profile Preload` (15) para USEC/BEAR `normal`, igual em Factory (cap 15) e Streets (cap 30); ondas registram níveis **adicionais permanentes** por dificuldade sorteada (até 2 facções × 3 dificuldades extras, só sem SAIN).
- Dificuldade: sem SAIN, sorteada por onda e honrada só quando o estoque tem a etiqueta exata (senão nasce "relaxado" — AC-X1 do 010); **com SAIN, sempre `normal`** → pesos do painel sem efeito.

## Comportamento desejado

- **Frente A — estoque com teto por mapa:** nível = `min(Initial Profile Preload, ceil(cap de bots do mapa / 2))` por facção, registrado só em `normal`. As ondas **não registram mais níveis por dificuldade sorteada** (o bloco por onda do 010 sai).
- **Frente B — etiqueta reescrita na escolha:** o `ChooseProfilePatch` grava a dificuldade **pedida** no perfil escolhido antes de entregá-lo. Todo bot nasce com a etiqueta exata da onda; o caso "nasceu com outra dificuldade" (AC-X1 do 010) deixa de existir.
- **Frente C — painel vale com SAIN:** remover o desvio `isSainActive ? normal : sorteio` (`DynamicSpawnManager.cs`, hoje ~`:790-793`). Com SAIN, a etiqueta sorteada seleciona a seção easy/normal/hard/impossible **do próprio SAIN**.

## Critérios de aceite (esboço — refinar no /create-technical-spec após a V2)

- [ ] **Não-regressão:** composição/cadência das ondas, bosses, snipers e reposição de estoque como no 010; nenhuma linha de estoque com dificuldade ≠ `normal` criada pelo mod.
- [ ] **Medível 1:** estoque total em Factory ≈ metade do de Streets (níveis derivados do cap); `profilesInList` estável e **menor** que na V2 do 010.
- [ ] **Medível 2:** com `Enable Debug Logs`, o log `CHOSEN PROFILE` nunca mais mostra `[difficulty relaxed]` — mostra a etiqueta reescrita = pedida.
- [ ] **Medível 3 (comportamento):** com SAIN ativo e painel 100% `impossible`, os bots nascem com etiqueta `impossible` (conferir no log do SAIN/`SPAWN ->`).
- [ ] **Mudança declarada (reverte item 004):** com SAIN, a dificuldade dos bots passa a seguir o painel web (antes: tudo `normal`). Alinhar com o Umbigo — é decisão de produto registrada no 004.
- [ ] **Fika/multiplayer** e **estado entre raids:** critérios padrão (guest inerte; nada sobrevive à raid).

## Corner cases (esboço)

- [ ] Perfil escolhido com `withDelete` — a reescrita acontece **após** a remoção da lista (o estoque não fica com etiqueta trocada).
- [ ] Setup **sem** ProgressiveBotSystem (fallback vanilla do SPT): verificar no server SPT se o gerador vanilla usa a dificuldade para equipamento antes de generalizar a Frente B para publicação do mod (para o nosso setup, verificado que não).
- [ ] Painel com pesos zerados/inválidos → `normal` (comportamento atual do `GetRandomDifficulty`).
- [ ] Cap dinâmico (bosses vivos) — o teto usa o cap **base** do mapa, não o dinâmico (estável durante a raid).

## Fora de escopo

- [ ] Mexer no ProgressiveBotSystem (vendorizado apenas como referência de leitura).
- [ ] Tiers/equipamento por dificuldade (não existe hoje; seria feature nova do PBS, não deste mod).

## Referências

- [Item 010](../010-perf-spawn-pipeline-r2/) — CR-01-01 (semântica do `AddToTargetBackup` = nível permanente, `GClass684.cs:113-118/:129-192/:258-263`)
- [mods/progressivebotsystem-csharp/](../../../progressivebotsystem-csharp/) — tag 2.2.1, auditoria equipamento × dificuldade (esta spec, "Fatos verificados")
- Item [004](../004-dificuldade-bots-sain-integration/) — a decisão revertida pela Frente C
- `D:\SPT\BepInEx\plugins\SAIN\Default Bot Config Values\` — evidência das seções por dificuldade do SAIN

## Histórico

| Data | Evento |
|---|---|
| 2026-08-23 | Item criado após investigação SAIN + ProgressiveBotSystem 2.2.1 (fluxo estoque→escolha→etiqueta→nascimento acordado com o usuário). Aguarda V2 do 010 para calibração |

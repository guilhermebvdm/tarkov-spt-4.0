# 089 — perf — Rodada 01 de otimização

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-08-22

> **Perfil de NÃO-REGRESSÃO.** Este item não entrega comportamento novo. O contrato funcional **é o comportamento atual** — cada critério de aceite descreve algo que precisa permanecer **idêntico**, mais uma meta mensurável de custo. Origem: [relatório de auditoria 01](../../docs/relatorio-auditoria-codigo-01.md) (modo `--perf`) + [revisão 01](../../docs/relatorio-auditoria-codigo-01-review-01.md). Decisão do usuário em 2026-08-22: aceitar os 8 achados e dispensar a mini-rodada de instrumentação prévia.

## Visão geral

Rodada agrupadora dos 8 achados da auditoria de performance. Nenhum é 🔴/🟠: são um cache de textura que cresce sem teto, um polling caro no menu, o custo unitário do gate mais chamado do mod, e cinco itens de higiene. A instrumentação temporária (`PERF-INSTR`) entra na **mesma build**, gated pelo toggle `Perk Diagnostics` que já existe (default `false`), para que a Fase 4 possa validar num único ciclo.

**Achados cobertos:** `AUD-01-01` · `AUD-01-02` · `AUD-01-03` · `AUD-01-04` · `AUD-01-05` · `AUD-01-06` · `AUD-01-07` (a/b/c/d) · `AUD-01-08`.

**Lado afetado:** **client apenas**. Nenhum arquivo de `modded/Server/` é tocado — não há reinício de `SPT.Server` neste item.

## Comportamento atual

Por achado, o que o código faz hoje:

| Achado | Comportamento atual |
|---|---|
| `AUD-01-08` | `ClassIconCache.GetTinted` guarda um `Sprite` por chave `nome\|corTopo\|corBase`; cada chave nova cria uma `Texture2D` 256×256 (256 KB de VRAM) e um `Color32[65536]`. Nada é liberado até `Plugin.OnDestroy`. Mudar a cor de uma classe no F12 gera uma entrada nova, por dois consumidores (menu e aba CLASS). |
| `AUD-01-01` | `MenuClassIdentityPatch.ApplyToMenu` chama `GameObject.Find("MainMenuPlayerModelView")` (busca global na cena) **uma vez por frame, até 60 frames**, a cada `MenuScreen.Show` e a cada evento do picker de cor. Sem Menu-Overhaul instalado o painel nunca aparece e as 60 buscas rodam sempre. Depois espera 90 frames fixos e faz mais uma busca global. |
| `AUD-01-02` | `SkillMultipliers.IsLocalClass(string)` compara o nome da classe com `string.Equals(…, OrdinalIgnoreCase)`. 42 call-sites; `ClassMoveSpeed.Apply` avalia até 3 por leitura de `MaxSpeed`, e `MaxSpeed` é lido 3× por frame de movimento. |
| `AUD-01-03` | 4 patches Harmony em `Player.ApplyDamageInfo`, 4 em `ProceduralWeaponAnimation.Shoot`, 3 em `FirearmController.SetAnimatorAndProceduralValues`, 2 em `FirearmController.TotalErgonomics` — cada um resolve o próprio gate. Em `Shoot`, a ordem correta depende de 3 `[HarmonyPriority]` coordenados + o campo estático `RecoilFloorCapturePatch.StrBefore`. |
| `AUD-01-04` | `SilentKnifePatch` usa `FieldInfo.GetValue` + `PropertyInfo.GetValue` (reflexão crua) a cada clip de som não-arma de qualquer entidade. |
| `AUD-01-05` | Os logs `[053-tabicon]`/`[053-tabtext]` saem em `LogInfo`, sem gate de config (só um flag de uma-vez-por-sessão), montando a string com LINQ + `string.Join` antes de qualquer verificação. |
| `AUD-01-06` | `UnderbarrelMasteryXpPatch` detecta o hideout com `p.GetType().Name.IndexOf("Hideout", OrdinalIgnoreCase) >= 0`, por disparo. |
| `AUD-01-07a` | `Medroso.OnBulletFlyBy` avalia config + classe (comparação de string) para **cada bala com estampido sônico do mapa**. |
| ~~`AUD-01-07b`~~ | ~~`AdrenalineState.WatchWindow` faz `yield return null` a cada frame durante os 25 s da janela só para detectar o fechamento.~~ **DROPADO (PA-01-07)** — ganho de ~30 µs por janela contra atraso de 50 ms no re-sync do reload, alocação nova e divergência sob `timeScale`. Cai na proibição de micro-otimização de código frio (`spt-performance-analysis` §8). |
| `AUD-01-07c` | `SkillPanelPatch` monta uma string de tooltip nova a cada refresh de linha da lista de Skills (que recicla células no scroll). |
| `AUD-01-07d` | `PerkDiagnostics.AppendPerkList` chama `PerksCatalog.LocalGroups()` (LINQ + `ToArray`) a cada Repaint, quando o overlay está ligado. |

## Comportamento desejado

O **mesmo comportamento observável**, com o custo abaixo. Nenhuma mudança perceptível ao jogador é proposta — não há AC de mudança nesta rodada (ver "Exceção declarada").

| Achado | Mudança |
|---|---|
| `AUD-01-08` | O cache mantém no máximo **uma variante tingida viva por ícone** (a anterior é destruída) e a cor é **quantizada em múltiplos de 8 por canal** na chave. |
| `AUD-01-01` | A coroutine **desiste imediatamente** se o Menu-Overhaul não estiver carregado; cacheia o transform achado; busca a cada 3 frames em vez de todo frame; troca os 90 frames fixos por espera em tempo real. |
| `AUD-01-02` | A classe vira um **id de enum** resolvido 1× no fetch; cada gate passa a ser comparação de inteiros. A conversão é validada pelo compilador (membros de enum, não literais de string). |
| `AUD-01-03` | **Um patch por alvo**, resolvendo o gate uma vez e chamando os branches em ordem explícita no corpo. Em `Shoot`, o `str` original vira variável local e o campo estático some. |
| `AUD-01-04` | Acessor do emissor compilado 1× (`Expression.Lambda`), no molde que o `SainSoundPatch` já usa. |
| `AUD-01-05` | Logs gateados por `PerkDiag.Enabled` (molde do `DumpNativeTexts`, no mesmo arquivo). |
| `AUD-01-06` | `p is HideoutPlayer`. |
| `AUD-01-07a` | Resolvido de graça pelo `AUD-01-02` (o gate de classe vira comparação de inteiros). Sem mudança estrutural — a subscrição continua como está. |
| ~~`AUD-01-07b`~~ | **Nenhuma — dropado (PA-01-07).** `AdrenalineState.cs` fica exatamente como está. |
| `AUD-01-07c` | Tooltip cacheado por `(ESkillId, fator)`. |
| `AUD-01-07d` | Grupos de perk cacheados por classe enquanto o overlay estiver aberto. |

Adicionalmente, **instrumentação temporária** (`// PERF-INSTR`), gated por `Perk Diagnostics` (já existe, default `false`): INSTR-1 (contagem de buscas do menu), INSTR-2 (censo periódico das superfícies quentes) e INSTR-3 (crescimento do cache de textura).

## Critérios de aceite

> ⚠️ **Linha de base desconhecida (PA-01-04).** A memória do mod tem **duas pendências 🔴 abertas** dizendo que boa parte do que esta rodada refatora **nunca foi validada in-game**: **P-10.1** (validação dos ~21 efeitos 050.0–050.4) e **P-16.1** (fixes de movimento v0.2.4 + perks 072, em cliente Fika). Consequência para o contrato: onde o comportamento atual não é conhecido, o critério é **"idêntico ao build anterior"**, não **"funciona"** — um perk que já estava inerte deve continuar inerte, e isso **não** é regressão desta rodada.
>
> **Por isso a Fase 4 exige uma raid de baseline na DLL ATUAL antes de instalar a nova**, percorrendo a matriz de perks e anotando o que funciona hoje. Custa uma raid e transforma "não sei" em linha de base. O `05-asbuild` deve marcar quais ACs foram verificados contra base conhecida e quais contra base desconhecida.

### A. Não-regressão funcional (o contrato) — verificar in-game

- [ ] **`AUD-01-08` · identidade visual intacta:** o ícone da classe aparece com o mesmo gradiente no **menu principal**, no **nome do jogador (chat/lista)**, na **tela de deploy** e na **aba CLASS**. Trocar a cor de uma classe no F12 continua refletindo **ao vivo** nessas quatro superfícies.
- [ ] **`AUD-01-01` · menu inalterado:** com Menu-Overhaul instalado, o ícone + a linha do nome da classe + a cor de destaque (`AccentColor`) + o brilho do topo (PvE) continuam corretos ao abrir o menu e ao trocar a cor no F12. **Sem** Menu-Overhaul, o menu segue vanilla (o mod não deve tentar desenhar nada).
- [ ] **`AUD-01-01` · transições de tela (PA-02-04):** entrar no **inventário e voltar** ao menu principal, e **sair de uma raid** para o menu — a identidade da classe continua no painel **visível** nas duas transições. É o cenário em que um painel antigo desativado poderia sequestrar o cache (`GameObject.Find` só acha objetos ativos; uma referência cacheada sobrevive à desativação).
- [ ] **`AUD-01-02` · cada perk vale só para a sua classe:** percorrer as **6 classes + Peladão + um perfil vanilla** com o overlay 052 ligado, confirmando que os efeitos aparecem exatamente onde apareciam. Cobertura mínima por classe: Tanque (Couraça, Pack Mule, Bunker, Heavy Frame, Tireless Arms, recarga de escopeta), Fuzileiro (Adrenalina, Cool Under Fire, Loud Operator, Saque Rápido), Caçador (Rooted, Sharpshooter, Iron Lungs, Calm Sights, Steady Arms, Stalker, Light Frame), Furtivo (Execution, Rattled, Ghost Step, Morte Silenciosa, Light Frame), Saqueador (Lebre, Quick Hands, Pack Mule, Silent Looter, Medroso/tremor), Médico (Rapid Care, Swift Surgeon, Restorative Surgery, Efficient Metabolism, Shaky Hands, Rattled).
- [ ] **`AUD-01-03` · recuo idêntico:** o campo `Recoil str` do overlay 052 mostra o **mesmo antes→depois** nos dois piores casos do Anexo C do balance board: **Tanque + LMG + maestria alta** e **Fuzileiro na janela de Adrenalina**. O piso B15 continua mordendo quando o produto passaria dele.
- [ ] **`AUD-01-03` · recuo idêntico COM O RealRecoil ATIVO (PA-01-01):** repetir o teste acima com o RealRecoil carregado. É o único cenário que flagra a perda das prioridades de fronteira (`Priority.First` na captura, `Priority.Last` no piso) — o overlay 052 sozinho **não** pega, porque só mede a nossa cadeia.
- [ ] **`AUD-01-03` · dano idêntico:** Couraça (Tanque com colete classe ≥ mínimo) reduz dano; Execution (Furtivo com faca) multiplica dano de melee; Adrenalina abre a janela ao causar **e** ao receber dano; o tranco de câmera (Rattled/Cool Under Fire) **não** dispara em dano de queda.
- [ ] **`AUD-01-03` · troca de arma e recarga idênticas:** Saque Rápido (draw-in + put-away do holster) e recarga rápida de escopeta tubular (Tanque) continuam com a mesma sensação; a pistola **não** fica acelerada depois do saque (o reset do `Animator.speed` continua funcionando).
- [ ] **`AUD-01-04` · som da faca:** a faca do Furtivo continua muda (sacar + golpear + acertar); som de arma de fogo, granada, meds e quick-use continuam audíveis; em coop, a faca de um **peer** Furtivo continua muda no seu cliente.
- [ ] **`AUD-01-06` · maestria de underbarrel:** no shooting range do hideout o GP-25 **não** dá XP mas **tem** o efeito de recuo por nível; numa raid, **dá** XP.
- [ ] ~~**`AUD-01-07b` · Adrenalina**~~ — **dropado (PA-01-07)**: `AdrenalineState` não é tocado nesta rodada, então não há o que verificar. O achado foi registrado como ❌ Rejeitado no relatório de auditoria.
- [ ] **`AUD-01-07c` · marcadores de skill:** os marcadores `▲ +X%` / `▼ −X%` e os tooltips continuam corretos ao **rolar** a lista de Skills (a lista recicla células — o teste é rolar, não só abrir). **(PA-01-03)** Adicionalmente: **trocar de perfil** (classe diferente) sem reiniciar o cliente e reabrir a tela de Skills — o tooltip tem de nomear a classe **nova**, não a anterior. Mesmo teste ao trocar o **idioma do EFT**.
- [ ] **`AUD-01-08` · classe de cor clara (PA-01-02):** abrir a aba CLASS do **Saqueador** (`#c4ad45`, cujo canal R do topo do gradiente passa de 251) e confirmar que o brasão **não inverte** para tom escuro — o canal quantizado não pode estourar o byte.
- [ ] **Fika/multiplayer:** como **cliente** num raid coop com 2+ jogadores — identidade de classe por jogador na tela de deploy, perks de som de peers (Ghost Step / Loud Operator / Silent Looter) e faca muda de peer Furtivo continuam funcionando. Como **host**, os perks de som de peers continuam sendo aplicados contra a IA.
- [ ] **Integração com o ICM (PA-01-06):** em coop, um **Médico opera um aliado** pelo TRL-ImmersiveCombatMedicine e o HP máximo do membro é preservado conforme o perk Restorative Surgery. É o único teste que cobre as 4 assinaturas públicas que o ICM chama **por reflexão** — o compilador não protege essa fronteira.
- [ ] **Estado entre raids:** raid1 → extract → raid2, e também alt-F4 / morte / MIA — a raid nova não herda janela de Adrenalina, cooldown do tremor, flag de saque acelerado nem throttle de log de peer. (O mod reseta tudo no **start** da raid seguinte; conferir que continua assim.)

### B. Metas medíveis (instrumentação `PERF-INSTR`, com `Perk Diagnostics` ligado)

- [ ] **`AUD-01-08`:** arrastando o picker de cor de uma classe por ~5 s, `tintedCache` **para de crescer** — fica ≤ número de ícones distintos usados na tela (esperado: 1 por classe visível). Critério absoluto, não comparativo.
- [ ] **`AUD-01-01`:** o log `finds=` por abertura de menu mostra **`finds=0`** quando o Menu-Overhaul está ausente e **`finds` ≤ 20** quando presente (era até 60).
- [ ] **`AUD-01-02` / `AUD-01-03`:** o censo periódico (INSTR-2) mostra que a fração de chamadas que **passa** do gate permanece ~1/N (o gate não afrouxou), e que a contagem total de chamadas por superfície **não muda** — a mudança é de custo unitário e de número de gates, não de frequência.
- [ ] **`AUD-01-03`:** o número de execuções de gate por evento cai de **4 → 2** em `ApplyDamageInfo` (1 Prefix + 1 Postfix) e de **4 → 2** em `Shoot` (captura em `Priority.First` + aplicação em `Priority.Last`). ⚠️ **PA-01-01:** a meta original dizia "4 → 1"; consolidar em 1 destruiria as prioridades de fronteira que ordenam contra mods externos.
- [ ] **`AUD-01-05`:** com `Perk Diagnostics` **desligado**, `grep '053-tab' LogOutput.log` volta **vazio** após abrir a tela de Skills; com ligado, as linhas voltam.
- [ ] **Volume de log em raid:** o mod continua emitindo ~a mesma quantidade de linhas de uma raid normal (baseline conhecida: 9 linhas) com o diagnóstico desligado.

### C. Higiene da rodada

- [ ] Todo código alterado cita o achado no comentário inline (`// ref: AUD-01-NN`).
- [ ] Todo bloco de instrumentação está marcado `// PERF-INSTR AUD-01-NN — temporary, remove after validation` e é inerte com `Perk Diagnostics` desligado (custo = um branch, sem formatação de string).
- [ ] Compila com **0 erros** e sem warning novo (o warning pré-existente `CS8602` em `ClassMovementPatches.cs:95` pode desaparecer com a refatoração — se persistir, não é regressão).
- [ ] Nenhum arquivo de `modded/Server/` alterado.

## Corner cases

- [ ] **Perfil vanilla** (edition que não é classe do mod): `EClassId.None`; nenhum perk dispara, nenhuma identidade é desenhada, e o menu volta à cor original do Menu-Overhaul.
- [ ] **Classe desconhecida** (edition órfã, ou nome novo vindo do editor web que o enum não conhece): tem de degradar para `None` **com um aviso único**, nunca casar com a classe errada nem lançar. É o principal risco novo introduzido pelo `AUD-01-02`.
- [ ] **Menu-Overhaul ausente:** o `AUD-01-01` faz a coroutine desistir — conferir que isso **não** desliga a identidade nas outras superfícies (chat, deploy, aba CLASS), que não dependem dele.
- [ ] **Troca de classe pelo editor web entre raids:** o `Prefetch()` do raid-start precisa continuar re-resolvendo **o id de enum** junto com o nome — senão o jogador fica com os perks da classe antiga.
- [ ] **Duas classes mutuamente exclusivas no mesmo alvo consolidado:** em `SetAnimatorAndProceduralValues`, `ReloadSpeedPatch` (Fuzileiro) e `ShotgunReloadPatch` (Tanque) escalam o mesmo campo `BuffInfo.ReloadSpeed`. Hoje coexistem por serem classes exclusivas; consolidados, o `__state` único precisa salvar/restaurar **uma vez só**, sem duplo escalonamento.
- [ ] **Recursão em `Shoot`:** o patch consolidado não pode reintroduzir o problema que o `RecoilFloorCapturePatch`/`ApplyPatch` resolviam — o `str` original tem de ser capturado **antes** de qualquer multiplicador e o piso aplicado **depois** de todos, dentro da mesma invocação.
- [ ] **Ícone com cor quantizada:** a quantização em múltiplos de 8 muda o valor final da cor em até 3/255 por canal. Conferir que isso é **imperceptível** — se o usuário achar que mudou, o passo de quantização é o primeiro a reverter (ver "Exceção declarada").
- [ ] **Destruir a variante tingida anterior** enquanto uma `Image` ainda a referencia: a `Image` ficaria com sprite destruído (quadrado branco/rosa). O fix precisa garantir que quem referencia é re-apontado no mesmo evento que troca a cor.

## Fora de escopo

- [ ] **Qualquer alteração em `modded/Server/`** — inclusive o editor web Blazor.
- [ ] **`/analyze-memory-leak`** — a varredura completa de retenção/VRAM fica como frente própria (recomendação RV-03 da revisão).
- [ ] **A observação sobre `PartyInfoPanelPrefetchPatch`** (uso do `Reset()+EnsureLoaded()` destrutivo) — registrada no relatório 01 como fora do escopo de performance; não entra nesta rodada.
- [ ] **Remoção da instrumentação** — acontece na Fase 4, depois da validação, não aqui.
- [ ] **Qualquer ajuste de balance** — nenhum valor de perk, multiplicador ou default de gameplay muda nesta rodada.

## Exceção declarada (mudança perceptível)

Só há **uma** candidata, e é marginal: a **quantização de cor** do `AUD-01-08` arredonda cada canal RGB para o múltiplo de 8 mais próximo na chave do cache, o que muda a cor renderizada do ícone em até **3/255 por canal** (~1,2%). Trade-off: corta a cardinalidade do cache em ~32×. É imperceptível a olho nu, mas é uma mudança real e por isso está declarada aqui em vez de entrar como efeito colateral silencioso. **Se o usuário rejeitar**, o `AUD-01-08` continua resolvido só pela parte (a) — destruir a variante anterior do mesmo ícone —, que sozinha já limita o cache; a quantização é otimização adicional, não requisito.

## Referências

- [Relatório de auditoria 01 (`--perf`)](../../docs/relatorio-auditoria-codigo-01.md) — os 8 achados, o panorama de execução e o plano de validação
- [Revisão 01 do relatório](../../docs/relatorio-auditoria-codigo-01-review-01.md) — reclassificações e o achado `AUD-01-08`
- [balance-review-2026-07-05.md](../balance-review-2026-07-05.md) — Anexo C (piores casos de recuo, usados como critério de não-regressão do `AUD-01-03`)
- [PROPRIEDADES.md](../../PROPRIEDADES.md) — `Perk Diagnostics` é o gate de toda a instrumentação

## Histórico

| Data | Evento |
|---|---|
| 2026-08-22 | Item criado via `/optimize-mod-performance` Fase 2 (perfil de não-regressão) |
| 2026-08-23 | Revisão técnica 01 aplicada — nota de linha de base desconhecida (PA-01-04) e raid de baseline exigida na Fase 4; `AUD-01-07b` dropado (PA-01-07); meta do `AUD-01-03` corrigida de 4→1 para **4→2** (PA-01-01); ACs novos: recuo com RealRecoil ativo, classe de cor clara, troca de perfil/idioma no tooltip, cirurgia de aliado via ICM. |

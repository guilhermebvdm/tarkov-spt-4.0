# 007 — Sincronização de arquivos por pasta · Spec funcional

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Kickoff:** [007-sincronizacao-arquivos-00-kickoff.md](./007-sincronizacao-arquivos-00-kickoff.md)

> Item mais crítico do plano: risco de deleção/sobrescrita de arquivos do usuário. Toda regra abaixo é critério de aceite testável. Desvio de processo registrado: review 03 fundida na 04 pós-código (instrução do coordenador).

## Vocabulário

- **Manifesto** — lista de arquivos servida por `GET /launcher/mods/manifest` (path relativo à raiz do jogo, hash MD5, size).
- **Baseline** — `user/launcher/sync-state.json`: hash MD5 por path gravado após cada apply bem-sucedido. Representa "o estado que a última sync deixou no disco".
- **Igual/Customizado** — ⚠️ assunção interpretativa do kickoff (já registrada): "substituir os iguais" = local **igual ao baseline** da última sync (arquivo não foi tocado pelo usuário), **não** igual server×local. Local ≠ baseline ⇒ customizado.
- **Extra** — arquivo local dentro de uma pasta-regra que **não existe** no manifesto.

## Regras por pasta (critérios de aceite)

### R1 — `config` (PreserveDivergent)

| # | Estado local | Baseline | Resultado |
|---|---|---|---|
| R1.1 | Arquivo não existe | — | Baixa (novo) |
| R1.2 | hash local == hash server | qualquer | Nada a fazer; baseline atualizado p/ hash server |
| R1.3 | hash local ≠ server, **local == baseline** | tem entrada | Baixa (arquivo não customizado, server evoluiu) |
| R1.4 | hash local ≠ server, **local ≠ baseline** | tem entrada | **Preserva** + lista como "preservado" no resumo/manifesto de mudanças |
| R1.5 | hash local ≠ server, **sem entrada no baseline** (primeiro run) | sem entrada | **Preserva** (conservador: sem baseline, todo divergente é tratado como customizado) + lista |

Extras em `config` **não** são deletados nem movidos (regra não é espelho).

### R2 — `config-server` (MirrorDelete)

- R2.1 Arquivo do manifesto ausente/desatualizado → baixa (com exceção Dev Mode, ver R5).
- R2.2 **Extra** local → **deletado** (lixeira quando disponível na UI; ver spec técnica) e listado no manifesto de mudanças.
- R2.3 Proteções que impedem deleção de um extra: entrada em `ignoredFiles` do manifesto, `ExcludeFromCleanup` do settings, path presente no manifesto como opcional (mesmo de grupo desativado), path na lista de protegidos extra (ex.: `GetAllKnownOptionalPaths()`).

### R3 — `patchers` / `plugins` (MirrorMoveDisabled)

- R3.1 Arquivo do manifesto ausente/desatualizado → baixa (exceção Dev Mode, R5).
- R3.2 **Extra** local → **movido** para `<pasta>-disabled/` do usuário (ex.: `BepInEx/plugins/X.dll` → `BepInEx/plugins-disabled/X.dll`), preservando a subestrutura de pastas.
- R3.3 Colisão no destino (`-disabled` já tem o arquivo) → o destino é substituído pela versão recém-movida (a mais recente é a que vale).
- R3.4 Pastas `-disabled` **nunca** são varridas como pasta-regra (conteúdo não é re-deletado/re-movido em runs seguintes).
- R3.5 Mesmas proteções de R2.3 valem antes de mover.

### R4 — Resto (Default = comportamento atual)

- R4.1 Arquivo do manifesto ausente/desatualizado → baixa.
- R4.2 Extra dentro de `managedPaths` do manifesto → deletado (comportamento atual do launcher), com as proteções R2.3.
- R4.3 Arquivos fora de pasta-regra e fora de `managedPaths` → intocados.

### R5 — Dev Mode ON (lição da memória do repo: sync não pode reverter build local)

- R5.1 Arquivo com hash local ≠ manifesto **e** ≠ baseline (ou sem baseline) → **preservado em qualquer pasta-regra** + aviso acumulado no resumo ("N arquivos preservados por Dev Mode").
- R5.2 Extras em pastas espelhadas com Dev Mode ON → **preservados** + aviso (não deleta nem move builds de dev).
- R5.3 Dev Mode OFF → regra da pasta vale integralmente.
- Nota: o fluxo legado (ProfileViewModel) **pula a sync inteira** com Dev Mode ON; o motor novo sincroniza normalmente e só protege o que diverge do baseline — comportamento superior, registrado.

## Requisitos transversais

### 4.1.2 — Cancelamento

- C1 Botão Cancelar visível durante verificação **e** download.
- C2 Clicar → dialog de confirmação (ConfirmationDialog existente) com alerta de consequência: "a instalação pode ficar em estado parcial; uma nova verificação completará a sincronização".
- C3 Confirmado → interrompe **entre arquivos**; o arquivo em curso termina de forma atômica (temp + move) ou é descartado — nunca fica meio-escrito no destino.
- C4 Após cancelar: baseline reflete só os arquivos efetivamente aplicados; manifesto de mudanças gravado com o parcial + flag `cancelled`.

### 4.1.3 — Manifesto de mudanças

- M1 Após cada run (completo, com erros ou cancelado), grava `user/launcher/last-update.json` com: timestamp do run, contagens, e lista de entradas `{path, ação, timestamp}` (ações: updated, preserved, preserved-devmode, deleted, moved-to-disabled, error).
- M2 Contagem por pasta/ação exposta ao caller p/ a UI futura ("X arquivos foram atualizados").
- M3 Helper "abrir pasta" (explorer na pasta `user/launcher`) disponível no motor, chamável da UI depois.

### Robustez

- E1 Download falho (rede) → arquivo local intocado, erro contado, run continua nos demais.
- E2 Arquivo destino locked / disco cheio → apply daquele arquivo falha sem corromper o destino (escrita em temp; move só se temp OK); erro listado.
- E3 Apply atômico: escrever em `<dest>.sync-tmp` no mesmo diretório e `File.Move(overwrite)` — nunca escrita direta no destino.
- E4 Baseline e last-update.json são persistidos mesmo em erro/cancelamento (estado consistente para o próximo run).

## Corner cases (aceite)

| # | Cenário | Resultado esperado |
|---|---|---|
| CC1 | Primeiro run sem baseline | `config` inteiro tratado como customizado (R1.5); espelhos aplicam regra normal (mover p/ `-disabled` é não-destrutivo; delete de `config-server` segue R2 — card manda espelhar) |
| CC2 | Arquivo criado pelo usuário em pasta espelhada | `config-server`: deletado (R2.2). `plugins`/`patchers`: movido p/ `-disabled` (R3.2). Dev Mode ON: preservado + aviso (R5.2) |
| CC3 | Mod opcional desabilitado × espelho de `plugins` | Arquivos de grupos opcionais constam no manifesto → **não são extras** → nunca deletados/movidos, mesmo com grupo OFF (R2.3/R3.5) |
| CC4 | Dev Mode ON com build local de mod client | Preservado + aviso; nunca revertido (R5.1) |
| CC5 | Cancelamento no meio de um download | C3/C4: arquivo em curso atômico, resto pendente, estado gravado |
| CC6 | Disco cheio / arquivo locked | E2: destino não corrompido, erro listado, run segue |
| CC7 | Arquivo igual em disco mas sem baseline | R1.2: baseline é semeado com o hash (o baseline converge sem risco) |
| CC8 | Colisão no `-disabled` | R3.3: substitui |

## Interação com cleanup existente

- `GameStarter.SetupGameFiles` remove apenas artefatos fixos do EFT live (BattlEye, Logs, `EscapeFromTarkov_BE.exe`, etc.) — **sem interseção** com `config`/`config-server`/`patchers`/`plugins`/`-disabled`. Nenhuma mudança necessária (análise na spec técnica).
- A deleção de extras que hoje vive na `ProfileViewModel` (varredura de `managedPaths`) continua ativa até a integração (pendência P-007.1) — o motor novo embute essa responsabilidade com as proteções R2.3.

## Fora de escopo deste item

- Integração na `ProfileView`/`ProfileViewModel` (outro agente está nos arquivos): o link "X arquivos foram atualizados" na ProfileView e o roteamento do fluxo de login pelo motor novo ficam como **pendência P-007.1**.
- UI nova além do que o `ModUpdateViewModel` (sem view hoje) expõe: progresso, cancelar, resumo "X atualizados · Y preservados · Z movidos p/ disabled".

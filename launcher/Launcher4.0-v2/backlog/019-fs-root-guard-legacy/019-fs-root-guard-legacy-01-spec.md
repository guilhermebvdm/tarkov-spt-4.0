# 019 — Guard de raiz + atomicidade nos caminhos legados de FS · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./019-fs-root-guard-legacy-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (§B2, §Motor de sync)<br>

---

## Objetivo

Fazer os **dois** caminhos de mutação de filesystem que ficaram **fora** do `SyncEngine` herdarem as mesmas três garantias que o motor já aplica desde o CR-01-05 (revisão de código do item 007):

1. **Guard de raiz** — todo destino de write/delete/move passa por uma verificação de contenção sob o `GameRoot` (`ResolveUnderRoot`), que rejeita `..`, caminho absoluto e prefixo-irmão antes de tocar o disco.
2. **Write atômico** — escrita em arquivo temporário (`.sync-tmp`) no mesmo volume + `Move` com overwrite + rollback, em vez de `File.WriteAllBytes` direto.
3. **Deleção para a lixeira** — remoção via lixeira (recuperável), consistente com o resto do fluxo, em vez de `File.Delete` permanente.

Os dois caminhos são:

- **`deleteFiles` do manifesto** — `ProfileViewModel.cs:643-659` (lista carregada em `:599`), roda **automático em todo login** dentro de `CheckForUpdatesCore`, **antes** do bloco do motor (`:661`).
- **`OptionalModsHelper`** — `DownloadOptionalGroupAsync` (`:233-255`), `RemoveOptionalGroupAsync` (`:301-303` → `DeleteFileIfExists`/`File.Delete` em `:386-389`) e `DownloadFromOpcionaisFolder` (`:351-354`).

Este item **não** altera o comportamento observável de um servidor honesto: paths legítimos sob a raiz continuam a ser escritos/deletados como hoje. O item fecha o vetor de dano quando o manifesto (ou o payload de opcionais) chega adulterado, e elimina a janela de corrupção do write não-atômico.

## Modelo de ameaça (por que é 🔴 Blocker)

O servidor é a fonte da verdade de `deleteFiles`, dos paths de arquivos opcionais e de `targetSubDir`. Nenhum desses valores é validado no cliente hoje. Como `Path.Combine(gamePath, x)` **descarta** `gamePath` quando `x` é absoluto e resolve `..` para fora da raiz, um servidor comprometido/MITM (ou um pack mal montado) consegue:

- Apagar um arquivo arbitrário do SO/usuário via `deleteFiles: ["../../Windows/System32/..."]` ou `["C:/Users/.../algo"]` — em **todo login**, em **todos os clientes** (não só o host).
- Escrever bytes arbitrários fora da raiz via um arquivo opcional com `path` contendo `..` ou `targetSubDir` malicioso.

## Critérios de aceite (testáveis)

Cada critério é um teste unitário no projeto `SPT.Launcher.Tests` (xUnit) salvo onde indicado o oposto (gate in-game).

### CA-1 — `deleteFiles` com traversal não escapa da raiz
- **Given** o `GameRoot` `R` e a lista `deleteFiles = ["../../evil.txt"]`
- **When** o loop de `deleteFiles` processa a entrada
- **Then** nenhum arquivo fora de `R` é deletado, um `Warning` é logado com o path rejeitado, e o loop continua nas entradas seguintes.

### CA-2 — `deleteFiles` com caminho absoluto não escapa da raiz
- **Given** `deleteFiles = ["C:/Windows/System32/kernel32.dll"]` (ou, em teste, um arquivo real fora de `R`)
- **When** o loop processa a entrada
- **Then** o arquivo-alvo fora de `R` **permanece intacto** (não vai nem para a lixeira) e a rejeição é logada.

### CA-3 — `deleteFiles` legítimo continua funcionando (não-regressão)
- **Given** um arquivo real `R/BepInEx/plugins/x.dll` e `deleteFiles = ["BepInEx/plugins/x.dll"]`
- **When** o loop processa
- **Then** o arquivo vai **para a lixeira** (não `File.Delete` permanente) e some do disco.

### CA-4 — write de opcional com traversal não escapa da raiz
- **Given** um grupo opcional cujo manifesto traz `path = "../../evil.dll"`
- **When** `DownloadOptionalGroupAsync` processa o arquivo
- **Then** nada é escrito fora de `R`, a falha é logada como `Warning`, e os demais arquivos do grupo continuam a ser processados.

### CA-5 — write de opcional é atômico
- **Given** um destino legítimo `R/<rel>` que já existe com conteúdo antigo
- **When** o download grava o novo conteúdo
- **Then** em nenhum instante o arquivo fica truncado/parcial: ou tem o conteúdo antigo, ou o novo (temp + move); e nenhum resíduo `.sync-tmp` sobra após sucesso.

### CA-6 — `targetSubDir` malicioso do offFolder não escapa da raiz
- **Given** um grupo com `targetSubDir = "../../.."` (ou um `file.path` com `..`) em `DownloadFromOpcionaisFolder`
- **When** os arquivos de desativação são aplicados
- **Then** o destino efetivo é validado sob `R`; entradas que escapam são rejeitadas+logadas, sem escrever fora da raiz.

### CA-7 — remoção de opcional vai para a lixeira
- **Given** um grupo sem `offFolders` cujos arquivos existem sob `R`
- **When** `RemoveOptionalGroupAsync` remove os arquivos
- **Then** cada arquivo vai **para a lixeira** (recuperável), não é `File.Delete` permanente.

### CA-8 — o guard é fonte única (equivalência com o motor)
- **Given** os mesmos pares (`GameRoot`, path) usados pelo teste de traversal do motor (`SyncEngineTests.Download_path_with_traversal_does_not_escape_game_root`)
- **When** o guard extraído é chamado direto
- **Then** ele aceita/rejeita **exatamente** os mesmos paths que o `SyncEngine` — não há duas regras divergentes.

### CA-9 — build e testes verdes
- `dotnet build SPT.Launcher.csproj -c Release`, `dotnet build SPT.Launcher.Base.csproj -c Release` e `dotnet test SPT.Launcher.Tests.csproj -c Release` passam. Nunca rodar o `.exe` num gate automatizado.

## Regras de negócio

- **RN-1 — rejeição é silenciosa-para-o-usuário, ruidosa-no-log.** Um path que escapa a raiz **não** aborta o run inteiro nem mostra erro ao jogador: é pulado e logado como `Warning` (mesma política de tolerância-por-arquivo do motor). O jogo não deve travar porque o servidor mandou uma entrada podre.
- **RN-2 — "sob a raiz" = regra idêntica ao motor.** O critério é o do `ResolveUnderRoot` atual: `Path.GetFullPath(combine(root, rel))` precisa começar com `Path.GetFullPath(root) + separador` (comparação `OrdinalIgnoreCase`). O separador final protege o prefixo-irmão (`D:\SPT` não casa `D:\SPTevil`).
- **RN-3 — deleção sempre recuperável.** Todos os deletes desses caminhos usam a lixeira, com fallback para `File.Delete` só em `PlatformNotSupportedException` (não-Windows), igual ao `DeleteToRecycleBin` já existente.
- **RN-4 — sem baseline.** Estes caminhos **não** gravam baseline de sync (não são manifest entries do motor). O guard e a atomicidade são adicionados sem introduzir memória de estado — o comportamento de "reaparece se apagado" dos opcionais é preservado.
- **RN-5 — não-regressão para servidor honesto.** Nenhum path legítimo sob a raiz pode passar a ser rejeitado. Em particular, subpastas e casing preservados; entradas com `/` ou `\` normalizadas como hoje.

## Corner cases

| Caso | Comportamento esperado |
|---|---|
| `deleteFiles` com path absoluto (`C:\...`) | `Path.Combine` descarta a raiz → `GetFullPath` fica fora → guard rejeita, nada deletado |
| `deleteFiles` com `../` que ainda cai **dentro** da raiz (ex.: `a/../b.txt`) | resolve para `R/b.txt` → **aceito** (está sob a raiz); é traversal benigno |
| path que resolve para a **própria raiz** (`""` ou `.`) | rejeitado (não é arquivo sob a raiz; `File.Exists` falha de qualquer forma) |
| prefixo-irmão (`GameRootEvil`) | rejeitado pelo separador final do prefixo (RN-2) |
| symlink/junction plantado **dentro** da raiz apontando pra fora | **residual conhecido** — o guard atual (e o do motor, `SyncEngine.cs:248-258`) não resolve link; exige que o atacante já tenha write dentro da raiz. Fora do escopo duro (ver §Fora de escopo) |
| `targetSubDir` vazio | `baseDestPath = GamePath`; destino = `GamePath/<file.path>`, validado normalmente |
| entrada duplicada em `deleteFiles` | idempotente: 2ª passada não acha o arquivo (`File.Exists` false), no-op |
| arquivo travado pelo EFT em execução | `IOException` logada por-arquivo, run continua (RN-1) — igual ao motor |

## Nota de coop (Fika PVE)

- O loop de `deleteFiles` roda **em cada cliente** a cada login, não só no host. Solo=host mascara o alcance: um `deleteFiles` malicioso atinge **todos os peers**. O guard confina o dano ao que está sob a raiz em todos eles.
- **Residual de confiança-no-servidor (não resolvido por este item):** um `deleteFiles: ["BepInEx/plugins/Fika.Core.dll"]` é *sob a raiz* e portanto **aceito** pelo guard — mas apaga um plugin crítico de coop em todos os clientes. O guard trata *traversal*, não *intenção*. Sinalizar para o humano (§Gates) que a lista real de `deleteFiles` de produção precisa ser inspecionada.
- O write **não-atômico** de opcional é um risco de coop específico: um cliente que caia/desconecte no meio de um `File.WriteAllBytes` de DLL deixa o binário truncado → o cliente falha ao carregar o plugin (dessync que o host solo nunca veria). O write atômico (CA-5) fecha essa janela.

## Fora de escopo

- **MD5 → SHA-256** (correlato 🟢 do kickoff): migração de formato de hash toca manifesto + server + baseline (compat/migração), afetando 007/008/016/017. Item 019 é *guard de dados*; misturar a troca de hash infla o risco. **Recomendado item próprio** (decisão de produto — ver retorno).
- **Resolução de symlink/junction no guard** (`SyncEngine.cs:248-258`, 🟢 do AUDIT): endurecimento opcional; exige link pré-plantado dentro da raiz. Documentado como residual, não vira critério duro. Pode entrar como stretch técnico (ver 02-spec-tech §Riscos).
- **Teto em `managedPaths`** (`SyncPlanner.cs:264-274`, 🟢): é sobre a *largura* do delete do próprio motor, não sobre os caminhos legados. Fora do 019.
- **`GetServerBaseUrl` derrubando porta/esquema** (`OptionalModsHelper.cs:45-57`) e **thread-safety de `OnOptionalToggled`** (I/O+MD5 na UI thread): pertencem ao item **021** (mesmo arquivo, região diferente). 019 só troca as linhas de write/delete.

## Gates humanos

Regra do projeto: **escrita em arquivos SPT precisa de validação no jogo, não só build+hash.** Antes do deploy:

1. **Teste de adulteração (manual, fora do jogo).** Montar um manifesto de teste com `deleteFiles` contendo (a) `../../<arquivo-fora-da-raiz>` e (b) um caminho absoluto; confirmar no log que ambos foram **rejeitados** e que os arquivos-alvo fora da raiz permanecem intactos. Repetir com um `path` de opcional contendo `..`.
2. **Validação in-game (host).** Login real → sync → ativar um mod opcional (ON) → iniciar EFT e confirmar que o jogo **boota** e o mod carrega (write atômico não corrompeu DLL/config). Desativar (OFF) → confirmar que os arquivos foram para a **lixeira** e o jogo boota sem o mod.
3. **Validação in-game (cliente Fika).** Repetir o ciclo ON/OFF num **cliente** (não host) e confirmar que o coop ainda conecta — o write atômico e o delete-para-lixeira não deixaram binário parcial nem removeram plugin de coop.
4. **Inspeção de produção.** Antes de subir a DLL do servidor, inspecionar a lista real de `deleteFiles` do manifesto de produção e confirmar que **nenhuma** entrada legítima depende de `..`/absoluto (senão o guard passaria a rejeitá-la) e que nenhuma aponta para arquivo crítico de coop (residual RN da §coop).

## Gates automatizados

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet build SPT.Launcher.Base.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` — verdes. Nunca rodar o exe num gate.

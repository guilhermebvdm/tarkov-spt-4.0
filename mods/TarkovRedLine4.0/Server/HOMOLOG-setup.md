# Homolog namespaced no mesmo server (Opção B)

> **Data:** 2026-07-05<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Objetivo:** rodar uma variante de **homologação** do mod `TarkovRedLine.Server` **no mesmo server de produção** (`D:\SPT 4.0`, IP `100.106.152.7`), sem colidir com o mod de produção, para testar o launcher-em-desenvolvimento contra o ambiente real.<br>

---

## Como funciona

A variante homolog é o **mesmo código**, compilado com o define `HOMOLOG` (`-p:Homolog=true`). Isso muda, via `ModRouting` (`Controllers/ModRouting.cs`):

| | Produção (build normal) | Homolog (`-p:Homolog=true`) |
|---|---|---|
| Prefixo das rotas | *(nenhum)* | `homolog/` → ex. `/homolog/launcher/mods/manifest` |
| Pasta de conteúdo | `Launcher-Updater` | `Launcher-Updater-Homolog` |
| Arquivos de estado | `player_ips.json`, `redline_passwords.json`, logs | `*.homolog.json` / `*.homolog.txt` |
| `ModGuid` | `com.saraiva.tarkovredline` | `com.saraiva.tarkovredline.homolog` |
| Nome do mod | `TarkovRedLine-ServerMod` | `TarkovRedLine-ServerMod-Homolog` |
| Assembly / DLL | `TarkovRedLine.Server.dll` | `TarkovRedLine.Server.Homolog.dll` |

Como as **rotas têm prefixo diferente**, os dois mods coexistem no mesmo Kestrel sem `AmbiguousMatchException`. Como o **estado tem sufixo**, o homolog não encosta nos dados de produção (exceto os **profiles do SPT**, que são do processo — ver ⚠️).

O **launcher** liga o "Modo homolog" (Configurações → Ferramentas Dev), que prefixa **só as rotas do mod** com `/homolog` (as do SPT core — login/register/connect/customclasses — ficam intactas). Assim o **mesmo launcher** testa homolog e produção só trocando o toggle.

## ⚠️ O que NÃO fica isolado

Como é o **mesmo processo/server**, os **profiles do SPT** (`user/profiles/*.json`) são compartilhados com produção. Consequência prática: **testar troca de senha altera o profile real** na memória/disco. → Use uma **conta de teste** para exercícios de senha. Sync/versões/opcionais/self-update/hwid/player-ips são seguros (o estado próprio é sufixado; downloads vão pro cliente).

## Passo a passo (na máquina de produção `D:\SPT 4.0`)

1. **Gerar a DLL homolog** (nesta máquina de dev):
   ```bash
   dotnet build "mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/TarkovRedLine.Server.csproj" -c Release -p:Homolog=true
   # saída: bin-homolog/Release/TarkovRedLine.Server.Homolog.dll  (output separado do prod, sem clobber)
   ```
2. **Copiar** `TarkovRedLine.Server.Homolog.dll` (+ `.pdb`) para uma pasta de mod PRÓPRIA no server de prod:
   `D:\SPT 4.0\SPT\user\mods\TarkovRedLine.Server.Homolog\`
   (pasta separada da de produção — não misturar DLLs na mesma pasta.)
3. **Criar a pasta de conteúdo** `D:\SPT 4.0\Launcher-Updater-Homolog\` (cópia da de produção, ou um subconjunto pra teste). Mesma estrutura do `Launcher-Updater`: `config.json`, `server-version.txt`, `mods_repo/`, `Opcionais/`, `config-performance/`. Ver o exemplo em `C:\Escape From Tarkov\SPT-4.0\Launcher-Updater\README.md`.
4. **Reiniciar o SPT.Server** de produção (janela combinada — é o server coop compartilhado). No boot, os dois mods carregam: produção (`/…`) e homolog (`/homolog/…`).
5. **No launcher-dev** (nesta máquina): Configurações → Ferramentas Dev → ligar **"Modo homolog"**; apontar a URL do servidor pra `https://100.106.152.7:<porta>`; reconectar. Agora todo o fluxo do mod bate no homolog; o SPT core (login) continua no server real.

## Verificação rápida (com o server de prod no ar)

```bash
# produção (sem prefixo)
curl -k -s "https://100.106.152.7:<porta>/redline/server/version"
# homolog (com prefixo) — deve responder com a versão do Launcher-Updater-Homolog/server-version.txt
curl -k -s "https://100.106.152.7:<porta>/homolog/redline/server/version"
curl -k -s "https://100.106.152.7:<porta>/homolog/launcher/mods/manifest"
```

Se o `/homolog/...` responder e o `/...` de produção continuar respondendo, os dois coexistem.

## Riscos a validar no primeiro boot (do review adversarial)

O review confirmou o contrato das 18 rotas e a coexistência como **seguros** (assembly distinto → `Type` CLR distinto; templates de rota globalmente únicos pelo prefixo; sem `[Route(Name=…)]`; `FikaProfilePatch.Enable()` é no-op → sem dupla aplicação de patch). Restam 2 checagens **operacionais** no primeiro co-deploy:

1. **ApplicationParts dos dois assemblies.** Confirmar que o `SPTarkov.Server.Web` (4.0.2) registra os controllers de **ambos** os mods (o `ApplicationPartManager` padrão faz isso, mas não dá pra verificar o interno do pacote daqui). Teste: com o server no ar, `curl /homolog/redline/server/version` **e** `/redline/server/version` — se os dois respondem, ambos os ApplicationParts subiram. Se o `/homolog/...` der 404, o assembly homolog não foi registrado.
2. **Vote in-game não coberto pelo toggle.** O `HomologMode` só reescreve rotas do launcher. O plugin cliente (`RedLineRestart`) bate em `/redline/vote/*` fixo → em modo homolog ele ainda fala com o **vote de produção**. Inerente a um toggle só-launcher; vote/restart não é exercitado contra o homolog.

## Nota de build

Não rodar os builds prod e homolog **em paralelo** contra o mesmo `obj/` (compartilhado) — os arquivos gerados (`*.AssemblyInfo.cs`) correriam. Sequencial é seguro (o `AssemblyName` distinto já evita clobber do DLL; o output vai pra `bin/` vs `bin-homolog/`).

## Reverter / limpar

Apagar a pasta `user/mods/TarkovRedLine.Server.Homolog\` e reiniciar → volta só a produção. O `Launcher-Updater-Homolog` e os `*.homolog.*` podem ficar (inertes sem o mod).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-05 | Guilherme | Criação — setup do homolog namespaced (Opção B). |

# DEPLOY — TRL Items Management no servidor SPT oficial (Windows, serviço, localhost)

Guia pra rodar o **item viewer / editor de preço de flea** no servidor SPT oficial: outra máquina
Windows, SPT em outro disco, acessado pelo navegador **da própria máquina do server** (via AnyDesk),
**sempre ligado** como serviço Windows.

O tool é **Node puro, zero dependências** (só precisa de Node instalado). Ele **lê e escreve** a
config do SPT (`ragfair.json`, `globals.json`, `database/templates/items.json`) e atualiza
`checks.dat`. As edições só aplicam **quando o servidor SPT reinicia** (o SPT valida o `checks.dat`
e parseia a config no boot).

> ⚠️ **`SPT_PATH` tem que ser a RAIZ do install** — a pasta que **contém** `SPT_Data/` (ex.:
> `E:\SPT\SPT`). O `serve.js` faz `join(SPT_PATH, 'SPT_Data')` fixo; apontar direto pra `SPT_Data`
> quebra o viewer.

> 🔒 **Segurança:** o serviço faz bind em `127.0.0.1` (loopback) e **não tem autenticação**. Quem
> tem o desktop do server (AnyDesk/RDP/físico) pode mudar preços, banir itens e mexer no nível do
> flea ao vivo. Não exponha na LAN (`ITEMDB_HOST=0.0.0.0`) sem colocar auth na frente.

---

## Pré-requisitos no server

1. **Node.js LTS (20 ou 22)** — instalar do nodejs.org. Anote o caminho do `node.exe`
   (padrão `C:\Program Files\nodejs\node.exe`).
2. **NSSM** (gerenciador de serviço) — `winget install nssm` ou baixar de nssm.cc.
   *(Alternativa sem NSSM: Task Scheduler — ver no fim.)*
3. SPT instalado e funcional na máquina; saber a **raiz** do install (contém `SPT_Data/`).
4. Acesso ao desktop do server (AnyDesk).

## Passo a passo

### 1. Conferir a versão do SPT
`SPT_Data/configs/core.json` → **`compatibleTarkovVersion`** deve ser **0.16.x** (EFT). A versão do
SPT server aparece no banner/log de boot — deve ser **4.0.x**. Este tool foi validado pra SPT 4.0.x /
EFT 0.16.x; se divergir em major/minor, valide os preços com cautela antes de confiar.
Confirme também que **`SPT_Data/checks.dat` existe** (todo install real tem).

### 2. Copiar o tool pro server
Empacote só os arquivos versionados (a partir do repo, na máquina de dev):
```bash
git archive --format=zip -o trl-items-management.zip HEAD:tools/trl-items-management
```
(O archive já extrai como `trl-items-management/` — nome igual ao do repo, pronto pra usar.)
Copie o zip pro server e extraia, ex.: `E:\tools\trl-items-management`. **Remova** do pacote:
- `data/items.json` (placeholder do dev box — será regerado no passo 4; sem ele o viewer fica vazio
  até o build rodar);
- scripts de teste/diagnóstico `scripts/*smoke*.js`, `scripts/action0-*.js`,
  `scripts/verify-trader-*.js` (não-runtime).

Crie a pasta de logs: `New-Item -ItemType Directory -Force E:\tools\trl-items-management\logs`.

### 3. Backup da config do SPT (antes de qualquer edição!)
O `install-service.ps1` faz isso automaticamente (se ainda não existir). Manual seria copiar pra
`*.bak`: `SPT_Data\configs\ragfair.json`, `SPT_Data\database\globals.json`,
`SPT_Data\database\templates\items.json`, `SPT_Data\checks.dat`.

> 🔑 **API key do tarkov-market — a forma mais simples (recomendada):** crie um arquivo **`.env`**
> na raiz do tool (`E:\tools\trl-items-management\.env`) com uma linha:
> ```
> TARKOV_MARKET_API_KEY=sua-key-aqui
> ```
> O `load-env.js` carrega automático em **qualquer** start (build, serviço, per-item, bulk) — sem
> env var do sistema nem do parâmetro `-MarketKey`. O `.env` é **gitignored** (não vai no pacote/git),
> então você cria um novo no server. É **opcional**: só afeta a coluna do tarkov-market — editar preço,
> ban, flea-level e tarkov.dev funcionam sem ela. *(Alternativas: `-MarketKey` no install-service.ps1,
> ou env var do sistema.)*

### 4. Regerar `data/items.json` pro install oficial (obrigatório)
```powershell
$env:SPT_PATH = "E:\SPT\SPT"                 # a RAIZ que contém SPT_Data
$env:TARKOV_MARKET_API_KEY = "<sua-key>"     # opcional — só pra coluna tarkov-market
& "C:\Program Files\nodejs\node.exe" scripts\build.js
```
Confira no log do `load-spt` o "SPT data dir" apontando pro disco do server.
**Offline / sem key:** o `build.js` aborta no fetch de market. Em vez dele: copie
`cache\tarkov-dev-raw.json` + `cache\tarkov-market-raw.json` da máquina de dev pra `cache\` aqui, e
rode `node scripts\load-spt.js` + `node scripts\normalize.js` (os caches seedados só afetam as
colunas de display dev/market — não a edição nem o que é escrito no SPT).

### 5. Instalar o serviço
```powershell
.\scripts\install-service.ps1 `
  -NodeExe "C:\Program Files\nodejs\node.exe" `
  -ToolDir "E:\tools\trl-items-management" `
  -SptPath "E:\SPT\SPT" `
  -Port 8080 `
  -MarketKey "<sua-key>"     # opcional
```
O script faz o backup (passo 3) se faltar, instala o serviço **TRLItemsManagement** (auto-start,
restart-on-crash, logs em `logs\service-*.log`, bind em `127.0.0.1`) e inicia.

### 6. Acessar
No desktop do server (AnyDesk), abra o navegador → `http://127.0.0.1:8080/TRLItemsManagement/`.
Confirme que a lista carrega e que uma edição de preço + **Restore** funcionam.

### 7. Mod de preços de trader (`TRLTraderPrices`) — necessário pra editar preço de trader

A edição de **preço de flea** (passos acima) escreve direto na config do SPT e não precisa de mod. Já a
edição de **preço de venda de trader** (a coluna do trader no viewer) é aplicada por um **companion server
mod** em C#: o viewer grava `user/mods/TRLTraderPrices/config/overrides.json` e o mod reescreve a assort do
trader no boot. **Sem o mod instalado, o viewer salva o override mas avisa "mod not installed" e nada aplica
no jogo.**

1. **Copiar o pacote do mod pro server.** Na máquina de dev, o pacote pronto fica em
   `mods/TRLTraderPrices/builds/TRLTraderPrices-vm-deploy.zip` (só a DLL + .pdb; ~20 KB). Copie pro server.
   *(Regerar o pacote, se faltar: `bash .agents/scripts/compile-mod.sh TRLTraderPrices` e zipe
   `mods/TRLTraderPrices/builds/server/TRLTraderPrices.dll` dentro de uma pasta `TRLTraderPrices/`.)*
2. **Extrair em `user/mods/`.** O zip já tem a estrutura `TRLTraderPrices/` — extraia direto na pasta
   `user/mods/` do install (ex.: `E:\SPT\SPT\user\mods\`), resultando em
   `E:\SPT\SPT\user\mods\TRLTraderPrices\TRLTraderPrices.dll`.
3. **NÃO copie `config/overrides.json`.** É user-data: o viewer cria a pasta `config/` e o arquivo na
   primeira edição de trader. (Por isso o pacote não traz config — shipar um default `{}` já fez o deploy
   sobrescrever os overrides ao vivo no passado.)
4. **Reiniciar o SPT.** O mod aplica os overrides no boot, ANTES do flea gerar as ofertas, então a compra
   direta no trader E o preço dele no flea refletem o override. O log mostra
   `[TRLTraderPrices] applied N entries (...)` no boot.

> O `SPT_PATH` do viewer (passo 5) já aponta pra raiz do install, então ele acha
> `user/mods/TRLTraderPrices/config/` automaticamente — nada extra a configurar.

> **Limitação conhecida:** ofertas de trader **quest-locked** (marcadas com 🔒 no viewer) só aplicam o
> override **depois** que a quest correspondente é concluída (até lá o item fica no quest-assort, fora da
> assort viva que o mod edita). O mod ignora com segurança o que não está na assort viva (log `tplNotSold`).

---

## Workflow operacional

- **Começa do zero:** o ragfair do server oficial tem só o que ele já tinha; edições feitas em outra
  máquina não vêm junto — você define os preços aqui pelo viewer.
- Editar preço / ban / nível do flea → o tool escreve `configs/ragfair.json`,
  `database/templates/items.json`, `database/globals.json` e atualiza `checks.dat` (writes atômicos).
- **Aplicar = reiniciar o servidor SPT.** As mudanças só entram no próximo boot. É seguro editar com
  o SPT ligado (só aplica no restart); evite editar **durante** a fase de boot do SPT.
- **Rescan** repega itens de mods novos (sem internet). **Refresh dev/market** repuxa preços online
  (market exige a key). Sem rede, esses botões falham na UI mas o serviço continua de pé.
- **Reverter desastre:** restaurar os `*.bak` do passo 3 e reiniciar o SPT.

## Troubleshooting

- **Viewer abre mas lista vazia / 404 em `/data/items.json`** → o build do passo 4 não rodou; rode-o.
- **Edição retorna 500** → a conta do serviço não tem write no disco do SPT. LocalSystem (padrão do
  NSSM) resolve disco local; conta restrita precisa de Modify em `SPT_Data\`. Veja `logs\service-err.log`.
- **SPT reprova validação no boot após uma edição interrompida** → `checks.dat` pode ter truncado
  (write não-atômico). Refaça 1 edição/Rescan pra regenerar, ou restaure `checks.dat.bak`.
- **Porta ocupada** → escolha outra porta (`-Port`). O SPT usa 6969; não conflita com 8080.

## Alternativa sem NSSM (Task Scheduler)

```powershell
$node = "C:\Program Files\nodejs\node.exe"; $tool = "E:\tools\trl-items-management"
[Environment]::SetEnvironmentVariable('SPT_PATH','E:\SPT\SPT','Machine')
[Environment]::SetEnvironmentVariable('ITEMDB_HOST','127.0.0.1','Machine')
# [Environment]::SetEnvironmentVariable('TARKOV_MARKET_API_KEY','<key>','Machine')  # opcional
$action  = New-ScheduledTaskAction -Execute $node -Argument "`"$tool\viewer\serve.js`" 8080" -WorkingDirectory $tool
$trigger = New-ScheduledTaskTrigger -AtStartup
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -RestartCount 999 -RestartInterval (New-TimeSpan -Minutes 1) -ExecutionTimeLimit ([TimeSpan]::Zero)
Register-ScheduledTask -TaskName "TRLItemsManagement" -Action $action -Trigger $trigger -Principal $principal -Settings $settings
Start-ScheduledTask -TaskName "TRLItemsManagement"
```
Limitações vs NSSM: sem captura de stdout/stderr embutida e restart mais grosseiro. Faça o backup
do passo 3 manualmente. Lembre que o backup também é feito pelo `install-service.ps1` (rota NSSM).

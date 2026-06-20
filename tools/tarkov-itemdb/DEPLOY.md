# DEPLOY — tarkov-itemdb no servidor SPT oficial (Windows, serviço, localhost)

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
git archive --format=zip -o tarkov-itemdb.zip HEAD:tools/tarkov-itemdb
```
Copie o zip pro server e extraia, ex.: `E:\tools\tarkov-itemdb`. **Remova** do pacote:
- `data/items.json` (placeholder do dev box — será regerado no passo 4; sem ele o viewer fica vazio
  até o build rodar);
- `viewer/profiles*.{html,js,css}` e `data/profiles-meta.json` (Profile Viewer — depende do mod
  RZCustomProfiles, fora do escopo de preço; a rota `/viewer/profiles*` só fica 404, inofensivo);
- scripts de teste `scripts/*smoke*.js`, `scripts/action0-*.js` (não-runtime).

Crie a pasta de logs: `New-Item -ItemType Directory -Force E:\tools\tarkov-itemdb\logs`.

### 3. Backup da config do SPT (antes de qualquer edição!)
O `install-service.ps1` faz isso automaticamente (se ainda não existir). Manual seria copiar pra
`*.bak`: `SPT_Data\configs\ragfair.json`, `SPT_Data\database\globals.json`,
`SPT_Data\database\templates\items.json`, `SPT_Data\checks.dat`.

> 🔑 **API key do tarkov-market — a forma mais simples (recomendada):** crie um arquivo **`.env`**
> na raiz do tool (`E:\tools\tarkov-itemdb\.env`) com uma linha:
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
  -ToolDir "E:\tools\tarkov-itemdb" `
  -SptPath "E:\SPT\SPT" `
  -Port 8080 `
  -MarketKey "<sua-key>"     # opcional
```
O script faz o backup (passo 3) se faltar, instala o serviço **TRLItemsManagement** (auto-start,
restart-on-crash, logs em `logs\service-*.log`, bind em `127.0.0.1`) e inicia.

### 6. Acessar
No desktop do server (AnyDesk), abra o navegador → `http://127.0.0.1:8080/viewer/`.
Confirme que a lista carrega e que uma edição de preço + **Restore** funcionam.

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
$node = "C:\Program Files\nodejs\node.exe"; $tool = "E:\tools\tarkov-itemdb"
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

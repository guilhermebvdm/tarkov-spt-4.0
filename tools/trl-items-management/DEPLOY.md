# DEPLOY — TRL-ItemsManagement no servidor SPT oficial (Windows, dentro do próprio SPT)

Guia pra instalar/atualizar o **mod TRL-ItemsManagement** (editor de preço de flea/trader, ban,
nível do flea, refresh/rescan) no servidor SPT de produção: outra máquina Windows, SPT em outro
disco, UI acessada pelo navegador **da própria máquina do server** (via AnyDesk).

> ⚠️ **Arquitetura mudou.** Até a v1.0.0 do mod, isso rodava como um app **Node separado**
> (`viewer/serve.js`, porta 8080, serviço NSSM/Task Scheduler) + um mod companion
> (`TRLTraderPrices`) só pra aplicar preço de trader. **Agora é um único mod C#**
> (`mods/TRL-ItemsManagement/`) que serve a própria UI de dentro do processo do **SPT.Server**
> (mesma porta do jogo, ex. `6969`) — não tem mais processo Node persistente; o Node só é
> chamado sob demanda (rescan/refresh) pelo próprio mod via `Process.Start`. Se a VM ainda tem o
> setup antigo, **não precisa desmontar nada na mão** — o `update-vm.ps1` faz a migração sozinho
> (ver "Migração automática" abaixo).

O mod **lê e escreve** a config do SPT (`ragfair.json`, `globals.json`,
`database/templates/items.json`) e atualiza `checks.dat`. As edições de flea/ban/nível só aplicam
**quando o servidor SPT reinicia** (o SPT valida o `checks.dat` e parseia a config no boot); edição
de ban de item de **mod** e trader **buy/sell** também exigem restart (ver
[docs/validacao-endpoints-api.md](docs/validacao-endpoints-api.md) do mod pro detalhe de cada
endpoint).

> 🔒 **Segurança:** a UI sobe no bind da própria Kestrel do SPT (mesmo listener usado por
> clientes Fika em coop) e **não tem autenticação**. Quem tem acesso à porta pode mudar preços,
> banir itens e mexer no nível do flea ao vivo. Risco aceito enquanto o bind for loopback-only;
> reavaliar se a porta for exposta em LAN/Tailscale pra jogar coop.

---

## Pré-requisitos no server

1. SPT 4.0.x / EFT 0.16.x instalado e funcional — confirme a versão como sempre
   (`SPT_Data/configs/core.json:compatibleTarkovVersion`) e que `SPT_Data/checks.dat` existe.
2. **Node.js LTS (20 ou 22)** — só pra rodar os scripts do pipeline sob demanda (rescan/refresh),
   não sobe mais como serviço. Anote o caminho do `node.exe` (padrão
   `C:\Program Files\nodejs\node.exe`).
3. Acesso ao desktop do server (AnyDesk).

## Instalar / atualizar (1 comando)

**No dev box** — gerar o bundle (a versão é lida do `<Version>` do csproj do server; o conteúdo
vem do install local em `D:\SPT`/`.spt-path`, então rode com esse install já testado e limpo):
```bash
bash tools/trl-items-management/scripts/package-release.sh D:/SPT/_vm-deploy
# → D:/SPT/_vm-deploy/trl-release-v<versão>.zip — espelha a estrutura real de pastas do SPT:
#   {BepInEx/plugins/TRL-ItemsManagement, SPT/user/mods/TRL-ItemsManagement,
#    trl-items-management-pipeline, update-vm.ps1}
```

**Na VM** — abra o PowerShell **como Administrador** (Run as Administrator), copie o zip
(AnyDesk), extraia e rode o updater de dentro da pasta extraída:
```powershell
Expand-Archive "D:\_deploy\trl-release-v1.0.0.zip" "D:\_deploy" -Force
cd "D:\_deploy\trl-release-v1.0.0"
powershell -ExecutionPolicy Bypass -File .\update-vm.ps1
```
> Sem elevação o script ainda roda, mas se o setup antigo (serviço NSSM ou Scheduled Task do
> viewer) tiver sido registrado como SYSTEM, a limpeza automática pode falhar em silêncio — o
> script avisa no início se detectar que a sessão não está elevada.
O `update-vm.ps1` faz tudo, **idempotente**: para o SPT (e o setup antigo, se ainda presente —
ver abaixo) → instala DLL+wwwroot do server em `user\mods\TRL-ItemsManagement\` (preserva
`config\` e `data\`) → instala a DLL do client em `BepInEx\plugins\TRL-ItemsManagement\` →
garante `config\pipeline.json` → atualiza o pipeline Node (preserva `.env`, `cache\`, `logs\`) →
regenera o catálogo (`data\items.json` etc., via `load-spt`+`normalize`, usando o cache de
preços) → sobe o SPT.
- Caminhos diferentes? passe `-SptPath`, `-GameRoot`, `-ToolDir`, `-NodeExe`.
  `Get-Help .\update-vm.ps1 -Full` (cabeçalho do arquivo).
- 1ª vez sem cache de preços (ou pra repuxar tarkov.dev/market): adicione **`-Fetch`** (precisa
  internet; a coluna de tarkov-market também precisa de `.env` com `TARKOV_MARKET_API_KEY` —
  sem a key só essa coluna fica ausente, o resto funciona normal).
- Só atualizar sem subir o jogo: **`-NoStartGame`**.

## Migração automática (do setup antigo Node + TRLTraderPrices)

Se a VM ainda tem `user\mods\TRLTraderPrices\` (o mod companion antigo), o `update-vm.ps1`
**detecta e migra sozinho, na mesma run**, sem exigir nenhum passo manual:
1. Para/remove o serviço NSSM ou a Scheduled Task do viewer antigo (nome `TRLItemsManagement`),
   se existir, e mata qualquer `node.exe` que esteja rodando `serve.js` na mão.
2. Faz backup de `TRLTraderPrices\config\{overrides.json,buy-overrides.json}` e **move** (não
   apaga) a pasta inteira pra `_removed-mods\TRLTraderPrices\` — mover pra FORA de `user\mods\` é
   necessário porque o SPT varre toda subpasta por conteúdo, não por nome; deixar renomeada no
   lugar mantém os dois mods (antigo + novo) patchando `TradeHelper.SellItem` ao mesmo tempo, com
   ordem indefinida entre os Harmony Prefixes.
3. Copia os overrides preservados pro `config\` do mod novo — **só se ele ainda não tiver um**
   (não sobrescreve edição feita depois pela UI nova, em nenhuma run seguinte).

Preços/ban/nível de flea editados pelo viewer antigo vivem direto em `SPT_Data` (`ragfair.json`,
`items.json`, `globals.json`) — **não em pasta de mod nenhuma**, então sobrevivem sozinhos a
qualquer uma dessas trocas; o script nunca toca em `SPT_Data` além de ler pra regenerar o
catálogo.

> **Limitação conhecida (herdada):** ofertas de trader **quest-locked** só aplicam o override
> **depois** que a quest correspondente é concluída (até lá o item fica fora da assort viva que o
> mod edita).

## Acessar

No desktop do server (AnyDesk): `https://127.0.0.1:6969/TRLItemsManagement-Server/index.html`
(cert self-signed do próprio SPT — aceite o aviso do navegador). Confirme que a lista carrega e
que uma edição de preço + restauração funcionam.

## Workflow operacional

- **Aplicar = reiniciar o servidor SPT.** Sell/ban/flea-cap/flea-level/buy-overrides só entram no
  próximo boot — exceção: ban de item de **mod** aplica na hora (muta o banco ao vivo, além de
  persistir). É seguro editar com o SPT ligado; evite editar **durante** a fase de boot.
- **Rescan** repega itens do DB do SPT (sem internet). **Refresh dev/market** repuxa preços online
  (market exige a key). **Refresh-all** repuxa o catálogo inteiro de uma fonte — mais lento.
- **Reverter desastre:** restaurar os backups de `SPT_Data` (se você mantém rotina própria) e
  reiniciar o SPT; pra reverter a MIGRAÇÃO em si, o `TRLTraderPrices` original está intacto em
  `_removed-mods\TRLTraderPrices\` e os overrides no backup `_backup-migration-items-management-*\`.

## Troubleshooting

- **UI abre mas lista vazia** → o passo de regenerar catálogo (5/6 do `update-vm.ps1`) falhou;
  rode de novo com `-Fetch` se for a 1ª vez sem cache de preços salvo.
- **404 na UI ou nos `/api/*`** → confira se o `AssemblyName`/prefixo bate
  (`TRLItemsManagement-Server`) e se o SPT subiu sem erro de `ModValidator` no log.
- **Edição retorna 500** → a conta rodando o SPT.Server não tem write em `SPT_Data\` ou em
  `user\mods\TRL-ItemsManagement\`. Veja o log do SPT.
- **SPT reprova validação no boot após uma edição interrompida** → `checks.dat` pode ter truncado
  (write não-atômico). Refaça 1 edição/Rescan pra regenerar.
- **Porta ocupada** → não deveria mais acontecer (não sobe porta própria); confirme que não
  sobrou o serviço/task do viewer antigo tentando subir na 8080.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-08 | Guilherme | Reescrito pra arquitetura pós-unificação (`mods/TRL-ItemsManagement/`, mod único servindo a própria UI dentro do SPT.Server) — versão anterior descrevia o viewer Node standalone (porta 8080, NSSM) + `TRLTraderPrices` companion, ambos aposentados. `update-vm.ps1`/`package-release.sh` reescritos junto, com migração automática do setup antigo. |
| 2026-07-08 | Guilherme | Duas rodadas de revisão crítica em `update-vm.ps1`/`package-release.sh` (testadas ponta a ponta contra uma VM fake isolada, `SPT_Data` real montado por junction NTFS): robocopy com `/R:5 /W:2` (default do robocopy trava horas em vez de falhar rápido), aviso de elevação (limpeza do setup antigo pode falhar em silêncio sem admin), `sc.exe delete` agora checa exit code, parse de `.spt-path` tolerante a CRLF, checagem de `wwwroot/` antes de empacotar, clobber simétrico no install do client, mensagens de erro mais específicas pra `-SptPath`/`-GameRoot` trocados. Instrução de rodar o PowerShell elevado adicionada aqui. |

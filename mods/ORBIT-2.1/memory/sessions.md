# Memory — ORBIT-2.1

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados). Cada entrada resume o que foi feito. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero.

## Estado atual (snapshot ao fim da última sessão)

**Port do ORBIT 2.1.0 (upstream para SPT 4.1) para SPT 4.0.13: compila e passa as verificações fora do jogo; NÃO validado em jogo; NÃO instalado.** O jogo em `D:\SPT` segue com o ORBIT 1.2.1.

- **Identidade:** cliente `ORBIT.dll` 2.1.0 (GUID `com.chazut.orbit`, o mesmo do 1.2.1), addon `Orbit.Fika.dll` 1.1.0, servidor `Orbit.Server.dll` 2.1.0 (GUID `com.chazut.orbit.server`, `SptVersion ~4.0.0`, `net9.0`). Upstream `Chazut/ORBIT` branch `2.1` @ `8fd7e661`.
- **Forma do port:** o fonte mantém os nomes 4.1; a diferença fica em `modded/Orbit/Compat/` (`Spt40TypeAliases.cs`, `Spt40TypeNames.cs`, `Spt40Members.cs`) e `modded/Orbit.Server/Compat/Spt40ServerCompat.cs`, mais edições pontuais. 53 arquivos, +496/−187 contra o upstream.
- **Build:** `/compile-mod ORBIT-2.1 --no-install`. As referências vêm do `.spt-path` via `modded/Directory.Build.props`.
- **Verificação:** `scripts/verify-port.sh` (patches + nomes em texto) e `scripts/server-smoke-test.ps1` (servidor). Estado em 2026-09-30: os dois passam.
- **Instalação:** `scripts/install-to-spt.sh` (move o 1.2.1 para `BepInEx/plugins-disabled/`); `--rollback` desfaz.
- **Itens de backlog:** 001 🟡 (artefatos 01 a 05 em `backlog/001-downgrade-spt41-spt40/`; falta só a validação em jogo) e 002 ⚪ (pacotes Fika do addon no padrão AP-11).

## Pendências / próximos passos conhecidos

- 🔴 [P-1.1] (aberta 2026-09-30) **Validação em jogo do port.** Roteiro em 7 passos no fim do as-built. Critérios 4 a 8 da spec funcional não foram exercidos. Log esperado: `ORBIT 2.1.0 (SPT 4.0 port) fully loaded`, `body guards ready (32 entry points)`, `scripted medicine guard ready`, `scoped resume guard ready`, sem `failed to enable` nem `unavailable`.
- 🔴 [P-1.2] (aberta 2026-09-30) **Coop/Fika não exercido.** Addon compila e os 4 postfixes ligam contra Fika.Core 2.3.21, mas nenhuma raid com host/headless + cliente foi feita. Host, headless e clientes precisam das mesmas DLLs. O servidor de produção pode ter SAIN e Fika em versões diferentes das de `D:\SPT`: rodar `verify-port.sh --spt-path` contra aquela instalação antes de subir.
- 🟡 [P-1.3] (aberta 2026-09-30) **Cinco métodos sem prova do lado 4.1:** `BotDoorOpener` `TryPassCurrentDoor`→`method_2`, `RunEnteringDoorSequence`→`method_7`, `WaitForDoorOpen`→`method_0`, `InteractionWithDoor`→`method_8`, e `BotFirstAid.ApplyToSelf`→`method_3`. Escolhidos pelo corpo do método 4.0 por dois leitores independentes, com o mesmo resultado. `WaitForDoorOpen` é o menos certo. Sintoma se errado: bot nativo dormindo executa (ou deixa de executar) uma etapa de porta.
- 🟡 [P-1.4] (aberta 2026-09-30) **Integrações com mods ausentes de `D:\SPT` não verificadas:** RUAF, Black Division, ISB, Combine Soldiers, RoguesVRaiders, InterchangeRework, mapas do Manimal, Fika headless. Sem o mod a integração fica inerte; com o mod em versão diferente ela se desliga com aviso no log.
- 🟡 [P-1.5] (aberta 2026-09-30) **Lacuna de coop herdada do upstream:** corpo de jogador humano remoto não gera ponto de corpo no host (`ObservedPlayer.CreateCorpse` do Fika não chama a base; igual no Fika atual). Não é efeito do port. CR-01-12.
- 🟡 [P-1.8] (aberta 2026-09-30) **Pacotes do addon Fika fora do padrão AP-11** (item 002): `OrbitDoorPacket` e `OrbitGhostFightPacket` leem com `Get*`, sem envelope de comprimento nem flag `Valid`; `DoorSyncBridge` chama `UnregisterPacket`. Herdado do upstream. Só a captura de exceção do callback foi corrigida. Sem colisão de hash com os outros mods (38924, 43571).
- 🟢 [P-1.6] (aberta 2026-09-30) **Launcher TRL:** com Dev Mode desligado o sync pode repor o ORBIT 1.2.1 do manifesto do servidor e desfazer a build local; para produção o 2.1 precisa entrar no manifesto e o 1.2.1 sair.
- 🟢 [P-1.7] (aberta 2026-09-30) **Exportação de ZIP de preset não executada** depois da correção do caminho (CR-01-05); só o texto da página foi conferido.

## Sessões

### 2026-09-30 22:10–23:30 — Port 4.1 → 4.0 de ponta a ponta (autônomo, `/g-autodev`)

- **Entrada:** `/add-mod-repo-for-modding` com a branch `2.1`; o mod declarava `~4.1.0`. Pedido: portar para o 4.0 usando o de-para de classes ao contrário.
- **Descoberta que definiu o método:** o commit `00e7ad8` do upstream ("Migrate to SPT 4.1.2") é o port 4.0→4.1 do próprio autor em 30 arquivos; lido ao contrário, dá o de-para de tipos e de membros conferido pelo autor. Depois dele vieram 124 commits (2.0 e 2.1) escritos direto em nomes 4.1; esses membros foram casados no decompile 4.0.
- **Decisão — camada de compatibilidade em vez de substituir nomes:** `global using NomeNovo = TipoAntigo;` mantém o fonte igual ao upstream e deixa todo `GClassNNNN` em um arquivo. **Por quê:** diff pequeno (reportar a próxima versão é reaplicar o diff) e nomes curtos como `Ammo`/`Vest`/`Money` não podem ser trocados por busca textual sem atingir identificadores do mod. Limites do apelido: não cobre genérico aberto, nome totalmente qualificado nem membro.
- **Decisão — não instalar:** o 2.1 tem o mesmo GUID do 1.2.1 e não foi validado; trocar o mod que funciona pelo que não foi testado ficou para o usuário, com script de instalação e de retorno. O `/compile-mod` ganhou `--no-install` por causa disso.
- **Lição — compilar pega menos da metade.** Depois de zero erros de compilação ainda havia: (a) 17 nomes de camada de IA comparados como texto com `GetType().Name` (`GClass86` no 4.0); (b) campo `_owner` e 5 métodos passados como texto a `AccessTools`, que faziam o Ghost Mode nativo se desligar sozinho; (c) parâmetros `___campo` de patch; (d) 7 campos lidos por reflexão; (e) o painel web morto porque o hospedeiro 4.0 não carrega o MudBlazor; (f) caminho `SPT_Runtime/` no ZIP de preset. Nenhum gera erro de compilação, e (e) respondia HTTP 200.
- **Lição — o Harmony não desvia método do jogo fora do jogo.** O runtime de desktop recusa compilar chamadas nativas da Unity ("ECall methods must be packaged into a system module"), em .NET Framework e em .NET moderno. O que funciona: carregar as DLLs reais, rodar o `Enable()` do próprio mod e interceptar `Harmony.Patch` para conferir o que o Harmony conferiria (alvo, nomes de parâmetro, `___campo`, transpilers sobre o IL lido com `PatchProcessor.GetOriginalInstructions`). Requisitos do ambiente: .NET Framework 4.8 (o HarmonyX do BepInEx não roda em .NET 8+), marcar delegates do jogo como `sealed` numa cópia, e tolerar `Assembly.GetTypes()` parcial.
- **Lição — HTTP 200 não prova Blazor.** As 13 páginas respondiam 200 e renderizavam no servidor com o circuito morto. Só o navegador mostrou. O servidor só fala HTTPS com certificado próprio, que o Chrome do MCP recusa; a saída foi um proxy HTTP local de 40 linhas com repasse de WebSocket.
- **Hipótese descartada:** gerar o `Assembly-CSharp` do 4.1 com a ferramenta oficial `SP-Tushonka/assembly-tool` para comparar membros. Não serve: o 4.1 usa outra build do jogo (EFT 0.16.9.5.40743, contra 0.16.9.40087 do 4.0) e os mapas da ferramenta são para aquela build.
- **Revisão:** quatro revisores de contexto limpo (reflexão por texto, patches e membros, servidor, spec técnica) + execução fora do jogo. 0 renomeação errada em 60 pares; code review com 15 achados (11 aplicados, 4 aceitos) e review técnica com 20 pontos, todos aplicados. Seis defeitos introduzidos de propósito, um por vez, fizeram a verificação falhar.
- **Lição — "aceitar porque o resultado é igual" esconde a condição.** O patch do Boar caía no mesmo método do patch irmão no 4.0 e três revisores aceitaram por dar o mesmo resultado. O quarto mostrou que o prefixo recebia um objeto de outro tipo no parâmetro e só não falhava por não tocar membro da subclasse. Ficou `Applies` no patch: no 4.0 ele não liga.
- **Commits:** `be679832` (port que compila), `a7854745` (nomes em texto + scripts), `03c9290e` (painel + campos por reflexão). Harness: `05462470` (`--no-install`), `d9a1abff` e `bd1ed69e` (wiki e endereços novos do SPT).

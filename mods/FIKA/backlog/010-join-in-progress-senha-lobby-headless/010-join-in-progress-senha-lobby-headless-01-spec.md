# 010 — Conexão em Raid em Andamento (Join In Progress / Invasão) e Sistema de Senha Opcional (Host & Headless)

**Mod:** FIKA  
**Target:** `mods/FIKA/modded-V2/`  
**Status:** Em andamento  
**Criado:** 2026-09-13  

## Visão geral

Atualmente no FIKA (SPT 4.0), quando uma raid cooperativa é iniciada (`Status == IN_GAME`), a partida é sumariamente bloqueada para a entrada de qualquer novo participante que não fizesse parte do lobby original pré-start. A única conexão permitida durante a raid é o "Reconnect" de jogadores que já estavam na partida e sofreram queda de conexão.

Além disso, não existe qualquer mecanismo nativo de proteção por senha nos lobbies: qualquer jogador que enxergue a partida no servidor pode conectar-se antes do início, sem qualquer controle de privacidade pelo anfitrião.

Este item de backlog unifica duas capacidades estruturais complementares:
1. **Conexão em Raid em Andamento (Join In Progress / Invasão de Raid):** Permite que novos participantes ingressem em raids já ativas no mundo, realizando o carregamento do estado atual da partida (loot coletado, portas abertas, outros jogadores) e efetuando spawn dinâmico no mapa, alavancando a infraestrutura de ressincronização autoritativa validada no Item 009.
2. **Sistema de Senha Opcional para Lobbies e Sessões (Host & Headless):** Adiciona um campo de senha opcional na janela modal de criação da partida — **"CONFIGURAÇÕES DE SESSÃO"** (`DediSelection`) — aplicável tanto para Host convencional de cliente quanto para FIKA Headless / servidor dedicado:
   - **Sem senha (campo vazio):** A partida fica pública e aberta. Qualquer jogador pode entrar no lobby pré-raid e também invadir/conectar-se durante a raid em andamento.
   - **Com senha (campo preenchido):** A partida é protegida. Apenas jogadores que fornecerem a senha correta conseguem conectar-se ao lobby ou ingressar na raid em andamento.
3. **Janela de Confirmação / Entrada Estilo "Configurações de Sessão":** Para os jogadores que vão ingressar ou invadir a partida, uma janela visualmente parecida (mesmo estilo de moldura escura, barra superior, botão `X` vermelho de fechar no canto superior direito, campo de digitação de senha e botão de confirmação "ENTRAR" / "INVADIR") é apresentada no momento do clique em "Entrar", funcionando de forma unificada em **dois pontos de entrada**:
   - Pela **Tela de Incursões / Server Browser** (`MatchMakerUIScript`).
   - Pelo botão de entrar na **Lista de Jogadores Online do Menu Principal** (`MainMenuUIScript`).

---

## Comportamento atual

- **Janela de Criação de Sessão (`DediSelection`):** Na tela de MatchMaker, ao clicar em hospedar, abre-se a janela modal **"CONFIGURAÇÕES DE SESSÃO"** contendo apenas a opção "Usar Host Headless", a seleção de instância dedicada e o botão "INICIAR". Não há campo de senha nem configuração de privacidade.
- **Bloqueio de Entrada em Raid em Andamento no MatchMaker:** Em [`MatchMakerUIScript.cs:619`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MatchMakerUIScript.cs#L619), quando uma partida está em `ELobbyStatus.IN_GAME`, caso o jogador local não estivesse registrado previamente na raid (`!localPlayerInRaid`), o botão de entrar é forçado para `button.enabled = false` e exibe o tooltip `LocaleUtils.UI_RAID_IN_PROGRESS` ("Raid em andamento").
- **Bloqueio de Entrada pelo Menu de Jogadores Online:** Em [`MainMenuUIScript.cs:240-320`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MainMenuUIScript.cs#L240), ao clicar em "Entrar" na lista de jogadores online, o cliente tenta o ping direto sem checagem de senha e é barrado caso a raid já tenha iniciado.
- **Bloqueio no Pinger de Descoberta UDP:** Em [`FikaServer.cs:822`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L822), caso a raid já tenha iniciado (`started && !reconnect`), o servidor responde `"fika.inprogress"`. O cliente receptor em [`FikaPingingClient.cs:309`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaPingingClient.cs#L309) marca `InProgress = true`, cancela o ping e aborta a conexão informando `"Session already in progress and you are not active in the session!"`.
- **Bloqueio no Handshake LiteNetLib:** Em [`FikaServer.cs:843-857`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Networking/FikaServer.cs#L843), no método `OnConnectionRequest`, se `RaidStarted == true` e a chave de conexão não for exatamente `"fika.reconnect"`, o servidor executa `request.Reject("Raid already started")`.

---

## Comportamento desejado

### 1. Campo de Senha Opcional na Janela "CONFIGURAÇÕES DE SESSÃO" (Host e Headless)
- Na janela modal **"CONFIGURAÇÕES DE SESSÃO"** (`DediSelection`), é adicionado um campo de texto `TMP_InputField`: **"Senha da Partida (Opcional)"**.
- **Host Normal:** Ao clicar em "INICIAR" com o modo dedicado desmarcado, a senha (se preenchida) é enviada em `CreateMatch` para o backend SPT e associada à sessão `FikaMatch`.
- **Modo FIKA Headless:** Ao marcar "Usar Host Headless" e definir a senha:
  1. O cliente anexa a senha no objeto `StartHeadlessRequest` enviado para o backend SPT (`/fika/raid/headless/start`).
  2. O backend SPT despacha o comando de início de raid via WebSocket (`HeadlessWebSocket`) para a instância ativa do `Fika.Headless`.
  3. O executável Headless (`FikaHeadlessPlugin.cs`) consome o campo de senha da requisição e repassa-o ao executar `FikaBackendUtils.CreateMatch(...)` no backend SPT.
  4. A raid do Headless é registrada no `MatchService` do servidor SPT com a proteção por senha ativa!

### 2. Janela Modal de Entrada (Estilo "CONFIGURAÇÕES DE SESSÃO")
- Quando um jogador clica no botão para entrar/invadir:
  - **Pela Tela de Incursões / Server Browser** (`MatchMakerUIScript.cs:525`), OU
  - **Pela Lista de Jogadores Online no Menu Principal** (`MainMenuUIScript.cs:240`),
- Abre-se uma janela modal esteticamente idêntica à de "CONFIGURAÇÕES DE SESSÃO":
  - **Barra Superior:** Título "ENTRAR EM RAID" / "INVADIR RAID" com botão `X` vermelho no canto superior direito para cancelar/fechar.
  - **Corpo:**
    - Se a partida **possuir senha**: exibe o campo `TMP_InputField` mascarado com placeholder *"Digite a senha da partida..."*.
    - Se a partida **não possuir senha**: exibe aviso informativo *"Partida pública (sem senha)"* ou confirmação de invasão.
  - **Botão Inferior:** Botão largo no mesmo padrão de "INICIAR", com o texto **"ENTRAR"** ou **"INVADIR"**.

### 3. Conexão em Raid em Andamento (Join In Progress / Invasão)
- No navegador de servidores, partidas em status `ELobbyStatus.IN_GAME` deixam de ficar cinzas/desabilitadas para novos participantes.
- O botão passa a exibir **"Invadir Raid"** (`UI_INVADE_RAID`).
- Se a partida não tiver senha, qualquer jogador pode invadir diretamente. Se tiver senha, apenas quem digitar a senha correta na janela modal consegue ingressar.
- O pinger responde `"fika.hello"`, o `OnConnectionRequest` aceita a conexão, o cliente carrega o delta de mundo (`WorldLootPacket`, portas) e spawna o jogador dinamicamente no mapa.

---

## Critérios de aceite

- [ ] Campo de senha opcional integrado perfeitamente dentro da janela "CONFIGURAÇÕES DE SESSÃO" (`DediSelection`).
- [ ] O fluxo via FIKA Headless transmite a senha via WebSocket (`StartHeadlessRequest`) até o `FikaHeadlessPlugin`, registrando a sala protegida no `FikaServer`.
- [ ] Partidas sem senha permitem entrada e invasão livre por qualquer participante sem exibição de solicitação de senha.
- [ ] Partidas com senha exibem indicador de cadeado 🔒 no navegador de servidores.
- [ ] Janela modal com layout idêntico ao de "CONFIGURAÇÕES DE SESSÃO" (moldura escura, botão `X` vermelho e botão de confirmação) abre ao clicar em entrar/invadir:
  - Funcional na tela de incursões / MatchMaker (`MatchMakerUIScript`).
  - Funcional na lista de jogadores online do menu principal (`MainMenuUIScript`).
- [ ] Senha incorreta digitada na janela de entrada impede a conexão e exibe erro amigável sem travar a UI do jogo.
- [ ] Raids em andamento (`IN_GAME`) permitem clique em "Invadir Raid", com handshake concluído com sucesso e injeção dinâmica de spawn.
- [ ] O jogador invasor carrega o estado de loot e portas interativas da raid em andamento sem desync.
- [ ] Preservação de compatibilidade total com o reconnect convencional (Item 009).

---

## Corner cases

- [ ] **Senha vazia / apenas espaços:** O sistema trata como ausência de senha (`HasPassword = false`), mantendo a raid pública.
- [ ] **Cancelamento pelo botão X vermelho:** Fechar a janela modal no `X` aborta a tentativa de entrada e restaura o estado interativo da tela anterior.
- [ ] **Entrada via Menu Principal para partida cheia:** Se o jogador clicar em entrar na lista de jogadores online mas a raid já atingiu a lotação máxima, a janela exibe aviso de sala cheia.
- [ ] **Invasor tentando reconectar após queda:** Se o invasor cair após ter entrado na raid, o fluxo de Reconnect in-place (Item 009) o reconhece como membro legítimo da raid e restaura sua posição sem pedir a senha novamente.

---

## Referências

- [`references/eft-decompiled/Assembly-CSharp/EFT/UI/Matchmaker/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/UI/Matchmaker/)
- [`mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MatchMakerUIScript.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MatchMakerUIScript.cs)
- [`mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MainMenuUIScript.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/UI/Custom/MainMenuUIScript.cs)
- [`mods/FIKA/modded-V2/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded-V2/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs)
- [`mods/FIKA/docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/docs/ROADMAP.md) (§2 e §3)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-13 | Item criado integrando Join In Progress e Senha Opcional (Host & Headless). |
| 2026-09-13 | Atualização de UI: campo de senha em "CONFIGURAÇÕES DE SESSÃO" (`DediSelection`) e criação de janela modal idêntica para os dois pontos de entrada (MatchMaker e Lista de Jogadores Online no Main Menu). |

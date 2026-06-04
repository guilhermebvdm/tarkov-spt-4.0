# Projeto: Mod de Alternância PVP / PVE (Baseado em Eventos)

**Status:** Planejamento / Descoberta
**Objetivo:** Criar um mod que permite aos jogadores escolherem entre mapas no modo PVP (jogadores spawnando dispersos, fogo hostil, respawn) e mapas no modo PVE (jogadores em cooperação spawnando juntos em grupo), utilizando a mecânica nativa de interface de eventos do jogo (similar ao evento Khorovod).

---

## 1. Descobertas: Mecânica Vanilla (Tarkov/SPT)

Durante a fase de exploração nos arquivos `Assembly-CSharp` do Tarkov e do cliente, descobrimos como o botão de transição de evento (visto no canto superior direito da tela de escolha de mapa durante o evento Khorovod) funciona:

*   **HUD Controlado pelo Servidor:** O botão não requer modificação no cliente (como um plugin BepInEx em C#). Ele é ativado dinamicamente pelo servidor ao enviar o JSON na rota `/client/settings`.
*   **A Flag Mestra:** A propriedade responsável por fazer o botão aparecer é a `EventSettings.EventActive = true` (dentro da classe `BackendConfigSettingsClass`).
*   **Filtragem de Mapas:** O botão aparecerá nos mapas a não ser que o mapa esteja contido na lista `EventSettings.LocationsToIgnore` (que por padrão ignora Hideout, Factory, Labs).
*   **O Que o Botão Faz:** Quando o jogador clica para alternar a interface entre "Normal" e "Evento", o cliente altera o valor da propriedade `transitionType` no objeto `RaidSettings`.
    *   `ELocationTransition.Common` ou `None` (0 / 1) = Mapa Normal
    *   `ELocationTransition.Event` (2) = Mapa de Evento
*   **Stand-alone vs Khorovod:** A propriedade `EventActive` é apenas um habilitador visual. O evento do Khorovod em si (com a árvore de natal e rituais) é gerido por outra chave (`runddansSettings.active`). Assim, é possível usar o botão visual de eventos no nosso mod mantendo o Khorovod desativado, evitando conflitos.

---

## 2. Descobertas: Integração Multiplayer (Fika)

Ao analisar o código-fonte do plugin e servidor do Fika (usado para cooperação multiplayer no SPT), validamos como a lógica de salas (lobbies) é tratada:

*   **Validação do Fika Cliente:** O arquivo `MatchMakerUIScript.cs` (na UI do Fika) faz a consulta da lista de servidores. Ele filtra os servidores visualmente validando `LocationId` (Mapa), `SelectedDateTime` (Horário) e `Side` (Facção PMC/Scav).
*   **Problema Identificado:** O cliente Fika **NÃO filtra** salas baseadas no modo de evento (`transitionType`). Isso significa que um jogador buscando PVP poderia enxergar a sala de um jogador Hosteando PVE e entrar nela.
*   **A Solução (Isolamento de Matchmaking):**
    A correção e lógica do nosso Mod deverão ocorrer no backend do Fika/SPT. Quando o cliente buscar as salas disponíveis (rota `/fika/location/raids`), o payload incluirá o `RaidSettings` do cliente (que contém o `transitionType` dele). O mod no servidor deverá cruzar essa informação com as salas abertas e retornar *somente* as partidas (Match IDs) que compartilham o mesmo `transitionType`. Outra validação deverá ocorrer na rota de entrada (`/fika/raid/join`) para recusar a conexão caso haja fraude ou desincronização.

---

## 3. Plano de Execução Sugerido (Server Mod)

Quando este projeto sair da fase de descoberta e for implementado, os seguintes passos deverão ser codificados no mod de servidor (TypeScript/JavaScript para SPT):

### Fase A: Habilitar o botão na UI
1. Interceptar a rota `/client/settings`.
2. Modificar o JSON de resposta injetando/alterando:
   * `EventSettings.EventActive = true`
   * Limpar o array `EventSettings.LocationsToIgnore` (para que o botão apareça em todos os mapas desejados).
3. Alterar os arquivos de texto (`locales`) no servidor para renomear os textos de interface de "Khorovod / Evento" para "PVE / PVP".

### Fase B: Isolamento de Fila no Fika (Matchmaking)
1. Interceptar a rota `/fika/location/raids` no servidor.
2. Ler o `transitionType` do Request. Filtrar a lista de `matches` gerada pelo Fika, descartando os matches em que o Host escolheu um modo diferente do jogador.
3. Interceptar a rota `/fika/raid/join` e negar a entrada se as transições não baterem (dupla verificação).

### Fase C: Aplicar as Regras da Raid (Spawn)
1. Interceptar a rota `/client/match/local/start` (ou eventos de raid start equivalentes no Fika).
2. Ler o `transitionType`.
3. **Se for 2 (PVE/Evento):**
   * Configurar a geração de bots (BotSpawner) para focar em Scavs/Bosses.
   * Utilizar a lógica cooperativa do Fika para agrupar os jogadores no mesmo ponto de spawn.
   * Desabilitar PMCs inimigos (opcional/a definir).
4. **Se não for 2 (PVP/Normal):**
   * Manter o comportamento padrão do SPT.
   * Dispersar os spawns dos jogadores pelo mapa de forma aleatória.
   * Habilitar mecânica de morte e respawn / deathmatch.
   * Habilitar PMCs bots.

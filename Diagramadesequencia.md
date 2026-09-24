'''mermaid
    sequenceDiagram
    autonumber
    actor Player as Jogador (Manodavis)
    participant ClientUIFixes as Mod UIFixes (Cliente)
    participant ClientInv as EFT Inventory (Cliente)
    participant FikaWatchdog as FIKA Watchdog (Cliente)
    participant Net as Rede LiteNetLib (UDP)
    participant HeadlessServer as Servidor Headless (Interchange)
    participant ServerInv as EFT Inventory (Servidor)
    %% FASE 1: O SWAP DA SA-58
    rect rgb(30, 40, 55)
        Note over Player, ServerInv: FASE 1: Swap do carregador da SA-58 (Origem do Desync)
        Player->>ClientUIFixes: Troca carregador da SA-58 (sem espaço livre 1x2 no rig)
        ClientUIFixes->>ClientInv: SwapIfNoSpacePatch: Executa Swap atômico (Op 128)
        ClientInv-->>ClientUIFixes: Execução local otimista: Succeeded (Status = Succeeded)
        ClientInv->>Net: Envia InventoryPacket (Op 128: Swap Arma <-> Rig Grid 3)
        Net->>HeadlessServer: Entrega pacote UDP no buffer do servidor (~2ms)
        FikaWatchdog->>FikaWatchdog: Inicia contagem regressiva de 5.0 segundos
        Note over HeadlessServer, ServerInv: Headless sobrecarregado (IA SAIN, dezenas de bots na Interchange).<br/>Processamento da Unity atrasa ~5.5s para drenar a fila de rede.
        FikaWatchdog->>FikaWatchdog: ⏱️ 5.0s atingidos! Watchdog dispara timeout!
        FikaWatchdog->>ClientInv: Auto-drena Callback 128 como Failed (Network timeout)
        
        Note over ClientInv: ❌ FALHA 1 NO FIKA: ClientInventoryOperationHandler chama<br/>Operation.Dispose() SEM forçar Status = Failed.<br/>O RollBack() nativo NÃO É CHAMADO! O cliente MANTÉM a posição local.
        ServerInv->>ServerInv: Aos 5.5s: Headless processa e conclui Swap com sucesso!
        HeadlessServer->>Net: Envia OperationCallbackPacket (Op 128: Succeeded)
        Net->>ClientInv: Resposta chega aos 5.6s
        Note over ClientInv: ❌ FALHA 2 NO FIKA: Linha 1940 de FikaPlayer.cs:<br/>"Pacote tardio ignorado (já auto-drenado por timeout)".<br/>Servidor e Cliente agora têm visões divergentes de quem ocupa os slots!
    end
    %% FASE 2: A RECARGA DA AK-12 COM "R"
    rect rgb(45, 30, 45)
        Note over Player, ServerInv: FASE 2: A Recarga com "R" da AK-12 5.45 (O Ponto de Ruptura)
        Player->>ClientInv: Pressiona "R" para recarregar a AK-12
        ClientInv->>ClientInv: Escolhe slot para guardar mag antigo 5.45 -> Seleciona Grid 3
        ClientInv->>Net: Dispara ReloadMagPacket(MagId: 5.45, Destino: Grid 3)
        ClientInv->>Player: Inicia imediatamente a animação de recarga em 1ª pessoa (Assíncrono)
        
        Net->>HeadlessServer: Headless recebe ReloadMagPacket
        HeadlessServer->>ServerInv: controller.ReloadMag(mag545, gridItemAddress: Grid 3, null)
        ServerInv->>ServerInv: GClass2006.Run tenta mover o mag antigo 5.45 para o Grid 3
        Note over ServerInv: ❌ FALHA 3 NO SERVIDOR: Grid 3 ESTÁ OCUPADO no Headless!<br/>Erro: "(x:0, y:0) in grid 3 is taken by another item".<br/>O EFT no Headless ABORTA A RECARGA silenciosamente!
        Note over HeadlessServer, ServerInv: NO HEADLESS: O mag 5.45 NUNCA SAIU DA ARMA!<br/>Continua em 'slot mod_magazine in weapon_ak12'.
        Note over Player, ClientInv: NO CLIENTE: A animação termina normalmente.<br/>O mag 5.45 foi movido para o Grid 3 do rig.
    end
    %% FASE 3: O ITEM FANTASMA E AS FALHAS SUBSEQUENTES
    rect rgb(50, 40, 30)
        Note over Player, ServerInv: FASE 3: O Carregador Fantasma (Sintomas relatados pelo jogador)
        
        Player->>ClientInv: Tenta municiar balas manualmente ou via mod CLA (ContinuousLoadAmmo)
        ClientInv->>Net: Envia Split/LoadMagazine para o mag 5.45
        Net->>HeadlessServer: Valida no servidor
        ServerInv-->>ClientInv: REJEITADO: "Result cannot hold as much / only allows 0"<br/>(Linha 2735 e 2859 do log)
        Player->>ClientInv: Tenta arrastar o mag 5.45 do Grid 3 para o Grid 1 no inventário
        ClientInv->>Net: Envia MoveOperationClass (Op 395)
        Net->>HeadlessServer: Valida no servidor
        ServerInv-->>ClientInv: REJEITADO: "Item is not located at grid 3! It's at slot mod_magazine in weapon_ak12!"<br/>(Linha 2749 do log)
        Player->>ClientInv: Tenta pegar novos carregadores no chão (Loot)
        ClientInv-->>Player: BLOQUEADO: Conflito de ocupação de slots e mãos dessincronizadas
        Player->>ClientInv: Pressiona tecla "END" (Mod HandsAreNotBusy)
        Note over Player, ClientInv: Tecla "END" reseta animações de mãos (HandsController),<br/>mas o problema é no grafo de itens (InventoryController). Nada muda!
    end
    '''
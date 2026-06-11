# Spec: Stance para Recarga e Checagem

## 1. Visão Geral
Tornar as animações de recarga e checagem de arma (verificar munição e câmara) mais estilosas e realistas, forçando o personagem a levantar a arma (mudar para a stance "Pronto Alto" / High Ready) automaticamente quando a ação é iniciada. Após o término da ação, a arma deve retornar para a postura original que o jogador estava usando.

## 2. Requisitos Funcionais

### 2.1 Mudança Automática de Postura
- Quando o jogador acionar qualquer comando de Recarregar (ReloadMag, QuickReload, etc.) ou de Checar Arma (CheckAmmo, CheckChamber, ExamineWeapon), o mod deve interceptar o início da ação.
- A stance atual deve ser salva na memória.
- A stance deve ser forçada para "Pronto Alto" (High Ready).

### 2.2 Retorno à Postura Original
- O mod deve monitorar quando a ação de recarga ou checagem foi concluída ou cancelada.
- Ao término, o mod deve restaurar a stance para o valor que estava salvo na memória antes da ação começar.

### 2.3 Restrições e Interrupções
- Se o jogador iniciar uma corrida (Sprint) ou trocar de arma no meio da recarga, a memória da postura original deve ser preservada ou a transição cancelada de forma elegante, retornando à postura correta.
- O sistema não deve interferir se o jogador **já** estiver na postura "Pronto Alto" antes de recarregar. Apenas não fará a troca de volta para outra diferente.

### 2.4 Configurações BepInEx (F12)
- `Enable Action Stance Swap`: Toggle para ligar/desligar essa mudança automática durante as ações (Bool, Padrão: true).

## 3. Critérios de Aceite
- [ ] Estando na Stance "Pronto Baixo", ao apertar para checar o carregador, o personagem sobe a arma para "Pronto Alto", realiza a checagem, e depois abaixa a arma novamente para "Pronto Baixo".
- [ ] O mesmo comportamento deve acontecer para recarregar a arma.
- [ ] Caso a funcionalidade seja desabilitada no F12, o recarregamento ocorre na stance atual sem forçar o "Pronto Alto".

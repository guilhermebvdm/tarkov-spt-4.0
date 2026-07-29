# Roadmap — TarkovIRL (SPT 4.0)

## 1. Arquitetura de Isolamento de Rede (Canal 3 Compartilhado TRL)

> ⚠️ **Diretriz de Segurança do FIKA:** Para evitar interferências no `Channel 0` do FIKA e prevenir erros de `ParseException: Undefined packet in NetDataReader`, qualquer sincronização visual de rotação e oscilação de arma para a 3ª pessoa deve utilizar o **Channel 3 (Canal Compartilhado TRL de Dados)**.

### 1.1 Sincronização de Oscilação de Arma (3ª Pessoa) no Canal 3
- **Conceito:** Transmitir o movimento de inércia e FreeAim visual para outros jogadores no **Channel 3** compartilhado com a assinatura `TIRL`.
- **Assinatura de Segurança (Magic Header):** Todo pacote enviado no Canal 3 é prefixado com a assinatura binária `TIRL` (`0x54 0x49 0x52 0x4C`), garantindo coexistência harmônica no mesmo canal.
- **Imunidade:** Garante que a movimentação visual de inércia ocorra sem interferir na movimentação ou inventário nativo do FIKA no `Channel 0`.

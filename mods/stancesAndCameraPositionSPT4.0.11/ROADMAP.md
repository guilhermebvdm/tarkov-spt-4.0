# Roadmap — Stances and Camera Position (TRL-Stances)

## 1. Arquitetura de Isolamento de Rede (Canal 3 Compartilhado TRL)

> ⚠️ **Diretriz de Segurança do FIKA:** Para evitar interferências no `Channel 0` do FIKA e prevenir erros de `ParseException: Undefined packet in NetDataReader`, a sincronização visual de postura de mão/arma para a 3ª pessoa deve utilizar o **Channel 3 (Canal Compartilhado TRL de Dados)**.

### 1.1 Sincronização de Postura no Canal 3
- **Conceito:** Transmitir as alterações de postura e posições de mão para a 3ª pessoa no **Channel 3** compartilhado com a assinatura `TRLS`.
- **Assinatura de Segurança (Magic Header):** Todo pacote enviado no Canal 3 é prefixado com a assinatura binária `TRLS` (`0x54 0x52 0x4C 0x53`), permitindo coexistência perfeita com os mods `Medicine` e `TarkovIRL`.
- **Resiliência:** Qualquer variação de rede na troca de postura é descartada ou reenviada no Canal 3 sem afetar a movimentação ou inventário do FIKA no `Channel 0`.

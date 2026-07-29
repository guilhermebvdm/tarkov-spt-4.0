# Roadmap — TRL-ImmersiveCombatMedicine (Band-Aid)

## 1. Arquitetura de Isolamento de Rede (Canal 3 Compartilhado TRL)

> ⚠️ **Diretriz de Segurança do FIKA:** Para evitar interferências no `Channel 0` do FIKA e prevenir erros de `ParseException: Undefined packet in NetDataReader`, os pacotes médicos do mod devem utilizar o **Channel 3 (Canal Compartilhado TRL de Dados)**.

### 1.1 Migração de Pacotes Médicos para o Canal 3
- **Conceito:** Transmitir os sinais de socorro, animações de socorrista, desmaios e redirecionamento de tratamento no **Channel 3** compartilhado com a assinatura `TRLM`.
- **Assinatura de Segurança (Magic Header):** Todo pacote enviado pelo mod no Canal 3 é prefixado com a assinatura binária `TRLM` (`0x54 0x52 0x4C 0x4D`), garantindo coexistência perfeita com os mods `Stances` e `TarkovIRL`.
- **Isolamento com o Inventário:** A comunicação médica ocorre no Canal 3 enquanto a baixa de durabilidade dos itens do inventário continua 100% sob a gestão autoritativa do FIKA no `Channel 0`.

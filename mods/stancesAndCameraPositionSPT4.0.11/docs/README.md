# Documentação Técnica — TRL-StancesAndMobility

Bem-vindo ao índice central da documentação técnica e arquitetural do mod **TRL-StancesAndMobility** (SPT 4.0 / EFT 0.16.9).

---

## 📚 Sumário de Artigos Técnicos

| Documento | Assunto / Escopo | Status |
| :--- | :--- | :--- |
| **[01. Visão Geral e Arquitetura](./01-visao-geral-e-arquitetura.md)** | Arquitetura central, ciclo de vida, convenção de eixos da arma e estrutura de código. | 🟢 Vivo |
| **[02. Sistema de Posturas e Transições](./02-sistema-de-posturas-e-transicoes.md)** | StanceManager, Action Stances, Snap on Fire e física de molas (SpringDamp). | 🟢 Vivo |
| **[03. Sistemas de Mira (ADS) e Movimentação](./03-sistemas-de-mira-ads-e-movimentacao.md)** | Compressão de velocidade de ADS, limites de velocidade e fadiga de braços. | 🟢 Vivo |
| **[04. Sistemas de Apoiamento e Respiração](./04-sistemas-de-apoiamento-e-respiracao.md)** | Passive Mount, bloqueio de mount ativo, Hold Breath e Oxygen UI. | 🟢 Vivo |
| **[05. Manual Chambering e Mecânicas de Armas](./05-manual-chambering-e-mecanicas-de-armas.md)** | Alimentação manual de câmara, inspeção de munição e cuidados com inventário. | 🟢 Vivo |
| **[06. Integração FIKA Coop e Rede](./06-integracao-fika-coop-e-rede.md)** | Sincronização em rede, Canal 3 LiteNetLib, pacotes e renderização pré-IK. | 🟢 Vivo |
| **[Relatório de Auditoria Técnica de Código (Review 01)](./relatorio-auditoria-codigo-01.md)** | Auditoria profunda estática, diagnóstico do bug de armas/inventário e plano de ação. | 🟢 Vivo |

---

## 📂 Arquivos de Código-Fonte Mapeados (`modded-testchannel/`)

- **Ponto de Entrada:** [`Plugin.cs`](../modded-testchannel/Plugin.cs)
- **Gerenciador de Estados:** [`StanceManager.cs`](../modded-testchannel/StanceManager.cs)
- **Segurança de Mãos:** [`HandsStateGuard.cs`](../modded-testchannel/HandsStateGuard.cs)
- **Fadiga / Stamina:** [`StaminaController.cs`](../modded-testchannel/StaminaController.cs)
- **Matemática de Molas:** [`SpringMath.cs`](../modded-testchannel/SpringMath.cs)
- **Patches de Postura & Mecânicas:** [`Patches/`](../modded-testchannel/Patches/)
- **Módulo de Rede FIKA:** [`Networking/`](../modded-testchannel/Networking/)
- **Overlays Gráficos:** [`UI/`](../modded-testchannel/UI/)

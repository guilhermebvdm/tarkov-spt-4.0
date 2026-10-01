# Documentação Técnica — SPT-MagCheckInterrupt

Bem-vindo ao índice central da documentação técnica e arquitetural do mod **SPT-MagCheckInterrupt** (versão `1.0.2`, compatível com SPT `4.0.13` / EFT `0.16.9`).

---

## 1. Sumário dos Artigos Técnicos

A documentação está decomposta em 5 módulos coesos e complementares:

| # | Artigo | Escopo e Conteúdo | Status |
| :---: | :--- | :--- | :---: |
| **01** | [Visão Geral e Arquitetura](01-visao-geral-e-arquitetura.md) | Propósito funcional, ciclo de vida do BepInEx, pipeline global de interrupção e inventário de subsistemas. | 🟢 Vivo |
| **02** | [Operações de Recarga e Máquina de Estados](02-operacoes-de-recarga-e-maquina-de-estados.md) | Implementação de `MagCheckReloadOperation`, `SwapReloadOperation`, normalização de tempo, controle de velocidade e animação Mecanim. | 🟢 Vivo |
| **03** | [Patches Harmony e Interceptação de Input](03-patches-harmony-e-interceptacao-de-input.md) | Detalhamento dos 8 patches Harmony, desvio de operações, atraso do painel da HUD e resolução de conflitos de teclas. | 🟢 Vivo |
| **04** | [Integração FIKA e Sincronização de Rede](04-integracao-fika-e-sincronizacao-de-rede.md) | Soft dependency com `Fika.Core`, pacotes de rede LiteNetLib (`ConfigPacket`, `ReloadCalledPacket`) e prevenção de desync no `FastForward`. | 🟢 Vivo |
| **05** | [Compatibilidade com UIFixes e Reload in Place](05-compatibilidade-com-uifixes-e-reload-in-place.md) | Mecanismo de swap de inventário, fusão de remoção e inserção em mãos e sinergia com o Reload in Place do UIFixes. | 🟢 Vivo |
| **Audit** | [Relatório de Auditoria Técnica de Código (Review 01)](relatorio-auditoria-codigo-01.md) | Auditoria minuciosa focando nos vilões de recarga, bloqueio de gatilho, desync de rede no FIKA e swap com UIFixes. | 🟢 Vivo |

---

## 2. Inventário de Arquivos do Código-Fonte

Relação estruturada de todos os arquivos C# do projeto ([`mods/SPT-MagCheckInterrupt/original/`](../original/)):

### Ponto de Entrada e Configuração Global
* [`MagCheckInterrupt.cs`](../original/MagCheckInterrupt/MagCheckInterrupt.cs) — Plugin principal BepInEx (`[BepInPlugin]`).
* [`GlobalUsings.cs`](../original/MagCheckInterrupt/GlobalUsings.cs) — Imports e aliases globais do C#.
* [`PROPRIEDADES.md`](../PROPRIEDADES.md) — Dicionário de parâmetros expostos no menu F12 (BepInEx Configuration Manager).

### Componentes de Operação e Debug
* [`Components/MagCheckReloadOperation.cs`](../original/MagCheckInterrupt/Components/MagCheckReloadOperation.cs) — Operação que substitui a checagem padrão de carregador.
* [`Components/SwapReloadOperation.cs`](../original/MagCheckInterrupt/Components/SwapReloadOperation.cs) — Operação composta para substituição de carregador com animação contínua.
* [`Components/PlayerStateDebug.cs`](../original/MagCheckInterrupt/Components/PlayerStateDebug.cs) — Componente auxiliar de telemetria e depuração de estado do jogador (compilado em Debug).

### Patches Harmony
* [`Patches/RunUtilityOpPatch.cs`](../original/MagCheckInterrupt/Patches/RunUtilityOpPatch.cs) — Redireciona a chamada de checagem para a operação customizada.
* [`Patches/OperationFactoryPatch.cs`](../original/MagCheckInterrupt/Patches/OperationFactoryPatch.cs) — Registra as novas operações na factory do `FirearmController`.
* [`Patches/CanQuickReloadPatch.cs`](../original/MagCheckInterrupt/Patches/CanQuickReloadPatch.cs) — Habilita a recarga rápida durante a checagem.
* [`Patches/ReloadAnimationPatch.cs`](../original/MagCheckInterrupt/Patches/ReloadAnimationPatch.cs) — Suprime reinício de animação padrão ao fazer *CrossFade*.
* [`Patches/ReloadFastAnimationPatch.cs`](../original/MagCheckInterrupt/Patches/ReloadFastAnimationPatch.cs) — Suprime reinício de animação rápida ao fazer *CrossFade*.
* [`Patches/AmmoDetailsPatch.cs`](../original/MagCheckInterrupt/Patches/AmmoDetailsPatch.cs) — Sincroniza a exibição da contagem de munição na HUD com a janela de decisão.
* [`Patches/UpdateBindingsPatch.cs`](../original/MagCheckInterrupt/Patches/UpdateBindingsPatch.cs) — Monitora atualizações de teclas do usuário.
* [`Patches/ReloadConflictPatch.cs`](../original/MagCheckInterrupt/Patches/ReloadConflictPatch.cs) — Evita recargas acidentais quando teclas são compartilhadas.

### Integrações Externas
* [`External/Fika.cs`](../original/MagCheckInterrupt/External/Fika.cs) — Handlers de eventos, sincronização de configurações e envio de pacotes de rede.
* [`External/UIFixes.cs`](../original/MagCheckInterrupt/External/UIFixes.cs) — Injeção de `CanExecuteSwapPatch` para autorizar operações de swap do UIFixes.

### Utilitários
* [`Utils/AnimationUtil.cs`](../original/MagCheckInterrupt/Utils/AnimationUtil.cs) — Gestão de estados Mecanim, cálculos de tempo e *CrossFade*.
* [`Utils/ConfigUtil.cs`](../original/MagCheckInterrupt/Utils/ConfigUtil.cs) — Controle de opções de configuração e leitura/gravação serializada.
* [`Utils/KeybindsUtil.cs`](../original/MagCheckInterrupt/Utils/KeybindsUtil.cs) — Análise de conflito de teclas.
* [`Utils/LoggerUtil.cs`](../original/MagCheckInterrupt/Utils/LoggerUtil.cs) — Encapsulamento padronizado de logs BepInEx.
* [`Utils/ConfigurationManagerAttributes.cs`](../original/MagCheckInterrupt/Utils/ConfigurationManagerAttributes.cs) — Metadados de UI para o Configuration Manager.

### Protocolo de Rede (LiteNetLib)
* [`MagCheckInterrupt.Net/ConfigPacket.cs`](../original/MagCheckInterrupt.Net/ConfigPacket.cs) — Pacote de sincronização de configurações mestre do Host.
* [`MagCheckInterrupt.Net/ReloadCalledPacket.cs`](../original/MagCheckInterrupt.Net/ReloadCalledPacket.cs) — Notificação de início de recarga para prevenir encerramento via `FastForward`.

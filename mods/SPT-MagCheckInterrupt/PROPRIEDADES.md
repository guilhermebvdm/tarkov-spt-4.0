# Propriedades de Configuração — MagCheckInterrupt

**Plugin:** `com.ozen.magcheckinterrupt` (MagCheckInterrupt)  
**Versão:** `1.0.2`  
**Código Fonte:** [`original/MagCheckInterrupt/Utils/ConfigUtil.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/ConfigUtil.cs) / [`original/MagCheckInterrupt/MagCheckInterrupt.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/MagCheckInterrupt.cs)  

> [!NOTE]
> Itens marcados com **(Avançado)** só aparecem no menu F12 (BepInEx Configuration Manager) com a opção **"Advanced settings"** habilitada.
> Quando jogando em modo cooperativo com o mod **FIKA**, as configurações de sincronização podem ser ditadas e travadas em modo somente-leitura pelo Host.

---

## 1. General (Geral)

| Nome (EN) | Nome (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **Reload Mode** | Modo de Recarga | `Enum (EReloadMode)` | `Press` | `Press`, `Release` | Não | Caso especial para quando as teclas de recarga e checagem de carregador estiverem em conflito (`Press to Reload` ou `Release to Reload`). |
| **Reload Window Start** | Início da Janela de Recarga | `float` | `0.1` (10%) | `0.0` a `1.0` | **Sim** | Quão cedo você pode recarregar durante a animação de checagem do carregador, em tempo normalizado. |
| **Reload Window End** | Fim da Janela de Recarga | `float` | `0.6` (60%) | `0.0` a `1.0` | **Sim** | Quão tarde você pode recarregar durante a animação de checagem do carregador, em tempo normalizado. |

---

## 2. Slow Animation (Animação Lenta)

| Nome (EN) | Nome (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **Enable** | Habilitar | `bool` | `true` | `true`, `false` | Não | Desacelera a animação de checagem de carregador por um determinado período. |
| **Slow Percentage** | Porcentagem de Lentidão | `float` | `0.25` (25%) | `0.01` a `1.0` | **Sim** | Multiplicador de velocidade da animação de checagem quando a Animação Lenta estiver ativada. |
| **Start** | Início | `float` | `0.3` (30%) | `0.0` a `1.0` | **Sim** | Momento de início da desaceleração da animação de checagem de carregador, em tempo normalizado. |
| **End** | Fim | `float` | `0.4` (40%) | `0.0` a `1.0` | **Sim** | Momento de restauração da velocidade da animação de checagem de carregador, em tempo normalizado. |
| **Smoothing Max Delta** | Delta Máximo de Suavização | `float` | `2.0` | `0.01` a `10.0` | **Sim** | Delta máximo para a suavização da animação lenta. Um valor maior desacelera/restaura a animação mais rapidamente. |

---

## 3. FIKA Host Sync (Sincronização Coop)

* **FikaHostConfig:** Propriedade interna gerenciada via netcode. Quando o cliente está conectado a uma sessão do FIKA recebendo as configurações do servidor, um banner informativo em azul é exibido no topo do menu F12: `Configuration is set by the Fika Host`, bloqueando alterações locais conflitantes.

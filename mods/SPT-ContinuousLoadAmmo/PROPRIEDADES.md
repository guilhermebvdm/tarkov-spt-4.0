# Propriedades — Continuous Load Ammo

**Plugin:** `com.ozen.continuousloadammo` | v1.1.6  
**Arquivo:** [original/ContinuousLoadAmmo.cs](original/ContinuousLoadAmmo.cs)  
**Dependências Opcionais:** `com.tyfon.uifixes`

> Itens marcados **(Avançado)** só aparecem com "Advanced settings" ligado no F12 (BepInEx ConfigurationManager).

---

## 1. General

| Nome EN | Nome PT-BR | Tipo | Padrão | Faixa | Tooltip PT-BR |
|---|---|---|---|---|---|
| Speed Limit | Limite de Velocidade | `float` | `0.45` (45%) | 0–1 (0% a 100%) | O limite de velocidade, como uma porcentagem da velocidade de caminhada, aplicado ao jogador enquanto carrega munição |
| Reachable Places Only | Apenas Locais Acessíveis | `bool` | `true` | — | Permitir carregar munição fora do inventário apenas quando o Carregador e a Munição estiverem no seu Colete (Vest), Bolsos ou Contêiner Seguro |
| Inventory Tabs | Abas do Inventário | `bool` | `true` | — | Não interromper o carregamento de munição ao alternar entre as abas do inventário (aba de mapas, tarefas, etc.) |
| Mag Preset Fallback | Fallback do Preset de Carregador | `bool` | `true` | — | Recorrer ao carregamento rápido (Quick Load) caso o preset de carregador não esteja disponível |

---

## 2. Quick Load

| Nome EN | Nome PT-BR | Tipo | Padrão | Faixa | Tooltip PT-BR |
|---|---|---|---|---|---|
| Hotkey | Tecla de Atalho | `KeyboardShortcut` | `K` | — | Tecla utilizada para carregar munição fora do inventário |
| Mode | Modo de Carregamento Rápido | `QuickLoadMode` | `LastMagazinePreset` | `HighestPenetration`, `LastBulletMagazine`, `LastMagazinePreset` | **Highest Penetration Available**: escolhe a munição com maior poder de penetração disponível.<br>**Last Bullet in Magazine**: prioriza a última bala do carregador da arma atual.<br>**Last Used Magazine Preset**: carrega o último preset de carregador utilizado. |
| Notify | Notificar | `bool` | `true` | — | Ao usar o Carregamento Rápido, notificar o jogador sobre a munição que está sendo carregada |

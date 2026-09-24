# Propriedades de Configuração — SPT-Dynamic-External-Resolution

Configurações expostas no menu F12 (BepInEx ConfigurationManager).

* **Plugin:** `com.shibatsui.dynamicexternalresolution`
* **Versão:** `4.0.0`
* **Código de referência:** [original/DynamicExternalResolutionConfig.cs](original/DynamicExternalResolutionConfig.cs)

> ℹ️ Itens marcados com **(Avançado)** só aparecem no menu F12 se a opção "Advanced settings" estiver habilitada.

---

## Settings

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **Enable Mod** | Habilitar Mod | `bool` | `true` | - | Sim | Ativa/desativa a redução de resolução da renderização externa ao mirar com lunetas telescópicas. |
| **Sampling Downgrade** | Redução de Amostragem | `float` | `0.5` (50%) | `0.01` a `0.99` | Não | (Funciona apenas com FSR/DLSS desativados) Porcentagem de redução da renderização externa ao mirar com lunetas telescópicas. Padrão 50%. |
| **DLSS Scaling Mode** | Modo de Escala DLSS | `EDLSSMode` | `UltraPerformance` | `Quality`, `Balanced`, `Performance`, `UltraPerformance` | Não | (Funciona apenas quando DLSS está ativado) Modo de escala ao usar DLSS que definirá a renderização externa ao mirar pela luneta. Padrão é Modo Ultra Performance (redução de ~65% na resolução). |
| **FSR2 Scaling Mode** | Modo de Escala FSR2 | `EFSR2Mode` | `UltraPerformance` | `Quality`, `Balanced`, `Performance`, `UltraPerformance` | Não | (Funciona apenas quando FSR 2.2 está ativado) Modo de escala ao usar FSR2 que definirá a renderização externa ao mirar pela luneta. Padrão é Modo Ultra Performance (redução de ~67% na resolução). |
| **FSR3 Scaling Mode** | Modo de Escala FSR3 | `EFSR3Mode` | `UltraPerformance` | `Quality`, `Balanced`, `Performance`, `UltraPerformance` | Não | (Funciona apenas quando FSR 3.0 está ativado) Modo de escala ao usar FSR3 que definirá a renderização externa ao mirar pela luneta. Padrão é Modo Ultra Performance (redução de ~67% na resolução). |

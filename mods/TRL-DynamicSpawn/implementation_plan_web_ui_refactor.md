# 📋 Plano de Implementação — Refatoração da Web UI Server (`TRL-DynamicSpawn`)

**Data:** 2026-07-31  
**Módulo:** `mods/TRL-DynamicSpawn/Server/Web`  
**Objetivo:** Substituir a infraestrutura Blazor / MudBlazor por uma Single Page App (SPA) ultra-leve em **HTML5 + TypeScript/Vanilla JS + TRL Design System**, mantendo **100% de paridade funcional** com zero perda de recursos e total conformidade legal (0 cópias visuais).

---

## 🎯 Requisitos & Objetivos

### 1. Desempenho & Leveza
- Reduzir o tempo de carregamento da página de 3–5s (WebAssembly/SignalR do Blazor) para **< 50ms**.
- Reduzir o tamanho da pasta `Server/` de ~40MB para **~200KB**.

### 2. Conformidade Legal & Identidade Visual (TRL Design System)
- Eliminar completamente o markup Razor e estilos CSS legados do ABPS.
- Aplicar o **TRL Design System v1.0.0** (`design-system/tokens.css`, `components.css`, `utilities.css`):
  - Fundo em Grafite Neutro (`#0d0f12` → `#13171f`).
  - Acentos em **Tan / Gold TRL (`#c8aa6e`)** para itens ativos, sliders e botões primários.
  - Linha laser vermelha TRL (`.trl-divider--laser`) na TopBar.
  - Cantos retos (`border-radius: 0px`) e bordas secas de 1px.
  - Tipografia condensada com caixa alta em rótulos e títulos.

### 3. Paridade Funcional de 100% (Zero Perda de Recursos)
- **Aba Configurações por Mapa (`MapConfigs`)**: Sliders e inputs de `MaxAliveBots`, `MinAliveBots`, `DelayBeforeFirstWave`, `SecondsBetweenWaves`, `WaveBotLimit`, `MaxHordeCap` para todos os mapas (`woods`, `factory4_day`, `factory4_night`, `sandbox`, `bigmap`, `shoreline`, `interchange`, `lighthouse`, `rezervbase`, `tarkovstreets`, `laboratory`, `global`).
- **Aba Configuração de Bosses (`BossConfig`)**: Sliders de chances de spawn, zonas de spawn e toggles para Reshala, Killa, Tagilla, Shturman, Sanitar, Glukhar, Kaban, Kollontay, Zryachiy, Goons, Raiders, Rogues e Cultistas.
- **Aba Dificuldades (`DifficultyConfig`)**: Seleção de modificadores (`easy`, `normal`, `hard`, `impossible`, `random`).
- **Aba Eventos (`Events`)**: Toggles para invasões e eventos sazonais.
- **Aba Spawns Customizados (`CustomSpawns`)**: Editor de coordenadas e zonas customizadas de spawn.
- **Ações Globais**: Botões **Salvar Configuração** (`POST /trl-dynamic-spawn/api/save`), **Recarregar Configuração** (`GET /trl-dynamic-spawn/api/config`), **Desfazer (Undo)**, **Restaurar Padrões** e **Seletor de Idioma (PT-BR / EN)**.

---

## 🏗️ Arquitetura Proposta

### Estrutura de Arquivos Substituta (`Server/wwwroot/`)

```text
mods/TRL-DynamicSpawn/Server/wwwroot/
├── index.html              # Shell principal da SPA (TopBar, NavMenu, Workspace, Toast Container)
├── css/
│   ├── trl-tokens.css      # Cópia local de design-system/tokens.css
│   ├── trl-components.css  # Cópia local de design-system/components.css
│   ├── trl-utilities.css   # Cópia local de design-system/utilities.css
│   └── app.css             # Estilos específicos da UI do mod usando tokens --trl-*
└── js/
    ├── app.js              # Controlador principal da SPA (roteamento, estado de edições pendentes/undo)
    ├── i18n.js             # Dicionário de tradução PT-BR / EN
    └── api.js              # Cliente de API Fetch (GET /config, POST /save)
```

### Rotas HTTP do Servidor Node.js (SPT 4.0 Server Plugin)

- `GET /trl-dynamic-spawn/` -> Serve o `index.html`.
- `GET /trl-dynamic-spawn/api/config` -> Retorna o `config.json` atual do servidor.
- `POST /trl-dynamic-spawn/api/save` -> Salva as alterações no `config.json` e notifica o SPT.
- `POST /trl-dynamic-spawn/api/reset` -> Restaura os valores padrões de fábrica do mod.

---

## 📋 Plano de Passos de Implementação

1. **Remoção da Infraestrutura Blazor**:
   - Remover arquivos `.razor` de `Server/Web/Pages/` e `Server/Web/Shared/`.
   - Limpar dependências de assemblies Blazor do `TRL-DynamicSpawn-Server.csproj`.

2. **Criação do Frontend HTML5/JS (`Server/wwwroot/index.html` e `js/app.js`)**:
   - Construir o Shell `.trl-shell` com TopBar laser, Drawer de navegação e Workspace em painéis.
   - Implementar os 5 visores funcionais: MapConfigs, BossConfig, DifficultyConfig, Events, CustomSpawns.

3. **Inclusão dos Estilos TRL Design System**:
   - Vincular os arquivos de tokens e componentes CSS do TRL Design System.

4. **Conexão de API REST com o Servidor SPT Node.js**:
   - Garantir comunicação fluida para carregamento e salvamento de `config.json`.

---

## 🧪 Plano de Verificação

- **Leveza & Velocidade**: Confirmar que a página abre instantaneamente (< 50ms) no navegador.
- **Validação de Edição**: Alterar parâmetros de mapa (ex: `woods` maxAliveBots = 20), clicar em **Salvar**, e verificar se o `config/config.json` é gravado corretamente no disco.
- **Validação Visual TRL**: Executar a verificação com as diretrizes do `PATTERNS.md` (0 hex hardcoded, vermelho restrito ao laser/dots, cantos retos).

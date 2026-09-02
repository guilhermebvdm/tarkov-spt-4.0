# Project FIKA — Ecossistema Cooperativo para SPT 4.0

Repositório unificado de desenvolvimento, auditoria e engenharia do **Project FIKA** para Escape From Tarkov / SPT 4.0.

O **FIKA** é a suíte definitiva para jogabilidade cooperativa no Single Player Tarkov, fornecendo infraestrutura de rede ponto-a-ponto/host-client e cliente dedicado headless, sincronização de física e balística em tempo real via UDP (LiteNetLib), compartilhamento de missões, marcação tática, reanimação e suporte a múltiplos jogadores simultâneos.

---

## 🧩 Componentes do Ecossistema

O mod FIKA é estruturado em 4 pilares modulares:

| Componente | Função Primária | Tecnologia | Localização | Versão |
| :--- | :--- | :--- | :--- | :--- |
| [**Fika-Plugin**](original/Fika-Plugin/) | Plugin Client BepInEx (`Fika.Core`), patches Harmony, HUD e rede in-game | C# / Unity / .NET 9 | `modded/Fika-Plugin/` | `v2.3.9` |
| [**Fika-Server-CSharp**](original/Fika-Server-CSharp/) | Módulo de servidor SPT para rotas HTTP, WebSockets, gerenciamento de sessões e perfis | C# / SPT Server | `modded/Fika-Server-CSharp/` | `v2.3.5` |
| [**Fika-Headless**](original/Fika-Headless/) | Cliente headless dedicado para hosting sem renderização gráfica de cliente | TypeScript / Node.js | `modded/Fika-Headless/` | `v1.4.15` |
| [**Fika Wiki**](wiki/) | Documentação técnica oficial de APIs, fluxos de rede e configurações | Markdown / GitBook | `wiki/` | `Snapshot` |

---

## 📁 Estrutura do Diretório

```
mods/FIKA/
├── original/                      # Snapshot oficial intocado (referência estrita)
│   ├── Fika-Plugin/               # Release v2.3.9
│   ├── Fika-Server-CSharp/        # Release v2.3.5
│   └── Fika-Headless/             # Release v1.4.15
├── modded/                        # Workspace ativo para auditorias, correções e otimizações
│   ├── Fika-Plugin/
│   ├── Fika-Server-CSharp/
│   └── Fika-Headless/
├── wiki/                          # Documentação e especificações de rede/API oficiais
├── assets/                        # Diagramas de arquitetura e recursos visuais
├── backlog/                       # Gestão de tarefas, issues e melhorias
├── builds/                        # Binários compilados isolados do mod
├── docs/                          # Relatórios detalhados de auditoria técnica de código
│   ├── original/                  # Auditorias sobre a base original
│   ├── modded/                    # Auditorias e validações das otimizações
│   ├── README.md                  # Índice da documentação técnica
│   └── ROADMAP.md                 # Roteiro de auditoria e desenvolvimento
├── scripts/                       # Scripts utilitários de suporte e validação
├── mod.json                       # Metadados do mod e versões upstream
├── PROPRIEDADES.md                # Catálogo completo das opções BepInEx F12
└── README.md                      # Este documento
```

---

## ⚙️ Configurações e Parâmetros (F12)

Para a relação completa de todas as configurações do BepInEx ConfigurationManager expostas pelo `Fika.Core`, consulte:
👉 [**PROPRIEDADES.md**](./PROPRIEDADES.md)

---

## 📚 Documentação Técnica e Auditorias

Para acompanhar a auditoria técnica de código, análise de desempenho de rede UDP, testes de garbage collection e sincronização de entidades:
👉 [**docs/README.md**](./docs/README.md) & [**docs/ROADMAP.md**](./docs/ROADMAP.md)

Para consultar a especificação completa de rede e APIs do Fika:
👉 [**wiki/advanced-features/fika-api.md**](./wiki/advanced-features/fika-api.md)
👉 [**wiki/faqandguides/advanced-how-fika-establishes-raid-connections.md**](./wiki/faqandguides/advanced-how-fika-establishes-raid-connections.md)

---

## 🛠️ Regras de Compilação e Isolamento

1. **Bump Obrigatório de Versão (SemVer):** Incrementar versão em `Plugin.cs`, `.csproj` e `package.json` antes de compilar melhorias.
2. **Isolamento Total de Builds:** Binários compilados devem ser gerados **exclusivamente** em `mods/FIKA/builds/` ou `mods/FIKA/modded/bin/Release/`. **Nunca** copie diretamente para o diretório de instalação do jogo.

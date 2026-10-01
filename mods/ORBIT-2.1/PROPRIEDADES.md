# ORBIT 2.1 — Propriedades de Configuração (F12 / BepInEx ConfigurationManager)

> **Plugin:** `com.chazut.orbit` — ORBIT v2.1.0  
> **Fonte:** [original/Orbit/Plugin.cs](original/Orbit/Plugin.cs) (método `SetupConfig`)  
> **Nota:** As configurações marcadas com **(Avançado)** ficam ocultas por padrão no menu F12 e só são exibidas quando a opção **"Advanced settings"** estiver ativada no ConfigurationManager do BepInEx.

A partir da 2.x o F12 contém só as opções de log do cliente e um botão que abre a interface web do servidor. Toda configuração de comportamento dos bots (facções, looting, extração, Ghost Mode, zonas, personalidades, presets) fica no mod de servidor do ORBIT, na interface web em `<endereço do servidor SPT>/orbit`, e não aparece no F12. O cliente lê essa configuração do servidor no boot e no início da raid.

O addon `Orbit.Fika` (`original/Orbit.Fika/`) não declara nenhuma entrada de F12.

---

## 01. Essentials

Seção gravada no arquivo `.cfg` como `01. Essentials`. No F12 o cabeçalho da seção não aparece: todas as entradas declaram `Category = ""`.

| Propriedade | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) | Ordem | Avançado |
|---|---|---|---|---|---|---|---|
| `Server config` | Configuração do Servidor | `string` (não usado; a entrada mostra o botão **"Open web config UI"**) | `""` | - | Todas as configurações de comportamento (facções, looting, extração, Ghost Mode...) ficam na interface web do servidor ORBIT. | 2 | Não |
| `Quiet logging` | Registro Silencioso | `bool` | `true` | - | LIGADO (padrão): log limpo — apenas avisos e erros, independentemente dos Níveis de log abaixo. DESLIGUE para usar os Níveis de log (ex.: marcar Debug ali antes de enviar um relatório de bug). | 1 | Não |
| `Log levels` | Níveis de Log | `OrbitLogLevel` | `Info, Warning, Error` | Flags: `None, Info, Debug, Warning, Error` | Quais níveis de mensagem o ORBIT grava (usado quando Quiet logging está DESLIGADO). Padrão: tudo exceto Debug. Marque Debug para um log detalhado de relatório de bug — agora funciona na build de release, não apenas em builds de debug. | 0 | Não |
| `Performance logging` | Registro de Desempenho | `bool` | `false` | - | LIGADO: grava no log uma linha de resumo 'PERF' (fps, engasgos, GC, contadores de atividade do ORBIT) a cada 30s, independentemente das outras configurações de log. Ligue antes de gravar uma raid para um relatório de desempenho. | -1 | **Sim** |

### Botão "Open web config UI"

A entrada `Server config` não tem campo de valor nem botão de restaurar padrão (`HideDefaultButton = true`). No lugar, mostra o botão **"Open web config UI"**, que abre no navegador padrão o endereço do servidor SPT seguido de `/orbit`. Quando o endereço do servidor não está disponível, o botão abre `https://127.0.0.1:6969/orbit`.

# 012 — Remover Targram do menu · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Insumo:** [00-kickoff](./012-remover-targram-00-kickoff.md)

## Pontos removidos (4 mapeados no kickoff — todos confirmados e removidos)

| Arquivo | O que era | Remoção |
|---|---|---|
| `project/SPT.Launcher/Views/SettingsView.axaml` (era :92-97) | Botão sidebar `TARGRAM` (Button + StackPanel + Path do ícone globo + TextBlock) | Bloco XAML completo |
| `project/SPT.Launcher/Views/ProfileView.axaml` (era :118-123) | Botão sidebar `TARGRAM` (idem) | Bloco XAML completo |
| `project/SPT.Launcher/ViewModels/SettingsViewModel.cs` (era :101-108) | `OpenTargramCommand()` → `Process.Start` em `https://targram.lovable.app/` | Método completo |
| `project/SPT.Launcher/ViewModels/ProfileViewModel.cs` (era :276-283) | `OpenTargramCommand()` (idem) | Método completo |

Botões vizinhos (LAUNCHER, CONFIGURAÇÕES, APOIE UM CAFEZINHO, LISTA DE MODS) intocados — nenhum restyle.

## Sobras encontradas no grep final (`Targram|targram`, case-insensitive, launcher inteiro)

| Sobra | Ação |
|---|---|
| `project/props.txt:125` — `private string _sidebar_targram;` | Linha removida (arquivo é dump/rascunho de código de localização no root do project/, não compilado; a key `sidebar_targram` **não existe** no LocalizationProvider real) |
| `project/props2.txt:125` — `englishLocale.sidebar_targram = "TARGRAM";` | Linha removida (idem) |
| Locale JSONs / Assets | Nenhuma ocorrência — não havia key de localização real nem asset/ícone com nome Targram (o ícone era `Path` inline no XAML, removido junto com o botão) |
| `backlog/012-remover-targram/*` e `backlog/mod-backlog.md` | Menções documentais — mantidas (docs do próprio item; `mod-backlog.md` fora do escopo deste agente) |

Grep final em `project/`: **0 ocorrências**.

## Notas

- `using System.Diagnostics` mantido nos dois ViewModels — ainda usado por `OpenKofiCommand` (e `OpenLinkCommand` no ProfileViewModel).
- Build NÃO rodado nesta sessão (proibido — csproj em build por outro agente); validação de compilação com o orquestrador.

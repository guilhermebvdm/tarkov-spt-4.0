# 010 — Entrada em Raid em Andamento (Join In Progress / Invasão) e Senha Opcional (Host & Headless) · Code Review 01

**Mod:** FIKA  
**Fork:** `mods/FIKA/modded-V2/`  
**Data:** 2026-09-14  
**Status da Revisão:** 🟢 Aprovado  

---

## 1. Escopo e Objetivos

Revisão estática e estrutural das alterações implementadas no fork `mods/FIKA/modded-V2/` para suportar:
1. Conexão e invasão em raid já iniciada (`Join In Progress`), sem travamento de pinger ou rejeição prematura de handshake.
2. Injeção de campo de senha opcional na janela nativa **"CONFIGURAÇÕES DE SESSÃO"** (`DediSelection`) tanto para host local quanto para sessões FIKA Headless.
3. Criação de janela modal com design idêntico à de configurações de sessão (`JoinSessionModal`), clonada diretamente do prefab de MatchMaker com `TMP_InputField` estilizado, invocada tanto pela tela de incursões/servidores (`MatchMakerUIScript`) quanto pela lista de amigos online do menu principal (`MainMenuUIScript`).
4. Isolamento estrito no fork `modded-V2` com bump de versões SemVer.

---

## 2. Inventário de Arquivos Alterados e Criados

| Componente | Arquivo | Modificação |
| --- | --- | --- |
| `Fika-Server-CSharp` | `StartHeadlessRequest.cs` | Adicionada propriedade `Password`. |
| `Fika-Server-CSharp` | `FikaRaidCreateRequestData.cs` | Adicionada propriedade `Password`. |
| `Fika-Server-CSharp` | `FikaRaidJoinRequestData.cs` | Adicionada propriedade `Password`. |
| `Fika-Server-CSharp` | `FikaMatch.cs` | Adicionados `Password` e helper booleano `HasPassword`. |
| `Fika-Server-CSharp` | `FikaRaidResponse.cs` | Adicionada propriedade `HasPassword`. |
| `Fika-Server-CSharp` | `LocationController.cs` | Mapeado `HasPassword` na lista de raids. |
| `Fika-Server-CSharp` | `MatchService.cs` | Persistência de `Password` na criação de partidas. |
| `Fika-Server-CSharp` | `RaidController.cs` | Validação autoritativa de senha no join de raid. |
| `Fika-Server-CSharp` | `FikaServer.csproj` | Bump SemVer para `2.4.0`. |
| `Fika-Headless` | `FikaHeadlessPlugin.cs` | Repasse de `request.Password` para `CreateMatch` e bump para `1.5.0`. |
| `Fika.Core` | `StartHeadlessRequest.cs` | Adicionado DataMember `Password`. |
| `Fika.Core` | `CreateMatchRequest.cs` | Adicionado campo `Password` na struct e construtor. |
| `Fika.Core` | `MatchJoinRequest.cs` | Adicionado campo `Password` na struct e construtor. |
| `Fika.Core` | `LobbyEntry.cs` | Adicionado campo `HasPassword` na struct e construtor. |
| `Fika.Core` | `FikaBackendUtils.cs` | Suporte a parâmetro `password` em `JoinMatch` e `CreateMatch`. |
| `Fika.Core` | `FikaServer.cs` | Pinger responde `"fika.hello"` e autoriza conexão `"fika.core"` em raid ativa. |
| `Fika.Core` | `MatchMakerUIScript.cs` | Injeção de input de senha no `DediSelection`, liberação do botão para invasão e modal de entrada. |
| `Fika.Core` | `MainMenuUIScript.cs` | Liberação do botão Join para raids iniciadas e abertura do `JoinSessionModal`. |
| `Fika.Core` | `JoinSessionModal.cs` (NOVO) | Componente reutilizável com layout idêntico a "Configurações de Sessão". |
| `Fika.Core` | `FikaPlugin.cs` / `.csproj` | Bump SemVer para `2.4.0`. |

---

## 3. Checklist de Qualidade e Segurança

- [x] **Compatibilidade reversa:** Partidas sem senha funcionam com valor nulo/em branco sem exigir nada do usuário.
- [x] **Segurança de handshake:** Servidor valida a senha antes de retornar os metadados de join (`ServerGuid`, `RaidCode`).
- [x] **Consistência visual:** `JoinSessionModal` usa a mesma estrutura prefab (`DediSelection`), cores e tipografia de Tarkov/FIKA.
- [x] **Tratamento de exceções:** Mensagens amigáveis para senha incorreta sem quebrar a UI do cliente.
- [x] **Isolamento de build:** Compilado 100% no fork `mods/FIKA/modded-V2/`, sem contaminar `modded/` nem paths locais do jogo.

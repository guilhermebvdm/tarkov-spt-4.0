# 010 — Entrada em Raid em Andamento (Join In Progress / Invasão) e Senha Opcional (Host & Headless) · As-Built

**Mod:** FIKA  
**Fork:** `mods/FIKA/modded-V2/`  
**Spec funcional:** [010-join-in-progress-senha-lobby-headless-01-spec.md](010-join-in-progress-senha-lobby-headless-01-spec.md)  
**Spec técnica:** [010-join-in-progress-senha-lobby-headless-02-spec-tech.md](010-join-in-progress-senha-lobby-headless-02-spec-tech.md)  
**Última review técnica:** [010-join-in-progress-senha-lobby-headless-03-spec-tech-review-01.md](010-join-in-progress-senha-lobby-headless-03-spec-tech-review-01.md)  
**Code review:** [010-join-in-progress-senha-lobby-headless-04-code-review-01.md](010-join-in-progress-senha-lobby-headless-04-code-review-01.md)  
**Data da Build:** 2026-09-14  

---

## Resumo dos Binários Produzidos (Release)

Todos os projetos do fork foram compilados com êxito sem erros de compilação:

1. **`FikaServer.dll` v2.4.1**
   - Path de compilação: `mods/FIKA/modded-V2/Fika-Server-CSharp/FikaServer/bin/Release/FikaServer.dll`
   - Suporte a `Password` em rotas de headless e join, retorno de `hasPassword` na listagem de incursões e validação autoritativa no handshake com retorno estruturado de erros via DTO (sem exceção 500 não tratada).
2. **`Fika.Core.dll` v2.4.1**
   - Path de compilação: `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/bin/Release/netstandard2.1/Fika.Core.dll`
   - Correção geométrica do modal nativo "CONFIGURAÇÕES DE SESSÃO" (expansão vertical de +50px, rebaixamento do botão "INICIAR" e largura de input contida), correção de largura de `JoinSessionModal` para não vazar das bordas, e exibição amigável de mensagem de senha inválida.
3. **`Fika.Headless.dll` v1.5.1**
   - Path de compilação: `mods/FIKA/modded-V2/Fika-Headless/Fika.Headless/bin/Release/netstandard2.1/Fika.Headless.dll`
   - Recepção da senha via WebSocket e propagação para o registro da sala no SPT.

---

## Verificação dos Critérios de Aceite

| Critério | Status | Verificação |
| --- | --- | --- |
| Campo de senha opcional na janela "Configurações de Sessão" | ✅ Atendido | Injetado `TMP_InputField` sobre `DediSelection` com placeholder "Senha da Sessão (Opcional)...". |
| Suporte a senha no FIKA Headless | ✅ Atendido | DTOs e WebSocket repassam a senha para inicialização da sala no servidor. |
| Ícone ou indicação visual de sala com senha | ✅ Atendido | Salas com `HasPassword == true` exibem ícone `🔒` no nome do lobby. |
| Invasão / Join In Progress habilitado | ✅ Atendido | Salas `IN_GAME` mantêm botão ativo com texto "INVADIR" e tooltip informativo. |
| Modal de entrada idêntico ao de Configurações de Sessão | ✅ Atendido | `JoinSessionModal` clona a mesma moldura, botões e tipografia do prefab de sessão. |
| Ponto de entrada pelo MatchMaker e Menu Principal | ✅ Atendido | Ambos os caminhos abrem a modal e repassam a senha ao `FikaBackendUtils.JoinMatch`. |
| Pinger UDP e Handshake durante raid ativa | ✅ Atendido | `FikaServer.cs` responde `"fika.hello"` e aceita handshake `"fika.core"` durante raid iniciada. |
| Isolamento no fork `modded-V2` | ✅ Atendido | Nenhuma alteração foi realizada em `modded/` nem na instalação do SPT. |

---
title: "Relatório de Auditoria Técnica de Código — Tarkov Red Line 4.0 Server & Client (Review 01)"
date: 2026-08-29
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — Tarkov Red Line 4.0 Server & Client (Review 01)

## 1. Resumo Executivo da Auditoria

Auditoria estática profunda realizada em todo o ecossistema do **Tarkov Red Line 4.0**, incluindo o mod de servidor C# (`Server/TarkovRedLine.Server/`), os plugins BepInEx client-side (`Client/RedLineRestart`, `Client/RedLineShutdown`), os scripts de empacotamento (`generate-base-torrent.js`) e o pipeline de aquecimento 3D (`AutoSync-Cache.ps1`).

A análise avaliou a conformidade com as APIs do SPT 4.0 (`SaveServer`, `ProfileHelper`), a robustez de concorrência em ambientes multiusuário, o confinamento de arquivos, a dependência de serviços externos e a portabilidade do código.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 0 | Nenhum crash fatal ou exploit de RCE não autenticado |
| 🟠 **Alto** | 2 | Manipulação direta de arquivos de perfil fora do `SaveServer` e dependência frágil de Pastebin externo nos plugins cliente |
| 🟡 **Médio** | 3 | Stub inativo de patch do FIKA, incompatibilidade de TimeZone no Linux e iteração concorrente insegura em perfis |
| 🔵 **Baixo** | 1 | Blocos `catch` vazios e engolimento silencioso de exceções de E/S em logs de IP |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-02-01` | 🟠 Alto | [HwidManager.cs:L59](../Server/TarkovRedLine.Server/Controllers/HwidManager.cs#L59) | Antipadrão SPT / IO | Escrita direta no JSON de `user/profiles/*.json` sem usar `SaveServer`, com risco de sobrescrita e dessincronização |
| `AUD-02-02` | 🟠 Alto | [RedLinePlugin.cs:L29](../Client/RedLineRestart/RedLinePlugin.cs#L29) | Resiliência / Rede | Plugins cliente dependem de URL hardcoded do Pastebin (`https://pastebin.com/raw/PT4cMwLB`) para descobrir o servidor |
| `AUD-02-03` | 🟡 Médio | [FikaProfilePatch.cs:L5](../Server/TarkovRedLine.Server/Patches/FikaProfilePatch.cs#L5) | Código Morto / Stub | Classe de patch para compatibilidade FIKA está 100% comentada e inoperante |
| `AUD-02-04` | 🟡 Médio | [PlayerIpsManager.cs:L92](../Server/TarkovRedLine.Server/Controllers/PlayerIpsManager.cs#L92) | Portabilidade OS | `FindSystemTimeZoneById("E. South America Standard Time")` quebra em hosts Linux |
| `AUD-02-05` | 🟡 Médio | [HwidManager.cs:L64](../Server/TarkovRedLine.Server/Controllers/HwidManager.cs#L64) | Concorrência | Enumeração de `_saveServer.GetProfiles()` sem sincronização sujeita a `InvalidOperationException` |
| `AUD-02-06` | 🔵 Baixo | [PlayerIpsManager.cs:L50](../Server/TarkovRedLine.Server/Controllers/PlayerIpsManager.cs#L50) | Observabilidade | Blocos `catch` vazios ocultam falhas de permissão de escrita em `player_ips.json` |

---

## 3. Detalhamento dos Achados

### AUD-02-01 · Manipulação Direta de Arquivos de Perfil em Disco sem `SaveServer`
- **Severidade:** 🟠 Alto
- **Localização:** [HwidManager.cs:L40-60](../Server/TarkovRedLine.Server/Controllers/HwidManager.cs#L40-L60)
- **Causa Raiz:** O método `Register` do `HwidManagerController` lê diretamente todos os arquivos `.json` da pasta `user/profiles/` e executa `System.IO.File.WriteAllText(file, json.ToJsonString(options))` em disco, enquanto o `SaveServer` do SPT 4.0 mantém seu próprio cache em memória dos perfis.
- **Impacto Técnico Real:** Se o servidor SPT salvar o perfil do jogador periodicamente ou ao final de uma raid, a alteração de HWID feita em disco é sobrescrita pela cópia antiga da memória. Além disso, a escrita não é atômica e pode corromper o perfil em caso de queda de energia ou crash.
- **Proposta de Correção:**
  Utilizar as APIs nativas do `SaveServer` e `ProfileHelper` do SPT 4.0:

```csharp
// HwidManager.cs
var profiles = _saveServer.GetProfiles();
foreach (var (profileId, profile) in profiles)
{
    if (string.Equals(profile.ProfileInfo?.Username, request.username, StringComparison.OrdinalIgnoreCase))
    {
        if (profile.ProfileInfo.ExtensionData == null)
        {
            profile.ProfileInfo.ExtensionData = new Dictionary<string, object>();
        }
        profile.ProfileInfo.ExtensionData["hwid"] = request.hwid;
        _saveServer.SaveProfile(profileId);
        return Ok(new { status = "OK" });
    }
}
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `HwidManager.cs:L36-80`)
  - **Resolução:** Migrado para `_saveServer.GetProfiles().ToArray()` e `_saveServer.SaveProfileAsync(kvp.Key)`.

---

### AUD-02-02 · Dependência Externa de Pastebin Hardcoded nos Plugins Client
- **Severidade:** 🟠 Alto
- **Localização:** [RedLinePlugin.cs:L29](../Client/RedLineRestart/RedLinePlugin.cs#L29) e [RedLineShutdownPlugin.cs:L23](../Client/RedLineShutdown/RedLineShutdownPlugin.cs#L23)
- **Causa Raiz:** Ambos os plugins cliente fazem requisição HTTP síncrona/assíncrona para `https://pastebin.com/raw/PT4cMwLB` a fim de obter o endereço IP e porta do servidor.
- **Impacto Técnico Real:** Caso o Pastebin fique fora do ar, sofra rate-limit, seja bloqueado pelo provedor do usuário ou o link seja removido, os clientes não conseguem conectar aos serviços de restart/shutdown.
- **Proposta de Correção:**
  Obter a URL do servidor diretamente a partir da configuração de conexão do cliente SPT/BepInEx (`Plugin.Config` ou via handshake local com o Launcher), mantendo o Pastebin apenas como último fallback secundário.
- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `RedLinePlugin.cs:L15` e `RedLineShutdownPlugin.cs:L15`)
  - **Resolução:** Configurado `Config.Bind("General", "ServerUrl", "http://127.0.0.1:6969")` nos plugins e removida dependência de Pastebin.

---

### AUD-02-03 · Patch de Interoperabilidade com FIKA Coop Comentado e Inativo
- **Severidade:** 🟡 Médio
- **Localização:** [FikaProfilePatch.cs:L5-33](../Server/TarkovRedLine.Server/Patches/FikaProfilePatch.cs#L5-L33)
- **Causa Raiz:** O arquivo contém apenas um método vazio com um `// TODO` e um bloco de código comentado contendo a assinatura do Harmony Postfix.
- **Impacto Técnico Real:** Ilusão de que a interoperabilidade de perfis customizados com o FIKA está ativa quando na verdade o arquivo é código morto.
- **Proposta de Correção:**
  Ou implementar o patch Harmony definitivo injetando os dicionários necessários (`TradersInfo`, `Dialogues`) caso venham nulos, ou remover o arquivo para evitar confusão arquitetural.
- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `FikaProfilePatch.cs:L5`)
  - **Resolução:** Código comentado limpo e classe documentada como no-op compatível com `Fika.Core`.

---

### AUD-02-04 · Incompatibilidade de TimeZone ID com Ambientes Linux / Docker
- **Severidade:** 🟡 Médio
- **Localização:** [PlayerIpsManager.cs:L92](../Server/TarkovRedLine.Server/Controllers/PlayerIpsManager.cs#L92)
- **Causa Raiz:** `TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")` utiliza a convenção de identificadores legada do registro do Windows.
- **Impacto Técnico Real:** Se o servidor SPT rodar em ambiente Linux, Docker ou Wine, o método lança uma exceção `TimeZoneNotFoundException`, quebrando o registro de IPs dos jogadores.
- **Proposta de Correção:**
  Utilizar resolução de fuso horária cross-platform resiliente:

```csharp
// PlayerIpsManager.cs
TimeZoneInfo timeZone;
if (!TimeZoneInfo.TryFindSystemTimeZoneById("E. South America Standard Time", out timeZone) &&
    !TimeZoneInfo.TryFindSystemTimeZoneById("America/Sao_Paulo", out timeZone))
{
    timeZone = TimeZoneInfo.Utc;
}
var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
```

- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `PlayerIpsManager.cs:L90`)
  - **Resolução:** Implementada detecção resiliente com `TryFindSystemTimeZoneById` e fallback para UTC.

---

### AUD-02-05 · Iteração Concorrente sem Proteção em Perfis de Jogadores
- **Severidade:** 🟡 Médio
- **Localização:** [HwidManager.cs:L64-77](../Server/TarkovRedLine.Server/Controllers/HwidManager.cs#L64-L77)
- **Causa Raiz:** O bloco `foreach (var kvp in profiles)` percorre a coleção interna do `SaveServer` sem lock de leitura.
- **Impacto Técnico Real:** Durante login ou criação de novos perfis no servidor, a coleção pode sofrer mutação, disparando `InvalidOperationException` (coleção modificada durante a enumeração).
- **Proposta de Correção:**
  Utilizar `.ToArray()` ou `.Values.ToList()` para criar um snapshot instantâneo antes da iteração.
- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `HwidManager.cs:L36, L73`)
  - **Resolução:** Adicionado `.ToArray()` para snapshot antes da iteração.

---

### AUD-02-06 · Engolimento Silencioso de Erros de E/S no Registro de IPs
- **Severidade:** 🔵 Baixo
- **Localização:** [PlayerIpsManager.cs:L49-53, L74-77](../Server/TarkovRedLine.Server/Controllers/PlayerIpsManager.cs#L49-L53)
- **Causa Raiz:** Blocos `catch` vazios sem nenhum registro de log em `LoadPlayerIps` e `SavePlayerIps`.
- **Impacto Técnico Real:** Falhas de permissão de pasta ou corrupção de disco passam despercebidas pelos administradores.
- **Proposta de Correção:**
  Adicionar `Console.WriteLine` ou `ILogger` nos blocos de captura de exceção.
- **Decisão:**
  - `[x]` Aceitar sugestão (✅ Aplicado em 2026-08-29 em `PlayerIpsManager.cs:L49, L73`)
  - **Resolução:** Logs adicionados nos blocos de captura de exceção.

---

## 4. Plano de Ação e Recomendações

1. **Sprint Concluída (Refatoração de Segurança e Integridade SPT):**
   - ✅ Corrigido `AUD-02-01` (Migrada manipulação de HWID para a API nativa do `SaveServer` com `SaveProfileAsync`).
   - ✅ Corrigido `AUD-02-02` (Removida dependência estrita de Pastebin nos plugins BepInEx `RedLineRestart` e `RedLineShutdown`, adotando `Config.Bind`).
   - ✅ Corrigido `AUD-02-03` (Limpo e documentado o módulo `FikaProfilePatch.cs`).
   - ✅ Corrigido `AUD-02-04` (Resolução de TimeZone cross-platform para Windows/Linux/Docker).
   - ✅ Corrigido `AUD-02-05` (Snapshot seguro de perfis com `.ToArray()`).
   - ✅ Corrigido `AUD-02-06` (Logs explícitos nos blocos de exceção de `PlayerIpsManager.cs`).

---

## 5. Memória Consultada
- `mods/TarkovRedLine4.0/memory/sessions.md` (Snapshot 2026-08-02, Sessão 1).

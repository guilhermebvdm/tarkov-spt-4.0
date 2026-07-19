# 001 — portabilidade-spt-4 · Code Review 05 (Comunicação e Fika)

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** Não aplicável
**Data:** 2026-07-17T23:15:00-03:00

> Análise crítica focada na arquitetura de comunicação cliente-servidor e na compatibilidade multiplayer com Fika (Plugin, Headless e Server) e SPT-Source.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-05-01 | D — Arquitetura | 🟠 Forte | IsHostOrSolo Falso-Positivo em Clientes Fika | Pendente |
| CR-05-02 | D — Arquitetura | 🟡 Médio | Falta de Tratamento de Erros e Timeouts em Requisições HTTP | Pendente |

---

## Pontos

### CR-05-01 · D — Arquitetura · 🟠 Forte

**IsHostOrSolo Falso-Positivo em Clientes Fika**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs:171-174`](../../Client/Components/BotDespawnManager.cs#L171-L174)

**Problema:**
A função `IsHostOrSolo()` está hardcoded para retornar sempre `true`:
```csharp
private bool IsHostOrSolo()
{
    return true;
}
```

**Por que importa:**
Em sessões cooperativas multiplayer da Fika, os clientes conectados (que não são o Host/Servidor local da raid) rodarão a rotina de despawn paralelamente ao Host. Isso gerará conflitos de sincronização, onde múltiplos computadores tentarão deletar/despawnar e pedir reposição dos mesmos bots na mesma raid coop.

**Sugestão:**
Refatorar para consultar se a sessão Fika ativa é do tipo Host ou se é partida Solo usando as propriedades oficiais da Fika (ex: `Fika.Core.Main.Utils.FikaBackendUtils.IsServer` ou `ClientType == EClientType.Host`):
```csharp
private bool IsHostOrSolo()
{
    // Se a Fika estiver acoplada e ativa, apenas o Host (IsServer) processa spawns/desspawns
    try
    {
        return Fika.Core.Main.Utils.FikaBackendUtils.IsServer || Fika.Core.Main.Utils.FikaBackendUtils.IsSinglePlayer;
    }
    catch
    {
        return true; // Fallback caso Fika.Core não esteja carregada (Modo Solo Puro)
    }
}
```

---

### CR-05-02 · D — Arquitetura · 🟡 Médio

**Falta de Tratamento de Erros e Timeouts em Requisições HTTP**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:51`](../../Client/Components/DynamicSpawnManager.cs#L51)

**Problema:**
As rotas de comunicação cliente-servidor usam chamadas HTTP bloqueantes síncronas diretamente no início do carregamento:
```csharp
string json = RequestHandler.GetJson("/trldynamicspawn/getConfig");
```

**Por que importa:**
Caso o servidor SPT falhe em responder imediatamente por sobrecarga ou atraso de porta, a thread principal do cliente congelará (stutter) ou lançará uma exceção crítica que interromperá o fluxo de carregamento da raid.

**Sugestão:**
Proteger as requisições envolvendo em blocos try-catch apropriados (já parcialmente feito, mas que necessita de tratamento de fallbacks locais consistentes com valores padrão de segurança).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 05 criada via `/code-review` |

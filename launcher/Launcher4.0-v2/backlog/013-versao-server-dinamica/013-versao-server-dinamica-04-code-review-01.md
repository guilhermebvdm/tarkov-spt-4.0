# 013 — Versão do server dinâmica · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Commit revisado:** `4fb26d7` · **Insumo:** [02-spec-tech](./013-versao-server-dinamica-02-spec-tech.md)

> Review de contexto limpo (revisor não escreveu o código). Escopo: `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/ServerVersionController.cs` (novo) + mudança de visibilidade em `LauncherUpdaterController.cs`. Build gate: `dotnet build TarkovRedLine.Server.csproj -c Release` → **0 erros** (33 warnings pré-existentes de nullability, mesmos da spec).

**Placar:** 0 🔴 · 0 🟡 · 2 🟢

---

## ServerVersionController.cs

### CR-01-01 [🟢 menor] Arquivo multi-linha vaza newline interno para dentro do JSON

`ServerVersionController.cs:32`: `ReadAllText(...).Trim()` só apara as pontas. Cenário: alguém edita `server-version.txt` e deixa uma segunda linha (nota, changelog, linha em branco no meio) → `version` vira `"1.2.0\r\nhotfix"` → o launcher exibe a string com quebra escapada no footer. **Fix:** ler só a primeira linha não-vazia (`File.ReadLines(path).FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim()`).

### CR-01-02 [🟢 menor] Fallback 100% silencioso — sem log, misconfig fica indistinguível de "sem arquivo"

`ServerVersionController.cs:40-43`: `catch { }` sem log. O fallback está no contrato congelado (comportamento correto), mas dois cenários ficam invisíveis: (a) arquivo existe porém ilegível por permissão → endpoint devolve `0.1.0-beta` para sempre e ninguém descobre por quê; (b) corrida com o deploy que reescreve o txt → sharing violation momentânea → default transiente. **Fix:** logar um warning no catch (uma linha; o projeto usa `Console.WriteLine`/logger nos outros controllers) — sem mudar o contrato.

---

## Áreas auditadas, sem achados

- **Contrato `{ "version": ... }` / camelCase:** o tipo anônimo `new { version }` já nasce com propriedade minúscula — sai `"version"` com qualquer naming policy do serializer. O vizinho `LauncherUpdaterController.GetLauncherVersion` (`/redline/launcher/version`) usa o mesmo padrão e já é consumido em produção pelo launcher. Contrato confere com a spec.
- **Encoding/BOM:** `File.ReadAllText` faz BOM sniffing (UTF-8/16/32) e descarta o BOM — inclusive o UTF-16 LE default do PowerShell 5.1 `Out-File`. Sem cenário de BOM vazando para a string.
- **Exceções de IO:** `File.Exists` + try/catch cobrindo `GetUpdaterBasePath()` (que não lança — tem fallback próprio na linha 30 do `LauncherUpdaterController`), leitura e trim; todo caminho termina em `200 OK` com versão válida. Nenhum 500 possível.
- **Cache:** leitura a cada request, sem cache — consistente com o vizinho (`GetLauncherVersion` lê `FileVersionInfo` do exe a cada request) e é o que habilita o requisito "editar o txt sem rebuild". Volume de tráfego (launcher no connect) não justifica cache.
- **Conflito de rota:** varrido todos os controllers do projeto — `redline/server` é prefixo novo; nenhuma outra action resolve para `redline/server/version` (os `[Route("redline")]` existentes usam paths `password/*`, `vote/*`, `player-ips`, `register-ip`, `profile/get`). Sem ambiguidade.
- **Visibilidade `internal` de `GetUpdaterBasePath`:** mesmo assembly, sem exposição externa; `GetLauncherExePath` continua `private`. Mudança mínima correta.

# B-2 · Transformar o TRL Items Management num MOD do SPT

> **Status:** 🟢 Spec (SDD) + **spike** · **Data:** 2026-07-04 · **Gate:** as features de UI (B-1/B-4 e a UI do B-3) nascem aqui · **Ref:** [BACKLOG.md](../BACKLOG.md) B-2

## 1. Funcional

**Objetivo:** o viewer deixa de ser um app Node separado (`serve.js`) e vira **um mod SPT** que serve a UI e a API pela HTTP do próprio SPT, instala em `user/mods/`, sobe junto com o servidor. Ganhos: 1 instalação, **auto-start** (resolve P-1.2), sem processo Node no runtime, sem `serve.js`/`update-vm.ps1`.

**Milestone 1 (esta rodada = spec + spike; port completo depois):**
- O mod serve `index.html` + assets + `data/items.json` como **estáticos** e expõe os endpoints do `serve.js` como **rotas C#**. A UI vanilla-JS **é reaproveitada**. O catálogo (`items.json`) **continua gerado pelo build Node** (`load-spt`+`normalize`) fora do mod.
- **Milestone 2 (fora do escopo):** portar `load-spt`/`normalize` p/ C# in-process.

**Critérios de aceite (M1):**
1. Server sobe com o mod carregado (`versão: ...` no log, sem erro de ModValidator).
2. `https://127.0.0.1:6969/TRLItemsManagement-Server/index.html` abre a UI.
3. Editar flea / trader / ban / nível pela UI servida pelo mod → grava nos mesmos arquivos (`configs/ragfair.json`, `items.json`, `globals.json`) + refresh `checks.dat`, **idêntico ao `serve.js`**.
4. **Nenhum processo Node no runtime** (só no build do catálogo).
5. `.env` (market key) e `overrides.json` (trader) preservados.

## 2. Técnico — arquitetura (validada por pesquisa, ver refs)

Novo projeto **`TRLItemsManagement.Server.csproj`** = cópia de `mods/CustomClasses/modded/Server/CustomClasses.Server.csproj`:
- `Sdk="Microsoft.NET.Sdk.Web"`, `OutputType=Library` (Sdk.Web default é Exe), `AssemblyName=TRLItemsManagement-Server` (**vira o prefixo de URL** `/TRLItemsManagement-Server/`), packages `SPTarkov.Server.{Core,Web},DI,Common` **4.0.2**.
- Metadata `record : AbstractModMetadata, IModWebMetadata` (o marcador `IModWebMetadata` de `SPTarkov.Server.Web` é o que **opta** o assembly no pipeline web — `SPTWeb.cs:16`).

**UI estática:** `wwwroot/index.html` + `wwwroot/assets/*` + `wwwroot/data/items.json` → servidos em `/{AssemblyName}/...` (`SPTWeb.cs:53-69`, `UseStaticFiles`).

**API:** **controllers ASP.NET** `[ApiController][Route("TRLItemsManagement-Server/api/...")]` (habilitados por `AddControllers().AddApplicationPart` + `MapControllers()`, `SPTWeb.cs:21-37`). Suportam **todos os verbos incl. PATCH/DELETE**, JSON não-comprimido, **mesma origem → sem CORS**. Injetar `ModHelper`/`FileUtil`/`JsonUtil` do DI compartilhado; escrita atômica + backups + **guard de path-traversal** (copiar `ClassEditorService.TryResolveClassFile:656-675`).

**Bind:** Kestrel única do SPT, `https://127.0.0.1:6969` (self-signed) — `http.json` `ip/port`. Sem porta própria. LAN/Fika = mudar `http.json ip` (global). [ver [[reference_spt_web_bind_ip]]]

**Refs de pesquisa (SPT source, read-only):** `SPTarkov.Server.Web/SPTWeb.cs` (wiring estático+controller+blazor), `Program.cs:195-232` (Kestrel), `Modding/ModValidator.cs:316-335` (rejeição `.js`/`.ts`), `configs/http.json`. Precedente: `mods/CustomClasses/modded/Server/{CustomClasses.Server.csproj, CustomClassesMetadata.cs, ClassEditorService.cs, wwwroot/**}`.

### GOTCHAS bloqueantes (da pesquisa)
- **A · `.js`/`.ts` = mod rejeitado.** `ModValidator` varre a pasta do mod recursivamente e rejeita o mod inteiro se achar QUALQUER `.js`/`.ts` (`ModValidator.cs:316-335`). → o **build Node (`load-spt`/`serve.js`/`normalize`) NÃO pode ficar dentro do mod**; fica em `tools/trl-items-management/` e só o **produto** (`index.html`+assets+`items.json`) entra no `wwwroot`. Scripts de browser: renomear `.js`→**`.mjs`** e `<script type="module">`. (A UI atual usa JS **inline** no `index.html` → ok; conferir se há `.js` externo.)
- **B · `IModWebMetadata` obrigatório** senão nem controller nem wwwroot registram.
- **C · Sem default document:** `/{AssemblyName}/` (raiz) dá 404 → linkar `index.html` explícito ou um controller no root que redireciona.
- **D · Mesma origem só:** sem CORS configurado no host → servir a UI do `wwwroot` (mesma 6969). As chamadas `fetch('/api/...')` do `index.html` mudam de `:8080/api/...` para `/TRLItemsManagement-Server/api/...`.
- **E · HTTPS self-signed** → aviso do browser na 1ª visita.

### Port dos endpoints (serve.js → controllers C#) — M1
`/api/price` (POST/DELETE), `/api/trader-price` (PATCH/DELETE + `/all`), `/api/ban` (POST), `/api/flea-level`, GETs de overrides, GET `data/items.json`. Cada um replica a escrita atômica + `checks.dat` do `serve.js`. **Deferido:** o `update-vm.ps1`/`package-release.sh` mudam (mod entra por `user/mods/`, não mais o zip do tool).

## 3. Spike (esta rodada) — de-risk
Mod mínimo `TRLItemsManagement-Server` que (a) serve um `wwwroot/index.html` de teste + (b) 1 controller `GET /TRLItemsManagement-Server/api/ping` → `{ok:true}`. Build + boot do SPT + verificar via rota que a página e a API respondem 200. **Prova a premissa inteira do B-2 sem portar nada.**

## 4. Verificação automatizável
- `dotnet build` do projeto spike exit 0.
- Boot do SPT: mod no log sem erro de ModValidator.
- `GET https://127.0.0.1:6969/TRLItemsManagement-Server/index.html` → 200 + HTML; `GET .../api/ping` → 200 `{ok:true}`.
- **Pendente in-game:** UX completa (editar pela UI servida) exige o port dos endpoints (M1) + validação visual.

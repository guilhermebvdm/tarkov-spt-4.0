# Workspace SPT 4.0

## Versões

- **SPT:** 4.0.x — [PREENCHER versão exata]
- **Tarkov:** [PREENCHER]
- **Game install path:** `[PREENCHER]` (ex: `C:\Program Files (x86)\Tarkov`)

## Tipos de mod

### Client (C# / BepInEx)
- **Local:** `mods/client/<NomeDoMod>/`
- **Compilação:** `dotnet build` → DLL em `bin/Release/net471/`
- **Deploy:** copiar DLL para `<game-path>/BepInEx/plugins/`

### Server (TypeScript)
- **Local:** `mods/server/<NomeDoMod>/`
- **Requisitos:** `package.json` com campo `sptVersion: 4.0.x`
- **Deploy:** copiar pasta para `<game-path>/user/mods/`

### Dependências
- **Local:** `mods/deps/<NomeDep>/`
- Mods que outros mods dependem (ex: BigBrain, Waypoints)

## Avisos críticos

- **SPT 3.x ≠ 4.0** — arquiteturas incompatíveis; nunca portar código diretamente
- **Game fechado** ao copiar DLLs (locks de arquivo no Windows)
- **JSONC files:** sempre usar parser AST, nunca regex (causa corrupção silenciosa)

## Referências

- Assembly-CSharp do jogo: `deps/Assembly-CSharp/` (read-only, quando existir)

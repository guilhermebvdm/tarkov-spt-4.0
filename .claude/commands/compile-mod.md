# /compile-mod

Compila um mod (client BepInEx ou server TypeScript) e instala automaticamente em `D:\SPT` no destino correto conforme o tipo.

## Uso

```
/compile-mod <ref> [--spt-path <path>] [--flat]
```

- `<ref>` — nome da pasta em `mods/`, path da pasta do mod, ou path de qualquer arquivo dentro.
- `--spt-path <path>` — sobrescreve o path do SPT (default: `D:/SPT`, ou env var `SPT_PATH`).
- `--flat` — (somente client) instala o `.dll` direto em `BepInEx/plugins/` sem subfolder.

## O que fazer

1. **Resolver `<ref>`** → `<mod>` (nome da pasta em `mods/`).
2. **Repassar para o script:**
   ```bash
   bash .agents/scripts/compile-mod.sh <mod> [flags]
   ```
3. Se o script falhar, mostrar o erro e parar.
4. Em sucesso, confirmar ao usuário:
   - Tipo detectado (client-csharp / server-typescript / server-csharp)
   - Path do build local (`mods/<mod>/builds/`)
   - Path de instalação no SPT (`BepInEx/plugins/<assembly>/` ou `SPT/user/mods/<mod>/`)

## O que o script faz

### Detecção do tipo

| Indicador em `mods/<mod>/modded/` | Tipo |
|---|---|
| `*.csproj` referenciando `BepInEx` ou `Assembly-CSharp` | **client-csharp** |
| `package.json` (TypeScript) | **server-typescript** |
| `*.csproj` sem BepInEx (server-side C#) | **server-csharp** (não suportado ainda) |

### Build

- **client-csharp:**
  - Lê `<AssemblyName>` do `.csproj` (ou usa o nome do arquivo).
  - `dotnet build <csproj> -c Release -o mods/<mod>/builds/`
  - Requer .NET SDK instalado (`dotnet --version`).
- **server-typescript:**
  - `cd modded/ && npm install`
  - Se houver script `build` no `package.json`, roda `npm run build`.
  - Copia `modded/` (sem `node_modules`) para `mods/<mod>/builds/`.
  - Requer Node.js + npm.

### Instalação no SPT

| Tipo | Destino (default) | Destino (com `--flat`) |
|---|---|---|
| client-csharp | `D:/SPT/BepInEx/plugins/<AssemblyName>/<AssemblyName>.dll` | `D:/SPT/BepInEx/plugins/<AssemblyName>.dll` |
| server-typescript | `D:/SPT/SPT/user/mods/<mod>/` | — |

- Pasta `.pdb` (símbolos de debug) é copiada junto se existir.
- Para server TS, instalação usa `rsync --delete` para sincronizar (apaga arquivos órfãos no destino).

## Regras

- Compila em `mods/<mod>/builds/` mesmo se `D:\SPT` não existir (build local sempre roda).
- Não toca em `mods/<mod>/original/` nem outros mods.
- Se houver múltiplos `.csproj` em `modded/`, falha (ambíguo).
- `dotnet`/`npm` precisam estar no `PATH`. Sem eles, falha com mensagem clara.

## Exemplos

```bash
# Client mod com subfolder
/compile-mod stancesAndCameraPositionSPT4.0.11

# Mesmo, com instalação flat (DLL direto na raiz de plugins/)
/compile-mod stancesAndCameraPositionSPT4.0.11 --flat

# SPT em outro lugar
/compile-mod meuModServer --spt-path E:/SPT-Dev
```

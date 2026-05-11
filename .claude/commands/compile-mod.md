# /compile-mod

Compila um mod (client BepInEx ou server TypeScript) e instala automaticamente em `D:\SPT` no destino correto conforme o tipo.

## Uso

```
/compile-mod <ref> [--spt-path <path>] [--flat] [--clean]
```

- `<ref>` — nome da pasta em `mods/`, path da pasta do mod, ou path de qualquer arquivo dentro.
- `--spt-path <path>` — sobrescreve o path do SPT (default: `D:/SPT`, ou env var `SPT_PATH`).
- `--flat` — (somente client) instala o `.dll` direto em `BepInEx/plugins/` sem subfolder.
- `--clean` — apaga `mods/<mod>/builds/` antes de compilar, forçando rebuild completo (útil quando o cache do MSBuild gera artefatos stale).

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

### Resolução automática de referências (client-csharp)

Antes de compilar, o script verifica se `modded/References/` tem os DLLs necessários. Se estiverem faltando, copia automaticamente até 13 DLLs do SPT install:

| DLL | Origem |
|---|---|
| `BepInEx.dll`, `0Harmony.dll` | `D:/SPT/BepInEx/core/` |
| `Assembly-CSharp.dll`, `UnityEngine*.dll`, `Comfort.dll`, `Sirenix.Serialization.dll`, `AnimationSystem.Types.dll` | `D:/SPT/EscapeFromTarkov_Data/Managed/` |
| `SPT.Reflection.dll`, `SPT.Common.dll` | `D:/SPT/BepInEx/plugins/spt/` |

Não sobrescreve DLLs que já existem em `References/`. Se o SPT install não existir, o passo é silenciosamente ignorado (o build vai falhar com erro de referência do MSBuild).

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

- Se já existir um `.dll` no destino, é feito backup automático como `<AssemblyName>.dll.bak` antes de sobrescrever.
- `.pdb` (símbolos de debug) é copiado junto se existir.
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

# Rebuild completo (limpa cache do MSBuild)
/compile-mod stancesAndCameraPositionSPT4.0.11 --flat --clean

# SPT em outro lugar
/compile-mod meuModServer --spt-path E:/SPT-Dev
```

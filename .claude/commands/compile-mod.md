# /compile-mod

Compila um mod (client BepInEx ou server TypeScript) e instala automaticamente em `D:\SPT` no destino correto conforme o tipo.

## Uso

```
/compile-mod <ref> [--spt-path <path>] [--flat] [--clean] [--allow-same-version] [--check-version]
```

- `<ref>` — nome da pasta em `mods/`, path da pasta do mod, ou path de qualquer arquivo dentro.
- `--spt-path <path>` — sobrescreve o path do SPT (default: `D:/SPT`, ou env var `SPT_PATH`).
- `--flat` — (somente client) instala o `.dll` direto em `BepInEx/plugins/` sem subfolder.
- `--clean` — apaga `mods/<mod>/builds/` antes de compilar, forçando rebuild completo (útil quando o cache do MSBuild gera artefatos stale).
- `--allow-same-version` — bypassa o gate de versão (recompilar deliberadamente sem bump — ex.: rebuild idêntico para re-deploy). Nunca usar como atalho para não bumpar.
- `--check-version` — só resolve e imprime as versões atuais do mod; não compila nem instala.

## O que fazer

1. **Resolver `<ref>`** → `<mod>` (nome da pasta em `mods/`).
2. **Bump de versão — OBRIGATÓRIO, antes de compilar.** Toda compilação evolui a versão semver `x.y.z` do mod (é a versão que o painel F12 do BepInEx exibe). Critério:

   | Parte | Quando bumpar | Efeito |
   |---|---|---|
   | `z` (patch) | **default** — fix, ajuste, iteração de desenvolvimento | `1.0.2 → 1.0.3` |
   | `y` (minor) | feature nova visível | zera `z`: `1.0.3 → 1.1.0` |
   | `x` (major) | breaking change — config/save/API incompatível | zera `y` e `z`: `1.1.0 → 2.0.0` |

   **Fontes que DEVEM ficar em sincronia** (a canônica é a que o F12 mostra):

   | Fonte | Onde | Vale para |
   |---|---|---|
   | 3º argumento de `[BepInPlugin(guid, name, "x.y.z")]` | `Plugin.cs` (às vezes via constante) | client — **é exatamente o que o F12 exibe** |
   | `<Version>` | `.csproj` | client + server C# |
   | metadata (ex.: `SemanticVersioning.Version`) | `*Metadata.cs` | server C# |
   | `"version"` | `package.json` | server TS |

   O script **falha antes do build** se a versão não evoluiu desde o último compile (estado por mod em `mods/<mod>/.last-compile-versions`, não versionado). Se falhar, a resposta é bumpar a versão — não usar `--allow-same-version`.
3. **Repassar para o script:**
   ```bash
   bash .agents/scripts/compile-mod.sh <mod> [flags]
   ```
4. Se o script falhar, mostrar o erro e parar.
5. Em sucesso, confirmar ao usuário — **sempre com a versão em primeiro lugar, nunca omitir**:
   - **Versão do mod: `<antiga> → <nova>`** + critério usado (patch/minor/major e por quê). É a versão que aparecerá no painel F12 (Plugin/mod settings) após reiniciar o cliente — o bloco `── VERSÃO DO MOD ──` no output do script confirma o valor extraído da fonte real.
   - Se o script avisar `⚠ VERSÃO NÃO DETECTADA` ou `BepInPlugin ≠ csproj`, resolver/sincronizar e reportar explicitamente.
   - Tipo detectado (client-csharp / server-typescript / server-csharp)
   - Path do build local (`mods/<mod>/builds/`)
   - Path de instalação no SPT (`BepInEx/plugins/<assembly>/` ou `SPT/user/mods/<mod>/`)

## O que o script faz

### Gate de versão

Antes de qualquer build, o script extrai a versão de cada projeto (client: 3º arg de `[BepInPlugin]`, resolvendo constantes; server C#: `<Version>` do csproj; server TS: `package.json`) e compara com `mods/<mod>/.last-compile-versions`. Versão igual à do último compile → **exit 1 sem compilar**. Em sucesso, imprime o bloco `── VERSÃO DO MOD (painel F12 / BepInEx) ──` com `antiga → nova` e a fonte, e atualiza o estado. Também avisa quando `BepInPlugin` e `<Version>` do csproj divergem.

### Detecção do tipo

| Indicador em `mods/<mod>/modded/` | Tipo |
|---|---|
| `*.csproj` referenciando `BepInEx` ou `Assembly-CSharp` | **client-csharp** |
| `package.json` (TypeScript) | **server-typescript** |
| `*.csproj` sem BepInEx (server-side C#) | **server-csharp** (não suportado ainda) |

### Resolução automática de referências (client-csharp)

Antes de compilar, o script verifica se `modded/References/` tem os DLLs necessários. Se estiverem faltando, copia automaticamente até 19 DLLs do SPT install:

| DLL | Origem |
|---|---|
| `BepInEx.dll`, `0Harmony.dll` | `D:/SPT/BepInEx/core/` |
| `Assembly-CSharp.dll`, `UnityEngine*.dll`, `Comfort.dll`, `Comfort.Unity.dll`, `Sirenix.Serialization.dll`, `AnimationSystem.Types.dll`, `bsg.console.core.dll`, `DissonanceVoip.dll` | `D:/SPT/EscapeFromTarkov_Data/Managed/` |
| `SPT.Reflection.dll`, `SPT.Common.dll` | `D:/SPT/BepInEx/plugins/spt/` (renomeadas a partir de `spt-reflection.dll`/`spt-common.dll`) |
| `ConfigurationManager.dll` | `D:/SPT/BepInEx/plugins/spt/ConfigurationManager/` |

Não sobrescreve DLLs que já existem em `References/`. Se o SPT install não existir, o passo é silenciosamente ignorado (o build vai falhar com erro de referência do MSBuild).

> ⚠️ **Nunca copiar essas DLLs manualmente.** Se `References/` estiver vazia/incompleta, a resposta é rodar `/compile-mod`, não copiar `.dll` do jogo à mão — o path do jogo é sempre resolvido via `.spt-path` (gitignored, por máquina), nunca hardcoded.

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

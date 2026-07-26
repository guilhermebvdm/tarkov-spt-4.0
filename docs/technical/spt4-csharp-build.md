---
title: Compilar Tarkov Red Line (SPT 4.0)
date: 2026-07-06
status: 🟢 Vivo
authors: Guilherme
---

# Compilar Tarkov Red Line (SPT 4.0)

Este documento descreve os comandos atualizados para gerar as builds do projeto na era do SPT 4.0 (que fez a transição completa de Node.js para .NET 9.0 no backend).

> ⚠️ **Escopo: só o Launcher (`SPT.Launcher`) e o mod de servidor `TarkovRedLine.Server`.** Ambos têm build manual porque fogem do padrão genérico de mod (launcher não é um mod; o server mod é `server-csharp`, tipo ainda não suportado pelo `/compile-mod`).
>
> **Para qualquer mod client C#/BepInEx** (`mods/<mod>/modded/Client/*.csproj`, referenciando `BepInEx`/`Assembly-CSharp`), use **`/compile-mod <mod>`** — ele resolve as DLLs de referência do jogo automaticamente a partir do path configurado em `.spt-path` (ver `AGENTS.md`). **Nunca copiar manualmente** DLLs do jogo para dentro de `modded/.../References/`.

---

## 1. Compilar Launcher (SingleFile)

A partir do SPT 4.0, o Launcher e o SPT rodam sobre o `net9.0`. Para compilar o Launcher como um executável único (sem jogar dezenas de DLLs na pasta raiz do usuário), execute o seguinte comando no PowerShell a partir da raiz do repo:

```powershell
dotnet publish "launcher/Launcher4.0-v2/project/SPT.Launcher/SPT.Launcher.csproj" -c Release -f net9.0 -r win-x64 /p:IncludeNativeLibrariesForSelfExtract=true -p:PublishSingleFile=true --self-contained true
```

### Detalhes Importantes (Launcher)

- **`-f net9.0`**: (Novo no SPT 4.0) Define o target framework correto da nova versão do launcher.
- **`--self-contained true`**: Inclui o runtime .NET embutido. O usuário final não precisa baixar e instalar o .NET 9.0 para jogar.
- **`-p:PublishSingleFile=true`**: Gera um ÚNICO `.exe`, escondendo todas as dependências (`.dll`).
- **`/p:IncludeNativeLibrariesForSelfExtract=true`**: Garante que DLLs nativas em C/C++ sejam compactadas dentro do `.exe` com sucesso.

### Saída (Launcher)

O executável compilado ficará limpo e pronto para uso no caminho:
`launcher/Launcher4.0-v2/project/SPT.Launcher/bin/Release/net9.0/win-x64/publish/Tarkov Red Line.exe`

---

## 2. Compilar Mod do Servidor C#

No SPT 4.0, os mods de servidor não são mais arquivos `mod.ts` (Node.js). Eles agora são bibliotecas de classe (`.dll`) em C# que herdam a estrutura nativa do BepInEx/ASP.NET.

Para compilar o seu Mod do Servidor (TarkovRedLine.Server), basta rodar o comando clássico de build:

```powershell
dotnet build "mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/TarkovRedLine.Server.csproj" -c Release
```

### Detalhes Importantes (Server Mod)

- **SemanticVersioning**: O SPT 4.0 exige a biblioteca `SemanticVersioning` para identificar a compatibilidade do Mod com as versões do jogo. Certifique-se de que a referência esteja em seu `.csproj` (`<PackageReference Include="SemanticVersioning" Version="3.0.0" />`).
- O seu arquivo principal (`Plugin.cs` ou o construtor estático do `AbstractModMetadata`) DEVE obrigatoriamente preencher os metadados (Version, SptVersion, Contributors, etc.).
- **Controllers Nativos**: A partir de agora você não precisa abrir portas secundárias (ex: 7075). Basta criar classes que herdam de `[ApiController]` e o próprio ASP.NET do jogo hospedará suas rotas na porta raiz!

### Saída (Server Mod)

A `.dll` final será gerada na pasta `Release`:
`mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/bin/Release/TarkovRedLine.Server.dll`

*Coloque esta `.dll` na pasta `user/mods/TarkovRedLine.Server/` (junto com o pacote BepInEx se necessário) na pasta principal do jogo para ser inicializada no Boot.*

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
| 2026-07-06 | Guilherme | Frontmatter normalizado (title/date/status/authors); paths absolutos de máquina antiga (`d:\Projetos\GITHUB TARKOV\...`, `Launcher4.0_1.4.0`) corrigidos para relativos ao repo; aviso de escopo adicionado apontando `/compile-mod` + `.spt-path` para mods client genéricos |
| 2026-07-06 | Guilherme | docs(references): adiciona SPT-Waypoints ao manifest e enxuga README |
| 2026-07-26 | Guilherme | docs(technical): reescreve guias de criacao e migracao alinhados ao /compile-mod e ao decompile local |

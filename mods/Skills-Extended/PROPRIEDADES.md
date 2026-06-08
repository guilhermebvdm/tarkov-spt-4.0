# Propriedades F12 — Skills-Extended (cliente)

> **Plugin:** `com.cj.SkillsExtended` (display: `Skills Extended`) · **Versão:** 2.2.2 (constante `SkillsExtendedInfo.VERSION`)<br>
> **Fonte:** [original/Plugin/SkillsExtendedPlugin.cs](original/Plugin/SkillsExtendedPlugin.cs) · binds em [original/Plugin/Config/ConfigManager.cs](original/Plugin/Config/ConfigManager.cs)<br>
> **Aba no F12:** `Skills Extended`

> ⚠️ **A maior parte da configuração NÃO está no F12.** Diferente de mods típicos, o Skills-Extended expõe quase tudo via:
> - **Web UI** — editor em `https://localhost:6969/skills-extended/` (servido pelo `Server/wwwroot/` enquanto o server SPT roda).
> - **JSON do servidor** — [original/Server/Resources/Configs/SkillsConfig.json](original/Server/Resources/Configs/SkillsConfig.json) (~30 KB; XP rates, bônus por nível, listas de armas por categoria, dificuldade de lockpicking por porta/mapa, etc.) e [original/Server/Resources/Configs/ServerConfig.json](original/Server/Resources/Configs/ServerConfig.json) (`CheckForUpdates`).
>
> O F12 (BepInEx ConfigurationManager) contém apenas **uma** opção configurável de verdade.
>
> Nenhuma entrada está marcada como **(Avançado)** (`IsAdvanced`).

---

## Seção: `LP Mini Game`

| # | Nome (EN) | Tradução (pt-BR) | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|-----------|------------------|------|--------|-------|-----------------|
| 1 | Turn Cylinder Key bind | Tecla para girar o cilindro | `KeyCode` | `A` | qualquer tecla | Tecla para girar o cilindro (no minigame de lockpicking). |

---

## Entradas presentes no código mas **não** configuráveis no F12

| Entrada | Onde | Por quê não conta |
|---------|------|-------------------|
| `Mini-game health bar` (`LpMiniEnableHealthBar`) | [ConfigManager.cs](original/Plugin/Config/ConfigManager.cs) (linhas 28-39) | **Comentada** no código — não é registrada. Reservada para uso futuro. |
| `TarkovVersion` | [SkillsExtendedPlugin.cs](original/Plugin/SkillsExtendedPlugin.cs) (~linha 130) e [FikaSync/VersionChecker.cs](original/FikaSync/VersionChecker.cs) (~linha 29) | Bind **condicional**: só ocorre quando a versão do EFT não bate (`TARKOV_VERSION = 40087`). É um label de erro vermelho (`CustomDrawer`/`ReadOnly`/`HideSettingName`, seção vazia), não uma configuração. |

---

## Notas de arquitetura (para futura modificação)

- **Mod híbrido, 6 projetos** numa `.sln`: `Plugin/` (BepInEx, F12 acima), `Server/` (server-side SPT 4.0.2 + web UI), `Common/` (lib interna compartilhada, vendorizada junto), `Prepatcher/` (BepInEx preloader via `Mono.Cecil` — injeta enums de buff / estende `SkillManager` no load), `FikaSync/` (sync multiplayer Fika), `__BUILD_RELEASE__/` (agrega os DLLs no release).
- **Auto-contido** — todos os `ProjectReference` apontam para projetos internos (`..\Common`, `..\Plugin`, etc.). Sem dependência de repo externo. Refs externas: DLLs do EFT (`Assembly-CSharp`, `UnityEngine*`, `spt-*`, `Fika-Core`) + NuGet `SPTarkov.Reflection/Server.Core/Server.Web 4.0.2`.
- **Soft dependencies** (BepInEx): `com.boogle.oldtarkovmovement` (compat lockpicking), `com.fika.core`, `com.fika.headless`.
- **Licença CC BY-NC-ND 4.0** — modificação local pessoal OK; **redistribuição de versão modificada é proibida** pela cláusula NoDerivatives.

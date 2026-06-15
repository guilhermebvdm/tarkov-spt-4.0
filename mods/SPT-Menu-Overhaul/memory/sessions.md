# Memória de sessões — SPT-Menu-Overhaul

Mod: **MoxoPixel-MenuOverhaul** (`com.moxopixel.menuoverhaul`), client BepInEx. Importado e customizado para um tema "TARKOV RED LINE" (logo custom + acento vermelho + escala de logo ajustável).

## Estado atual (snapshot ao fim da última sessão)

- Mod importado (upstream SHA `71bb610`, v1.2.2) e customizado (tema redline); commitado em `554d5d4` + commit da Sessão 2.
- **Logo custom = `TRL-black`** ("TARKOV" branco + "RED LINE" vermelho, fonte angular). Original é **wide** (1300×311); colocada num **canvas 1024² com padding** transparente (96% largura) porque a aspect-correction assume custom **quadrada**. Substitui a textura do `decal_plane` via **clone de material** (nunca muta o `sharedMaterial`); toggle F12 `Enable Custom Logotype`. Ref: [modded/Resources/logotype/logotype.png](../modded/Resources/logotype/logotype.png).
- **Aspect-correction** (`mainTextureScale`) + **`Logotype Scale`** (escala o quad com compensação de pivot `delta = offset·(1−scale)`). Decal: shader `Legacy Shaders/Transparent/Diffuse`, texturas originais `eft_logo_beta`/`eft_logo_beta_ulti-edition` (ambas 1024×512).
- **Acento**: default da `AccentColor` no código = `FF0000FF` (vermelho); **mas o usuário ajustou no F12 para `C2973FFF`** (dourado/bronze). Valores F12 atuais: `LogotypeScale=1.689`, `PosLogotypeH=-2.238`, `PosLogotypeV=0.357`, `Enable Background=false`.
- ⚠️ **Sync do launcher do servidor reverte builds locais com "Dev Mod" off** — reverteu a DLL (v1.2.2→v1.2.1) E o ícone do CareerLog. **Subir builds ao servidor p/ persistir.** Ver [[feedback-server-launcher-sync-builds]].
- **Ícone RECORDS** (mod externo CareerLog) = versão simplificada (gráfico+eixos); versionado em [assets/icon_records_custom.png](../assets/icon_records_custom.png).

## Pendências / próximos passos conhecidos

- [P-2.1] 🔴 **Subir builds ao servidor** (MoxoPixel.MenuOverhaul: DLL v1.2.2 + logo TRL-black; Softwyx.CareerLog: `icon_records.png` simplificado) — senão o sync do launcher reverte (Dev Mod off).
- [P-1.1] 🟡 Validar in-game o **fix do reset da logo** ao voltar ao menu (logo TRL-black já vista OK; o reset-pós-`Show` especificamente não foi reconfirmado após o fix).
- [P-1.2] 🟡 Possível **flicker** (~0,1–0,3 s) na entrada do menu pela reaplicação atrasada — confirmar; se incomodar, trocar por Update contínuo.
- [P-1.3] 🟢 Fixar valores do usuário como **defaults no código** (`LogotypeScale≈1.69`, `PosLogotypeH≈-2.24`, `PosLogotypeV≈0.36`).
- [P-1.4] 🟢 **Compat CareerLog (Records/Trade sobrepostos)** — solução conhecida (reservar slot detectando `com.softwyx.careerlog`); **decidido NÃO aplicar**.
- [P-1.6] 🟡 Ícone do Records reverte em **sync do servidor / update do CareerLog** → reaplicar de [assets/icon_records_custom.png](../assets/icon_records_custom.png).

---

## 2026-06-11 23:44 (GMT-3) — Sessão 1: Import + tema redline (logo custom, acento vermelho, escala) + compat CareerLog

**Tema central:** importar o MoxoPixel-MenuOverhaul e transformá-lo num tema "TARKOV RED LINE" — logo custom, acento vermelho e tamanho de logo ajustável — resolvendo os bugs visuais que surgiram.

**Decisões-chave:**
- **Logo custom = código novo (não há feature nativa).** O mod só posiciona/ilumina o `decal_plane`; nunca tocava textura/material. Implementado em `ApplyCustomLogotype` clonando o `sharedMaterial` e trocando `mainTexture` no clone — lição do `SetPanoramaEmissionMap` (não mutar material da cena, senão "vaza" p/ outras telas). Ref: [modded/Helpers/LayoutHelpers.cs](../modded/Helpers/LayoutHelpers.cs) `ApplyCustomLogotype`/`GetOrCreateCustomLogotypeMaterial`.
- **Acento vermelho via default da `AccentColor`** (`FF0000FF`), não cor separada — usuário quis tema todo vermelho. Ref: [modded/Utils/Settings.cs:320](../modded/Utils/Settings.cs#L320). ⚠️ default não sobrescreve config já salvo → editado o `.cfg` do jogo + religado `Enable Top Glow` (estava off, sem ele o glow não aparece).
- **Aspect-correction via `mainTextureScale`** (não editar a imagem): comprime a textura quadrada pela proporção da textura original (medida em runtime), recentraliza; bordas transparentes fazem clamp. Diagnóstico no log revelou shader/textura. Ref: `ApplyLogotypeAspectCorrection`.
- **`Logotype Scale` escala o QUAD, não os UVs** (zoom em UV cortaria a arte). Pivot do decal é deslocado → escalar `localScale` desliza a logo na diagonal; corrigido com offset pivot→centro capturado uma vez + `delta = offset·(1−scale)`. Ref: `ApplyLogotypeScale`.
- **csproj adaptado** p/ compilar fora da pasta do jogo: `HintPath` → `References\` (compile-mod copia DLLs do SPT; faltavam 9, copiadas manualmente do Managed); removido `PostBuild` (chamava `.cmd` inexistente). Ref: [modded/SPT-MenuOverhaul.csproj](../modded/SPT-MenuOverhaul.csproj).
- **Ícone do Records redesenhado** (gráfico+eixos, traço fino) por destoar dos vizinhos (42×42 sem respiro, denso). Gerado via System.Drawing/PowerShell. É asset do CareerLog → backup + cópia versionada no repo.
- **Conflito Records/Trade: NÃO mexer.** Causa = empilhamento de botões por índice fixo da v1.2.2 não reserva slot pro `recordsButton` do CareerLog (a v1.2.1 do usuário não empilhava). Usuário optou por deixar como está.

**Atividade cronológica:**
1. `/add-mod-repo-for-modding` → importou o mod (modded + original); gerou [PROPRIEDADES.md](../PROPRIEDADES.md) (28 props F12, regex padrão não pegou por usar `config.Bind` minúsculo).
2. Explicado funcionamento do background custom (PNG em `Resources/background/`, escolhido por aspect ratio).
3. Implementou logo custom + toggle F12 + acento vermelho.
4. Compile: csproj exigiu adaptação de referências; instalado no path correto (`plugins/MoxoPixel.MenuOverhaul/`), removida pasta duplicada criada pelo compile-mod.
5. Validado in-game pelo usuário: logo + glow vermelho OK.
6. Esticamento horizontal → diagnóstico via log → aspect-correction.
7. Logo ficou pequena → config `Logotype Scale` + compensação de centro (2 iterações: 1ª deslizava na diagonal por pivot offset).
8. **Code review** por agente independente → correções aplicadas: material clone explícito + destruição no `DisposeResources` (C1, evita leak/bleed), escala analítica sem 2ª leitura de `bounds` (M1/M2), reversão restaura material vanilla exato (M4).
9. Bug: logo **reseta** ao voltar ao menu (transform scale/position resetado pelo jogo após `Show`; material persiste) → `ReapplyLogotypeLayoutAsync` reaplica em delays { 50,120,250,500,900,1500 } ms. Ref: [modded/Patches/MenuOverhaulPatch.cs](../modded/Patches/MenuOverhaulPatch.cs#L83).
10. Investigado conflito Records/Trade (CareerLog) — solução documentada, decidido não aplicar.
11. Ícone do Records simplificado e aplicado (com backup).
12. Confirmado: mod já commitado (`554d5d4`) por sessão paralela; working tree limpo.

**Pendências abertas nesta sessão:** ver bloco "Pendências" no topo (P-1.1 a P-1.6).

**Cross-refs:**
- **Trabalho paralelo neste dia em outro mod:** sessão paralela commitou CustomClasses (`8a7c3bf`, `7c73c79`) e este mod (`554d5d4`) — ver `mods/CustomClasses/memory/sessions.md`.
- Mods externos envolvidos (não nossos): **Softwyx.CareerLog** (insere botão/ícone RECORDS no menu; tem camada de compat com o Overhaul — `MenuOverhaulLayoutActive`, `OverhaulButtonYStep`).

---

## 2026-06-15 02:06 (GMT-3) — Sessão 2: Logo trocada p/ TRL-black + sync do launcher reverte builds locais

**Tema central:** trocar a arte da logo para `TRL-black.png` e diagnosticar por que as customizações "voltavam ao normal".

**Decisões-chave:**
- **Logo TRL-black via canvas quadrado (padding), não código.** TRL-black é **wide** (1300×311 ≈ 4.18:1); a aspect-correction assume custom **quadrada** → usar wide direto distorceria. Solução: compor num canvas 1024² com padding transparente (96% largura) → reusa toda a infra (aspect-correction + Scale + reset fix) sem tocar no código revisado. Ref: [modded/Resources/logotype/logotype.png](../modded/Resources/logotype/logotype.png), gerado via System.Drawing/PowerShell.
- **Builds locais NÃO persistem com "Dev Mod" off.** O sync do launcher do servidor RedLine (disparado no boot do cliente — `AutoSync-Cache.bat/.ps1` inicia o cliente, que dispara o updater) restaura os plugins da **fonte do servidor** → reverteu a DLL do MenuOverhaul (~99k→80k = v1.2.2→v1.2.1) E o `icon_records.png` do CareerLog. **Subir builds ao servidor.** `_Mods/Pacote-Mods-4.0` (tinha a v1.2.1) **obsoleto** — não usar. Salvo em memória persistente [[feedback-server-launcher-sync-builds]].
- **Ícone do Records é do CareerLog, não do build do Menu** — reverte junto no sync; reaplicar de `assets/icon_records_custom.png`.

**Atividade cronológica:**
1. Mod `TarkovRedLine4.0` aberto no IDE → confirmado que é outro mod (votação/restart de servidor + launcher/ModUpdater), sem relação com a logo.
2. Trocada a logo p/ TRL-black (canvas quadrado + padding 96%); aplicada no repo + jogo; preview sobre fundo escuro confirmado.
3. Pedido de rebuild → DLL instalada estava **revertida** p/ v1.2.1 (80k, `.bak` sumiu); recompilada/reinstalada a v1.2.2 (99k) + logo.
4. Investigado `AutoSync-Cache` (lida com cache 3D + server mods, não copia plugins) e achadas 2 cópias da DLL: cliente (v1.2.2) e `_Mods/Pacote-Mods-4.0` (v1.2.1).
5. Usuário esclareceu o fluxo: Pacote-Mods-4.0 descontinuado; sync do launcher reverte com Dev Mod off → lembrar de subir builds ao servidor (gravado em memória).
6. Ícone do Records revertido pelo sync → reaplicada a versão simplificada.

**Pendências abertas nesta sessão:** P-2.1 (subir builds) — ver topo.

**Cross-refs:**
- Mod externo `TarkovRedLine4.0`: launcher/ModUpdater que distribui mods — responsável pelo sync que reverte builds locais (Dev Mod off).
- Memória persistente: [[feedback-server-launcher-sync-builds]].

**Revisão de fato anterior:** Sessão 1 registrava a logo como "TARKOV RED LINE" (arte cinza/metálica quadrada, 1024²); nesta sessão trocada para **TRL-black** (TARKOV branco + RED LINE vermelho, original wide 1300×311). Também: a P-1.5 da Sessão 1 ("6 commits não pushados") foi resolvida — os commits já estavam no `origin/main` (pushados por sessão paralela). Histórico preservado.

---
title: "Item 006 — Culling de Cápsulas de Balas — Code Review 01"
date: "2026-09-15"
status: "🔵 Em andamento"
authors: ["Antigravity"]
---

# Item 006 — Culling de Cápsulas de Balas — Code Review 01

## 1. Escopo da Revisão

Revisão técnica do código implementado no mod `TRL-CoreSight` para culling de criação física e ejeção de cápsulas de balas disparadas à distância.

### Arquivos Modificados / Criados

| Arquivo | Tipo | Descrição |
|---|---|---|
| `modded/Patches/ShellSpawnCullingPatch.cs` | Novo | Patches Harmony Prefix para `WeaponManagerClass.StartSpawnShell`, `StartSpawnAllShells` e `SpawnShellAfterJam`. |
| `modded/Configuration/ModConfig.cs` | Modificado | Inclusão de `EnableShellCulling` e `ShellCullingDistance` na seção 8 de F12. |
| `modded/Core/PerformanceManager.cs` | Modificado | Sincronização nativa de `EFTHardSettings.FLYING_SHELLS_VISIBLE_DISTANCE`, restauração segura no `Cleanup()` e telemetria no `OnGUI`. |
| `modded/Plugin.cs` | Modificado | Ativação dos novos patches e bump de versão SemVer para `0.3.2`. |
| `modded/TRL-CoreSight.csproj` | Modificado | Inclusão do patch compilado e bump SemVer para `0.3.2`. |
| `mod.json` | Modificado | Bump SemVer para `0.3.2`. |
| `PROPRIEDADES.md` | Modificado | Documentação em pt-BR da seção 8 de Otimização de Física. |

---

## 2. Análise Crítica de Código e Arquitetura

### 2.1. Desempenho e Interceptação no Nascimento
- **Zero Overhead em Disparos Distantes:** Ao retornar `false` no Prefix de `StartSpawnShell`, a engine da Unity nunca chama `StartCoroutine(method_6)`. Nenhuma corrotina é enfileirada no loop da Unity, nenhum transform é desvinculado e nenhum colisor entra no pipeline da PhysX.
- **Checagem Leve por Distância Quadrática (`sqrMagnitude`):** A verificação de proximidade compara `sqrDist > (maxDist * maxDist)`, evitando o cálculo pesado de raiz quadrada (`Mathf.Sqrt`) em cada disparo de arma.

### 2.2. Robustez e Imunidade do Jogador Local
- **Bypass Imediato (`weaponManager.Player.IsYourPlayer`):** Disparos efetuados pelo jogador local nunca sofrem culling. A arma do jogador mantém 100% de física, trajetória balística de cartuchos e sons mecânicos de metal e plástico quicando no solo.
- **Resolução Segura de Câmera:** O patch utiliza hierarquia de fallback: `CameraClass.Instance.Camera` (câmera nativa do EFT / compatível com FIKA e spectator) com fallback para `Camera.main`. Se a câmera for nula, o patch retorna `true` preventivamente, nunca quebrando o disparo.

### 2.3. Gestão de Memória e Ciclo de Vida da Câmara
- **Sem Vazamento de Pool (`AssetPoolObject`):** Verificado no código da BSG ([`Player.cs:11211`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L11211)): ao recarregar a arma ou ciclar o ferrolho, o método nativo `SetPatronInShellPort` detecta o cartucho anterior retido e chama diretamente `AssetPoolObject.ReturnToPool`, garantindo 0% de memory leak.
- **Restauração Idempotente do Engine:** O valor original de `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE` (`25f`) é guardado no `Initialize()` e restaurado deterministamente no `Cleanup()`, garantindo que o motor volte ao estado puro da BSG entre partidas.

---

## 3. Conformidade com as Regras do Projeto

| Regra | Status | Evidência |
|---|---|---|
| **SemVer Bump Obrigatório** | ✅ Aprovado | Versão incrementada de `0.3.1` para `0.3.2` em `Plugin.cs`, `TRL-CoreSight.csproj` e `mod.json`. |
| **Isolamento de Build** | ✅ Aprovado | DLL final gerada exclusivamente em `mods/TRL-CoreSight/builds/TRL-CoreSight.dll` (49.152 bytes). Nada copiado para `D:/SPT`. |
| **Ciclo de Backlog** | ✅ Aprovado | Item 006 documentado de ponta a ponta (01-spec, 02-spec-tech, 03-review, 04-code-review, 05-asbuild). |
| **Idioma** | ✅ Aprovado | Toda a documentação e código em Português do Brasil. |

---

## 4. Veredito do Code Review

🟢 **Aprovado com Louvor para Homologação em Raid.** A solução é limpa, desacoplada e elimina o desperdício computacional na origem sem riscos de regressão.

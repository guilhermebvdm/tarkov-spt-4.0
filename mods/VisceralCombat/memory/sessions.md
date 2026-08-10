# Visceral Combat — Memória de Sessões

## Snapshot Delta
- **Versão:** 3.8.1 (SPT 4.0 / FIKA 2.2.6)
- **Estado:** Compilação 100% limpa em C# 12 (0 erros). Efeitos de esguicho e sangramento customizados via C# com dois caminhos de shader (`VD 3D Blood Shader V14` e `Legacy Alpha Blended Premultiply`) tratados individualmente para eliminar brilho branco. Zero SPY/debug remanescente no código.
- **Origem dos Bugs de Gigantismo/Inflamento:** Todos os episódios de "gigantismo" e "corpo inflando" foram causados por código experimental no `modded/` durante a refatoração de agonia/músculos. Corrigidos por nós — **não vieram do mod original.**
- **Pendências:** 🟢 Nenhuma pendência blocker ou alta aberta. Feature 001 (desmembramento de perna em bots vivos) planejada no backlog.

---

## Sessão 2026-08-10 — Estilização de Sangue Escuro, SPY de Shader & Backlog 001

### Estilização de Sangue Escuro Coagulado & Remoção de Brilho Branco
- **Iteração 1:** Criado `ApplyDarkCoagulatedBloodFx(ParticleSystem ps)` em `RagdollHelperClass.cs` com escopo em `ps.transform.root` (erro) — atingia toda a hierarquia do bot.
- **Iteração 2 (SPY):** Instalado `[SPY-BLOOD-FX]` para inspecionar todos os materiais/shaders dos efeitos em runtime durante o jogo.
- **Descoberta do SPY:** Dois shaders diferentes no mesmo prefab de esguicho:
  - **`Particles/VD 3D Blood Shader V14`** → sub-partículas `Arcs`, `Spray`, `Drops`; responde a `_Color`, `_TintColor`, `_Glossiness`.
  - **`Legacy Shaders/Particles/Alpha Blended Premultiply`** → prefab raiz `Blood_Spray_Directional_S` (o filamento/jato transparente branco); brilho causado por **alpha alto + premultiplicação**, não emissão.
  - **Problema de Scope:** O `GetComponentsInChildren` com `transform.root` subia até a raiz do personagem e modificava shaders do corpo, roupas e equipamentos do bot.
- **Iteração 3 (Fix Final):**
  - Escopo corrigido para `ps.gameObject.GetComponentsInChildren<Renderer>()` — apenas o subtree do prefab de partícula.
  - Tratamento bifurcado por shader: `Premultiply` recebe `_Color` com alpha `0.35` (elimina blowout branco de premultiplicação); `VD 3D Blood Shader V14` recebe cor + zeragem de especular.
  - SPY removido do código após coleta de dados.
- **Chamadas ativas:** `BleedPatch.HitEffect`, `BleedPatch.BleedEffect`, `KillPatch.SpawnArterialSprays`, `PlayerDetonationPatch.FuckingCrazyGoreyExplosion`.

### Backlog 001 — Desmembramento de Perna em Bots Vivos
- **Feature planejada:** Tiro de grosso calibre na perna de bot vivo → queda em prone, bloqueio permanente de postura, agonia, rastro de sangue no chão, morte por exsanguição.
- **Nota FIKA registrada:** Em sessões coop, convidados sem o mod veriam o bot rastejando com pernas intactas. Pré-requisito: investigar API FIKA para detecção de mod nos clientes antes de implementar.
- **Artefatos criados:** `mods/VisceralCombat/mod-backlog.md`, `mods/VisceralCombat/backlog/001-alive-leg-dismemberment/001-alive-leg-dismemberment-01-spec.md`, fase 5 adicionada ao `docs/refactor-roadmap.md`.

### Desmembramento Pós-Morte, Calibres & Code Review 02
- Estratégia dupla em `LimbKillPatch.cs` para desmembramento de cadáveres (bots vivos por `BodyPartColliderType`, mortos por matching de nome de osso físico).
- Cadastrados calibres de pistolas/PDWs (`9x19PARA`, `9x18PM`, etc.) com probabilidade `0.0` em `VD_Calibers.json`.
- Removidos 100% dos SPY/debug do Code Review 02; protegido callback `InterruptAgony` com `(UnityEngine.Object)pm != null`.

---

## Sessão 2026-08-07 — Execução do Refactor, Build Clean 3.7.1 e Aplicação do Code Review 01
- Refatoração concluída de `PlayerInitPatch`, `ShellCasingPatch`, `PhysicalItemsPatch`, `KillPatch`.
- Code Review 01 aplicado (CR-01-01 a CR-01-05): callbacks protegidos, cache estático, destruição de `AnimatorOverrideController` anterior, pool de gore conectado.

---

## Sessão 2026-07-28 — Code Review e Roadmap de Refatoração
- Code-review minucioso: 15+ gargalos de FPS, vazamentos de RAM, `async void`, propriedades F12 placebo.
- Entregável: roadmap de refatoração em `docs/refactor-roadmap.md`.

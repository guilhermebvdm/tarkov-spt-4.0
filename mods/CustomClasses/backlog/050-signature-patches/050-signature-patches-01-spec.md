# 050 — Perks + drawbacks de signature · Spec funcional

**Mod:** CustomClasses · **Data:** 2026-06-22 · **Status:** 🔵 Em andamento
**Refs:** [00-kickoff](./050-signature-patches-00-kickoff.md) · **autoridade do "o quê": [class-design.md](../../docs/class-design.md)** (perks/drawbacks por classe, valores, Contrato de gating, patch-points, fatiamento) · deps: **054 (rename Furtivo)**

> Funcional (sem classes/refs EFT — isso é a 02-spec-tech). O **catálogo de efeitos e valores não é duplicado aqui** — vive no [class-design.md](../../docs/class-design.md). Esta spec fixa **comportamento, critérios de aceite, gating, Fika e estado entre raids**.

## Objetivo

Cada uma das 6 classes ganha seus **perks 🔧 e o drawback 🔻** (flat, client-side, configuráveis no F12), aplicados **só ao player daquela classe**. Sem skills custom que escalam.

## Escopo (entrega por fatia — ordem)

1. **050.0 — Infra + 2 provas:** helper de **gating per-classe** + framework de **F12-live** + **Bulwark** (dano recebido) + **Pack Mule** (limite de carga). Prova ponta-a-ponta.
2. **050.1** movimento/inércia · **050.2** recuo/aim-punch · **050.3** combate/saúde · **050.4** som/arma/inventário. *(Detalhe e efeitos por fatia: kickoff + class-design.)*
3. **Fora do 050:** zona stances → **051**; validação final → **052**; aba de UI → **053**.

## Critérios de aceite

**Por efeito (DoD real — não por perk):** cada efeito é **observável in-game na classe certa** e **ausente nas outras** (gating). Exemplos âncora:
- **Bulwark** — com a mesma munição/distância, o player Tanque perde **~15% menos HP** que uma classe sem o perk.
- **Pack Mule** — o limite de peso antes de *overweight* sobe **+30%** (Saqueador e Tanque); **não** muda o peso dos itens; **não** soma com a Strength (é **piso** — efetivo = o maior; teto +30%).
- Demais efeitos: ver tabela por classe no class-design (cada um com seu valor mediano).

**Gating (crítico):**
- O efeito aplica **só** se o player local é da classe dona. Gate pela **chave estável `name`** (`Combat Medic`/`Rifleman`/`Hunter`/`Stealth`/`Scavenger`/`Tank`), resolvida a partir de `Info.GameVersion` (= `displayName[lang]`, muda com idioma) — **não hardcodar string de idioma**.
- **Furtivo:** depende do **054** (runtime vira `Stealth`); sem o 054 os perks do Furtivo não casam a chave.
- Outras classes / scavs / bots **não** recebem o efeito.

**F12 (DoD de configurabilidade):**
- Cada perk/drawback tem `Enabled` (bool) + seus valores (ver árvore no class-design §F12).
- **Mudança no F12 vale durante a raid** (ler `ConfigEntry` no apply-time; nada cacheado no boot). Exceção: o que a 02-spec-tech marcar explicitamente como restart.

## Fika / coop

- Gating é **per-player** (cada player resolve a própria classe pelo seu profile) → num raid coop, o perk de A não vaza para B. **Critério:** validar com 2 classes diferentes em coop (efeito certo em cada, zero cross-bleed).
- Efeitos são **locais** (client) — não exigem sync de estado entre peers.

## Estado entre raids

- Perks/drawbacks são **stateless** (derivam da classe + config; re-aplicam a cada raid). **Nada a persistir** no profile.
- A **Adrenaline** tem estado **só em-raid** (janela/cooldown) — zera ao iniciar/encerrar raid (não persiste).

## Corner cases

- **Troca de classe** (perfil novo) → o gating passa a casar a nova classe na próxima raid (a edition do profile muda).
- **F12 alterado no meio da raid** → próximo disparo do efeito usa o novo valor (apply-time).
- **Perk desabilitado no F12** → efeito some imediatamente (sem reiniciar).
- **Efeito composto com a matriz/stances** (ex.: Pack Mule × Strength = piso; Bulwark × HeavyVests = multiplica; velocidade × stances = compõe) — ver overlaps (class-design §K).
- **Patch-point incerto (🟡)** → a 02-spec-tech **re-confirma o alvo no assembly carregado** antes de implementar (a confiança do recon é estimativa).

## Out of scope

- Zona stences (arm-stamina) → 051. · Skills da matriz 🎯 (server) → já no 047. · UI/aba → 053. · Loadout/hideout → já nos `.jsonc`.

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-22 | Guilherme | Criação. Spec funcional do 050 (gating por `name` estável, F12-live, Fika per-player, stateless + Adrenaline em-raid, aceite por efeito, fatiamento 050.0→050.4). Refs class-design como autoridade. |

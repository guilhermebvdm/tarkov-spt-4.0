#!/usr/bin/env node
/**
 * skill-progression.mjs — modelo de progressão das skills custom (itens 048/049).
 *
 * Matemática confirmada no decompile 0.16.x (references/eft-decompiled/Assembly-CSharp/AbstractSkillClass.cs):
 *   Level = floor(Current/100); Current ∈ [0, 5100]  → 100 de "Current" por nível; máx NATIVO = 51.
 *   Cada evento soma `val` ao Current (OnTrigger → SetCurrent).
 *
 * Fadiga (globals.json SkillsSettings): SkillFreshEffectiveness 1.3 · SkillFreshPoints 1 ·
 *   SkillPointsBeforeFatigue 1 · SkillFatiguePerPoint 0.6 · SkillMinEffectiveness 0.0001 ·
 *   SkillFatigueReset 200s. → ganhar XP rápido satura a effectiveness; espaçar (>200s) recupera.
 *
 * MODELO (previsível): nossas skills custom dão XP via SetCurrent direto (bypass da fadiga),
 *   então a progressão é exata e tunável:
 *     raids p/ nível L = (100·L) / (eventos_por_raid × XP_por_evento)
 *   Deriva-se XP_por_evento a partir de um ALVO de "raids até o máximo" no jogo NORMAL:
 *     XP_por_evento = (100·MAX) / (TARGET × eventos_normal)
 *   (Alternativa honrando a fadiga = anti-grind porém imprevisível — não modelada aqui.)
 *
 * Uso: node skill-progression.mjs [MAX=10] [TARGET_RAIDS_ATE_MAX=40]
 */
'use strict';

const PER_LEVEL = 100;
const MAX = Number(process.argv[2]) || 10;                // nível máximo das custom (proposto: 10)
const TARGET = Number(process.argv[3]) || 40;             // alvo: atingir o máx em ~N raids (jogo normal)

// Estimativas de eventos/raid por playstyle (light=casual, normal=médio, heavy=agressivo/grinder).
const SKILLS = [
  { name: 'Adrenalina',        cls: 'Fuzileiro', ev: 'abate (PMC/Scav)',                  freq: { light: 1,   normal: 4,  heavy: 10 } },
  { name: 'Fôlego de Aço',     cls: 'Caçador',   ev: 'tiro c/ respiração presa / abate à distância', freq: { light: 2, normal: 6, heavy: 15 } },
  { name: 'Mula de Carga',     cls: 'Saq/Tan',   ev: 'extrair carregando peso',           freq: { light: 0.6, normal: 1,  heavy: 1.5 } },
  { name: 'Mãos Rápidas',      cls: 'Saqueador', ev: 'container revistado',               freq: { light: 10,  normal: 30, heavy: 60 } },
  { name: 'Passo Fantasma',    cls: 'Fantasma',  ev: 'trecho furtivo / abate não-detectado', freq: { light: 1, normal: 3, heavy: 6 } },
  { name: 'Couraça',           cls: 'Tanque',    ev: 'hit absorvido (sobrevivido)',       freq: { light: 2,   normal: 8,  heavy: 20 } },
  { name: 'Médico de Combate', cls: 'Médico',    ev: 'cura realizada',                    freq: { light: 1,   normal: 3,  heavy: 6 } },
  { name: 'Execução',          cls: 'Fantasma',  ev: 'abate com melee',                   freq: { light: 0.3, normal: 1,  heavy: 3 } },
];

const currentMax = PER_LEVEL * MAX;
const raidsToLevel = (L, freq, xp) => (freq <= 0 ? Infinity : (PER_LEVEL * L) / (freq * xp));
const fmt = n => (n === Infinity ? '∞' : n < 10 ? n.toFixed(1) : Math.round(n).toString());

console.log(`\nMODELO DE PROGRESSÃO — máx nível = ${MAX} (Current ${currentMax}) · alvo = máx em ~${TARGET} raids (jogo normal)\n`);
console.log(`${'Skill'.padEnd(18)} ${'evento'.padEnd(34)} ${'XP/ev'.padStart(6)} ${'→lvl1'.padStart(6)} ${'→lvl' + Math.ceil(MAX / 2)}`.padEnd(0) + `   raids→máx (light/normal/heavy)`);
console.log('─'.repeat(110));

for (const s of SKILLS) {
  // XP/evento p/ bater o máx em TARGET raids no jogo normal.
  const xp = currentMax / (TARGET * s.freq.normal);
  const mid = Math.ceil(MAX / 2);
  const r1 = raidsToLevel(1, s.freq.normal, xp);
  const rMid = raidsToLevel(mid, s.freq.normal, xp);
  const rMaxL = raidsToLevel(MAX, s.freq.light, xp);
  const rMaxN = raidsToLevel(MAX, s.freq.normal, xp);
  const rMaxH = raidsToLevel(MAX, s.freq.heavy, xp);
  console.log(
    `${s.name.padEnd(18)} ${s.ev.padEnd(34)} ${xp.toFixed(1).padStart(6)} ${fmt(r1).padStart(6)} ${fmt(rMid).padStart(6).padEnd(6)}` +
    `      ${fmt(rMaxL)} / ${fmt(rMaxN)} / ${fmt(rMaxH)}`
  );
}

console.log('\nLeitura:');
console.log(`  • XP/ev = quanto somar ao Current por evento (botão de tuning). 100·MAX=${currentMax} é o total p/ o máx.`);
console.log(`  • →lvl1 / →lvl${Math.ceil(MAX / 2)} = raids p/ o 1º nível e o meio (jogo normal) — sente cedo?`);
console.log(`  • raids→máx light/normal/heavy = sensibilidade ao playstyle (heavy chega bem antes).`);
console.log(`  • Nível inicial (matriz/boost) reduz tudo proporcionalmente. Fadiga do EFT NÃO aplicada (modelo previsível).`);
console.log(`  • Trocar cenário: node skill-progression.mjs <MAX> <TARGET>  (ex.: 20 60)\n`);

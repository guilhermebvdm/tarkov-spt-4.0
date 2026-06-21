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
 *   SkillFatigueReset 200s. → MODELO previsível assume XP via SetCurrent direto (bypass da fadiga).
 *     raids p/ nível L = (100·L) / (eventos_por_raid × XP_por_evento)
 *     XP_por_evento = (100·MAX) / (TARGET × eventos_normal)   (deriva do alvo de raids→máx no jogo normal)
 *
 * FREQUÊNCIA POR RAID (`freq`): a base de cada estimativa está em `basis`.
 *   Anchors pesquisados (jun/2026): raid ≈ 30–40 min; sobrevivência ≈ 20–40% (mesmo bons players).
 *   BSG NÃO publica abates/loot por raid → esses são [reasoned] (raciocinados do tempo de raid + playstyle),
 *   a validar com o próprio jogo. [grounded] = ancorado em dado público.
 *
 * Uso: node skill-progression.mjs [MAX=10] [TARGET_RAIDS_ATE_MAX=40]
 */
'use strict';

const PER_LEVEL = 100;
const MAX = Number(process.argv[2]) || 10;
const TARGET = Number(process.argv[3]) || 40;

// freq = eventos/raid {light=casual, normal=médio, heavy=agressivo/grinder}.
// Eventos ancorados nos SkillActionClass compartilhados do vanilla (SkillManager.cs:1455-1485).
// "tick" = disparo contínuo (movimento/mira) — freq alta, XP/ev minúsculo, igual às skills de movimento do EFT.
const SKILLS = [
  { name: 'Adrenalina',        cls: 'Fuzileiro', ev: 'abate (hook de kill)',                    freq: { light: 1,   normal: 4,   heavy: 10 },  basis: 'reasoned — sem SkillAction de kill nativo; hook próprio' },
  { name: 'Fôlego de Aço',     cls: 'Caçador',   ev: 'mira/respiração (WeaponAim/HoldBreath)',  freq: { light: 5,   normal: 15,  heavy: 40 },  basis: 'reasoned — sniper mira muito/raid' },
  { name: 'Mula de Carga',     cls: 'Saq/Tan',   ev: 'mover/correr OVERWEIGHT (=Strength)',     freq: { light: 300, normal: 700, heavy: 1500 }, basis: 'grounded — Movement/SprintAction overweight (0.005-0.12/tick), contínuo' },
  { name: 'Mãos Rápidas',      cls: 'Saqueador', ev: 'container (SearchAction, =Search)',       freq: { light: 10,  normal: 30,  heavy: 60 },  basis: 'grounded — SearchAction 0.33/container' },
  { name: 'Passo Fantasma',    cls: 'Fantasma',  ev: 'mover SILENCIOSO (=CovertMovement)',      freq: { light: 200, normal: 500, heavy: 1000 }, basis: 'grounded — MovementAction com Noise<1 (0.025/tick), contínuo' },
  { name: 'Couraça',           cls: 'Tanque',    ev: 'dano recebido (=Vitality)',              freq: { light: 3,   normal: 10,  heavy: 25 },  basis: 'reasoned — hit recebido e sobrevivido' },
  { name: 'Médico de Combate', cls: 'Médico',    ev: 'cura realizada (HealAction)',            freq: { light: 1,   normal: 3,   heavy: 6 },   basis: 'reasoned — curas/raid' },
  { name: 'Execução',          cls: 'Fantasma',  ev: 'golpe de melee (FistfightAction)',       freq: { light: 1,   normal: 5,   heavy: 15 },  basis: 'grounded — melee HIT (não kill) → frequente' },
];

const currentMax = PER_LEVEL * MAX;
const raidsTo = (L, f, xp) => (f <= 0 ? Infinity : (PER_LEVEL * L) / (f * xp));
const fmt = n => (n === Infinity ? '∞' : n < 10 ? n.toFixed(1) : Math.round(n).toString());

console.log(`\nPROGRESSÃO — máx=${MAX} (Current ${currentMax}) · alvo: máx em ~${TARGET} raids (jogo normal)`);
console.log(`Anchors: raid ~30-40min · sobrevivência ~20-40%. [grounded]=dado público · [reasoned]=estimado (validar in-game)\n`);

const head = `${'Skill'.padEnd(17)} ${'evento'.padEnd(30)} ${'freq L/N/H'.padStart(13)} ${'XP/ev'.padStart(6)} ${'→nv1'.padStart(5)} ${'→máx L/N/H'.padStart(14)}`;
console.log(head);
console.log('─'.repeat(head.length));
for (const s of SKILLS) {
  const xp = currentMax / (TARGET * s.freq.normal);
  const freqStr = `${s.freq.light}/${s.freq.normal}/${s.freq.heavy}`;
  const maxStr = `${fmt(raidsTo(MAX, s.freq.light, xp))}/${fmt(raidsTo(MAX, s.freq.normal, xp))}/${fmt(raidsTo(MAX, s.freq.heavy, xp))}`;
  console.log(`${s.name.padEnd(17)} ${s.ev.padEnd(30)} ${freqStr.padStart(13)} ${xp.toFixed(1).padStart(6)} ${fmt(raidsTo(1, s.freq.normal, xp)).padStart(5)} ${maxStr.padStart(14)}`);
}
console.log('\nbasis das frequências:');
for (const s of SKILLS) console.log(`  • ${s.name.padEnd(17)} freq=${s.freq.light}/${s.freq.normal}/${s.freq.heavy}  — ${s.basis}`);
console.log(`\n(XP/ev = Current somado por evento. →nv1/→máx em raids. Nível inicial reduz proporcional. Trocar: node skill-progression.mjs <MAX> <TARGET>)\n`);

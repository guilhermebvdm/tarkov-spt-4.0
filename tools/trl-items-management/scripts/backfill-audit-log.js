#!/usr/bin/env node
/**
 * backfill-audit-log.js — one-time seed of the audit log (<mod-dir>/logs/audit.jsonl) with a
 * "baseline-unknown-date" entry for every pre-existing override/ban found on disk — overrides made
 * before the audit-logging feature existed have no real timestamp, so this records an honest lower
 * bound ("existed on/before the moment this script ran") instead of leaving the log silently blank
 * for them.
 *
 * Usage:
 *   node scripts/backfill-audit-log.js --spt-data <path-to-SPT_Data> --mod-dir <path-to-user/mods/TRL-ItemsManagement> [--apply]
 *
 * --spt-data: path to SPT_Data (contains configs/ragfair.json, database/templates/items.json).
 * --mod-dir:  path to user/mods/TRL-ItemsManagement (contains config/, logs/).
 * No default for either — always point explicitly at the target install (production or a local
 * test copy); never silently falls back to a dev path.
 *
 * --apply: actually appends baseline entries. Without it (default): dry run, prints a summary only.
 *
 * Idempotent per (tpl, feature) — NOT per file or per tpl alone: skips any (tpl, feature) pair that
 * already has ANY entry (live or a previous baseline run) in the log. Safe to re-run any number of
 * times, in any order relative to real edits already made through the running mod — a tpl with a
 * live entry for one feature (e.g. flea-price) still gets backfilled for a DIFFERENT feature (e.g.
 * trader-sell-price) if that one has no entry yet.
 */
'use strict';

const fs = require('fs');
const path = require('path');

const args = process.argv.slice(2);
const sptData = args[args.indexOf('--spt-data') + 1];
const modDir = args[args.indexOf('--mod-dir') + 1];
const APPLY = args.includes('--apply');

if (!sptData || !modDir) {
  console.error('Usage: node backfill-audit-log.js --spt-data <path> --mod-dir <path> [--apply]');
  console.error('  --spt-data: path to SPT_Data (contains configs/ragfair.json, database/templates/items.json)');
  console.error('  --mod-dir:  path to user/mods/TRL-ItemsManagement (contains config/, logs/)');
  process.exit(1);
}

function readJsonSafe(p) {
  if (!fs.existsSync(p)) return null;
  try { return JSON.parse(fs.readFileSync(p, 'utf8')); }
  catch (e) { console.error(`WARN: failed to parse ${p}: ${e.message}`); return null; }
}

const auditLogPath = path.join(modDir, 'logs', 'audit.jsonl');

// (tpl, feature) pairs already logged, from ANY origin (a real edit or a previous baseline run) —
// the idempotency guard. A corrupt/truncated last line is tolerated, same as AuditLogController.
const existing = new Set();
if (fs.existsSync(auditLogPath)) {
  for (const line of fs.readFileSync(auditLogPath, 'utf8').split('\n')) {
    if (!line.trim()) continue;
    try {
      const entry = JSON.parse(line);
      if (entry.tpl) existing.add(entry.tpl + '|' + entry.feature);
    } catch (e) { /* tolerate — same reasoning as AuditLogController's per-line parse */ }
  }
}

const now = new Date().toISOString();
const toWrite = []; // { tpl, feature, after, extra }

function collect(tpl, feature, after, extra) {
  const key = tpl + '|' + feature;
  if (existing.has(key)) return;
  existing.add(key); // guards against a duplicate tpl within the SAME source dict too
  toWrite.push({ tpl, feature, after, extra: extra || null });
}

// 1. ragfair.json — flea overrides (additive override, vanilla) + multipliers (mod items).
const ragfair = readJsonSafe(path.join(sptData, 'configs', 'ragfair.json'));
const overrideMap = (ragfair && ragfair.dynamic && ragfair.dynamic.itemPriceOverrideRouble) || {};
const multiplierMap = (ragfair && ragfair.dynamic && ragfair.dynamic.itemPriceMultiplier) || {};
for (const [tpl, value] of Object.entries(overrideMap)) collect(tpl, 'flea-price', { mode: 'override', value });
for (const [tpl, value] of Object.entries(multiplierMap)) collect(tpl, 'flea-price', { mode: 'multiplier', value });

// 2/3. config/overrides.json (trader sell) + config/buy-overrides.json (trader buy) — same shape:
// { traderId: { tpl: { count, currency } } }.
function collectTraderOverrides(fileName, feature) {
  const raw = readJsonSafe(path.join(modDir, 'config', fileName)) || {};
  for (const [traderId, tplMap] of Object.entries(raw)) {
    for (const [tpl, ovr] of Object.entries(tplMap || {})) {
      collect(tpl, feature, { count: ovr.count, currency: ovr.currency }, { traderId });
    }
  }
}
collectTraderOverrides('overrides.json', 'trader-sell-price');
collectTraderOverrides('buy-overrides.json', 'trader-buy-price');

// 4. config/mod-item-bans.json — mod-item bans ({ tpl: true }, absence = not banned).
const modBans = readJsonSafe(path.join(modDir, 'config', 'mod-item-bans.json')) || {};
for (const [tpl, banned] of Object.entries(modBans)) {
  if (banned) collect(tpl, 'ban', { banned: true }, { modItem: true });
}

// 5. SPT_Data/database/templates/items.json — vanilla bans (_props.CanSellOnRagfair === false).
const items = readJsonSafe(path.join(sptData, 'database', 'templates', 'items.json')) || {};
for (const [tpl, item] of Object.entries(items)) {
  if (item && item._props && item._props.CanSellOnRagfair === false) {
    collect(tpl, 'ban', { banned: true }, { modItem: false });
  }
}

// ── Summary (always printed, dry run or not) ────────────────────────────────
const byFeature = {};
for (const { feature } of toWrite) byFeature[feature] = (byFeature[feature] || 0) + 1;

console.error(`Found ${toWrite.length} (tpl, feature) pair(s) without an existing audit-log entry:`);
for (const [feature, count] of Object.entries(byFeature)) console.error(`  ${feature}: ${count}`);
if (toWrite.length > 0) {
  console.error('Sample:');
  for (const sample of toWrite.slice(0, 5)) {
    console.error(`  ${sample.tpl} / ${sample.feature}: ${JSON.stringify(sample.after)}`);
  }
}

if (!APPLY) {
  console.error('\nDry run — nothing written. Re-run with --apply to write these entries.');
  process.exit(0);
}

if (toWrite.length === 0) {
  console.error('\nNothing to write.');
  process.exit(0);
}

fs.mkdirSync(path.join(modDir, 'logs'), { recursive: true });
const lines = toWrite.map(({ tpl, feature, after, extra }) => JSON.stringify({
  ts: now,
  feature,
  action: 'set',
  tpl,
  before: null,
  after,
  extra,
  origin: 'baseline-unknown-date',
}));
fs.appendFileSync(auditLogPath, lines.join('\n') + '\n', 'utf8');
console.error(`\nWrote ${toWrite.length} baseline entries to ${auditLogPath}`);

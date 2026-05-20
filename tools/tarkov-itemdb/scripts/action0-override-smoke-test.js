#!/usr/bin/env node
/**
 * Ação 0 — Smoke test prévio do plano flea-override.
 * Plan ref: C:\Users\guime\.claude\plans\curried-dreaming-marble.md
 *
 * Modos:
 *   prep    — backup ragfair.json + injeta 2 overrides de teste + refresh checks.dat
 *   revert  — restaura ragfair.json do backup + refresh checks.dat
 *   status  — mostra estado atual (override values + se backup existe)
 *
 * O que faz no modo prep:
 *   - Salva ragfair.json → ragfair.json.pre-action0-backup (se já existir, NÃO sobrescreve)
 *   - Adiciona ao itemPriceOverrideRouble:
 *       Bolts (craft):    57347c5b245977448d35f6e1 → 123456
 *       GPU (non-craft):  57347ca924597744596b4e71 → 654321
 *   - Recalcula hash MD5 e atualiza checks.dat
 *   - Imprime instruções de smoke test
 *
 * Pré-requisitos:
 *   - prices.json e handbook.json em estado vanilla (sem edits residuais de testes anteriores)
 *   - LiveFleaPrices DESATIVADO
 *
 * Uso:
 *   node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js prep
 *   node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js status
 *   node tools/tarkov-itemdb/scripts/action0-override-smoke-test.js revert
 */
'use strict';

const fs     = require('fs');
const path   = require('path');
const crypto = require('crypto');

const SPT_PATH      = process.env.SPT_PATH || 'D:/SPT/SPT';
const SPT_DATA      = path.join(SPT_PATH, 'SPT_Data');
const RAGFAIR_JSON  = path.join(SPT_DATA, 'configs', 'ragfair.json');
const RAGFAIR_BAK   = path.join(SPT_DATA, 'configs', 'ragfair.json.pre-action0-backup');
const CHECKS_DAT    = path.join(SPT_DATA, 'checks.dat');

const TEST_OVERRIDES = {
  '57347c5b245977448d35f6e1': { name: 'Bolts (craft)',     value: 123456 },
  '57347ca924597744596b4e71': { name: 'GPU (non-craft)',  value: 654321 },
};

function readJson(p) {
  return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, ''));
}

// Detect indent (tab vs N spaces) used by an existing JSON file so we can
// rewrite it preserving the original style — important because checks.dat hash
// covers the exact bytes, but more importantly so diffs against vanilla stay
// minimal. SPT vanilla configs use TAB indent.
function detectIndent(rawText) {
  const m = rawText.match(/^([\t ]+)"/m);
  if (!m) return '\t';
  return m[1][0] === '\t' ? '\t' : m[1].length;
}

function writeJsonPreservingStyle(p, obj) {
  const original = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  const indent = detectIndent(original);
  const trailingNewline = original.endsWith('\n');
  let serialized = JSON.stringify(obj, null, indent);
  if (trailingNewline) serialized += '\n';
  fs.writeFileSync(p, serialized, 'utf8');
}

function md5OfFile(p) {
  return crypto.createHash('md5').update(fs.readFileSync(p)).digest('hex').toUpperCase();
}

function refreshChecksDat(relPath) {
  if (!fs.existsSync(CHECKS_DAT)) throw new Error('checks.dat not found at ' + CHECKS_DAT);
  const raw = fs.readFileSync(CHECKS_DAT, 'utf8');
  const manifest = JSON.parse(Buffer.from(raw, 'base64').toString('utf8'));
  const entry = manifest.find(x => x.Path === relPath);
  if (!entry) throw new Error(`checks.dat has no entry for ${relPath}`);
  const absPath = path.join(SPT_DATA, relPath);
  const newHash = md5OfFile(absPath);
  const oldHash = entry.Hash;
  entry.Hash = newHash;
  const reencoded = Buffer.from(JSON.stringify(manifest, null, 2), 'utf8').toString('base64') + '\n';
  fs.writeFileSync(CHECKS_DAT, reencoded, 'utf8');
  return { oldHash, newHash, changed: oldHash !== newHash };
}

function cmdPrep() {
  if (!fs.existsSync(RAGFAIR_JSON)) throw new Error('ragfair.json not found at ' + RAGFAIR_JSON);

  // Backup (only if not already present — don't overwrite a real backup)
  if (fs.existsSync(RAGFAIR_BAK)) {
    console.log('→ Backup já existe em', RAGFAIR_BAK);
    console.log('  (não sobrescrito; rode `revert` antes de re-executar `prep` se quiser começar do zero)');
  } else {
    fs.copyFileSync(RAGFAIR_JSON, RAGFAIR_BAK);
    console.log('→ Backup criado:', RAGFAIR_BAK);
  }

  // Inject overrides
  const ragfair = readJson(RAGFAIR_JSON);
  if (!ragfair.dynamic) throw new Error('ragfair.json missing .dynamic');
  ragfair.dynamic.itemPriceOverrideRouble = ragfair.dynamic.itemPriceOverrideRouble || {};
  for (const [tpl, info] of Object.entries(TEST_OVERRIDES)) {
    ragfair.dynamic.itemPriceOverrideRouble[tpl] = info.value;
  }
  // Preserve original indent style (SPT vanilla uses TAB) and trailing-newline state.
  writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);
  console.log('→ Overrides injetados em ragfair.json:');
  for (const [tpl, info] of Object.entries(TEST_OVERRIDES)) {
    console.log(`    ${tpl}  ${info.name.padEnd(20)}  ${info.value}`);
  }

  // Refresh checks.dat
  const r = refreshChecksDat('configs/ragfair.json');
  console.log(`→ checks.dat refreshed: ${r.oldHash} → ${r.newHash} (changed=${r.changed})`);

  // Instruções
  console.log('\n========================================');
  console.log('PRÓXIMOS PASSOS (manuais):');
  console.log('========================================');
  console.log('1. LiveFleaPrices DESATIVADO (mod renomeado .disabled).');
  console.log('2. Restart full do SPT server.');
  console.log('3. Abrir flea no jogo, filtrar por cada item:');
  console.log('   - Bolts:  ofertas esperadas em 123456 × 0.8..1.2 ≈ 98K..148K');
  console.log('             (se vier ~11.5M, override foi ignorado pela fórmula vanilla)');
  console.log('   - GPU:    ofertas esperadas em 654321 × 0.8..1.2 ≈ 523K..785K');
  console.log('             (se vier ~7.5M, override foi ignorado)');
  console.log('4. Anote observações em tools/tarkov-itemdb/docs/flea-override-validation.md');
  console.log('5. Quando terminar: `node action0-override-smoke-test.js revert`');
}

function cmdRevert() {
  if (!fs.existsSync(RAGFAIR_BAK)) {
    console.error('✗ Backup não encontrado:', RAGFAIR_BAK);
    console.error('  Nada para reverter (ou rode `prep` primeiro).');
    process.exit(1);
  }
  fs.copyFileSync(RAGFAIR_BAK, RAGFAIR_JSON);
  console.log('→ ragfair.json restaurado do backup.');
  const r = refreshChecksDat('configs/ragfair.json');
  console.log(`→ checks.dat refreshed: ${r.oldHash} → ${r.newHash} (changed=${r.changed})`);
  fs.unlinkSync(RAGFAIR_BAK);
  console.log('→ Backup removido. Estado pré-Ação 0 restaurado.');
}

function cmdStatus() {
  if (!fs.existsSync(RAGFAIR_JSON)) {
    console.error('✗ ragfair.json não encontrado em', RAGFAIR_JSON);
    process.exit(1);
  }
  const ragfair = readJson(RAGFAIR_JSON);
  const overrides = ragfair.dynamic?.itemPriceOverrideRouble || {};

  console.log('Backup existe?', fs.existsSync(RAGFAIR_BAK) ? `SIM (${RAGFAIR_BAK})` : 'NÃO');
  console.log('\nOverrides atuais em ragfair.json:');
  if (Object.keys(overrides).length === 0) {
    console.log('  (nenhum)');
  } else {
    for (const [tpl, value] of Object.entries(overrides)) {
      const test = TEST_OVERRIDES[tpl];
      const tag  = test ? ` ← teste Ação 0 (${test.name})` : '';
      console.log(`  ${tpl}  ${String(value).padStart(10)}${tag}`);
    }
  }
  console.log('\nTest overrides ainda ativos?',
    Object.entries(TEST_OVERRIDES).every(([tpl, info]) => overrides[tpl] === info.value)
      ? 'SIM (Ação 0 em andamento)' : 'NÃO (revertido ou nunca preparado)');
}

const cmd = process.argv[2];
switch (cmd) {
  case 'prep':   cmdPrep();   break;
  case 'revert': cmdRevert(); break;
  case 'status': cmdStatus(); break;
  default:
    console.log('Uso: node action0-override-smoke-test.js <prep|revert|status>');
    process.exit(1);
}

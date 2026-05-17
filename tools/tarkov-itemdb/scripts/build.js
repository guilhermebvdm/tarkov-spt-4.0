#!/usr/bin/env node
/**
 * build.js — orchestrator. Runs fetch + load + normalize in sequence.
 *
 * Skip behavior:
 *   - fetch-tarkov-dev    skips if cache <6h, unless --force
 *   - fetch-tarkov-market skips if cache <6h, unless --force
 *   - load-spt            always runs (local IO, cheap)
 *   - normalize           always runs (depends on three caches)
 *
 * Usage:  node scripts/build.js [--force]
 */
'use strict';

const { spawnSync } = require('child_process');
const path = require('path');

const FORCE = process.argv.includes('--force');
const SCRIPTS = ['fetch-tarkov-dev.js', 'fetch-tarkov-market.js', 'load-spt.js', 'normalize.js'];

function run(script) {
  const args = [path.join(__dirname, script)];
  if (FORCE && script.startsWith('fetch-')) args.push('--force');
  console.error(`\n▶ ${script}${FORCE && script.startsWith('fetch-') ? ' --force' : ''}`);
  const res = spawnSync(process.execPath, args, { stdio: 'inherit' });
  if (res.status !== 0) {
    console.error(`\n✗ ${script} failed (exit ${res.status})`);
    process.exit(res.status || 1);
  }
}

for (const s of SCRIPTS) run(s);
console.error('\n✓ build complete');

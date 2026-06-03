#!/usr/bin/env node
/**
 * deploy.js
 *
 * Copia os arquivos .json de modded/profiles/ para o install do SPT.
 *
 * Localização do mod no SPT (auto-detecta a primeira que existir):
 *   - <SPT>/user/mods/RZCustomProfiles/profiles/      (server mod SPT 4.0)
 *   - <SPT>/BepInEx/plugins/RZCustomProfiles/profiles/ (BepInEx plugin)
 *
 * Resolução do path base do SPT (em ordem):
 *   1. --target <abs-path>          → caminho absoluto direto para a pasta profiles/
 *   2. --spt-path <abs-path>        → raiz do SPT (substitui SPT_PATH)
 *   3. env var SPT_PATH             → mesma convenção do tools/tarkov-itemdb
 *   4. default 'D:/SPT/SPT'
 *
 * Uso:
 *   node mods/RZCustomProfiles/scripts/deploy.js
 *   node mods/RZCustomProfiles/scripts/deploy.js --dry-run
 *   node mods/RZCustomProfiles/scripts/deploy.js --target "D:/SPT/.../profiles"
 *   node mods/RZCustomProfiles/scripts/deploy.js --spt-path "E:/Tarkov"
 *
 * Comportamento:
 *   - Idempotente: sobrescreve arquivos existentes com mesmo nome.
 *   - NÃO toca em outros arquivos (ex: exampleProfile.json fica como está).
 *   - --dry-run: só lista o que seria feito, sem escrever.
 *   - Aborta se nenhum candidato de destino existir.
 */

'use strict';
const fs   = require('fs');
const path = require('path');

const MOD_ROOT = path.resolve(__dirname, '..');
const SOURCE_DIR = path.join(MOD_ROOT, 'modded/profiles');

// ── Parse CLI ───────────────────────────────────────────────────────────────

const args = process.argv.slice(2);
function flag(name) {
  return args.includes(name);
}
function arg(name) {
  const i = args.indexOf(name);
  return i >= 0 && i + 1 < args.length ? args[i + 1] : null;
}

const dryRun     = flag('--dry-run');
const targetOpt  = arg('--target');
const sptPathOpt = arg('--spt-path');
const sptPath    = path.resolve(sptPathOpt || process.env.SPT_PATH || 'D:/SPT/SPT');

// ── Resolver destino ─────────────────────────────────────────────────────────

function resolveTargetDir() {
  if (targetOpt) {
    const p = path.resolve(targetOpt);
    if (!fs.existsSync(p)) {
      console.error(`ERRO: --target "${p}" não existe.`);
      process.exit(1);
    }
    return p;
  }

  const candidates = [
    path.join(sptPath, 'user/mods/RZCustomProfiles/profiles'),
    path.join(sptPath, 'BepInEx/plugins/RZCustomProfiles/profiles'),
  ];

  for (const c of candidates) {
    if (fs.existsSync(c)) return c;
  }

  console.error('ERRO: nenhum candidato de destino encontrado. Tentei:');
  for (const c of candidates) console.error('  -', c);
  console.error('');
  console.error('Soluções:');
  console.error('  - Verifique se SPT_PATH está correto (atual:', sptPath + ')');
  console.error('  - Ou rode com --target "<path absoluto para profiles/>"');
  console.error('  - Ou rode com --spt-path "<raiz do SPT>" se for diferente do default');
  process.exit(1);
}

// ── Listar arquivos a copiar ────────────────────────────────────────────────

if (!fs.existsSync(SOURCE_DIR)) {
  console.error(`ERRO: ${SOURCE_DIR} não existe. Rode build-profile-jsons.js antes.`);
  process.exit(1);
}

const jsonFiles = fs.readdirSync(SOURCE_DIR)
  .filter(f => f.endsWith('.json') && f !== 'exampleProfile.json')
  .sort();

if (jsonFiles.length === 0) {
  console.error(`ERRO: nenhum .json (exceto exampleProfile) em ${SOURCE_DIR}.`);
  process.exit(1);
}

// ── Executar ────────────────────────────────────────────────────────────────

const targetDir = resolveTargetDir();

console.log(`Origem:  ${SOURCE_DIR}`);
console.log(`Destino: ${targetDir}`);
console.log(`Modo:    ${dryRun ? 'DRY-RUN (nada será escrito)' : 'EXECUÇÃO'}`);
console.log('');

let copied = 0;
let overwritten = 0;

for (const f of jsonFiles) {
  const src = path.join(SOURCE_DIR, f);
  const dst = path.join(targetDir, f);
  const exists = fs.existsSync(dst);

  if (dryRun) {
    console.log(`  ${exists ? '↻' : '+'} ${f}${exists ? ' (sobrescreveria)' : ' (novo)'}`);
  } else {
    fs.copyFileSync(src, dst);
    console.log(`  ${exists ? '↻' : '+'} ${f}${exists ? ' (sobrescrito)' : ''}`);
    if (exists) overwritten++;
    copied++;
  }
}

console.log('');
if (dryRun) {
  console.log(`Dry-run OK — ${jsonFiles.length} arquivo(s) seriam copiado(s).`);
} else {
  console.log(`✓ ${copied} arquivo(s) copiado(s) (${overwritten} sobrescrito(s)).`);
  console.log('');
  console.log('Próximo: reinicie o servidor SPT para os perfis aparecerem no launcher.');
}

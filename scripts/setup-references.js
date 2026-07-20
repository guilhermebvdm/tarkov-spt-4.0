#!/usr/bin/env node
/**
 * setup-references.js
 * Clona as referencias read-only vendorizadas faltantes, lendo o inventario
 * canonico em references/manifest.json.
 *
 * Uso:
 *   node scripts/setup-references.js              # clona o que faltar (fetch:true)
 *   node scripts/setup-references.js --check      # dry-run: relata presente/faltando; exit 1 se faltar algo
 *   node scripts/setup-references.js --check-manifest  # so valida o manifesto (usado no pre-commit); exit 1 se invalido
 *   node scripts/setup-references.js --force      # re-clona mesmo se ja existir (remove a pasta antes)
 *   node scripts/setup-references.js --only <id>  # restringe a uma fonte (repetivel)
 */

'use strict';

const fs   = require('fs');
const path = require('path');
const { execFileSync } = require('child_process');

const ROOT     = path.resolve(__dirname, '..');
const MANIFEST = path.join(ROOT, 'references/manifest.json');

// ── args ────────────────────────────────────────────────────────────────────
const argv          = process.argv.slice(2);
const CHECK         = argv.includes('--check');
const CHECK_MANIFEST = argv.includes('--check-manifest');
const FORCE         = argv.includes('--force');
const ONLY = argv.reduce((acc, a, i) => {
  if (a === '--only' && argv[i + 1]) acc.push(argv[i + 1]);
  return acc;
}, []);

// ── helpers ───────────────────────────────────────────────────────────────────
function loadManifest() {
  let raw;
  try {
    raw = fs.readFileSync(MANIFEST, 'utf8');
  } catch (e) {
    fail(`Nao foi possivel ler ${rel(MANIFEST)}: ${e.message}`);
  }
  try {
    return JSON.parse(raw);
  } catch (e) {
    fail(`references/manifest.json nao e JSON valido: ${e.message}`);
  }
}

function validateManifest(m) {
  const errors = [];
  if (!m || typeof m !== 'object') errors.push('raiz deve ser um objeto');
  if (!Array.isArray(m.sources)) {
    errors.push('campo "sources" ausente ou nao e array');
    return errors;
  }
  const ids   = new Set();
  const paths = new Set();
  m.sources.forEach((s, i) => {
    const tag = `sources[${i}]${s && s.id ? ` (${s.id})` : ''}`;
    if (!s.id   || typeof s.id   !== 'string') errors.push(`${tag}: "id" ausente/invalido`);
    if (!s.path || typeof s.path !== 'string') errors.push(`${tag}: "path" ausente/invalido`);
    if (!s.role || typeof s.role !== 'string') errors.push(`${tag}: "role" ausente/invalido`);
    if (typeof s.tier  !== 'number')           errors.push(`${tag}: "tier" deve ser numero`);
    if (typeof s.fetch !== 'boolean')          errors.push(`${tag}: "fetch" deve ser boolean`);
    if (s.id) { if (ids.has(s.id)) errors.push(`${tag}: "id" duplicado`); ids.add(s.id); }
    if (s.path) {
      if (paths.has(s.path)) errors.push(`${tag}: "path" duplicado`);
      paths.add(s.path);
      if (!s.path.startsWith('references/')) errors.push(`${tag}: "path" deve comecar com references/`);
    }
    if (s.fetch === true) {
      if (!s.upstream || typeof s.upstream !== 'string') errors.push(`${tag}: fetch:true exige "upstream"`);
      if (!s.pin || typeof s.pin !== 'object')           errors.push(`${tag}: fetch:true exige "pin"`);
      else {
        if (!['commit', 'branch'].includes(s.pin.type)) errors.push(`${tag}: pin.type deve ser "commit" ou "branch"`);
        if (!s.pin.ref || typeof s.pin.ref !== 'string') errors.push(`${tag}: pin.ref ausente/invalido`);
      }
    }
  });
  return errors;
}

function isPresent(absPath) {
  try {
    return fs.statSync(absPath).isDirectory() && fs.readdirSync(absPath).length > 0;
  } catch {
    return false;
  }
}

function git(args, cwd) {
  execFileSync('git', args, { cwd: cwd || ROOT, stdio: 'inherit' });
}

function hasGitLfs() {
  try { execFileSync('git', ['lfs', 'version'], { stdio: 'ignore' }); return true; }
  catch { return false; }
}

function fetchSource(s) {
  const dest = path.join(ROOT, s.path);

  if (isPresent(dest)) {
    if (!FORCE) { log(`  • ${s.id}: ja presente — pulando`); return 'present'; }
    log(`  • ${s.id}: --force → removendo ${s.path}`);
    fs.rmSync(dest, { recursive: true, force: true });
  }

  if (s.lfs && !hasGitLfs()) {
    throw new Error('git-lfs nao instalado — necessario para ' + s.id + ' (instale: https://git-lfs.com)');
  }

  log(`  ↓ ${s.id}: clonando ${s.upstream} (${s.pin.type} ${s.pin.ref})`);
  if (s.pin.type === 'branch') {
    git(['clone', '--depth', '1', '--branch', s.pin.ref, s.upstream, dest]);
  } else {
    // commit: clone parcial + checkout exato. Reproduz o snapshot por outro caminho
    // que o original (full clone + rm .git), mas o conteudo e equivalente.
    // Requisito: o commit precisa ser alcancavel de um branch buscado pelo clone.
    git(['clone', '--filter=blob:none', '--no-checkout', s.upstream, dest]);
    git(['checkout', s.pin.ref], dest);
  }

  if (s.lfs)      { log(`    git lfs pull (${s.id})`); git(['lfs', 'pull'], dest); }
  if (s.stripGit) { log(`    removendo .git (snapshot ${s.id})`); fs.rmSync(path.join(dest, '.git'), { recursive: true, force: true }); }

  return 'cloned';
}

// ── output ────────────────────────────────────────────────────────────────────
function rel(p)  { return path.relative(ROOT, p).replace(/\\/g, '/'); }
function log(m)  { process.stdout.write(m + '\n'); }
function fail(m) { process.stderr.write(`❌ ${m}\n`); process.exit(1); }

// ── main ────────────────────────────────────────────────────────────────────
const manifest = loadManifest();
const errors   = validateManifest(manifest);

if (errors.length) {
  process.stderr.write('❌ references/manifest.json invalido:\n');
  errors.forEach(e => process.stderr.write(`   - ${e}\n`));
  process.exit(1);
}

if (CHECK_MANIFEST) {
  log(`✓ references/manifest.json valido (${manifest.sources.length} fontes)`);
  process.exit(0);
}

let sources = manifest.sources;
if (ONLY.length) {
  sources = sources.filter(s => ONLY.includes(s.id));
  const found = new Set(sources.map(s => s.id));
  ONLY.filter(id => !found.has(id)).forEach(id => process.stderr.write(`⚠ --only ${id}: id inexistente no manifesto\n`));
}

const fetchable = sources.filter(s => s.fetch === true);
// Três categorias, não duas: além de "vem no git" (tracked) e "clonavel" (fetch), agora existe
// "local" — artefato grande demais para o git que NÃO é clonavel de um upstream; é gerado por um
// comando local (campo `generate`) ou, no futuro, baixado de um host privado (`download`).
const local     = sources.filter(s => s.fetch === false && s.tracked === false);
const tracked   = sources.filter(s => s.fetch === false && s.tracked !== false);

// ── modo --check (dry-run) ────────────────────────────────────────────────────
if (CHECK) {
  let missing = 0;
  log('Referencias rastreadas (vem com o repo):');
  tracked.forEach(s => {
    const ok = isPresent(path.join(ROOT, s.path));
    if (!ok) missing++;
    log(`  ${ok ? '✓' : '✗ FALTANDO'} ${s.id} (${s.path})`);
  });
  log('\nReferencias vendorizadas (fetch):');
  fetchable.forEach(s => {
    const ok = isPresent(path.join(ROOT, s.path));
    if (!ok) missing++;
    log(`  ${ok ? '✓ presente' : '✗ faltando'}  ${s.id} (${s.path})`);
  });

  // Artefatos locais: nao sao clonaveis nem vem no git. Quando faltam, o impacto NAO e obvio
  // (um grep no dump ausente volta vazio e parece "o tipo nao existe"), por isso o relatorio
  // imprime o COMO OBTER e o QUE QUEBRA SEM, em vez de so marcar como faltando.
  let localMissing = 0;
  if (local.length) {
    log('\nArtefatos locais (nao vem no git, gerados/baixados na maquina):');
    local.forEach(s => {
      const ok = isPresent(path.join(ROOT, s.path));
      if (!ok) localMissing++;
      log(`  ${ok ? '✓ presente' : '✗ FALTANDO'}  ${s.id} (${s.path})`);
      if (!ok) {
        if (s.generate)      log(`      gerar:     ${s.generate}`);
        if (s.requires)      log(`      requer:    ${s.requires}`);
        if (s.download?.url) log(`      baixar:    ${s.download.url}`);
        else if (s.download === null && !s.generate) log('      baixar:    (host ainda nao definido)');
        if (s.withoutIt)     log(`      ⚠ sem ele: ${s.withoutIt}`);
      }
    });
  }

  if (missing || localMissing) {
    if (missing)      log(`\n${missing} referencia(s) clonavel(is) faltando — rode: node scripts/setup-references.js`);
    if (localMissing) log(`${localMissing} artefato(s) local(is) faltando — veja o comando de cada um acima.`);
    process.exit(1);
  }
  log('\nTudo presente ✓');
  process.exit(0);
}

// ── modo padrao: clonar faltantes ─────────────────────────────────────────────
log(`setup-references — ${fetchable.length} fonte(s) vendorizada(s)${FORCE ? ' [--force]' : ''}`);
const summary = { present: 0, cloned: 0, failed: 0 };
for (const s of fetchable) {
  try {
    const r = fetchSource(s);
    summary[r === 'present' ? 'present' : 'cloned']++;
  } catch (e) {
    summary.failed++;
    process.stderr.write(`❌ ${s.id}: falha ao clonar — ${e.message}\n`);
  }
}
log(`\nResumo: ${summary.cloned} clonada(s), ${summary.present} ja presente(s), ${summary.failed} falha(s).`);
process.exit(summary.failed ? 1 : 0);

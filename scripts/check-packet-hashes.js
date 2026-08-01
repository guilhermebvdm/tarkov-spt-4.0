#!/usr/bin/env node
/**
 * check-packet-hashes.js — detecta colisão de hash entre pacotes FIKA.
 *
 * O NetPacketProcessor do FIKA identifica cada INetSerializable por um CRC-16-CCITT de
 * `typeof(T).ToString()` (namespace + nome do tipo) — apenas 16 bits. O registro é um
 * `Dictionary<ushort, ...>`: dois tipos que colidam fazem o segundo `RegisterPacket`
 * SOBRESCREVER silenciosamente o handler do primeiro. O sintoma é um mod (ou o próprio FIKA)
 * parar de receber uma classe de pacote sem erro nenhum no log.
 *
 * Fonte da hash: references/fika-plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs
 * (ShortHashCache<T>, poly 0x1021, init 0xFFFF).
 *
 * Uso:
 *   node scripts/check-packet-hashes.js          # varre mods/ + Fika.Core, falha se houver colisão
 *   node scripts/check-packet-hashes.js --list   # imprime todas as hashes
 *
 * Exit code 1 em colisão — serve como gate de CI/pre-commit.
 */

const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');

function crc16(str) {
  let crc = 0xffff;
  for (const ch of str) {
    crc ^= (ch.charCodeAt(0) << 8) & 0xffff;
    for (let i = 0; i < 8; i++) {
      crc = crc & 0x8000 ? ((crc << 1) ^ 0x1021) & 0xffff : (crc << 1) & 0xffff;
    }
  }
  return crc;
}

// Pastas que não representam código vivo instalado no jogo.
const SKIP_DIRS = new Set([
  'bin', 'obj', 'node_modules', '.git', '.archived', 'References', 'builds', 'dist',
  'original', 'modded-bak', 'graphify-out',
]);

function walk(dir, out) {
  let entries;
  try {
    entries = fs.readdirSync(dir, { withFileTypes: true });
  } catch {
    return out;
  }
  for (const e of entries) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) {
      if (SKIP_DIRS.has(e.name) || e.name.endsWith('-bak')) continue;
      walk(p, out);
    } else if (e.name.endsWith('.cs')) {
      out.push(p);
    }
  }
  return out;
}

/** Extrai os tipos que implementam INetSerializable, com namespace. */
function collectTypes(files, source) {
  const found = [];
  const typeRe =
    /^[ \t]*(?:public|internal)[ \t]+(?:sealed[ \t]+|readonly[ \t]+|partial[ \t]+)*(?:struct|class)[ \t]+(\w+)[ \t]*:[ \t]*[^{\r\n]*\bINetSerializable\b/gm;

  for (const f of files) {
    let text;
    try {
      text = fs.readFileSync(f, 'utf8');
    } catch {
      continue;
    }
    if (!text.includes('INetSerializable')) continue;

    const ns = (text.match(/^\s*namespace\s+([\w.]+)/m) || [])[1];
    if (!ns) continue;

    let m;
    while ((m = typeRe.exec(text)) !== null) {
      found.push({
        fqn: `${ns}.${m[1]}`,
        file: path.relative(ROOT, f).replace(/\\/g, '/'),
        source,
      });
    }
  }
  return found;
}

const modFiles = walk(path.join(ROOT, 'mods'), []);
const fikaDir = path.join(ROOT, 'references', 'fika-plugin');
const fikaFiles = fs.existsSync(fikaDir) ? walk(fikaDir, []) : [];

const types = [
  ...collectTypes(modFiles, 'mod'),
  ...collectTypes(fikaFiles, 'fika'),
];

if (!fikaFiles.length) {
  console.warn(
    '! references/fika-plugin/ ausente — só os tipos dos mods foram verificados.\n' +
      '  Rode `node scripts/setup-references.js` para cobrir também os pacotes nativos do FIKA.'
  );
}

// Um mesmo FQN pode aparecer em mais de um arquivo (cópias); só o FQN distinto importa.
const byHash = new Map();
for (const t of types) {
  const h = crc16(t.fqn);
  if (!byHash.has(h)) byHash.set(h, new Map());
  byHash.get(h).set(t.fqn, t);
}

const collisions = [...byHash.entries()].filter(([, m]) => m.size > 1);

if (process.argv.includes('--list')) {
  const rows = [...byHash.entries()]
    .flatMap(([h, m]) => [...m.values()].map((t) => ({ h, ...t })))
    .sort((a, b) => a.fqn.localeCompare(b.fqn));
  for (const r of rows) {
    console.log(`${String(r.h).padStart(5)}  [${r.source}] ${r.fqn}`);
  }
  console.log('');
}

// Mesmo FQN declarado por MODS DIFERENTES: a hash é idêntica por construção, então instalar os
// dois faz o segundo RegisterPacket sobrescrever o handler do primeiro. Não aparece como
// "colisão" acima porque o FQN é o mesmo — precisa de checagem própria.
const modOf = (file) => (file.match(/^mods\/([^/]+)\//) || [])[1] || file;
const byFqn = new Map();
for (const t of types.filter((x) => x.source === 'mod')) {
  if (!byFqn.has(t.fqn)) byFqn.set(t.fqn, new Map());
  byFqn.get(t.fqn).set(modOf(t.file), t.file);
}
const duplicated = [...byFqn.entries()].filter(([, mods]) => mods.size > 1);

const distinct = [...byHash.values()].reduce((n, m) => n + m.size, 0);
console.log(
  `Tipos INetSerializable distintos: ${distinct} ` +
    `(mods: ${new Set(types.filter((t) => t.source === 'mod').map((t) => t.fqn)).size}, ` +
    `FIKA: ${new Set(types.filter((t) => t.source === 'fika').map((t) => t.fqn)).size})`
);

if (duplicated.length) {
  console.warn(
    `\n! ${duplicated.length} tipo(s) declarado(s) por MAIS DE UM mod — se dois deles estiverem\n` +
      '  instalados ao mesmo tempo, o segundo registro sobrescreve o handler do primeiro:\n'
  );
  for (const [fqn, mods] of duplicated) {
    console.warn(`  ${fqn}`);
    for (const [mod, file] of mods) console.warn(`    - ${mod}  (${file})`);
  }
  console.warn('  (aviso, não falha: só é problema se os mods coexistirem no jogo)\n');
}

if (!collisions.length) {
  console.log('✓ Nenhuma colisão de hash CRC-16.');
  process.exit(0);
}

console.error(`\n✗ ${collisions.length} colisão(ões) de hash — o registro de um SOBRESCREVE o outro:\n`);
for (const [h, m] of collisions) {
  console.error(`  hash ${h}:`);
  for (const t of m.values()) console.error(`    [${t.source}] ${t.fqn}  (${t.file})`);
  console.error('');
}
console.error('Renomeie um dos tipos (o namespace conta na hash) e recompile.');
process.exit(1);

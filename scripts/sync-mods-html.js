#!/usr/bin/env node
/**
 * sync-mods-html.js
 * Parses docs/migration/mods-inventory.md and updates the const MODS array
 * in docs/migration/mods-inventory.html.
 *
 * Usage: node scripts/sync-mods-html.js
 */

'use strict';

const fs   = require('fs');
const path = require('path');

const ROOT      = path.resolve(__dirname, '..');
const MD_FILE   = path.join(ROOT, 'docs/migration/mods-inventory.md');
const HTML_FILE = path.join(ROOT, 'docs/migration/mods-inventory.html');

// ── Text helpers ──────────────────────────────────────────────────────────────

// Strip leading emoji/symbols up to the first Latin (or accented) letter.
// "🖥️ Client" → "Client"   "🤖 IA" → "IA"   "🧩 Framework/Base" → "Framework/Base"
function stripEmoji(s) {
  return s.replace(/^[^A-Za-zÀ-ÿĀ-ɏ]+/, '').trim();
}

function isSearch(s) { return s.includes('🔍'); }
function isAbsent(s) { const t = s.trim(); return t === '—' || t.startsWith('— '); }

// ── Field parsers ─────────────────────────────────────────────────────────────

function parseTipo(cell) {
  if (isSearch(cell)) return '?';
  const word = stripEmoji(cell).split(/[\s(]/)[0];
  return ['Client', 'Server', 'Misto'].includes(word) ? word : '?';
}

function parseAtuacao(cell) {
  if (isSearch(cell)) return '?';
  const word = stripEmoji(cell).split(/[\s(]/)[0];
  return ['Hideout', 'Raid', 'Ambos', 'Geral'].includes(word) ? word : '?';
}

function parseCategoria(cell) {
  if (isSearch(cell)) return '?';
  const text = stripEmoji(cell);
  if (text.startsWith('Quality of Life')) return 'QoL';
  if (text.startsWith('Framework/Base')) return 'Framework';
  const word = text.split(/[\s/]/)[0];
  const VALID = ['QoL','Realismo','Hardcore','Balanceamento','Progressão','Conteúdo','Framework','Cosmético'];
  return VALID.includes(word) ? word : '?';
}

function parseEscopo(cell) {
  if (isSearch(cell)) return '?';
  return cell
    .split(/\s*·\s*/)
    .map(p => stripEmoji(p).split(/[\s/]/)[0])
    .filter(Boolean)
    .join(',');
}

// Forge column: "[236](https://forge.sp-tarkov.com/mod/236/...)" → "236"
// "—" or "— (note)" → null    "🔍" → ""
function parseForge(cell) {
  if (isAbsent(cell)) return null;
  if (isSearch(cell)) return '';
  const m = cell.match(/forge\.sp-tarkov\.com\/mod\/(\d+)/);
  if (m) return m[1];
  return '';
}

// Repo 4.0 column: GitHub → "user/repo"  GitLab/other → full URL
// "—" → null    "🔍" → ""
function parseR4(cell) {
  if (isAbsent(cell)) return null;
  if (isSearch(cell)) return '';
  const m = cell.match(/\[.*?\]\((https?:\/\/[^)]+)\)/);
  if (!m) return '';
  const url = m[1];
  if (url.includes('gitlab.com') || !url.includes('github.com')) return url;
  // GitHub: keep only user/repo, strip /tree/... or /blob/... suffixes
  const gm = url.match(/github\.com\/([^/]+\/[^/\s)#]+)/);
  return gm ? gm[1] : url;
}

// Remove markdown links, keep display text
function parseFn(cell) {
  return cell.replace(/\[([^\]]+)\]\([^)]+\)/g, '$1').trim();
}

function parseStatus(cell) {
  if (cell.includes('Evoluir'))    return 'Evoluir';
  if (cell.includes('Instalar'))   return 'Instalar';
  if (cell.includes('Desenvolver')) return 'Desenvolver';
  if (cell.includes('Aguardar'))   return 'Aguardar';
  if (cell.includes('Bloqueado'))  return 'Bloqueado';
  if (cell.includes('incluir'))    return 'NaoIncluir';
  return 'Avaliar';
}

function parsePrioridade(cell) {
  if (isSearch(cell)) return '?';
  const word = stripEmoji(cell).split(/[\s—]/)[0];
  const VALID = ['Crítica','Alta','Média','Baixa'];
  return VALID.includes(word) ? word : '?';
}

function parseSpt4(cell) {
  if (cell.includes('✅')) return true;
  if (cell.includes('❌')) return false;
  return null;
}

function parseTrl(cell) {
  const t = cell.trim();
  if (t === 'New') return 'New';
  if (t === 'Sim') return 'Sim';
  return null;
}

// Installed column: "✓" → true, anything else ("—", empty, missing) → false
function parseInstalado(cell) {
  return (cell || '').includes('✓');
}

// ── Main table (mods #1–N) ────────────────────────────────────────────────────

function parseTable(md) {
  const start = md.indexOf('## Inventário completo');
  if (start === -1) throw new Error('"## Inventário completo" not found');

  const rows = [];
  const lineRe = /^\|\s*(\d+)\s*\|(.+)$/gm;
  lineRe.lastIndex = start;
  let match;

  while ((match = lineRe.exec(md)) !== null) {
    const cols = match[2].split('|').map(c => c.trim());
    // cols: [Mod, Tipo, Atuação, Categoria, Escopo, Forge, Repo3x, Repo4.0, SPT4?, Função, Status, Prioridade, TRL3?, Instalado, ...]
    if (cols.length < 12) continue;

    const n      = parseInt(match[1]);
    const name   = parseFn(cols[0]); // strip any markdown links in mod name
    const interno = name.includes('🏠');

    rows.push([
      n,
      name,
      parseTipo(cols[1]),
      parseAtuacao(cols[2]),
      parseCategoria(cols[3]),
      parseEscopo(cols[4]),
      parseForge(cols[5]),
      // cols[6] = Repo 3.x — skipped
      parseR4(cols[7]),
      parseFn(cols[9]),
      parseStatus(cols[10]),
      parsePrioridade(cols[11]),
      interno,
      parseSpt4(cols[8]),
      parseTrl(cols[12] || ''),
      parseInstalado(cols[13]),
    ]);
  }

  return rows;
}

// ── Serialise MODS array ──────────────────────────────────────────────────────

function jsVal(v) {
  if (v === null)           return 'null';
  if (typeof v === 'boolean') return String(v);
  if (typeof v === 'number')  return String(v);
  return '"' + String(v).replace(/\\/g, '\\\\').replace(/"/g, '\\"') + '"';
}

function formatMods(mods) {
  const lines = mods.map(m =>
    '[' + m.map(jsVal).join(',') + ']'
  );
  return 'const MODS = [\n' + lines.join(',\n') + ',\n];';
}

// ── History entry ─────────────────────────────────────────────────────────────

function addHistory(md) {
  const date  = new Date().toISOString().slice(0, 10);
  const entry = `| ${date} | sync-script | docs(migration): sync mods-inventory.html from markdown |`;
  // Insert after the last history row (any line starting with "| 20")
  const lastRow = md.lastIndexOf('\n| 20');
  if (lastRow === -1) return md;
  const lineEnd = md.indexOf('\n', lastRow + 1);
  return md.slice(0, lineEnd + 1) + entry + '\n' + md.slice(lineEnd + 1);
}

// ── Validation ──────────────────────────────────────────────────────────────────

// Surfaces issues that would silently corrupt the HTML or make a row uneditable by
// the server: a '|' inside a cell (shifts every column → wrong fields parsed) or a
// reused mod number (#n matches the first row only on write, and the HTML gets two
// entries for the same n). Advisory by design: the server still regenerates and the
// .md write already landed, but the CLI fails loudly so add-mod surfaces it.
function collectIssues(md, mods) {
  const issues = [];

  const start = md.indexOf('## Inventário completo');
  if (start !== -1) {
    const re = /^\|\s*(\d+)\s*\|(.+)$/gm;
    re.lastIndex = start;
    let m;
    while ((m = re.exec(md)) !== null) {
      const cols = m[2].split('|'); // 14 cells + trailing '' = 15 when well-formed
      if (cols.length !== 15) {
        issues.push(`linha #${m[1]}: ${cols.length} colunas (esperado 15) — provável '|' dentro de uma célula; o servidor não conseguirá editá-la`);
      }
    }
  }

  const seen = new Set();
  for (const r of mods) {
    if (seen.has(r[0])) issues.push(`número de mod duplicado: #${r[0]}`);
    else seen.add(r[0]);
  }

  return issues;
}

// ── Sync (md → html) ────────────────────────────────────────────────────────────

// Regenerates the `const MODS` block in the HTML from the markdown.
// history:true appends a row to the markdown "## Histórico" (use for manual/CLI
// publishing). history:false skips it (use for per-click writes from the server,
// to avoid spamming the history on every toggle). Returns the mod count.
function syncHtml({ history = true } = {}) {
  const md   = fs.readFileSync(MD_FILE,   'utf8');
  let   html = fs.readFileSync(HTML_FILE, 'utf8');

  const mods = parseTable(md); // inventory starts at mod #1 (no separate base block)

  const issues = collectIssues(md, mods);
  syncHtml.lastIssues = issues;                       // CLI reads this to set its exit code
  issues.forEach(w => console.warn('  ⚠ validação: ' + w));

  const modsBlock = formatMods(mods);
  const MODS_RE   = /const MODS = \[[\s\S]*?\];/;
  if (!MODS_RE.test(html)) throw new Error('const MODS block not found in HTML');

  html = html.replace(MODS_RE, modsBlock);
  fs.writeFileSync(HTML_FILE, html, 'utf8');

  if (history) fs.writeFileSync(MD_FILE, addHistory(md), 'utf8');

  return mods.length;
}

module.exports = { syncHtml, collectIssues, MD_FILE, HTML_FILE };

// ── CLI ─────────────────────────────────────────────────────────────────────────
if (require.main === module) {
  const count = syncHtml({ history: true });
  console.log(`Parsed ${count} mods (starting at #1)`);
  console.log(`✓ Updated ${HTML_FILE}`);
  console.log(`✓ History entry added to ${MD_FILE}`);

  const issues = syncHtml.lastIssues || [];
  if (issues.length) {
    console.error(`\n✗ ${issues.length} problema(s) de validação — corrija o .md antes de prosseguir:`);
    issues.forEach(w => console.error('  • ' + w));
    process.exit(1);
  }
}

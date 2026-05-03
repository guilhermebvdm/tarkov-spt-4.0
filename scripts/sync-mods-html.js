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

// ── UltraFika (vertical table, mod #0) ────────────────────────────────────────

function parseUltraFika(md) {
  const sec = md.match(/## Base — 🏠 UltraFika-Plugin[\s\S]*?(?=\n## )/);
  if (!sec) throw new Error('UltraFika section not found');
  const text = sec[0];

  function row(label) {
    const re = new RegExp(`\\|\\s*\\*\\*${label}\\*\\*\\s*\\|\\s*(.+?)\\s*\\|`);
    const m = text.match(re);
    return m ? m[1].trim() : '';
  }

  return [
    0,
    '🏠 UltraFika-Plugin',
    parseTipo(row('Tipo')),
    parseAtuacao(row('Atuação')),
    parseCategoria(row('Categoria')),
    parseEscopo(row('Escopo')),
    parseForge(row('Forge')),
    parseR4(row('Repo 4[.]0')),
    parseFn(row('Função')),
    parseStatus(row('Status')),
    parsePrioridade(row('Prioridade')),
    true,
  ];
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
    // cols: [Mod, Tipo, Atuação, Categoria, Escopo, Forge, Repo3x, Repo4.0, Função, Status, Prioridade, ...]
    if (cols.length < 11) continue;

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
      parseFn(cols[8]),
      parseStatus(cols[9]),
      parsePrioridade(cols[10]),
      interno,
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

// ── Main ──────────────────────────────────────────────────────────────────────

function main() {
  const md   = fs.readFileSync(MD_FILE,   'utf8');
  let   html = fs.readFileSync(HTML_FILE, 'utf8');

  const ultra = parseUltraFika(md);
  const rows  = parseTable(md);
  const mods  = [ultra, ...rows];

  console.log(`Parsed ${mods.length} mods (0–${mods.length - 1})`);

  const modsBlock = formatMods(mods);
  const MODS_RE   = /const MODS = \[[\s\S]*?\];/;
  if (!MODS_RE.test(html)) throw new Error('const MODS block not found in HTML');

  html = html.replace(MODS_RE, modsBlock);
  fs.writeFileSync(HTML_FILE, html, 'utf8');
  console.log(`✓ Updated ${HTML_FILE}`);

  const updatedMd = addHistory(md);
  fs.writeFileSync(MD_FILE, updatedMd, 'utf8');
  console.log(`✓ History entry added to ${MD_FILE}`);
}

main();

#!/usr/bin/env node
/**
 * serve-inventory.js
 * Local Node server (stdlib only — no npm install) that turns the mods inventory
 * page into a small editing system: changing the "Instalado" toggle or the "Status"
 * dropdown in the HTML writes the matching column in docs/migration/mods-inventory.md
 * (the source of truth / "database") and re-syncs the HTML. Commit + push to share
 * with other editors — git is the sync layer between machines. Binds to 127.0.0.1.
 *
 * Usage:  node scripts/serve-inventory.js   (then open http://localhost:8787)
 *         PORT=9000 node scripts/serve-inventory.js
 */
'use strict';

const http = require('http');
const fs   = require('fs');
const { syncHtml, MD_FILE, HTML_FILE } = require('./sync-mods-html');

const PORT = Number(process.env.PORT) || 8787;

// Column indices within the inventory table (after the leading "| N |"):
// Mod(0) Tipo(1) Atuação(2) Categoria(3) Escopo(4) Forge(5) Repo3x(6) Repo4.0(7)
// SPT4?(8) Função(9) Status(10) Prioridade(11) TRL(12) Instalado(13) + trailing(14)
const COL_STATUS    = 10;
const COL_INSTALLED = 13;

// Canonical status-key → markdown cell text (Option 1). Writing a status via the
// server replaces the whole Status cell with this text, so free-form notes in the
// cell (e.g. "🟠 Aguardar upstream" → kept; "...| 🟠 Aguardar (nota) |" → loses the
// note). Each value round-trips back to its key through parseStatus in the sync.
const STATUS_MD = {
  Avaliar:     '🟡 Avaliar',
  Instalar:    '🟢 À Instalar',
  Evoluir:     '⬆️ Evoluir p/ 4.0',
  Desenvolver: '🔧 Desenvolver',
  Aguardar:    '🟠 Aguardar upstream',
  Bloqueado:   '🔴 Bloqueado',
  NaoIncluir:  '⚫ Não incluir',
};

// ━━━ Write a single column cell (by index) for mod #n in the markdown table ━━━━━━━
function setCellInMd(n, colIndex, value) {
  const lines = fs.readFileSync(MD_FILE, 'utf8').split('\n');
  
  if (n === 0 && colIndex === COL_STATUS) {
    for (let i = 0; i < lines.length; i++) {
      if (lines[i].startsWith('## Inventário completo')) break;
      if (lines[i].startsWith('| **Status** |')) {
        lines[i] = `| **Status** | ${value} |`;
        fs.writeFileSync(MD_FILE, lines.join('\n'), 'utf8');
        return true;
      }
    }
    return false;
  }

  for (let i = 0; i < lines.length; i++) {
    const m = lines[i].match(/^\|\s*(\d+)\s*\|(.+)$/);
    if (m && parseInt(m[1]) === n) {
      const cols = m[2].split('|');          // Mod(0) … Instalado(13), trailing ''(14)
      if (cols.length !== 15) return false;  // unexpected row shape — bail (avoid corruption)
      cols[colIndex] = ` ${value} `;
      lines[i] = `| ${m[1]} |` + cols.join('|');
      fs.writeFileSync(MD_FILE, lines.join('\n'), 'utf8');
      return true;
    }
  }
  return false; // no matching row (e.g. #0 UltraFika lives in a separate block)
}

// ── HTTP helpers ─────────────────────────────────────────────────────────────────
function sendJson(res, code, obj) {
  res.writeHead(code, { 'Content-Type': 'application/json; charset=utf-8' });
  res.end(JSON.stringify(obj));
}

function badRequest(msg) { const e = new Error(msg); e.statusCode = 400; return e; }

function readBody(req) {
  return new Promise((resolve, reject) => {
    let data = '';
    req.on('data', c => {
      data += c;
      if (data.length > 1e6) { req.destroy(); reject(badRequest('corpo da requisição muito grande')); }
    });
    req.on('end', () => resolve(data));
    req.on('error', reject);
  });
}

async function readJson(req) {
  const raw = await readBody(req);
  try { return JSON.parse(raw || '{}'); }
  catch { throw badRequest('corpo JSON inválido'); }
}

// ── Server ────────────────────────────────────────────────────────────────────────
const server = http.createServer(async (req, res) => {
  try {
    if (req.method === 'GET' && (req.url === '/' || req.url.split('?')[0] === '/mods-inventory.html')) {
      res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8', 'Cache-Control': 'no-store' });
      return res.end(fs.readFileSync(HTML_FILE));
    }

    if (req.method === 'GET' && req.url === '/favicon.ico') {
      res.writeHead(204).end(); // no favicon — keep the console clean
      return;
    }

    if (req.method === 'GET' && req.url === '/api/health') {
      return sendJson(res, 200, { ok: true });
    }

    if (req.method === 'POST' && req.url === '/api/installed') {
      const payload = await readJson(req);
      const n = Number(payload.n);
      const installed = !!payload.installed;
      if (!Number.isInteger(n)) return sendJson(res, 400, { ok: false, error: 'n must be an integer' });

      if (!setCellInMd(n, COL_INSTALLED, installed ? '✓' : '—')) {
        return sendJson(res, 404, { ok: false, error: `mod #${n} não encontrado na tabela` });
      }
      const count = syncHtml({ history: false }); // regen HTML, no history spam per click
      console.log(`[installed] #${n} → ${installed ? '✓' : '—'} (synced ${count} mods)`);
      return sendJson(res, 200, { ok: true, n, installed });
    }

    if (req.method === 'POST' && req.url === '/api/status') {
      const payload = await readJson(req);
      const n = Number(payload.n);
      const status = String(payload.status || '');
      if (!Number.isInteger(n)) return sendJson(res, 400, { ok: false, error: 'n must be an integer' });
      if (!STATUS_MD[status]) return sendJson(res, 400, { ok: false, error: `status inválido: ${status}` });

      if (!setCellInMd(n, COL_STATUS, STATUS_MD[status])) {
        return sendJson(res, 404, { ok: false, error: `mod #${n} não encontrado na tabela` });
      }
      const count = syncHtml({ history: false });
      console.log(`[status] #${n} → ${status} (synced ${count} mods)`);
      return sendJson(res, 200, { ok: true, n, status });
    }

    sendJson(res, 404, { ok: false, error: 'not found' });
  } catch (err) {
    if (err && err.statusCode) return sendJson(res, err.statusCode, { ok: false, error: err.message });
    console.error(err);
    sendJson(res, 500, { ok: false, error: String((err && err.message) || err) });
  }
});

server.on('error', (err) => {
  if (err.code === 'EADDRINUSE') {
    console.error(`\n  ✗ Porta ${PORT} já está em uso. Rode com outra: PORT=9000 node scripts/serve-inventory.js\n`);
    process.exit(1);
  }
  throw err;
});

server.listen(PORT, '127.0.0.1', () => {
  console.log(`
  Mods Inventory — servidor de edição
  → http://localhost:${PORT}

  Mexer em "Instalado" ou "Status" na página escreve a coluna correspondente em:
    ${MD_FILE}
  e re-sincroniza o HTML. Faça commit + push para compartilhar com os outros editores.
  Pare com Ctrl+C.
`);
});

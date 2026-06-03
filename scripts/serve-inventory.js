#!/usr/bin/env node
/**
 * serve-inventory.js
 * Local Node server (stdlib only — no npm install) that turns the mods inventory
 * page into a small editing system: clicking the "Instalado" toggle in the HTML
 * writes the Instalado column in docs/migration/mods-inventory.md (the source of
 * truth / "database") and re-syncs the HTML. Commit + push to share with other
 * editors — git is the sync layer between machines.
 *
 * Usage:  node scripts/serve-inventory.js   (then open http://localhost:8787)
 *         PORT=9000 node scripts/serve-inventory.js
 */
'use strict';

const http = require('http');
const fs   = require('fs');
const { syncHtml, MD_FILE, HTML_FILE } = require('./sync-mods-html');

const PORT = Number(process.env.PORT) || 8787;

// ── Write the Instalado cell (last column) for mod #n in the markdown ────────────
function setInstalledInMd(n, installed) {
  const lines = fs.readFileSync(MD_FILE, 'utf8').split('\n');
  const cell  = installed ? ' ✓ |' : ' — |';
  for (let i = 0; i < lines.length; i++) {
    const m = lines[i].match(/^\|\s*(\d+)\s*\|/);
    if (m && parseInt(m[1]) === n) {
      lines[i] = lines[i].replace(/\|[^|]*\|\s*$/, '|' + cell); // replace last cell
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

function readBody(req) {
  return new Promise((resolve, reject) => {
    let data = '';
    req.on('data', c => { data += c; if (data.length > 1e6) req.destroy(); });
    req.on('end', () => resolve(data));
    req.on('error', reject);
  });
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
      const payload = JSON.parse(await readBody(req) || '{}');
      const n = Number(payload.n);
      const installed = !!payload.installed;
      if (!Number.isInteger(n)) return sendJson(res, 400, { ok: false, error: 'n must be an integer' });

      if (!setInstalledInMd(n, installed)) {
        return sendJson(res, 404, { ok: false, error: `mod #${n} não encontrado na tabela` });
      }
      const count = syncHtml({ history: false }); // regen HTML, no history spam per click
      console.log(`[installed] #${n} → ${installed ? '✓' : '—'} (synced ${count} mods)`);
      return sendJson(res, 200, { ok: true, n, installed });
    }

    sendJson(res, 404, { ok: false, error: 'not found' });
  } catch (err) {
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

server.listen(PORT, () => {
  console.log(`
  Mods Inventory — servidor de edição
  → http://localhost:${PORT}

  Clicar em "Instalado" na página escreve a coluna Instalado em:
    ${MD_FILE}
  e re-sincroniza o HTML. Faça commit + push para compartilhar com os outros editores.
  Pare com Ctrl+C.
`);
});

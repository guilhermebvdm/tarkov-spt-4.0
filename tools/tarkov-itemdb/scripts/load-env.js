'use strict';
// Zero-dependency .env loader. Populates process.env from <tool-root>/.env
// (KEY=VALUE per line) for keys not already set — so TARKOV_MARKET_API_KEY works
// without exporting a shell/system env var. Required by serve.js and
// fetch-tarkov-market.js. The .env file is gitignored (keeps the key out of git).
const fs   = require('fs');
const path = require('path');

const envPath = path.join(__dirname, '..', '.env');
try {
  for (const line of fs.readFileSync(envPath, 'utf8').replace(/^﻿/, '').split(/\r?\n/)) {
    if (!line || /^\s*#/.test(line)) continue;
    const m = line.match(/^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.*?)\s*$/);
    if (m && process.env[m[1]] === undefined) {
      process.env[m[1]] = m[2].replace(/^(["'])(.*)\1$/, '$2');
    }
  }
} catch (_) { /* no .env file — fine, env may be set elsewhere */ }

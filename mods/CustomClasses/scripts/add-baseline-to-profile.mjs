#!/usr/bin/env node
/**
 * add-baseline-to-profile.mjs — injeta o KIT BÁSICO (BASELINE) no stash de um profile SPT salvo,
 * para que o profile-fonte vire fonte completa do extract-from-profile. Sem deps.
 *
 * ⚠️ EDITA UM SAVE REAL. Pré-condições: jogo EFT e SPT.Server FECHADOS (senão o autosave sobrescreve).
 * Faz backup `<profile>.json.bak` antes de gravar. União por tpl (não duplica itens já presentes).
 * Posiciona os itens em linhas LIVRES abaixo de tudo (sem sobrepor), usando tamanho real da DB de itens.
 *
 * Uso: node mods/CustomClasses/scripts/add-baseline-to-profile.mjs --profile <id> [--dry-run]
 * SPT path via env SPT_PATH (default D:/SPT).
 */
'use strict';
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';

const SPT = process.env.SPT_PATH ?? 'D:/SPT';
const PROFILE_DIR = path.join(SPT, 'SPT', 'user', 'profiles');
const ITEMS_DB = path.join(SPT, 'SPT', 'SPT_Data', 'database', 'templates', 'items.json');

// Mesmo BASELINE do gerador (build-class-jsons.js) e do extract-from-profile.mjs.
const BASELINE = [
  { tpl: '5449016a4bdc2d6f028b456f', count: 300000 }, // Roubles (baseline-v2: 300k)
  { tpl: '544fb45d4bdc2dee738b4568', count: 1 },      // Salewa
  { tpl: '5751a25924597722c463c472', count: 2 },      // Army bandage
  { tpl: '5af0454c86f7746bf20992e8', count: 1 },      // Aluminum splint
  { tpl: '544fb37f4bdc2dee738b4567', count: 1 },      // Analgin
  { tpl: '590c5f0d86f77413997acfab', count: 1 },      // MRE
  { tpl: '5448ff904bdc2d6f028b456e', count: 1 },      // Army crackers
  { tpl: '5c0fa877d174af02a012e1cf', count: 1 },      // Aquamari
  { tpl: '5bffdc370db834001d23eca8', count: 1 },      // 6Kh5 Bayonet
];

const warn = m => process.stderr.write(`[baseline] ${m}\n`);
const mongoId = () => crypto.randomBytes(12).toString('hex');

function parseArgs(argv) {
  const a = { dryRun: false };
  for (let i = 2; i < argv.length; i++) {
    if (argv[i] === '--profile') a.profileId = argv[++i];
    else if (argv[i] === '--dry-run') a.dryRun = true;
    else warn(`flag ignorada: ${argv[i]}`);
  }
  if (!a.profileId) { warn('uso: --profile <id> [--dry-run]'); process.exit(2); }
  return a;
}

function main() {
  const args = parseArgs(process.argv);
  const file = path.join(PROFILE_DIR, `${args.profileId}.json`);
  if (!fs.existsSync(file)) { warn(`profile não encontrado: ${file}`); process.exit(1); }
  if (!fs.existsSync(ITEMS_DB)) { warn(`DB de itens não encontrada: ${ITEMS_DB}`); process.exit(1); }

  const raw = fs.readFileSync(file, 'utf8');
  const profile = JSON.parse(raw);
  const db = JSON.parse(fs.readFileSync(ITEMS_DB, 'utf8'));
  const sizeOf = tpl => {
    const p = db[tpl]?._props;
    return { w: p?.Width ?? 1, h: p?.Height ?? 1 };
  };

  const inv = profile?.characters?.pmc?.Inventory;
  if (!inv?.items || !inv.stash) { warn('profile sem Inventory.items/stash'); process.exit(1); }

  const stashId = inv.stash;
  const stashItem = inv.items.find(i => i._id === stashId);
  if (!stashItem) { warn('item raiz do stash não encontrado'); process.exit(1); }
  const grid = db[stashItem._tpl]?._props?.Grids?.[0]?._props;
  const cellsH = grid?.cellsH || 10;
  const cellsV = grid?.cellsV || 999;   // 0/ausente → tratar como muito alto

  // Itens já no stash → ocupação (tamanho real + rotação) e união por tpl.
  const stashChildren = inv.items.filter(i => i.parentId === stashId);
  const present = new Set(stashChildren.map(i => i._tpl));
  let maxY = 0;
  for (const it of stashChildren) {
    const { w, h } = sizeOf(it._tpl);
    const vert = it.location?.r === 'Vertical';
    const iw = vert ? h : w, ih = vert ? w : h;
    const x = it.location?.x ?? 0, y = it.location?.y ?? 0;
    maxY = Math.max(maxY, y + ih);
    void iw;
  }
  const startY = maxY + 2;   // margem de 2 linhas abaixo de tudo (segurança contra ExtraSize)

  const toAdd = BASELINE.filter(b => !present.has(b.tpl));
  if (toAdd.length === 0) { warn('nada a adicionar — baseline já presente.'); return; }

  // Coloca em linhas livres a partir de startY (first-fit simples, esquerda→direita, quebra de linha).
  const newItems = [];
  let cx = 0, cy = startY, rowH = 0;
  for (const b of toAdd) {
    const { w, h } = sizeOf(b.tpl);
    if (cx + w > cellsH) { cx = 0; cy += rowH || 1; rowH = 0; }
    if (cy + h > cellsV) { warn(`sem espaço no stash (cellsV=${cellsV}) — ${b.tpl} pulado`); continue; }
    const item = {
      _id: mongoId(),
      _tpl: b.tpl,
      parentId: stashId,
      slotId: 'hideout',
      location: { x: cx, y: cy, r: 'Horizontal', isSearched: true },
    };
    const dbItem = db[b.tpl]?._props;
    const stackMax = dbItem?.StackMaxSize ?? 1;
    if (stackMax > 1 || b.count > 1) item.upd = { StackObjectsCount: Math.min(b.count, stackMax > 1 ? stackMax : b.count) };
    newItems.push(item);
    cx += w; rowH = Math.max(rowH, h);
  }

  warn(`stash grid ${cellsH}x${cellsV}; itens existentes ${stashChildren.length} (maxY=${maxY}); adicionando ${newItems.length} a partir de y=${startY}`);
  for (const it of newItems) warn(`  + ${it._tpl} @ (${it.location.x},${it.location.y}) x${it.upd?.StackObjectsCount ?? 1}`);

  if (args.dryRun) { warn('DRY-RUN — nada escrito.'); return; }

  fs.copyFileSync(file, `${file}.bak`);
  inv.items.push(...newItems);
  fs.writeFileSync(file, JSON.stringify(profile, null, 2));   // profile usa indentação de 2 espaços
  warn(`gravado: ${file} (backup .bak criado). Reabra o jogo e confira o stash.`);
}

main();

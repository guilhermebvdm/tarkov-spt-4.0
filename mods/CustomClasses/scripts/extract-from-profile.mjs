#!/usr/bin/env node
/**
 * extract-from-profile.mjs — copia OUTFIT + EQUIPADOS + STASH + DINHEIRO de um profile SPT 4.0
 * para a definição de uma classe do CustomClasses (`config/classes/*.jsonc`). Sem deps.
 *
 * O profile é a fonte visual; quem reconstrói o inventário é o builder C# (InventoryBuilder/OutfitBuilder).
 * Este script emite o ItemSpec MÍNIMO que, ao passar pelo builder, reproduz o profile (cópia fiel da
 * árvore de mods — não casa presets). Faz MERGE CIRÚRGICO: substitui só outfit + loadout.equipped +
 * loadout.stash, preservando name/skills/skillMultipliers/hideout/etc. (não destrói o balance).
 *
 * Uso:
 *   node mods/CustomClasses/scripts/extract-from-profile.mjs --profile <id> --class <arquivo.jsonc>
 *        [--no-stash] [--no-money] [--faction usec|bear|both] [--dry-run]
 *
 * Grava em repo E install (backup .bak no install). SPT path via env SPT_PATH (default D:/SPT).
 * Detalhes/decisões: docs/balance-model.md vizinho + plano do épico 046.
 *
 * ROBUSTEZ (evita as quebras observadas ao criar classes):
 *  - Valida todo tpl contra items.json → pula itens phantom/órfãos (cache miss → boneco não renderiza).
 *  - NÃO pina x/y no stash → auto-pack do builder (pinning de uma stash grande estourava p/ sorting table).
 *  - Outfit por SUIT (reverse-lookup appearance→suit) com Side válido → entra no dropdown e persiste;
 *    roupa Savage/inválida é pulada preservando o lado existente.
 *  LIMITAÇÃO: items.json é a DB VANILLA — itens adicionados por MODS (WTT, etc.) não estão nela e serão
 *  pulados com aviso. Se precisar de gear modado, conferir o aviso e o mod correspondente.
 */
'use strict';
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { stripJsonc } from './skill-weights.mjs';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const SPT = process.env.SPT_PATH ?? 'D:/SPT';
const REPO_CLASSES = path.join(__dirname, '..', 'modded', 'Server', 'config', 'classes');
const INSTALL_CLASSES = path.join(SPT, 'SPT', 'user', 'mods', 'CustomClasses', 'config', 'classes');
const PROFILE_DIR = path.join(SPT, 'SPT', 'user', 'profiles');
const ITEMS_DB = path.join(SPT, 'SPT', 'SPT_Data', 'database', 'templates', 'items.json');
const CUST_DB = path.join(SPT, 'SPT', 'SPT_Data', 'database', 'templates', 'customization.json');

// Estado das DBs do server (carregado em loadDbs). VALID = tpls que existem na items.json (evita
// copiar itens phantom/moddados que não existem server-side e quebram o cache/render). SUIT_BY_* =
// reverse-lookup de aparência→suit p/ o outfit (o dropdown lista SUITS, não appearances cruas).
let VALID = new Set();
const EXCLUDE = new Set();         // tpls a ignorar explicitamente (--exclude), ex.: DSP
const SUIT_BY_BODY = new Map();   // appearanceId(Body) → [{ id, sides:Set }]
const SUIT_BY_FEET = new Map();   // appearanceId(Feet) → [{ id, sides:Set }]
const MODS_DIR = path.join(SPT, 'SPT', 'user', 'mods');
const HEX24 = /^[a-f0-9]{24}$/;

/** Coleta recursiva de tpls de item declarados nas DBs dos mods (ex.: WTT-PackNStrap CustomItems —
 *  top-level keys = tpls novos; formato padrão = objetos com _id). Sem isso, gear de mod (que existe em
 *  RUNTIME mas não na items.json vanilla) seria dropado como "phantom". */
function collectModItemIds(dir, into) {
  let entries;
  try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return; }
  for (const e of entries) {
    const full = path.join(dir, e.name);
    if (e.isDirectory()) { collectModItemIds(full, into); continue; }
    if (!e.name.endsWith('.json')) continue;
    let j; try { j = JSON.parse(fs.readFileSync(full, 'utf8')); } catch { continue; }
    const scan = node => {
      if (!node || typeof node !== 'object') return;
      if (Array.isArray(node)) { for (const v of node) scan(v); return; }
      for (const [k, v] of Object.entries(node)) {
        if (HEX24.test(k)) into.add(k);                 // WTT: key = tpl novo
        if (k === '_id' && typeof v === 'string' && HEX24.test(v)) into.add(v);  // formato padrão
      }
    };
    scan(j);
  }
}

function loadDbs() {
  if (!fs.existsSync(ITEMS_DB)) { warn(`DB de itens não encontrada: ${ITEMS_DB}`); process.exit(1); }
  VALID = new Set(Object.keys(JSON.parse(fs.readFileSync(ITEMS_DB, 'utf8'))));
  const before = VALID.size;
  for (const d of (fs.existsSync(MODS_DIR) ? fs.readdirSync(MODS_DIR) : [])) {
    collectModItemIds(path.join(MODS_DIR, d, 'db'), VALID);
  }
  warn(`DB: ${before} itens vanilla + ${VALID.size - before} de mods = ${VALID.size} válidos.`);
  if (fs.existsSync(CUST_DB)) {
    const cust = JSON.parse(fs.readFileSync(CUST_DB, 'utf8'));
    for (const [id, c] of Object.entries(cust)) {
      const p = c?._props; if (!p) continue;
      const sides = new Set((p.Side ?? []).map(s => String(s).toLowerCase()));   // 'usec'|'bear'|'savage'
      if (p.Body) (SUIT_BY_BODY.get(p.Body) ?? SUIT_BY_BODY.set(p.Body, []).get(p.Body)).push({ id, sides });
      if (p.Feet) (SUIT_BY_FEET.get(p.Feet) ?? SUIT_BY_FEET.set(p.Feet, []).get(p.Feet)).push({ id, sides });
    }
  } else {
    warn(`DB de customization não encontrada: ${CUST_DB} — outfit não será resolvido p/ suit.`);
  }
}

// Moedas (tpl → código) — verificados no profile real. Teto sanitário por moeda (o profile de teste
// vem com ~497M RUB; uma classe não deve nascer com isso).
const CURRENCY = {
  '5449016a4bdc2d6f028b456f': 'RUB',
  '5696686a4bdc2da3298b456a': 'USD',
  '569668774bdc2da2298b4568': 'EUR',
  '5d235b4d86f7742e017bc88a': 'GP',
};
const CURRENCY_CAP = { RUB: 500000, USD: 5000, EUR: 5000, GP: 5000 };
const RUB_TPL = '5449016a4bdc2d6f028b456f';
const DEFAULT_RUBLES = 100000;   // rublos-padrão adicionados ao stash (--rubles N p/ mudar, 0 = nenhum)

// Kit básico de stash compartilhado por todas as classes exceto o Peladão (BASELINE do gerador
// build-class-jsons.js). Com --with-baseline, é unido ao stash do profile (sem duplicar por tpl).
const BASELINE_STASH = [
  { tpl: '5449016a4bdc2d6f028b456f', count: 100000 }, // Roubles
  { tpl: '544fb45d4bdc2dee738b4568', count: 1 },      // Salewa
  { tpl: '5751a25924597722c463c472', count: 2 },      // Army bandage
  { tpl: '5af0454c86f7746bf20992e8', count: 1 },      // Aluminum splint
  { tpl: '544fb37f4bdc2dee738b4567', count: 1 },      // Analgin
  { tpl: '590c5f0d86f77413997acfab', count: 1 },      // MRE
  { tpl: '5448ff904bdc2d6f028b456e', count: 1 },      // Army crackers
  { tpl: '5c0fa877d174af02a012e1cf', count: 1 },      // Aquamari
  { tpl: '5bffdc370db834001d23eca8', count: 1 },      // 6Kh5 Bayonet
];

// Slots de equipamento válidos (EquipmentSlots do EFT). SKIP: gamma + bolso + bainha são fixos da edition.
const EQUIPMENT_SLOTS = new Set([
  'Headwear', 'Earpiece', 'FaceCover', 'ArmorVest', 'Eyewear', 'ArmBand',
  'TacticalVest', 'Pockets', 'Backpack', 'SecuredContainer',
  'FirstPrimaryWeapon', 'SecondPrimaryWeapon', 'Holster', 'Scabbard',
]);
const SKIP_SLOTS = new Set(['SecuredContainer', 'Pockets', 'Scabbard']);

const warn = msg => process.stderr.write(`[extract] ${msg}\n`);

// ── CLI ────────────────────────────────────────────────────────────────────────
function parseArgs(argv) {
  const a = { stash: true, money: true, outfit: true, faction: 'both', dryRun: false, withBaseline: false, rubles: DEFAULT_RUBLES, exclude: [] };
  for (let i = 2; i < argv.length; i++) {
    const t = argv[i];
    if (t === '--profile') a.profileId = argv[++i];
    else if (t === '--class') a.classFile = argv[++i];
    else if (t === '--no-stash') a.stash = false;
    else if (t === '--no-money') a.money = false;
    else if (t === '--no-outfit') a.outfit = false;
    else if (t === '--with-baseline') a.withBaseline = true;
    else if (t === '--rubles') a.rubles = Math.max(0, parseInt(argv[++i], 10) || 0);
    else if (t === '--exclude') a.exclude = (argv[++i] ?? '').split(',').map(s => s.trim()).filter(Boolean);
    else if (t === '--faction') a.faction = argv[++i];
    else if (t === '--dry-run') a.dryRun = true;
    else warn(`flag desconhecida ignorada: ${t}`);
  }
  if (!a.profileId || !a.classFile) {
    warn('uso: --profile <id> --class <arquivo.jsonc> [--no-stash] [--no-money] [--no-outfit] [--with-baseline] [--rubles N] [--exclude tpl,tpl] [--faction usec|bear|both] [--dry-run]');
    process.exit(2);
  }
  if (!['usec', 'bear', 'both'].includes(a.faction)) {
    warn(`--faction inválido: ${a.faction} (use usec|bear|both)`); process.exit(2);
  }
  if (!a.classFile.endsWith('.jsonc') && !a.classFile.endsWith('.json')) a.classFile += '.jsonc';
  return a;
}

/** Une o kit básico (BASELINE) ao stash, sem duplicar por tpl (o item do profile prevalece). */
function mergeBaseline(stash) {
  const present = new Set(stash.map(s => s.tpl));
  let added = 0;
  for (const b of BASELINE_STASH) {
    if (present.has(b.tpl)) continue;
    stash.push({ ...b });
    added++;
  }
  if (added) warn(`baseline: +${added} item(ns) do kit básico adicionados (sem duplicar).`);
  return stash;
}

// ── IO ─────────────────────────────────────────────────────────────────────────
function loadProfile(id) {
  const file = path.join(PROFILE_DIR, `${id}.json`);
  if (!fs.existsSync(file)) { warn(`profile não encontrado: ${file}`); process.exit(1); }
  const pmc = JSON.parse(fs.readFileSync(file, 'utf8'))?.characters?.pmc;
  if (!pmc?.Inventory?.items) { warn('profile sem characters.pmc.Inventory.items'); process.exit(1); }
  return pmc;
}

/** Lê a definição base da classe. Prefere o INSTALL (fonte de verdade do loadout); fallback ao repo. */
function loadClassDef(classFile) {
  const inst = path.join(INSTALL_CLASSES, classFile);
  const repo = path.join(REPO_CLASSES, classFile);
  const src = fs.existsSync(inst) ? inst : (fs.existsSync(repo) ? repo : null);
  if (!src) { warn(`classe não encontrada (install nem repo): ${classFile}`); process.exit(1); }
  return { def: JSON.parse(stripJsonc(fs.readFileSync(src, 'utf8'))), from: src };
}

// ── índice ──────────────────────────────────────────────────────────────────────
function indexItems(items) {
  const childrenOf = new Map();
  for (const it of items) {
    if (it.parentId) {
      if (!childrenOf.has(it.parentId)) childrenOf.set(it.parentId, []);
      childrenOf.get(it.parentId).push(it);
    }
  }
  return { children: id => childrenOf.get(id) ?? [] };
}

// ── munição derivada ──────────────────────────────────────────────────────────
/** Varre descendentes da arma: cartridges (dentro do mod_magazine) → loadedMag; patron_in_weapon → chambered.
 *  Ignora cartucho cujo tpl não existe na DB (não dá pra carregar munição inexistente). */
function deriveAmmo(weapon, idx) {
  let ammo, loadedMag = false, chambered = false;
  const walk = id => {
    for (const c of idx.children(id)) {
      if (!VALID.has(c._tpl)) { walk(c._id); continue; }
      if (c.slotId === 'cartridges') { ammo ??= c._tpl; loadedMag = true; }
      else if (c.slotId?.startsWith('patron')) { ammo ??= c._tpl; chambered = true; }
      walk(c._id);
    }
  };
  walk(weapon._id);
  return { ammo, loadedMag, chambered };
}

// ── árvore de mods (filhos SEM location, exceto cartridges/patron) ───────────────
function buildModTree(item, idx) {
  const mods = [];
  for (const c of idx.children(item._id)) {
    if (c.location) continue;                                  // grade → contents, não mod
    if (c.slotId === 'cartridges' || c.slotId?.startsWith('patron')) continue;  // → ammo derivado
    if (EXCLUDE.has(c._tpl)) continue;                                         // --exclude
    if (!VALID.has(c._tpl)) { warn(`mod '${c._tpl}' (slot ${c.slotId}) fora da DB — pulado`); continue; }  // phantom/modado
    const node = { slotId: c.slotId, tpl: c._tpl };
    const sub = buildModTree(c, idx);
    if (sub.length) node.mods = sub;
    mods.push(node);
  }
  return mods;
}

// ── ItemSpec de um item (equipado / content / stash root) ───────────────────────
/** Retorna null se o item não existe na DB do server (phantom/modado) — o caller deve pular. */
function buildItemSpec(item, idx) {
  if (EXCLUDE.has(item._tpl)) return null;   // --exclude (ex.: DSP)
  if (!VALID.has(item._tpl)) { warn(`item '${item._tpl}' (slot ${item.slotId}) fora da DB — pulado`); return null; }
  const count = item.upd?.StackObjectsCount ?? 1;
  const spec = { tpl: item._tpl };

  if (count > 1) {
    // Empilhável (munição/moeda/meds em stack): caminho stack-aware do builder — NÃO emitir mods.
    spec.count = count;
  } else {
    // count==1: SEMPRE emitir mods (árvore, [] se vazio) para suprimir o auto-complete de preset default.
    spec.mods = buildModTree(item, idx);
    const { ammo, loadedMag, chambered } = deriveAmmo(item, idx);
    if (loadedMag) { spec.loadedMag = true; spec.ammo = ammo; }
    if (chambered) { spec.chambered = true; spec.ammo = ammo; }
    const contents = idx.children(item._id).filter(c => c.location).map(c => buildItemSpec(c, idx)).filter(Boolean);
    if (contents.length) spec.contents = contents;
  }
  // NB: NÃO emitir x/y/rotated — o auto-pack do builder posiciona. Pinning x/y de uma stash de origem
  // (edição grande) estourava p/ o sorting table numa stash menor (problema observado). Auto-pack é robusto.
  return spec;
}

// ── extrações ────────────────────────────────────────────────────────────────
function extractEquipped(pmc, idx) {
  const out = {};
  for (const it of idx.children(pmc.Inventory.equipment)) {
    const slot = it.slotId;
    if (!EQUIPMENT_SLOTS.has(slot)) { warn(`slot equipado desconhecido '${slot}' — ignorado`); continue; }
    if (SKIP_SLOTS.has(slot)) continue;
    const spec = buildItemSpec(it, idx);
    if (!spec) { warn(`slot '${slot}' pulado (item fora da DB)`); continue; }
    delete spec.count;   // count é ignorado em equipado
    if (out[slot]) warn(`slot '${slot}' duplicado — último vence`);
    out[slot] = spec;
  }
  return out;
}

function extractStash(pmc, idx, { money }) {
  const out = [];
  // Preserva a ORDEM que o usuário organizou: lê em ordem de leitura da grade (y, depois x).
  // Itens sem location vão ao fim. (Não pinamos x/y — só usamos a posição p/ ordenar; ver buildItemSpec.)
  const kids = [...idx.children(pmc.Inventory.stash)].sort((a, b) => {
    const ay = a.location?.y ?? 1e9, by = b.location?.y ?? 1e9;
    if (ay !== by) return ay - by;
    return (a.location?.x ?? 1e9) - (b.location?.x ?? 1e9);
  });
  for (const it of kids) {
    const code = CURRENCY[it._tpl];
    if (code) {
      if (!money) continue;
      const raw = it.upd?.StackObjectsCount ?? 1;
      const cap = CURRENCY_CAP[code] ?? raw;
      const count = Math.min(raw, cap);
      if (count < raw) warn(`${code} ${raw} → capado em ${count}`);
      out.push({ tpl: it._tpl, count });   // sem x/y — auto-pack
      continue;
    }
    const spec = buildItemSpec(it, idx);
    if (spec) out.push(spec);   // itens fora da DB já avisados e pulados
  }
  return out;
}

/** Resolve a aparência (Customization.Body/Feet) para a peça a gravar no outfit:
 *  prefere o SUIT (kit) que produz essa aparência — qualquer facção, inclusive Savage/AllTheClothes —
 *  porque o suit aparece no dropdown e libera/persiste. Sem suit → usa a aparência DIRETA (o
 *  OutfitBuilder aplica via BodyPart). Retorna o id da peça (suit ou aparência), ou null se ausente. */
function resolveOutfitPiece(appearanceId, byMap, faction) {
  if (!appearanceId) return null;
  const cands = byMap.get(appearanceId) ?? [];
  if (cands.length === 0) return appearanceId;   // sem suit → aparência direta (fiel ao profile)
  // prefere suit que cubra a facção pedida; senão o 1º (mesmo Savage — OutfitBuilder relaxado aplica).
  return (cands.find(s => s.sides.has(faction)) ?? cands[0]).id;
}

/**
 *  Outfit: copia QUALQUER roupa (upper/lower) do profile, inclusive Savage/AllTheClothes — resolve p/ suit
 *  quando existe (dropdown/persistência) ou aparência direta. Grava nas facções pedidas (default ambas).
 *  Requer o OutfitBuilder com o guard de Side relaxado (aplica roupa de facção diferente). Mescla
 *  não-destrutiva: lado sem peça resolvida preserva o existente.
 */
function extractOutfit(pmc, base, { faction }) {
  const out = JSON.parse(JSON.stringify(base.outfit ?? {}));   // preserva o existente
  const want = faction === 'both' ? ['usec', 'bear'] : [faction];
  const pref = faction === 'both' ? (pmc.Info?.Side ?? 'usec').toLowerCase() : faction;
  const upper = resolveOutfitPiece(pmc.Customization?.Body, SUIT_BY_BODY, pref);
  const lower = resolveOutfitPiece(pmc.Customization?.Feet, SUIT_BY_FEET, pref);
  for (const side of want) {
    if (upper) (out[side] ??= {}).upper = upper;
    if (lower) (out[side] ??= {}).lower = lower;
  }
  if (!upper) warn('outfit upper: Customization.Body ausente — preservado');
  if (!lower) warn('outfit lower: Customization.Feet ausente — preservado');
  return out;
}

// ── merge + escrita ─────────────────────────────────────────────────────────────
function mergeIntoDef(def, { outfit, equipped, stash }) {
  const skillsBefore = JSON.stringify(def.skills ?? null);
  const multsBefore = JSON.stringify(def.skillMultipliers ?? null);

  def.outfit = outfit;
  def.loadout = def.loadout ?? {};
  def.loadout.equipped = equipped;
  // stash null = --no-stash sem baseline → preserva o stash existente; senão grava (inclui o caso
  // --no-stash --with-baseline, em que stash = só o kit básico). Bug fix: antes gateava em opts.stash.
  if (stash != null) def.loadout.stash = stash;

  // Asserção anti-regressão: o balance (skills/skillMultipliers) NÃO pode mudar.
  if (JSON.stringify(def.skills ?? null) !== skillsBefore || JSON.stringify(def.skillMultipliers ?? null) !== multsBefore) {
    warn('ABORT: skills/skillMultipliers mudaram no merge — isso nunca deveria acontecer.');
    process.exit(1);
  }
  return def;
}

const serialize = def => JSON.stringify(def, null, 2) + '\n';

function writeWithBackup(file, text, { backup }) {
  if (backup && fs.existsSync(file)) fs.copyFileSync(file, `${file}.bak`);
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, text);
}

// ── main ─────────────────────────────────────────────────────────────────────
function main() {
  const args = parseArgs(process.argv);
  for (const t of args.exclude) EXCLUDE.add(t);
  loadDbs();   // VALID (itens vanilla + mods) + índice de suits
  const pmc = loadProfile(args.profileId);
  const { def, from } = loadClassDef(args.classFile);
  const idx = indexItems(pmc.Inventory.items);

  const outfit = args.outfit ? extractOutfit(pmc, def, args) : def.outfit;   // --no-outfit preserva
  const equipped = extractEquipped(pmc, idx);
  let stash = args.stash ? extractStash(pmc, idx, args) : null;
  if (args.withBaseline) stash = mergeBaseline(stash ?? []);   // garante o kit básico mesmo com --no-stash
  if (args.rubles > 0) {                                       // rublos-padrão (--rubles, default 100k)
    stash = stash ?? [];
    if (!stash.some(s => s.tpl === RUB_TPL)) { stash.push({ tpl: RUB_TPL, count: args.rubles }); warn(`+${args.rubles} RUB (padrão) no stash`); }
  }

  const merged = mergeIntoDef(def, { outfit, equipped, stash });
  const text = serialize(merged);

  warn(`base lida de: ${from}`);
  warn(`excluídos (--exclude): ${[...EXCLUDE].join(', ') || '—'}`);
  warn(`equipados: ${Object.keys(equipped).join(', ')}`);
  if (stash) warn(`stash: ${stash.length} item(ns)`);
  warn(`outfit: ${args.outfit ? `usec=${JSON.stringify(outfit?.usec ?? null)} bear=${JSON.stringify(outfit?.bear ?? null)}` : 'PRESERVADO (--no-outfit)'}`);

  if (args.dryRun) {
    process.stdout.write(text);
    warn('DRY-RUN — nada escrito.');
    return;
  }

  const repoFile = path.join(REPO_CLASSES, args.classFile);
  const installFile = path.join(INSTALL_CLASSES, args.classFile);
  writeWithBackup(repoFile, text, { backup: false });            // repo: git é o backup
  writeWithBackup(installFile, text, { backup: true });          // install: .bak antes de sobrescrever
  warn(`gravado: ${repoFile}`);
  warn(`gravado: ${installFile} (backup .bak criado)`);
}

main();

'use strict';

let META = null;
let activeKey = null;
let _tooltip = null;

const EQUIP_CELL = 28;
const MIN_EQUIP  = 64;

// Todas as skills vivas em SPT 4.0, em ordem fixa Ph→M→C→P.
// Exibidas sempre na mesma posição para facilitar comparação entre perfis.
const SKILL_MASTER = [
  { name: 'Endurance',       cat: 'Ph' },
  { name: 'Strength',        cat: 'Ph' },
  { name: 'Vitality',        cat: 'Ph' },
  { name: 'Health',          cat: 'Ph' },
  { name: 'StressResistance',cat: 'Ph' },
  { name: 'Metabolism',      cat: 'Ph' },
  { name: 'Immunity',        cat: 'Ph' },
  { name: 'Perception',      cat: 'M'  },
  { name: 'Intellect',       cat: 'M'  },
  { name: 'Attention',       cat: 'M'  },
  { name: 'Charisma',        cat: 'M'  },
  { name: 'Memory',          cat: 'M'  },
  { name: 'Pistol',          cat: 'C'  },
  { name: 'Revolver',        cat: 'C'  },
  { name: 'Assault',         cat: 'C'  },
  { name: 'Shotgun',         cat: 'C'  },
  { name: 'Sniper',          cat: 'C'  },
  { name: 'DMR',             cat: 'C'  },
  { name: 'Throwing',        cat: 'C'  },
  { name: 'Melee',           cat: 'C'  },
  { name: 'RecoilControl',   cat: 'C'  },
  { name: 'AimDrills',       cat: 'C'  },
  { name: 'TroubleShooting', cat: 'C'  },
  { name: 'Surgery',         cat: 'P'  },
  { name: 'CovertMovement',  cat: 'P'  },
  { name: 'Search',          cat: 'P'  },
  { name: 'MagDrills',       cat: 'P'  },
  { name: 'LightVests',      cat: 'P'  },
  { name: 'HeavyVests',      cat: 'P'  },
  { name: 'WeaponTreatment', cat: 'P'  },
  { name: 'Crafting',        cat: 'P'  },
  { name: 'HideoutManagement', cat: 'P' },
];

// ── Fetch ─────────────────────────────────────────────────────────────────────

initTooltip();

fetch(`../data/profiles-meta.json?v=${Date.now()}`)
  .then(r => {
    if (!r.ok) throw new Error(`HTTP ${r.status}`);
    return r.json();
  })
  .then(data => {
    META = data;
    renderSidebar();
    const firstKey = Object.keys(META.profiles)[0];
    if (firstKey) selectProfile(firstKey);
  })
  .catch(err => {
    document.getElementById('sidebar-list').innerHTML =
      `<div class="profile-list__loading" style="color:var(--color-danger)">Erro: ${escHtml(err.message)}</div>`;
  });

// ── Sidebar ───────────────────────────────────────────────────────────────────

function renderSidebar() {
  const list = document.getElementById('sidebar-list');
  list.innerHTML = '';
  for (const [key, p] of Object.entries(META.profiles)) {
    const el = document.createElement('div');
    el.className = 'profile-item';
    el.dataset.key = key;
    el.innerHTML = `
      <span class="profile-item__name">${escHtml(p.name)}</span>
      <span class="profile-item__cost">${p.totalCost.toFixed(2)}</span>
    `;
    el.addEventListener('click', () => selectProfile(key));
    list.appendChild(el);
  }
}

function selectProfile(key) {
  activeKey = key;
  document.querySelectorAll('.profile-item').forEach(el => {
    el.classList.toggle('is-active', el.dataset.key === key);
  });
  renderMain(META.profiles[key]);
}

// ── Main panel ────────────────────────────────────────────────────────────────

function renderMain(p) {
  const main = document.getElementById('main');
  main.innerHTML = `
    <div class="profile-main">
      ${renderHeader(p)}
      <div class="profile-body">
        <div class="profile-left">
          ${renderSkills(p)}
          ${renderHideout(p)}
        </div>
        <div class="profile-right">
          ${renderLoadout(p)}
        </div>
      </div>
    </div>
  `;
}

function renderHeader(p) {
  const rubStr = p.totalRub != null
    ? `₽ ${p.totalRub.toLocaleString('pt-BR')}`
    : '—';
  return `
    <section>
      <div class="profile-header__name">${escHtml(p.name)}</div>
      <div class="profile-header__desc">${escHtml(p.description)}</div>
      <div class="profile-header__meta">
        <div class="meta-badge">
          <span class="meta-badge__label">Custo ponderado</span>
          <span class="meta-badge__value meta-badge__value--accent">${p.totalCost.toFixed(2)} pts</span>
        </div>
        <div class="meta-badge">
          <span class="meta-badge__label">Loadout total</span>
          <span class="meta-badge__value">${rubStr}</span>
        </div>
        <div class="meta-badge">
          <span class="meta-badge__label">Base profile</span>
          <span class="meta-badge__value">Zero to Hero (#${p.baseProfile})</span>
        </div>
      </div>
    </section>
  `;
}

// ── Skills ────────────────────────────────────────────────────────────────────

const CAT_LABELS = { Ph: 'Físico (Ph)', M: 'Mental (M)', C: 'Combate (C)', P: 'Prático (P)' };

function renderSkills(p) {
  // Monta lookup nome→{level,cost} a partir de skillsByCategory
  const lookup = {};
  for (const skills of Object.values(p.skillsByCategory)) {
    for (const s of skills) lookup[s.name] = s;
  }

  let lastCat = null;
  const rows = SKILL_MASTER.map(({ name, cat }) => {
    const s = lookup[name];
    const level = s ? s.level : 0;
    const pct   = level > 0 ? (level / 10) * 100 : 0;
    const active = level > 0;

    // Separador de categoria
    let separator = '';
    if (cat !== lastCat) {
      separator = `<div class="skill-cat-sep skill-cat-sep--${cat}">${escHtml(CAT_LABELS[cat])}</div>`;
      lastCat = cat;
    }

    return separator + `<div class="skill-row ${active ? '' : 'skill-row--zero'}">
      <span class="skill-row__name">${escHtml(name)}</span>
      <div class="skill-bar"><div class="skill-fill skill-fill--${cat}" style="width:${pct}%"></div></div>
      <span class="skill-row__level ${active ? '' : 'skill-row__level--zero'}">${active ? level : '—'}</span>
    </div>`;
  }).join('');

  return `
    <section>
      <div class="section-title">Skills</div>
      <div class="skills-list">${rows}</div>
    </section>
  `;
}

// ── Hideout ───────────────────────────────────────────────────────────────────

function renderHideout(p) {
  const stashBadge = `
    <div class="hideout-station hideout-station--default">
      Stash
      <span class="hideout-station__level">L${p.hideout.stashLevel}</span>
      <span style="font-size:10px;color:var(--fg-dim)">(padrão)</span>
    </div>
  `;
  const thematic = Object.entries(p.hideout.thematic).map(([name, level]) => `
    <div class="hideout-station hideout-station--thematic">
      ${escHtml(name)}
      <span class="hideout-station__level">L${level}</span>
    </div>
  `).join('');

  return `
    <section>
      <div class="section-title">Hideout inicial</div>
      <div class="hideout-grid">${stashBadge}${thematic}</div>
    </section>
  `;
}

// ── Loadout ───────────────────────────────────────────────────────────────────

function classifySlot(item) {
  const types = item.types || [];
  const cat   = item.categoryName || '';
  if (types.includes('helmet'))   return 'headwear';
  if (types.includes('rig'))      return 'rig';      // antes de armor — BlackRock tem ["armor","rig"]
  if (types.includes('armor'))    return 'armor';
  if (types.includes('backpack')) return 'backpack';
  if (types.includes('gun') && cat === 'Pistols') return 'holster';
  if (types.includes('gun'))      return 'onBack';
  if (cat === 'Melee weapons')    return 'sheath';
  return 'stash';
}

function tipJson(item) {
  return escAttr(JSON.stringify({
    name: item.name,
    shortName: item.shortName,
    iconUrl: item.iconUrl,
    dims: item.dims,
    weight: item.weight ?? null,
    priceRub: item.priceRub ?? null,
    categoryName: item.categoryName,
    qty: item.qty,
  }));
}

function equipItemHtml(item) {
  const w = Math.max(item.dims.width  * EQUIP_CELL, MIN_EQUIP);
  const h = Math.max(item.dims.height * EQUIP_CELL, MIN_EQUIP);
  const img = item.iconUrl
    ? `<img src="${escAttr(item.iconUrl)}" loading="lazy" onerror="_imgErr(this)">`
    : `<div class="no-img">—</div>`;
  return `<div class="equip-item" style="width:${w}px;height:${h}px" data-tip="${tipJson(item)}">
    ${img}
  </div>`;
}

function renderLoadout(p) {
  const l = p.loadout;

  // ── Primary: classify into equipment slots ────────────────────────────
  const slots = {};
  const primaryRest = [];
  for (const item of l.primary) {
    const s = classifySlot(item);
    if (s === 'stash' || slots[s] !== undefined) primaryRest.push(item);
    else slots[s] = item;
  }

  // ── Gear rows matching Tarkov layout ──────────────────────────────────
  const ROW1 = ['headwear', 'armor', 'rig', 'backpack'];
  const ROW2 = ['onBack', 'holster', 'sheath'];
  const SLOT_LABELS = {
    headwear: 'HEADWEAR', armor: 'ARMOR', rig: 'RIG', backpack: 'BACKPACK',
    onBack: 'ON BACK', holster: 'HOLSTER', sheath: 'SHEATH',
  };

  function renderSlot(key) {
    const item = slots[key];
    const nameLabel = item
      ? `<span class="gear-slot__item-name">${escHtml(item.shortName || item.name || '')}</span>`
      : '';
    return `<div class="gear-slot gear-slot--${key}">
      <span class="gear-slot__label">${SLOT_LABELS[key]}</span>
      ${item ? equipItemHtml(item) : '<div class="equip-empty">—</div>'}
      ${nameLabel}
    </div>`;
  }

  const gearHtml = `
    <div class="gear-row">${ROW1.map(renderSlot).join('')}</div>
    <div class="gear-row">${ROW2.map(renderSlot).join('')}</div>
  `;

  // ── Stash: baseline + tema + primaryRest + backup ─────────────────────
  const stashAll = [...l.baseline, ...l.tema, ...primaryRest, ...l.backup];

  const groupMap = new Map();
  for (const item of stashAll) {
    const cat = item.categoryName || 'Outros';
    if (!groupMap.has(cat)) groupMap.set(cat, []);
    groupMap.get(cat).push(item);
  }

  const stashGroups = [...groupMap.entries()].map(([cat, items]) => {
    const itemsHtml = items.map(item => {
      const w = Math.max(item.dims.width  * EQUIP_CELL, MIN_EQUIP);
      const h = Math.max(item.dims.height * EQUIP_CELL, MIN_EQUIP);
      const img = item.iconUrl
        ? `<img src="${escAttr(item.iconUrl)}" loading="lazy" onerror="_imgErr(this)">`
        : `<div class="no-img">—</div>`;
      const qty = item.qty > 1 ? `<span class="item-cell__qty">${item.qty}</span>` : '';
      return `<div class="stash-item" style="width:${w}px;height:${h}px" data-tip="${tipJson(item)}">
        ${img}${qty}
      </div>`;
    }).join('');
    return `<div class="stash-group">
      <div class="stash-group__label">${escHtml(cat)}</div>
      <div class="stash-items">${itemsHtml}</div>
    </div>`;
  }).join('');

  return `
    <div class="loadout-screen">
      <div class="gear-panel">
        <div class="gear-panel__title">EQUIPADO</div>
        <div class="gear-slots">${gearHtml}</div>
      </div>
      <div class="stash-panel">
        <div class="stash-panel__title">STASH / INVENTÁRIO</div>
        ${stashGroups}
      </div>
    </div>
  `;
}

// ── Tooltip ───────────────────────────────────────────────────────────────────

function initTooltip() {
  _tooltip = document.createElement('div');
  _tooltip.className = 'item-tooltip';
  _tooltip.style.display = 'none';
  document.body.appendChild(_tooltip);

  let _activeTarget = null;

  document.addEventListener('mouseover', e => {
    const target = e.target.closest('[data-tip]');
    if (!target) return;
    if (target === _activeTarget) return;
    _activeTarget = target;
    try { populateTooltip(JSON.parse(target.dataset.tip)); } catch (_) { return; }
    _tooltip.style.display = 'block';
    positionTooltip(e.clientX, e.clientY);
  });

  document.addEventListener('mousemove', e => {
    if (_tooltip.style.display === 'none') return;
    positionTooltip(e.clientX, e.clientY);
  });

  document.addEventListener('mouseout', e => {
    const target = e.target.closest('[data-tip]');
    if (!target) return;
    if (target.contains(e.relatedTarget)) return;
    _activeTarget = null;
    _tooltip.style.display = 'none';
  });
}

function positionTooltip(cx, cy) {
  const tw = _tooltip.offsetWidth || 268;
  const th = _tooltip.offsetHeight || 200;
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  let x = cx + 16;
  let y = cy + 16;
  if (x + tw > vw - 8) x = cx - tw - 16;
  if (y + th > vh - 8) y = cy - th - 16;
  _tooltip.style.left = x + 'px';
  _tooltip.style.top  = y + 'px';
}

function populateTooltip(d) {
  const slots    = d.dims.width * d.dims.height;
  const priceStr = d.priceRub != null
    ? `₽ ${d.priceRub.toLocaleString('en-US')}`
    : '—';
  const perSlot  = (d.priceRub != null && slots > 0)
    ? `₽ ${Math.round(d.priceRub / slots).toLocaleString('en-US')}`
    : '—';
  const wStr     = d.weight != null ? `${d.weight.toFixed(2)} kg` : '—';
  const imgHtml  = d.iconUrl
    ? `<img src="${escAttr(d.iconUrl)}" class="item-tooltip__img" onerror="_imgErr(this)">`
    : `<div class="no-img">—</div>`;
  const qtyRow   = d.qty > 1
    ? `<div class="item-tooltip__stat"><span class="item-tooltip__lbl">Qty</span><span class="item-tooltip__val">× ${d.qty}</span></div>`
    : '';

  _tooltip.innerHTML = `
    <div class="item-tooltip__header">
      <div class="item-tooltip__img-wrap">${imgHtml}</div>
      <div class="item-tooltip__title">
        <div class="item-tooltip__name">${escHtml(d.name || d.shortName || '—')}</div>
        ${d.categoryName ? `<div class="item-tooltip__cat">${escHtml(d.categoryName)}</div>` : ''}
      </div>
    </div>
    <div class="item-tooltip__stats">
      <div class="item-tooltip__stat">
        <span class="item-tooltip__lbl">Size</span>
        <span class="item-tooltip__val">${d.dims.width} × ${d.dims.height} &nbsp;(${slots} slot${slots !== 1 ? 's' : ''})</span>
      </div>
      <div class="item-tooltip__stat">
        <span class="item-tooltip__lbl">Weight</span>
        <span class="item-tooltip__val">${wStr}</span>
      </div>
      <div class="item-tooltip__stat">
        <span class="item-tooltip__lbl">Flea price</span>
        <span class="item-tooltip__val">${priceStr}</span>
      </div>
      <div class="item-tooltip__stat">
        <span class="item-tooltip__lbl">Per slot</span>
        <span class="item-tooltip__val">${perSlot}</span>
      </div>
      ${qtyRow}
    </div>
  `;
}

// ── Utils ─────────────────────────────────────────────────────────────────────

function _imgErr(img) {
  const noImg = document.createElement('div');
  noImg.className = 'no-img';
  noImg.textContent = '—';
  img.replaceWith(noImg);
}

function escHtml(s) {
  if (s == null) return '';
  return String(s)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function escAttr(s) {
  if (s == null) return '';
  return String(s).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

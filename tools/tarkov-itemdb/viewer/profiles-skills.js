'use strict';

const CAT_LABELS = {
  Ph: 'Físico (Ph)',
  M:  'Mental (M)',
  C:  'Combate (C)',
  P:  'Prático (P)',
};

fetch(`../data/profiles-meta.json?v=${Date.now()}`)
  .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
  .then(data => {
    document.getElementById('main').innerHTML = renderMatrix(data);
  })
  .catch(err => {
    document.getElementById('main').innerHTML =
      `<div class="skills-loading" style="color:var(--color-danger)">Erro: ${escHtml(err.message)}</div>`;
  });

function renderMatrix(data) {
  const profiles = Object.values(data.profiles);

  // Collect all unique skills per category, preserving first-seen order
  const cats = { Ph: [], M: [], C: [], P: [] };
  const seen = { Ph: new Set(), M: new Set(), C: new Set(), P: new Set() };
  for (const p of profiles) {
    for (const [cat, skills] of Object.entries(p.skillsByCategory)) {
      if (!cats[cat]) continue;
      for (const s of skills) {
        if (!seen[cat].has(s.name)) { seen[cat].add(s.name); cats[cat].push(s.name); }
      }
    }
  }

  // Build lookup: profileKey -> skillName -> level
  const lookup = {};
  for (const p of profiles) {
    lookup[p.fileName] = {};
    for (const skills of Object.values(p.skillsByCategory)) {
      for (const s of skills) lookup[p.fileName][s.name] = s.level;
    }
  }

  // Profile header row (rotated labels)
  const headerCells = profiles.map(p => {
    const shortName = p.name.replace('Operador ', 'Op. ').replace('Gerente de Operações', 'Gerente');
    return `<th class="skill-col-header"><div class="skill-col-header__inner">${escHtml(shortName)}</div></th>`;
  }).join('');

  // Category + skill rows
  let rows = '';
  for (const [cat, skills] of Object.entries(cats)) {
    if (skills.length === 0) continue;
    rows += `
      <tr class="cat-header-row cat-header-row--${escHtml(cat)}">
        <td class="cat-label">${escHtml(CAT_LABELS[cat] || cat)}</td>
        ${profiles.map(() => '<td class="cat-spacer"></td>').join('')}
      </tr>
    `;
    for (const skill of skills) {
      const cells = profiles.map(p => {
        const lvl = lookup[p.fileName]?.[skill] ?? 0;
        if (lvl === 0) return `<td class="skill-cell skill-cell--empty"></td>`;
        const tier = lvl <= 3 ? 'low' : lvl <= 6 ? 'mid' : 'high';
        return `<td class="skill-cell skill-cell--${tier}" title="${escAttr(p.name)}: ${skill} ${lvl}">
          <span class="skill-cell__val">${lvl}</span>
        </td>`;
      }).join('');
      rows += `
        <tr class="skill-row-tr">
          <td class="skill-name">${escHtml(skill)}</td>
          ${cells}
        </tr>
      `;
    }
  }

  return `
    <div class="matrix-wrap">
      <table class="skill-matrix">
        <thead>
          <tr>
            <th class="skill-name-header"></th>
            ${headerCells}
          </tr>
        </thead>
        <tbody>
          ${rows}
        </tbody>
      </table>
    </div>
  `;
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

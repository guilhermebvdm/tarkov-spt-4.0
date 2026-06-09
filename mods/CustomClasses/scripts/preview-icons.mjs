// preview-icons.mjs — authoring-time review helper (NOT part of the mod runtime).
//
// Builds a contact sheet of the generated class PNGs, each tinted with its class
// `nameColor` over a dark EFT-menu-like background, so the white silhouettes can
// actually be reviewed (on transparent they'd be invisible). Output:
//   scripts/preview/contact-sheet.png
// Usage:  node preview-icons.mjs   (after build-icons.mjs)

import { mkdir } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import sharp from "sharp";

const here = dirname(fileURLToPath(import.meta.url));
const ICONS = join(here, "..", "modded", "Client", "icons");
const OUT = join(here, "preview");

// order, file, color, label (matches CLASS_VISUAL)
// Tarkov-dark palette: desaturated, earthy/military (bronze, rust, olive, khaki, amber)
// + muted cool tones (steel, slate, gunmetal) for recon/stealth. Inspired by EFT edition
// icons (Unheard/EOD amber-gold). Tinted over a dark menu background.
const CLASSES = [
  ["armeiro", "#a8824e", "Armeiro"],
  ["batedor", "#5f7f93", "Batedor"],
  ["cacador", "#c2973f", "Cacador"],
  ["fuzileiro", "#b0573a", "Fuzileiro"],
  ["gerenteDeOperacoes", "#4f8a80", "Gerente de Operacoes"],
  ["medicoDeCombate", "#6f9455", "Medico de Combate"],
  ["operadorFurtivo", "#7d7392", "Operador Furtivo"],
  ["operadorTatico", "#7a818c", "Operador Tatico"],
  ["saqueador", "#c4ad45", "Saqueador"],
  ["sobrevivencialista", "#97934e", "Sobrevivencialista"],
  ["peladao", "#c28a60", "Peladao"],
];

const COLS = 3;
const CELL_W = 280;
const CELL_H = 220;
const ICON = 120;
const BG = { r: 18, g: 22, b: 25, alpha: 1 }; // ~#121619 dark menu

const hexToRgb = (hex) => {
  const n = parseInt(hex.slice(1), 16);
  return { r: (n >> 16) & 255, g: (n >> 8) & 255, b: n & 255 };
};

const run = async () => {
  await mkdir(OUT, { recursive: true });
  const rows = Math.ceil(CLASSES.length / COLS);
  const W = COLS * CELL_W;
  const H = rows * CELL_H;

  const composites = [];
  const labels = [];

  for (let i = 0; i < CLASSES.length; i++) {
    const [file, color, label] = CLASSES[i];
    const col = i % COLS;
    const row = Math.floor(i / COLS);
    const cx = col * CELL_W + CELL_W / 2;
    const iconTop = row * CELL_H + 30;

    const tinted = await sharp(join(ICONS, `${file}.png`))
      .resize(ICON, ICON, { fit: "contain", background: { r: 0, g: 0, b: 0, alpha: 0 } })
      .tint(hexToRgb(color))
      .png()
      .toBuffer();

    composites.push({ input: tinted, left: Math.round(cx - ICON / 2), top: iconTop });
    labels.push(
      `<text x="${cx}" y="${iconTop + ICON + 34}" fill="${color}" font-family="Segoe UI, sans-serif" ` +
        `font-size="20" font-weight="600" text-anchor="middle">${label}</text>`,
    );
  }

  const overlay = Buffer.from(
    `<svg width="${W}" height="${H}" xmlns="http://www.w3.org/2000/svg">${labels.join("")}</svg>`,
  );
  composites.push({ input: overlay, left: 0, top: 0 });

  const out = join(OUT, "contact-sheet.png");
  await sharp({ create: { width: W, height: H, channels: 4, background: BG } })
    .composite(composites)
    .png()
    .toFile(out);

  console.log(`✓ ${out} (${W}x${H})`);
};

run().catch((e) => {
  console.error(e);
  process.exit(1);
});

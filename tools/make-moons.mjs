// Writes the six theme moons the Tsukimichi widget shows on Umbra's bar (assets/moons/<theme>.svg), one per
// Tsukimichi.GetTheme key. The faces are Tsukimichi's own (docs/design/v8/mock-src/v722.js, FACE22 and face22, the
// "full" decoration level), copied here from the Tsukimichi repository (MIT, same author) so this repository builds alone.
//
//   node tools/make-moons.mjs
//   resvg -w 64 -h 64 assets/moons/<theme>.svg assets/moons/<theme>.png   (for each theme)
//
// or run tools/make-moons.ps1, which does both.

import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const FACE = {
  "medallion": { rim: ["#E6CF98", "#9A7E4A", "#7C6236", "#5C4724"], well: ["#2C3D80", "#16204A", "#0C1230"], moon: ["#F7F4EA", "#D9D2BE"], earth: "#3C4C8A",
    extra: '<circle cx="18" cy="44" r=".7" fill="#DCE5FF" opacity=".55"/><circle cx="46" cy="47" r=".6" fill="#DCE5FF" opacity=".45"/><circle cx="49" cy="20" r=".55" fill="#FFE2A8" opacity=".5"/>' },
  "classic": { rim: null, well: ["#24305E", "#1C2752", "#141C40"], moon: ["#F4E9C6", "#E2D3A6"], earth: "#2C3866", extra: "" },
  "ishgard-glass": { rim: ["#B8C0D0", "#8C95B0", "#5A6278", "#323950"], well: ["#4A68AE", "#2E4682", "#1E3064"], moon: ["#F6F1E2", "#E3DAC3"], earth: "#3A5292",
    extra: '<path d="M5 41c9-3 18-3 27 0s18 3 27 0" fill="none" stroke="#323950" stroke-width="1.3" opacity=".9"/><path d="M5 41.6c9-3 18-3 27 0s18 3 27 0V60H5z" fill="#1B2A58" opacity=".55"/><path d="M14 14l5 5" stroke="#FFFFFF" stroke-opacity=".22" stroke-width="2" stroke-linecap="round"/>', moonStroke: "#323950" },
  "aether-crystal": { rim: ["#E2E8F4", "#A9B5D0", "#7B8AAF", "#5E6E97"], well: ["#2C3C62", "#18233E", "#0E1528"], moon: ["#EEF8FF", "#C9E3F2"], earth: "#2A3E66",
    extra: '<path d="M10 40L32 54 54 38M32 54V60M20 12l12 8 12-8" fill="none" stroke="#9BE6FF" stroke-opacity=".16" stroke-width=".8"/>', facet: true },
  "astrologian-orrery": { rim: ["#EAD3A0", "#B8924E", "#7C6034", "#4E3B1E"], well: ["#2A3778", "#1A245A", "#111A40"], moon: ["#F8EBCF", "#E2CFA6"], earth: "#2E3C7A",
    extra: '<path d="M38 48l6-3 5 4" fill="none" stroke="#EAD3A0" stroke-opacity=".35" stroke-width=".6"/><circle cx="38" cy="48" r=".9" fill="#F4E4BC" opacity=".8"/><circle cx="44" cy="45" r=".7" fill="#F4E4BC" opacity=".7"/><circle cx="49" cy="49" r="1" fill="#F4E4BC" opacity=".85"/>', inner: "#7C6034" },
  "sumi-to-kinpaku": { rim: ["#4A444C", "#2A262E", "#141216", "#060508"], well: ["#16131A", "#0E0C10", "#060508"], moon: ["#F3EAD3", "#E2D7BB"], earth: "#2A2832",
    extra: '<path d="M8 22L22 8" stroke="#FFFFFF" stroke-opacity=".07" stroke-width="9" stroke-linecap="round"/>', kiri: true },
};

function face(th) {
  const id = "f", F = FACE[th];
  let d = "", g = "";
  d += `<radialGradient id="${id}w" cx=".38" cy=".32" r=".75"><stop offset="0" stop-color="${F.well[0]}"/><stop offset=".6" stop-color="${F.well[1]}"/><stop offset="1" stop-color="${F.well[2]}"/></radialGradient>`;
  d += `<linearGradient id="${id}mo" x1="0" y1="0" x2="1" y2="1"><stop offset=".2" stop-color="${F.moon[0]}"/><stop offset="1" stop-color="${F.moon[1]}"/></linearGradient>`;
  d += `<mask id="${id}m"><circle cx="32" cy="32" r="15" fill="#fff"/><circle cx="37.6" cy="36.4" r="14.2" fill="#000"/></mask>`;
  d += `<clipPath id="${id}c"><circle cx="32" cy="32" r="27"/></clipPath>`;
  if (F.rim) d += `<linearGradient id="${id}r" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="${F.rim[0]}"/><stop offset=".45" stop-color="${F.rim[1]}"/><stop offset=".8" stop-color="${F.rim[2]}"/><stop offset="1" stop-color="${F.rim[3]}"/></linearGradient>`;
  if (!F.rim) {
    g += `<circle cx="32" cy="32" r="29.5" fill="url(#${id}w)"/><circle cx="32" cy="32" r="29.5" fill="none" stroke="#DDE3F0" stroke-opacity=".62" stroke-width="1.3"/>`;
  } else {
    g += `<circle cx="32" cy="32" r="31" fill="url(#${id}r)"/><circle cx="32" cy="32" r="31.4" fill="none" stroke="#080B16" stroke-opacity=".6" stroke-width=".8"/>`;
    if (F.kiri) g += '<circle cx="32" cy="32" r="29.2" fill="none" stroke="#DEB862" stroke-width=".9"/><circle cx="32" cy="32" r="31" fill="none" stroke="#F2ECDD" stroke-opacity=".34" stroke-width=".5"/>';
    if (F.inner) g += `<circle cx="32" cy="32" r="28.4" fill="none" stroke="${F.inner}" stroke-width=".9"/>`;
    if (F.facet) g += [45, 135, 225, 315].map((a) => {
      const r = 30.2, x = 32 + r * Math.cos(a * Math.PI / 180), y = 32 + r * Math.sin(a * Math.PI / 180);
      return `<rect x="${x - 1.6}" y="${y - 1.6}" width="3.2" height="3.2" transform="rotate(45 ${x} ${y})" fill="${a === 225 ? "#F6F9FE" : "#B9C4DC"}" stroke="#5E6E97" stroke-width=".4"/>`;
    }).join("");
    g += '<circle cx="32" cy="32" r="27.7" fill="#080B16" fill-opacity=".75"/>';
    g += `<circle cx="32" cy="32" r="27" fill="url(#${id}w)"/>`;
    g += `<path d="M8.6 20.5A26 26 0 0 1 20.5 8.6" fill="none" stroke="#FFFFFF" stroke-opacity="${F.kiri ? ".25" : ".38"}" stroke-width="1.2" stroke-linecap="round" transform="translate(-2.2 -2.2) scale(1.07)"/>`;
  }
  g += `<g clip-path="url(#${id}c)">${F.extra}</g>`;
  g += `<circle cx="32" cy="32" r="15" fill="${F.earth}" fill-opacity=".38"/>`;
  g += `<circle cx="32" cy="32" r="15" fill="url(#${id}mo)" mask="url(#${id}m)"/>`;
  if (F.moonStroke) g += `<g mask="url(#${id}m)"><circle cx="32" cy="32" r="14.6" fill="none" stroke="${F.moonStroke}" stroke-width="1"/></g>`;
  if (F.facet) g += `<path d="M21 23.5L25.5 31 22 40" fill="none" stroke="#9BE6FF" stroke-opacity=".55" stroke-width=".6" mask="url(#${id}m)"/>`;
  return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64" width="64" height="64"><defs>${d}</defs>${g}</svg>\n`;
}

const out = join(dirname(fileURLToPath(import.meta.url)), "..", "assets", "moons");
mkdirSync(out, { recursive: true });
for (const th of Object.keys(FACE)) {
  writeFileSync(join(out, `${th}.svg`), face(th));
  console.log(`${th}.svg`);
}

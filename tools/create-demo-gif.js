const fs = require("fs");
const path = require("path");
const zlib = require("zlib");

const width = 640;
const height = 360;
const frameCount = 36;
const delayCs = 8;

const root = path.resolve(__dirname, "..");
const outDir = path.join(root, "assets");
const outFile = path.join(outDir, "demo.gif");
const imageDir = path.join(root, "dist", "images");

const palette = buildPalette();
const colorCache = new Map();

const glyphs = {
  " ": ["00000", "00000", "00000", "00000", "00000", "00000", "00000"],
  ".": ["00000", "00000", "00000", "00000", "00000", "01100", "01100"],
  "%": ["11001", "11010", "00100", "01000", "10110", "00110", "00000"],
  "/": ["00001", "00010", "00100", "01000", "10000", "00000", "00000"],
  ":": ["00000", "01100", "01100", "00000", "01100", "01100", "00000"],
  "-": ["00000", "00000", "00000", "11111", "00000", "00000", "00000"],
  "+": ["00000", "00100", "00100", "11111", "00100", "00100", "00000"],
  "0": ["01110", "10001", "10011", "10101", "11001", "10001", "01110"],
  "1": ["00100", "01100", "00100", "00100", "00100", "00100", "01110"],
  "2": ["01110", "10001", "00001", "00010", "00100", "01000", "11111"],
  "3": ["11110", "00001", "00001", "01110", "00001", "00001", "11110"],
  "4": ["00010", "00110", "01010", "10010", "11111", "00010", "00010"],
  "5": ["11111", "10000", "10000", "11110", "00001", "00001", "11110"],
  "6": ["00110", "01000", "10000", "11110", "10001", "10001", "01110"],
  "7": ["11111", "00001", "00010", "00100", "01000", "01000", "01000"],
  "8": ["01110", "10001", "10001", "01110", "10001", "10001", "01110"],
  "9": ["01110", "10001", "10001", "01111", "00001", "00010", "11100"],
  A: ["01110", "10001", "10001", "11111", "10001", "10001", "10001"],
  B: ["11110", "10001", "10001", "11110", "10001", "10001", "11110"],
  C: ["01111", "10000", "10000", "10000", "10000", "10000", "01111"],
  D: ["11110", "10001", "10001", "10001", "10001", "10001", "11110"],
  E: ["11111", "10000", "10000", "11110", "10000", "10000", "11111"],
  F: ["11111", "10000", "10000", "11110", "10000", "10000", "10000"],
  G: ["01111", "10000", "10000", "10011", "10001", "10001", "01111"],
  H: ["10001", "10001", "10001", "11111", "10001", "10001", "10001"],
  I: ["11111", "00100", "00100", "00100", "00100", "00100", "11111"],
  J: ["00111", "00010", "00010", "00010", "00010", "10010", "01100"],
  K: ["10001", "10010", "10100", "11000", "10100", "10010", "10001"],
  L: ["10000", "10000", "10000", "10000", "10000", "10000", "11111"],
  M: ["10001", "11011", "10101", "10101", "10001", "10001", "10001"],
  N: ["10001", "11001", "10101", "10011", "10001", "10001", "10001"],
  O: ["01110", "10001", "10001", "10001", "10001", "10001", "01110"],
  P: ["11110", "10001", "10001", "11110", "10000", "10000", "10000"],
  Q: ["01110", "10001", "10001", "10001", "10101", "10010", "01101"],
  R: ["11110", "10001", "10001", "11110", "10100", "10010", "10001"],
  S: ["01111", "10000", "10000", "01110", "00001", "00001", "11110"],
  T: ["11111", "00100", "00100", "00100", "00100", "00100", "00100"],
  U: ["10001", "10001", "10001", "10001", "10001", "10001", "01110"],
  V: ["10001", "10001", "10001", "10001", "10001", "01010", "00100"],
  W: ["10001", "10001", "10001", "10101", "10101", "10101", "01010"],
  X: ["10001", "10001", "01010", "00100", "01010", "10001", "10001"],
  Y: ["10001", "10001", "01010", "00100", "00100", "00100", "00100"],
  Z: ["11111", "00001", "00010", "00100", "01000", "10000", "11111"],
};

const poses = {
  idle: readPng(path.join(imageDir, "idle.png")),
  point: readPng(path.join(imageDir, "point.png")),
  cheer: readPng(path.join(imageDir, "cheer.png")),
};

function buildPalette() {
  const p = [[246, 248, 252]];
  const levels = [0, 51, 102, 153, 204, 255];
  for (const r of levels) {
    for (const g of levels) {
      for (const b of levels) p.push([r, g, b]);
    }
  }
  for (let i = 0; i < 24; i++) {
    const v = Math.round((i / 23) * 255);
    p.push([v, v, v]);
  }
  [
    [15, 23, 42],
    [30, 41, 59],
    [71, 85, 105],
    [100, 116, 139],
    [203, 213, 225],
    [226, 232, 240],
    [236, 242, 249],
    [24, 128, 91],
    [38, 78, 140],
    [30, 64, 175],
    [16, 185, 129],
    [248, 199, 79],
    [255, 255, 255],
    [20, 83, 45],
    [220, 38, 38],
  ].forEach((c) => p.push(c));
  return p.slice(0, 256);
}

function readPng(file) {
  const data = fs.readFileSync(file);
  const signature = "89504e470d0a1a0a";
  if (data.slice(0, 8).toString("hex") !== signature) {
    throw new Error(`Not a PNG: ${file}`);
  }

  let offset = 8;
  let pngWidth = 0;
  let pngHeight = 0;
  let bitDepth = 0;
  let colorType = 0;
  const idat = [];

  while (offset < data.length) {
    const length = data.readUInt32BE(offset);
    const type = data.slice(offset + 4, offset + 8).toString("ascii");
    const chunk = data.slice(offset + 8, offset + 8 + length);
    offset += 12 + length;

    if (type === "IHDR") {
      pngWidth = chunk.readUInt32BE(0);
      pngHeight = chunk.readUInt32BE(4);
      bitDepth = chunk[8];
      colorType = chunk[9];
    } else if (type === "IDAT") {
      idat.push(chunk);
    } else if (type === "IEND") {
      break;
    }
  }

  if (bitDepth !== 8 || (colorType !== 6 && colorType !== 2)) {
    throw new Error(`Unsupported PNG format in ${file}`);
  }

  const channels = colorType === 6 ? 4 : 3;
  const stride = pngWidth * channels;
  const raw = zlib.inflateSync(Buffer.concat(idat));
  const pixels = new Uint8ClampedArray(pngWidth * pngHeight * 4);
  let rawOffset = 0;
  let previous = Buffer.alloc(stride);

  for (let y = 0; y < pngHeight; y++) {
    const filter = raw[rawOffset++];
    const row = Buffer.from(raw.slice(rawOffset, rawOffset + stride));
    rawOffset += stride;
    unfilter(row, previous, channels, filter);

    for (let x = 0; x < pngWidth; x++) {
      const src = x * channels;
      const dst = (y * pngWidth + x) * 4;
      pixels[dst] = row[src];
      pixels[dst + 1] = row[src + 1];
      pixels[dst + 2] = row[src + 2];
      pixels[dst + 3] = channels === 4 ? row[src + 3] : 255;
    }

    previous = row;
  }

  return { width: pngWidth, height: pngHeight, pixels };
}

function unfilter(row, previous, bpp, filter) {
  for (let i = 0; i < row.length; i++) {
    const left = i >= bpp ? row[i - bpp] : 0;
    const up = previous[i] || 0;
    const upLeft = i >= bpp ? previous[i - bpp] || 0 : 0;
    if (filter === 1) row[i] = (row[i] + left) & 255;
    else if (filter === 2) row[i] = (row[i] + up) & 255;
    else if (filter === 3) row[i] = (row[i] + Math.floor((left + up) / 2)) & 255;
    else if (filter === 4) row[i] = (row[i] + paeth(left, up, upLeft)) & 255;
  }
}

function paeth(a, b, c) {
  const p = a + b - c;
  const pa = Math.abs(p - a);
  const pb = Math.abs(p - b);
  const pc = Math.abs(p - c);
  if (pa <= pb && pa <= pc) return a;
  if (pb <= pc) return b;
  return c;
}

function makeRgbCanvas() {
  const canvas = new Uint8ClampedArray(width * height * 3);
  fillRect(canvas, 0, 0, width, height, [246, 248, 252]);
  return canvas;
}

function setPixel(canvas, x, y, color) {
  if (x < 0 || y < 0 || x >= width || y >= height) return;
  const i = (y * width + x) * 3;
  canvas[i] = color[0];
  canvas[i + 1] = color[1];
  canvas[i + 2] = color[2];
}

function fillRect(canvas, x, y, w, h, color) {
  const x0 = Math.max(0, Math.floor(x));
  const y0 = Math.max(0, Math.floor(y));
  const x1 = Math.min(width, Math.ceil(x + w));
  const y1 = Math.min(height, Math.ceil(y + h));
  for (let yy = y0; yy < y1; yy++) {
    for (let xx = x0; xx < x1; xx++) setPixel(canvas, xx, yy, color);
  }
}

function fillCircle(canvas, cx, cy, radius, color) {
  const r2 = radius * radius;
  for (let y = Math.floor(cy - radius); y <= Math.ceil(cy + radius); y++) {
    for (let x = Math.floor(cx - radius); x <= Math.ceil(cx + radius); x++) {
      const dx = x - cx;
      const dy = y - cy;
      if (dx * dx + dy * dy <= r2) setPixel(canvas, x, y, color);
    }
  }
}

function fillRoundRect(canvas, x, y, w, h, r, color) {
  fillRect(canvas, x + r, y, w - r * 2, h, color);
  fillRect(canvas, x, y + r, w, h - r * 2, color);
  fillCircle(canvas, x + r, y + r, r, color);
  fillCircle(canvas, x + w - r - 1, y + r, r, color);
  fillCircle(canvas, x + r, y + h - r - 1, r, color);
  fillCircle(canvas, x + w - r - 1, y + h - r - 1, r, color);
}

function strokeRect(canvas, x, y, w, h, color, t = 1) {
  fillRect(canvas, x, y, w, t, color);
  fillRect(canvas, x, y + h - t, w, t, color);
  fillRect(canvas, x, y, t, h, color);
  fillRect(canvas, x + w - t, y, t, h, color);
}

function drawText(canvas, text, x, y, color, scale = 2) {
  let cx = x;
  for (const char of text.toUpperCase()) {
    const glyph = glyphs[char] || glyphs[" "];
    for (let row = 0; row < glyph.length; row++) {
      for (let col = 0; col < glyph[row].length; col++) {
        if (glyph[row][col] === "1") fillRect(canvas, cx + col * scale, y + row * scale, scale, scale, color);
      }
    }
    cx += 6 * scale;
  }
}

function textWidth(text, scale) {
  return text.length * 6 * scale - scale;
}

function drawCenteredText(canvas, text, cx, cy, color, scale) {
  drawText(canvas, text, Math.round(cx - textWidth(text, scale) / 2), Math.round(cy - 7 * scale / 2), color, scale);
}

function drawHangulHan(canvas, cx, cy, color, scale) {
  const x = Math.round(cx - 8 * scale);
  const y = Math.round(cy - 8 * scale);
  fillRect(canvas, x + 1 * scale, y + 0 * scale, 7 * scale, 1 * scale, color);
  fillRect(canvas, x + 4 * scale, y + 0 * scale, 1 * scale, 4 * scale, color);
  fillRect(canvas, x + 1 * scale, y + 5 * scale, 7 * scale, 1 * scale, color);
  fillRect(canvas, x + 3 * scale, y + 8 * scale, 4 * scale, 2 * scale, color);
  fillRect(canvas, x + 11 * scale, y + 0 * scale, 1 * scale, 11 * scale, color);
  fillRect(canvas, x + 11 * scale, y + 5 * scale, 4 * scale, 1 * scale, color);
  fillRect(canvas, x + 2 * scale, y + 14 * scale, 12 * scale, 1 * scale, color);
  fillRect(canvas, x + 2 * scale, y + 11 * scale, 1 * scale, 4 * scale, color);
}

function drawImage(canvas, image, x, y, w, h) {
  for (let yy = 0; yy < h; yy++) {
    const sy = Math.min(image.height - 1, Math.floor((yy / h) * image.height));
    for (let xx = 0; xx < w; xx++) {
      const sx = Math.min(image.width - 1, Math.floor((xx / w) * image.width));
      const src = (sy * image.width + sx) * 4;
      const alpha = image.pixels[src + 3] / 255;
      if (alpha <= 0.01) continue;

      const dx = x + xx;
      const dy = y + yy;
      if (dx < 0 || dy < 0 || dx >= width || dy >= height) continue;

      const dst = (dy * width + dx) * 3;
      canvas[dst] = Math.round(image.pixels[src] * alpha + canvas[dst] * (1 - alpha));
      canvas[dst + 1] = Math.round(image.pixels[src + 1] * alpha + canvas[dst + 1] * (1 - alpha));
      canvas[dst + 2] = Math.round(image.pixels[src + 2] * alpha + canvas[dst + 2] * (1 - alpha));
    }
  }
}

function drawMascot(canvas, pose, label, x, y, size) {
  drawImage(canvas, poses[pose], x, y, size, size);
  const faceCxRatio = pose === "point" ? 0.543 : pose === "cheer" ? 0.505 : 0.5;
  const faceCx = x + size * faceCxRatio;
  const faceCy = y + size * 0.37;
  const labelColor = label === "han" ? [24, 128, 91] : label === "EN" ? [30, 64, 175] : [38, 78, 140];

  if (label === "han") {
    drawHangulHan(canvas, faceCx, faceCy + size * 0.005, labelColor, Math.max(1, Math.round(size / 88)));
  } else {
    const scale = Math.max(2, Math.round(size / 58));
    drawCenteredText(canvas, label, faceCx, faceCy + size * 0.01, labelColor, scale);
  }
}

function fillPolygon(canvas, points, color) {
  const minX = Math.floor(Math.min(...points.map((p) => p[0])));
  const maxX = Math.ceil(Math.max(...points.map((p) => p[0])));
  const minY = Math.floor(Math.min(...points.map((p) => p[1])));
  const maxY = Math.ceil(Math.max(...points.map((p) => p[1])));
  for (let y = minY; y <= maxY; y++) {
    for (let x = minX; x <= maxX; x++) {
      let inside = false;
      for (let i = 0, j = points.length - 1; i < points.length; j = i++) {
        const xi = points[i][0], yi = points[i][1], xj = points[j][0], yj = points[j][1];
        if (yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi) inside = !inside;
      }
      if (inside) setPixel(canvas, x, y, color);
    }
  }
}

function drawLine(canvas, x0, y0, x1, y1, color) {
  let dx = Math.abs(x1 - x0);
  let dy = -Math.abs(y1 - y0);
  const sx = x0 < x1 ? 1 : -1;
  const sy = y0 < y1 ? 1 : -1;
  let err = dx + dy;
  while (true) {
    setPixel(canvas, x0, y0, color);
    if (x0 === x1 && y0 === y1) break;
    const e2 = 2 * err;
    if (e2 >= dy) { err += dy; x0 += sx; }
    if (e2 <= dx) { err += dx; y0 += sy; }
  }
}

function strokePolygon(canvas, points, color) {
  for (let i = 0; i < points.length; i++) {
    const j = (i + 1) % points.length;
    drawLine(canvas, Math.round(points[i][0]), Math.round(points[i][1]), Math.round(points[j][0]), Math.round(points[j][1]), color);
  }
}

function drawCursor(canvas, x, y) {
  const shape = [[x, y], [x, y + 33], [x + 9, y + 25], [x + 15, y + 38], [x + 21, y + 36], [x + 15, y + 23], [x + 29, y + 23]];
  fillPolygon(canvas, shape.map(([px, py]) => [px + 3, py + 3]), [203, 213, 225]);
  fillPolygon(canvas, shape, [255, 255, 255]);
  strokePolygon(canvas, shape, [15, 23, 42]);
}

function drawBase(canvas, caption) {
  fillRoundRect(canvas, 50, 34, 540, 250, 12, [255, 255, 255]);
  strokeRect(canvas, 50, 34, 540, 250, [203, 213, 225]);
  fillRect(canvas, 50, 34, 540, 34, [236, 242, 249]);
  drawText(canvas, "HANEN CURSOR INDICATOR", 74, 47, [15, 23, 42], 2);

  fillRoundRect(canvas, 88, 88, 452, 56, 8, [236, 242, 249]);
  strokeRect(canvas, 88, 88, 452, 56, [203, 213, 225]);
  drawText(canvas, "TYPE ANYWHERE", 112, 109, [100, 116, 139], 2);

  fillRoundRect(canvas, 90, 238, 460, 34, 8, [246, 248, 252]);
  drawCenteredText(canvas, caption, 320, 255, [71, 85, 105], 2);

  fillRect(canvas, 0, 304, width, 56, [15, 23, 42]);
  drawText(canvas, "TRAY", 472, 329, [226, 232, 240], 2);
  fillRoundRect(canvas, 524, 318, 30, 30, 15, [24, 128, 91]);
  drawText(canvas, "ON", 534, 328, [255, 255, 255], 1);
}

function drawSizePanel(canvas, percent) {
  fillRoundRect(canvas, 372, 168, 192, 102, 8, [255, 255, 255]);
  strokeRect(canvas, 372, 168, 192, 102, [203, 213, 225]);
  drawText(canvas, "SIZE", 392, 184, [30, 41, 59], 2);
  drawText(canvas, `${percent}%`, 500, 184, [30, 64, 175], 2);
  fillRoundRect(canvas, 394, 219, 136, 10, 5, [226, 232, 240]);
  const knobX = 394 + Math.round(((percent - 50) / 200) * 136);
  fillCircle(canvas, knobX, 224, 11, [30, 64, 175]);
  drawText(canvas, "DRAG SLIDER", 407, 245, [100, 116, 139], 1);
}

function ease(t) {
  return t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;
}

function makeFrame(frameIndex) {
  const canvas = makeRgbCanvas();
  let pose = "idle";
  let label = "en";
  let caption = "MINIMI IDLE POSE FOLLOWS CURSOR";
  let mascotSize = 118;
  let mascotX = 268;
  let mascotY = 114;
  let cursorX = 250;
  let cursorY = 151;
  let showSize = false;
  let percent = 100;

  if (frameIndex < 9) {
    const t = ease(frameIndex / 8);
    mascotX = Math.round(214 + 54 * t);
    cursorX = mascotX - 28;
  } else if (frameIndex < 18) {
    const t = ease((frameIndex - 9) / 8);
    pose = "point";
    label = "han";
    caption = "INPUT CHANGED - POINTS FOR 1 SECOND";
    mascotX = Math.round(268 + 36 * t);
    cursorX = mascotX - 26;
  } else if (frameIndex < 25) {
    pose = "idle";
    label = "han";
    caption = "RETURNS TO IDLE WITH FACE LABEL";
    mascotX = 304;
    cursorX = mascotX - 28;
  } else if (frameIndex < 30) {
    pose = "cheer";
    label = "EN";
    caption = "CAPS OR SHIFT SHOWS EN";
    mascotX = 304;
    mascotY = 108;
    cursorX = mascotX - 26;
  } else {
    const t = ease((frameIndex - 30) / 5);
    pose = "idle";
    label = frameIndex % 2 === 0 ? "EN" : "en";
    percent = Math.round(100 + 50 * t);
    mascotSize = Math.round(118 * (percent / 100));
    mascotX = Math.round(265 - (mascotSize - 118) / 2);
    mascotY = Math.round(120 - (mascotSize - 118) / 2);
    cursorX = mascotX - 20;
    cursorY = mascotY + Math.round(mascotSize / 2) - 6;
    caption = "DRAG SIZE CONTROL - SAVES PERCENT";
    showSize = true;
  }

  drawBase(canvas, caption);
  drawMascot(canvas, pose, label, mascotX, mascotY, mascotSize);
  drawCursor(canvas, cursorX, cursorY);
  if (showSize) drawSizePanel(canvas, percent);
  return quantize(canvas);
}

function quantize(canvas) {
  const indexed = new Uint8Array(width * height);
  for (let i = 0, p = 0; i < canvas.length; i += 3, p++) {
    indexed[p] = colorIndex(canvas[i], canvas[i + 1], canvas[i + 2]);
  }
  return indexed;
}

function colorIndex(r, g, b) {
  const key = (r << 16) | (g << 8) | b;
  if (colorCache.has(key)) return colorCache.get(key);

  const qr = Math.max(0, Math.min(5, Math.round(r / 51)));
  const qg = Math.max(0, Math.min(5, Math.round(g / 51)));
  const qb = Math.max(0, Math.min(5, Math.round(b / 51)));
  const index = 1 + qr * 36 + qg * 6 + qb;
  colorCache.set(key, index);
  return index;
}

function writeUint16(out, value) {
  out.push(value & 0xff, (value >> 8) & 0xff);
}

function writeAscii(out, value) {
  for (let i = 0; i < value.length; i++) out.push(value.charCodeAt(i));
}

function writeSubBlocks(out, data) {
  for (let offset = 0; offset < data.length; offset += 255) {
    const chunk = data.slice(offset, offset + 255);
    out.push(chunk.length);
    for (const byte of chunk) out.push(byte);
  }
  out.push(0);
}

function lzwRawEncode(indices) {
  const clearCode = 256;
  const endCode = 257;
  const codeSize = 9;
  const bytes = [];
  let bitBuffer = 0;
  let bitCount = 0;
  let codesSinceClear = 0;

  function output(code) {
    bitBuffer |= code << bitCount;
    bitCount += codeSize;
    while (bitCount >= 8) {
      bytes.push(bitBuffer & 0xff);
      bitBuffer >>= 8;
      bitCount -= 8;
    }
  }

  output(clearCode);
  for (const index of indices) {
    output(index);
    codesSinceClear++;
    if (codesSinceClear >= 100) {
      output(clearCode);
      codesSinceClear = 0;
    }
  }
  output(endCode);
  if (bitCount > 0) bytes.push(bitBuffer & 0xff);
  return Uint8Array.from(bytes);
}

function createGif() {
  const out = [];
  writeAscii(out, "GIF89a");
  writeUint16(out, width);
  writeUint16(out, height);
  out.push(0xf7, 0, 0);
  for (const [r, g, b] of palette) out.push(r, g, b);

  writeAscii(out, "!\xff\x0bNETSCAPE2.0\x03\x01");
  writeUint16(out, 0);
  out.push(0);

  for (let i = 0; i < frameCount; i++) {
    out.push(0x21, 0xf9, 0x04, 0x08);
    writeUint16(out, delayCs);
    out.push(0, 0);
    out.push(0x2c);
    writeUint16(out, 0);
    writeUint16(out, 0);
    writeUint16(out, width);
    writeUint16(out, height);
    out.push(0);
    out.push(8);
    writeSubBlocks(out, lzwRawEncode(makeFrame(i)));
  }

  out.push(0x3b);
  return Buffer.from(out);
}

fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(outFile, createGif());
console.log(`Created ${path.relative(root, outFile)}`);

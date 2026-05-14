const fs = require("fs");
const path = require("path");

const width = 640;
const height = 360;
const frameCount = 36;
const delayCs = 9;

const root = path.resolve(__dirname, "..");
const outDir = path.join(root, "assets");
const outFile = path.join(outDir, "demo.gif");

const palette = [
  [246, 248, 252],
  [255, 255, 255],
  [214, 221, 232],
  [30, 41, 59],
  [100, 116, 139],
  [17, 24, 39],
  [203, 213, 225],
  [24, 128, 91],
  [38, 78, 140],
  [255, 255, 255],
  [236, 242, 249],
  [248, 199, 79],
  [220, 38, 38],
  [241, 245, 249],
  [16, 185, 129],
  [148, 163, 184],
  [226, 232, 240],
  [230, 247, 239],
  [232, 240, 255],
  [20, 83, 45],
  [30, 64, 175],
  [15, 23, 42],
];

while (palette.length < 32) {
  palette.push([0, 0, 0]);
}

const glyphs = {
  " ": ["00000", "00000", "00000", "00000", "00000", "00000", "00000"],
  ".": ["00000", "00000", "00000", "00000", "00000", "01100", "01100"],
  "-": ["00000", "00000", "00000", "11110", "00000", "00000", "00000"],
  "/": ["00001", "00010", "00100", "01000", "10000", "00000", "00000"],
  ":": ["00000", "01100", "01100", "00000", "01100", "01100", "00000"],
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

function makeCanvas() {
  return new Uint8Array(width * height);
}

function setPixel(canvas, x, y, color) {
  if (x < 0 || y < 0 || x >= width || y >= height) return;
  canvas[y * width + x] = color;
}

function fillRect(canvas, x, y, w, h, color) {
  const x0 = Math.max(0, Math.floor(x));
  const y0 = Math.max(0, Math.floor(y));
  const x1 = Math.min(width, Math.ceil(x + w));
  const y1 = Math.min(height, Math.ceil(y + h));
  for (let yy = y0; yy < y1; yy++) {
    canvas.fill(color, yy * width + x0, yy * width + x1);
  }
}

function strokeRect(canvas, x, y, w, h, color, thickness = 1) {
  fillRect(canvas, x, y, w, thickness, color);
  fillRect(canvas, x, y + h - thickness, w, thickness, color);
  fillRect(canvas, x, y, thickness, h, color);
  fillRect(canvas, x + w - thickness, y, thickness, h, color);
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

function strokeRoundRect(canvas, x, y, w, h, r, color) {
  strokeRect(canvas, x + r, y, w - r * 2, 1, color);
  strokeRect(canvas, x + r, y + h - 1, w - r * 2, 1, color);
  strokeRect(canvas, x, y + r, 1, h - r * 2, color);
  strokeRect(canvas, x + w - 1, y + r, 1, h - r * 2, color);
  for (let i = 0; i <= r; i++) {
    const dx = Math.round(Math.sqrt(r * r - i * i));
    setPixel(canvas, x + r - dx, y + r - i, color);
    setPixel(canvas, x + w - r - 1 + dx, y + r - i, color);
    setPixel(canvas, x + r - dx, y + h - r - 1 + i, color);
    setPixel(canvas, x + w - r - 1 + dx, y + h - r - 1 + i, color);
  }
}

function drawText(canvas, text, x, y, color, scale = 2) {
  let cursorX = x;
  const normalized = text.toUpperCase();
  for (const char of normalized) {
    const glyph = glyphs[char] || glyphs[" "];
    for (let row = 0; row < glyph.length; row++) {
      for (let col = 0; col < glyph[row].length; col++) {
        if (glyph[row][col] === "1") {
          fillRect(canvas, cursorX + col * scale, y + row * scale, scale, scale, color);
        }
      }
    }
    cursorX += 6 * scale;
  }
}

function drawHangulHan(canvas, x, y, color) {
  fillRect(canvas, x + 5, y + 1, 12, 3, color);
  fillRect(canvas, x + 10, y, 3, 5, color);
  fillRect(canvas, x + 5, y + 8, 12, 3, color);
  fillRect(canvas, x + 8, y + 13, 7, 4, color);
  fillRect(canvas, x + 23, y + 1, 3, 18, color);
  fillRect(canvas, x + 23, y + 8, 6, 3, color);
  fillRect(canvas, x + 6, y + 23, 21, 3, color);
  fillRect(canvas, x + 6, y + 18, 3, 8, color);
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
        const xi = points[i][0];
        const yi = points[i][1];
        const xj = points[j][0];
        const yj = points[j][1];
        const intersects = yi > y !== yj > y && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi;
        if (intersects) inside = !inside;
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
    if (e2 >= dy) {
      err += dy;
      x0 += sx;
    }
    if (e2 <= dx) {
      err += dx;
      y0 += sy;
    }
  }
}

function strokePolygon(canvas, points, color) {
  for (let i = 0; i < points.length; i++) {
    const next = (i + 1) % points.length;
    drawLine(canvas, Math.round(points[i][0]), Math.round(points[i][1]), Math.round(points[next][0]), Math.round(points[next][1]), color);
  }
}

function drawCursor(canvas, x, y) {
  const shape = [
    [x, y],
    [x, y + 33],
    [x + 9, y + 25],
    [x + 15, y + 38],
    [x + 21, y + 36],
    [x + 15, y + 23],
    [x + 29, y + 23],
  ];
  const shadow = shape.map(([px, py]) => [px + 3, py + 3]);
  fillPolygon(canvas, shadow, 6);
  fillPolygon(canvas, shape, 9);
  strokePolygon(canvas, shape, 5);
}

function drawBubble(canvas, label, x, y) {
  const korean = label === "han";
  const fill = korean ? 7 : 8;
  fillRoundRect(canvas, x, y, 48, 30, 7, fill);
  if (korean) {
    drawHangulHan(canvas, x + 9, y + 2, 9);
  } else {
    drawText(canvas, "en", x + 10, y + 8, 9, 2);
  }
}

function lerp(a, b, t) {
  return Math.round(a + (b - a) * t);
}

function ease(t) {
  return t < 0.5 ? 2 * t * t : 1 - Math.pow(-2 * t + 2, 2) / 2;
}

function drawBase(canvas) {
  fillRect(canvas, 0, 0, width, height, 0);
  fillRoundRect(canvas, 55, 42, 530, 238, 12, 1);
  strokeRoundRect(canvas, 55, 42, 530, 238, 12, 2);
  fillRect(canvas, 55, 42, 530, 34, 13);
  drawText(canvas, "HANEN CURSOR INDICATOR", 78, 55, 3, 2);

  fillRoundRect(canvas, 95, 102, 450, 54, 8, 10);
  strokeRoundRect(canvas, 95, 102, 450, 54, 8, 2);
  drawText(canvas, "TYPE ANYWHERE", 119, 121, 4, 2);

  fillRoundRect(canvas, 95, 178, 196, 50, 8, 18);
  strokeRoundRect(canvas, 95, 178, 196, 50, 8, 2);
  drawText(canvas, "ENGLISH MODE", 118, 197, 20, 2);

  fillRoundRect(canvas, 333, 178, 176, 50, 8, 17);
  strokeRoundRect(canvas, 333, 178, 176, 50, 8, 2);
  drawText(canvas, "KOREAN MODE", 354, 197, 19, 2);

  fillRect(canvas, 0, 304, width, 56, 21);
  fillRoundRect(canvas, 522, 318, 30, 30, 15, 7);
  drawText(canvas, "ON", 532, 328, 9, 1);
  drawText(canvas, "TRAY", 475, 329, 16, 2);
}

function drawTrayMenu(canvas) {
  fillRoundRect(canvas, 390, 210, 188, 94, 8, 1);
  strokeRoundRect(canvas, 390, 210, 188, 94, 8, 2);
  fillRoundRect(canvas, 405, 226, 24, 24, 12, 14);
  fillRect(canvas, 415, 231, 4, 14, 9);
  drawText(canvas, "SHOW ON", 440, 232, 3, 2);
  fillRect(canvas, 405, 259, 150, 1, 16);
  drawText(canvas, "EXIT", 440, 273, 4, 2);
}

function makeFrame(frameIndex) {
  const canvas = makeCanvas();
  drawBase(canvas);

  let x = 160;
  let y = 128;
  let label = "en";
  let caption = "RUN EXE - INDICATOR FOLLOWS CURSOR";
  let showMenu = false;

  if (frameIndex < 10) {
    const t = ease(frameIndex / 9);
    x = lerp(140, 257, t);
    y = lerp(126, 204, t);
    label = "en";
  } else if (frameIndex < 20) {
    const t = ease((frameIndex - 10) / 9);
    x = lerp(257, 398, t);
    y = lerp(204, 203, t);
    label = "han";
    caption = "SWITCH INPUT - LABEL CHANGES INSTANTLY";
  } else if (frameIndex < 28) {
    const t = ease((frameIndex - 20) / 7);
    x = lerp(398, 526, t);
    y = lerp(203, 316, t);
    label = "han";
    caption = "OPEN TRAY - TURN DISPLAY ON OR OFF";
    showMenu = frameIndex > 23;
  } else {
    const t = ease((frameIndex - 28) / 7);
    x = lerp(526, 202, t);
    y = lerp(316, 126, t);
    label = frameIndex % 8 < 4 ? "han" : "en";
    caption = "ONE EXE - NO INSTALLER REQUIRED";
    showMenu = frameIndex < 31;
  }

  drawText(canvas, caption, 94, 250, 4, 2);
  drawBubble(canvas, label, x + 19, y + 2);
  if (showMenu) drawTrayMenu(canvas);
  drawCursor(canvas, x, y);
  return canvas;
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

function lzwEncode(indices) {
  const minCodeSize = 5;
  const clearCode = 1 << minCodeSize;
  const endCode = clearCode + 1;
  let codeSize = minCodeSize + 1;
  const bytes = [];
  let bitBuffer = 0;
  let bitCount = 0;

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
  let codesSinceClear = 0;

  for (let i = 0; i < indices.length; i++) {
    output(indices[i]);
    codesSinceClear++;

    if (codesSinceClear >= 24 && i < indices.length - 1) {
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
  out.push(0xf4, 0, 0);
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
    out.push(5);
    writeSubBlocks(out, lzwEncode(makeFrame(i)));
  }

  out.push(0x3b);
  return Buffer.from(out);
}

fs.mkdirSync(outDir, { recursive: true });
fs.writeFileSync(outFile, createGif());
console.log(`Created ${path.relative(root, outFile)}`);

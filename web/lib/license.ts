import crypto from "crypto";
import { intEnv, requireEnv } from "./env";

export type LicenseTokenPayload = {
  licenseId: string;
  machineHash: string;
  exp: number;
};

export function normalizeLicenseKey(value: string): string {
  return value.trim().toUpperCase().replace(/\s+/g, "");
}

export function createLicenseKey(): string {
  const alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
  const chars: string[] = [];
  const bytes = crypto.randomBytes(20);
  for (const byte of bytes) {
    chars.push(alphabet[byte % alphabet.length]);
  }

  return `HCI-${chars.slice(0, 4).join("")}-${chars.slice(4, 8).join("")}-${chars
    .slice(8, 12)
    .join("")}-${chars.slice(12, 16).join("")}`;
}

export function hashLicenseKey(licenseKey: string): string {
  return crypto.createHash("sha256").update(normalizeLicenseKey(licenseKey)).digest("hex");
}

export function encryptLicenseKey(licenseKey: string): string {
  const iv = crypto.randomBytes(12);
  const cipher = crypto.createCipheriv("aes-256-gcm", encryptionKey(), iv);
  const encrypted = Buffer.concat([cipher.update(licenseKey, "utf8"), cipher.final()]);
  const tag = cipher.getAuthTag();
  return `${iv.toString("base64url")}.${tag.toString("base64url")}.${encrypted.toString("base64url")}`;
}

export function decryptLicenseKey(ciphertext: string): string {
  const [ivText, tagText, encryptedText] = ciphertext.split(".");
  if (!ivText || !tagText || !encryptedText) {
    return "";
  }

  const decipher = crypto.createDecipheriv("aes-256-gcm", encryptionKey(), Buffer.from(ivText, "base64url"));
  decipher.setAuthTag(Buffer.from(tagText, "base64url"));
  return Buffer.concat([decipher.update(Buffer.from(encryptedText, "base64url")), decipher.final()]).toString("utf8");
}

export function offlineUntilDate(): Date {
  const days = intEnv("LICENSE_OFFLINE_DAYS", 14);
  return new Date(Date.now() + days * 24 * 60 * 60 * 1000);
}

export function createLicenseToken(payload: LicenseTokenPayload): string {
  const body = base64UrlEncode(JSON.stringify(payload));
  const signature = sign(body);
  return `${body}.${signature}`;
}

export function verifyLicenseToken(token: string): LicenseTokenPayload | null {
  const [body, signature] = token.split(".");
  if (!body || !signature) {
    return null;
  }

  if (!safeEqual(signature, sign(body))) {
    return null;
  }

  const payload = JSON.parse(Buffer.from(body, "base64url").toString("utf8")) as LicenseTokenPayload;
  if (!payload.licenseId || !payload.machineHash || !payload.exp || payload.exp < Math.floor(Date.now() / 1000)) {
    return null;
  }

  return payload;
}

export function maxActivations(): number {
  return intEnv("LICENSE_MAX_ACTIVATIONS", 2);
}

function sign(body: string): string {
  return crypto.createHmac("sha256", requireEnv("LICENSE_TOKEN_SECRET")).update(body).digest("base64url");
}

function encryptionKey(): Buffer {
  return crypto.createHash("sha256").update(requireEnv("LICENSE_TOKEN_SECRET")).digest();
}

function safeEqual(a: string, b: string): boolean {
  const left = Buffer.from(a);
  const right = Buffer.from(b);
  return left.length === right.length && crypto.timingSafeEqual(left, right);
}

function base64UrlEncode(value: string): string {
  return Buffer.from(value, "utf8").toString("base64url");
}

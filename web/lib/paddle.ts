import crypto from "crypto";
import { optionalEnv, requireEnv } from "./env";

export function verifyPaddleWebhook(rawBody: string, signatureHeader: string | null): boolean {
  const secret = requireEnv("PADDLE_WEBHOOK_SECRET");
  if (!signatureHeader) {
    return false;
  }

  const parts = Object.fromEntries(
    signatureHeader.split(";").map((part) => {
      const [key, value] = part.split("=");
      return [key, value];
    })
  );

  const timestamp = parts.ts;
  const signature = parts.h1;
  if (!timestamp || !signature) {
    return false;
  }

  const expected = crypto.createHmac("sha256", secret).update(`${timestamp}:${rawBody}`).digest("hex");
  const left = Buffer.from(signature, "hex");
  const right = Buffer.from(expected, "hex");
  return left.length === right.length && crypto.timingSafeEqual(left, right);
}

export async function getPaddleCustomerEmail(customerId: string | null | undefined): Promise<string> {
  if (!customerId) {
    return "";
  }

  const apiKey = optionalEnv("PADDLE_API_KEY");
  if (!apiKey) {
    return "";
  }

  const baseUrl = optionalEnv("PADDLE_API_BASE_URL", "https://api.paddle.com");
  const response = await fetch(`${baseUrl}/customers/${customerId}`, {
    headers: {
      Authorization: `Bearer ${apiKey}`,
      Accept: "application/json"
    }
  });

  if (!response.ok) {
    return "";
  }

  const payload = await response.json();
  return payload?.data?.email || "";
}

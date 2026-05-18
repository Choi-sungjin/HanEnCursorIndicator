import { NextRequest, NextResponse } from "next/server";
import { optionalEnv } from "../../../../lib/env";
import { sendLicenseEmail } from "../../../../lib/email";
import { createLicenseKey, encryptLicenseKey, hashLicenseKey, maxActivations } from "../../../../lib/license";
import { getPaddleCustomerEmail, verifyPaddleWebhook } from "../../../../lib/paddle";
import { getServiceSupabase } from "../../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const rawBody = await request.text();
  if (!verifyPaddleWebhook(rawBody, request.headers.get("paddle-signature"))) {
    return NextResponse.json({ ok: false, error: "Invalid Paddle signature" }, { status: 401 });
  }

  const event = JSON.parse(rawBody);
  if (event.event_type !== "transaction.completed") {
    return NextResponse.json({ ok: true, skipped: true });
  }

  const transaction = event.data;
  const transactionId = transaction.id;
  const customerId = transaction.customer_id || null;
  const productName = optionalEnv("PADDLE_PRODUCT_NAME", "HanEn Cursor Indicator Personal Lifetime");
  const email = transaction.customer?.email || transaction.custom_data?.email || (await getPaddleCustomerEmail(customerId));
  const supabase = getServiceSupabase();

  const { data: existing } = await supabase
    .from("licenses")
    .select("id")
    .eq("paddle_transaction_id", transactionId)
    .maybeSingle();

  if (existing) {
    return NextResponse.json({ ok: true, idempotent: true });
  }

  const licenseKey = createLicenseKey();
  const { error } = await supabase.from("licenses").insert({
    key_hash: hashLicenseKey(licenseKey),
    key_ciphertext: encryptLicenseKey(licenseKey),
    key_suffix: licenseKey.slice(-4),
    email: email || null,
    paddle_transaction_id: transactionId,
    paddle_customer_id: customerId,
    product_name: productName,
    max_activations: maxActivations(),
    status: "active"
  });

  if (error) {
    return NextResponse.json({ ok: false, error: error.message }, { status: 500 });
  }

  await sendLicenseEmail(email, licenseKey);
  return NextResponse.json({ ok: true });
}

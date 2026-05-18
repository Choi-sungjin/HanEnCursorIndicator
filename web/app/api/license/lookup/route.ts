import { NextRequest, NextResponse } from "next/server";
import { decryptLicenseKey } from "../../../../lib/license";
import { getServiceSupabase } from "../../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const body = await request.json();
  const transactionId = String(body.transactionId || "").trim();
  const email = String(body.email || "").trim().toLowerCase();

  if (!transactionId) {
    return NextResponse.json({ ok: false, error: "transactionId is required" }, { status: 400 });
  }

  const supabase = getServiceSupabase();
  let query = supabase
    .from("licenses")
    .select("key_ciphertext,email,status")
    .eq("paddle_transaction_id", transactionId)
    .maybeSingle();

  const { data: license } = await query;
  if (!license || license.status !== "active") {
    return NextResponse.json({ ok: false, error: "License was not found" }, { status: 404 });
  }

  if (email && license.email && license.email.toLowerCase() !== email) {
    return NextResponse.json({ ok: false, error: "Email does not match this transaction" }, { status: 403 });
  }

  const licenseKey = decryptLicenseKey(license.key_ciphertext || "");
  if (!licenseKey) {
    return NextResponse.json({ ok: false, error: "License key cannot be recovered" }, { status: 500 });
  }

  return NextResponse.json({ ok: true, licenseKey });
}

import { NextRequest, NextResponse } from "next/server";
import { createLicenseToken, hashLicenseKey, normalizeLicenseKey, offlineUntilDate, verifyLicenseToken } from "../../../../lib/license";
import { getServiceSupabase } from "../../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const body = await request.json();
  const licenseKey = normalizeLicenseKey(body.licenseKey || "");
  const machineHash = String(body.machineHash || "").trim();
  const token = String(body.token || "").trim();

  if (!licenseKey || !machineHash) {
    return NextResponse.json({ ok: false, error: "licenseKey and machineHash are required" }, { status: 400 });
  }

  const supabase = getServiceSupabase();
  const { data: license } = await supabase
    .from("licenses")
    .select("id,status")
    .eq("key_hash", hashLicenseKey(licenseKey))
    .maybeSingle();

  if (!license || license.status !== "active") {
    return NextResponse.json({ ok: false, error: "Invalid or inactive license" }, { status: 403 });
  }

  const tokenPayload = token ? verifyLicenseToken(token) : null;
  if (token && (!tokenPayload || tokenPayload.licenseId !== license.id || tokenPayload.machineHash !== machineHash)) {
    return NextResponse.json({ ok: false, error: "Invalid token" }, { status: 403 });
  }

  const { data: activation } = await supabase
    .from("license_activations")
    .select("id")
    .eq("license_id", license.id)
    .eq("machine_hash", machineHash)
    .is("deactivated_at", null)
    .maybeSingle();

  if (!activation) {
    return NextResponse.json({ ok: false, error: "This PC is not activated" }, { status: 403 });
  }

  await supabase.from("license_activations").update({ last_seen_at: new Date().toISOString() }).eq("id", activation.id);

  const offlineUntil = offlineUntilDate();
  const refreshedToken = createLicenseToken({
    licenseId: license.id,
    machineHash,
    exp: Math.floor(offlineUntil.getTime() / 1000)
  });

  return NextResponse.json({ ok: true, token: refreshedToken, offlineUntil: offlineUntil.toISOString() });
}

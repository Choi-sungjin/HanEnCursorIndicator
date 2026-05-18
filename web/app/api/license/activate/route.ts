import { NextRequest, NextResponse } from "next/server";
import { createLicenseToken, hashLicenseKey, maxActivations, normalizeLicenseKey, offlineUntilDate } from "../../../../lib/license";
import { getServiceSupabase } from "../../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const body = await request.json();
  const licenseKey = normalizeLicenseKey(body.licenseKey || "");
  const machineHash = String(body.machineHash || "").trim();
  const appVersion = String(body.appVersion || "").trim();

  if (!licenseKey || !machineHash) {
    return NextResponse.json({ ok: false, error: "licenseKey and machineHash are required" }, { status: 400 });
  }

  const supabase = getServiceSupabase();
  const { data: license, error } = await supabase
    .from("licenses")
    .select("id,status,max_activations")
    .eq("key_hash", hashLicenseKey(licenseKey))
    .maybeSingle();

  if (error || !license || license.status !== "active") {
    return NextResponse.json({ ok: false, error: "Invalid or inactive license" }, { status: 403 });
  }

  const { data: existingActivations } = await supabase
    .from("license_activations")
    .select("id,machine_hash")
    .eq("license_id", license.id)
    .is("deactivated_at", null);

  const alreadyActive = (existingActivations || []).some((activation) => activation.machine_hash === machineHash);
  const limit = license.max_activations || maxActivations();
  if (!alreadyActive && (existingActivations || []).length >= limit) {
    return NextResponse.json({ ok: false, error: "Activation limit reached" }, { status: 403 });
  }

  if (alreadyActive) {
    await supabase
      .from("license_activations")
      .update({ last_seen_at: new Date().toISOString(), app_version: appVersion })
      .eq("license_id", license.id)
      .eq("machine_hash", machineHash)
      .is("deactivated_at", null);
  } else {
    await supabase.from("license_activations").insert({
      license_id: license.id,
      machine_hash: machineHash,
      app_version: appVersion
    });
  }

  const offlineUntil = offlineUntilDate();
  const token = createLicenseToken({
    licenseId: license.id,
    machineHash,
    exp: Math.floor(offlineUntil.getTime() / 1000)
  });

  return NextResponse.json({ ok: true, token, offlineUntil: offlineUntil.toISOString() });
}

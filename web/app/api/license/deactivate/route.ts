import { NextRequest, NextResponse } from "next/server";
import { hashLicenseKey, normalizeLicenseKey } from "../../../../lib/license";
import { getServiceSupabase } from "../../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const body = await request.json();
  const licenseKey = normalizeLicenseKey(body.licenseKey || "");
  const machineHash = String(body.machineHash || "").trim();

  if (!licenseKey || !machineHash) {
    return NextResponse.json({ ok: false, error: "licenseKey and machineHash are required" }, { status: 400 });
  }

  const supabase = getServiceSupabase();
  const { data: license } = await supabase
    .from("licenses")
    .select("id")
    .eq("key_hash", hashLicenseKey(licenseKey))
    .maybeSingle();

  if (license) {
    await supabase
      .from("license_activations")
      .update({ deactivated_at: new Date().toISOString() })
      .eq("license_id", license.id)
      .eq("machine_hash", machineHash)
      .is("deactivated_at", null);
  }

  return NextResponse.json({ ok: true });
}

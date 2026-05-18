import { NextRequest, NextResponse } from "next/server";
import { optionalEnv, requireEnv } from "../../../lib/env";
import { hashLicenseKey, normalizeLicenseKey } from "../../../lib/license";
import { getServiceSupabase } from "../../../lib/supabase";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const body = await request.json();
  const licenseKey = normalizeLicenseKey(body.licenseKey || "");

  if (!licenseKey) {
    return NextResponse.json({ error: "licenseKey is required" }, { status: 400 });
  }

  const supabase = getServiceSupabase();
  const { data: license } = await supabase
    .from("licenses")
    .select("id,status")
    .eq("key_hash", hashLicenseKey(licenseKey))
    .maybeSingle();

  if (!license || license.status !== "active") {
    return NextResponse.json({ error: "Invalid or inactive license" }, { status: 403 });
  }

  const bucket = requireEnv("SUPABASE_DOWNLOAD_BUCKET");
  const objectPath = requireEnv("SUPABASE_DOWNLOAD_OBJECT");
  const expiresIn = Number.parseInt(optionalEnv("DOWNLOAD_URL_TTL_SECONDS", "300"), 10);
  const { data, error } = await supabase.storage.from(bucket).createSignedUrl(objectPath, expiresIn);

  if (error || !data?.signedUrl) {
    return NextResponse.json({ error: "Download file is not available" }, { status: 500 });
  }

  await supabase.from("download_events").insert({
    license_id: license.id,
    machine_hash: String(body.machineHash || "").trim() || null
  });

  return NextResponse.json({ url: data.signedUrl });
}

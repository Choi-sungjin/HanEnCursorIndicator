import { optionalEnv } from "./env";

export async function sendLicenseEmail(to: string, licenseKey: string) {
  const apiKey = optionalEnv("RESEND_API_KEY");
  const from = optionalEnv("LICENSE_EMAIL_FROM");
  const appUrl = optionalEnv("NEXT_PUBLIC_APP_URL", "https://hanen-cursor-indicator.vercel.app");
  if (!apiKey || !from || !to) {
    return;
  }

  await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${apiKey}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      from,
      to,
      subject: "HanEn Cursor Indicator license key",
      text: `Thank you for purchasing HanEn Cursor Indicator.\n\nLicense key: ${licenseKey}\nDownload: ${appUrl}/account\n`
    })
  });
}

# Commercial Launch Checklist

## 1. Repository Policy

- Keep GitHub public for README, GIFs, docs, issues, and development source.
- Do not commit paid EXE, production ZIPs, `.env` files, Paddle keys, Supabase service role keys, or license token secrets.
- Publish customer downloads through the web app after license verification.

## 2. Paddle Setup

- Create a Paddle account and complete seller verification.
- Create `Personal Lifetime`.
- Price v1 at `14,900 KRW` and `12.99 USD`.
- Set the success URL to `/success`.
- Create a webhook endpoint for `/api/paddle/webhook`.
- Copy the webhook secret into Vercel as `PADDLE_WEBHOOK_SECRET`.

## 3. Supabase Setup

- Create a Supabase project.
- Run `web/supabase/schema.sql`.
- Create a private Storage bucket, for example `releases`.
- Upload the commercial ZIP, for example `HanEnCursorIndicator.zip`.
- Put `SUPABASE_URL`, `SUPABASE_SERVICE_ROLE_KEY`, `SUPABASE_DOWNLOAD_BUCKET`, and `SUPABASE_DOWNLOAD_OBJECT` into Vercel.

## 4. Web Deployment

- Deploy `web/` to Vercel.
- Set every variable in `web/.env.example`.
- Generate a long random `LICENSE_TOKEN_SECRET`.
- Test `/api/license/activate`, `/api/license/validate`, `/api/license/deactivate`, and `/api/download`.
- Enable email delivery by adding `RESEND_API_KEY` and `LICENSE_EMAIL_FROM`, or prepare a manual license lookup process for the first buyers.

## 5. Windows App Release

- Build with `build.bat`.
- Zip `CursorImeIndicator.exe` with the `images` folder when needed.
- Upload the ZIP to the private Supabase bucket.
- In the app tray, open `라이선스 등록` and activate against the deployed API URL.
- Confirm the same key activates on 2 PCs and rejects the 3rd PC.
- Confirm offline validation works for 14 days after a successful activation.

## 6. Microsoft Store

- Create or verify a Microsoft Partner Center account.
- Prepare app icon, screenshots, description, privacy URL, support URL, and website URL.
- Package with MSIX or submit the Win32 installer/download-link flow supported by Microsoft Store.
- Keep the Store listing free/installable if using the website license flow inside the app.

## 7. Legal and Operations

- Confirm business registration and mail-order sales reporting requirements.
- Finalize privacy policy, terms, refund policy, and EULA.
- Add customer support email to the website.
- Decide refund handling after license issue/download.

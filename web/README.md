# HanEn Cursor Indicator Web MVP

Next.js launch site and server API for the paid lifetime-license flow.

## What It Does

- Public landing page with Paddle checkout link.
- Paddle `transaction.completed` webhook creates a license key.
- Account page can recover a license key from a Paddle transaction ID.
- License activation API supports two PCs per lifetime license by default.
- Download API returns a short-lived Supabase Storage signed URL.
- No Paddle, Supabase, or email secret is shipped in the Windows app.

## Setup

1. Copy `.env.example` to `.env.local`.
2. Create a Supabase project and run `supabase/schema.sql`.
3. Create a private Storage bucket and upload `HanEnCursorIndicator.zip`.
4. Create a Paddle product named `Personal Lifetime`.
5. Point Paddle webhook to `/api/paddle/webhook`.
6. Set `NEXT_PUBLIC_PADDLE_CHECKOUT_URL` to the hosted checkout link.

## Commands

```bash
npm install
npm run dev
npm run typecheck
npm run build
```

## Security Notes

- `SUPABASE_SERVICE_ROLE_KEY`, `PADDLE_API_KEY`, `PADDLE_WEBHOOK_SECRET`, and `LICENSE_TOKEN_SECRET` are server-only.
- The desktop app stores the license key and offline token with Windows DPAPI.
- GitHub should contain this web source and docs, not paid binaries or production `.env` files.

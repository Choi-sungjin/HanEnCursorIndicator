create extension if not exists pgcrypto;

create table if not exists public.licenses (
  id uuid primary key default gen_random_uuid(),
  key_hash text not null unique,
  key_ciphertext text,
  key_suffix text not null,
  email text,
  paddle_transaction_id text unique,
  paddle_customer_id text,
  product_name text not null,
  max_activations integer not null default 2,
  status text not null default 'active',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index if not exists licenses_email_idx on public.licenses (email);
create index if not exists licenses_status_idx on public.licenses (status);

create table if not exists public.license_activations (
  id uuid primary key default gen_random_uuid(),
  license_id uuid not null references public.licenses(id) on delete cascade,
  machine_hash text not null,
  app_version text,
  activated_at timestamptz not null default now(),
  last_seen_at timestamptz not null default now(),
  deactivated_at timestamptz,
  unique (license_id, machine_hash)
);

create index if not exists license_activations_license_idx on public.license_activations (license_id);
create index if not exists license_activations_machine_idx on public.license_activations (machine_hash);

create table if not exists public.download_events (
  id uuid primary key default gen_random_uuid(),
  license_id uuid not null references public.licenses(id) on delete cascade,
  machine_hash text,
  created_at timestamptz not null default now()
);

alter table public.licenses enable row level security;
alter table public.license_activations enable row level security;
alter table public.download_events enable row level security;

-- This MVP uses SUPABASE_SERVICE_ROLE_KEY from server-only Next.js API routes.
-- Do not create public anon policies for these tables.

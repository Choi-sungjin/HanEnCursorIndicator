"use client";

import { FormEvent, useState } from "react";

export default function AccountPage() {
  const [licenseKey, setLicenseKey] = useState("");
  const [transactionId, setTransactionId] = useState("");
  const [email, setEmail] = useState("");
  const [machineHash, setMachineHash] = useState("");
  const [message, setMessage] = useState("");
  const [downloadUrl, setDownloadUrl] = useState("");
  const [loading, setLoading] = useState(false);

  async function onLookup(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setMessage("");
    setDownloadUrl("");

    const response = await fetch("/api/license/lookup", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ transactionId, email })
    });
    const data = await response.json();
    setLoading(false);

    if (!response.ok) {
      setMessage(data.error || "라이선스 키를 찾지 못했습니다.");
      return;
    }

    setLicenseKey(data.licenseKey);
    setMessage("라이선스 키를 찾았습니다. 아래에서 다운로드 링크를 받을 수 있습니다.");
  }

  async function onDownload(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setMessage("");
    setDownloadUrl("");

    const response = await fetch("/api/download", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ licenseKey, machineHash })
    });
    const data = await response.json();
    setLoading(false);

    if (!response.ok) {
      setMessage(data.error || "다운로드 링크를 만들지 못했습니다.");
      return;
    }

    setDownloadUrl(data.url);
    setMessage("다운로드 링크가 생성되었습니다. 링크는 짧은 시간만 유효합니다.");
  }

  return (
    <main className="shell page">
      <h1>Account</h1>
      <p>결제 거래 ID로 라이선스 키를 찾고, 라이선스 키로 전용 다운로드 링크를 발급합니다.</p>
      <form className="form" onSubmit={onLookup}>
        <input
          required
          value={transactionId}
          onChange={(event) => setTransactionId(event.target.value)}
          placeholder="Paddle transaction ID"
          autoComplete="off"
        />
        <input
          value={email}
          onChange={(event) => setEmail(event.target.value)}
          placeholder="Purchase email optional"
          autoComplete="email"
        />
        <button className="button secondary" disabled={loading} type="submit">
          {loading ? "확인 중" : "라이선스 키 찾기"}
        </button>
      </form>
      <form className="form" onSubmit={onDownload}>
        <input
          required
          value={licenseKey}
          onChange={(event) => setLicenseKey(event.target.value)}
          placeholder="License key"
          autoComplete="off"
        />
        <input
          value={machineHash}
          onChange={(event) => setMachineHash(event.target.value)}
          placeholder="Machine hash optional"
          autoComplete="off"
        />
        <button className="button" disabled={loading} type="submit">
          {loading ? "확인 중" : "다운로드 링크 받기"}
        </button>
      </form>
      {message ? <p className="notice">{message}</p> : null}
      {downloadUrl ? (
        <p>
          <a className="button secondary" href={downloadUrl}>
            Download
          </a>
        </p>
      ) : null}
    </main>
  );
}

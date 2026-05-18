const checkoutUrl = process.env.NEXT_PUBLIC_PADDLE_CHECKOUT_URL || "";

export default function HomePage() {
  return (
    <main className="shell">
      <section className="hero">
        <div>
          <p className="eyebrow">Windows input mode utility</p>
          <h1>HanEn Cursor Indicator</h1>
          <p className="lead">
            마우스 커서 옆에서 현재 입력 상태를 한, en, EN으로 바로 보여주는 Windows 전용 트레이 앱입니다.
            캐릭터팩, 글자 위치 조정, 음성 설정, 커스텀 이미지 흐름을 Pro 라이선스로 제공합니다.
          </p>
          <div className="actions">
            <a className="button" href={checkoutUrl || "/account"}>
              Lifetime license 구매
            </a>
            <a className="button secondary" href="/account">
              라이선스 다운로드
            </a>
          </div>
        </div>
        <div className="demoPanel" aria-label="Product preview">
          <div className="demoStage">
            <div className="cursor" />
            <div className="mascot">
              <div className="face">한</div>
              <div className="body" />
            </div>
          </div>
        </div>
      </section>

      <section className="features">
        <article className="card">
          <h2>Lifetime</h2>
          <p>출시 가격은 14,900 KRW / 12.99 USD 기준이며, 라이선스 1개로 PC 2대까지 활성화합니다.</p>
        </article>
        <article className="card">
          <h2>Private Download</h2>
          <p>실행 파일은 GitHub가 아니라 결제 후 라이선스 검증을 거친 짧은 유효시간 다운로드 링크로 제공합니다.</p>
        </article>
        <article className="card">
          <h2>Windows First</h2>
          <p>Google Play가 아닌 Windows 앱 흐름에 맞춰 자체 웹 결제와 Microsoft Store 등록을 준비합니다.</p>
        </article>
      </section>
    </main>
  );
}

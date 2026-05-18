export default function SuccessPage() {
  return (
    <main className="shell page">
      <h1>결제가 접수되었습니다</h1>
      <p>
        Paddle 결제 완료 웹훅이 도착하면 라이선스가 자동 생성됩니다. 설정된 이메일 발송 키가 있으면 구매 이메일로
        라이선스 키가 전송됩니다.
      </p>
      <p>
        이메일을 받지 못했다면 결제 이메일과 거래 ID를 고객지원으로 보내주세요. 라이선스 키를 받은 뒤 Account에서
        다운로드 링크를 받을 수 있습니다.
      </p>
      <p>
        <a className="button" href="/account">Account 열기</a>
      </p>
    </main>
  );
}

# HARU Community

간단한 게시판 중심 커뮤니티 웹앱입니다. ASP.NET Core MVC, Entity Framework Core, Razor 뷰를 사용하며 로그인·게시판·공지사항·관리자 기능을 제공합니다.

## 실행 방법

```bash
dotnet restore
dotnet build
dotnet run
```

기본 실행 포트는 `https://localhost:5001`, `http://localhost:5000` 입니다.

## 관리자 계정 초기화

최초 실행 시 `DefaultAdmin` 구성이 존재하면 Admin 역할과 기본 관리자 계정이 자동으로 생성됩니다. 비밀번호는 Git에 커밋하지 말고 [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) 또는 환경 변수로 관리하세요.

```jsonc
// appsettings.Development.json 또는 user-secrets
{
  "DefaultAdmin": {
    "Email": "admin@example.com",
    "Password": "안전한_비밀번호!"
  }
}
```

## 주요 기능

- 회원가입 및 로그인 (ASP.NET Core Identity)
- 게시판 생성, 게시글 작성/수정/삭제
- 공지사항 페이지
- 관리자 대시보드: 사용자 목록 확인, 게시판 삭제 등

## 개발 메모

- 로컬 DB 연결 문자열은 `appsettings.Development.json`에서 관리하며 Git에 포함되지 않습니다.
- 관리자 역할을 사용하려면 최초 실행 시 `Admin` 롤 생성과 계정 할당이 필요합니다.
- 새 기능 추가 후에는 `dotnet build` 및 수동 테스트를 권장합니다.

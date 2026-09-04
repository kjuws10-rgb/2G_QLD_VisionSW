# Vision_Align 비정상 종료 대응 패치

- 작업 브랜치: `fix/crash-diagnostics-hardening`
- 기준 커밋: `0bfa9df` (`origin/main`)
- 기능 변경 커밋: `cddf972`
- 빌드: Release / x64
- 작성일: 2026-09-04

## 구성

- `0001-Harden-Vision_Align-crash-handling-and-diagnostics.patch`: 바이너리 변경까지 포함한 Git 패치입니다.
- `files`: 변경된 25개 파일의 전체 복사본이며 저장소 루트 기준 경로를 그대로 유지합니다.
- `changed-files.txt`: 패치에 들어간 파일 목록입니다.
- `SHA256.txt`: 적용용 패치와 실행 파일/PDB의 무결성 값입니다.

## 적용 방법

Git 저장소에는 기준 커밋에서 아래 명령으로 적용합니다.

```powershell
git am --3way .\PATCH\20260904_crash_diagnostics_hardening\0001-Harden-Vision_Align-crash-handling-and-diagnostics.patch
```

Git을 사용할 수 없는 경우 `files` 아래 내용을 저장소 루트의 동일한 경로에 덮어쓸 수 있습니다. 반드시 기존 파일을 먼저 백업하고 프로그램을 종료한 상태에서 적용합니다.

운영 PC에 실행 파일만 배포할 때는 아래 두 파일을 기존 `BIN` 폴더에 백업 후 교체합니다.

- `files\BIN\Vision_Align.exe`
- `files\BIN\Vision_Align.pdb`

기존 운영 PC의 `CONFIG`, `RECIPE`, `RESULT`, `LOG` 폴더는 교체하거나 삭제하지 않습니다.

## 검증 결과

- Visual Studio MSBuild, Release x64 전체 Rebuild: 오류 0건
- PowerShell 지원 스크립트 구문 검사: 오류 0건
- 진단 자료 수집 스크립트 실행 및 ZIP 내용 검증: 통과
- 기준 커밋의 별도 worktree에서 Git 패치 적용 및 기능 커밋과 트리 비교: 통과
- 원본과 `files` 복사본 25개 SHA-256 비교: 불일치 0건
- 실제 카메라·PLC·모션·조명을 연결한 설비 검증은 별도로 필요합니다.

원인 분석, 현장 시험 항목 및 재발 시 자료 수집법은 `files\docs\CRASH_INVESTIGATION.md`를 참고합니다.

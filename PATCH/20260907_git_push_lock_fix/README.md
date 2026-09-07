# Git Push 잠금 오류 수정 패치

- 기준 커밋: `c0428228b3042002693059140794181b97728d49`
- 작업 브랜치: `fix/git-push-locked-vs-files`
- 기능 커밋: `37e0a09` (`Fix push workflow and ignore locked IDE files`)

## 원인

`wonik_sd_vision_align/.vs`와 `Vision_Align/obj`가 Git에 이미 추적된 상태였다. Visual Studio가 `.vsidx` 파일을 열어 둔 동안 기존 `push_file.bat`가 `git add -A`를 실행해 `Permission denied`가 발생했다.

기존 배치 파일은 `git add`, `commit`, `push`의 실패 코드를 확인하지 않아 실제 작업이 실패했어도 마지막에 `Push Complete`를 출력했다. 또한 실행할 때마다 원격을 제거하고 다시 만들며, `wonik_sd_vision_align` 전체를 Git 인덱스에서 제거하는 위험한 동작이 포함돼 있었다.

## 변경 내용

- `.gitignore`에 `.vs`, `obj`, 사용자 IDE 설정, `BIN/LOG`, `BIN/RESULT`를 추가했다.
- Git에 들어 있던 `.vs` 16개 및 `obj` 57개 파일의 추적을 해제했다. 실제 로컬 캐시 파일을 삭제하는 변경은 아니다.
- `push_file.bat`의 고정 사용자 경로를 제거하고 배치 파일이 위치한 저장소를 사용하도록 변경했다.
- 저장소 재초기화, 원격 강제 재설정, 하위 저장소 강제 삭제, 전체 소스 인덱스 제거 동작을 삭제했다.
- 모든 Git 명령의 실패 코드를 검사하고 실패 시 종료 코드 1과 `Push Failed`를 출력한다.
- 성공 흐름을 다음과 같이 변경했다.
  1. `main` 최신화
  2. `update/local-yyyyMMdd-HHmmss` 작업 브랜치 생성
  3. 커밋 및 작업 브랜치 푸시
  4. 작업 브랜치를 `main`에 `--no-ff` 병합
  5. 원격 `main` 푸시

## 검증

- `.vs`, `obj`, `BIN/LOG`, `BIN/RESULT` ignore 규칙 확인
- `.vs`와 `obj`의 Git 추적 파일 수 0 확인
- 실제 저장소에서 `git add -A -- .` 성공 확인
- 임시 로컬 원격 저장소에서 작업 브랜치 생성, 커밋, 브랜치 푸시, 병합 커밋, `main` 푸시 전체 통합 시험 통과
- 변경 없음 실행 시 정상 종료 확인
- 존재하지 않는 원격을 사용한 실패 시험에서 종료 코드 1, `Push Failed`, 거짓 `Push Complete` 미출력 확인

## 오류가 발생했던 PC의 1회 복구

기존 폴더에는 원격보다 오래된 `main`, 수정된 실행 파일, 삭제된 레시피가 함께 있어 바로 pull하면 충돌할 수 있다. 다음 방법이 가장 안전하다.

1. Visual Studio와 Vision 프로그램을 종료한다.
2. `C:\Users\jwkang01\Downloads\2G_QLD_VisionSW` 폴더를 삭제하지 말고 백업 이름으로 변경한다.
3. 같은 위치에 저장소를 새로 clone한다.

```bat
cd /d C:\Users\jwkang01\Downloads
git clone https://github.com/kjuws10-rgb/2G_QLD_VisionSW.git 2G_QLD_VisionSW
```

4. 백업 폴더에서 실제 반영할 파일만 새 폴더에 복사한다. `.vs`, `obj`, `BIN\LOG`, `BIN\RESULT`는 복사할 필요가 없다.
5. 새 폴더의 `push_file.bat`를 실행한다. 커밋 메시지는 선택적으로 첫 번째 인자로 전달할 수 있다.

```bat
push_file.bat "Update Vision files"
```

## 패치 적용

```powershell
git am PATCH/20260907_git_push_lock_fix/0001-Fix-push-workflow-and-ignore-locked-IDE-files.patch
```

Git 패치를 사용할 수 없는 경우 `files` 폴더의 `.gitignore`와 `push_file.bat`를 저장소 루트에 복사한다. 기존에 추적된 `.vs/obj` 제거까지 반영하려면 Git 패치를 사용하는 것이 권장된다.

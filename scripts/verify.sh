#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

echo "== restore ==";  dotnet restore --locked-mode
echo "== format ==";   dotnet format --verify-no-changes --no-restore
echo "== build ==";    dotnet build -c Release --no-restore -warnaserror
echo "== test ==";     dotnet test -c Release --no-build

echo "== secret scan =="
PATTERNS='(AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{36}|github_pat_[A-Za-z0-9_]{50,}|sk-[A-Za-z0-9]{32,}|xox[baprs]-[A-Za-z0-9-]{10,}|-----BEGIN [A-Z ]*PRIVATE KEY-----|AIza[0-9A-Za-z_-]{35})'
if git ls-files -z | grep -zv -e 'packages.lock.json' | xargs -0 grep -nIE "$PATTERNS" ; then
  echo "SECRET PATTERN FOUND"; exit 1
fi
echo "OK"

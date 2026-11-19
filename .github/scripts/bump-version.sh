#!/usr/bin/env bash
# Podbija <Version> w podanym pliku .csproj. Zastępuje npm-owe dotnettools (dnt):
# wersje >= 2.0 nie mają binarek dla Linuksa, a 1.4.1 nie działa z SDK nowszym niż 3.1
# (i przy błędzie kończy się kodem 0, więc wersja po cichu się nie zmieniała).
#
#   patch   – 1.2.3 -> 1.2.4                (master: wersja commitowana i publikowana)
#   preview – 1.2.3 -> 1.2.4-preview.<N>    (develop / PR-y; N = GITHUB_RUN_NUMBER)
set -euo pipefail

usage="użycie: $0 patch|preview <plik.csproj>"
mode="${1:?$usage}"
project="${2:?$usage}"

current="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$project")"
if [[ ! "$current" =~ ^([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
  echo "::error file=$project::Oczekiwano dokładnie jednego <Version>X.Y.Z</Version>, znaleziono: '$current'" >&2
  exit 1
fi
next="${BASH_REMATCH[1]}.${BASH_REMATCH[2]}.$((BASH_REMATCH[3] + 1))"

case "$mode" in
  patch)   new="$next" ;;
  preview) new="$next-preview.${GITHUB_RUN_NUMBER:-0}" ;;
  *)       echo "$usage" >&2; exit 1 ;;
esac

sed -i "s:<Version>$current</Version>:<Version>$new</Version>:" "$project"
grep -q "<Version>$new</Version>" "$project"
echo "Bumped $(basename "$project") from $current to $new"

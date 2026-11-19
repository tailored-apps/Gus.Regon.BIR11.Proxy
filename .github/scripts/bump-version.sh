#!/usr/bin/env bash
# Wylicza wersję do zbudowania i wpisuje ją do <Version> w podanym .csproj (tylko w kopii
# roboczej CI, bez commita: master jest chroniony, a bot nie może na niego pushować).
#
# Baza = większa z dwóch: najwyższy tag vX.Y.Z albo <Version> z csproj.
#   - csproj > ostatni tag (ręczne podbicie minor/major w PR) -> wersja z csproj, np. 0.2.0
#   - w pozostałych przypadkach                              -> ostatni tag + 1 w patch
#
#   patch   – wersja wydania, np. v0.1.6 -> 0.1.7          (release.yml, potem tag v0.1.7)
#   preview – podgląd następnego wydania, 0.1.7-preview.<N> (develop / PR-y; N = GITHUB_RUN_NUMBER)
#
# Wymaga tagów w checkoutcie (actions/checkout z fetch-depth: 0).
# W GitHub Actions wersja trafia też do outputu kroku: steps.<id>.outputs.version
set -euo pipefail

usage="użycie: $0 patch|preview <plik.csproj>"
mode="${1:?$usage}"
project="${2:?$usage}"
semver='^([0-9]+)\.([0-9]+)\.([0-9]+)$'

csproj="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$project")"
if [[ ! "$csproj" =~ $semver ]]; then
  echo "::error file=$project::Oczekiwano dokładnie jednego <Version>X.Y.Z</Version>, znaleziono: '$csproj'" >&2
  exit 1
fi

tag="$(git tag --list 'v*' | sed -n 's/^v//p' | grep -E "$semver" | sort -V | tail -n 1 || true)"

if [[ -z "$tag" ]] || [[ "$csproj" != "$tag" && "$(printf '%s\n' "$csproj" "$tag" | sort -V | tail -n 1)" == "$csproj" ]]; then
  release="$csproj"
  source="csproj"
else
  [[ "$tag" =~ $semver ]]
  release="${BASH_REMATCH[1]}.${BASH_REMATCH[2]}.$((BASH_REMATCH[3] + 1))"
  source="tag v$tag"
fi

case "$mode" in
  patch)   new="$release" ;;
  preview) new="$release-preview.${GITHUB_RUN_NUMBER:-0}" ;;
  *)       echo "$usage" >&2; exit 1 ;;
esac

sed -i "s:<Version>$csproj</Version>:<Version>$new</Version>:" "$project"
grep -q "<Version>$new</Version>" "$project"
if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  echo "version=$new" >> "$GITHUB_OUTPUT"
fi
echo "Version $new (baza: $source; csproj: $csproj, ostatni tag: ${tag:-brak})"

#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$(realpath "$0")")"
if [ "$#" -eq 0 ]; then
  echo "Usage: ./Setup-Linux.sh --revive-path /path/to/Revive101 [--seed SEED] [--check-only]" >&2
  exit 2
fi
exec python3 setup_linux.py "$@"

#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$(realpath "$0")")"
exec python3 linux_ctl.py start

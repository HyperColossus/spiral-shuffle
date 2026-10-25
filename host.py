"""Serve only the game patch directories on the configured interface."""
import argparse
import mimetypes
import re
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import unquote, urlsplit

from profile_paths import PATCH, base_version


def resolve_file(url: str, roots: tuple[Path, ...]) -> Path | None:
    relative = unquote(urlsplit(url).path).lstrip("/")
    if ".." in Path(relative).parts or "\\" in relative or ":" in relative:
        return None
    for root in roots:
        candidate = (root / relative).resolve()
        if candidate.is_relative_to(root.resolve()) and candidate.is_file():
            return candidate
    return None


class PatchHandler(BaseHTTPRequestHandler):
    roots: tuple[Path, ...] = ()
    server_version = "SpiralShuffle/1.0"

    def do_HEAD(self) -> None:
        self.serve_file(False)

    def do_GET(self) -> None:
        self.serve_file(True)

    def serve_file(self, include_body: bool) -> None:
        file = resolve_file(self.path, self.roots)
        if file is None:
            self.send_error(404)
            return
        size = file.stat().st_size
        first, last = 0, size - 1
        request_range = self.headers.get("Range")
        if request_range:
            match = re.fullmatch(r"bytes=(\d+)-(\d*)", request_range)
            if not match:
                self.send_error(416)
                return
            first = int(match[1])
            last = min(int(match[2]) if match[2] else last, last)
            if first > last:
                self.send_error(416)
                return
        self.send_response(206 if request_range else 200)
        self.send_header("Content-Type", mimetypes.guess_type(file.name)[0] or "application/octet-stream")
        self.send_header("Content-Length", str(max(0, last - first + 1)))
        self.send_header("Accept-Ranges", "bytes")
        if request_range:
            self.send_header("Content-Range", f"bytes {first}-{last}/{size}")
        self.end_headers()
        if include_body:
            try:
                with file.open("rb") as stream:
                    stream.seek(first)
                    remaining = last - first + 1
                    while remaining > 0:
                        chunk = stream.read(min(1024 * 1024, remaining))
                        if not chunk:
                            break
                        self.wfile.write(chunk)
                        remaining -= len(chunk)
            except (ConnectionResetError, BrokenPipeError, ConnectionAbortedError):
                return


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default="127.0.0.1")
    parser.add_argument("--port", type=int, default=12379)
    args = parser.parse_args()
    PatchHandler.roots = (PATCH, base_version().parent)
    ThreadingHTTPServer((args.host, args.port), PatchHandler).serve_forever()


if __name__ == "__main__":
    main()

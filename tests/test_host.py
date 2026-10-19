"""Check patch downloads and keep local files outside the served roots."""
import tempfile
import threading
import unittest
import urllib.error
import urllib.request
from http.server import ThreadingHTTPServer
from pathlib import Path

from host import PatchHandler, resolve_file


class PatchServerTests(unittest.TestCase):
    def test_ranges_and_private_paths(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            allowed = root / "patch"
            allowed.mkdir()
            (allowed / "Root.wad").write_bytes(b"KIWAD-example")
            (root / "login.txt").write_text("private sentinel", encoding="utf-8")
            handler = type("TestHandler", (PatchHandler,), {"roots": (allowed,)})
            server = ThreadingHTTPServer(("127.0.0.1", 0), handler)
            thread = threading.Thread(target=server.serve_forever, daemon=True)
            thread.start()
            base = f"http://127.0.0.1:{server.server_address[1]}"
            try:
                request = urllib.request.Request(base + "/Root.wad", headers={"Range": "bytes=0-4"})
                with urllib.request.urlopen(request) as response:
                    self.assertEqual(response.status, 206)
                    self.assertEqual(response.read(), b"KIWAD")
                request = urllib.request.Request(base + "/Root.wad", method="HEAD")
                with urllib.request.urlopen(request) as response:
                    self.assertEqual(int(response.headers["Content-Length"]), 13)
                    self.assertEqual(response.read(), b"")
                for path in ["/../login.txt", "/%2e%2e/login.txt", "/..%5clogin.txt", "/C:/login.txt", "/login.txt"]:
                    with self.assertRaises(urllib.error.HTTPError) as caught:
                        urllib.request.urlopen(base + path)
                    self.assertEqual(caught.exception.code, 404)
                request = urllib.request.Request(base + "/Root.wad", headers={"Range": "bytes=999-"})
                with self.assertRaises(urllib.error.HTTPError) as caught:
                    urllib.request.urlopen(request)
                self.assertEqual(caught.exception.code, 416)
                self.assertIsNone(resolve_file("/%2e%2e/login.txt", (allowed,)))
            finally:
                server.shutdown()
                server.server_close()
                thread.join()


if __name__ == "__main__":
    unittest.main()

"""Offline checks; never touch a live server or Docker daemon."""
from pathlib import Path
from tempfile import TemporaryDirectory
from unittest import TestCase, mock
import importlib.util

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("spiral_docker_config", ROOT / "docker/db_config.py")
db = importlib.util.module_from_spec(spec)
spec.loader.exec_module(db)

CONFIG_TEXT = """[Database]
PlayerDatabaseUrl =
PlayerDatabaseName = Playerdata
PlayerDatabaseCertificatePath =
AdminAccountUsername = admin
AdminAccountPassword = unprinted-secret-value
"""

class DockerDBTests(TestCase):
    def test_only_requested_config_keys_change(self):
        newer = db.replace_setting(CONFIG_TEXT, "PlayerDatabaseUrl", db.DB_URL)
        newer = db.replace_setting(newer, "PlayerDatabaseCertificatePath", "/abs/no-cert.pfx")
        self.assertIn("PlayerDatabaseUrl = http://127.0.0.1:18080", newer)
        self.assertIn("PlayerDatabaseCertificatePath = /abs/no-cert.pfx", newer)
        self.assertIn("AdminAccountPassword = unprinted-secret-value", newer)

    def test_config_without_expected_key_refused(self):
        with self.assertRaises(ValueError):
            db.replace_setting("PlayerDatabaseUrlx = ignored", "PlayerDatabaseUrl", "new")
        with self.assertRaises(ValueError):
            db.get_setting("PlayerDatabaseUrl = x\nPlayerDatabaseUrl = y", "PlayerDatabaseUrl")

    def test_refuses_docker_switch_when_game_running(self):
        import sys
        sys.path.insert(0, str(ROOT))
        import linux_ctl
        with mock.patch.object(linux_ctl, "owned_process", return_value=123):
            with self.assertRaisesRegex(RuntimeError, "Stop Spiral Shuffle"):
                db.switch()

    def test_switch_preserves_source_file_and_makes_private_backup(self):
        with TemporaryDirectory() as folder:
            base = Path(folder)
            config = base / "Imlight.ini"
            config.write_text(CONFIG_TEXT)
            no_cert = base / "not-present.pfx"
            import sys
            sys.path.insert(0, str(ROOT))
            import linux_ctl
            with mock.patch.object(db, "CONFIG", config), \
                 mock.patch.object(db, "NO_CERT", no_cert), \
                 mock.patch.object(db, "check_destination"), \
                 mock.patch.object(linux_ctl, "owned_process", return_value=None):
                db.switch()
            saved = config.read_text()
            self.assertIn(db.DB_URL, saved)
            self.assertIn("unprinted-secret-value", saved)
            backups = list(base.glob("Imlight.ini.before-docker.*"))
            self.assertEqual(len(backups), 1)
            self.assertEqual(backups[0].read_text(), CONFIG_TEXT)
            self.assertEqual(backups[0].stat().st_mode & 0o777, 0o600)

    def test_compose_unsecured_mode_stays_host_loopback_only(self):
        compose = (ROOT / "docker/ravendb.compose.yml").read_text()
        # The 0.0.0.0 bind is INSIDE the container. The published host port
        # MUST remain loopback-only because RavenDB HTTP has no auth here.
        self.assertIn('RAVEN_Security_UnsecuredAccessAllowed: "PublicNetwork"', compose)
        self.assertIn('"127.0.0.1:18080:8080"', compose)
        self.assertNotIn('"0.0.0.0:18080:8080"', compose)
        self.assertNotIn('"18080:8080"', compose)

    def test_import_not_performed_by_script(self):
        source = (ROOT / "docker/db_config.py").read_text()
        self.assertNotIn("docker rm", source)
        self.assertNotIn("docker compose down", source)
        self.assertNotIn("shutil.rmtree", source)

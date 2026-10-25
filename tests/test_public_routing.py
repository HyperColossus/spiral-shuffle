import os
import unittest
from unittest import mock

import linux_ctl as ctl


class PatchBindingTests(unittest.TestCase):
    def test_public_game_ip_does_not_have_to_be_a_local_interface(self):
        with mock.patch.object(ctl, 'configured_game_ip', return_value='70.23.164.208'), \
                mock.patch.dict(os.environ, {'SPIRAL_PATCH_BIND_IP': '0.0.0.0'}):
            self.assertEqual(ctl.configured_patch_binding(), ('0.0.0.0', '127.0.0.1'))

    def test_existing_private_binding_is_preserved_without_override(self):
        with mock.patch.object(ctl, 'configured_game_ip', return_value='100.91.208.95'), \
                mock.patch.dict(os.environ, {'SPIRAL_PATCH_BIND_IP': ''}):
            self.assertEqual(ctl.configured_patch_binding(), ('100.91.208.95', '100.91.208.95'))

    def test_invalid_binding_is_rejected(self):
        for value in ('not-an-ip', '::1'):
            with self.subTest(value=value), mock.patch.dict(os.environ, {'SPIRAL_PATCH_BIND_IP': value}):
                with self.assertRaises(ValueError):
                    ctl.configured_patch_binding()

# Free Crown-priced items

The asset builder sets every explicit item `m_creditsCost` to zero, including items
outside randomized equipment. Gold, arena and other currency prices remain intact.
The original BIND fields are retained, including item behaviors that the current
Imcodec decoder does not understand. No commercial assets are committed.

To stage an update to an already customized private patch:

```sh
python3 build_crown_patch.py .runtime/patch/V_r806919.Wizard_1_610 .runtime/crown-stage-output
```

This runs regression checks, creates an item-only override archive, rebuilds Root.wad
and both patch manifests, and compares every asset with the previous patch. It fails
if any explicit Crown item price remains nonzero or any other item field changes.
It does not install the patch or modify accounts, saves, configuration or assemblies.

Installation requires a coordinated game/patch service restart and a backup of the
previous Root.wad and both manifests. Game asset changes intentionally remain outside
the automatic portal deployment scope. Existing clients need to patch again.

This changes item template prices. It does not implement missing Crown Shop catalog
or purchase services in upstream Imlight; a zero template price alone does not prove
that an unsupported shop transaction works.

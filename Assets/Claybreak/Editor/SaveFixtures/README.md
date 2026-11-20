# Save fixtures (RT-5, RT-6)

Real-format saves the save validator (`ClaybreakValidation`) and the tests (`SaveMigrationTests`) read.

- `v1-*.json`: exactly what the RT-4 slice wrote (`SaveData` version 1, `JsonUtility` field order). They
  must migrate to the current version and pass `SaveValidator` after `Progression` normalizes them.
- `v2-*.json`: the RT-5 format (world 1 with 7 levels); `v2-unknown-ids` has the problems a catalog change can
  cause (removed ids, steps out of range); `v2-world1-at-boss` must migrate to version 3 (world 1 with 10
  levels: the boss stars move to the 10th level).
- `v3-*.json`: current format (RT-6), four worlds.
- `damaged-*.json`: must be refused (fresh save, no crash).

Never edit a `v1-*` or `v2-*` file to make a test pass: it stands for a save already on a phone.

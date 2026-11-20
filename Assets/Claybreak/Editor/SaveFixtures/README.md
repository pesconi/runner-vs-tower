# Save fixtures (RT-5)

Real-format saves the save validator (`ClaybreakValidation`) and the tests (`SaveMigrationTests`) read.

- `v1-*.json`: exactly what the RT-4 slice wrote (`SaveData` version 1, `JsonUtility` field order). They
  must migrate to version 2 and pass `SaveValidator` after `Progression` normalizes them.
- `v2-*.json`: current format with problems a catalog change can cause (removed ids, steps out of range).
- `damaged-*.json`: must be refused (fresh save, no crash).

Never edit a `v1-*` file to make a test pass: it stands for a save already on a phone.

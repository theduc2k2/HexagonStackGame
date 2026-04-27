# ECS Migration Notes

This folder is reserved for DOTS/ECS migration.

Suggested order:
1. Create data-only components in `Components`.
2. Add authoring MonoBehaviours in `Authoring`.
3. Add Bakers in `Bakers`.
4. Implement systems in `Systems`.
5. Keep bridge scripts in `Bridges` for classic MonoBehaviour interop.

Do not move all gameplay at once. Migrate one use case at a time (spawn, merge check, scoring).

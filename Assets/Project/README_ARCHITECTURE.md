# Project Architecture (SOLID -> ECS)

This folder structure is prepared for gradual migration.

Layer dependency rules:
- Presentation -> Application -> Core
- Infrastructure -> Application + Core
- Core must not depend on UnityEngine gameplay APIs.
- ECS code stays isolated under `90_ECS` and can bridge from Presentation.

Migration approach:
1. Keep existing scripts under `Scenes/Scripts` running.
2. Introduce interfaces/use cases in `20_Application`.
3. Move concrete adapters into `30_Infrastructure`.
4. Replace logic in old MonoBehaviours step-by-step.
5. Move performance-critical flows to `90_ECS` via bridge classes.

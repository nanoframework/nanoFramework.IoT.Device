# AI-Native IoT: Automatic MCP Generation from Device Metadata
### Microsoft Hackathon 2026 — .NET nanoFramework

Team: Laurent · Jose · Ignacio · Walter

---

## The Problem

- .NET nanoFramework already supports MCP in the `WebServer`, letting AI agents (Copilot, Claude, Semantic Kernel) interact with embedded devices
- But today it requires **manually decorating** every method with `[McpServerTool]` and registering everything by hand
- Meanwhile, hundreds of device bindings already carry rich metadata via `System.Device.Model` (`Telemetry`, `Property`, `Command`, `Component`) — used only for documentation/README today

**Duplicated effort: the metadata already exists, but isn't leveraged for AI.**

---

## The Idea

```
Device Driver
   +
System.Device.Model Metadata
   ↓
Automatic MCP Generation
   ↓
AI-Discoverable Device
```

Reflection over existing metadata → automatic generation of MCP Tools, Resources, Skills Discovery manifest, and Agent Cards — **no new annotations, no hand-written wrappers**.

---

## Protocol Strategy

- **MCP is the primary hackathon target** — Tools + Resources, concrete and demoable
- **Skills Discovery + Agent Card explored in parallel from day one**, not bolted on at the end — designing the capability model to serve both forces a clean, protocol-agnostic core
- Longer-term generalization: the same capability model could drive renderers beyond MCP — e.g. **Home Assistant (HASS)** entities. MCP is the first renderer, not the only one.

---

## Technical Scope

- **No firmware work.** 100% C#, library living inside `nanoFramework.IoT.Device` (new module)
- Built on three blocks:
  1. **Reflection engine** — reads `[Interface]`, `[Telemetry]`, `[Property]`, `[Command]`, `[Component]` and builds a protocol-agnostic capability model
  2. **MCP renderer** — maps the model to Tools/Resources and integrates with `nanoFramework.WebServer.Mcp`
  3. **Skills renderer** — produces the Skills Discovery manifest + Agent Card JSON, in parallel with (2)

---

## Mapping — the details

| Attribute | Where it appears | Key constraints | MCP mapping |
|---|---|---|---|
| `[Interface]` | Class-level | Inheritance-aware; requires a display name | Defines the device's namespace/grouping — not a leaf primitive itself |
| `[Telemetry]` | Property (no args) / method (no args, returns value) / method (returns bool, one `out` arg) | No duplicate names per Interface; non-typed-unit values need a `displayName` | MCP **Resource** (read-only) |
| `[Property]` read-only | Property getter / no-arg method returning value | — | MCP **Resource** |
| `[Property]` writable | Get/set pair, or getter+setter methods merged by matching name | Setter method takes exactly 1 non-ref argument; no duplicate writers/readers per name | MCP **Resource** (current value) *and* MCP **Tool** (to set it) — open question, confirm at kickoff |
| `[Command]` | Method only | Can take **multiple** parameters (unlike Property setters) | MCP **Tool** — multi-param commands packed into a single complex-object argument (WebServer.Mcp only accepts 0/1 param) |
| `[Component]` | Property only, referencing another `[Interface]`-annotated type | Enables nesting/composition of sub-devices | Recursively flattened into the parent's MCP surface — the actual mechanism multi-device aggregation builds on |
| Type serialization | Applies to Telemetry/Property/Command types | Only enums (no `Flags`), UnitsNet units, basic C# types, `Vector2/3/4`, `System.Drawing.Color` | Generator needs an explicit type → MCP/JSON Schema mapping table |

---

## Testing Strategy

- Build a small **synthetic/fake device binding** — annotated with `System.Device.Model` attributes covering every case (multiple telemetry forms, writable property, multi-parameter command, nested component)
- Used to unit-test the reflection engine and both renderers **without needing real hardware** — decouples Core Engine work from ESP32 flashing, unblocks parallel work from day 1
- Real hardware (2 bindings: sensor + actuator) still used for the final end-to-end demo

---

## Team & Responsibilities

| Person | Workstream |
|---|---|
|  | Core Engine — reflection over metadata, capability model, synthetic test binding |
|  | MCP renderer — mapping → `WebServer.Mcp` |
|  | Skills renderer — Skills Discovery manifest + Agent Card, demo hardware |
|  | Multi-device aggregation, end-to-end AI client connection, testing |

---

## Week Plan

*(assumes Mon–Fri — confirm at kickoff)*

| Day | Focus |
|---|---|
| **Mon** | Kickoff, architecture, reflection spike, pick demo devices, build the synthetic test binding |
| **Tue** | Core engine MVP against the synthetic binding · MCP mapping schema · Skills manifest draft (parallel) |
| **Wed** | First end-to-end on real hardware: tools/resources generated and called by a real MCP client |
| **Thu** | Multi-device aggregation via Component flattening · Skills Discovery/Agent Card wired in · tests with a real agent |
| **Fri** | Code freeze · 1-min video · README · rehearsal · submission |

---

## Definition of Done (demo)

At least **2 existing bindings**, with zero changes to their code, automatically expose Telemetry/Commands as MCP Resources/Tools — discovered and used by a real AI agent, with no hand-written wrapper.

---

## Known Risks & Constraints

- nanoFramework reflection is limited — validate on Day 1 what's actually readable at runtime
- `WebServer.Mcp` supports *server features* only (no notifications/SSE)
- MCP tools accept at most 1 parameter → multi-param Commands must be packed into a complex object
- Writable Properties as both Resource and Tool — decide the convention before implementation starts
- `System.Device.Model` is experimental — subject to change

---

## Next Steps (today)

1. Finalize the task split with the group
2. Reflection spike — confirm core technical feasibility
3. Pick the 2 demo bindings (sensor + actuator)
4. Build the synthetic test binding

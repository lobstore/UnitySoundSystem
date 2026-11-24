# Changelog

All notable changes to this package are documented here. The format follows Keep a Changelog and this project uses Semantic Versioning.

## [Unreleased]

### Added
- XML documentation for public runtime types, methods, properties, and serialized sound settings.
- Structured API contract for AI agents.
- Complete human-readable manual and a Quick Start PDF with a Unity usage example.

### Changed
- Initialize the emitter pool lazily so playback does not depend on Start execution order.
- Track active emitters by identity and return them to the manager that rented them.
- Apply the pitch-offset range stored in SoundData; explicit builder settings override it.
- Clamp logarithmic conversion inputs to their documented normalized ranges.

### Fixed
- Handle a zero frequent-sound limit without accessing an empty linked list.
- Guard against null clips, invalid emitter prefabs, and duplicate pool returns.
- Remove frequent-sound list nodes when emitters are returned to the pool.

## [0.1.0] - 2026-11-22

### Added
- Initial release of LOGIYGames Audio.

# Change Log

## Current Version

v2.0.x

- Migrate from a static class
- Allow users to add their own default `JsonSerializerOptions`
- Allow users to add and manage their own default list of `JsonConverter` objects
- Add Touchstone-based test infrastructure: `Test.Shared` (runner-agnostic test descriptors, the single source of truth), `Test.Automated` (Touchstone CLI runner), `Test.Xunit` (xUnit adapter), and `Test.Nunit` (NUnit adapter), with exhaustive positive and negative coverage of the serializer and all bundled converters

## Previous Versions

v1.0.x

- Initial release

# FrabJouS
A Fast, source-generated, Json Serializer for dotnet.

# Overview
This is a fast serializer for generating Json representation of objects, based on source-generated code which generates code on par with what a developer would write by hand.

# Goals
1. Enable fast serialization of objects.
1. Avoid reflection and virtual dispatch within the generated code.
1. Allow a certain level of customization for generated Json.
1. Generate fully trimmable serialization code.
1. Support all languages for which source generators are available.

# Non-goals
1. Complete feature parity with Newtonsoft.Json and System.Text.Json.
1. Runtime customization of serialization code.

# Planned Feature Support
- [ ] Attribute name customization
- [ ] Attribute naming convention customization
    - [ ] snake case
    - [ ] kebab case
    - [ ] camel case
    - [ ] pascal case
- [ ] Polymorphic serialization
- [ ] Ignore property or type
- [ ] Field serialization
- [ ] Cyclic references
    - [ ] Type cycles
    - [ ] Object cycles
- [ ] Object converters
- [ ] Nullable value types
- [ ] Collection, sequences, spans, immutable collections
    - [ ] Arrays
    - [ ] List<T>
    - [ ] Dictionary<Key, Value>
    - [ ] Primitive objects as element
    - [ ] Complex objects as element
    - [ ] Complex objects as value
    - [ ] [Readonly]Span<T>
- [ ] Complex properties
- [ ] Primitive types
    - [ ] byte
    - [ ] sbyte
    - [ ] short
    - [ ] ushort
    - [ ] int
    - [ ] uint
    - [ ] long
    - [ ] ulong
    - [ ] float
    - [ ] double
    - [ ] string
    - [ ] Guid
    - [ ] Version
    - [ ] Timespace
    - [ ] DateTime
    - [ ] DateOnly
    - [ ] TimeOnly
    - [ ] char
    - [ ] Rune
- [ ] Test cases
- [ ] Build pipeline
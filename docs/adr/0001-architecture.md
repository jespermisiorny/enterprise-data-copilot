# ADR 0001 – Arkitektur

Vi använder Clean-ish layering:
- Domain: typer och regler (ingen IO)
- Application: orkestrering/use cases (ingen IO)
- Infrastructure: IO (audit store, data access, knowledge reading)
- Api: endpoints + DI

Syfte: spårbarhet, testbarhet, möjlighet att byta provider.

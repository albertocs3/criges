# ADR-0018: Aislar VeriFactu detrás de un adaptador versionado

## Estado

Aceptada.

## Contexto

VeriFactu es un requisito legal relevante para facturación, pero sus detalles definitivos pueden cambiar. La integración afectará emisión, conservación, certificados, errores, reintentos y trazabilidad.

## Decisión

VeriFactu se implementará detrás de un adaptador aislado, con contratos versionados, Outbox, idempotencia y conservación de solicitud/respuesta.

## Alternativas consideradas

- Integrar llamadas directamente en facturación: acopla reglas legales cambiantes al dominio principal.
- Posponer toda estructura de integración: reduce trabajo inicial, pero dificulta diseñar emisión e inmutabilidad.
- Usar una dependencia externa sin encapsular: rápido, pero arriesga dependencia tecnológica fuerte.

## Consecuencias

- Facturación dependerá de un puerto, no de detalles técnicos de AEAT.
- Los cambios normativos podrán concentrarse en el adaptador y contratos.
- Las pruebas usarán dobles del adaptador.
- La validación legal definitiva seguirá pendiente hasta confirmar requisitos vigentes.


# ADR-0001: Usar monolito modular

## Estado

Aceptada.

## Contexto

CriGes integra clientes, suscripciones, facturación, contabilidad, tesorería, incidencias, catálogo y plataforma. Varias operaciones críticas requieren consistencia inmediata: emisión de facturas, generación de asientos, movimientos de IVA, cobros, stock y rectificaciones.

El producto se desplegará inicialmente en una única empresa, con aplicación de escritorio Windows y servidor central.

## Decisión

CriGes se implementará como un monolito modular.

Habrá un único despliegue servidor principal, pero el código se dividirá por módulos con límites explícitos, contratos propios, persistencia encapsulada y reglas de dependencia verificables.

## Alternativas consideradas

- Microservicios: aportan independencia de despliegue, pero aumentan complejidad operativa, transacciones distribuidas y observabilidad.
- Monolito por capas clásico: simplifica el inicio, pero tiende a mezclar responsabilidades funcionales.
- Aplicación de escritorio con acceso directo a base de datos: se descarta por seguridad, concurrencia y control de reglas.

## Consecuencias

- Las transacciones económicas pueden mantenerse ACID dentro de SQL Server.
- El despliegue inicial es más sencillo.
- Se exige disciplina de límites internos para no convertirlo en un monolito acoplado.
- Las pruebas de arquitectura deben impedir dependencias indebidas entre módulos.
- Una separación futura por servicios será posible solo si los contratos y eventos se mantienen limpios desde el principio.


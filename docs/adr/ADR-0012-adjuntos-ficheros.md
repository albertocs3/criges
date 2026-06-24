# ADR-0012: Guardar adjuntos en repositorio protegido y metadatos en SQL Server

## Estado

Aceptada.

## Contexto

El sistema manejará adjuntos en incidencias, gastos, configuración y otros módulos. Se han definido límites de tamaño, extensiones, conservación, antivirus y trazabilidad.

## Decisión

Los archivos se guardarán en un repositorio protegido del servidor, fuera de la raíz pública.

SQL Server almacenará metadatos, hash, estado, permisos, relación funcional y trazabilidad.

## Alternativas consideradas

- Guardar binarios en base de datos: simplifica consistencia, pero puede penalizar tamaño, backup y rendimiento.
- Guardar rutas directas visibles: inseguro y difícil de controlar.
- Almacenamiento de objetos desde el inicio: viable, pero innecesario para el primer despliegue.

## Consecuencias

- Las copias deben coordinar base de datos y repositorio de archivos.
- La API será la única vía de descarga.
- La subida tendrá cuarentena, validación de tipo real, SHA-256 y antivirus.
- La interfaz `IFileStorage` permitirá migrar a otro almacenamiento en el futuro.


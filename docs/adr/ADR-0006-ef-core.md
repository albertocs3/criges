# ADR-0006: Usar Entity Framework Core con SQL parametrizado cuando sea necesario

## Estado

Aceptada.

## Contexto

La aplicación tendrá un modelo de dominio rico en Plataforma y módulos económicos, pero también consultas complejas para listados, auditoría, contabilidad y conciliación.

## Decisión

Se usará Entity Framework Core 8 como ORM principal.

Cuando una consulta o una operación masiva sea más clara, eficiente o segura en SQL, se usará SQL parametrizado dentro de la capa Infrastructure.

## Alternativas consideradas

- Solo SQL manual: aporta control, pero aumenta código repetitivo y riesgo de inconsistencias.
- Micro ORM puro: adecuado para consultas, menos expresivo para agregados y cambios.
- EF Core exclusivo sin SQL explícito: simple al inicio, pero puede forzar consultas ineficientes.

## Consecuencias

- Los agregados se persisten con configuraciones Fluent API.
- Las consultas de lectura podrán usar proyecciones específicas.
- No se expondrá `DbContext` fuera de la infraestructura del módulo.
- Las consultas SQL deberán estar parametrizadas, probadas y localizadas en Infrastructure.


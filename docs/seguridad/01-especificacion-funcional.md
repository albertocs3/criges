# Especificación funcional: Usuarios, Roles, Permisos, Auditoría y Sesiones

## 0. Contexto

Este módulo proporciona identidad, autorización y trazabilidad a todo el software de gestión.

Se aplica a:

- Clientes y tiendas.
- Catálogo e inventario.
- Suscripciones.
- Facturación.
- Atención al cliente e incidencias.
- Contabilidad, compras y proveedores.
- Tesorería y SEPA.
- Configuración general.

Los usuarios son exclusivamente empleados internos.

## 1. Propósito

El módulo permitirá:

- Autenticar usuarios mediante nombre de usuario y contraseña.
- Mantener usuarios activos, bloqueados o desactivados.
- Asignar un único rol a cada usuario.
- Administrar roles personalizados.
- Autorizar acciones por módulo.
- Restringir el acceso a información económica y sensible.
- Controlar sesiones.
- Auditar acciones, consultas y accesos.
- Alertar sobre eventos de seguridad.

## 2. Alcance

### Incluido

- Usuarios internos.
- Acceso mediante usuario y contraseña.
- Roles base protegidos.
- Roles personalizados.
- Matriz de permisos.
- Un único rol por usuario.
- Una única sesión simultánea.
- Bloqueo temporal por intentos fallidos.
- Caducidad por inactividad.
- Cierre remoto de sesiones.
- Auditoría central e inmutable.
- Alertas internas de seguridad.

### Fuera de alcance

- Usuarios externos o portal de clientes.
- Autenticación mediante Microsoft, Google o certificado.
- Segundo factor de autenticación.
- Varios roles simultáneos.
- Permisos asignados directamente a usuarios.
- Restricciones por cliente o registro concreto.
- Simulación o suplantación de roles.
- Caducidad periódica de contraseñas.
- Prohibición de reutilizar contraseñas anteriores.

## 3. Usuarios

### Datos

- Identificador.
- Nombre y apellidos.
- Nombre de usuario.
- Teléfono.
- Rol.
- Estado.
- Fecha de alta.
- Fecha de baja o desactivación.
- Último acceso correcto.
- Fecha del último cambio de contraseña.
- Intentos fallidos.
- Fecha hasta la que permanece bloqueado.

### Reglas

- El nombre de usuario es único.
- Un nombre utilizado no puede reutilizarse después de una baja.
- Los usuarios nunca se eliminan.
- Un usuario con actividad histórica conserva su identidad en la auditoría.
- Cada usuario tiene exactamente un rol activo para acceder.

## 4. Estados de usuario

- Activo.
- Bloqueado.
- Desactivado.

### Activo

Puede iniciar sesión si su rol también está activo.

### Bloqueado

No puede iniciar sesión hasta:

- Finalizar el bloqueo temporal.
- Ser desbloqueado por un administrador.

### Desactivado

- No puede iniciar sesión.
- Sus sesiones se cierran inmediatamente.
- Conserva historial y auditoría.
- Puede reactivarse por un administrador.

## 5. Gestión de usuarios

Solo el administrador puede:

- Crear usuarios.
- Modificar sus datos.
- Asignar o cambiar el rol.
- Bloquear y desbloquear.
- Desactivar y reactivar.
- Restablecer contraseñas.
- Cerrar sesiones activas.

Cambiar el rol, bloquear o desactivar al usuario cierra inmediatamente su sesión.

## 6. Contraseñas

### Requisitos

- Mínimo 10 caracteres.
- Al menos una letra mayúscula.
- Al menos una letra minúscula.
- Al menos un número.
- Al menos un carácter especial.

### Reglas

- El usuario puede cambiar su propia contraseña.
- El administrador puede establecer directamente una contraseña nueva.
- No se exige cambiarla en el primer acceso.
- No caduca periódicamente.
- No se controla la reutilización de contraseñas anteriores.
- Nunca se guarda ni registra su contenido.
- Se registra la fecha del cambio.
- Cambiar o restablecer la contraseña cierra todas las sesiones existentes.

El almacenamiento deberá utilizar un algoritmo de hash de contraseñas adecuado y configurable.

## 7. Bloqueo por intentos fallidos

- Se permiten cinco intentos fallidos consecutivos.
- Al quinto intento, la cuenta se bloquea durante 30 minutos.
- Un intento durante el bloqueo reinicia el plazo de 30 minutos.
- El administrador puede desbloquearla antes.
- El administrador recibe una notificación interna.

Cada intento registra:

- Usuario indicado.
- Fecha y hora.
- Dirección IP o identificador de origen disponible.
- Resultado.
- Motivo del rechazo.

## 8. Roles

### Roles base

- Administrador.
- Facturación.
- Contabilidad.
- Técnico.

Los roles base:

- Están protegidos.
- No pueden eliminarse.
- No pueden desactivarse.
- Mantienen su finalidad funcional.

### Roles personalizados

El administrador puede:

- Crear roles.
- Copiar un rol existente.
- Modificar permisos.
- Combinar permisos de distintos módulos.
- Activar o desactivar.

No se permite guardar un rol sin ningún permiso.

Un rol personalizado desactivado bloquea el acceso de todos sus usuarios hasta que se reactive o se les asigne otro.

## 9. Asignación de roles

- Cada usuario tiene un único rol.
- No se asignan permisos directamente a usuarios.
- Cambiar el rol cierra la sesión activa.
- Los cambios de permisos afectan inmediatamente a las siguientes peticiones.
- No se conserva una copia completa de cada versión anterior del rol.
- Sí se audita quién realizó el cambio, cuándo y qué permisos fueron añadidos o retirados.

## 10. Modelo de permisos

Los permisos se organizan por módulo y acción.

Acciones comunes:

- Consultar.
- Crear.
- Modificar.
- Inactivar o cancelar.
- Eliminar, solo donde esté expresamente permitido.
- Emitir o contabilizar.
- Aprobar o procesar.
- Exportar.
- Administrar configuración.
- Ver datos económicos.
- Ver costes y márgenes.

Los roles personalizados pueden combinar permisos de cualquier módulo.

## 11. Aplicación de permisos

- Los módulos sin acceso se ocultan del menú.
- Las acciones no permitidas se ocultan.
- La interfaz no es una medida de seguridad suficiente.
- El servidor o capa de aplicación valida cada operación.
- Los cambios de permisos se aplican de inmediato.
- No existen restricciones por clientes concretos.
- Exportar y descargar adjuntos o PDF no requieren permisos independientes; dependen del permiso de consulta del contenido.

## 12. Roles base

### Administrador

- Acceso completo e irrevocable.
- Gestión de usuarios, roles y permisos.
- Acceso a configuración.
- Acceso a datos económicos, bancarios, costes y márgenes.
- Consulta de auditoría.

### Facturación

Acceso inicial a:

- Clientes y tiendas.
- Catálogo.
- Suscripciones.
- Facturación.
- Cobros y vencimientos.

Puede ver datos económicos y bancarios.

No registra operaciones contables salvo que un rol personalizado le conceda otros permisos.

No puede consultar costes ni márgenes.

### Contabilidad

Acceso inicial a:

- Clientes y tiendas.
- Catálogo.
- Facturación.
- Compras y proveedores.
- Contabilidad.
- Tesorería.

Puede ver datos económicos y bancarios.

No puede consultar costes ni márgenes.

### Técnico

Acceso inicial a:

- Identificación básica de clientes.
- Tiendas.
- Contactos.
- Comunicaciones.
- Incidencias.

No puede ver:

- NIF o VAT.
- Direcciones fiscales.
- IBAN o mandatos.
- Facturas.
- Cobros.
- Datos contables.
- Suscripciones.
- Costes o márgenes.

## 13. Datos económicos y sensibles

### Ver datos económicos

Incluye:

- Precios.
- Facturas.
- Vencimientos.
- Cobros.
- Pagos.
- Saldos.
- Contabilidad.
- Tesorería.

### Ver costes y márgenes

- Es exclusivo del administrador.
- No puede concederse efectivamente a un rol personalizado no administrador.

### Datos bancarios

Los datos completos pueden ser consultados por:

- Administrador.
- Facturación.
- Contabilidad.

No existirá un permiso independiente para mostrar el IBAN parcialmente oculto.

## 14. Incidencias y permisos contextuales

Los permisos generales se combinan con las reglas funcionales de Incidencias:

- Todos los técnicos pueden consultar incidencias.
- Solo el responsable modifica los datos principales.
- Los colaboradores pueden añadir actuaciones y adjuntos.
- Un usuario con permiso de consulta puede añadir comentarios si las reglas de la incidencia se lo permiten.
- El administrador puede realizar cualquier cambio.

Las reglas contextuales no sustituyen la validación del permiso general del módulo.

## 15. Matriz de permisos

El administrador dispondrá de una pantalla con:

- Módulos en filas o grupos.
- Acciones disponibles.
- Roles en columnas o selección.
- Permisos concedidos.

Permitirá:

- Consultar roles base.
- Crear y copiar roles personalizados.
- Modificar permisos.
- Detectar roles sin usuarios.
- Detectar usuarios con rol inactivo.

## 16. Sesiones

### Reglas

- Un usuario solo puede mantener una sesión activa.
- Un nuevo inicio de sesión se rechaza si ya existe otra sesión.
- El usuario puede cerrar sesión manualmente.
- El administrador puede cerrarla remotamente.
- La sesión caduca tras cinco horas sin actividad.
- El tiempo de inactividad es configurable por el administrador.
- Se muestra un aviso cinco minutos antes de caducar.
- La actividad válida con el servidor renueva la última actividad.

### Datos de sesión

- Usuario.
- Fecha y hora de inicio.
- Última actividad.
- Dirección IP o identificador de origen.
- Aplicación o dispositivo, cuando sea posible.
- Estado.
- Motivo de cierre.

## 17. Cierre de sesiones

Una sesión se cierra por:

- Cierre manual.
- Caducidad.
- Cambio o restablecimiento de contraseña.
- Cambio de rol.
- Bloqueo.
- Desactivación.
- Desactivación del rol.
- Acción remota del administrador.

El cierre debe invalidar inmediatamente las credenciales de sesión.

## 18. Auditoría central

La auditoría es común e inmutable para todos los módulos.

Cada evento incluirá:

- Fecha y hora.
- Usuario.
- Dirección IP o identificador de origen.
- Módulo.
- Acción.
- Entidad.
- Identificador.
- Resultado.
- Valor anterior.
- Valor nuevo.
- Motivo.
- Descripción legible.
- Proceso de origen, cuando sea automático.

## 19. Eventos auditados

Se auditarán:

- Accesos correctos y fallidos.
- Cierres de sesión.
- Bloqueos y desbloqueos.
- Cambios y restablecimientos de contraseña.
- Creación, modificación y desactivación de usuarios.
- Cambios de rol.
- Cambios de permisos.
- Acciones denegadas.
- Consultas de datos sensibles.
- Consultas de IBAN.
- Consultas contables.
- Consultas de costes y márgenes.
- Exportaciones.
- Descargas.
- Operaciones funcionales relevantes de todos los módulos.
- Procesos automáticos.

Las tareas automáticas se identifican mediante el usuario técnico `Sistema` e indican el proceso que las originó.

## 20. Motivos obligatorios

Se exigirá motivo en acciones sensibles, incluyendo:

- Reabrir un ejercicio.
- Ignorar un límite de crédito.
- Fusionar clientes o proveedores.
- Modificar un asiento.
- Deshacer una conciliación.
- Desactivar un usuario.
- Desactivar un rol con usuarios.
- Forzar un desbloqueo.
- Cerrar remotamente una sesión, cuando proceda.

## 21. Consulta de auditoría

Solo el administrador puede consultar la auditoría.

Filtros:

- Usuario.
- Fecha desde y hasta.
- Módulo.
- Acción.
- Entidad.
- Resultado.
- Dirección IP.

La auditoría podrá exportarse.

Sus registros:

- No se modifican.
- No se eliminan desde la aplicación.
- Se conservan durante el plazo legal y de seguridad aplicable.

## 22. Alertas de seguridad

El administrador recibirá alertas internas por:

- Bloqueo de cuentas.
- Intentos repetidos de acceso.
- Acciones denegadas reiteradas.
- Intentos de acceder a datos sensibles sin permiso.
- Cierre anómalo o invalidación de sesiones, cuando proceda.

Las alertas enlazarán con los eventos de auditoría relacionados.

## 23. Seguridad técnica

Aunque la interfaz sea una aplicación de escritorio, deberán protegerse:

- Transporte entre aplicación y servidor.
- Credenciales y tokens de sesión.
- Almacenamiento local de secretos.
- Invalidación de sesiones.
- Autorización en servidor.
- Contraseñas mediante hash seguro.
- Datos sensibles en registros y errores.

La protección CSRF será obligatoria únicamente si la arquitectura utiliza navegador o autenticación basada en cookies susceptible a este ataque.

Si se utiliza una API con tokens fuera de cookies, deberán aplicarse los controles equivalentes apropiados para ese modelo.

## 24. Pantallas mínimas

- Listado de usuarios.
- Alta y edición de usuario.
- Bloqueo, desactivación y reactivación.
- Restablecimiento de contraseña.
- Sesiones activas.
- Listado de roles.
- Alta y copia de rol.
- Matriz de permisos.
- Auditoría.
- Alertas de seguridad.
- Configuración de sesiones.

## 25. Criterios generales de aceptación

1. Los usuarios no se eliminan.
2. Los nombres de usuario no se reutilizan.
3. Cada usuario tiene un único rol.
4. Los permisos se asignan solo mediante roles.
5. Los roles base están protegidos.
6. Un rol personalizado sin permisos no puede guardarse.
7. Un rol inactivo bloquea a sus usuarios.
8. Las acciones sin permiso se ocultan y se rechazan en el servidor.
9. Costes y márgenes son exclusivos del administrador.
10. Los técnicos no acceden a datos fiscales, económicos ni bancarios.
11. Tras cinco intentos fallidos se bloquea durante 30 minutos.
12. Solo se admite una sesión simultánea.
13. La sesión caduca después de cinco horas sin actividad.
14. Se avisa cinco minutos antes.
15. Los cambios de seguridad invalidan la sesión.
16. La auditoría registra accesos, consultas sensibles y exportaciones.
17. La auditoría no puede modificarse ni eliminarse.
18. Los procesos automáticos se identifican como `Sistema`.
19. Las acciones sensibles exigen motivo.
20. Solo el administrador consulta la auditoría.

## 26. Decisiones pendientes para el diseño técnico

- Arquitectura exacta de autenticación para la aplicación de escritorio.
- Algoritmo y parámetros de hash de contraseñas.
- Identificación fiable del dispositivo.
- Gestión y almacenamiento seguro de tokens.
- Mecanismo para garantizar una única sesión.
- Tratamiento de desconexiones sin cierre explícito.
- Política exacta de retención de auditoría.
- Catálogo definitivo de permisos por módulo.
- Identificación y enmascaramiento de datos sensibles en registros.
- Requisitos de cifrado local y en tránsito.

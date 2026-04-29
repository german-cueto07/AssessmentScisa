**

# Assessment SCISA - API .NET de Categorías y Productos

Hola, Samuel e Ibrahim. Antes de nada, muchas gracias por la oportunidad y por el reto. Les comparto mi solución al assessment. 

Quiero ser honesto con ustedes sobre mi proceso. Como estudiante de sexto semestre en la carrera, sentar las bases de la API REST con **.NET Core 8** y modelar la base de datos relacional con **Entity Framework Core** fue mi zona de confort. Es algo que ya sabía codear y estructurar.

Sin embargo, el requerimiento de las pruebas BDD (Behavior Driven Development) con **Reqnroll y xUnit** fue un mundo completamente nuevo para mí. Nunca había hecho pruebas automatizadas de esta manera y fue un proceso de mucho aprendizaje. Para lograrlo en el tiempo establecido, me apoyé viendo varios tutoriales en YouTube sobre *SpecFlow* (que es el predecesor de Reqnroll) y utilicé Inteligencia Artificial como un "tutor guiado" para entender conceptos más avanzados como el uso de `WebApplicationFactory` para levantar la API en memoria durante los tests. 

Me llevo muchísimo aprendizaje de esta semana, especialmente en cómo estructurar código limpio (usando SOLID) para que las pruebas pasen correctamente. ¡Espero que les guste el resultado!

---

##  Tecnologías y Herramientas Utilizadas
* **.NET Core 8** (Web API)
* **Entity Framework Core** (Code-First)
* **MSSQL** (Corriendo en un contenedor de Docker)
* **Reqnroll + xUnit** (Para las pruebas BDD)
* **Patrón DTO** (Para moldear las respuestas JSON y evitar referencias circulares)

---

## El paso a paso

1.  **Modelado y Relación N:N:** Comencé configurando las entidades de `Category` y `Product`. Dejé que EF Core manejara la tabla intermedia por mí para mantener el código limpio.
2.  **Lógica Centralizada (DRY):** Para cumplir con el requerimiento de no repetir código, extraje las validaciones pesadas (como la de *Producto único por categoría*) a un método asíncrono privado en el controlador. Así, tanto el `POST` como el `PUT` lo reutilizan.
3.  **La parte de las Pruebas:** Aquí aprendí a mapear los archivos `.feature` de Reqnroll con clases en C#. Usé `WebApplicationFactory` para simular un cliente HTTP que le pega a la API real y guarda cosas en la base de datos de prueba, validando los códigos de estado `201`, `422`, etc.

---

## ¿Cómo correr el proyecto localmente?

Para revisar el código y probar que todo funciona, son estos pasos:

### 1. Levantar la Base de Datos
El proyecto está configurado para conectarse a un contenedor de Docker en el puerto `1434` porque ya tenía un contenedor en el puerto `1433` que uso para las clases y preferí hacer uno dedicado al assessment. Puedes levantar este servidor específicamente para la prueba corriendo este comando en la terminal:

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=SuperAssessment123' -p 1434:1433 --name mssql_scisa_test -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configurar la Conexión (appsettings.json)
Es necesario indicarle a la API dónde está la base de datos. Abre el archivo `appsettings.json` (dentro del proyecto de la API) y asegúrate de que la cadena de conexión apunte al puerto `1434` y tenga la contraseña del contenedor que acabamos de levantar. Debe verse así:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1434;Database=AssessmentDB;User Id=sa;Password=SuperAssessment123;TrustServerCertificate=True;"
  }
}

```

### 3. Crear las Tablas (Migraciones)
Una vez que el contenedor esté corriendo, colócate en la raíz del proyecto de la API `Assessment.AP` (aquí tuve un error de type por eso es .AP) y corre la migración para que EF Core construya las tablas:

```bash
dotnet ef database update
```

### 4. Correr la API
Para probar los endpoints desde Swagger o Postman, ejecuta:
```bash
dotnet run --project Assesment.AP
```

---

##  ¿Cómo correr las pruebas automatizadas?

Para comprobar que todos los escenarios requeridos se cumplen:

1. Abre la terminal.
2. Navega a la carpeta del proyecto de pruebas (`cd Assessment.Tests`).
3. Ejecuta el comando:

```bash
dotnet test
```

Deberías ver cómo se ejecutan todos los escenarios del `Given/When/Then` descritos en el assessment y finalizan exitosamente.

PD. Le pedí a una IA que me diera dummy data para probar los endpoints. Dejo el script debajo para usarlo igualmente

```sql
INSERT INTO Categories (Name, Description) VALUES
('Ciberseguridad', 'Herramientas, hardware y recursos para pentesting'),
('Ropa Deportiva', 'Indumentaria para entrenamiento físico y fútbol'),
('Software', 'Licencias y herramientas para desarrollo y bases de datos'),
('Refrescos', 'Bebidas carbonatadas'); 

-- Insertar Productos
INSERT INTO Products (Name, Description) VALUES
('Adaptador de red USB', 'Tarjeta compatible con modo monitor para escanear redes'),
('Jersey FC Barcelona', 'Camiseta de local temporada 25/26'),
('Cuerda de saltar', 'Cuerda de alta velocidad para calentamiento y resistencia'),
('Suscripción IDE Rider', 'Licencia anual para desarrollo en C# y .NET'),
('Balón de Oro Minimalista', 'Trofeo decorativo liso y sin animaciones realistas');

-- Insertar la relación N:N (Categoría - Producto)
INSERT INTO CategoryProduct (CategoriesId, ProductsId) VALUES
(1, 1), 
(2, 2),
(2, 3), 
(3, 4), 
(2, 5);
```

***

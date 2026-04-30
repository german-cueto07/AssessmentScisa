Feature: El usuario puede gestionar las categorias

Scenario: El usuario puede crear una categoria
    Given Tengo datos para crear una categoria
    When hago un POST a /api/v1/categories
    Then Deberia recibir status 201
    And Deberia haber creado la categoria en la base de datos

Scenario: El usuario intenta crear una categoria pero el nombre ya esta en uso
    Given Tengo una categoria con nombre "Refrescos"
    And Tengo datos para crear una categoria con nombre "Refrescos"
    When hago un POST a /api/v1/categories
    Then Deberia recibir status 422
    And Deberia recibir un mensaje de error que el nombre ya esta en uso

Scenario: El usuario puede ver el listado de las categorias
    Given Existen categorias en la base de datos
    When hago un GET de lista a "/api/v1/categories"
    Then Deberia recibir status 200

Scenario: El usuario puede ver cada una de las categorias
    Given Existe una categoria en la base de datos
    When hago un GET a la categoria especifica
    Then Deberia recibir status 200

Scenario: El usuario puede editar la categoria
    Given Existe una categoria en la base de datos
    And Modifico el nombre de la categoria a "Actualizada"
    When hago un PUT a la categoria especifica
    Then Deberia recibir status 204

Scenario: El usuario intenta editar la categoria con un nombre vacio
    Given Existe una categoria en la base de datos
    And Modifico el nombre de la categoria a vacio
    When hago un PUT a la categoria especifica
    Then Deberia recibir status 422

Scenario: El usuario puede destruir una categoria
    Given Existe una categoria en la base de datos
    When hago un DELETE a la categoria especifica
    Then Deberia recibir status 204

Scenario: El usuario intenta destruir una categoria pero tiene productos asignados. Entonces eliminas los productos y ya puedes eliminar la categoria
    Given Existe una categoria con productos asignados
    When hago un DELETE a la categoria especifica
    Then Deberia recibir status 400
    When elimino el producto asignado
    And hago un DELETE a la categoria especifica
    Then Deberia recibir status 204
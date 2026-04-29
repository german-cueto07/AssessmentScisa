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
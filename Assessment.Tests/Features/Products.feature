Feature: El usuario puede gestionar los productos

Scenario: El usuario puede crear un producto
    Given Existe una categoria en la base de datos para el producto
    And Tengo datos para crear un producto
    When hago un POST de producto a /api/v1/products
    Then Deberia recibir status 201
    And Deberia haber creado el producto en la base de datos

Scenario: El usuario intenta crear un producto pero el nombre ya esta en uso
    Given Existe una categoria en la base de datos para el producto
    And Tengo un producto con nombre "Monitor" en esa categoria
    And Tengo datos para intentar crear otro producto con nombre "Monitor" en la misma categoria
    When hago un POST de producto a /api/v1/products
    Then Deberia recibir status 422
    And Deberia recibir un mensaje de error de producto duplicado

Scenario: El usuario puede ver el listado de los productos con sus categorias
    Given Existe una categoria en la base de datos para el producto
    And Tengo un producto con nombre "Teclado" en esa categoria
    When hago un GET a /api/v1/products
    Then Deberia recibir status 200
    And Debe de ver que categorias pertenece con su nombre
    
Scenario: El usuario puede ver el listado de los productos
    Given Existen productos en la base de datos
    When hago un GET de lista a "/api/v1/products"
    Then Deberia recibir status 200

Scenario: El usuario puede ver cada uno de los productos
    Given Existe un producto en la base de datos
    When hago un GET al producto especifico
    Then Deberia recibir status 200

Scenario: El usuario puede editar el producto
    Given Existe un producto en la base de datos
    And Modifico el nombre del producto a "Actualizado"
    When hago un PUT al producto especifico
    Then Deberia recibir status 204

Scenario: El usuario intenta editar el producto con un nombre vacio
    Given Existe un producto en la base de datos
    And Modifico el nombre del producto a vacio
    When hago un PUT al producto especifico
    Then Deberia recibir status 422

Scenario: El usuario puede destruir un producto
    Given Existe un producto en la base de datos
    When hago un DELETE al producto especifico
    Then Deberia recibir status 204
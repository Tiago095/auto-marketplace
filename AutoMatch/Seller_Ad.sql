INSERT INTO Vendedores (
    Id_User, Tipo, NIF, Contactos, Rua, Codigo_Postal
)
VALUES (
    1,        -- Id_User from Utilizadores
    1,         -- Tipo = 1 (Professional), 0 = Individual
    123456789, -- NIF
    '912345678',
    'Rua Teste, 42',
    '1000-100'  -- MUST exist in CodigoPostais table
);

INSERT INTO Anuncios (
    Id_Vendedor,
    Id_Admin,
    Id_Modelo,
    Titulo,
    Descricao,
    Ano,
    Preco,
    Kilometros,
    Localizacao,
    Estado,
    Matricula,
    AdministradorId_User,
    ModeloId_Modelo
)
VALUES (
    1,            -- Id_Vendedor (same as Id_User of seller)
    1,             -- Id_Admin
    3,             -- Id_Modelo (must exist!)
    'BMW Série 3',
    'Carro em excelente estado',
    '2018-01-01',
    23000,
    85000,
    'Lisboa',
    1,             -- Estado = ativo
    'AA11BB',
    1,             -- AdministradorId_User
    1              -- ModeloId_Modelo
);


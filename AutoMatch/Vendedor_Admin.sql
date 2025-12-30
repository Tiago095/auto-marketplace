INSERT INTO Administradores (
    Id_User,
    Id_Admin
)
VALUES (
    2,   -- Id_User
    1       -- Id_Admin (exemplo)
);

INSERT INTO Vendedores (
    Id_User,
    Tipo,
    NIF,
    Contactos,
    Rua,
    Codigo_Postal
)
VALUES (
    1,        -- Id_User
    1,           -- Tipo (1 = vendedor / 0 = outro)
    245789123,   -- NIF
    '912345678', -- Contactos
    'Rua das Flores 25', -- Rua
    '1000-000'  -- Codigo_Postal
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
    Matricula
)
VALUES (
    1,                        -- Id_Vendedor (existe em Vendedores)
    2,                        -- Id_Admin (existe em Administradores)
    1,                           -- Id_Modelo (BMW, exemplo)
    'BMW Série 3 2018',          -- Titulo
    'Carro em ótimo estado',     -- Descricao
    '2018-01-01',                -- Ano
    24500,                       -- Preco
    98000,                       -- Kilometros
    'Porto',                     -- Localizacao
    1,                           -- Estado (1 = ativo)
    '23-AB-45'                  -- Matricula
);

using AutoMatch.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace AutoMatch.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AutoMatchContext context)
        {
            // Verifica se a base de dados já tem dados
            if (context.Utilizadores.Any())
            {
                return; // A base de dados já foi inicializada
            }

            // Hash password helper (mesma função do AccountController)
            string HashPassword(string password)
            {
                using var sha = SHA256.Create();
                var hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashed);
            }

            // 1. Inserir CodigosPostais
            var codigosPostais = new List<CodigoPostal>
            {
                new CodigoPostal { Codigo_Postal = "0000-000", Localidade = "Desconhecido" },
                new CodigoPostal { Codigo_Postal = "3800-000", Localidade = "Aveiro" },
                new CodigoPostal { Codigo_Postal = "7800-000", Localidade = "Beja" },
                new CodigoPostal { Codigo_Postal = "4700-000", Localidade = "Braga" },
                new CodigoPostal { Codigo_Postal = "5300-000", Localidade = "Bragança" },
                new CodigoPostal { Codigo_Postal = "6000-000", Localidade = "Castelo Branco" },
                new CodigoPostal { Codigo_Postal = "3000-000", Localidade = "Coimbra" },
                new CodigoPostal { Codigo_Postal = "7000-000", Localidade = "Évora" },
                new CodigoPostal { Codigo_Postal = "8000-000", Localidade = "Faro" },
                new CodigoPostal { Codigo_Postal = "6300-000", Localidade = "Guarda" },
                new CodigoPostal { Codigo_Postal = "2400-000", Localidade = "Leiria" },
                new CodigoPostal { Codigo_Postal = "1000-000", Localidade = "Lisboa" },
                new CodigoPostal { Codigo_Postal = "7300-000", Localidade = "Portalegre" },
                new CodigoPostal { Codigo_Postal = "1405-043", Localidade = "Porto" },
                new CodigoPostal { Codigo_Postal = "2000-000", Localidade = "Santarém" },
                new CodigoPostal { Codigo_Postal = "2900-000", Localidade = "Setúbal" },
                new CodigoPostal { Codigo_Postal = "4900-000", Localidade = "Viana do Castelo" },
                new CodigoPostal { Codigo_Postal = "5000-000", Localidade = "Vila Real" },
                new CodigoPostal { Codigo_Postal = "3430-000", Localidade = "Viseu" }
            };
            context.CodigoPostais.AddRange(codigosPostais);
            context.SaveChanges();

            // 2. Inserir Modelos
            var modelos = new List<Modelo>
            {
                // BMW
                new Modelo { Marca = "BMW", NomeModelo = "Series 3", Transmissao = true, Combustivel = "Gasolina", Categoria = "Sedan" },
                new Modelo { Marca = "BMW", NomeModelo = "X5", Transmissao = true, Combustivel = "Diesel", Categoria = "SUV" },
                new Modelo { Marca = "BMW", NomeModelo = "Series 1", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                // Mercedes
                new Modelo { Marca = "Mercedes-Benz", NomeModelo = "C-Class", Transmissao = true, Combustivel = "Gasolina", Categoria = "Sedan" },
                new Modelo { Marca = "Mercedes-Benz", NomeModelo = "A-Class", Transmissao = true, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Mercedes-Benz", NomeModelo = "GLC", Transmissao = true, Combustivel = "Diesel", Categoria = "SUV" },
                // Audi
                new Modelo { Marca = "Audi", NomeModelo = "A3", Transmissao = true, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Audi", NomeModelo = "A4", Transmissao = true, Combustivel = "Diesel", Categoria = "Sedan" },
                new Modelo { Marca = "Audi", NomeModelo = "Q5", Transmissao = true, Combustivel = "Diesel", Categoria = "SUV" },
                // Volkswagen
                new Modelo { Marca = "Volkswagen", NomeModelo = "Golf", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Volkswagen", NomeModelo = "Passat", Transmissao = true, Combustivel = "Diesel", Categoria = "Sedan" },
                new Modelo { Marca = "Volkswagen", NomeModelo = "Tiguan", Transmissao = true, Combustivel = "Diesel", Categoria = "SUV" },
                // Toyota
                new Modelo { Marca = "Toyota", NomeModelo = "Corolla", Transmissao = true, Combustivel = "Híbrido", Categoria = "Sedan" },
                new Modelo { Marca = "Toyota", NomeModelo = "Yaris", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Toyota", NomeModelo = "RAV4", Transmissao = true, Combustivel = "Híbrido", Categoria = "SUV" },
                // Honda
                new Modelo { Marca = "Honda", NomeModelo = "Civic", Transmissao = false, Combustivel = "Gasolina", Categoria = "Sedan" },
                new Modelo { Marca = "Honda", NomeModelo = "CR-V", Transmissao = true, Combustivel = "Gasolina", Categoria = "SUV" },
                new Modelo { Marca = "Honda", NomeModelo = "Jazz", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                // Ford
                new Modelo { Marca = "Ford", NomeModelo = "Focus", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Ford", NomeModelo = "Mondeo", Transmissao = true, Combustivel = "Diesel", Categoria = "Sedan" },
                new Modelo { Marca = "Ford", NomeModelo = "Kuga", Transmissao = true, Combustivel = "Gasolina", Categoria = "SUV" },
                // Renault
                new Modelo { Marca = "Renault", NomeModelo = "Clio", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Renault", NomeModelo = "Mégane", Transmissao = false, Combustivel = "Gasolina", Categoria = "Hatchback" },
                new Modelo { Marca = "Renault", NomeModelo = "Captur", Transmissao = true, Combustivel = "Gasolina", Categoria = "SUV" }
            };
            context.Modelos.AddRange(modelos);
            context.SaveChanges();

            // 3. Criar Utilizadores
            var user = new Utilizador
            {
                Nome = "User Test",
                UserName = "user",
                Email = "user@gmail.com",
                Senha = HashPassword("password123"),
                Estado = true
            };

            var seller1 = new Utilizador
            {
                Nome = "Seller One",
                UserName = "seller1",
                Email = "seller1@gmail.com",
                Senha = HashPassword("password123"),
                Estado = true
            };

            var seller2 = new Utilizador
            {
                Nome = "Seller Two",
                UserName = "seller2",
                Email = "seller2@gmail.com",
                Senha = HashPassword("password123"),
                Estado = true
            };

            var admin = new Utilizador
            {
                Nome = "Admin User",
                UserName = "admin",
                Email = "admin@gmail.com",
                Senha = HashPassword("password123"),
                Estado = true
            };

            context.Utilizadores.AddRange(user, seller1, seller2, admin);
            context.SaveChanges();

            var comprador = new Comprador
            {
                Id_User = user.Id_User,
                Contactos = "912345678",
                Rua = "Rua do Comprador",
                Codigo_Postal = "1000-000"
            };
            context.Compradores.Add(comprador);
            context.SaveChanges();


            var vendedor1 = new Vendedor
            {
                Id_User = seller1.Id_User,
                Tipo = true, // Professional
                NIF = 123456789,
                Contactos = "912345678",
                Rua = "Rua do Vendedor 1",
                Codigo_Postal = "1000-000"
            };

            var vendedor2 = new Vendedor
            {
                Id_User = seller2.Id_User,
                Tipo = true, // Professional
                NIF = 987654321,
                Contactos = "923456789",
                Rua = "Rua do Vendedor 2",
                Codigo_Postal = "1405-043"
            };

            context.Vendedores.AddRange(vendedor1, vendedor2);
            context.SaveChanges();

            var administrador = new Administrador
            {
                Id_User = admin.Id_User,
                Id_Admin = 1
            };
            context.Administradores.Add(administrador);
            context.SaveChanges();

            var bmwSeries3 = context.Modelos.First(m => m.Marca == "BMW" && m.NomeModelo == "Series 3");
            var mercedesCClass = context.Modelos.First(m => m.Marca == "Mercedes-Benz" && m.NomeModelo == "C-Class");
            var audiA4 = context.Modelos.First(m => m.Marca == "Audi" && m.NomeModelo == "A4");
            var bmwX5 = context.Modelos.First(m => m.Marca == "BMW" && m.NomeModelo == "X5");
            var toyotaCorolla = context.Modelos.First(m => m.Marca == "Toyota" && m.NomeModelo == "Corolla");
            var vwGolf = context.Modelos.First(m => m.Marca == "Volkswagen" && m.NomeModelo == "Golf");

            var anuncios = new List<Anuncio>
            {
                // Anúncios do seller1
                new Anuncio
                {
                    Id_Vendedor = seller1.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = bmwSeries3.Id_Modelo, // BMW Series 3
                    Titulo = "BMW Series 3",
                    Descricao = "Carro em excelente estado, bem conservado",
                    Ano = new DateTime(2019, 1, 1),
                    Preco = 35000,
                    Kilometros = 45000,
                    Localizacao = "Lisboa",
                    Estado = true,
                    Matricula = "AB-12-CD"
                },
                new Anuncio
                {
                    Id_Vendedor = seller1.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = mercedesCClass.Id_Modelo, // Mercedes C-Class
                    Titulo = "Mercedes C-Class",
                    Descricao = "Veículo premium com todas as extras",
                    Ano = new DateTime(2020, 6, 1),
                    Preco = 42000,
                    Kilometros = 35000,
                    Localizacao = "Porto",
                    Estado = true,
                    Matricula = "75-XR-15"
                },
                new Anuncio
                {
                    Id_Vendedor = seller1.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = audiA4.Id_Modelo, // Audi A4
                    Titulo = "Audi A4",
                    Descricao = "Sedan confortável e eficiente",
                    Ano = new DateTime(2018, 3, 1),
                    Preco = 28000,
                    Kilometros = 60000,
                    Localizacao = "Braga",
                    Estado = true,
                    Matricula = "IJ-56-KL"
                },
                // Anúncios do seller2
                new Anuncio
                {
                    Id_Vendedor = seller2.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = bmwX5.Id_Modelo, // BMW X5
                    Titulo = "BMW X5",
                    Descricao = "SUV espaçoso e potente",
                    Ano = new DateTime(2021, 1, 1),
                    Preco = 55000,
                    Kilometros = 25000,
                    Localizacao = "Coimbra",
                    Estado = true,
                    Matricula = "MN-78-OP"
                },
                new Anuncio
                {
                    Id_Vendedor = seller2.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = toyotaCorolla.Id_Modelo, // Toyota Corolla
                    Titulo = "Toyota Corolla",
                    Descricao = "Híbrido económico e fiável",
                    Ano = new DateTime(2020, 9, 1),
                    Preco = 24000,
                    Kilometros = 40000,
                    Localizacao = "Aveiro",
                    Estado = true,
                    Matricula = "QR-90-ST"
                },
                new Anuncio
                {
                    Id_Vendedor = seller2.Id_User,
                    Id_Admin = admin.Id_User,
                    Id_Modelo = vwGolf.Id_Modelo, // Volkswagen Golf
                    Titulo = "Volkswagen Golf",
                    Descricao = "Hatchback prático e versátil",
                    Ano = new DateTime(2019, 5, 1),
                    Preco = 19000,
                    Kilometros = 50000,
                    Localizacao = "Setúbal",
                    Estado = true,
                    Matricula = "UV-12-WX"
                }
            };

            context.Anuncios.AddRange(anuncios);
            context.SaveChanges();

            var imagens = new List<Imagens>();
            foreach (var anuncio in anuncios)
            {
                for (int i = 1; i <= 5; i++)
                {
                    imagens.Add(new Imagens
                    {
                        Id_Anuncio = anuncio.Id_Anuncio,
                        CaminhoImagem = $"/Anuncios/Anuncio{anuncio.Id_Anuncio}/Imagens/img{i}.png"
                    });
                }
            }

            context.Imagens.AddRange(imagens);
            context.SaveChanges();
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoMatch.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CodigoPostais",
                columns: table => new
                {
                    Codigo_Postal = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Localidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigoPostais", x => x.Codigo_Postal);
                });

            migrationBuilder.CreateTable(
                name: "Modelos",
                columns: table => new
                {
                    Id_Modelo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NomeModelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Transmissao = table.Column<bool>(type: "bit", nullable: false),
                    Combustivel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modelos", x => x.Id_Modelo);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id_User = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id_User);
                });

            migrationBuilder.CreateTable(
                name: "Administradores",
                columns: table => new
                {
                    Id_User = table.Column<int>(type: "int", nullable: false),
                    Id_Admin = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administradores", x => x.Id_User);
                    table.ForeignKey(
                        name: "FK_Administradores_Utilizadores_Id_User",
                        column: x => x.Id_User,
                        principalTable: "Utilizadores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compradores",
                columns: table => new
                {
                    Id_User = table.Column<int>(type: "int", nullable: false),
                    Contactos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rua = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Codigo_Postal = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compradores", x => x.Id_User);
                    table.ForeignKey(
                        name: "FK_Compradores_CodigoPostais_Codigo_Postal",
                        column: x => x.Codigo_Postal,
                        principalTable: "CodigoPostais",
                        principalColumn: "Codigo_Postal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compradores_Utilizadores_Id_User",
                        column: x => x.Id_User,
                        principalTable: "Utilizadores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id_User = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<bool>(type: "bit", nullable: false),
                    NIF = table.Column<int>(type: "int", nullable: true),
                    Contactos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rua = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Codigo_Postal = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CodigoPostalCodigo_Postal = table.Column<string>(type: "nvarchar(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id_User);
                    table.ForeignKey(
                        name: "FK_Vendedores_CodigoPostais_CodigoPostalCodigo_Postal",
                        column: x => x.CodigoPostalCodigo_Postal,
                        principalTable: "CodigoPostais",
                        principalColumn: "Codigo_Postal",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendedores_Utilizadores_Id_User",
                        column: x => x.Id_User,
                        principalTable: "Utilizadores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preferencias",
                columns: table => new
                {
                    Preferencias_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Comprador = table.Column<int>(type: "int", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Detalhe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferencias", x => x.Preferencias_Id);
                    table.ForeignKey(
                        name: "FK_Preferencias_Compradores_Id_Comprador",
                        column: x => x.Id_Comprador,
                        principalTable: "Compradores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Anuncios",
                columns: table => new
                {
                    Id_Anuncio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Vendedor = table.Column<int>(type: "int", nullable: false),
                    Id_Admin = table.Column<int>(type: "int", nullable: false),
                    Id_Modelo = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ano = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Preco = table.Column<int>(type: "int", nullable: false),
                    Kilometros = table.Column<int>(type: "int", nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Matricula = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    AdministradorId_User = table.Column<int>(type: "int", nullable: false),
                    ModeloId_Modelo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anuncios", x => x.Id_Anuncio);
                    table.ForeignKey(
                        name: "FK_Anuncios_Administradores_AdministradorId_User",
                        column: x => x.AdministradorId_User,
                        principalTable: "Administradores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anuncios_Modelos_ModeloId_Modelo",
                        column: x => x.ModeloId_Modelo,
                        principalTable: "Modelos",
                        principalColumn: "Id_Modelo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anuncios_Vendedores_Id_Vendedor",
                        column: x => x.Id_Vendedor,
                        principalTable: "Vendedores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DadosFaturacoes",
                columns: table => new
                {
                    Dados_Faturacao_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Vendedor = table.Column<int>(type: "int", nullable: false),
                    Fatura = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DadosFaturacoes", x => x.Dados_Faturacao_Id);
                    table.ForeignKey(
                        name: "FK_DadosFaturacoes_Vendedores_Id_Vendedor",
                        column: x => x.Id_Vendedor,
                        principalTable: "Vendedores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id_notificacao = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Vendedor = table.Column<int>(type: "int", nullable: false),
                    Id_Comprador = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Data_Envio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id_notificacao);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Compradores_Id_Comprador",
                        column: x => x.Id_Comprador,
                        principalTable: "Compradores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Vendedores_Id_Vendedor",
                        column: x => x.Id_Vendedor,
                        principalTable: "Vendedores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Compras",
                columns: table => new
                {
                    Id_Compra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Anuncio = table.Column<int>(type: "int", nullable: false),
                    Id_Comprador = table.Column<int>(type: "int", nullable: false),
                    Data_Compra = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Compras", x => x.Id_Compra);
                    table.ForeignKey(
                        name: "FK_Compras_Anuncios_Id_Anuncio",
                        column: x => x.Id_Anuncio,
                        principalTable: "Anuncios",
                        principalColumn: "Id_Anuncio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Compras_Compradores_Id_Comprador",
                        column: x => x.Id_Comprador,
                        principalTable: "Compradores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id_Doc = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Anuncio = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CaminhoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id_Doc);
                    table.ForeignKey(
                        name: "FK_Documentos_Anuncios_Id_Anuncio",
                        column: x => x.Id_Anuncio,
                        principalTable: "Anuncios",
                        principalColumn: "Id_Anuncio",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Imagens",
                columns: table => new
                {
                    Id_Imagem = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Anuncio = table.Column<int>(type: "int", nullable: false),
                    CaminhoImagem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagens", x => x.Id_Imagem);
                    table.ForeignKey(
                        name: "FK_Imagens_Anuncios_Id_Anuncio",
                        column: x => x.Id_Anuncio,
                        principalTable: "Anuncios",
                        principalColumn: "Id_Anuncio",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id_Reserva = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Comprador = table.Column<int>(type: "int", nullable: false),
                    Id_Anuncio = table.Column<int>(type: "int", nullable: false),
                    Data_Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data_Fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id_Reserva);
                    table.ForeignKey(
                        name: "FK_Reservas_Anuncios_Id_Anuncio",
                        column: x => x.Id_Anuncio,
                        principalTable: "Anuncios",
                        principalColumn: "Id_Anuncio",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Compradores_Id_Comprador",
                        column: x => x.Id_Comprador,
                        principalTable: "Compradores",
                        principalColumn: "Id_User",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Visitas",
                columns: table => new
                {
                    Id_Visita = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_Reserva = table.Column<int>(type: "int", nullable: false),
                    Data_Hora = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitas", x => x.Id_Visita);
                    table.ForeignKey(
                        name: "FK_Visitas_Reservas_Id_Reserva",
                        column: x => x.Id_Reserva,
                        principalTable: "Reservas",
                        principalColumn: "Id_Reserva",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_AdministradorId_User",
                table: "Anuncios",
                column: "AdministradorId_User");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_Id_Vendedor",
                table: "Anuncios",
                column: "Id_Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_Anuncios_ModeloId_Modelo",
                table: "Anuncios",
                column: "ModeloId_Modelo");

            migrationBuilder.CreateIndex(
                name: "IX_Compradores_Codigo_Postal",
                table: "Compradores",
                column: "Codigo_Postal");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_Id_Anuncio",
                table: "Compras",
                column: "Id_Anuncio");

            migrationBuilder.CreateIndex(
                name: "IX_Compras_Id_Comprador",
                table: "Compras",
                column: "Id_Comprador");

            migrationBuilder.CreateIndex(
                name: "IX_DadosFaturacoes_Id_Vendedor",
                table: "DadosFaturacoes",
                column: "Id_Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_Id_Anuncio",
                table: "Documentos",
                column: "Id_Anuncio");

            migrationBuilder.CreateIndex(
                name: "IX_Imagens_Id_Anuncio",
                table: "Imagens",
                column: "Id_Anuncio");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_Id_Comprador",
                table: "Notificacoes",
                column: "Id_Comprador");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_Id_Vendedor",
                table: "Notificacoes",
                column: "Id_Vendedor");

            migrationBuilder.CreateIndex(
                name: "IX_Preferencias_Id_Comprador",
                table: "Preferencias",
                column: "Id_Comprador");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Id_Anuncio",
                table: "Reservas",
                column: "Id_Anuncio");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Id_Comprador",
                table: "Reservas",
                column: "Id_Comprador");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_CodigoPostalCodigo_Postal",
                table: "Vendedores",
                column: "CodigoPostalCodigo_Postal");

            migrationBuilder.CreateIndex(
                name: "IX_Visitas_Id_Reserva",
                table: "Visitas",
                column: "Id_Reserva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Compras");

            migrationBuilder.DropTable(
                name: "DadosFaturacoes");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Imagens");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "Preferencias");

            migrationBuilder.DropTable(
                name: "Visitas");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Anuncios");

            migrationBuilder.DropTable(
                name: "Compradores");

            migrationBuilder.DropTable(
                name: "Administradores");

            migrationBuilder.DropTable(
                name: "Modelos");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropTable(
                name: "CodigoPostais");

            migrationBuilder.DropTable(
                name: "Utilizadores");
        }
    }
}

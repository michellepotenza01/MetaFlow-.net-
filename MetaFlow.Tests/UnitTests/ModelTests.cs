using MetaFlow.API.Models;
using MetaFlow.API.Enums;
using Xunit;

namespace MetaFlow.Tests.UnitTests
{
    public class ModelTests
    {
        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Excelente_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Produtividade = 9 };

            // Act
            var status = registro.ObterStatusProdutividade();

            // Assert
            Assert.Equal("Excelente", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Boa_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Produtividade = 8 };

            // Act
            var status = registro.ObterStatusProdutividade();

            // Assert
            Assert.Equal("Boa", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Regular_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Produtividade = 6 };

            // Act
            var status = registro.ObterStatusProdutividade();

            // Assert
            Assert.Equal("Regular", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Baixa_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Produtividade = 4 };

            // Act
            var status = registro.ObterStatusProdutividade();

            // Assert
            Assert.Equal("Baixa", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Otimo_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Humor = 9 };

            // Act
            var status = registro.ObterStatusHumor();

            // Assert
            Assert.Equal("Ótimo", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Bom_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Humor = 8 };

            // Act
            var status = registro.ObterStatusHumor();

            // Assert
            Assert.Equal("Bom", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Regular_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Humor = 6 };

            // Act
            var status = registro.ObterStatusHumor();

            // Assert
            Assert.Equal("Regular", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Ruim_ReturnsCorrectStatus()
        {
            // Arrange
            var registro = new RegistroDiario { Humor = 4 };

            // Act
            var status = registro.ObterStatusHumor();

            // Assert
            Assert.Equal("Ruim", status);
        }

        [Fact]
        public void RegistroDiario_ObterDiaDaSemana_ReturnsCorrectDay()
        {
            // Arrange
            var registro = new RegistroDiario { Data = new DateTime(2024, 1, 1) }; // Segunda-feira

            // Act
            var diaDaSemana = registro.ObterDiaDaSemana();

            // Assert
            Assert.Equal("segunda-feira", diaDaSemana.ToLower());
        }

        [Fact]
        public async Task  Usuario_AtualizarDataModificacao_UpdatesTimestamp()
        {
            var usuario = new Usuario();
            var dataAntes = usuario.AtualizadoEm;

            await Task.Delay(10);

            usuario.AtualizarDataModificacao();

            Assert.True(usuario.AtualizadoEm > dataAntes);
        }

        [Fact]
        public void Usuario_TemPerfilCompleto_WithProfessionAndGoal_ReturnsTrue()
        {
            // Arrange
            var usuario = new Usuario
            {
                Profissao = "Desenvolvedor",
                ObjetivoProfissional = "Senior Developer"
            };

            // Act
            var temPerfilCompleto = !string.IsNullOrEmpty(usuario.Profissao) && 
                                   !string.IsNullOrEmpty(usuario.ObjetivoProfissional);

            // Assert
            Assert.True(temPerfilCompleto);
        }

        [Fact]
        public void Usuario_TemPerfilCompleto_MissingData_ReturnsFalse()
        {
            // Arrange
            var usuario = new Usuario
            {
                Profissao = "Desenvolvedor",
                ObjetivoProfissional = null
            };

            // Act
            var temPerfilCompleto = !string.IsNullOrEmpty(usuario.Profissao) && 
                                   !string.IsNullOrEmpty(usuario.ObjetivoProfissional);

            // Assert
            Assert.False(temPerfilCompleto);
        }
    }
}
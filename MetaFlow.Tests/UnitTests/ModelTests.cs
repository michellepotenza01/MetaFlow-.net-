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
             var registro = new RegistroDiario { Produtividade = 9 };

             var status = registro.ObterStatusProdutividade();

             Assert.Equal("Excelente", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Boa_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Produtividade = 8 };

             var status = registro.ObterStatusProdutividade();

             Assert.Equal("Boa", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Regular_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Produtividade = 6 };

             var status = registro.ObterStatusProdutividade();

             Assert.Equal("Regular", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusProdutividade_Baixa_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Produtividade = 4 };

             var status = registro.ObterStatusProdutividade();

             Assert.Equal("Baixa", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Otimo_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Humor = 9 };

             var status = registro.ObterStatusHumor();

             Assert.Equal("Ótimo", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Bom_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Humor = 8 };

             var status = registro.ObterStatusHumor();

             Assert.Equal("Bom", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Regular_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Humor = 6 };

             var status = registro.ObterStatusHumor();

             Assert.Equal("Regular", status);
        }

        [Fact]
        public void RegistroDiario_ObterStatusHumor_Ruim_ReturnsCorrectStatus()
        {
             var registro = new RegistroDiario { Humor = 4 };

             var status = registro.ObterStatusHumor();

            Assert.Equal("Ruim", status);
        }

        [Fact]
        public void RegistroDiario_ObterDiaDaSemana_ReturnsCorrectDay()
        {
            var registro = new RegistroDiario { Data = new DateTime(2024, 1, 1) }; // Segunda-feira

            var diaDaSemana = registro.ObterDiaDaSemana();

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
            var usuario = new Usuario
            {
                Profissao = "Desenvolvedor",
                ObjetivoProfissional = "Senior Developer"
            };

            var temPerfilCompleto = !string.IsNullOrEmpty(usuario.Profissao) && 
                                   !string.IsNullOrEmpty(usuario.ObjetivoProfissional);

            Assert.True(temPerfilCompleto);
        }

        [Fact]
        public void Usuario_TemPerfilCompleto_MissingData_ReturnsFalse()
        {
            var usuario = new Usuario
            {
                Profissao = "Desenvolvedor",
                ObjetivoProfissional = null
            };

            var temPerfilCompleto = !string.IsNullOrEmpty(usuario.Profissao) && 
                                   !string.IsNullOrEmpty(usuario.ObjetivoProfissional);

            Assert.False(temPerfilCompleto);
        }
    }
}
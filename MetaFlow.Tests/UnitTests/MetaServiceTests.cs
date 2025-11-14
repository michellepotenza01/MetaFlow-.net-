using MetaFlow.API.Models;
using MetaFlow.API.Enums;
using Xunit;

namespace MetaFlow.Tests.UnitTests
{
    public class MetaServiceTests
    {
        [Fact]
        public void Meta_CalcularDiasRestantes_FutureDate_ReturnsPositive()
        {
            // Arrange
            var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(10)
            };

            // Act
            var diasRestantes = meta.CalcularDiasRestantes();

            // Assert
            Assert.True(diasRestantes >= 0);
        }

        [Fact]
        public void Meta_CalcularDiasRestantes_PastDate_ReturnsZero()
        {
            // Arrange
            var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-5)
            };

            // Act
            var diasRestantes = meta.CalcularDiasRestantes();

            // Assert
            Assert.Equal(0, diasRestantes);
        }

        [Fact]
        public void Meta_EstaAtrasada_PastDateAndActive_ReturnsTrue()
        {
            // Arrange
            var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-1),
                Status = StatusMeta.Ativa
            };

            // Act
            var estaAtrasada = meta.EstaAtrasada();

            // Assert
            Assert.True(estaAtrasada);
        }

        [Fact]
        public void Meta_EstaAtrasada_PastDateButCompleted_ReturnsFalse()
        {
            // Arrange
            var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-1),
                Status = StatusMeta.Concluida
            };

            // Act
            var estaAtrasada = meta.EstaAtrasada();

            // Assert
            Assert.False(estaAtrasada);
        }

        [Fact]
        public void Meta_AtualizarStatusBaseadoNoProgresso_100Percent_SetsConcluida()
        {
            // Arrange
            var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Ativa
            };

            // Act
            meta.AtualizarStatusBaseadoNoProgresso();

            // Assert
            Assert.Equal(StatusMeta.Concluida, meta.Status);
        }

        [Fact]
        public void Meta_AtualizarStatusBaseadoNoProgresso_LessThan100_KeepsActive()
        {
            // Arrange
            var meta = new Meta
            {
                Progresso = 80,
                Status = StatusMeta.Ativa
            };

            // Act
            meta.AtualizarStatusBaseadoNoProgresso();

            // Assert
            Assert.Equal(StatusMeta.Ativa, meta.Status);
        }

        [Fact]
        public void Meta_PodeSerConcluida_Progresso100AndActive_ReturnsTrue()
        {
            // Arrange
            var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Ativa
            };

            // Act
            var podeSerConcluida = meta.PodeSerConcluida();

            // Assert
            Assert.True(podeSerConcluida);
        }

        [Fact]
        public void Meta_PodeSerConcluida_AlreadyCompleted_ReturnsFalse()
        {
            // Arrange
            var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Concluida
            };

            // Act
            var podeSerConcluida = meta.PodeSerConcluida();

            // Assert
            Assert.False(podeSerConcluida);
        }
    }
}
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
             var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(10)
            };

             var diasRestantes = meta.CalcularDiasRestantes();

             Assert.True(diasRestantes >= 0);
        }

        [Fact]
        public void Meta_CalcularDiasRestantes_PastDate_ReturnsZero()
        {
             var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-5)
            };

             var diasRestantes = meta.CalcularDiasRestantes();

             Assert.Equal(0, diasRestantes);
        }

        [Fact]
        public void Meta_EstaAtrasada_PastDateAndActive_ReturnsTrue()
        {
             var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-1),
                Status = StatusMeta.Ativa
            };

             var estaAtrasada = meta.EstaAtrasada();

             Assert.True(estaAtrasada);
        }

        [Fact]
        public void Meta_EstaAtrasada_PastDateButCompleted_ReturnsFalse()
        {
             var meta = new Meta
            {
                Prazo = DateTime.Now.AddDays(-1),
                Status = StatusMeta.Concluida
            };

             var estaAtrasada = meta.EstaAtrasada();

             Assert.False(estaAtrasada);
        }

        [Fact]
        public void Meta_AtualizarStatusBaseadoNoProgresso_100Percent_SetsConcluida()
        {
             var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Ativa
            };

             meta.AtualizarStatusBaseadoNoProgresso();

             Assert.Equal(StatusMeta.Concluida, meta.Status);
        }

        [Fact]
        public void Meta_AtualizarStatusBaseadoNoProgresso_LessThan100_KeepsActive()
        {
             var meta = new Meta
            {
                Progresso = 80,
                Status = StatusMeta.Ativa
            };

             meta.AtualizarStatusBaseadoNoProgresso();

             Assert.Equal(StatusMeta.Ativa, meta.Status);
        }

        [Fact]
        public void Meta_PodeSerConcluida_Progresso100AndActive_ReturnsTrue()
        {
             var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Ativa
            };

             var podeSerConcluida = meta.PodeSerConcluida();

             Assert.True(podeSerConcluida);
        }

        [Fact]
        public void Meta_PodeSerConcluida_AlreadyCompleted_ReturnsFalse()
        {
             var meta = new Meta
            {
                Progresso = 100,
                Status = StatusMeta.Concluida
            };

             var podeSerConcluida = meta.PodeSerConcluida();

             Assert.False(podeSerConcluida);
        }
    }
}
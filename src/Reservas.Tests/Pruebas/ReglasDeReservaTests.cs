using Reservas.Core.Domain;

namespace Reservas.Tests.Pruebas;

public sealed class ReglasDeReservaTests
{
    private static readonly DateTime Ahora = new(2026, 11, 24, 10, 0, 0);

    private static DateTime Dia(int hora, int minuto = 0) =>
        new(2026, 11, 25, hora, minuto, 0);

    [Fact]
    public void Una_reserva_de_media_hora_ocupa_una_franja()
    {
        IReadOnlyList<int> franjas = HorarioInstitucional.FranjasEntre(Dia(7), Dia(7, 30));

        Assert.Equal(new[] { 0 }, franjas);
    }

    [Fact]
    public void Una_reserva_de_dos_horas_ocupa_cuatro_franjas()
    {
        IReadOnlyList<int> franjas = HorarioInstitucional.FranjasEntre(Dia(8), Dia(10));

        Assert.Equal(new[] { 2, 3, 4, 5 }, franjas);
    }

    [Fact]
    public void El_dia_tiene_treinta_franjas_y_termina_a_las_veintidos()
    {
        DateOnly fecha = new(2026, 11, 25);

        Assert.Equal(30, HorarioInstitucional.TotalFranjas);
        Assert.Equal(Dia(7), HorarioInstitucional.InicioDeFranja(fecha, 0));
        Assert.Equal(Dia(21, 30), HorarioInstitucional.InicioDeFranja(fecha, 29));
        Assert.Equal(Dia(22), HorarioInstitucional.FinDeFranja(fecha, 29));
    }

    [Fact]
    public void Los_extremos_del_horario_son_validos()
    {
        // Desde la regla de las cuatro horas, el día completo ya no es una
        // solicitud válida: se comprueban los extremos por separado.
        ReglasDeReserva.Validar(Dia(7), Dia(11), Ahora);
        ReglasDeReserva.Validar(Dia(18), Dia(22), Ahora);
    }

    [Fact]
    public void Una_reserva_que_empieza_antes_de_la_apertura_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(6), Dia(8), Ahora));

        Assert.Contains("07:00", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Una_reserva_que_termina_despues_del_cierre_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(21), Dia(23), Ahora));

        Assert.Contains("22:00", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void El_fin_debe_ser_posterior_al_inicio()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(10), Dia(10), Ahora));

        Assert.Contains("posterior", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Una_reserva_desalineada_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(8, 15), Dia(9, 15), Ahora));

        Assert.Contains("múltiplos", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Una_reserva_de_quince_minutos_es_rechazada()
    {
        Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(8), Dia(8, 15), Ahora));
    }

    [Fact]
    public void Una_reserva_en_el_pasado_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(8), Dia(9), new DateTime(2026, 11, 25, 12, 0, 0)));

        Assert.Contains("futuro", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Una_reserva_que_cruza_la_medianoche_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(21), Dia(1).AddDays(1), Ahora));

        Assert.Contains("mismo día", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Solo_pendiente_y_aprobada_son_estados_activos()
    {
        Assert.True(EstadoReserva.Pendiente.EsActiva());
        Assert.True(EstadoReserva.Aprobada.EsActiva());
        Assert.False(EstadoReserva.Rechazada.EsActiva());
        Assert.False(EstadoReserva.Cancelada.EsActiva());
    }

    [Fact]
    public void Una_reserva_de_cuatro_horas_es_valida()
    {
        ReglasDeReserva.Validar(Dia(8), Dia(12), Ahora);
    }

    [Fact]
    public void Una_reserva_de_mas_de_cuatro_horas_es_rechazada()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.Validar(Dia(8), Dia(12, 30), Ahora));

        Assert.Contains("4 horas", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Dos_reservas_contiguas_que_suman_cuatro_horas_son_validas()
    {
        ReglasDeReserva.ValidarLimiteConsecutivo(
            new[] { (Dia(8), Dia(10)) },
            Dia(10),
            Dia(12));
    }

    [Fact]
    public void Un_bloque_de_cinco_horas_es_rechazado()
    {
        ReglaNegocioException error = Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.ValidarLimiteConsecutivo(
                new[] { (Dia(8), Dia(10)), (Dia(10), Dia(12)) },
                Dia(12),
                Dia(13)));

        Assert.Contains("4 horas", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Un_hueco_corta_la_cadena()
    {
        ReglasDeReserva.ValidarLimiteConsecutivo(
            new[] { (Dia(8), Dia(10)) },
            Dia(10, 30),
            Dia(12, 30));
    }

    [Fact]
    public void La_cadena_se_calcula_aunque_las_reservas_lleguen_desordenadas()
    {
        Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.ValidarLimiteConsecutivo(
                new[] { (Dia(12), Dia(14)), (Dia(8), Dia(10)), (Dia(10), Dia(12)) },
                Dia(14),
                Dia(15)));
    }

    [Fact]
    public void Las_reservas_que_no_tocan_no_se_suman()
    {
        ReglasDeReserva.ValidarLimiteConsecutivo(
            new[] { (Dia(8), Dia(10)), (Dia(14), Dia(16)) },
            Dia(11),
            Dia(12));
    }

    [Fact]
    public void Una_reserva_que_solapa_tambien_se_conecta()
    {
        Assert.Throws<ReglaNegocioException>(
            () => ReglasDeReserva.ValidarLimiteConsecutivo(
                new[] { (Dia(8), Dia(12)) },
                Dia(11),
                Dia(14)));
    }
}

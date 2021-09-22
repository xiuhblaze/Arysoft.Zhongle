using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class DateTimeExtensions
{
    public static bool IsWeekend(this DayOfWeek d)
    {
            return !d.IsWeekday();
    }

    public static bool IsWeekday(this DayOfWeek d)
    {
        switch (d)
        {
            case DayOfWeek.Sunday:
            case DayOfWeek.Saturday: return false;
            default: return true;
        }
    }

    //public static DateTime AddWorkdays(this DateTime d, int days)
    //{
    //    Zapotlan.Transparencia.ControlSolicitudes.Models.DefaultContext db = new Zapotlan.Transparencia.ControlSolicitudes.Models.DefaultContext();
    //    DateTime inicio, fin;

    //    inicio = d;
    //    // Si la fecha inicial cae en fin de semana, avanzar al primer dia habil
    //    if (d.DayOfWeek.IsWeekend())
    //    {
    //        while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(1.0); }
    //        days--; // fuera del ciclo, toma el primer dia entre semana como el dia 1
    //    }
    //    do
    //    {
    //        // contar sólo dias entre semana
    //        while (d.DayOfWeek.IsWeekday() && days > 0)
    //        {
    //            d = d.AddDays(1.0); days--;
    //        }
    //        if (days >= 0)
    //        {
    //            // saltarse los fines de semana
    //            while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(1.0); }
    //        }
    //    } while (days > 0);

    //    fin = d;
    //    // buscar dias festivos en ese periodo
    //    var festivos = (from df in db.DiasFestivos
    //                    where df.Fecha >= inicio && df.Fecha <= fin
    //                    select df);
    //    if (festivos.Count() > 0)
    //    {
    //        foreach (var DiaFestivo in festivos)
    //        {
    //            if (DiaFestivo.Fecha.DayOfWeek.IsWeekday()) d = d.AddDays(1.0);
    //        }
    //        while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(1.0); }
    //    }
    //    return d;
    //}

    //public static int CountWorkDays(this DateTime fromdate, DateTime todate)
    //{
    //    Zapotlan.Transparencia.ControlSolicitudes.Models.DefaultContext db = new Zapotlan.Transparencia.ControlSolicitudes.Models.DefaultContext();
    //    int days = 0;
    //    DateTime d = fromdate;
    //    if (d <= todate)
    //    {
    //        // Si la fecha inicial cae en fin de semana, avanzar al primer dia habil
    //        if (d.DayOfWeek.IsWeekend())
    //        {
    //            while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(1.0); }
    //            days++;
    //        }
    //        do
    //        {
    //            // contar sólo dias entre semana
    //            while (d.DayOfWeek.IsWeekday() && d < todate)
    //            {
    //                d = d.AddDays(1.0); days++;
    //            }
    //            if (d < todate)
    //            {
    //                // saltarse los fines de semana
    //                while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(1.0); }
    //            }
    //        } while (d < todate);

    //        // buscar dias festivos en ese periodo
    //        var festivos = (from df in db.DiasFestivos
    //                        where df.Fecha >= fromdate && df.Fecha <= todate
    //                        select df);
    //        if (festivos.Count() > 0)
    //        {
    //            foreach (var DiaFestivo in festivos)
    //            {
    //                if (DiaFestivo.Fecha.DayOfWeek.IsWeekday()) days--; 
    //            }                
    //        }

    //    }
    //    else
    //    {
    //        // Si la fecha inicial cae en fin de semana, avanzar al primer dia habil
    //        if (d.DayOfWeek.IsWeekend())
    //        {
    //            while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(-1.0); }
    //            days--;
    //        }
    //        do
    //        {
    //            // contar sólo dias entre semana
    //            while (d.DayOfWeek.IsWeekday() && d > todate)
    //            {
    //                d = d.AddDays(-1.0); days--;
    //            }
    //            if (d > todate)
    //            {
    //                // saltarse los fines de semana
    //                while (d.DayOfWeek.IsWeekend()) { d = d.AddDays(-1.0); }
    //            }
    //        } while (d > todate);

    //        // buscar dias festivos en ese periodo
    //        var festivos = (from df in db.DiasFestivos
    //                        where df.Fecha >= fromdate && df.Fecha <= todate
    //                        select df);
    //        if (festivos.Count() > 0)
    //        {
    //            foreach (var DiaFestivo in festivos)
    //            {
    //                if (DiaFestivo.Fecha.DayOfWeek.IsWeekday()) days++;
    //            }
    //        }
    //    }

    //    return days;
    //}
}
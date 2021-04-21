using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Arysoft.ProyectoN.Models;

public static class Comun
{
    // CONSTANTES

    public const string TITULO_APP = "PROYECTO N21";
    public const int ELEMENTOS_PAGINA = 50;

    // PERSONA

    public static Persona ObtenerPersona(PersonaEditViewModel personaViewModel, bool soloLectura)
    {
        Persona persona = new Persona();

        persona.PersonaID = personaViewModel.PersonaID;
        persona.SectorBrigadaID = personaViewModel.SectorBrigadaID;
        persona.SeccionID = personaViewModel.SeccionID;
        persona.PersonaPromotorID = personaViewModel.PersonaPromotorID;
        persona.UbicacionViveID = personaViewModel.UbicacionViveID;
        persona.UbicacionVotaID = personaViewModel.UbicacionVotaID;        
        persona.CasillaID = personaViewModel.CasillaID;
        persona.Nombres = personaViewModel.Nombres;
        persona.ApellidoPaterno = personaViewModel.ApellidoPaterno;
        persona.ApellidoMaterno = personaViewModel.ApellidoMaterno;
        persona.Sexo = personaViewModel.Sexo;
        persona.VotaEnDomicilio = personaViewModel.VotaEnDomicilio;
        persona.Telefono = personaViewModel.Telefono;
        persona.Celular = personaViewModel.Celular;
        persona.CorreoElectronico = personaViewModel.CorreoElectronico;
        persona.Afinidad = personaViewModel.Afinidad;
        persona.VotanteSeguro = personaViewModel.VotanteSeguro;
        persona.TieneBarda = personaViewModel.TieneBarda;
        persona.TieneLona= personaViewModel.TieneLona;
        persona.RepresentanteAsistencia = personaViewModel.RepresentanteAsistencia;
        persona.RepresentanteTipo = personaViewModel.RepresentanteTipo;
        persona.RepresentanteCapacitacion = personaViewModel.RepresentanteCapacitacion;        
        persona.Verificada = personaViewModel.Verificada;

        // Campos agregados para 2021
        persona.FechaNacimiento = personaViewModel.FechaNacimiento;
        persona.EstadoCivil = personaViewModel.EstadoCivil;
        persona.Whatsapp = personaViewModel.Whatsapp;
        persona.Ocupacion = personaViewModel.Ocupacion;

        persona.FechaAlta = personaViewModel.FechaAlta;
        persona.Status= personaViewModel.Status;
        persona.FechaActualizacion = personaViewModel.FechaActualizacion;
        persona.UserNameActualizacion = personaViewModel.UserNameActualizacion;

        persona.Voto = personaViewModel.Voto;
        persona.Casilla = personaViewModel.Casilla;
        persona.Promotor = personaViewModel.Promotor; // == null ? new Persona() : personaViewModel.Promotor;
        persona.Notas = personaViewModel.Notas; // == null ? new List<Nota>() : personaViewModel.Notas;
        persona.UbicacionVive = personaViewModel.UbicacionVive; // == null ? new Ubicacion() : personaViewModel.UbicacionVive;
        persona.UbicacionVota = personaViewModel.UbicacionVota; // == null ? new Ubicacion() : personaViewModel.UbicacionVota;
        persona.Sector = personaViewModel.Sector; // == null ? new Sector() : personaViewModel.Sector;
        persona.Seccion = personaViewModel.Seccion; // == null ? new Seccion() : personaViewModel.Seccion;
        persona.PersonasAfines = personaViewModel.PersonasAfines;
        persona.AuditoriasRealizadas = personaViewModel.AuditoriasRealizadas;

        persona.SoloLectura = soloLectura;
        persona.NmOrigen = personaViewModel.NmOrigen;

        return persona;
    } // ObtenerPersona

    public static PersonaEditViewModel ObtenerPersonaEditViewModel(Persona persona, bool soloLectura)
    {        
        PersonaEditViewModel p = new PersonaEditViewModel();

        p.PersonaID = persona.PersonaID;
        p.SectorBrigadaID = persona.SectorBrigadaID;
        p.SeccionID = persona.SeccionID;
        p.PersonaPromotorID = persona.PersonaPromotorID;
        p.UbicacionViveID = persona.UbicacionViveID;
        p.UbicacionVotaID = persona.UbicacionVotaID;        
        p.CasillaID = persona.CasillaID;
        p.Nombres = persona.Nombres;
        p.ApellidoPaterno = persona.ApellidoPaterno;
        p.ApellidoMaterno = persona.ApellidoMaterno;
        p.Sexo = persona.Sexo;
        p.VotaEnDomicilio = persona.VotaEnDomicilio;
        p.Telefono = persona.Telefono;
        p.Celular = persona.Celular;
        p.CorreoElectronico = persona.CorreoElectronico;
        p.Afinidad = persona.Afinidad;
        p.VotanteSeguro = persona.VotanteSeguro;
        p.TieneBarda = persona.TieneBarda;
        p.TieneLona = persona.TieneLona;
        p.RepresentanteTipo = persona.RepresentanteTipo;
        p.RepresentanteAsistencia = persona.RepresentanteAsistencia;
        p.RepresentanteCapacitacion = persona.RepresentanteCapacitacion;
        p.Verificada = persona.Verificada;

        // Campos agregados para 2021
        p.FechaNacimiento = persona.FechaNacimiento;
        p.EstadoCivil = persona.EstadoCivil;        
        p.Whatsapp = persona.Whatsapp;
        p.Ocupacion = persona.Ocupacion;

        p.FechaAlta = persona.FechaAlta;
        p.Status = persona.Status;
        p.FechaActualizacion = persona.FechaActualizacion;
        p.UserNameActualizacion = persona.UserNameActualizacion;

        p.Voto = persona.Voto ?? new Voto();
        p.Casilla = persona.Casilla ?? new Casilla();
        p.Promotor = persona.Promotor ?? new Persona();
        p.Notas = persona.Notas ?? new List<Nota>();
        p.UbicacionVive = persona.UbicacionVive ?? new Ubicacion();
        p.UbicacionVota = persona.UbicacionVota ?? new Ubicacion();
        p.Sector = persona.Sector ?? new Sector();
        p.Seccion = persona.Seccion ?? new Seccion();
        p.PersonasAfines = persona.PersonasAfines ?? new List<Persona>();
        p.AuditoriasRealizadas = (persona.AuditoriasRealizadas ?? new List<AuditoriaPersona>()).Where(ap => ap.Auditoria.Status != AuditoriaStatusTipo.Ninguno).ToList();        

        if (persona.UbicacionVive != null)
        {
            p.CalleID = persona.UbicacionVive.CalleID;
            p.ColoniaID = persona.UbicacionVive.ColoniaID;
            p.NumExterior = persona.UbicacionVive.NumExterior;
            p.Letra = persona.UbicacionVive.Letra;
            p.NumInterior = persona.UbicacionVive.NumInterior;
            p.Descripcion = persona.UbicacionVive.Descripcion;
            p.Latitud = persona.UbicacionVive.Latitud;
            p.Longitud = persona.UbicacionVive.Longitud;
        }

        if (persona.UbicacionVota != null)
        {
            p.VotaCalleID = persona.UbicacionVota.CalleID;
            p.VotaColoniaID = persona.UbicacionVota.ColoniaID;
            p.VotaNumExterior = persona.UbicacionVota.NumExterior;
            p.VotaLetra = persona.UbicacionVota.Letra;
            p.VotaNumInterior = persona.UbicacionVota.NumInterior;
            p.VotaDescripcion = persona.UbicacionVota.Descripcion;
            p.VotaLatitud = persona.UbicacionVota.Latitud;
            p.VotaLongitud = persona.UbicacionVota.Longitud;
        }

        p.SoloLectura = soloLectura;
        p.NmOrigen = persona.NmOrigen;
        p.EsResponsableSector = EsPersonaResponsable(persona.PersonaID);

        return p;
    } // ObtenerPersonaEditViewModel

    public static bool EsPersonaResponsable(Guid personaID)
    {
        ApplicationDbContext db = new ApplicationDbContext();
        return (from s in db.Sectores where s.ResponsableID == personaID select s).Count() > 0;
    }

    public static async Task<bool> EsPersonaResponsableAsync(Guid personaID)
    {
        ApplicationDbContext db = new ApplicationDbContext();
        return await (from s in db.Sectores where s.ResponsableID == personaID select s).CountAsync() > 0;
    }

    // CASILLAS

    public static Casilla ObtenerCasilla(CasillaEditModel casillaEditModel, bool soloLectura)
    {
        Casilla c = new Casilla
        {
            CasillaID = casillaEditModel.CasillaID,
            SeccionID = casillaEditModel.SeccionID,
            UbicacionID = casillaEditModel.UbicacionID,
            PersonaResponsableID = casillaEditModel.PersonaResponsableID,
            Tipo = casillaEditModel.Tipo,
            PersonaDuenno = casillaEditModel.PersonaDuenno,
            NumeroVotantes = casillaEditModel.NumeroVotantes,
            Status = casillaEditModel.Status,
            UserNameActualizacion = casillaEditModel.UserNameActualizacion,
            FechaActualizacion = casillaEditModel.FechaActualizacion,

            Seccion = casillaEditModel.Seccion,
            Ubicacion = casillaEditModel.Ubicacion,
            PersonaResponsable = casillaEditModel.PersonaResponsable,
            Votantes = casillaEditModel.Votantes,
            Representantes = casillaEditModel.Representantes,
            Notas = casillaEditModel.Notas,

            SoloLectura = soloLectura,
            NmOrigen = casillaEditModel.NmOrigen
        };

        return c;
    } // ObtenerCasilla

    public static CasillaEditModel ObtenerCasillaEditModel(Casilla casilla, bool soloLectura)
    {
        CasillaEditModel c = new CasillaEditModel
        {
            CasillaID = casilla.CasillaID,
            SeccionID = casilla.SeccionID,
            UbicacionID = casilla.UbicacionID,
            PersonaResponsableID = casilla.PersonaResponsableID,
            Tipo = casilla.Tipo,
            PersonaDuenno = casilla.PersonaDuenno,
            NumeroVotantes = casilla.NumeroVotantes,
            Status = casilla.Status,
            UserNameActualizacion = casilla.UserNameActualizacion,
            FechaActualizacion = casilla.FechaActualizacion,

            Seccion = casilla.Seccion ?? new Seccion(),
            Ubicacion = casilla.Ubicacion ?? new Ubicacion(),
            PersonaResponsable = casilla.PersonaResponsable ?? new Persona(),
            Votantes = casilla.Votantes ?? new List<Voto>(),
            Representantes = casilla.Representantes ?? new List<Persona>(),
            Notas = casilla.Notas ?? new List<Nota>(),
            NmOrigen = casilla.NmOrigen
        };

        if (casilla.Ubicacion != null)
        {
            c.CalleID = casilla.Ubicacion.CalleID;
            c.ColoniaID = casilla.Ubicacion.ColoniaID;
            c.NumExterior = casilla.Ubicacion.NumExterior;
            c.Letra = casilla.Ubicacion.Letra;
            c.NumInterior = casilla.Ubicacion.NumInterior;
            c.Descripcion = casilla.Ubicacion.Descripcion;
            c.Latitud = casilla.Ubicacion.Latitud;
            c.Longitud = casilla.Ubicacion.Longitud;
        }

        c.SoloLectura = soloLectura;

        return c;
    } // ObtenerCasillaEditModel

} // Comun
